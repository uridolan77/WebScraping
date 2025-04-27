using System.Threading.Tasks;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// The condition to wait for before considering navigation complete
    /// </summary>
    public enum NavigationWaitUntil
    {
        /// <summary>
        /// Wait until the DOMContentLoaded event is fired
        /// </summary>
        DOMContentLoaded,
        
        /// <summary>
        /// Wait until the load event is fired
        /// </summary>
        Load,
        
        /// <summary>
        /// Wait until the network is idle (no requests for at least 500ms)
        /// </summary>
        NetworkIdle,
        
        /// <summary>
        /// Wait until there are no more than 0 network connections for at least 500ms
        /// </summary>
        NetworkAlmostIdle
    }

    /// <summary>
    /// Interface for headless browser operations
    /// </summary>
    public interface IBrowserHandler
    {
        /// <summary>
        /// Initialize the browser
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Create a new browser context
        /// </summary>
        Task<string> CreateContextAsync();

        /// <summary>
        /// Create a new page in a context
        /// </summary>
        Task<string> CreatePageAsync(string contextId);

        /// <summary>
        /// Navigate to a URL
        /// </summary>
        Task<NavigationResult> NavigateToUrlAsync(string contextId, string pageId, string url, NavigationWaitUntil waitUntil);

        /// <summary>
        /// Extract content from a page
        /// </summary>
        Task<PageContent> ExtractContentAsync(string contextId, string pageId);

        /// <summary>
        /// Take a screenshot of a page
        /// </summary>
        Task<string> TakeScreenshotAsync(string contextId, string pageId, string? outputPath = null);

        /// <summary>
        /// Close the browser
        /// </summary>
        Task CloseAsync();
    }

    /// <summary>
    /// Result of a navigation operation
    /// </summary>
    public class NavigationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public int StatusCode { get; set; }
        
        public NavigationResult()
        {
            Success = false;
            StatusCode = 0;
        }
    }

    /// <summary>
    /// Content extracted from a page
    /// </summary>
    public class PageContent
    {
        public string HtmlContent { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}