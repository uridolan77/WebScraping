using System;
using System.Collections.Generic;

namespace WebScraper.Processing
{
    /// <summary>
    /// Represents content scraped from a website
    /// </summary>
    public class ContentItem
    {
        /// <summary>
        /// Gets or sets the URL the content was scraped from
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the scraper that scraped this content
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// Gets or sets the HTTP status code from the last request
        /// </summary>
        public int LastStatusCode { get; set; }
        
        /// <summary>
        /// Gets or sets the content type of the response
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Gets or sets whether the URL is reachable
        /// </summary>
        public bool IsReachable { get; set; }
        
        /// <summary>
        /// Gets or sets the raw content of the page
        /// </summary>
        public string RawContent { get; set; }
        
        /// <summary>
        /// Gets or sets the extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Gets or sets the hash of the content
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is regulatory content
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// Gets or sets when this content was captured
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Represents the result of processing a document
    /// </summary>
    public class DocumentProcessingResult
    {
        /// <summary>
        /// Gets or sets the extracted text from the document
        /// </summary>
        public string Text { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the document
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the author of the document
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// Gets or sets the creation date of the document
        /// </summary>
        public DateTime? CreationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the modification date of the document
        /// </summary>
        public DateTime? ModificationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the page count in the document
        /// </summary>
        public int PageCount { get; set; }
        
        /// <summary>
        /// Gets or sets keywords associated with the document
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();
    }
}