using System;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Interface for content scraped from a website
    /// </summary>
    public interface ContentItem
    {
        /// <summary>
        /// Gets or sets the URL the content was scraped from
        /// </summary>
        string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the scraper that scraped this content
        /// </summary>
        string ScraperId { get; set; }
        
        /// <summary>
        /// Gets or sets the HTTP status code from the last request
        /// </summary>
        int LastStatusCode { get; set; }
        
        /// <summary>
        /// Gets or sets the content type of the response
        /// </summary>
        string ContentType { get; set; }
        
        /// <summary>
        /// Gets or sets whether the URL is reachable
        /// </summary>
        bool IsReachable { get; set; }
        
        /// <summary>
        /// Gets or sets the raw content of the page
        /// </summary>
        string RawContent { get; set; }
        
        /// <summary>
        /// Gets or sets the hash of the content
        /// </summary>
        string ContentHash { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is regulatory content
        /// </summary>
        bool IsRegulatoryContent { get; set; }
    }
}