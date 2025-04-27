// filepath: c:\dev\WebScraping\WebScraper\HeadlessBrowser\HeadlessBrowserHandler.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace WebScraper.HeadlessBrowser
{
    /// <summary>
    /// Handles headless browser operations for dynamic content using Playwright
    /// </summary>
    public class HeadlessBrowserHandler : IDisposable
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private readonly Dictionary<string, BrowserContextData> _contexts = new Dictionary<string, BrowserContextData>();
        private readonly Action<string> _logger;
        private readonly string _screenshotDirectory;
        private readonly BrowserType _browserType;
        private readonly HeadlessBrowserOptions _options;
        private int _contextCounter = 0;
        private bool _initialized = false;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        /// <summary>
        /// Initializes a new instance of the HeadlessBrowserHandler class
        /// </summary>
        /// <param name="options">Configuration options for the headless browser</param>
        /// <param name="logger">Logger callback function</param>
        public HeadlessBrowserHandler(HeadlessBrowserOptions options = null, Action<string> logger = null)
        {
            _options = options ?? new HeadlessBrowserOptions();
            _logger = logger ?? Console.WriteLine;
            _screenshotDirectory = _options.ScreenshotDirectory ?? Path.Combine(Directory.GetCurrentDirectory(), "screenshots");
            _browserType = _options.BrowserType;

            // Create screenshot directory if it doesn't exist
            if (!Directory.Exists(_screenshotDirectory))
            {
                Directory.CreateDirectory(_screenshotDirectory);
            }
        }

        /// <summary>
        /// Initializes the headless browser
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task InitializeAsync()
        {
            if (_initialized) return;

            try
            {
                await _semaphore.WaitAsync();

                if (_initialized) return;

                _logger("Initializing headless browser...");
                _playwright = await Playwright.CreateAsync();

                var browserOptions = new BrowserTypeLaunchOptions
                {
                    Headless = _options.Headless,
                    SlowMo = _options.SlowMotion,
                    Timeout = _options.LaunchTimeout
                };

                // Launch the browser based on the specified type
                _browser = _browserType switch
                {
                    BrowserType.Chromium => await _playwright.Chromium.LaunchAsync(browserOptions),
                    BrowserType.Firefox => await _playwright.Firefox.LaunchAsync(browserOptions),
                    BrowserType.Webkit => await _playwright.Webkit.LaunchAsync(browserOptions),
                    _ => await _playwright.Chromium.LaunchAsync(browserOptions)
                };

                _logger($"Headless browser initialized ({_browserType})");
                _initialized = true;
            }
            catch (Exception ex)
            {
                _logger($"Error initializing headless browser: {ex.Message}");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Creates a new browser context for isolated sessions
        /// </summary>
        /// <returns>The context ID</returns>
        public async Task<string> CreateContextAsync()
        {
            await EnsureInitializedAsync();

            var contextId = Interlocked.Increment(ref _contextCounter).ToString();
            
            var contextOptions = new BrowserNewContextOptions
            {
                UserAgent = _options.UserAgent,
                // Convert from our custom ViewportSize to Playwright's ViewportSize
                ViewportSize = _options.Viewport != null 
                    ? new Microsoft.Playwright.ViewportSize 
                    { 
                        Width = _options.Viewport.Width, 
                        Height = _options.Viewport.Height 
                    }
                    : null,
                BypassCSP = _options.BypassCSP,
                JavaScriptEnabled = _options.JavaScriptEnabled
            };
            
            var context = await _browser.NewContextAsync(contextOptions);
            
            // Add various listeners for monitoring
            context.Page += async (_, page) => 
            {
                await page.SetExtraHTTPHeadersAsync(new Dictionary<string, string>
                {
                    ["Accept-Language"] = "en-US,en;q=0.9"
                });
            };

            // Store the context
            _contexts[contextId] = new BrowserContextData
            {
                Context = context,
                Pages = new Dictionary<string, IPage>()
            };

            _logger($"Created browser context: {contextId}");
            return contextId;
        }

        /// <summary>
        /// Creates a new page in the specified context
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <returns>The page ID</returns>
        public async Task<string> CreatePageAsync(string contextId)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            var page = await contextData.Context.NewPageAsync();
            var pageId = Guid.NewGuid().ToString();
            
            // Configure page behaviors
            await page.SetViewportSizeAsync(_options.Viewport?.Width ?? 1280, _options.Viewport?.Height ?? 800);
            
            // Set up event handlers
            page.Console += (_, msg) => _logger($"[Console] [{pageId}] {msg.Type}: {msg.Text}");
            
            // Higher severity console messages
            if (_options.LogJavaScriptErrors)
            {
                page.PageError += (_, error) => _logger($"[Error] [{pageId}] {error}");
                page.Dialog += async (_, dialog) => 
                {
                    _logger($"[Dialog] [{pageId}] {dialog.Type}: {dialog.Message}");
                    await dialog.DismissAsync();
                };
            }
            
            // Store the page
            contextData.Pages[pageId] = page;
            
            _logger($"Created page {pageId} in context {contextId}");
            return pageId;
        }

        /// <summary>
        /// Navigates to a URL and waits for it to load
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <param name="url">The URL to navigate to</param>
        /// <param name="waitUntil">When to consider navigation complete</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<BrowserNavigationResult> NavigateToUrlAsync(
            string contextId, 
            string pageId, 
            string url, 
            NavigationWaitUntil waitUntil = NavigationWaitUntil.Load)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                throw new ArgumentException($"Page not found: {pageId}");
            }

            _logger($"Navigating to {url}...");
            
            try
            {
                var waitUntilOption = waitUntil switch
                {
                    NavigationWaitUntil.Load => WaitUntilState.Load,
                    NavigationWaitUntil.DOMContentLoaded => WaitUntilState.DOMContentLoaded, // Fix: Correct case to match the API
                    NavigationWaitUntil.NetworkIdle => WaitUntilState.NetworkIdle,
                    _ => WaitUntilState.Load
                };

                var options = new PageGotoOptions
                {
                    WaitUntil = waitUntilOption,
                    Timeout = _options.NavigationTimeout
                };

                var response = await page.GotoAsync(url, options);
                
                // Take a screenshot if enabled
                if (_options.TakeScreenshots)
                {
                    var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                    var screenshotPath = Path.Combine(_screenshotDirectory, $"navigation_{timestamp}.png");
                    await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
                    _logger($"Screenshot saved to {screenshotPath}");
                }

                if (response == null)
                {
                    return new BrowserNavigationResult
                    {
                        Success = false,
                        StatusCode = 0,
                        Url = url,
                        ErrorMessage = "Navigation failed (null response)"
                    };
                }

                int statusCode = response.Status;
                bool success = response.Ok;

                _logger($"Navigation to {url} completed with status code {statusCode}");
                
                return new BrowserNavigationResult
                {
                    Success = success,
                    StatusCode = statusCode,
                    Url = page.Url,
                    ErrorMessage = success ? null : $"HTTP error: {statusCode}"
                };
            }
            catch (PlaywrightException ex)
            {
                _logger($"Navigation error: {ex.Message}");
                
                // Try to take a screenshot of the error state
                if (_options.TakeScreenshots)
                {
                    try
                    {
                        var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                        var screenshotPath = Path.Combine(_screenshotDirectory, $"error_{timestamp}.png");
                        await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
                        _logger($"Error screenshot saved to {screenshotPath}");
                    }
                    catch (Exception screenshotEx)
                    {
                        _logger($"Failed to take error screenshot: {screenshotEx.Message}");
                    }
                }
                
                return new BrowserNavigationResult
                {
                    Success = false,
                    StatusCode = 0,
                    Url = url,
                    ErrorMessage = ex.Message
                };
            }
        }

        /// <summary>
        /// Extracts content from a page after navigation
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <returns>The extracted content</returns>
        public async Task<BrowserPageContent> ExtractContentAsync(string contextId, string pageId)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                throw new ArgumentException($"Page not found: {pageId}");
            }

            _logger($"Extracting content from page {pageId}...");
            
            try
            {
                // Wait for content to be fully rendered and stabilized
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                
                // Extract the page title
                string title = await page.TitleAsync();
                
                // Extract the page URL (which might have changed due to redirects)
                string url = page.Url;
                
                // Extract the HTML content
                string htmlContent = await page.ContentAsync();
                
                // Extract the visible text content
                string textContent = await page.EvaluateAsync<string>("document.body.innerText");
                
                // Extract all links
                var links = await page.EvaluateAsync<List<LinkInfo>>(@"
                    Array.from(document.querySelectorAll('a[href]')).map(a => {
                        const rect = a.getBoundingClientRect();
                        return {
                            href: a.href,
                            text: a.innerText,
                            isVisible: rect.width > 0 && rect.height > 0
                        };
                    });
                ");
                
                // Extract metadata (Open Graph, Twitter cards, etc.)
                var metadata = await page.EvaluateAsync<Dictionary<string, string>>(@"
                    Array.from(document.querySelectorAll('meta')).reduce((acc, meta) => {
                        const name = meta.getAttribute('name') || meta.getAttribute('property') || meta.getAttribute('itemprop');
                        const content = meta.getAttribute('content');
                        if (name && content) {
                            acc[name] = content;
                        }
                        return acc;
                    }, {});
                ");
                
                // Additional dynamic content checks for client-side rendering
                bool containsDynamicFrameworks = await page.EvaluateAsync<bool>(@"
                    !!window.React || !!window.Vue || !!window.angular || !!window.$ || !!window.jQuery
                ");
                
                _logger($"Content extraction successful: {title} ({textContent.Length} chars)");
                
                return new BrowserPageContent
                {
                    Title = title,
                    Url = url,
                    HtmlContent = htmlContent,
                    TextContent = textContent,
                    Links = links,
                    Metadata = metadata,
                    HasDynamicFrameworks = containsDynamicFrameworks,
                    ExtractionTime = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger($"Content extraction error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Executes JavaScript on a page
        /// </summary>
        /// <typeparam name="T">The return type of the script</typeparam>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <param name="script">The script to execute</param>
        /// <param name="args">Arguments to pass to the script</param>
        /// <returns>The result of the script execution</returns>
        public async Task<T> ExecuteJavaScriptAsync<T>(
            string contextId, 
            string pageId, 
            string script, 
            object[] args = null)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                throw new ArgumentException($"Page not found: {pageId}");
            }

            _logger($"Executing JavaScript on page {pageId}...");
            
            try
            {
                T result = await page.EvaluateAsync<T>(script, args ?? Array.Empty<object>());
                _logger($"JavaScript execution completed successfully");
                return result;
            }
            catch (Exception ex)
            {
                _logger($"JavaScript execution error: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Takes a screenshot of the current page
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <param name="filename">Optional filename override</param>
        /// <param name="fullPage">Whether to capture the full page</param>
        /// <returns>Path to the saved screenshot</returns>
        public async Task<string> TakeScreenshotAsync(
            string contextId, 
            string pageId, 
            string filename = null, 
            bool fullPage = true)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                throw new ArgumentException($"Page not found: {pageId}");
            }

            try
            {
                filename ??= $"screenshot_{DateTime.Now:yyyyMMddHHmmss}.png";
                string screenshotPath = Path.Combine(_screenshotDirectory, filename);
                
                await page.ScreenshotAsync(new PageScreenshotOptions
                {
                    Path = screenshotPath,
                    FullPage = fullPage
                });
                
                _logger($"Screenshot saved to {screenshotPath}");
                return screenshotPath;
            }
            catch (Exception ex)
            {
                _logger($"Error taking screenshot: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Waits for a specific selector to appear on the page
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <param name="selector">The CSS selector to wait for</param>
        /// <param name="timeoutMs">The timeout in milliseconds</param>
        /// <returns>True if the selector appeared, false if it timed out</returns>
        public async Task<bool> WaitForSelectorAsync(
            string contextId,
            string pageId,
            string selector,
            int? timeoutMs = null)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                throw new ArgumentException($"Context not found: {contextId}");
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                throw new ArgumentException($"Page not found: {pageId}");
            }

            _logger($"Waiting for selector '{selector}' on page {pageId}...");
            
            try
            {
                var options = new PageWaitForSelectorOptions
                {
                    Timeout = timeoutMs ?? _options.WaitTimeout
                };
                
                await page.WaitForSelectorAsync(selector, options);
                _logger($"Found selector '{selector}'");
                return true;
            }
            catch (TimeoutException)
            {
                _logger($"Timed out waiting for selector '{selector}'");
                return false;
            }
            catch (Exception ex)
            {
                _logger($"Error waiting for selector '{selector}': {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Closes a specific browser page
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <param name="pageId">The page ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task ClosePageAsync(string contextId, string pageId)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                return;
            }

            if (!contextData.Pages.TryGetValue(pageId, out var page))
            {
                return;
            }

            try
            {
                await page.CloseAsync();
                contextData.Pages.Remove(pageId);
                _logger($"Closed page {pageId}");
            }
            catch (Exception ex)
            {
                _logger($"Error closing page {pageId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Closes a browser context and all its pages
        /// </summary>
        /// <param name="contextId">The context ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task CloseContextAsync(string contextId)
        {
            if (!_contexts.TryGetValue(contextId, out var contextData))
            {
                return;
            }

            try
            {
                await contextData.Context.CloseAsync();
                _contexts.Remove(contextId);
                _logger($"Closed context {contextId}");
            }
            catch (Exception ex)
            {
                _logger($"Error closing context {contextId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Ensures the browser is initialized
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }
        }

        /// <summary>
        /// Disposes of resources used by the headless browser
        /// </summary>
        public async void Dispose()
        {
            if (_browser != null)
            {
                foreach (var contextId in new List<string>(_contexts.Keys))
                {
                    await CloseContextAsync(contextId);
                }
                
                await _browser.CloseAsync();
                _browser = null;
            }
            
            if (_playwright != null)
            {
                _playwright.Dispose();
                _playwright = null;
            }
            
            _semaphore.Dispose();
            _initialized = false;
            
            _logger("Headless browser disposed");
        }

        /// <summary>
        /// Browser context data for managing multiple contexts
        /// </summary>
        private class BrowserContextData
        {
            public IBrowserContext Context { get; set; }
            public Dictionary<string, IPage> Pages { get; set; }
        }
    }

    /// <summary>
    /// Options for configuring the headless browser
    /// </summary>
    public class HeadlessBrowserOptions
    {
        /// <summary>
        /// The type of browser to use
        /// </summary>
        public BrowserType BrowserType { get; set; } = BrowserType.Chromium;
        
        /// <summary>
        /// Whether to run the browser in headless mode
        /// </summary>
        public bool Headless { get; set; } = true;
        
        /// <summary>
        /// Directory for saving screenshots
        /// </summary>
        public string ScreenshotDirectory { get; set; }
        
        /// <summary>
        /// Whether to take screenshots during navigation and errors
        /// </summary>
        public bool TakeScreenshots { get; set; } = false;
        
        /// <summary>
        /// Whether JavaScript is enabled
        /// </summary>
        public bool JavaScriptEnabled { get; set; } = true;
        
        /// <summary>
        /// User agent string
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36";
        
        /// <summary>
        /// Whether to bypass Content Security Policy
        /// </summary>
        public bool BypassCSP { get; set; } = false;
        
        /// <summary>
        /// Viewport size
        /// </summary>
        public ViewportSize Viewport { get; set; } = new ViewportSize { Width = 1280, Height = 800 };
        
        /// <summary>
        /// Slow down browser operations by the specified number of milliseconds
        /// </summary>
        public int SlowMotion { get; set; } = 0;
        
        /// <summary>
        /// Timeout for browser launch in milliseconds
        /// </summary>
        public int LaunchTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Timeout for navigation in milliseconds
        /// </summary>
        public int NavigationTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Timeout for wait operations in milliseconds
        /// </summary>
        public int WaitTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Whether to log JavaScript errors
        /// </summary>
        public bool LogJavaScriptErrors { get; set; } = true;
    }

    /// <summary>
    /// Result of a browser navigation operation
    /// </summary>
    public class BrowserNavigationResult
    {
        /// <summary>
        /// Whether the navigation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Final URL (may be different from the requested URL due to redirects)
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Error message if navigation failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// Content extracted from a browser page
    /// </summary>
    public class BrowserPageContent
    {
        /// <summary>
        /// Page title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Page URL
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Full HTML content
        /// </summary>
        public string HtmlContent { get; set; }
        
        /// <summary>
        /// Visible text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Links found on the page
        /// </summary>
        public List<LinkInfo> Links { get; set; }
        
        /// <summary>
        /// Metadata extracted from meta tags
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; }
        
        /// <summary>
        /// Whether the page uses dynamic frameworks like React, Vue, Angular, etc.
        /// </summary>
        public bool HasDynamicFrameworks { get; set; }
        
        /// <summary>
        /// When the content was extracted
        /// </summary>
        public DateTime ExtractionTime { get; set; }
    }

    /// <summary>
    /// Information about a link on a page
    /// </summary>
    public class LinkInfo
    {
        /// <summary>
        /// The href attribute of the link
        /// </summary>
        public string Href { get; set; }
        
        /// <summary>
        /// The text content of the link
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Whether the link is visible on the page
        /// </summary>
        public bool IsVisible { get; set; }
    }

    /// <summary>
    /// Viewport size for browser pages
    /// </summary>
    public class ViewportSize
    {
        /// <summary>
        /// Width in pixels
        /// </summary>
        public int Width { get; set; }
        
        /// <summary>
        /// Height in pixels
        /// </summary>
        public int Height { get; set; }
    }

    /// <summary>
    /// When to consider navigation complete
    /// </summary>
    public enum NavigationWaitUntil
    {
        /// <summary>
        /// Consider navigation complete when the load event is fired
        /// </summary>
        Load,
        
        /// <summary>
        /// Consider navigation complete when the DOMContentLoaded event is fired
        /// </summary>
        DOMContentLoaded,
        
        /// <summary>
        /// Consider navigation complete when the network is idle
        /// </summary>
        NetworkIdle
    }

    /// <summary>
    /// Type of browser to use
    /// </summary>
    public enum BrowserType
    {
        /// <summary>
        /// Chromium-based browser (Chrome, Edge, etc.)
        /// </summary>
        Chromium,
        
        /// <summary>
        /// Firefox browser
        /// </summary>
        Firefox,
        
        /// <summary>
        /// WebKit-based browser (Safari)
        /// </summary>
        Webkit
    }
}