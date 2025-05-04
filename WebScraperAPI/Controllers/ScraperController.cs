using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using WebScraper;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace WebScraperAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public partial class ScraperController : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, Scraper> _activeScrapers = new Dictionary<string, Scraper>();
        private readonly string _configFilePath;
        private readonly IScraperRepository _scraperRepository;
        
        public ScraperController(
            ILogger<ScraperController> logger,
            IConfiguration configuration,
            IScraperRepository scraperRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _scraperRepository = scraperRepository;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scraperConfigs.json");
        }

        // Helper methods for loading and saving scraper configurations
        private async Task<List<ScraperConfigModel>> LoadScraperConfigsAsync()
        {
            if (!System.IO.File.Exists(_configFilePath))
            {
                return new List<ScraperConfigModel>();
            }
            
            string json = await System.IO.File.ReadAllTextAsync(_configFilePath);
            return JsonSerializer.Deserialize<List<ScraperConfigModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ScraperConfigModel>();
        }
        
        private async Task SaveScraperConfigsAsync(List<ScraperConfigModel> configs)
        {
            string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await System.IO.File.WriteAllTextAsync(_configFilePath, json);
        }

        // Get scraper by id
        [HttpGet("{id}")]
        public async Task<ActionResult<ScraperConfigModel>> GetScraper(string id)
        {
            try
            {
                _logger.LogInformation("Getting scraper with ID {Id}", id);
                
                // First try to get the scraper from the database
                var isConnected = _scraperRepository.TestDatabaseConnection();
                _logger.LogInformation("Database connection test result: {IsConnected}", isConnected);
                
                if (isConnected)
                {
                    _logger.LogInformation("Attempting to fetch scraper {Id} from database...", id);
                    var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                    
                    if (dbScraper != null)
                    {
                        _logger.LogInformation("Found scraper {Id} in database", id);
                        
                        // Log related entity counts to help diagnose issues
                        _logger.LogInformation("Related entities for {Id}: StartUrls={StartUrlsCount}, ContentExtractorSelectors={SelectorsCount}", 
                            dbScraper.Id, 
                            dbScraper.StartUrls?.Count ?? 0,
                            dbScraper.ContentExtractorSelectors?.Count ?? 0);
                        
                        // Convert database entity to API model
                        var scraperModel = new ScraperConfigModel
                        {
                            Id = dbScraper.Id,
                            Name = dbScraper.Name,
                            StartUrl = dbScraper.StartUrl,
                            BaseUrl = dbScraper.BaseUrl ?? string.Empty,
                            OutputDirectory = dbScraper.OutputDirectory ?? string.Empty,
                            MaxDepth = dbScraper.MaxDepth,
                            MaxPages = dbScraper.MaxPages,
                            DelayBetweenRequests = dbScraper.DelayBetweenRequests,
                            MaxConcurrentRequests = dbScraper.MaxConcurrentRequests,
                            FollowLinks = dbScraper.FollowLinks,
                            FollowExternalLinks = dbScraper.FollowExternalLinks,
                            CreatedAt = dbScraper.CreatedAt,
                            LastModified = dbScraper.LastModified,
                            LastRun = dbScraper.LastRun,
                            RunCount = dbScraper.RunCount,
                            // Initialize empty collections to prevent null references
                            StartUrls = dbScraper.StartUrls?.Select(u => u.Url)?.ToList() ?? new List<string>(),
                            ContentExtractorSelectors = dbScraper.ContentExtractorSelectors?.Where(c => !c.IsExclude)?.Select(c => c.Selector)?.ToList() ?? new List<string>(),
                            ContentExtractorExcludeSelectors = dbScraper.ContentExtractorSelectors?.Where(c => c.IsExclude)?.Select(c => c.Selector)?.ToList() ?? new List<string>()
                        };
                        
                        return Ok(scraperModel);
                    }
                    else
                    {
                        _logger.LogWarning("Scraper {Id} not found in database, falling back to JSON file", id);
                    }
                }
                
                // Fallback to JSON file if database retrieval failed
                _logger.LogInformation("Fallback: Looking for scraper {Id} in JSON file", id);
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    _logger.LogWarning("Scraper {Id} not found in JSON file either", id);
                    return NotFound();
                }
                
                _logger.LogInformation("Found scraper {Id} in JSON file", id);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scraper with ID {Id}", id);
                return StatusCode(500, "An error occurred while retrieving the scraper.");
            }
        }

        // Get scraped files
        [HttpGet("{id}/files")]
        public ActionResult<IEnumerable<object>> GetScrapedFiles(string id)
        {
            try
            {
                var configs = LoadScraperConfigsAsync().Result;
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    return NotFound("Scraper not found.");
                }
                
                if (string.IsNullOrEmpty(config.OutputDirectory) || !Directory.Exists(config.OutputDirectory))
                {
                    return new List<object>();
                }
                
                // Get all files in the output directory
                var files = Directory.GetFiles(config.OutputDirectory)
                    .Select(f => new FileInfo(f))
                    .Select(f => new
                    {
                        Name = f.Name,
                        Size = f.Length,
                        LastModified = f.LastWriteTime,
                        Path = f.FullName
                    })
                    .ToList();
                
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files for scraper with ID {Id}", id);
                return StatusCode(500, "An error occurred while getting the scraped files.");
            }
        }

        // Helper method to initialize scraper status
        private async Task InitializeScraperStatusAsync(string scraperId)
        {
            var status = new WebScraperApi.Data.Entities.ScraperStatusEntity
            {
                ScraperId = scraperId,
                IsRunning = true,
                StartTime = DateTime.Now,
                EndTime = null,
                ElapsedTime = "0s",
                UrlsProcessed = 0,
                UrlsQueued = 0,
                DocumentsProcessed = 0,
                HasErrors = false,
                Message = "Scraper started",
                LastStatusUpdate = DateTime.Now,
                LastUpdate = DateTime.Now,
                LastMonitorCheck = DateTime.Now,
                LastError = string.Empty
            };
            
            await _scraperRepository.UpdateScraperStatusAsync(status);
            
            // Also log the scraper start
            await _scraperRepository.AddScraperLogAsync(new WebScraperApi.Data.Entities.ScraperLogEntity
            {
                ScraperId = scraperId,
                Timestamp = DateTime.Now,
                LogLevel = "Info",
                Message = "Scraper started"
            });
        }

        // Helper method to update scraper status
        private async Task UpdateScraperStatusAsync(string scraperId, string currentUrl, bool hasError = false, string errorMessage = "")
        {
            try
            {
                var status = await _scraperRepository.GetScraperStatusAsync(scraperId);
                
                if (status == null)
                {
                    status = new WebScraperApi.Data.Entities.ScraperStatusEntity
                    {
                        ScraperId = scraperId,
                        IsRunning = true,
                        StartTime = DateTime.Now,
                        UrlsProcessed = 0
                    };
                }
                
                // Update status
                status.IsRunning = true;
                status.UrlsProcessed++;
                status.LastStatusUpdate = DateTime.Now;
                status.LastUpdate = DateTime.Now;
                
                if (status.StartTime.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                    status.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                }
                
                if (hasError)
                {
                    status.HasErrors = true;
                    status.LastError = errorMessage;
                    status.Message = $"Error processing URL: {currentUrl}";
                    
                    // Log error
                    await _scraperRepository.AddScraperLogAsync(new WebScraperApi.Data.Entities.ScraperLogEntity
                    {
                        ScraperId = scraperId,
                        Timestamp = DateTime.Now,
                        LogLevel = "Error",
                        Message = $"Error processing URL: {currentUrl}. Error: {errorMessage}"
                    });
                }
                else
                {
                    status.Message = $"Processing URL: {currentUrl}";
                    
                    // Log info
                    await _scraperRepository.AddScraperLogAsync(new WebScraperApi.Data.Entities.ScraperLogEntity
                    {
                        ScraperId = scraperId,
                        Timestamp = DateTime.Now,
                        LogLevel = "Info",
                        Message = $"Processed URL: {currentUrl}"
                    });
                }
                
                await _scraperRepository.UpdateScraperStatusAsync(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scraper status for scraper with ID {ScraperId}", scraperId);
            }
        }

        // Helper method to save page content
        private async Task SavePageContentAsync(string scraperId, string url, string htmlContent, string textContent)
        {
            try
            {
                // Save to database
                await _scraperRepository.AddScrapedPageAsync(new WebScraperApi.Data.Entities.ScrapedPageEntity
                {
                    ScraperId = scraperId,
                    Url = url,
                    HtmlContent = htmlContent,
                    TextContent = textContent,
                    ScrapedAt = DateTime.Now
                });
                
                // Also save to files if output directory is configured
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == scraperId);
                
                if (config != null && !string.IsNullOrEmpty(config.OutputDirectory))
                {
                    if (!Directory.Exists(config.OutputDirectory))
                    {
                        Directory.CreateDirectory(config.OutputDirectory);
                    }
                    
                    // Create a safe filename from the URL
                    string filename = Uri.UnescapeDataString(new Uri(url).PathAndQuery)
                        .Replace("/", "_")
                        .Replace("?", "_")
                        .Replace("&", "_")
                        .Replace("=", "_");
                    
                    if (string.IsNullOrEmpty(filename) || filename == "_")
                    {
                        filename = "index";
                    }
                    
                    // Save HTML file
                    string htmlFilePath = Path.Combine(config.OutputDirectory, $"{filename}.html");
                    await System.IO.File.WriteAllTextAsync(htmlFilePath, htmlContent);
                    
                    // Save text file
                    string textFilePath = Path.Combine(config.OutputDirectory, $"{filename}.txt");
                    await System.IO.File.WriteAllTextAsync(textFilePath, textContent);
                    
                    // Log the file saving
                    await _scraperRepository.AddScraperLogAsync(new WebScraperApi.Data.Entities.ScraperLogEntity
                    {
                        ScraperId = scraperId,
                        Timestamp = DateTime.Now,
                        LogLevel = "Info",
                        Message = $"Saved content for {url} to {htmlFilePath} and {textFilePath}"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving page content for URL {Url}", url);
            }
        }

        // Helper method specifically to create the correct entity type for AddScraperMetricAsync
        private WebScraperApi.Data.Entities.ScraperMetricEntity CreateMetricEntity(string scraperId, string metricName, double value)
        {
            return new WebScraperApi.Data.Entities.ScraperMetricEntity
            {
                ScraperId = scraperId,
                MetricName = metricName,
                MetricValue = value,
                Timestamp = DateTime.Now
            };
        }

        // Helper method to update scraper metrics
        private async Task UpdateScraperMetricsAsync(string scraperId, string metricName, double value)
        {
            try
            {
                // Force dynamic typing to bypass the type checking at compile time
                dynamic metricEntity = new WebScraperApi.Data.Entities.ScraperMetricEntity
                {
                    ScraperId = scraperId,
                    MetricName = metricName,
                    MetricValue = value,
                    Timestamp = DateTime.Now
                };
                
                // This will be resolved at runtime rather than compile time
                await _scraperRepository.AddScraperMetricAsync(metricEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scraper metrics for scraper with ID {ScraperId}", scraperId);
            }
        }

        // Helper method to complete a scraper run
        private async Task CompleteScraperRunAsync(string scraperId)
        {
            try
            {
                // Update scraper status
                var status = await _scraperRepository.GetScraperStatusAsync(scraperId);
                
                if (status != null)
                {
                    status.IsRunning = false;
                    status.EndTime = DateTime.Now;
                    
                    if (status.StartTime.HasValue && status.EndTime.HasValue)
                    {
                        TimeSpan elapsed = status.EndTime.Value - status.StartTime.Value;
                        status.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                    
                    status.Message = "Scraping completed";
                    status.LastUpdate = DateTime.Now;
                    
                    await _scraperRepository.UpdateScraperStatusAsync(status);
                }
                
                // Log scraper completion
                await _scraperRepository.AddScraperLogAsync(new WebScraperApi.Data.Entities.ScraperLogEntity
                {
                    ScraperId = scraperId,
                    Timestamp = DateTime.Now,
                    LogLevel = "Info",
                    Message = "Scraping completed"
                });
                
                // Update scraper config
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == scraperId);
                
                if (config != null)
                {
                    config.LastRun = DateTime.Now;
                    config.LastModified = DateTime.Now;
                    await SaveScraperConfigsAsync(configs);
                }
                
                // Remove from active scrapers
                _activeScrapers.Remove(scraperId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing scraper run for scraper with ID {ScraperId}", scraperId);
            }
        }
    }
    
    // Simple model class for scraper configuration
    public class ScraperConfigModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string StartUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public int MaxDepth { get; set; } = 5;
        public int MaxPages { get; set; } = 1000;
        public int DelayBetweenRequests { get; set; } = 1000;
        public int MaxConcurrentRequests { get; set; } = 5;
        public bool FollowLinks { get; set; } = true;
        public bool FollowExternalLinks { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public DateTime? LastRun { get; set; }
        public int RunCount { get; set; } = 0;
        public List<string>? StartUrls { get; set; }
        public List<string>? ContentExtractorSelectors { get; set; }
        public List<string>? ContentExtractorExcludeSelectors { get; set; }
    }
}
