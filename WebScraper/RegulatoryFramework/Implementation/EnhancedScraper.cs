using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Implementation
{
    /// <summary>
    /// Enhanced scraper implemented with dependency injection
    /// </summary>
    public class EnhancedScraper
    {
        private readonly RegulatoryScraperConfig _config;
        private readonly ILogger<EnhancedScraper> _logger;
        private readonly ICrawlStrategy _crawlStrategy;
        private readonly IContentExtractor _contentExtractor;
        private readonly IDocumentProcessor _documentProcessor;
        private readonly IChangeDetector _changeDetector;
        private readonly IContentClassifier _contentClassifier;
        private readonly IDynamicContentRenderer _contentRenderer;
        private readonly IAlertService _alertService;
        private readonly IStateStore _stateStore;
        private readonly HttpClient _httpClient;
        
        public EnhancedScraper(
            RegulatoryScraperConfig config,
            ILogger<EnhancedScraper> logger,
            ICrawlStrategy crawlStrategy = null,
            IContentExtractor contentExtractor = null,
            IDocumentProcessor documentProcessor = null,
            IChangeDetector changeDetector = null,
            IContentClassifier contentClassifier = null,
            IDynamicContentRenderer contentRenderer = null,
            IAlertService alertService = null,
            IStateStore stateStore = null)
        {
            _config = config;
            _logger = logger;
            _crawlStrategy = crawlStrategy;
            _contentExtractor = contentExtractor;
            _documentProcessor = documentProcessor;
            _changeDetector = changeDetector;
            _contentClassifier = contentClassifier;
            _contentRenderer = contentRenderer;
            _alertService = alertService;
            _stateStore = stateStore;
            
            // Initialize HTTP client with appropriate settings
            _httpClient = new HttpClient(new RateLimitedHttpClientHandler(
                config.MaxConcurrentRequests,
                config.RequestTimeoutSeconds));
            
            _httpClient.DefaultRequestHeaders.Add("User-Agent", config.UserAgent);
        }
        
        /// <summary>
        /// Main method to process a URL
        /// </summary>
        public async Task ProcessUrlAsync(string url)
        {
            try
            {
                _logger.LogInformation("Processing URL: {Url}", url);
                
                // Determine content acquisition method based on configuration
                string content;
                HtmlDocument document = new HtmlDocument();
                
                if (_config.EnableDynamicContentRendering && _contentRenderer != null)
                {
                    // Render with headless browser
                    document = await _contentRenderer.GetRenderedDocumentAsync(url);
                    content = document.DocumentNode.OuterHtml;
                }
                else
                {
                    // Standard HTTP request
                    content = await _httpClient.GetStringAsync(url);
                    document.LoadHtml(content);
                }
                
                // Extract content based on configuration
                string textContent;
                List<ContentNode> structuredContent = null;
                
                if (_config.EnableHierarchicalExtraction && _contentExtractor != null)
                {
                    structuredContent = _contentExtractor.ExtractStructuredContent(document);
                    textContent = _contentExtractor.ExtractTextContent(document);
                }
                else
                {
                    // Simple text extraction
                    textContent = document.DocumentNode.InnerText;
                }
                
                // Process documents if enabled
                if (_config.EnableDocumentProcessing && _documentProcessor != null)
                {
                    await _documentProcessor.ProcessLinkedDocumentsAsync(url, document);
                }
                
                // Change detection
                PageVersion currentVersion = null;
                if (_changeDetector != null && _stateStore != null)
                {
                    // Get latest version if any
                    var previousVersion = await _stateStore.GetLatestVersionAsync(url);
                    
                    // Track current version
                    currentVersion = await _changeDetector.TrackPageVersionAsync(url, content, textContent);
                    
                    // Check for significant changes
                    if ((int)currentVersion.ChangeFromPrevious != (int)ChangeType.None)
                    {
                        // Handle change detection
                        await HandlePageChangeAsync(url, previousVersion, currentVersion);
                    }
                    
                    // Save version to state store
                    await _stateStore.SaveVersionAsync(currentVersion);
                }
                
                // Content classification
                if (_contentClassifier != null && _stateStore != null)
                {
                    var classification = _contentClassifier.ClassifyContent(url, textContent, document);
                    _logger.LogInformation("Classified {Url} as {Category}", url, classification.PrimaryCategory);
                    
                    // Store classification results
                    await _stateStore.SetAsync($"classification:{url}", classification);
                }
                
                // Update crawl strategy metadata
                if (_crawlStrategy != null)
                {
                    _crawlStrategy.UpdatePageMetadata(url, document, textContent);
                }
                
                _logger.LogInformation("Successfully processed: {Url}", url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing URL: {Url}", url);
            }
        }
        
        /// <summary>
        /// Prioritizes URLs for crawling
        /// </summary>
        public IEnumerable<string> PrioritizeUrls(List<string> urls, int maxUrls = 10)
        {
            if (_crawlStrategy == null)
            {
                _logger.LogWarning("No crawl strategy configured, returning unprioritized URLs");
                return urls.GetRange(0, Math.Min(urls.Count, maxUrls));
            }
            
            return _crawlStrategy.PrioritizeUrls(urls, maxUrls);
        }
        
        /// <summary>
        /// Gets the current state of the scraper
        /// </summary>
        public async Task<ScraperState> GetStateAsync()
        {
            var state = new ScraperState
            {
                ConfiguredDomain = _config.DomainName,
                EnabledFeatures = new Dictionary<string, bool>
                {
                    { "PriorityCrawling", _config.EnablePriorityCrawling },
                    { "HierarchicalExtraction", _config.EnableHierarchicalExtraction },
                    { "DocumentProcessing", _config.EnableDocumentProcessing },
                    { "ComplianceChangeDetection", _config.EnableComplianceChangeDetection },
                    { "DomainClassification", _config.EnableDomainClassification },
                    { "DynamicContentRendering", _config.EnableDynamicContentRendering },
                    { "AlertSystem", _config.EnableAlertSystem }
                }
            };
            
            if (_crawlStrategy != null)
            {
                state.CrawlStrategyMetadata = _crawlStrategy.GetPageMetadata();
            }
            
            return state;
        }

        private async Task HandlePageChangeAsync(string url, PageVersion previousVersion, PageVersion currentVersion)
        {
            var significantChanges = _changeDetector.DetectSignificantChanges(
                previousVersion.TextContent, currentVersion.TextContent);
            
            // Send alerts if needed
            if (_alertService != null && significantChanges.HasSignificantChanges)
            {
                await _alertService.ProcessAlertAsync(url, significantChanges);
            }
        }
        
        // Fix ContentNode conversion in ProcessStructuredContent method
        public async Task<List<ContentNode>> ProcessStructuredContent(string url, List<WebScraper.ContentNode> structuredContent)
        {
            try
            {
                LogInfo($"Processing structured content for {url}");
                
                // Convert WebScraper.ContentNode to RegulatoryFramework.Interfaces.ContentNode
                List<ContentNode> convertedNodes = new List<ContentNode>();
                
                foreach (var node in structuredContent)
                {
                    convertedNodes.Add(ConvertContentNode(node));
                }
                
                return convertedNodes;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing structured content for {url}");
                return new List<ContentNode>();
            }
        }

        // Helper method to convert between ContentNode types
        private ContentNode ConvertContentNode(WebScraper.ContentNode node)
        {
            if (node == null)
                return null;
                
            var convertedNode = new ContentNode
            {
                NodeType = node.NodeType,
                Content = node.Content,
                Depth = node.Depth,
                Title = node.Title,
                RelevanceScore = node.RelevanceScore
            };
            
            // Copy attributes
            foreach (var kvp in node.Attributes)
            {
                convertedNode.Attributes[kvp.Key] = kvp.Value;
            }
            
            // Convert children recursively
            foreach (var child in node.Children)
            {
                convertedNode.Children.Add(ConvertContentNode(child));
            }
            
            // Copy metadata
            foreach (var kvp in node.Metadata)
            {
                convertedNode.Metadata[kvp.Key] = kvp.Value;
            }
            
            return convertedNode;
        }

        // Helper logging methods
        private void LogInfo(string message)
        {
            _logger?.LogInformation(message);
        }

        private void LogError(Exception ex, string message)
        {
            _logger?.LogError(ex, message);
        }
        
        // Fix conversion of ClassificationResult type by adding a helper method
        private string GetPrimaryCategoryFromClassification(ClassificationResult classification)
        {
            // Use the Category property if PrimaryCategory is missing
            return classification?.PrimaryCategory ?? classification?.Category ?? "Uncategorized";
        }

        // Add this method to handle the difference between ClassificationResult implementations
        private bool HasCriticalRegulatoryImpact(ClassificationResult classification)
        {
            if (classification == null)
                return false;
                
            // Check for high or critical impact
            return classification.Impact == RegulatoryImpact.High || 
                   classification.Impact == RegulatoryImpact.Critical;
        }
    }
    
    /// <summary>
    /// State information for the scraper
    /// </summary>
    public class ScraperState
    {
        public string ConfiguredDomain { get; set; }
        public Dictionary<string, bool> EnabledFeatures { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, object> CrawlStrategyMetadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// HTTP client handler with rate limiting
    /// </summary>
    public class RateLimitedHttpClientHandler : HttpClientHandler
    {
        private readonly SemaphoreSlim _throttler;
        private readonly int _timeoutSeconds;
        
        public RateLimitedHttpClientHandler(int maxConcurrent, int timeoutSeconds)
        {
            _throttler = new SemaphoreSlim(maxConcurrent);
            _timeoutSeconds = timeoutSeconds;
        }
        
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            await _throttler.WaitAsync(cancellationToken);
            
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));
                
                return await base.SendAsync(request, cts.Token);
            }
            finally
            {
                _throttler.Release();
            }
        }
    }
}