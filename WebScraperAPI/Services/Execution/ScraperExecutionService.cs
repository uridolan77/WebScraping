using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.Validation;
using WebScraperApi.Models;
using WebScraperApi.Services.Factories;

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
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _componentFactory = new ScraperComponentFactory(loggerFactory);
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
            ScraperState scraperState,
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
                
                // Always use EnhancedScraper for all scrapers
                Scraper scraper = await CreateEnhancedScraperAsync(scraperConfig, logAction);
                
                // Store the scraper instance in the state
                scraperState.Scraper = scraper;
                
                // Initialize the scraper
                logAction("Initializing scraper...");
                await scraper.InitializeAsync();
                
                // Start scraping
                logAction("Starting scraping process...");
                await scraper.StartScrapingAsync();
                
                // Set up continuous monitoring if enabled
                if (config.EnableContinuousMonitoring)
                {
                    var interval = config.GetMonitoringInterval();
                    await scraper.SetupContinuousScrapingAsync(interval);
                    logAction($"Continuous monitoring enabled with interval: {interval.TotalHours:F1} hours");
                }
                
                logAction("Scraping operation completed successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing scraper {config.Name}");
                logAction($"Error during scraping: {ex.Message}");
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
            var scraperLogger = _loggerFactory.CreateLogger($"Scraper_{config.Name}");
            
            // Create the scraper
            logAction("Creating EnhancedScraper instance...");
            var scraper = new EnhancedScraper(config, scraperLogger);
            
            // Create and set components from factory
            await Task.Run(() =>
            {
                logAction("Setting up components...");
                
                // Create scraper components based on configuration
                if (config.UseHeadlessBrowser)
                {
                    logAction("Setting up headless browser handler...");
                    var headlessBrowser = _componentFactory.CreateHeadlessBrowserHandler(config);
                    scraper.SetHeadlessBrowserHandler(headlessBrowser);
                }
                
                // Set up state management if enabled
                if (config.EnablePersistentState)
                {
                    logAction("Setting up persistent state management...");
                    
                    // Ensure output directory exists
                    if (!Directory.Exists(config.OutputDirectory))
                    {
                        Directory.CreateDirectory(config.OutputDirectory);
                    }
                    
                    var stateManager = _componentFactory.CreateStateManager(config);
                    scraper.SetStateManager(stateManager);
                }
                
                // Set up adaptive crawling if enabled
                if (config.EnableAdaptiveCrawling)
                {
                    logAction("Setting up adaptive crawling...");
                    var adaptiveCrawler = _componentFactory.CreateAdaptiveCrawler(config);
                    scraper.SetCrawlStrategy(adaptiveCrawler);
                }
                
                // Set up content change detection if enabled
                if (config.DetectContentChanges)
                {
                    logAction("Setting up content change detection...");
                    var changeDetector = _componentFactory.CreateChangeDetector(config);
                    scraper.SetChangeDetector(changeDetector);
                }
                
                // Setup rate limiting if enabled
                if (config.EnableRateLimiting)
                {
                    logAction("Setting up rate limiting...");
                    var rateLimiter = _componentFactory.CreateRateLimiter(config);
                    scraper.SetRateLimiter(rateLimiter);
                }
                
                // Set up PDF and Office document handling if enabled
                if (config.EnableDocumentProcessing)
                {
                    logAction("Setting up document processing...");
                    
                    var pdfHandler = _componentFactory.CreatePdfDocumentHandler(config);
                    var officeHandler = _componentFactory.CreateOfficeDocumentHandler(config);
                    
                    scraper.SetDocumentHandlers(pdfHandler, officeHandler);
                }
                
                // Set up validation
                var validator = new ConfigurationValidator(_loggerFactory.CreateLogger<ConfigurationValidator>());
                scraper.SetConfigurationValidator(validator);
            });
            
            // Return the configured scraper
            return scraper;
        }
        
        /// <summary>
        /// Validates the scraper configuration
        /// </summary>
        private async Task<bool> ValidateConfigurationAsync(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Validating configuration...");
                
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
                if (config.ScraperType == ScraperType.RegulatoryMonitor)
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
                scraper.StopScrapingAsync().Wait();
                logAction("Scraper stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraper");
                logAction($"Error stopping scraper: {ex.Message}");
            }
        }
    }
}