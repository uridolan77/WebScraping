using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.Processing;
using WebScraper.RateLimiting;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that processes URLs using HTTP requests
    /// </summary>
    public class HttpUrlProcessor : ScraperComponentBase, IUrlProcessor, IDisposable
    {
        private HttpClient? _httpClient;
        private readonly ConcurrentBag<string> _pendingUrls = new ConcurrentBag<string>();
        private readonly HashSet<string> _visitedUrls = new HashSet<string>();
        private SemaphoreSlim _processingLimiter;
        private int _maxConcurrentRequests = 5;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the HttpUrlProcessor class
        /// </summary>
        public HttpUrlProcessor()
        {
            // Initialize with 1, will be properly set in InitializeAsync
            _processingLimiter = new SemaphoreSlim(1);
        }

        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);

            // Initialize HTTP client
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                AutomaticDecompression = System.Net.DecompressionMethods.All
            };

            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(Config.RequestTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", Config.UserAgent);

            _maxConcurrentRequests = Config.MaxConcurrentRequests;

            // Dispose old semaphore and create a new one with correct count
            _processingLimiter.Dispose();
            _processingLimiter = new SemaphoreSlim(_maxConcurrentRequests);

            LogInfo("HttpUrlProcessor initialized");
        }

        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public override Task OnScrapingStartedAsync()
        {
            _visitedUrls.Clear();
            _pendingUrls.Clear();
            LogInfo("HttpUrlProcessor ready to process URLs");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Processes a URL
        /// </summary>
        public async Task ProcessUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return;

            // Normalize the URL
            url = NormalizeUrl(url);

            // Check if the URL has already been visited
            lock (_visitedUrls)
            {
                if (_visitedUrls.Contains(url))
                    return;

                _visitedUrls.Add(url);
            }

            // Check with the state manager if this URL has been visited
            var stateManager = GetComponent<IStateManager>();
            if (stateManager != null)
            {
                bool alreadyVisited = await stateManager.HasUrlBeenVisitedAsync(url);
                if (alreadyVisited)
                {
                    LogInfo($"Skipping already visited URL: {url}");
                    return;
                }
            }

            try
            {
                // Check if we should use browser rendering
                var browserHandler = GetComponent<IBrowserHandler>();
                if (browserHandler != null && ShouldUseHeadlessBrowser(url))
                {
                    await ProcessWithBrowserAsync(url, browserHandler);
                }
                else
                {
                    await ProcessWithHttpClientAsync(url);
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing URL: {url}");

                // Mark URL as visited with error
                if (stateManager != null)
                {
                    await stateManager.MarkUrlVisitedAsync(url, 500); // HTTP 500 to indicate error
                }
            }
        }

        /// <summary>
        /// Process a batch of URLs using Parallel.ForEachAsync for better concurrency control
        /// </summary>
        public async Task ProcessUrlBatchAsync(IEnumerable<string> urls)
        {
            // Get cancellation token from Core if available
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                var coreType = Core.GetType();
                var tokenProperty = coreType.GetProperty("CancellationToken", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (tokenProperty != null)
                {
                    var token = tokenProperty.GetValue(Core) as CancellationToken?;
                    if (token.HasValue)
                    {
                        cancellationToken = token.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Could not get cancellation token from Core: {ex.Message}");
            }

            // Use Parallel.ForEachAsync for better control over parallelism
            await Parallel.ForEachAsync(
                urls,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _maxConcurrentRequests,
                    CancellationToken = cancellationToken
                },
                async (url, token) =>
                {
                    try
                    {
                        await ProcessUrlAsync(url);
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Error in parallel processing of URL: {url}");
                    }
                });
        }

        /// <summary>
        /// Process a URL using the headless browser
        /// </summary>
        private async Task ProcessWithBrowserAsync(string url, IBrowserHandler browserHandler)
        {
            LogInfo($"Processing with browser: {url}");

            var result = await browserHandler.NavigateToUrlAsync(url);
            if (!result.Success)
            {
                LogWarning($"Browser navigation failed: {url} - {result.ErrorMessage}");
                return;
            }

            // Process content
            await ProcessContentAsync(url, result.HtmlContent, result.TextContent, "text/html");

            // Process extracted links
            if (result.Links?.Any() == true)
            {
                var validLinks = new List<string>();
                foreach (var link in result.Links.Where(l => l.IsVisible && !string.IsNullOrEmpty(l.Href)))
                {
                    // Resolve relative URLs
                    string absoluteUrl = new Uri(new Uri(url), link.Href).ToString();

                    // Only add if we should crawl this URL
                    if (ShouldCrawlUrl(absoluteUrl))
                    {
                        validLinks.Add(absoluteUrl);
                        _pendingUrls.Add(absoluteUrl);
                    }
                }

                // Process a batch of URLs
                if (validLinks.Any())
                {
                    await ProcessUrlBatchAsync(validLinks);
                }
            }
        }

        /// <summary>
        /// Process a URL using the HTTP client
        /// </summary>
        private async Task ProcessWithHttpClientAsync(string url)
        {
            LogInfo($"Processing with HTTP client: {url}");

            // Ensure HTTP client is initialized
            if (_httpClient == null)
            {
                LogError(new InvalidOperationException("HTTP client not initialized"), $"Failed to process {url}");
                return;
            }

            try
            {
                // Get content
                var response = await _httpClient.GetAsync(url);

                // Track the status code without throwing exceptions for non-2xx status codes
                int statusCode = (int)response.StatusCode;

                // Update state manager with status code
                var stateManager = GetComponent<IStateManager>();
                if (stateManager != null)
                {
                    await stateManager.MarkUrlVisitedAsync(url, statusCode);
                }

                // Handle non-success status codes
                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = $"URL returned status code {statusCode}: {response.ReasonPhrase}";
                    LogWarning($"{errorMessage} - {url}");

                    // Add error to core's error collection so it can be displayed in UI
                    Core.AddError(url, errorMessage);

                    // Don't proceed with content processing for non-success responses
                    return;
                }

                var content = await response.Content.ReadAsStringAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "text/html";

                // If it's a document, handle it separately
                if (IsDocumentType(contentType))
                {
                    await ProcessDocumentAsync(url, await response.Content.ReadAsByteArrayAsync(), contentType);
                    return;
                }

                // Extract text content
                string textContent = "";
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);

                var contentExtractor = GetComponent<IContentExtractor>();
                if (contentExtractor != null)
                {
                    textContent = await contentExtractor.ExtractTextContentAsync(content);
                }
                else
                {
                    // Simple extraction if no extractor is available
                    textContent = htmlDoc.DocumentNode.InnerText;
                }

                // Process content
                await ProcessContentAsync(url, content, textContent, contentType);

                // Extract and process links
                var links = ExtractLinks(htmlDoc, url);
                if (links.Any())
                {
                    await ProcessUrlBatchAsync(links);
                }
            }
            catch (Exception ex)
            {
                // Log the error
                LogError(ex, $"Exception processing URL: {url}");

                // Add error to core's error collection so it can be displayed in UI
                Core.AddError(url, ex.Message);

                // Re-throw to propagate through standard error handling
                throw;
            }
        }

        /// <summary>
        /// Process content from a URL
        /// </summary>
        private async Task ProcessContentAsync(string url, string content, string textContent, string contentType)
        {
            // Log detailed information about the page being processed
            LogInfo($"Processing content from URL: {url} (Content type: {contentType}, Content length: {content?.Length ?? 0} bytes)");

            // Extract title from HTML if possible
            string title = "Unknown";
            try {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);
                var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null) {
                    title = titleNode.InnerText.Trim();
                    LogInfo($"Page title: {title}");
                }
            } catch (Exception ex) {
                LogWarning($"Could not extract title: {ex.Message}");
            }

            // Update scraper metrics
            Core.AddError(url, $"Processed page: {title}"); // Use AddError as a way to track processed pages

            // Track metrics in a way that's compatible with the current implementation
            try {
                var metricsField = Core.GetType().GetField("_metrics", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (metricsField != null) {
                    var metrics = metricsField.GetValue(Core) as Dictionary<string, object>;
                    if (metrics != null) {
                        // Update metrics
                        if (metrics.ContainsKey("PagesProcessed") && metrics["PagesProcessed"] is int count) {
                            metrics["PagesProcessed"] = count + 1;
                        } else {
                            metrics["PagesProcessed"] = 1;
                        }
                        metrics["LastProcessedUrl"] = url;
                        metrics["LastProcessedTitle"] = title;
                        metrics["LastProcessedTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                }
            } catch (Exception ex) {
                LogWarning($"Could not update metrics: {ex.Message}");
            }

            // Save content to state manager
            var stateManager = GetComponent<IStateManager>();
            if (stateManager != null)
            {
                await stateManager.SaveContentAsync(url, content ?? string.Empty, contentType);
                await stateManager.MarkUrlVisitedAsync(url, 200); // HTTP 200 OK

                // Update state with more detailed information
                try {
                    // Use SaveStateAsync with additional information
                    await stateManager.SaveStateAsync("Running");

                    // Log the page processing for monitoring
                    LogInfo($"Page processed: {url} - {title}");
                } catch (Exception ex) {
                    LogWarning($"Could not update detailed state: {ex.Message}");
                }
            }

            // Save content to both files and database using ContentSaverAdapter
            try {
                // First, try to find ContentSaverAdapter specifically
                var contentSaver = GetAllComponents().FirstOrDefault(c => c.GetType().Name == "ContentSaverAdapter");
                if (contentSaver != null)
                {
                    try
                    {
                        var saveMethod = contentSaver.GetType().GetMethod("SaveContentAsync");
                        if (saveMethod != null)
                        {
                            LogInfo($"Found ContentSaverAdapter, calling SaveContentAsync for URL: {url}");
                            var task = saveMethod.Invoke(contentSaver, new object[] { url, content ?? string.Empty, textContent ?? string.Empty });
                            if (task is Task taskObj)
                            {
                                await taskObj;
                                LogInfo($"Successfully saved content using ContentSaverAdapter for URL: {url}");
                            }
                        }
                        else
                        {
                            LogWarning($"ContentSaverAdapter found but SaveContentAsync method not found");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Error calling ContentSaverAdapter.SaveContentAsync for URL: {url}");
                    }
                }
                else
                {
                    // Fallback to looking for any component that has a SaveContentAsync method
                    LogWarning("ContentSaverAdapter not found, looking for any component with SaveContentAsync method");
                    var components = GetAllComponents();
                    bool found = false;
                    foreach (var component in components)
                    {
                        try
                        {
                            var saveMethod = component.GetType().GetMethod("SaveContentAsync");
                            if (saveMethod != null && saveMethod.GetParameters().Length == 3)
                            {
                                LogInfo($"Found component {component.GetType().Name} with SaveContentAsync method");
                                var task = saveMethod.Invoke(component, new object[] { url, content ?? string.Empty, textContent ?? string.Empty });
                                if (task is Task taskObj)
                                {
                                    await taskObj;
                                    LogInfo($"Content saved using {component.GetType().Name}");
                                    found = true;
                                    break; // Found a component that can save content
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogWarning($"Error calling SaveContentAsync on {component.GetType().Name}: {ex.Message}");
                        }
                    }

                    if (!found)
                    {
                        LogWarning($"No component found that can save content for URL: {url}");
                    }
                }
            } catch (Exception ex) {
                LogError(ex, $"Could not save content for URL: {url}");
            }

            // Track content changes if change detector is available
            var changeDetector = GetComponent<IChangeDetector>();
            if (changeDetector != null)
            {
                await changeDetector.TrackPageVersionAsync(url, content ?? string.Empty, contentType);
            }

            // Classify content if machine learning classifier is available
            try
            {
                // Use reflection to find the MachineLearningContentClassifier
                var components = GetAllComponents();
                var serviceProvider = Core.GetType().GetProperty("ServiceProvider")?.GetValue(Core);

                if (serviceProvider != null)
                {
                    // Try to get the classifier from the service provider
                    try
                    {
                        var getServiceMethod = serviceProvider.GetType().GetMethod("GetService");
                        if (getServiceMethod != null)
                        {
                            var classifierType = Type.GetType("WebScraper.Processing.MachineLearningContentClassifier, WebScraper");
                            if (classifierType != null)
                            {
                                var classifier = getServiceMethod.MakeGenericMethod(classifierType).Invoke(serviceProvider, null);

                                if (classifier != null)
                                {
                                    LogInfo($"Classifying content for URL: {url}");
                                    var classifyMethod = classifierType.GetMethod("ClassifyContentAsync");

                                    if (classifyMethod != null)
                                    {
                                        var task = classifyMethod.Invoke(classifier, new object[] { textContent ?? string.Empty });
                                        if (task is Task<object> classificationTask)
                                        {
                                            var classification = await classificationTask;

                                            if (classification != null)
                                            {
                                                // Get document type and confidence using reflection
                                                var documentType = classification.GetType().GetProperty("DocumentType")?.GetValue(classification) as string;
                                                var confidence = classification.GetType().GetProperty("Confidence")?.GetValue(classification);

                                                LogInfo($"Content classified as {documentType} with {confidence} confidence");

                                                // Try to save classification to database using ContentClassificationService
                                                try
                                                {
                                                    // Use reflection to find and call ContentClassificationService
                                                    foreach (var component in components)
                                                    {
                                                        if (component.GetType().Name.Contains("ContentClassificationService"))
                                                        {
                                                            var method = component.GetType().GetMethod("ClassifyContentAsync");
                                                            if (method != null)
                                                            {
                                                                LogInfo($"Found ContentClassificationService, saving classification for URL: {url}");
                                                                var scraperId = Core.GetType().GetProperty("Id")?.GetValue(Core) as string ?? "unknown";
                                                                var serviceTask = method.Invoke(component, new object[] { scraperId, url, textContent ?? string.Empty });
                                                                if (serviceTask is Task taskObj)
                                                                {
                                                                    await taskObj;
                                                                    LogInfo($"Successfully saved classification for URL: {url}");
                                                                }
                                                            }
                                                            break;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    LogWarning($"Error saving classification: {ex.Message}");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogWarning($"Error getting classifier from service provider: {ex.Message}");
                    }
                }
                else
                {
                    // Fallback: Try to find the classifier in the components
                    foreach (var component in components)
                    {
                        if (component.GetType().Name.Contains("ContentClassificationService"))
                        {
                            try
                            {
                                LogInfo($"Found ContentClassificationService, classifying content for URL: {url}");
                                var method = component.GetType().GetMethod("ClassifyContentAsync");
                                if (method != null)
                                {
                                    var scraperId = Core.GetType().GetProperty("Id")?.GetValue(Core) as string ?? "unknown";
                                    var task = method.Invoke(component, new object[] { scraperId, url, textContent ?? string.Empty });
                                    if (task is Task taskObj)
                                    {
                                        await taskObj;
                                        LogInfo($"Successfully classified and saved content for URL: {url}");
                                    }
                                }
                                break;
                            }
                            catch (Exception ex)
                            {
                                LogWarning($"Error classifying content with service: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Error classifying content: {ex.Message}");
            }

            // Log the page processing for monitoring purposes
            try {
                // Create a log entry that can be picked up by monitoring systems
                LogInfo($"PAGE_PROCESSED|{url}|{title}|{content?.Length ?? 0}|{textContent?.Length ?? 0}|{DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                // Try to access components field via reflection
                try {
                    var componentsField = Core.GetType().GetField("_components", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (componentsField != null) {
                        var components = componentsField.GetValue(Core) as IEnumerable<IScraperComponent>;
                        if (components != null) {
                            foreach (var component in components) {
                                if (component.GetType().Name.Contains("Monitor")) {
                                    try {
                                        // Try to call a method that might exist on monitoring components
                                        var method = component.GetType().GetMethod("ReportPageProcessed");
                                        if (method != null) {
                                            method.Invoke(component, new object[] { url, title, content?.Length ?? 0 });
                                        }
                                    } catch {
                                        // Ignore reflection errors
                                    }
                                }
                            }
                        }
                    }
                } catch (Exception ex) {
                    LogWarning($"Could not access components: {ex.Message}");
                }
            } catch (Exception ex) {
                LogWarning($"Could not report to monitoring: {ex.Message}");
            }
        }

        /// <summary>
        /// Process a document
        /// </summary>
        private async Task ProcessDocumentAsync(string url, byte[] content, string contentType)
        {
            var documentProcessor = GetComponent<IDocumentProcessor>();
            if (documentProcessor != null)
            {
                await documentProcessor.ProcessDocumentAsync(url, content, contentType);
            }
            else
            {
                LogWarning($"No document processor available for: {url} ({contentType})");
            }
        }

        /// <summary>
        /// Extracts links from HTML
        /// </summary>
        private List<string> ExtractLinks(HtmlDocument htmlDoc, string baseUrl)
        {
            var result = new List<string>();
            var baseUri = new Uri(baseUrl);

            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes == null)
                return result;

            foreach (var linkNode in linkNodes)
            {
                var href = linkNode.GetAttributeValue("href", string.Empty);

                if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#") || href.StartsWith("javascript:"))
                    continue;

                // Use a null-safe approach for URI creation
                Uri? absoluteUri = null;
                try
                {
                    absoluteUri = new Uri(baseUri, href);
                }
                catch
                {
                    // Skip invalid URIs
                    continue;
                }

                if (absoluteUri != null)
                {
                    var absoluteUrl = absoluteUri.ToString();

                    // Only add if we should crawl this URL
                    if (ShouldCrawlUrl(absoluteUrl))
                    {
                        result.Add(absoluteUrl);
                        _pendingUrls.Add(absoluteUrl);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Normalizes a URL
        /// </summary>
        private string NormalizeUrl(string url)
        {
            // Remove fragments
            int fragmentIndex = url.IndexOf('#');
            if (fragmentIndex >= 0)
            {
                url = url.Substring(0, fragmentIndex);
            }

            // Remove trailing slashes
            url = url.TrimEnd('/');

            return url;
        }

        /// <summary>
        /// Determines if a URL should be crawled
        /// </summary>
        private bool ShouldCrawlUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;

            try
            {
                var uri = new Uri(url);

                // Check if URL is in allowed domains
                bool inAllowedDomain = Config.AllowedDomains?.Any(domain =>
                    uri.Host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.EndsWith($".{domain}", StringComparison.OrdinalIgnoreCase)) ?? false;

                // If no allowed domains specified, just use the domain from the start URL
                if (Config.AllowedDomains == null || Config.AllowedDomains.Count == 0)
                {
                    var startUri = new Uri(Config.StartUrl);
                    inAllowedDomain = uri.Host.Equals(startUri.Host, StringComparison.OrdinalIgnoreCase);
                }

                // Check if URL matches any exclude patterns
                bool isExcluded = Config.ExcludeUrlPatterns?.Any(pattern =>
                    url.Contains(pattern, StringComparison.OrdinalIgnoreCase)) ?? false;

                return inAllowedDomain && !isExcluded;
            }
            catch
            {
                // If we can't parse the URL, don't crawl it
                return false;
            }
        }

        /// <summary>
        /// Determines if a URL should be processed with headless browser
        /// </summary>
        private bool ShouldUseHeadlessBrowser(string url)
        {
            // First check if headless browser processing is enabled at all
            if (!Config.ProcessJsHeavyPages)
                return false;

            // Check if it's a specific site that needs JavaScript processing
            if (Config.IsUKGCWebsite)
                return true;

            // Check if it's a document type that might need special handling
            string extension = System.IO.Path.GetExtension(url).ToLowerInvariant();
            if (Config.ProcessPdfDocuments && extension == ".pdf")
                return true;

            return false;
        }

        /// <summary>
        /// Determines if a content type is a document type
        /// </summary>
        private bool IsDocumentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;

            contentType = contentType.ToLowerInvariant();
            return contentType.Contains("pdf") ||
                   contentType.Contains("msword") ||
                   contentType.Contains("excel") ||
                   contentType.Contains("powerpoint") ||
                   contentType.Contains("openxmlformat") ||
                   contentType.Contains("opendocument");
        }

        /// <summary>
        /// Gets all components from the scraper core
        /// </summary>
        private IEnumerable<IScraperComponent> GetAllComponents()
        {
            try
            {
                var componentsField = Core.GetType().GetField("_components", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (componentsField != null)
                {
                    var components = componentsField.GetValue(Core) as IEnumerable<IScraperComponent>;
                    if (components != null)
                    {
                        return components;
                    }
                }
            }
            catch (Exception ex)
            {
                LogWarning($"Could not get components: {ex.Message}");
            }

            return Array.Empty<IScraperComponent>();
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            // Dispose HTTP client
            _httpClient?.Dispose();

            // Dispose semaphore
            _processingLimiter?.Dispose();
        }
    }
}