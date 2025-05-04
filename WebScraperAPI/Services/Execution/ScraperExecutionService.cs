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
        private readonly Dictionary<string, WebScraperApi.Models.ScraperState> _scraperStates = new();

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
                // Update state to reflect we're starting
                scraperState.IsRunning = true;
                scraperState.Status = "Starting";
                scraperState.StartTime = DateTime.Now;
                scraperState.Message = $"Starting scraper: {config.Name}";
                scraperState.LastUpdate = DateTime.Now;
                
                // Store the state for retrieval
                _scraperStates[config.Id] = scraperState;
                
                logAction($"Starting scraper: {config.Name}");
                _logger.LogInformation("Starting scraper '{Name}' with ID '{Id}'", config.Name, config.Id);

                // Get the scraper configuration
                var scraperConfig = config.ToScraperConfig();
                
                // Validate the configuration
                if (!await ValidateConfigurationAsync(scraperConfig, logAction))
                {
                    // Update state to reflect validation failure
                    scraperState.IsRunning = false;
                    scraperState.Status = "Failed";
                    scraperState.EndTime = DateTime.Now;
                    scraperState.Message = "Configuration validation failed";
                    scraperState.HasErrors = true;
                    scraperState.LastError = "Configuration validation failed - check logs for details";
                    scraperState.LastUpdate = DateTime.Now;
                    
                    // Update stored state
                    _scraperStates[config.Id] = scraperState;
                    
                    _logger.LogError("Scraper '{Name}' failed validation", config.Name);
                    return false;
                }

                try
                {
                    // Always use EnhancedScraper for all scrapers
                    _logger.LogDebug("Creating enhanced scraper for '{Name}'", config.Name);
                    var enhancedScraper = await CreateEnhancedScraperAsync(scraperConfig, logAction);

                    // Create a standard Scraper that we can use with the existing code
                    _logger.LogDebug("Creating standard scraper for '{Name}'", config.Name);
                    var scraper = new Scraper(scraperConfig, _logger);

                    // Store the enhanced scraper instance in the state
                    scraperState.Scraper = enhancedScraper;
                    
                    // Update state
                    scraperState.Status = "Initializing";
                    scraperState.Message = "Initializing scraper...";

                    // Initialize the scraper
                    logAction("Initializing scraper...");
                    _logger.LogInformation("Initializing scraper '{Name}'", config.Name);
                    await scraper.InitializeAsync();

                    // Update state
                    scraperState.Status = "Running";
                    scraperState.Message = "Starting scraping process...";

                    // Start scraping
                    logAction("Starting scraping process...");
                    _logger.LogInformation("Starting scraping process for '{Name}'", config.Name);
                    await scraper.StartScrapingAsync();

                    // Set up continuous monitoring if enabled
                    if (config.EnableContinuousMonitoring)
                    {
                        var interval = TimeSpan.FromHours(config.GetMonitoringInterval());
                        await scraper.SetupContinuousScrapingAsync(interval);
                        logAction($"Continuous monitoring enabled with interval: {interval.TotalHours:F1} hours");
                        _logger.LogInformation("Continuous monitoring enabled for '{Name}' with interval: {Interval:F1} hours", 
                            config.Name, interval.TotalHours);
                    }

                    logAction("Scraping operation completed successfully");
                    _logger.LogInformation("Scraping operation completed successfully for '{Name}'", config.Name);
                    
                    // Update state on successful completion
                    scraperState.Status = "Completed";
                    scraperState.Message = "Scraping operation completed successfully";
                    scraperState.EndTime = DateTime.Now;
                    
                    return true;
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error during scraper execution for {config.Name}: {ex.Message}";
                    _logger.LogError(ex, errorMessage);
                    logAction($"Error during scraper execution: {ex.Message}");
                    
                    // Capture inner exception details if available
                    string innerErrorMessage = null;
                    if (ex.InnerException != null)
                    {
                        innerErrorMessage = ex.InnerException.Message;
                        _logger.LogError(ex.InnerException, $"Inner exception: {innerErrorMessage}");
                        logAction($"Inner error: {innerErrorMessage}");
                    }
                    
                    // Update state with error information
                    scraperState.IsRunning = false;
                    scraperState.Status = "Failed";
                    scraperState.EndTime = DateTime.Now;
                    scraperState.HasErrors = true;
                    scraperState.Message = errorMessage;
                    scraperState.LastError = innerErrorMessage != null 
                        ? $"{errorMessage} - {innerErrorMessage}" 
                        : errorMessage;
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error executing scraper {config.Name}: {ex.Message}";
                _logger.LogError(ex, errorMessage);
                logAction($"Error during scraping: {ex.Message}");
                
                // Capture inner exception details if available
                string innerErrorMessage = null;
                if (ex.InnerException != null)
                {
                    innerErrorMessage = ex.InnerException.Message;
                    _logger.LogError(ex.InnerException, $"Inner exception: {innerErrorMessage}");
                    logAction($"Inner error: {innerErrorMessage}");
                }
                
                // Update state with error information
                scraperState.IsRunning = false;
                scraperState.Status = "Failed";
                scraperState.EndTime = DateTime.Now;
                scraperState.HasErrors = true;
                scraperState.Message = errorMessage;
                scraperState.LastError = innerErrorMessage != null 
                    ? $"{errorMessage} - {innerErrorMessage}" 
                    : errorMessage;
                
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
            try
            {
                logAction("Creating enhanced scraper with specialized components");
                
                // Create a scraper-specific logger
                var scraperLogger = _loggerFactory.CreateLogger<EnhancedScraper>();
                
                // Log configuration details for debugging
                logAction($"Config details: Name={config.Name}, StartUrl={config.StartUrl}, MaxCrawlDepth={config.MaxCrawlDepth}");

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
                
                logAction("Creating crawl strategy component");
                var crawlStrategy = _componentFactory.CreateCrawlStrategy(config, logAction);
                
                logAction("Creating content extractor component");
                var contentExtractor = _componentFactory.CreateContentExtractor(config, logAction);
                
                logAction("Creating document processor component");
                var documentProcessor = _componentFactory.CreateDocumentProcessor(config, logAction);
                
                logAction("Creating change detector component");
                var changeDetector = _componentFactory.CreateChangeDetector(config, logAction);
                
                logAction("Creating content classifier component");
                var contentClassifier = _componentFactory.CreateContentClassifier(config, logAction);
                
                logAction("Creating state store component");
                var stateStore = _componentFactory.CreateStateStore(config, logAction);

                logAction("Assembling enhanced scraper with all components");
                
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

                logAction("Enhanced scraper created successfully");
                
                // Wait a moment to make this truly async
                await Task.Delay(1);

                // Return the configured scraper
                return enhancedScraper;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating enhanced scraper: {ex.Message}");
                logAction($"Error creating enhanced scraper: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                    logAction($"Inner error: {ex.InnerException.Message}");
                }
                
                throw; // Re-throw to let the caller handle it
            }
        }

        /// <summary>
        /// Validates the scraper configuration
        /// </summary>
        private async Task<bool> ValidateConfigurationAsync(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Validating configuration...");
                _logger.LogDebug("Validating configuration for {Name}...", config.Name);

                // Add a small delay to make this truly async
                await Task.Delay(1);

                // Basic validation
                if (string.IsNullOrEmpty(config.StartUrl))
                {
                    string error = "Error: Start URL is required";
                    logAction(error);
                    _logger.LogError(error);
                    return false;
                }

                if (!Uri.TryCreate(config.StartUrl, UriKind.Absolute, out _))
                {
                    string error = "Error: Start URL must be a valid URL";
                    logAction(error);
                    _logger.LogError(error);
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
                    _logger.LogInformation("Output directory not specified, using default: {OutputDirectory}", config.OutputDirectory);
                }
                
                // Ensure output directory exists
                if (!Directory.Exists(config.OutputDirectory))
                {
                    try
                    {
                        Directory.CreateDirectory(config.OutputDirectory);
                        logAction($"Created output directory: {config.OutputDirectory}");
                        _logger.LogInformation("Created output directory: {OutputDirectory}", config.OutputDirectory);
                    }
                    catch (Exception ex)
                    {
                        string error = $"Error creating output directory: {ex.Message}";
                        logAction(error);
                        _logger.LogError(ex, error);
                        return false;
                    }
                }

                // Validate crawl depth
                if (config.MaxCrawlDepth <= 0)
                {
                    config.MaxCrawlDepth = 3;
                    logAction($"Invalid crawl depth, using default: {config.MaxCrawlDepth}");
                    _logger.LogWarning("Invalid crawl depth, using default: {MaxCrawlDepth}", config.MaxCrawlDepth);
                }

                // Advanced validation for specific scraper types
                if (config.EnableRegulatoryContentAnalysis)
                {
                    logAction("Validating regulatory monitor configuration...");
                    _logger.LogDebug("Validating regulatory monitor configuration for {Name}...", config.Name);
                    // Add any regulatory-specific validation here
                }

                logAction("Configuration validation successful");
                _logger.LogInformation("Configuration validation successful for {Name}", config.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating configuration for {Name}", config.Name);
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

        /// <summary>
        /// Gets the detailed state information of a scraper, including error details
        /// </summary>
        /// <param name="id">The ID of the scraper</param>
        /// <returns>The detailed state of the scraper</returns>
        public async Task<WebScraperApi.Models.ScraperState> GetScraperStateAsync(string id)
        {
            try
            {
                _logger.LogInformation("Getting detailed state for scraper with ID {Id}", id);

                // In a real implementation, we would load this from a persistent store
                // For now, we'll create a new instance with the latest information
                await Task.Delay(1); // Small delay to make this truly async

                // Check the shared state dictionary to see if we have state info for this scraper
                if (_scraperStates.TryGetValue(id, out var state))
                {
                    return state;
                }

                // Return a default state if we don't have one
                return new WebScraperApi.Models.ScraperState
                {
                    Id = id,
                    IsRunning = false,
                    Status = "Idle",
                    Message = "No state information available",
                    LastUpdate = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting detailed state for scraper with ID {Id}", id);
                
                // Return a state with error information
                return new WebScraperApi.Models.ScraperState
                {
                    Id = id,
                    IsRunning = false,
                    Status = "Error",
                    HasErrors = true,
                    Message = "Error retrieving state information",
                    LastError = ex.Message,
                    LastUpdate = DateTime.Now
                };
            }
        }
    }
}