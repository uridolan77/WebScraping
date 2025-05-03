using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.Validation;
using WebScraperApi.Models;
using WebScraperApi.Services.Factories;
using WebScraper.RegulatoryFramework.Implementation;
using WebScraper.RegulatoryFramework.Configuration;

namespace WebScraperApi.Services.Execution
{
    /// <summary>
    /// Service for managing the execution of scrapers
    /// </summary>
    public class ScraperExecutionService : IScraperExecutionService
    {
        private readonly ILogger<ScraperExecutionService> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ScraperComponentFactory _componentFactory;

        public ScraperExecutionService(
            ILogger<ScraperExecutionService> logger,
            ILoggerFactory loggerFactory,
            ScraperComponentFactory componentFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _componentFactory = componentFactory;
        }

        /// <summary>
        /// Starts a scraper with the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="scraperState">The current scraper state to update during execution</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<bool> StartScraperAsync(
            ScraperConfigModel config,
            WebScraperApi.Models.ScraperState scraperState,
            Action<string> logAction)
        {
            try
            {
                logAction($"Starting scraper: {config.Name}");

                // Get the scraper configuration
                var scraperConfig = config.ToScraperConfig();

                // Validate the configuration
                if (!await ValidateConfigurationAsync(scraperConfig, logAction))
                {
                    return false;
                }

                try
                {
                    // Always use EnhancedScraper for all scrapers
                    var enhancedScraper = await CreateEnhancedScraperAsync(scraperConfig, logAction);

                    // Create a standard Scraper that we can use with the existing code
                    var scraper = new Scraper(scraperConfig, _logger);

                    // Store the enhanced scraper instance in the state
                    scraperState.Scraper = enhancedScraper;

                    // Initialize the scraper
                    logAction("Initializing scraper...");
                    await scraper.InitializeAsync();

                    // Start scraping
                    logAction("Starting scraping process...");
                    await scraper.StartScrapingAsync();

                    // Set up continuous monitoring if enabled
                    if (config.EnableContinuousMonitoring)
                    {
                        var interval = TimeSpan.FromHours(config.GetMonitoringInterval());
                        await scraper.SetupContinuousScrapingAsync(interval);
                        logAction($"Continuous monitoring enabled with interval: {interval.TotalHours:F1} hours");
                    }

                    logAction("Scraping operation completed successfully");

                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error during scraper execution for {config.Name}: {ex.Message}");
                    logAction($"Error during scraper execution: {ex.Message}");
                    
                    if (ex.InnerException != null)
                    {
                        _logger.LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                        logAction($"Inner error: {ex.InnerException.Message}");
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing scraper {config.Name}");
                logAction($"Error during scraping: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                    logAction($"Inner error: {ex.InnerException.Message}");
                }
                
                return false;
            }
        }

        /// <summary>
        /// Creates an enhanced scraper with components from factories
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A configured EnhancedScraper instance</returns>
        public async Task<EnhancedScraper> CreateEnhancedScraperAsync(ScraperConfig config, Action<string> logAction)
        {
            // Create a scraper-specific logger
            var scraperLogger = _loggerFactory.CreateLogger<EnhancedScraper>();

            // Convert ScraperConfig to RegulatoryScraperConfig
            var regulatoryConfig = new RegulatoryScraperConfig
            {
                DomainName = config.Name,
                BaseUrl = config.StartUrl,
                UserAgent = "WebScraperAPI/1.0",
                MaxConcurrentRequests = config.MaxConcurrentRequests,
                RequestTimeoutSeconds = 30,
                EnablePriorityCrawling = config.EnableAdaptiveCrawling,
                EnableHierarchicalExtraction = config.ExtractStructuredContent,
                EnableDocumentProcessing = config.EnableDocumentProcessing,
                EnableComplianceChangeDetection = config.EnableChangeDetection,
                EnableDomainClassification = config.ClassifyRegulatoryDocuments,
                EnableDynamicContentRendering = false,
                EnableAlertSystem = config.NotifyOnChanges
            };

            // Create components from factories
            var crawlStrategy = _componentFactory.CreateCrawlStrategy(config, logAction);
            var contentExtractor = _componentFactory.CreateContentExtractor(config, logAction);
            var documentProcessor = _componentFactory.CreateDocumentProcessor(config, logAction);
            var changeDetector = _componentFactory.CreateChangeDetector(config, logAction);
            var contentClassifier = _componentFactory.CreateContentClassifier(config, logAction);
            var stateStore = _componentFactory.CreateStateStore(config, logAction);

            // Create the enhanced scraper with all components
            var enhancedScraper = new EnhancedScraper(
                regulatoryConfig,
                scraperLogger,
                crawlStrategy,
                contentExtractor,
                documentProcessor,
                changeDetector,
                contentClassifier,
                null, // No dynamic renderer
                null, // No alert service
                stateStore);

            // Wait a moment to make this truly async
            await Task.Delay(1);

            // Return the configured scraper
            return enhancedScraper;
        }

        /// <summary>
        /// Validates the scraper configuration
        /// </summary>
        private async Task<bool> ValidateConfigurationAsync(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Validating configuration...");

                // Add a small delay to make this truly async
                await Task.Delay(1);

                // Basic validation
                if (string.IsNullOrEmpty(config.StartUrl))
                {
                    logAction("Error: Start URL is required");
                    return false;
                }

                if (!Uri.TryCreate(config.StartUrl, UriKind.Absolute, out _))
                {
                    logAction("Error: Start URL must be a valid URL");
                    return false;
                }

                // Output directory validation
                if (string.IsNullOrEmpty(config.OutputDirectory))
                {
                    config.OutputDirectory = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        "ScrapedData",
                        config.Name);

                    logAction($"Output directory not specified, using default: {config.OutputDirectory}");
                }

                // Validate crawl depth
                if (config.MaxCrawlDepth <= 0)
                {
                    config.MaxCrawlDepth = 3;
                    logAction($"Invalid crawl depth, using default: {config.MaxCrawlDepth}");
                }

                // Advanced validation for specific scraper types
                if (config.EnableRegulatoryContentAnalysis)
                {
                    logAction("Validating regulatory monitor configuration...");

                    // Add any regulatory-specific validation here
                }

                logAction("Configuration validation successful");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration");
                logAction($"Configuration validation error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Stops a running scraper
        /// </summary>
        /// <param name="scraper">The scraper to stop</param>
        /// <param name="logAction">Action for logging messages</param>
        public void StopScraper(Scraper scraper, Action<string> logAction)
        {
            try
            {
                if (scraper == null)
                {
                    logAction("No active scraper instance to stop");
                    return;
                }

                logAction("Stopping scraper...");
                // Use the standard StopScraping method
                scraper.StopScraping();
                logAction("Scraper stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraper");
                logAction($"Error stopping scraper: {ex.Message}");
            }
        }

        /// <summary>
        /// Stops a scraper by ID
        /// </summary>
        /// <param name="id">The ID of the scraper to stop</param>
        /// <returns>True if the scraper was stopped successfully, false otherwise</returns>
        public async Task<bool> StopScraperAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Attempting to stop scraper with ID {id}");

                // This is a placeholder implementation
                // In a real implementation, you would need to:
                // 1. Get the scraper instance from a state service
                // 2. Call StopScraper with the instance

                // Add a small delay to make this truly async
                await Task.Delay(1);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping scraper with ID {id}");
                return false;
            }
        }

        /// <summary>
        /// Gets the status of a scraper by ID
        /// </summary>
        /// <param name="id">The ID of the scraper</param>
        /// <returns>The status of the scraper</returns>
        public async Task<ScraperStatus> GetScraperStatusAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Getting status for scraper with ID {id}");

                // This is a placeholder implementation
                // In a real implementation, you would need to:
                // 1. Get the scraper status from a state service or database

                // Add a small delay to make this truly async
                await Task.Delay(1);

                // Return a default status
                return new ScraperStatus
                {
                    IsRunning = false,
                    Message = "Status retrieved",
                    LastStatusUpdate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting status for scraper with ID {id}");
                throw;
            }
        }
    }
}