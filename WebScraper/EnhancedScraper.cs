using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using HtmlAgilityPack;
using WebScraper.ContentChange;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;
using WebScraper.HeadlessBrowser;
using WebScraper.StateManagement;
using WebScraper.Processing;
using WebScraper.Validation;
using WebScraper.Interfaces;

namespace WebScraper
{
    /// <summary>
    /// Enhanced version of the Scraper class with additional regulatory features
    /// </summary>
    public class EnhancedScraper : Scraper
    {
        private readonly ILogger<EnhancedScraper> _enhancedLogger;
        private ICrawlStrategy _customCrawlStrategy;
        private readonly IContentExtractor _customContentExtractor;
        private readonly IDocumentProcessor _documentProcessor;
        
        // Changed from readonly to allow reconfiguration
        private IChangeDetector _customChangeDetector;
        private IStateStore _stateStore;
        
        // New components
        private HeadlessBrowserHandler _headlessBrowser;
        private PersistentStateManager _persistentStateManager;
        private OfficeDocumentHandler _officeDocumentHandler;
        private PdfDocumentHandler _pdfDocumentHandler;
        private ConfigurationValidator _configurationValidator;
        private AsyncPipeline<string, ProcessingResult> _processingPipeline;
        
        // GamblingRegulationMonitor for the RegulatoryContent implementation
        private GamblingRegulationMonitor _regulationMonitor;
        
        // Pipeline and processing management
        private int _maxConcurrentProcessing;
        private int _processingTimeoutMs;
        private string _dbConnectionString;
        
        // Crawl depth tracking
        private ThreadLocal<int> _currentDepth = new ThreadLocal<int>(() => 0);
        
        // New fields for RegulatoryFramework components
        private IContentClassifier _contentClassifier;
        private IDynamicContentRenderer _dynamicRenderer;
        private IAlertService _alertService;
        
        private System.Collections.Concurrent.ConcurrentDictionary<string, bool> _urlsProcessed = new System.Collections.Concurrent.ConcurrentDictionary<string, bool>();

        /// <summary>
        /// Initializes a new instance of the EnhancedScraper class with full dependency injection
        /// </summary>
        public EnhancedScraper(
            ScraperConfig config,
            ILogger<EnhancedScraper> logger,
            ICrawlStrategy crawlStrategy = null,
            IContentExtractor contentExtractor = null,
            IDocumentProcessor documentProcessor = null,
            IChangeDetector changeDetector = null,
            IStateStore stateStore = null)
            : base(config, message => logger.LogInformation(message))
        {
            _enhancedLogger = logger;
            _customCrawlStrategy = crawlStrategy;
            _customContentExtractor = contentExtractor;
            _documentProcessor = documentProcessor;
            _customChangeDetector = changeDetector;
            _stateStore = stateStore;
            
            // Set up processing parameters
            _maxConcurrentProcessing = config.MaxConcurrentRequests;
            _processingTimeoutMs = 60000;  // Default to 60 seconds
            _dbConnectionString = $"Data Source={_outputDirectory}/scraper_state.db";
            
            // Initialize regulatory monitor if config enables it
            if (config.EnableRegulatoryContentAnalysis)
            {
                InitializeRegulationMonitor();
            }
            
            logger.LogInformation("EnhancedScraper initialized with regulatory capabilities");
        }
        
        /// <summary>
        /// Alternative constructor with manually provided logAction
        /// </summary>
        public EnhancedScraper(
            ScraperConfig config, 
            Action<string> logAction,
            ICrawlStrategy crawlStrategy = null,
            IContentExtractor contentExtractor = null,
            IDocumentProcessor documentProcessor = null)
            : base(config, logAction)
        {
            _customCrawlStrategy = crawlStrategy;
            _customContentExtractor = contentExtractor;
            _documentProcessor = documentProcessor;
            
            // Set up processing parameters
            _maxConcurrentProcessing = config.MaxConcurrentRequests;
            _processingTimeoutMs = 60000;  // Default to 60 seconds
            _dbConnectionString = $"Data Source={_outputDirectory}/scraper_state.db";
            
            // Initialize regulatory monitor if config enables it
            if (config.EnableRegulatoryContentAnalysis)
            {
                InitializeRegulationMonitor();
            }
        }

        /// <summary>
        /// Initialize the scraper and its components
        /// </summary>
        public override async Task InitializeAsync()
        {
            // First, validate the configuration
            await ValidateConfiguration();
            
            // Initialize base scraper functionality
            await base.InitializeAsync();
            
            // Initialize persistent state manager
            await InitializePersistentStateManager();
            
            // Initialize document handlers
            InitializeDocumentHandlers();
            
            // Initialize headless browser if needed
            if (NeedsHeadlessBrowser())
            {
                await InitializeHeadlessBrowser();
            }
            
            // Initialize processing pipeline
            InitializeProcessingPipeline();
            
            // Log successful initialization of all components
            LogInfo("Enhanced scraper initialized with all components");
        }
        
        /// <summary>
        /// Validates the scraper configuration
        /// </summary>
        private async Task ValidateConfiguration()
        {
            LogInfo("Validating configuration...");
            
            _configurationValidator = new ConfigurationValidator(logger: LogInfo);
            var validationResult = await _configurationValidator.ValidateConfigurationAsync(_config);
            
            if (!validationResult.IsValid)
            {
                var errorMessage = string.Join("\n", validationResult.Errors);
                LogError(new InvalidOperationException(errorMessage), "Configuration validation failed");
                
                // Don't throw an exception here - instead, we'll log the errors and continue,
                // though some functionality may be limited
                LogWarning("Scraper will attempt to run with invalid configuration");
            }
            else if (validationResult.CanRunWithWarnings)
            {
                var warningMessage = string.Join("\n", validationResult.Warnings);
                LogWarning($"Configuration has warnings: \n{warningMessage}");
            }
            else
            {
                LogInfo("Configuration validation successful");
            }
        }
        
        /// <summary>
        /// Initialize the persistent state manager
        /// </summary>
        private async Task InitializePersistentStateManager()
        {
            try
            {
                LogInfo("Initializing persistent state manager...");
                
                _persistentStateManager = new PersistentStateManager(_dbConnectionString, LogInfo);
                await _persistentStateManager.InitializeAsync();
                
                // Save initial scraper state
                var initialState = new ScraperState
                {
                    ScraperId = _config.Name,
                    Status = "Initializing",
                    LastRunStartTime = DateTime.Now,
                    ProgressData = "{}",
                    ConfigSnapshot = System.Text.Json.JsonSerializer.Serialize(_config)
                };
                
                await _persistentStateManager.SaveScraperStateAsync(initialState);
                
                LogInfo("Persistent state manager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize persistent state manager");
            }
        }
        
        /// <summary>
        /// Initialize document handlers
        /// </summary>
        private void InitializeDocumentHandlers()
        {
            try
            {
                // Initialize PDF document handler
                if (_config.ProcessPdfDocuments)
                {
                    LogInfo("Initializing PDF document handler...");
                    _pdfDocumentHandler = new PdfDocumentHandler(_outputDirectory, logger: LogInfo);
                    LogInfo("PDF document handler initialized");
                }
                
                // Initialize Office document handler
                LogInfo("Initializing Office document handler...");
                _officeDocumentHandler = new OfficeDocumentHandler(_outputDirectory, logger: LogInfo);
                LogInfo("Office document handler initialized");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize document handlers");
            }
        }
        
        /// <summary>
        /// Initialize headless browser handler
        /// </summary>
        private async Task InitializeHeadlessBrowser()
        {
            try
            {
                LogInfo("Initializing headless browser...");
                
                var browserOptions = new HeadlessBrowserOptions
                {
                    Headless = true,
                    ScreenshotDirectory = System.IO.Path.Combine(_outputDirectory, "screenshots"),
                    BrowserType = BrowserType.Chromium,
                    TakeScreenshots = _config.IsUKGCWebsite, // Take screenshots for UKGC websites
                    UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36",
                    JavaScriptEnabled = true,
                    NavigationTimeout = 30000,
                    WaitTimeout = 30000
                };
                
                _headlessBrowser = new HeadlessBrowserHandler(browserOptions, LogInfo);
                await _headlessBrowser.InitializeAsync();
                
                LogInfo("Headless browser initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize headless browser");
            }
        }
        
        /// <summary>
        /// Initialize processing pipeline
        /// </summary>
        private void InitializeProcessingPipeline()
        {
            try
            {
                LogInfo("Initializing processing pipeline...");
                
                // Create processor function for the pipeline
                Func<string, Task<ProcessingResult>> processor = async (url) =>
                {
                    try
                    {
                        // Process URL
                        await ProcessUrlAsync(url);
                        
                        // Return a success result
                        return new ProcessingResult
                        {
                            Url = url,
                            Success = true
                        };
                    }
                    catch (Exception ex)
                    {
                        // Return a failure result
                        return new ProcessingResult
                        {
                            Url = url,
                            Success = false,
                            Error = ex.Message
                        };
                    }
                };
                
                // Create error handler for the pipeline
                Action<string, Exception> errorHandler = (url, ex) =>
                {
                    LogError(ex, $"Error processing URL: {url}");
                };
                
                // Create the pipeline
                _processingPipeline = new AsyncPipeline<string, ProcessingResult>(
                    processor,
                    _maxConcurrentProcessing,
                    _config.MaxConcurrentRequests * 2, // Set bounded capacity to twice the max concurrent requests
                    errorHandler,
                    LogInfo,
                    _processingTimeoutMs,
                    10000 // Report metrics every 10 seconds
                );
                
                LogInfo($"Processing pipeline initialized with {_maxConcurrentProcessing} concurrent processors");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize processing pipeline");
            }
        }
        
        /// <summary>
        /// Initialize the regulatory monitor based on configuration
        /// </summary>
        private void InitializeRegulationMonitor()
        {
            // Create a wrapping action for the logger to avoid null reference issues
            Action<string> loggingAction = message => {
                if (_enhancedLogger != null) 
                    _enhancedLogger.LogInformation(message);
                else 
                    _logger(message);
            };
            
            // Create the GamblingRegulationMonitor from the RegulatoryContent namespace
            _regulationMonitor = new GamblingRegulationMonitor(
                this,
                _outputDirectory,
                loggingAction);
                
            loggingAction("Regulatory monitoring initialized");
        }

        /// <summary>
        /// Check if headless browser is needed based on configuration
        /// </summary>
        private bool NeedsHeadlessBrowser()
        {
            return _config.IsUKGCWebsite || 
                   _config.EnableRegulatoryContentAnalysis ||
                   _config.ProcessPdfDocuments ||
                   _config.ProcessJsHeavyPages;
        }
        
        /// <summary>
        /// Override base method to use our processing pipeline instead
        /// </summary>
        public override async Task StartScrapingAsync()
        {
            LogInfo($"Starting enhanced scraping of {_config.StartUrl}");
            
            try
            {
                // Add the start URL to the processing pipeline
                bool accepted = await _processingPipeline.TryAddAsync(_config.StartUrl);
                
                if (!accepted)
                {
                    LogWarning("Failed to add start URL to processing pipeline");
                    return;
                }
                
                // Process URLs from the pipeline until complete
                while (true)
                {
                    // Get a result from the pipeline (with a 100ms timeout)
                    var result = await _processingPipeline.TryReceiveAsync(100);
                    
                    if (result.Success)
                    {
                        // Process the result
                        if (result.Value.Success)
                        {
                            LogInfo($"Successfully processed {result.Value.Url}");
                        }
                        else
                        {
                            LogWarning($"Failed to process {result.Value.Url}: {result.Value.Error}");
                        }
                    }
                    else if (_processingPipeline.GetStatus().ProcessingItems == 0)
                    {
                        // No items processing and no results available - we're done
                        break;
                    }
                    
                    // Yield to other tasks
                    await Task.Delay(10);
                }
                
                // Complete the pipeline
                await _processingPipeline.CompleteAsync();
                
                LogInfo("Scraping completed successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error during scraping");
            }
            finally
            {
                // Update the scraper state
                await UpdateScraperStateAsync("Completed");
            }
        }
        
        /// <summary>
        /// Override to process URLs with regulatory capabilities and our new components
        /// </summary>
        public override async Task ProcessUrlAsync(string url)
        {
            LogInfo($"Processing URL: {url}");
            
            try
            {
                // Check if URL has already been visited
                if (_persistentStateManager != null)
                {
                    bool alreadyVisited = await _persistentStateManager.HasUrlBeenVisitedAsync(_config.Name, url);
                    if (alreadyVisited)
                    {
                        LogInfo($"Skipping already visited URL: {url}");
                        return;
                    }
                }
                
                // Process with headless browser for dynamic content if available
                if (_headlessBrowser != null && NeedsHeadlessBrowser())
                {
                    await ProcessWithHeadlessBrowserAsync(url);
                }
                else
                {
                    // Use base implementation for static content
                    await base.ProcessUrlAsync(url);
                }
                
                // Track that we've visited this URL
                if (_persistentStateManager != null)
                {
                    await _persistentStateManager.MarkUrlVisitedAsync(_config.Name, url, 200, 0); // Assuming success
                }
                
                // Then, if regulatory content analysis is enabled, process with regulatory monitor
                if (_config.EnableRegulatoryContentAnalysis && _regulationMonitor != null)
                {
                    await ProcessRegulatoryContentAsync(url);
                }
                
                // Process documents if enabled and URL points to a document
                await ProcessDocumentsAsync(url);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing URL: {url}");
                
                // Track failed visit
                if (_persistentStateManager != null)
                {
                    await _persistentStateManager.MarkUrlVisitedAsync(_config.Name, url, 500, 0); // Assuming error
                }
            }
        }
        
        /// <summary>
        /// Dispose of resources
        /// </summary>
        public override void Dispose()
        {
            try
            {
                // Stop any continuous scraping
                StopContinuousScraping();
                
                // Dispose headless browser
                _headlessBrowser?.Dispose();
                
                // Dispose processing pipeline
                _processingPipeline?.Dispose();
                
                // Other disposals
                if (_dynamicRenderer is IDisposable dynamicRendererDisposable)
                {
                    dynamicRendererDisposable.Dispose();
                }
                
                // Cleanup
                _currentDepth?.Dispose();
            }
            catch (Exception ex)
            {
                LogError(ex, "Error during disposal");
            }
            finally
            {
                // Call base disposal
                base.Dispose();
            }
        }
        
        // Helper method to convert between ContentItem types to resolve ambiguity
        private Processing.ContentItem ToProcessingContentItem(Interfaces.ContentItem item)
        {
            if (item == null)
                return null;
                
            return new Processing.ContentItem
            {
                Url = item.Url,
                Title = item.Title,
                ScraperId = item.ScraperId,
                LastStatusCode = item.LastStatusCode,
                ContentType = item.ContentType,
                IsReachable = item.IsReachable,
                RawContent = item.RawContent,
                ContentHash = item.ContentHash,
                IsRegulatoryContent = item.IsRegulatoryContent
            };
        }
        
        // Helper method to convert between ContentItem types to resolve ambiguity
        private Interfaces.ContentItem ToInterfaceContentItem(Processing.ContentItem item)
        {
            if (item == null)
                return null;
                
            // Create a new implementation of the ContentItem interface
            return new StateManagement.ContentItemImpl
            {
                Url = item.Url,
                Title = item.Title,
                ScraperId = item.ScraperId,
                LastStatusCode = item.LastStatusCode,
                ContentType = item.ContentType,
                IsReachable = item.IsReachable,
                RawContent = item.RawContent,
                ContentHash = item.ContentHash,
                IsRegulatoryContent = item.IsRegulatoryContent
            };
        }
        
        // Helper method to handle DateTime? to DateTime conversion
        private DateTime ToDateTime(DateTime? dateTime)
        {
            return dateTime ?? DateTime.MinValue;
        }
    }
    
    /// <summary>
    /// Result of URL processing
    /// </summary>
    public class ProcessingResult
    {
        public string Url { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}