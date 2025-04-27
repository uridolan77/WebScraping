using System;
using System.Collections.Generic;

namespace WebScraper.Processing
{
    /// <summary>
    /// Represents a content item with its metadata
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
        
        /// <summary>
        /// Size of the content in bytes
        /// </summary>
        public long ContentSize => RawContent?.Length ?? 0;
        
        /// <summary>
        /// When the content was captured
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Additional metadata for this content
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Represents structured content extracted from a document
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Type of node (e.g., "heading", "paragraph", "list", etc.)
        /// </summary>
        public string NodeType { get; set; }
        
        /// <summary>
        /// Text content of the node
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Importance level of the node (1-10)
        /// </summary>
        public int Importance { get; set; }
        
        /// <summary>
        /// Nesting level in the document structure
        /// </summary>
        public int Level { get; set; }
        
        /// <summary>
        /// Child nodes
        /// </summary>
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Metadata for this node
        /// </summary>
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Metadata for document processing
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Author of the document
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// When the document was created
        /// </summary>
        public DateTime? CreatedDate { get; set; }
        
        /// <summary>
        /// When the document was last modified
        /// </summary>
        public DateTime? ModifiedDate { get; set; }
        
        /// <summary>
        /// When the document was published
        /// </summary>
        public DateTime? PublishDate { get; set; }
        
        /// <summary>
        /// When the document was processed
        /// </summary>
        public DateTime ProcessedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Document type
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Number of pages in the document
        /// </summary>
        public int PageCount { get; set; }
        
        /// <summary>
        /// Size of the document in bytes
        /// </summary>
        public long SizeInBytes { get; set; }
        
        /// <summary>
        /// Extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Path to the local file (if downloaded)
        /// </summary>
        public string LocalFilePath { get; set; }
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Extracted metadata from document properties
        /// </summary>
        public Dictionary<string, object> ExtractedMetadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Represents a page's content with structured data
    /// </summary>
    public class PageContent
    {
        /// <summary>
        /// URL of the page
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Title of the page
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// HTML content of the page
        /// </summary>
        public string HtmlContent { get; set; }
        
        /// <summary>
        /// Extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Structured content extracted from the page
        /// </summary>
        public List<ContentNode> StructuredContent { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Metadata extracted from the page
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// When the content was captured
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.Now;
    }
}