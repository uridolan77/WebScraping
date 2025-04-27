using System;
using System.Collections.Generic;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Content item model for interface usage
    /// </summary>
    public class ContentItem
    {
        /// <summary>
        /// URL of the content
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Title of the content
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Type of content (e.g. "text/html", "application/pdf")
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// ID of the scraper that captured the content
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// HTTP status code of the response
        /// </summary>
        public int LastStatusCode { get; set; }
        
        /// <summary>
        /// Raw content data
        /// </summary>
        public string RawContent { get; set; }
        
        /// <summary>
        /// Hash of the content for comparison
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Whether the URL was reachable
        /// </summary>
        public bool IsReachable { get; set; }
        
        /// <summary>
        /// Whether this content is classified as regulatory content
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
    }
}