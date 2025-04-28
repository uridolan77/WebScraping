using System;
using System.Collections.Generic;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents a processed document from a scraper
    /// </summary>
    public class ProcessedDocument
    {
        /// <summary>
        /// Unique identifier for the document
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// URL where the document was found
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of document (HTML, PDF, etc.)
        /// </summary>
        public string DocumentType { get; set; } = "HTML";
        
        /// <summary>
        /// When the document was processed
        /// </summary>
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Size of the document content in bytes
        /// </summary>
        public long ContentSizeBytes { get; set; }
        
        /// <summary>
        /// Metadata associated with the document
        /// </summary>
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }
}
