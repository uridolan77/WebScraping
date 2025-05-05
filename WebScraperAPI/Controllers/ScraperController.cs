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
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Threading;

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

        // Static fields for managing the metrics timer
        private static Timer _metricsReportingTimer;
        private static readonly object _metricsTimerLock = new object();
        private static IServiceScopeFactory _serviceScopeFactory;
        private static bool _metricsTimerInitialized = false;
        private static readonly Dictionary<string, WeakReference<Scraper>> _activeScraperRefs =
            new Dictionary<string, WeakReference<Scraper>>();

        public ScraperController(
            ILogger<ScraperController> logger,
            IConfiguration configuration,
            IScraperRepository scraperRepository,
            IServiceScopeFactory serviceScopeFactory)
        {
            _logger = logger;
            _configuration = configuration;
            _scraperRepository = scraperRepository;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scraperConfigs.json");

            // Use IServiceScopeFactory instead of IServiceProvider
            _serviceScopeFactory = serviceScopeFactory;

            // Initialize the metrics reporting timer if it hasn't been already
            InitializeMetricsReportingTimer();
        }

        // Initialize the timer for periodic metrics reporting
        private void InitializeMetricsReportingTimer()
        {
            lock (_metricsTimerLock)
            {
                if (!_metricsTimerInitialized)
                {
                    _logger.LogInformation("Initializing metrics reporting timer");

                    // Create and start a timer that calls ReportMetricsForAllScrapers every 30 seconds
                    _metricsReportingTimer = new Timer(
                        ReportMetricsForAllScrapers,
                        null,
                        TimeSpan.FromSeconds(10),  // Start after 10 seconds
                        TimeSpan.FromSeconds(30)   // Then every 30 seconds
                    );

                    _metricsTimerInitialized = true;
                    _logger.LogInformation("Metrics reporting timer initialized");
                    Console.WriteLine("METRICS-TIMER: Background metrics reporting initialized");

                    // Register for application shutdown to properly dispose the timer
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        StopMetricsTimer();
                    };
                }
            }
        }

        // Method to stop the metrics timer
        private static void StopMetricsTimer()
        {
            lock (_metricsTimerLock)
            {
                if (_metricsTimerInitialized)
                {
                    try
                    {
                        _metricsReportingTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                        _metricsReportingTimer?.Dispose();
                        _metricsReportingTimer = null;
                        _metricsTimerInitialized = false;
                        Console.WriteLine("METRICS-TIMER: Timer stopped and disposed");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"METRICS-TIMER-ERROR: Error stopping timer: {ex.Message}");
                    }
                }
            }
        }

        // Add/update an active scraper reference
        private void RegisterActiveScraper(string id, Scraper scraper)
        {
            if (id == null || scraper == null) return;

            lock (_activeScraperRefs)
            {
                if (_activeScraperRefs.ContainsKey(id))
                {
                    _activeScraperRefs[id] = new WeakReference<Scraper>(scraper);
                }
                else
                {
                    _activeScraperRefs.Add(id, new WeakReference<Scraper>(scraper));
                }
            }
        }

        // Remove an active scraper reference
        private void UnregisterActiveScraper(string id)
        {
            if (id == null) return;

            lock (_activeScraperRefs)
            {
                if (_activeScraperRefs.ContainsKey(id))
                {
                    _activeScraperRefs.Remove(id);
                }
            }
        }

        // Callback method for the timer
        private static async void ReportMetricsForAllScrapers(object state)
        {
            try
            {
                Console.WriteLine($"METRICS-TIMER: Running metrics update at {DateTime.Now}");

                // Snapshot the active scrapers to minimize lock contention
                Dictionary<string, Scraper> activeScrapers = new Dictionary<string, Scraper>();

                lock (_activeScraperRefs)
                {
                    foreach (var pair in _activeScraperRefs.ToList())
                    {
                        if (pair.Value.TryGetTarget(out var scraper))
                        {
                            activeScrapers[pair.Key] = scraper;
                        }
                        else
                        {
                            // Remove expired weak references
                            _activeScraperRefs.Remove(pair.Key);
                        }
                    }
                }

                if (activeScrapers.Count == 0)
                {
                    Console.WriteLine("METRICS-TIMER: No active scrapers found");
                    return;
                }

                // Create a new scope for each timer callback
                using (var scope = _serviceScopeFactory?.CreateScope())
                {
                    if (scope == null)
                    {
                        Console.WriteLine("METRICS-TIMER-ERROR: Could not create service scope - service scope factory is null");
                        return;
                    }

                    try
                    {
                        // Get the metrics service instead of the controller
                        var metricsService = scope.ServiceProvider.GetRequiredService<WebScraperApi.Services.Monitoring.IScraperMetricsService>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScraperController>>();

                        logger.LogInformation("Starting periodic metrics reporting for all active scrapers");

                        // Use the metrics service to update metrics for all active scrapers
                        await metricsService.UpdateMetricsForAllScrapersAsync(activeScrapers);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"METRICS-TIMER-ERROR: Error processing metrics in scope: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"METRICS-TIMER-ERROR-INNER: {ex.InnerException.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"METRICS-TIMER-ERROR: Error updating metrics: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"METRICS-TIMER-ERROR-INNER: {ex.InnerException.Message}");
                }
                // We catch exceptions here to prevent the timer from stopping
            }
        }

        // Method to load scraper configurations from JSON file
        private async Task<List<ScraperConfigModel>> LoadScraperConfigsAsync()
        {
            try
            {
                _logger.LogInformation("Loading scraper configurations from: {ConfigPath}", _configFilePath);

                if (!System.IO.File.Exists(_configFilePath))
                {
                    _logger.LogWarning("Configuration file does not exist. Creating empty configuration file.");
                    await System.IO.File.WriteAllTextAsync(_configFilePath, "[]");
                    return new List<ScraperConfigModel>();
                }

                string json = await System.IO.File.ReadAllTextAsync(_configFilePath);
                var configs = JsonSerializer.Deserialize<List<ScraperConfigModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ScraperConfigModel>();

                _logger.LogInformation("Loaded {Count} scraper configurations", configs.Count);
                return configs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scraper configurations");
                return new List<ScraperConfigModel>();
            }
        }

        // Method to save scraper configurations to JSON file
        private async Task SaveScraperConfigsAsync(List<ScraperConfigModel> configs)
        {
            try
            {
                _logger.LogInformation("Saving {Count} scraper configurations to: {ConfigPath}", configs.Count, _configFilePath);

                string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await System.IO.File.WriteAllTextAsync(_configFilePath, json);
                _logger.LogInformation("Scraper configurations saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving scraper configurations");
                throw;
            }
        }

        // Helper method to get a scraper by ID
        private async Task<ScraperConfigModel> GetScraper(string id)
        {
            // First try to get the scraper from the database
            var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);

            if (dbScraper != null)
            {
                // Convert the database entity to API model
                return new ScraperConfigModel
                {
                    Id = dbScraper.Id,
                    Name = dbScraper.Name,
                    StartUrl = dbScraper.StartUrl,
                    BaseUrl = dbScraper.BaseUrl,
                    OutputDirectory = dbScraper.OutputDirectory,
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
                    // Include related entity collections
                    StartUrls = dbScraper.StartUrls?.Select(u => u.Url)?.ToList(),
                    ContentExtractorSelectors = dbScraper.ContentExtractorSelectors?.Where(c => !c.IsExclude)?.Select(c => c.Selector)?.ToList(),
                    ContentExtractorExcludeSelectors = dbScraper.ContentExtractorSelectors?.Where(c => c.IsExclude)?.Select(c => c.Selector)?.ToList()
                };
            }

            // Fallback to JSON file if not found in database
            var configs = await LoadScraperConfigsAsync();
            return configs.FirstOrDefault(c => c.Id == id);
        }

        // Other methods and logic remain unchanged
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
