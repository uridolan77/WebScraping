using System;
using System.Threading.Tasks;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Interface for all scraper components
    /// </summary>
    public interface IScraperComponent
    {
        /// <summary>
        /// Initializes the component
        /// </summary>
        /// <param name="core">The scraper core instance</param>
        Task InitializeAsync(ScraperCore core);
        
        /// <summary>
        /// Called when scraping starts
        /// </summary>
        Task OnScrapingStartedAsync();
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        Task OnScrapingCompletedAsync();
        
        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        Task OnScrapingStoppedAsync();
    }
    
    /// <summary>
    /// Interface for components that handle document processing
    /// </summary>
    public interface IDocumentProcessor : IScraperComponent
    {
        /// <summary>
        /// Processes a document
        /// </summary>
        /// <param name="url">The URL of the document</param>
        /// <param name="content">The document content</param>
        /// <param name="contentType">The content type of the document</param>
        Task ProcessDocumentAsync(string url, byte[] content, string contentType);
    }
    
    /// <summary>
    /// Interface for components that handle state management
    /// </summary>
    public interface IStateManager : IScraperComponent
    {
        /// <summary>
        /// Saves the state of the scraper
        /// </summary>
        /// <param name="status">The current status</param>
        Task SaveStateAsync(string status);
        
        /// <summary>
        /// Marks a URL as visited
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="statusCode">The HTTP status code</param>
        Task MarkUrlVisitedAsync(string url, int statusCode);
        
        /// <summary>
        /// Checks if a URL has been visited
        /// </summary>
        /// <param name="url">The URL to check</param>
        /// <returns>True if the URL has been visited</returns>
        Task<bool> HasUrlBeenVisitedAsync(string url);
        
        /// <summary>
        /// Saves content for a URL
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="content">The content</param>
        /// <param name="contentType">The content type</param>
        Task SaveContentAsync(string url, string content, string contentType);
    }
    
    /// <summary>
    /// Interface for components that handle content change detection
    /// </summary>
    public interface IChangeDetector : IScraperComponent
    {
        /// <summary>
        /// Tracks a new version of a page
        /// </summary>
        /// <param name="url">The URL of the page</param>
        /// <param name="content">The content of the page</param>
        /// <param name="contentType">The content type</param>
        /// <returns>Information about the changes</returns>
        Task<object> TrackPageVersionAsync(string url, string content, string contentType);
    }
    
    /// <summary>
    /// Result of a browser page navigation
    /// </summary>
    public class BrowserPageResult
    {
        /// <summary>
        /// The HTML content of the page
        /// </summary>
        public string HtmlContent { get; set; }
        
        /// <summary>
        /// The extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// The page title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// The HTTP status code
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Whether the navigation was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if navigation failed
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Links extracted from the page
        /// </summary>
        public System.Collections.Generic.List<PageLink> Links { get; set; } = new System.Collections.Generic.List<PageLink>();
    }
    
    /// <summary>
    /// Represents a link on a page
    /// </summary>
    public class PageLink
    {
        /// <summary>
        /// The href attribute value
        /// </summary>
        public string Href { get; set; }
        
        /// <summary>
        /// The link text
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Whether the link is visible on the page
        /// </summary>
        public bool IsVisible { get; set; }
    }
}