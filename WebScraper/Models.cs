using System;
using System.Collections.Generic;

namespace WebScraper
{
    /// <summary>
    /// Scraped page data
    /// </summary>
    public class ScrapedPage
    {
        /// <summary>
        /// URL of the scraped page
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Date and time when the page was scraped
        /// </summary>
        public DateTime ScrapedDateTime { get; set; }
        
        /// <summary>
        /// Textual content of the page
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Depth of the page in the site hierarchy
        /// </summary>
        public int Depth { get; set; }
    }

    /// <summary>
    /// Custom string builder with logging
    /// </summary>
    public class CustomStringBuilder
    {
        private readonly System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();
        private readonly Action<string> _logAction;

        public CustomStringBuilder(Action<string> logAction)
        {
            _logAction = logAction;
        }

        // Property to expose the internal StringBuilder
        public System.Text.StringBuilder InternalBuilder => _stringBuilder;

        // Forward needed methods to the internal StringBuilder
        public CustomStringBuilder Append(string value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        public CustomStringBuilder AppendLine(string value)
        {
            _stringBuilder.AppendLine(value);
            return this;
        }

        public override string ToString()
        {
            var result = _stringBuilder.ToString();
            _logAction(result);
            _stringBuilder.Clear();
            return result;
        }

        public void Clear()
        {
            _stringBuilder.Clear();
        }
    }

    /// <summary>
    /// Result of processing a pipeline
    /// </summary>
    public class PipelineProcessingResult
    {
        /// <summary>
        /// URL of the processed item
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Whether processing was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Legacy ContentNode implementation for backward compatibility
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Title of the content node
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Textual content of the node
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Type of content node (e.g., Section, Article, Division)
        /// </summary>
        public string NodeType { get; set; }
        
        /// <summary>
        /// Depth of the node in the hierarchy (0 for root)
        /// </summary>
        public int Depth { get; set; }
        
        /// <summary>
        /// Child content nodes
        /// </summary>
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Metadata about the content node
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Relevance score for the content (0.0-1.0)
        /// </summary>
        public double RelevanceScore { get; set; }
        
        /// <summary>
        /// HTML attributes of the source node
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Converts this node to a RegulatoryFramework ContentNode
        /// </summary>
        public RegulatoryFramework.Interfaces.ContentNode ToFrameworkContentNode()
        {
            return RegulatoryFramework.Interfaces.ContentNode.FromLegacyContentNode(this);
        }
    }

    /// <summary>
    /// PipelineStatus for legacy compatibility
    /// </summary>
    public class PipelineStatus
    {
        /// <summary>
        /// Whether the pipeline is currently running
        /// </summary>
        public bool IsRunning { get; set; }
        
        /// <summary>
        /// Total number of items processed through the pipeline
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Number of successfully processed items
        /// </summary>
        public int ProcessedItems { get; set; }
        
        /// <summary>
        /// Number of failed items
        /// </summary>
        public int FailedItems { get; set; }
        
        /// <summary>
        /// Current operation description
        /// </summary>
        public string CurrentOperation { get; set; }
        
        /// <summary>
        /// Time when the pipeline started processing
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// Time when the pipeline finished processing
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// State information for a scraper
    /// </summary>
    public class ScraperState
    {
        /// <summary>
        /// ID of the scraper
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// Current status of the scraper
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// When the last run started
        /// </summary>
        public DateTime LastRunStartTime { get; set; }
        
        /// <summary>
        /// When the last run ended
        /// </summary>
        public DateTime LastRunEndTime { get; set; }
        
        /// <summary>
        /// When the last successful run completed
        /// </summary>
        public DateTime? LastSuccessfulRunTime { get; set; }
        
        /// <summary>
        /// Progress data in JSON format
        /// </summary>
        public string ProgressData { get; set; }
        
        /// <summary>
        /// Configuration snapshot in JSON format
        /// </summary>
        public string ConfigSnapshot { get; set; }
        
        /// <summary>
        /// Domain being scraped
        /// </summary>
        public string ConfiguredDomain { get; set; }
        
        /// <summary>
        /// Enabled features
        /// </summary>
        public List<string> EnabledFeatures { get; set; } = new List<string>();
        
        /// <summary>
        /// Last error message
        /// </summary>
        public string LastError { get; set; }
        
        /// <summary>
        /// Number of pages scraped
        /// </summary>
        public int PagesScraped { get; set; }
        
        /// <summary>
        /// Number of errors occurred
        /// </summary>
        public int ErrorCount { get; set; }
        
        /// <summary>
        /// When the state was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Regulatory impact levels for changes
    /// </summary>
    public enum RegulatoryImpact
    {
        None = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }

    /// <summary>
    /// Result of change analysis
    /// </summary>
    public class ChangeAnalysisResult
    {
        /// <summary>
        /// Whether there are changes
        /// </summary>
        public bool HasChanges { get; set; }
        
        /// <summary>
        /// Percentage of change
        /// </summary>
        public double ChangePercentage { get; set; }
        
        /// <summary>
        /// Content that was added
        /// </summary>
        public List<string> AddedContent { get; set; } = new List<string>();
        
        /// <summary>
        /// Content that was removed
        /// </summary>
        public List<string> RemovedContent { get; set; } = new List<string>();
        
        /// <summary>
        /// Content that was modified
        /// </summary>
        public List<string> ModifiedContent { get; set; } = new List<string>();
    }

    /// <summary>
    /// Result of content classification
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Whether the content is regulatory
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// Confidence score for the classification
        /// </summary>
        public double ConfidenceScore { get; set; }
        
        /// <summary>
        /// Category of the content
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Regulatory impact of the content
        /// </summary>
        public RegulatoryImpact Impact { get; set; }
        
        /// <summary>
        /// List of keywords found in the content
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();
        
        /// <summary>
        /// Primary category of the content
        /// </summary>
        public string PrimaryCategory { get; set; }
    }

    /// <summary>
    /// Metadata extracted from a document
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// URL of the document
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Title of the document
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Type of the document (e.g., PDF, DOCX)
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Publication date of the document
        /// </summary>
        public DateTime? PublicationDate { get; set; }
        
        /// <summary>
        /// Author of the document
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// Number of pages in the document
        /// </summary>
        public int PageCount { get; set; }
        
        /// <summary>
        /// Text content of the document
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Hash of the content for integrity checking
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Classification result of the document
        /// </summary>
        public ClassificationResult Classification { get; set; }
        
        /// <summary>
        /// Extracted metadata in key-value pairs
        /// </summary>
        public Dictionary<string, object> ExtractedMetadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Publish date for the document
        /// </summary>
        public DateTime? PublishDate { get; set; }
        
        /// <summary>
        /// Local file path where the document is stored
        /// </summary>
        public string LocalFilePath { get; set; }
        
        /// <summary>
        /// When the document was processed
        /// </summary>
        public DateTime ProcessedDate { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Validation result for configurations or content
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the configuration or content is valid
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Whether it can run with warnings
        /// </summary>
        public bool CanRunWithWarnings { get; set; }
        
        /// <summary>
        /// List of error messages
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// List of warning messages
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}