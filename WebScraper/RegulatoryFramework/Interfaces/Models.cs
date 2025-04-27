using System;
using System.Collections.Generic;

namespace WebScraper.RegulatoryFramework.Interfaces
{
    /// <summary>
    /// Represents a version of a page's content
    /// </summary>
    public class PageVersion
    {
        /// <summary>
        /// URL of the page
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Content hash for version comparison
        /// </summary>
        public string Hash { get; set; }
        
        /// <summary>
        /// When this version was captured
        /// </summary>
        public DateTime CapturedAt { get; set; }
        
        /// <summary>
        /// Short summary of the content
        /// </summary>
        public string ContentSummary { get; set; }
        
        /// <summary>
        /// Full HTML content
        /// </summary>
        public string FullContent { get; set; }
        
        /// <summary>
        /// Extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Type of change from previous version
        /// </summary>
        public object ChangeFromPrevious { get; set; }
        
        /// <summary>
        /// Additional metadata for this version
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Represents metadata about a regulatory document
    /// </summary>
    public class RegulatoryDocumentMetadata
    {
        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Author or issuing organization
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// Date the document was published
        /// </summary>
        public DateTime? PublishedDate { get; set; }
        
        /// <summary>
        /// Date the document was last updated
        /// </summary>
        public DateTime? LastUpdatedDate { get; set; }
        
        /// <summary>
        /// Document type (e.g., "regulation", "guidance", "consultation")
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Regulatory jurisdiction
        /// </summary>
        public string Jurisdiction { get; set; }
        
        /// <summary>
        /// Regulatory body that issued the document
        /// </summary>
        public string RegulatoryBody { get; set; }
        
        /// <summary>
        /// Importance or impact level
        /// </summary>
        public int ImportanceLevel { get; set; }
        
        /// <summary>
        /// Key topics addressed in the document
        /// </summary>
        public List<string> Topics { get; set; } = new List<string>();
        
        /// <summary>
        /// Additional metadata
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Results of change analysis
    /// </summary>
    public class ChangeAnalysisResult
    {
        /// <summary>
        /// Whether changes were detected
        /// </summary>
        public bool HasChanges { get; set; }
        
        /// <summary>
        /// Type of changes detected
        /// </summary>
        public string ChangeType { get; set; }
        
        /// <summary>
        /// Significance of the changes
        /// </summary>
        public int SignificanceLevel { get; set; }
        
        /// <summary>
        /// Summary of the changes
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Sections that were changed
        /// </summary>
        public List<string> AffectedSections { get; set; } = new List<string>();
        
        /// <summary>
        /// Detailed changes
        /// </summary>
        public Dictionary<string, object> Changes { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// Results of content classification
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Primary category of the content
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Confidence level of the classification (0-1)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Alternative categories with confidence levels
        /// </summary>
        public Dictionary<string, double> AlternativeCategories { get; set; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Topics identified in the content
        /// </summary>
        public List<string> Topics { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether the content is regulatory in nature
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// Relevance score for regulatory analysis (0-1)
        /// </summary>
        public double RegulatoryRelevance { get; set; }
    }
}