using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Implementation
{
    /// <summary>
    /// Renders dynamic content using Microsoft Playwright
    /// </summary>
    public class PlaywrightRenderer : IDynamicContentRenderer, IDisposable
    {
        private readonly DynamicContentConfig _config;
        private readonly ILogger<PlaywrightRenderer> _logger;
        private readonly SemaphoreSlim _browserSemaphore;
        private readonly ConcurrentDictionary<string, string> _contentCache = new ConcurrentDictionary<string, string>();
        
        private IPlaywright _playwright;
        private IBrowser _browser;
        private bool _initialized;
        private bool _disposed;
        
        public PlaywrightRenderer(DynamicContentConfig config, ILogger<PlaywrightRenderer> logger)
        {
            _config = config;
            _logger = logger;
            _browserSemaphore = new SemaphoreSlim(_config.MaxConcurrentSessions, _config.MaxConcurrentSessions);
        }
        
        /// <summary>
        /// Gets rendered HTML for a URL
        /// </summary>
        public async Task<string> GetRenderedHtmlAsync(string url)
        {
            try
            {
                // Check cache first
                if (_contentCache.TryGetValue(url, out var cachedContent))
                {
                    return cachedContent;
                }
                
                // Initialize playwright if needed
                await EnsureInitializedAsync();
                
                // Acquire browser semaphore
                await _browserSemaphore.WaitAsync();
                
                try
                {
                    // Create a new browser context for isolation
                    var context = await _browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        UserAgent = _config.BrowserUserAgent ?? _browser.BrowserType.Name == "chromium" 
                            ? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36"
                            : "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:90.0) Gecko/20100101 Firefox/90.0",
                        JavaScriptEnabled = !_config.DisableJavaScript
                    });
                    
                    // Create a new page
                    var page = await context.NewPageAsync();
                    
                    // Set up navigation timeout
                    page.SetDefaultNavigationTimeout(_config.NavigationTimeout);
                    
                    // Navigate to the URL
                    var response = await page.GotoAsync(url);
                    
                    if (!response.Ok)
                    {
                        _logger.LogWarning("Received non-OK response ({StatusCode}) for {Url}", response.Status, url);
                    }
                    
                    // Wait for selector if specified
                    if (!string.IsNullOrEmpty(_config.WaitForSelector))
                    {
                        try
                        {
                            await page.WaitForSelectorAsync(_config.WaitForSelector, 
                                new PageWaitForSelectorOptions { Timeout = _config.NavigationTimeout });
                        }
                        catch (TimeoutException)
                        {
                            _logger.LogWarning("Timeout waiting for selector {Selector} on {Url}", _config.WaitForSelector, url);
                        }
                    }
                    
                    // Click element if specified (like cookie banners)
                    if (!string.IsNullOrEmpty(_config.AutoClickSelector))
                    {
                        try
                        {
                            var clickable = await page.QuerySelectorAsync(_config.AutoClickSelector);
                            if (clickable != null)
                            {
                                await clickable.ClickAsync();
                                _logger.LogInformation("Clicked element {Selector} on {Url}", _config.AutoClickSelector, url);
                                
                                // Wait a bit after clicking
                                await Task.Delay(500);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error clicking element {Selector} on {Url}", _config.AutoClickSelector, url);
                        }
                    }
                    
                    // Wait for additional delay if specified
                    if (_config.PostNavigationDelay > 0)
                    {
                        await Task.Delay(_config.PostNavigationDelay);
                    }
                    
                    // Get the page content
                    var content = await page.ContentAsync();
                    
                    // Clean up
                    await context.CloseAsync();
                    
                    // Cache the content
                    _contentCache[url] = content;
                    
                    return content;
                }
                finally
                {
                    _browserSemaphore.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rendering content for {Url}", url);
                throw;
            }
        }
        
        /// <summary>
        /// Gets rendered HTML document for a URL
        /// </summary>
        public async Task<HtmlDocument> GetRenderedDocumentAsync(string url)
        {
            var html = await GetRenderedHtmlAsync(url);
            var document = new HtmlDocument();
            document.LoadHtml(html);
            return document;
        }
        
        /// <summary>
        /// Ensures Playwright is initialized
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (_initialized)
            {
                return;
            }
            
            try
            {
                _logger.LogInformation("Initializing Playwright with browser type: {BrowserType}", _config.BrowserType);
                
                // Install Playwright and dependencies if needed
                var exitCode = Microsoft.Playwright.Program.Main(new[] { "install", _config.BrowserType });
                if (exitCode != 0)
                {
                    throw new Exception($"Failed to install Playwright browser {_config.BrowserType}");
                }
                
                // Create Playwright instance
                _playwright = await Playwright.CreateAsync();
                
                // Launch browser
                var launchOptions = new BrowserTypeLaunchOptions
                {
                    Headless = true,
                };
                
                // Add proxy if specified
                if (!string.IsNullOrEmpty(_config.Proxy))
                {
                    launchOptions.Proxy = new ProxySettings { Server = _config.Proxy };
                }
                
                // Launch the appropriate browser type
                switch (_config.BrowserType.ToLower())
                {
                    case "chromium":
                        _browser = await _playwright.Chromium.LaunchAsync(launchOptions);
                        break;
                    case "firefox":
                        _browser = await _playwright.Firefox.LaunchAsync(launchOptions);
                        break;
                    case "webkit":
                        _browser = await _playwright.Webkit.LaunchAsync(launchOptions);
                        break;
                    default:
                        throw new ArgumentException($"Unsupported browser type: {_config.BrowserType}");
                }
                
                _initialized = true;
                _logger.LogInformation("Playwright initialized successfully with {BrowserType}", _config.BrowserType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing Playwright");
                throw;
            }
        }
        
        /// <summary>
        /// Disposes of Playwright resources
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            
            try
            {
                _browser?.CloseAsync().GetAwaiter().GetResult();
                _browser?.DisposeAsync().GetAwaiter().GetResult();
                _playwright?.Dispose();
                _browserSemaphore?.Dispose();
                
                _disposed = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing Playwright");
            }
        }
    }
}