using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Enhanced scraper for regulatory content
    /// </summary>
    public class EnhancedScraper
    {
        private readonly RegulatoryScraperConfig _config;
        private readonly ILogger<EnhancedScraper> _logger;
        private readonly ICrawlStrategy? _crawlStrategy;
        private readonly IContentExtractor? _contentExtractor;
        private readonly IDocumentProcessor? _documentProcessor;
        private readonly IChangeDetector? _changeDetector;
        private readonly IContentClassifier? _contentClassifier;
        private readonly IDynamicContentRenderer? _contentRenderer;
        private readonly IAlertService? _alertService;
        private readonly IStateStore? _stateStore;
        private readonly HttpClient _httpClient;

        public EnhancedScraper(
            RegulatoryScraperConfig config,
            ILogger<EnhancedScraper> logger,
            ICrawlStrategy? crawlStrategy = null,
            IContentExtractor? contentExtractor = null,
            IDocumentProcessor? documentProcessor = null,
            IChangeDetector? changeDetector = null,
            IContentClassifier? contentClassifier = null,
            IDynamicContentRenderer? contentRenderer = null,
            IAlertService? alertService = null,
            IStateStore? stateStore = null)
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
                List<WebScraper.ContentNode>? structuredContent = null;

                if (_config.EnableHierarchicalExtraction && _contentExtractor != null)
                {
                    var extractedContent = _contentExtractor.ExtractStructuredContent(document);
                    // Convert ContentNode from framework to WebScraper namespace
                    if (_contentExtractor is StructureAwareExtractor structureAwareExtractor)
                    {
                        structuredContent = structureAwareExtractor.ConvertToWebScraperContentNodes(extractedContent);
                    }
                    else
                    {
                        // Fallback to our local conversion method if not using StructureAwareExtractor
                        structuredContent = SafeConvertToWebScraperContentNodes(extractedContent);
                    }
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
                PageVersion? currentVersion = null;
                if (_changeDetector != null && _stateStore != null)
                {
                    // Get latest version if any
                    var previousVersion = await _stateStore.GetLatestVersionAsync(url);

                    // Track current version
                    currentVersion = await _changeDetector.TrackPageVersionAsync(url, content, textContent);

                    // Check for significant changes
                    if (currentVersion != null && (int)currentVersion.ChangeFromPrevious != (int)ChangeType.None)
                    {
                        // Handle change detection
                        await HandlePageChangeAsync(url, previousVersion, currentVersion);
                    }

                    // Save version to state store
                    if (currentVersion != null)
                    {
                        await _stateStore.SaveVersionAsync(currentVersion);
                    }
                }

                // Content classification
                if (_contentClassifier != null && _stateStore != null)
                {
                    var classification = _contentClassifier.ClassifyContent(url, textContent, document);
                    // Fix the type mismatch by using a string format directly rather than calling a helper method that expects another type
                    _logger.LogInformation("Classified {Url} as {Category}", url, classification?.Category ?? "Uncategorized");

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
        /// Handles page changes by detecting significant changes and sending alerts
        /// </summary>
        private async Task HandlePageChangeAsync(string url, PageVersion? previousVersion, PageVersion currentVersion)
        {
            if (_changeDetector == null || previousVersion == null)
                return;

            try
            {
                var significantChanges = _changeDetector.DetectSignificantChanges(
                    previousVersion.TextContent, currentVersion.TextContent);

                // Send alerts if needed
                if (_alertService != null && significantChanges.HasSignificantChanges)
                {
                    await _alertService.ProcessAlertAsync(url, significantChanges);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling page change for {Url}", url);
            }
        }

        // Helper methods for ContentNode conversion between different namespaces
        private List<WebScraper.ContentNode> SafeConvertToWebScraperContentNodes(List<WebScraper.RegulatoryFramework.Interfaces.ContentNode> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return new List<WebScraper.ContentNode>();

            return nodes.Select(n => ConvertToWebScraperContentNode(n)).ToList();
        }

        private WebScraper.ContentNode ConvertToWebScraperContentNode(WebScraper.RegulatoryFramework.Interfaces.ContentNode? node)
        {
            if (node == null)
                return new WebScraper.ContentNode
                {
                    NodeType = "Unknown",
                    Content = string.Empty,
                    Depth = 0,
                    Title = string.Empty
                };

            var convertedNode = new WebScraper.ContentNode
            {
                NodeType = node.Type ?? "Unknown",
                Content = node.Content ?? string.Empty,
                Depth = node.Level,
                Title = node.Title ?? string.Empty,
                RelevanceScore = node.RelevanceScore
            };

            // Copy attributes
            if (node.Attributes != null)
            {
                foreach (var kvp in node.Attributes)
                {
                    convertedNode.Attributes[kvp.Key] = kvp.Value;
                }
            }

            // Convert children recursively
            if (node.Children != null && node.Children.Count > 0)
            {
                convertedNode.Children = new List<WebScraper.ContentNode>();
                foreach (var child in node.Children)
                {
                    // Cast the child to WebScraper.RegulatoryFramework.Interfaces.ContentNode since that's what the method expects
                    var convertedChild = ConvertToWebScraperContentNode((WebScraper.RegulatoryFramework.Interfaces.ContentNode)child);
                    if (convertedChild != null)
                    {
                        convertedNode.Children.Add(convertedChild);
                    }
                }
            }

            // Copy metadata
            if (node.Metadata != null)
            {
                foreach (var kvp in node.Metadata)
                {
                    convertedNode.Metadata[kvp.Key] = kvp.Value;
                }
            }

            return convertedNode;
        }

        private ContentNode ConvertToFrameworkContentNode(WebScraper.ContentNode? node)
        {
            if (node == null)
                return new ContentNode
                {
                    Type = "Unknown",
                    Content = string.Empty,
                    Level = 0,
                    Title = string.Empty
                };

            var convertedNode = new ContentNode
            {
                Type = node.NodeType ?? "Unknown",
                Content = node.Content ?? string.Empty,
                Level = node.Depth,
                Title = node.Title ?? string.Empty,
                RelevanceScore = node.RelevanceScore
            };

            // Copy attributes
            if (node.Attributes != null)
            {
                foreach (var kvp in node.Attributes)
                {
                    convertedNode.Attributes[kvp.Key] = kvp.Value;
                }
            }

            // Convert children recursively
            if (node.Children != null)
            {
                foreach (var child in node.Children)
                {
                    // Use a local variable to hold the result of the conversion
                    var convertedChild = ConvertToFrameworkContentNode(child);
                    convertedNode.Children.Add(convertedChild);
                }
            }

            // Copy metadata
            if (node.Metadata != null)
            {
                foreach (var kvp in node.Metadata)
                {
                    convertedNode.Metadata[kvp.Key] = kvp.Value;
                }
            }

            return convertedNode;
        }

        // Helper method to get primary category from classification
        private string GetPrimaryCategoryFromClassification(ClassificationResult classification)
        {
            // Use the Category property if PrimaryCategory is missing
            return classification?.Category ?? "Uncategorized";
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

        // For backward compatibility - redirects to async version
        public Task ProcessUrl(string url)
        {
            return ProcessUrlAsync(url);
        }

        // For backward compatibility with code that needs to get state
        public async Task<ScraperState> GetStateAsync()
        {
            var state = new ScraperState
            {
                ConfiguredDomain = _config.DomainName,
                EnabledFeatures = new Dictionary<string, bool>
                {
                    { "DynamicContentRendering", _config.EnableDynamicContentRendering },
                    { "DocumentProcessing", _config.EnableDocumentProcessing },
                    { "HierarchicalExtraction", _config.EnableHierarchicalExtraction }
                }
            };

            // Add crawl strategy metadata if available
            if (_crawlStrategy != null)
            {
                state.CrawlStrategyMetadata = _crawlStrategy.GetPageMetadata();
            }

            // Add an await operation to properly use the async keyword
            await Task.Delay(1); // Minimal delay to make the async keyword meaningful

            return state;
        }
    }

    /// <summary>
    /// State information for the scraper
    /// </summary>
    public class ScraperState
    {
        /// <summary>
        /// Gets or sets the configured domain
        /// </summary>
        public string ConfiguredDomain { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the enabled features
        /// </summary>
        public Dictionary<string, bool> EnabledFeatures { get; set; } = new Dictionary<string, bool>();

        /// <summary>
        /// Gets or sets the crawl strategy metadata
        /// </summary>
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