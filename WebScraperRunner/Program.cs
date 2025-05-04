using System;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WebScraperRunner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("WebScraperRunner starting...");

            string scraperId = null;
            string connectionString = null;

            // Parse command line arguments
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--scraper-id" && i + 1 < args.Length)
                {
                    scraperId = args[i + 1];
                    i++;
                }
                else if (args[i] == "--connection-string" && i + 1 < args.Length)
                {
                    connectionString = args[i + 1];
                    i++;
                }
            }

            if (string.IsNullOrEmpty(scraperId))
            {
                Console.WriteLine("Error: --scraper-id parameter is required");
                return;
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("Error: --connection-string parameter is required");
                return;
            }

            Console.WriteLine($"Starting scraper with ID: {scraperId}");

            try
            {
                // Set up dependency injection
                var services = new ServiceCollection();

                // Add logging
                services.AddLogging(builder =>
                {
                    builder.AddConsole();
                    builder.SetMinimumLevel(LogLevel.Information);
                });

                // Add configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                services.AddSingleton<IConfiguration>(configuration);

                // Add database context
                services.AddDbContext<WebScraperDbContext>(options =>
                    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

                // Add repositories
                services.AddScoped<IScraperRepository, ScraperRepository>();

                // Build service provider
                var serviceProvider = services.BuildServiceProvider();

                // Get repository
                var repository = serviceProvider.GetRequiredService<IScraperRepository>();
                var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

                // Get scraper configuration
                var scraperConfig = await repository.GetScraperByIdAsync(scraperId);

                if (scraperConfig == null)
                {
                    Console.WriteLine($"Error: Scraper with ID {scraperId} not found");
                    return;
                }

                // Create scraper configuration
                var config = new ScraperConfig
                {
                    Name = scraperConfig.Name,
                    StartUrl = scraperConfig.StartUrl,
                    BaseUrl = scraperConfig.BaseUrl,
                    MaxDepth = scraperConfig.MaxDepth,
                    DelayBetweenRequests = scraperConfig.DelayBetweenRequests,
                    OutputDirectory = scraperConfig.OutputDirectory,
                    UserAgent = scraperConfig.UserAgent,
                    FollowExternalLinks = scraperConfig.FollowExternalLinks,
                    StoreContentInDatabase = true
                };

                // Create logger action
                Action<string> logAction = (message) =>
                {
                    Console.WriteLine($"Scraper {scraperId}: {message}");
                    logger.LogInformation($"Scraper {scraperId}: {message}");

                    // Also log to database
                    try
                    {
                        var logEntry = new WebScraperApi.Data.Entities.ScraperLogEntity
                        {
                            ScraperId = scraperId,
                            Timestamp = DateTime.Now,
                            LogLevel = "Info",
                            Message = message
                        };

                        repository.AddScraperLogAsync(logEntry).Wait();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error logging to database: {ex.Message}");
                    }
                };

                // Update scraper status to running
                try
                {
                    var status = await repository.GetScraperStatusAsync(scraperId);

                    if (status == null)
                    {
                        status = new WebScraperApi.Data.Entities.ScraperStatusEntity
                        {
                            ScraperId = scraperId,
                            IsRunning = true,
                            StartTime = DateTime.Now,
                            UrlsProcessed = 0,
                            UrlsQueued = 0,
                            DocumentsProcessed = 0,
                            HasErrors = false,
                            Message = "Scraper started by WebScraperRunner",
                            ElapsedTime = "00:00:00",
                            LastStatusUpdate = DateTime.Now,
                            LastUpdate = DateTime.Now
                        };
                    }
                    else
                    {
                        status.IsRunning = true;
                        status.StartTime = DateTime.Now;
                        status.Message = "Scraper started by WebScraperRunner";
                        status.LastStatusUpdate = DateTime.Now;
                        status.LastUpdate = DateTime.Now;
                    }

                    await repository.UpdateScraperStatusAsync(status);
                    Console.WriteLine("Updated scraper status to running");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating scraper status: {ex.Message}");
                }

                // Create and run the scraper
                var scraper = new Scraper(config, logAction);

                try
                {
                    Console.WriteLine("Initializing scraper...");
                    await scraper.InitializeAsync();

                    Console.WriteLine("Starting scraping process...");
                    await scraper.StartScrapingAsync();

                    Console.WriteLine("Scraping completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error running scraper: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    // Update status to error
                    try
                    {
                        var status = await repository.GetScraperStatusAsync(scraperId);

                        if (status != null)
                        {
                            status.IsRunning = false;
                            status.EndTime = DateTime.Now;
                            status.HasErrors = true;
                            status.Message = $"Error: {ex.Message}";
                            status.LastError = ex.Message;
                            status.LastStatusUpdate = DateTime.Now;
                            status.LastUpdate = DateTime.Now;

                            if (status.StartTime.HasValue)
                            {
                                TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                                status.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                            }

                            await repository.UpdateScraperStatusAsync(status);
                            Console.WriteLine("Updated scraper status to error");
                        }
                    }
                    catch (Exception statusEx)
                    {
                        Console.WriteLine($"Error updating scraper status: {statusEx.Message}");
                    }
                }
                finally
                {
                    // Update status to completed
                    try
                    {
                        var status = await repository.GetScraperStatusAsync(scraperId);

                        if (status != null)
                        {
                            status.IsRunning = false;
                            status.EndTime = DateTime.Now;

                            if (!status.HasErrors)
                            {
                                status.Message = "Scraping completed successfully";
                            }

                            status.LastStatusUpdate = DateTime.Now;
                            status.LastUpdate = DateTime.Now;

                            if (status.StartTime.HasValue)
                            {
                                TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                                status.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                            }

                            await repository.UpdateScraperStatusAsync(status);
                            Console.WriteLine("Updated scraper status to completed");
                        }
                    }
                    catch (Exception statusEx)
                    {
                        Console.WriteLine($"Error updating scraper status: {statusEx.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("WebScraperRunner completed");
        }
    }
}
