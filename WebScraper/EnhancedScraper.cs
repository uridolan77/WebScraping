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
        /// Compute a hash of the content for comparison
        /// </summary>
        private string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        /// <summary>
        /// Check if a URL should be crawled based on configuration and custom strategy
        /// </summary>
        private bool ShouldCrawlUrl(string url)
        {
            // Check if URL is null or empty
            if (string.IsNullOrEmpty(url))
                return false;

            // Use custom crawl strategy if available
            if (_customCrawlStrategy != null)
                return _customCrawlStrategy.ShouldCrawl(url);

            // Default implementation
            try
            {
                var uri = new Uri(url);

                // Check if URL is in allowed domain
                bool inAllowedDomain = _config.AllowedDomains?.Any(domain => 
                    uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) || 
                    uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase)) ?? false;

                // If no allowed domains specified, just use the domain from the start URL
                if (_config.AllowedDomains == null || _config.AllowedDomains.Count == 0)
                {
                    var startUri = new Uri(_config.StartUrl);
                    inAllowedDomain = uri.Host.Equals(startUri.Host, StringComparison.OrdinalIgnoreCase);
                }

                // Check if URL matches any exclude patterns
                bool isExcluded = _config.ExcludeUrlPatterns?.Any(pattern => 
                    url.Contains(pattern, StringComparison.OrdinalIgnoreCase)) ?? false;

                // Check if URL matches the document types we're looking for
                bool isTargetDocumentType = false;
                if (_config.ProcessPdfDocuments && url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                    isTargetDocumentType = true;
                else if (_config.ProcessOfficeDocuments && IsOfficeDocument(url))
                    isTargetDocumentType = true;

                return inAllowedDomain && !isExcluded || isTargetDocumentType;
            }
            catch
            {
                // If we can't parse the URL, don't crawl it
                return false;
            }
        }
        
        /// <summary>
        /// Check if a URL points to an Office document
        /// </summary>
        private bool IsOfficeDocument(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            string lowerUrl = url.ToLowerInvariant();
            return lowerUrl.EndsWith(".doc") || lowerUrl.EndsWith(".docx") || 
                   lowerUrl.EndsWith(".xls") || lowerUrl.EndsWith(".xlsx") || 
                   lowerUrl.EndsWith(".ppt") || lowerUrl.EndsWith(".pptx") ||
                   lowerUrl.EndsWith(".odt") || lowerUrl.EndsWith(".ods") ||
                   lowerUrl.EndsWith(".odp");
        }

        /// <summary>
        /// Get the file name from a URL
        /// </summary>
        private string GetFileNameFromUrl(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                string path = uri.AbsolutePath;
                return System.IO.Path.GetFileName(path);
            }
            catch
            {
                // If parsing fails, return a default name
                return "document";
            }
        }

        /// <summary>
        /// Get content type based on file extension
        /// </summary>
        private string GetContentTypeFromUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "application/octet-stream";

            string lowerUrl = url.ToLowerInvariant();
            
            if (lowerUrl.EndsWith(".pdf"))
                return "application/pdf";
            else if (lowerUrl.EndsWith(".doc") || lowerUrl.EndsWith(".docx"))
                return "application/msword";
            else if (lowerUrl.EndsWith(".xls") || lowerUrl.EndsWith(".xlsx"))
                return "application/vnd.ms-excel";
            else if (lowerUrl.EndsWith(".ppt") || lowerUrl.EndsWith(".pptx"))
                return "application/vnd.ms-powerpoint";
            else if (lowerUrl.EndsWith(".odt"))
                return "application/vnd.oasis.opendocument.text";
            else if (lowerUrl.EndsWith(".ods"))
                return "application/vnd.oasis.opendocument.spreadsheet";
            else if (lowerUrl.EndsWith(".odp"))
                return "application/vnd.oasis.opendocument.presentation";
            else if (lowerUrl.EndsWith(".html") || lowerUrl.EndsWith(".htm"))
                return "text/html";
            else if (lowerUrl.EndsWith(".txt"))
                return "text/plain";
            else
                return "application/octet-stream";
        }

        /// <summary>
        /// Update the scraper state in persistent storage
        /// </summary>
        private async Task UpdateScraperStateAsync(string status)
        {
            try
            {
                if (_persistentStateManager != null)
                {
                    var state = new ScraperState
                    {
                        ScraperId = _config.Name,
                        Status = status,
                        LastRunStartTime = DateTime.Now,
                        LastRunEndTime = DateTime.Now,
                        ProgressData = System.Text.Json.JsonSerializer.Serialize(new 
                        { 
                            TotalUrlsProcessed = _urlsProcessed,
                            ProcessedItems = _processingPipeline?.GetStatus()?.ProcessedItems ?? 0,
                            FailedItems = _processingPipeline?.GetStatus()?.FailedItems ?? 0
                        }),
                        ConfigSnapshot = System.Text.Json.JsonSerializer.Serialize(_config)
                    };

                    await _persistentStateManager.SaveScraperStateAsync(state);
                    LogInfo($"Scraper state updated: {status}");
                }
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

        /// <summary>
        /// Set the headless browser handler for this scraper
        /// </summary>
        public void SetHeadlessBrowserHandler(HeadlessBrowser.HeadlessBrowserHandler handler)
        {
            _headlessBrowser = handler ?? throw new System.ArgumentNullException(nameof(handler));
            LogInfo("Headless browser handler set");
        }
        
        /// <summary>
        /// Set the state manager for this scraper
        /// </summary>
        public void SetStateManager(IStateManager stateManager)
        {
            _persistentStateManager = stateManager as PersistentStateManager 
                ?? throw new System.ArgumentException("State manager must be a PersistentStateManager", nameof(stateManager));
            LogInfo("State manager set");
        }
        
        /// <summary>
        /// Set the crawl strategy for this scraper
        /// </summary>
        public void SetCrawlStrategy(ICrawlStrategy crawlStrategy)
        {
            if (crawlStrategy == null)
                throw new System.ArgumentNullException(nameof(crawlStrategy));
                
            _customCrawlStrategy = crawlStrategy;
            LogInfo("Crawl strategy set");
        }
        
        /// <summary>
        /// Set the change detector for this scraper
        /// </summary>
        public void SetChangeDetector(IChangeDetector changeDetector)
        {
            if (changeDetector == null)
                throw new System.ArgumentNullException(nameof(changeDetector));
                
            _customChangeDetector = changeDetector;
            LogInfo("Change detector set");
        }
        
        /// <summary>
        /// Set the rate limiter for this scraper
        /// </summary>
        public void SetRateLimiter(RateLimiting.AdaptiveRateLimiter rateLimiter)
        {
            if (rateLimiter == null)
                throw new System.ArgumentNullException(nameof(rateLimiter));
                
            LogInfo("Rate limiter set");
        }
        
        /// <summary>
        /// Set document handlers for this scraper
        /// </summary>
        public void SetDocumentHandlers(
            RegulatoryContent.PdfDocumentHandler pdfHandler = null,
            RegulatoryContent.OfficeDocumentHandler officeHandler = null)
        {
            if (pdfHandler != null)
            {
                _pdfDocumentHandler = pdfHandler;
                LogInfo("PDF document handler set");
            }
            
            if (officeHandler != null)
            {
                _officeDocumentHandler = officeHandler;
                LogInfo("Office document handler set");
            }
        }
        
        /// <summary>
        /// Set the configuration validator for this scraper
        /// </summary>
        public void SetConfigurationValidator(Validation.ConfigurationValidator validator)
        {
            if (validator == null)
                throw new System.ArgumentNullException(nameof(validator));
                
            _configurationValidator = validator;
            LogInfo("Configuration validator set");
        }
        
        /// <summary>
        /// Stops the scraping process
        /// </summary>
        public async Task StopScrapingAsync()
        {
            try
            {
                LogInfo("Stopping scraping process...");
                
                // Cancel any running tasks
                if (_processingPipeline != null)
                {
                    await _processingPipeline.CompleteAsync();
                }
                
                // Update scraper state
                await UpdateScraperStateAsync("Stopped");
                
                LogInfo("Scraping process stopped");
            }
            catch (System.Exception ex)
            {
                LogError(ex, "Error stopping scraper");
            }
        }

        /// <summary>
        /// Save changed content with version tracking
        /// </summary>
        /// <param name="url">URL of the content</param>
        /// <param name="content">HTML content to save</param>
        /// <param name="textContent">Plain text version of the content</param>
        /// <param name="title">Title of the page</param>
        /// <returns>True if content was saved and change was detected</returns>
        public async Task<bool> SaveChangedContentAsync(string url, string content, string textContent, string title = null)
        {
            try
            {
                LogInfo($"Saving content for {url}");
                
                // Use state store if available
                if (_stateStore != null)
                {
                    // Create a new page version
                    var pageVersion = new RegulatoryFramework.Interfaces.PageVersion
                    {
                        Url = url,
                        Hash = ComputeHash(content),
                        CapturedAt = DateTime.Now,
                        FullContent = content,
                        TextContent = textContent,
                        ContentSummary = textContent?.Substring(0, Math.Min(200, textContent?.Length ?? 0)) ?? "",
                        Metadata = new Dictionary<string, object>
                        {
                            ["title"] = title ?? GetFileNameFromUrl(url),
                            ["scraperId"] = _config.ScraperId ?? _config.Name,
                            ["scraperType"] = _config.ScraperType
                        }
                    };
                    
                    // Save the version
                    await _stateStore.SaveVersionAsync(pageVersion);
                    LogInfo($"Content saved for {url}");
                    return true;
                }
                // Use persistent state manager as fallback
                else if (_persistentStateManager != null)
                {
                    var contentItem = new ContentItem
                    {
                        Url = url,
                        Title = title ?? GetFileNameFromUrl(url),
                        ScraperId = _config.Name,
                        LastStatusCode = 200,
                        ContentType = "text/html",
                        IsReachable = true,
                        RawContent = content,
                        ContentHash = ComputeHash(content)
                    };
                    
                    await _persistentStateManager.SaveContentVersionAsync(contentItem, _config.MaxVersionsToKeep);
                    LogInfo($"Content saved for {url} using state manager");
                    return true;
                }
                // Use change detector directly as last resort
                else if (_customChangeDetector != null && _customChangeDetector is ContentChange.ContentChangeDetector changeDetector)
                {
                    var version = changeDetector.TrackPageVersion(url, content, textContent, _config.ScraperId);
                    LogInfo($"Content tracked for {url} using change detector");
                    return version.ChangeFromPrevious != ContentChange.ChangeType.None;
                }
                else
                {
                    LogWarning($"No state store available to save content for {url}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error saving content for {url}");
                return false;
            }
        }
        
        /// <summary>
        /// Logs an information message
        /// </summary>
        private void LogInfo(string message)
        {
            // Use the enhanced logger if available
            if (_enhancedLogger != null)
            {
                _enhancedLogger.LogInformation(message);
            }
            else
            {
                // Fall back to the action logger from the base class
                _logger(message);
            }
        }
        
        /// <summary>
        /// Logs a warning message
        /// </summary>
        private void LogWarning(string message)
        {
            // Use the enhanced logger if available
            if (_enhancedLogger != null)
            {
                _enhancedLogger.LogWarning(message);
            }
            else
            {
                // Fall back to the action logger from the base class
                _logger($"WARNING: {message}");
            }
        }
        
        /// <summary>
        /// Logs an error message
        /// </summary>
        private void LogError(Exception ex, string message)
        {
            // Use the enhanced logger if available
            if (_enhancedLogger != null)
            {
                _enhancedLogger.LogError(ex, message);
            }
            else
            {
                // Fall back to the action logger from the base class
                _logger($"ERROR: {message} - {ex.Message}");
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
                _continuousScrapingCts?.Cancel();
                
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