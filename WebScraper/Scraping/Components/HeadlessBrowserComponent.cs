using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebScraper.HeadlessBrowser;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles browser automation for JavaScript-heavy pages
    /// </summary>
    public class HeadlessBrowserComponent : ScraperComponentBase, IBrowserHandler, IDisposable
    {
        private HeadlessBrowserHandler _browserHandler;
        private bool _isDisposed;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            if (Config.ProcessJsHeavyPages || Config.IsUKGCWebsite || Config.ProcessPdfDocuments)
            {
                await InitializeBrowserHandlerAsync();
            }
            else
            {
                LogInfo("Headless browser not required by configuration, skipping initialization");
            }
        }
        
        /// <summary>
        /// Initializes the browser handler
        /// </summary>
        private async Task InitializeBrowserHandlerAsync()
        {
            try
            {
                LogInfo("Initializing headless browser...");
                
                // Ensure the screenshot directory exists
                var screenshotDir = Path.Combine(Config.OutputDirectory, "screenshots");
                if (!Directory.Exists(screenshotDir))
                {
                    Directory.CreateDirectory(screenshotDir);
                }
                
                var browserOptions = new HeadlessBrowserOptions
                {
                    Headless = true,
                    ScreenshotDirectory = screenshotDir,
                    BrowserType = BrowserType.Chromium,
                    TakeScreenshots = Config.IsUKGCWebsite,
                    UserAgent = Config.UserAgent ?? "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Safari/537.36",
                    JavaScriptEnabled = true,
                    NavigationTimeout = 30000,
                    WaitTimeout = 30000
                };
                
                _browserHandler = new HeadlessBrowserHandler(browserOptions, LogInfo);
                await _browserHandler.InitializeAsync();
                
                LogInfo("Headless browser initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize headless browser");
            }
        }
        
        /// <summary>
        /// Navigates to a URL using the headless browser
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        /// <returns>The result of the navigation</returns>
        public async Task<BrowserPageResult> NavigateToUrlAsync(string url)
        {
            if (_browserHandler == null)
            {
                return new BrowserPageResult
                {
                    Success = false,
                    ErrorMessage = "Headless browser not initialized"
                };
            }
            
            string contextId = null;
            string pageId = null;
            
            try
            {
                // Create browser context and page
                contextId = await _browserHandler.CreateContextAsync();
                pageId = await _browserHandler.CreatePageAsync(contextId);
                
                // Navigate to URL
                var navResult = await _browserHandler.NavigateToUrlAsync(
                    contextId, pageId, url, NavigationWaitUntil.NetworkIdle);
                
                if (!navResult.Success)
                {
                    return new BrowserPageResult
                    {
                        Success = false,
                        StatusCode = navResult.StatusCode,
                        ErrorMessage = navResult.ErrorMessage
                    };
                }
                
                // Wait for content to stabilize
                await Task.Delay(1000);
                
                // Extract content from the page
                var content = await _browserHandler.ExtractContentAsync(contextId, pageId);
                
                // Build and return the result
                var result = new BrowserPageResult
                {
                    Success = true,
                    HtmlContent = content.HtmlContent,
                    TextContent = content.TextContent,
                    Title = content.Title,
                    StatusCode = navResult.StatusCode,
                    Links = new List<PageLink>()
                };
                
                // Convert links
                foreach (var link in content.Links)
                {
                    result.Links.Add(new PageLink
                    {
                        Href = link.Href,
                        Text = link.Text,
                        IsVisible = link.IsVisible
                    });
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing with headless browser: {url}");
                return new BrowserPageResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                // Always clean up resources
                if (pageId != null && contextId != null)
                {
                    await _browserHandler.ClosePageAsync(contextId, pageId);
                }
                
                if (contextId != null)
                {
                    await _browserHandler.CloseContextAsync(contextId);
                }
            }
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override async Task OnScrapingCompletedAsync()
        {
            // No special cleanup needed here
            await base.OnScrapingCompletedAsync();
        }
        
        /// <summary>
        /// Called when scraping stops
        /// </summary>
        public override async Task OnScrapingStoppedAsync()
        {
            // No special cleanup needed here
            await base.OnScrapingStoppedAsync();
        }
        
        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
                
            _isDisposed = true;
            _browserHandler?.Dispose();
        }
    }
}