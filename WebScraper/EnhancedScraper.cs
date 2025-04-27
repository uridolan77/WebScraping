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

namespace WebScraper
{
    /// <summary>
    /// Enhanced scraper with regulatory monitoring capabilities
    /// </summary>
    public class EnhancedScraper : Scraper
    {
        private readonly ILogger<EnhancedScraper> _enhancedLogger;
        private readonly ICrawlStrategy _customCrawlStrategy;
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
        protected override async Task ProcessUrlAsync(string url)
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
        /// Process a URL using the headless browser
        /// </summary>
        private async Task ProcessWithHeadlessBrowserAsync(string url)
        {
            LogInfo($"Processing with headless browser: {url}");
            
            string contextId = null;
            string pageId = null;
            
            try
            {
                // Create browser context and page
                contextId = await _headlessBrowser.CreateContextAsync();
                pageId = await _headlessBrowser.CreatePageAsync(contextId);
                
                // Navigate to URL
                var navResult = await _headlessBrowser.NavigateToUrlAsync(contextId, pageId, url, NavigationWaitUntil.NetworkIdle);
                
                if (!navResult.Success)
                {
                    LogWarning($"Navigation failed: {navResult.ErrorMessage}");
                    return;
                }
                
                // Wait for content to stabilize
                await Task.Delay(1000);
                
                // Extract content
                var content = await _headlessBrowser.ExtractContentAsync(contextId, pageId);
                
                LogInfo($"Extracted {content.TextContent?.Length ?? 0} chars from {url}");
                
                // Save content to state manager if available
                if (_persistentStateManager != null)
                {
                    var contentItem = new ContentItem
                    {
                        Url = url,
                        Title = content.Title,
                        ScraperId = _config.Name,
                        LastStatusCode = navResult.StatusCode,
                        ContentType = "text/html",
                        IsReachable = true,
                        RawContent = content.HtmlContent,
                        ContentHash = ComputeHash(content.HtmlContent)
                    };
                    
                    await _persistentStateManager.SaveContentVersionAsync(contentItem, _config.MaxVersionsToKeep);
                }
                
                // Process links if we should continue crawling
                if (_currentDepth.Value < _config.MaxDepth)
                {
                    foreach (var link in content.Links)
                    {
                        if (link.IsVisible && !string.IsNullOrEmpty(link.Href))
                        {
                            // Resolve relative URLs
                            string absoluteUrl = new Uri(new Uri(url), link.Href).ToString();
                            
                            // Add to processing queue using base methods
                            if (ShouldCrawlUrl(absoluteUrl))
                            {
                                await _processingPipeline.TryAddAsync(absoluteUrl);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing with headless browser: {url}");
            }
            finally
            {
                // Clean up resources
                if (pageId != null && contextId != null)
                {
                    await _headlessBrowser.ClosePageAsync(contextId, pageId);
                }
                
                if (contextId != null)
                {
                    await _headlessBrowser.CloseContextAsync(contextId);
                }
            }
        }
        
        /// <summary>
        /// Process regulatory content for a URL
        /// </summary>
        private async Task ProcessRegulatoryContentAsync(string url)
        {
            try
            {
                // Fetch the HTML document from the URL
                var httpClient = new System.Net.Http.HttpClient();
                var htmlContent = await httpClient.GetStringAsync(url);
                
                // Parse the HTML
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);
                
                // Get text content - use either our custom extractor or fallback to node text
                string textContent = htmlDoc.DocumentNode.InnerText;
                if (_customContentExtractor != null)
                {
                    textContent = _customContentExtractor.ExtractTextContent(htmlDoc);
                }
                
                // Process with regulatory monitor
                var regulatoryDoc = await _regulationMonitor.ProcessRegulatoryPage(
                    url, htmlDoc, htmlContent, textContent);
                
                // Log the result
                if (regulatoryDoc != null)
                {
                    LogInfo(
                        $"Processed regulatory document: {regulatoryDoc.Title} " +
                        $"(Type: {regulatoryDoc.DocumentType}, Importance: {regulatoryDoc.Importance})");
                }
                
                // If tracking regulatory changes is enabled
                if (_config.TrackRegulatoryChanges && _persistentStateManager != null)
                {
                    // Get previous version if available
                    var previousResult = await _persistentStateManager.GetLatestContentVersionAsync(url);
                    var previousVersion = previousResult.Version;
                    
                    // Check if it has content before processing
                    if (previousVersion != null && !string.IsNullOrEmpty(previousVersion.RawContent))
                    {
                        // Compare with current content and detect changes
                        var changeResult = await _regulationMonitor.MonitorForChanges(
                            url, previousVersion.RawContent, htmlContent);
                            
                        if (changeResult != null && changeResult.RegulatoryImpact > RegulatoryImpact.None)
                        {
                            LogInfo($"Detected {changeResult.RegulatoryImpact} impact change at {url}");
                            
                            // Process high impact changes
                            if (_config.MonitorHighImpactChanges && 
                                changeResult.RegulatoryImpact >= RegulatoryImpact.Medium)
                            {
                                LogWarning("High-impact regulatory change detected!");
                                LogWarning(changeResult.ImpactSummary);
                                
                                // Here you could integrate with notification systems
                                // or regulatory compliance workflows
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing regulatory content for {url}");
            }
        }
        
        /// <summary>
        /// Process document URLs (PDF, Office documents)
        /// </summary>
        private async Task ProcessDocumentsAsync(string url)
        {
            try
            {
                // Check if URL points to a PDF document
                if (_config.ProcessPdfDocuments && url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    await ProcessPdfDocumentAsync(url);
                }
                
                // Check if URL points to an Office document
                else if (IsOfficeDocument(url))
                {
                    await ProcessOfficeDocumentAsync(url);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing document: {url}");
            }
        }
        
        /// <summary>
        /// Process a PDF document
        /// </summary>
        private async Task ProcessPdfDocumentAsync(string url)
        {
            if (_pdfDocumentHandler != null)
            {
                try
                {
                    LogInfo($"Processing PDF document: {url}");
                    
                    // Extract text from PDF
                    string extractedText = await _pdfDocumentHandler.ExtractTextFromPdfUrl(url);
                    
                    // Save content to state manager if available
                    if (_persistentStateManager != null && !string.IsNullOrEmpty(extractedText))
                    {
                        var contentItem = new ContentItem
                        {
                            Url = url,
                            Title = GetFileNameFromUrl(url),
                            ScraperId = _config.Name,
                            LastStatusCode = 200,
                            ContentType = "application/pdf",
                            IsReachable = true,
                            RawContent = extractedText,
                            ContentHash = ComputeHash(extractedText),
                            IsRegulatoryContent = _config.ClassifyRegulatoryDocuments
                        };
                        
                        await _persistentStateManager.SaveContentVersionAsync(contentItem, _config.MaxVersionsToKeep);
                    }
                    
                    LogInfo($"Processed PDF document: {url} ({extractedText?.Length ?? 0} chars)");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error processing PDF document {url}");
                }
            }
            else if (_regulationMonitor != null)
            {
                // Use PdfDocumentHandler from GamblingRegulationMonitor as fallback
                LogInfo($"Processing PDF document using RegulatoryContent handler: {url}");
            }
        }
        
        /// <summary>
        /// Process an Office document
        /// </summary>
        private async Task ProcessOfficeDocumentAsync(string url)
        {
            if (_officeDocumentHandler != null)
            {
                try
                {
                    LogInfo($"Processing Office document: {url}");
                    
                    // Extract text from Office document
                    string extractedText = await _officeDocumentHandler.ExtractTextFromDocument(url);
                    
                    // Save content to state manager if available
                    if (_persistentStateManager != null && !string.IsNullOrEmpty(extractedText))
                    {
                        var contentItem = new ContentItem
                        {
                            Url = url,
                            Title = GetFileNameFromUrl(url),
                            ScraperId = _config.Name,
                            LastStatusCode = 200,
                            ContentType = GetContentTypeFromUrl(url),
                            IsReachable = true,
                            RawContent = extractedText,
                            ContentHash = ComputeHash(extractedText),
                            IsRegulatoryContent = _config.ClassifyRegulatoryDocuments
                        };
                        
                        await _persistentStateManager.SaveContentVersionAsync(contentItem, _config.MaxVersionsToKeep);
                    }
                    
                    LogInfo($"Processed Office document: {url} ({extractedText?.Length ?? 0} chars)");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error processing Office document {url}");
                }
            }
        }
        
        /// <summary>
        /// Update the scraper state in the persistent store
        /// </summary>
        private async Task UpdateScraperStateAsync(string status)
        {
            if (_persistentStateManager == null)
                return;
            
            try
            {
                // Get the current state
                var currentState = await _persistentStateManager.GetScraperStateAsync(_config.Name);
                
                if (currentState == null)
                {
                    currentState = new ScraperState
                    {
                        ScraperId = _config.Name,
                        LastRunStartTime = DateTime.Now,
                        ConfigSnapshot = System.Text.Json.JsonSerializer.Serialize(_config)
                    };
                }
                
                // Update state properties
                currentState.Status = status;
                currentState.LastRunEndTime = DateTime.Now;
                
                if (status == "Completed")
                {
                    currentState.LastSuccessfulRunTime = DateTime.Now;
                }
                
                // Save the updated state
                await _persistentStateManager.SaveScraperStateAsync(currentState);
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to update scraper state");
            }
        }
        
        /// <summary>
        /// Public method to process a URL that can be called from external code
        /// </summary>
        public async Task ProcessUrl(string url)
        {
            await ProcessUrlAsync(url);
        }
        
        /// <summary>
        /// Check if a URL points to an Office document
        /// </summary>
        private bool IsOfficeDocument(string url)
        {
            var lowercaseUrl = url.ToLowerInvariant();
            return lowercaseUrl.EndsWith(".docx") ||
                   lowercaseUrl.EndsWith(".xlsx") ||
                   lowercaseUrl.EndsWith(".pptx") ||
                   lowercaseUrl.EndsWith(".doc") ||
                   lowercaseUrl.EndsWith(".xls") ||
                   lowercaseUrl.EndsWith(".ppt");
        }
        
        /// <summary>
        /// Get the file name from a URL
        /// </summary>
        private string GetFileNameFromUrl(string url)
        {
            try
            {
                var uri = new Uri(url);
                return System.IO.Path.GetFileName(uri.LocalPath);
            }
            catch
            {
                return url.Split('/').Last();
            }
        }
        
        /// <summary>
        /// Get the content type from a URL based on its extension
        /// </summary>
        private string GetContentTypeFromUrl(string url)
        {
            var lowercaseUrl = url.ToLowerInvariant();
            
            if (lowercaseUrl.EndsWith(".pdf"))
                return "application/pdf";
            else if (lowercaseUrl.EndsWith(".docx") || lowercaseUrl.EndsWith(".doc"))
                return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
            else if (lowercaseUrl.EndsWith(".xlsx") || lowercaseUrl.EndsWith(".xls"))
                return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            else if (lowercaseUrl.EndsWith(".pptx") || lowercaseUrl.EndsWith(".ppt"))
                return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
            else
                return "application/octet-stream";
        }
        
        /// <summary>
        /// Compute a hash for content
        /// </summary>
        private string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
            
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var hash = sha.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        /// <summary>
        /// Calls Dispose on all disposable resources
        /// </summary>
        public override void Dispose()
        {
            // Dispose persistent state manager
            _persistentStateManager?.Dispose();
            
            // Dispose headless browser
            _headlessBrowser?.Dispose();
            
            // Dispose pipeline
            _processingPipeline?.Dispose();
            
            // Call base dispose
            base.Dispose();
        }
        
        /// <summary>
        /// Log information message using the appropriate logger
        /// </summary>
        private void LogInfo(string message)
        {
            if (_enhancedLogger != null)
                _enhancedLogger.LogInformation(message);
            else
                _logger(message);
        }
        
        /// <summary>
        /// Log warning message using the appropriate logger
        /// </summary>
        private void LogWarning(string message)
        {
            if (_enhancedLogger != null)
                _enhancedLogger.LogWarning(message);
            else
                _logger($"WARNING: {message}");
        }
        
        /// <summary>
        /// Log error message using the appropriate logger
        /// </summary>
        private void LogError(Exception ex, string message)
        {
            if (_enhancedLogger != null)
                _enhancedLogger.LogError(ex, message);
            else
                _logger($"ERROR: {message} - {ex.Message}");
        }
        
        /// <summary>
        /// Get statistics about regulatory monitoring
        /// </summary>
        public string GetRegulatoryStatistics()
        {
            if (_regulationMonitor != null)
            {
                return _regulationMonitor.GetRegulatoryStatistics();
            }
            
            return "Regulatory monitoring not enabled";
        }
        
        /// <summary>
        /// Get high importance regulatory documents
        /// </summary>
        public List<GamblingRegulationMonitor.RegulatoryDocument> GetHighImportanceDocuments()
        {
            if (_regulationMonitor != null)
            {
                return _regulationMonitor.GetHighImportanceDocuments();
            }
            
            return new List<GamblingRegulationMonitor.RegulatoryDocument>();
        }
        
        /// <summary>
        /// Get current state information about the scraper
        /// </summary>
        public async Task<ScraperState> GetStateAsync()
        {
            if (_persistentStateManager != null)
            {
                try
                {
                    // Get state from persistent store
                    return await _persistentStateManager.GetScraperStateAsync(_config.Name);
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to get scraper state");
                }
            }
            
            // Fallback to basic state
            var state = new StateManagement.ScraperState
            {
                ScraperId = _config.Name,
                ConfigSnapshot = System.Text.Json.JsonSerializer.Serialize(_config),
                Status = "Unknown",
                UpdatedAt = DateTime.UtcNow
            };
            
            return state;
        }
        
        /// <summary>
        /// Get pipeline processing metrics
        /// </summary>
        public PipelineStatus GetPipelineStatus()
        {
            return _processingPipeline?.GetStatus();
        }
        
        /// <summary>
        /// Determines whether a URL should be crawled based on configuration and crawl strategy
        /// </summary>
        private bool ShouldCrawlUrl(string url)
        {
            try
            {
                // Skip if URL is null or empty
                if (string.IsNullOrEmpty(url))
                    return false;

                // Parse URL to make sure it's valid
                if (!Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
                    return false;
                
                // Check if scheme is HTTP or HTTPS
                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                    return false;
                
                // Check if external URL and if we should follow external links
                bool isExternal = !url.StartsWith(_config.BaseUrl);
                if (isExternal && !_config.FollowExternalLinks)
                    return false;
                
                // If using custom crawl strategy, use its logic
                if (_customCrawlStrategy != null)
                {
                    return _customCrawlStrategy.ShouldCrawl(url);
                }
                
                // Skip URLs that are likely to be non-content pages
                if (url.Contains("logout") || 
                    url.Contains("login") || 
                    url.Contains("signin") || 
                    url.Contains("register") ||
                    url.EndsWith(".jpg") || 
                    url.EndsWith(".jpeg") || 
                    url.EndsWith(".png") || 
                    url.EndsWith(".gif") ||
                    url.EndsWith(".css") || 
                    url.EndsWith(".js"))
                {
                    return false;
                }
                
                // If it's a UKGC website and we have regulatory monitoring enabled,
                // use more specific crawling rules
                if (_config.IsUKGCWebsite && _config.EnableRegulatoryContentAnalysis)
                {
                    // Prioritize regulatory content
                    if (url.Contains("regulations") || 
                        url.Contains("guidance") || 
                        url.Contains("compliance") || 
                        url.Contains("rules") ||
                        url.Contains("laws") || 
                        url.Contains("policies"))
                    {
                        return true;
                    }
                    
                    // Prioritize based on config options
                    if (_config.PrioritizeEnforcementActions && url.Contains("enforcement"))
                        return true;
                        
                    if (_config.PrioritizeLCCP && (url.Contains("lccp") || 
                        url.Contains("license-conditions") || 
                        url.Contains("code-of-practice")))
                        return true;
                        
                    if (_config.PrioritizeAML && (url.Contains("aml") || 
                        url.Contains("money-laundering")))
                        return true;
                }
                
                // By default, allow crawling
                return true;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error evaluating URL for crawling: {url}");
                return false;
            }
        }
        
        /// <summary>
        /// Configure content classification with a classifier
        /// </summary>
        public void ConfigureContentClassification(IContentClassifier classifier)
        {
            if (classifier == null)
                throw new ArgumentNullException(nameof(classifier));
                
            _contentClassifier = classifier;
            LogInfo("Content classification configured");
        }
        
        /// <summary>
        /// Configure change detection with a detector and state store
        /// </summary>
        public void ConfigureChangeDetection(IChangeDetector changeDetector, IStateStore stateStore)
        {
            if (changeDetector == null)
                throw new ArgumentNullException(nameof(changeDetector));
                
            if (stateStore == null)
                throw new ArgumentNullException(nameof(stateStore));
                
            _customChangeDetector = changeDetector;
            _stateStore = stateStore;
            LogInfo("Change detection configured");
        }
        
        /// <summary>
        /// Configure dynamic content rendering with a renderer
        /// </summary>
        public void ConfigureDynamicRendering(IDynamicContentRenderer renderer)
        {
            if (renderer == null)
                throw new ArgumentNullException(nameof(renderer));
                
            _dynamicRenderer = renderer;
            LogInfo("Dynamic content rendering configured");
        }
        
        /// <summary>
        /// Configure alert service for regulatory changes
        /// </summary>
        public void ConfigureAlertService(IAlertService alertService)
        {
            if (alertService == null)
                throw new ArgumentNullException(nameof(alertService));
                
            _alertService = alertService;
            LogInfo("Alert service configured");
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