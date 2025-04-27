using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.ContentChange;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;

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
        private readonly IChangeDetector _customChangeDetector;
        private readonly IStateStore _stateStore;
        
        // GamblingRegulationMonitor for the RegulatoryContent implementation
        private GamblingRegulationMonitor _regulationMonitor;
        
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
            
            // Initialize regulatory monitor if config enables it
            if (config.EnableRegulatoryContentAnalysis)
            {
                InitializeRegulationMonitor();
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
        /// Override to process URLs with regulatory capabilities
        /// </summary>
        protected override async Task ProcessUrlAsync(string url)
        {
            // First, use the base implementation to process the URL
            await base.ProcessUrlAsync(url);
            
            // Then, if regulatory content analysis is enabled, process with regulatory monitor
            if (_config.EnableRegulatoryContentAnalysis && _regulationMonitor != null)
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
                    if (_config.TrackRegulatoryChanges && _stateStore != null)
                    {
                        // Get previous version if available
                        var previousVersion = await _stateStore.GetLatestVersionAsync(url);
                        
                        // Check if it has content before processing
                        if (previousVersion != null)
                        {
                            // Access the Content property through reflection if necessary
                            string previousContent = GetPageVersionContent(previousVersion);
                            
                            if (!string.IsNullOrEmpty(previousContent))
                            {
                                // Compare with current content and detect changes
                                var changeResult = await _regulationMonitor.MonitorForChanges(
                                    url, previousContent, htmlContent);
                                    
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
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Error processing regulatory content for {url}");
                }
            }
            
            // Process PDF documents if enabled
            if (_config.ProcessPdfDocuments && url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                await ProcessPdfDocumentAsync(url);
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
        /// Gets the content from a PageVersion object safely via reflection if needed
        /// </summary>
        private string GetPageVersionContent(object pageVersion)
        {
            // If it's our known PageVersion type
            if (pageVersion is WebScraper.ContentChange.PageVersion version)
            {
                return version.Content;
            }
            
            // Otherwise try reflection
            try
            {
                var contentProperty = pageVersion.GetType().GetProperty("Content");
                if (contentProperty != null)
                {
                    return contentProperty.GetValue(pageVersion) as string;
                }
            }
            catch
            {
                // Ignore reflection errors
            }
            
            return null;
        }
        
        private async Task ProcessPdfDocumentAsync(string url)
        {
            if (_documentProcessor != null)
            {
                try
                {
                    // Download PDF content
                    var httpClient = new System.Net.Http.HttpClient();
                    var pdfContent = await httpClient.GetByteArrayAsync(url);
                    
                    // Extract filename from URL
                    var uri = new Uri(url);
                    var fileName = System.IO.Path.GetFileName(uri.LocalPath);
                    
                    // Process with document processor
                    await _documentProcessor.ProcessDocumentAsync(url, fileName, pdfContent);
                    LogInfo($"Processed PDF document: {fileName}");
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
            var state = new ScraperState
            {
                ConfiguredDomain = _config.BaseUrl,
                EnabledFeatures = new Dictionary<string, bool>
                {
                    ["RegulatoryContentAnalysis"] = _config.EnableRegulatoryContentAnalysis,
                    ["TrackRegulatoryChanges"] = _config.TrackRegulatoryChanges,
                    ["ClassifyRegulatoryDocuments"] = _config.ClassifyRegulatoryDocuments,
                    ["ExtractStructuredContent"] = _config.ExtractStructuredContent,
                    ["ProcessPdfDocuments"] = _config.ProcessPdfDocuments,
                    ["MonitorHighImpactChanges"] = _config.MonitorHighImpactChanges,
                    ["AdaptiveCrawling"] = _config.EnableAdaptiveCrawling,
                    ["AdaptiveRateLimiting"] = _config.EnableAdaptiveRateLimiting
                }
            };
            
            return state;
        }
    }
    
    /// <summary>
    /// Represents the current state of the enhanced scraper
    /// </summary>
    public class ScraperState
    {
        public string ConfiguredDomain { get; set; }
        public Dictionary<string, bool> EnabledFeatures { get; set; }
    }
}