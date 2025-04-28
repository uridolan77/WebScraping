using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
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
        /// Process a batch of URLs
        /// </summary>
        public async Task ProcessUrlBatchAsync(IEnumerable<string> urls)
        {
            var tasks = new List<Task>();
            foreach (var url in urls)
            {
                // Wait until we have a processing slot available
                await _processingLimiter.WaitAsync();
                
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        await ProcessUrlAsync(url);
                    }
                    finally
                    {
                        _processingLimiter.Release();
                    }
                }));
            }
            
            await Task.WhenAll(tasks);
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
            
            // Get content
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
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
        
        /// <summary>
        /// Process content from a URL
        /// </summary>
        private async Task ProcessContentAsync(string url, string content, string textContent, string contentType)
        {
            var stateManager = GetComponent<IStateManager>();
            if (stateManager != null)
            {
                await stateManager.SaveContentAsync(url, content, contentType);
                await stateManager.MarkUrlVisitedAsync(url, 200); // HTTP 200 OK
            }
            
            var changeDetector = GetComponent<IChangeDetector>();
            if (changeDetector != null)
            {
                await changeDetector.TrackPageVersionAsync(url, content, contentType);
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