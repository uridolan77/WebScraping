using System;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Interfaces;

namespace WebScraper.BrowserHandling
{
    /// <summary>
    /// Implementation of IBrowserHandler that wraps the HeadlessBrowserHandler
    /// </summary>
    public class EnhancedBrowserHandler : IBrowserHandler
    {
        private readonly HeadlessBrowser.HeadlessBrowserHandler _headlessBrowserHandler;
        private readonly Action<string> _logger;
        
        public EnhancedBrowserHandler(HeadlessBrowserOptions options, Action<string> logAction)
        {
            _logger = logAction ?? (msg => { });
            _headlessBrowserHandler = new HeadlessBrowser.HeadlessBrowserHandler(options, logAction);
        }
        
        public async Task InitializeAsync()
        {
            await _headlessBrowserHandler.InitializeAsync();
            _logger("Enhanced browser handler initialized successfully");
        }
        
        public async Task<string> CreateContextAsync()
        {
            return await _headlessBrowserHandler.CreateContextAsync();
        }
        
        public async Task<string> CreatePageAsync(string contextId)
        {
            return await _headlessBrowserHandler.CreatePageAsync(contextId);
        }
        
        public async Task<NavigationResult> NavigateToUrlAsync(string contextId, string pageId, string url, NavigationWaitUntil waitUntil)
        {
            try
            {
                var result = await _headlessBrowserHandler.NavigateToUrlAsync(contextId, pageId, url, (HeadlessBrowser.NavigationWaitUntil)waitUntil);
                
                return new NavigationResult
                {
                    Success = result.Success,
                    ErrorMessage = result.ErrorMessage,
                    StatusCode = result.StatusCode
                };
            }
            catch (Exception ex)
            {
                _logger($"Error navigating to URL {url}: {ex.Message}");
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
                var html = await GetPageHtmlAsync(contextId, pageId);
                var text = await GetPageTextContentAsync(contextId, pageId);
                var title = await GetPageTitleAsync(contextId, pageId);
                
                return new PageContent
                {
                    HtmlContent = html,
                    TextContent = text,
                    Title = title
                };
            }
            catch (Exception ex)
            {
                _logger($"Error extracting content: {ex.Message}");
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
                return await _headlessBrowserHandler.TakeScreenshotAsync(contextId, pageId, outputPath);
            }
            catch (Exception ex)
            {
                _logger($"Error taking screenshot: {ex.Message}");
                return null;
            }
        }

        private object GetPageWrapper(string contextId, string pageId)
        {
            try
            {
                var context = _headlessBrowserHandler.GetBrowserContext(contextId);
                return context?.Pages.FirstOrDefault(p => p.Id == pageId);
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error getting page wrapper for {pageId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPageHtmlAsync(string contextId, string pageId)
        {
            try
            {
                var page = GetPageWrapper(contextId, pageId);
                if (page == null) return null;
                
                var pageType = page.GetType();
                var getContentMethod = pageType.GetMethod("GetContentAsync");
                
                if (getContentMethod != null)
                {
                    var task = getContentMethod.Invoke(page, null) as Task<string>;
                    if (task != null)
                    {
                        return await task;
                    }
                }
                
                _logger?.Invoke($"Could not get HTML content for page {pageId} - method not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error getting HTML for page {pageId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPageTextContentAsync(string contextId, string pageId)
        {
            try
            {
                var context = _headlessBrowserHandler.GetBrowserContext(contextId);
                var page = context?.Pages.FirstOrDefault(p => p.Id == pageId);
                
                if (page != null)
                {
                    var pageType = page.GetType();
                    var evaluateMethod = pageType.GetMethod("EvaluateAsync", new[] { typeof(string) });
                    
                    if (evaluateMethod != null)
                    {
                        var task = evaluateMethod.MakeGenericMethod(typeof(string))
                            .Invoke(page, new object[] { "document.body.innerText" }) as Task<string>;
                        
                        if (task != null)
                        {
                            return await task;
                        }
                    }
                }
                
                _logger?.Invoke($"Could not get text content for page {pageId}");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error getting text content for page {pageId}: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetPageTitleAsync(string contextId, string pageId)
        {
            try
            {
                var page = GetPageWrapper(contextId, pageId);
                if (page == null) return null;
                
                var pageType = page.GetType();
                var titleMethod = pageType.GetMethod("TitleAsync");
                
                if (titleMethod != null)
                {
                    var task = titleMethod.Invoke(page, null) as Task<string>;
                    if (task != null)
                    {
                        return await task;
                    }
                }
                
                _logger?.Invoke($"Could not get title for page {pageId} - method not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error getting title for page {pageId}: {ex.Message}");
                return null;
            }
        }

        public async Task CloseAsync(string contextId)
        {
            try
            {
                await _headlessBrowserHandler.CloseContextAsync(contextId);
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error closing context {contextId}: {ex.Message}");
            }
        }

        // Fix for interface implementation - Adding parameterless CloseAsync()
        public async Task CloseAsync()
        {
            try
            {
                // Close all contexts
                await _headlessBrowserHandler.CloseAllContextsAsync();
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error closing browser: {ex.Message}");
            }
        }

        // Implement the CloseAllContextsAsync method
        public async Task CloseAllContextsAsync()
        {
            try
            {
                var contexts = _headlessBrowserHandler.GetBrowserContexts();
                foreach (var context in contexts)
                {
                    await _headlessBrowserHandler.CloseContextAsync(context.Id);
                }
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error closing all browser contexts: {ex.Message}");
            }
        }
    }
}