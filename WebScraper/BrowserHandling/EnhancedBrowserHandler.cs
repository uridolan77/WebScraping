using System;
using System.Threading.Tasks;
using System.IO;
using WebScraper.Interfaces;

namespace WebScraper.BrowserHandling
{
    /// <summary>
    /// Implementation of IBrowserHandler that wraps the HeadlessBrowserHandler
    /// </summary>
    public class EnhancedBrowserHandler : IBrowserHandler
    {
        private readonly HeadlessBrowser.HeadlessBrowserHandler _browserHandler;
        private readonly Action<string> _logAction;
        
        public EnhancedBrowserHandler(HeadlessBrowserOptions options, Action<string> logAction)
        {
            _logAction = logAction ?? (msg => { });
            _browserHandler = new HeadlessBrowser.HeadlessBrowserHandler(options, logAction);
        }
        
        public async Task InitializeAsync()
        {
            await _browserHandler.InitializeAsync();
            _logAction("Enhanced browser handler initialized successfully");
        }
        
        public async Task<string> CreateContextAsync()
        {
            return await _browserHandler.CreateContextAsync();
        }
        
        public async Task<string> CreatePageAsync(string contextId)
        {
            return await _browserHandler.CreatePageAsync(contextId);
        }
        
        public async Task<NavigationResult> NavigateToUrlAsync(string contextId, string pageId, string url, NavigationWaitUntil waitUntil)
        {
            try
            {
                var result = await _browserHandler.NavigateToUrlAsync(contextId, pageId, url, (HeadlessBrowser.NavigationWaitUntil)waitUntil);
                
                return new NavigationResult
                {
                    Success = result.Success,
                    ErrorMessage = result.ErrorMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logAction($"Error navigating to URL {url}: {ex.Message}");
                return new NavigationResult
                {
                    Success = false,
                    ErrorMessage = ex.Message,
                    StatusCode = 0
                };
            }
        }
        
        public async Task<PageContent> ExtractContentAsync(string contextId, string pageId)
        {
            try
            {
                var html = await _browserHandler.GetPageHtmlAsync(contextId, pageId);
                var text = await _browserHandler.GetPageTextContentAsync(contextId, pageId);
                var title = await _browserHandler.GetPageTitleAsync(contextId, pageId);
                
                return new PageContent
                {
                    HtmlContent = html,
                    TextContent = text,
                    Title = title
                };
            }
            catch (Exception ex)
            {
                _logAction($"Error extracting content: {ex.Message}");
                return new PageContent
                {
                    HtmlContent = string.Empty,
                    TextContent = string.Empty,
                    Title = string.Empty
                };
            }
        }
        
        public async Task<string> TakeScreenshotAsync(string contextId, string pageId, string outputPath = null)
        {
            try
            {
                return await _browserHandler.TakeScreenshotAsync(contextId, pageId, outputPath);
            }
            catch (Exception ex)
            {
                _logAction($"Error taking screenshot: {ex.Message}");
                return null;
            }
        }
        
        public async Task CloseAsync()
        {
            try
            {
                await _browserHandler.CloseAsync();
                _logAction("Browser closed successfully");
            }
            catch (Exception ex)
            {
                _logAction($"Error closing browser: {ex.Message}");
            }
        }
    }
}