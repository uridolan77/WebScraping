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
    /// Unified ContentNode implementation that consolidates functionality from all versions
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Type of the node
        /// </summary>
        public string NodeType { get; set; }
        
        /// <summary>
        /// Type property for backward compatibility
        /// </summary>
        public string Type
        {
            get { return NodeType; }
            set { NodeType = value; }
        }
        
        /// <summary>
        /// Content of the node
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Depth in the hierarchy
        /// </summary>
        public int Depth { get; set; }
        
        /// <summary>
        /// Level property for backward compatibility
        /// </summary>
        public int Level
        {
            get { return Depth; }
            set { Depth = value; }
        }
        
        /// <summary>
        /// Title of the node
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Relevance score of the node
        /// </summary>
        public double RelevanceScore { get; set; }
        
        /// <summary>
        /// Attributes of the node
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Children of the node
        /// </summary>
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Metadata of the node
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Additional metadata as string values for backward compatibility
        /// </summary>
        public Dictionary<string, string> MetadataStrings
        {
            get
            {
                var result = new Dictionary<string, string>();
                foreach (var kvp in Metadata)
                {
                    result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
                return result;
            }
            set
            {
                foreach (var kvp in value)
                {
                    Metadata[kvp.Key] = kvp.Value;
                }
            }
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
        /// Gets or sets the configured domain
        /// </summary>
        public string ConfiguredDomain { get; set; }
        
        /// <summary>
        /// Gets or sets the enabled features
        /// </summary>
        public Dictionary<string, bool> EnabledFeatures { get; set; } = new Dictionary<string, bool>();
        
        /// <summary>
        /// Gets or sets the crawl strategy metadata
        /// </summary>
        public Dictionary<string, object> CrawlStrategyMetadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets or sets the last run time
        /// </summary>
        public DateTime? LastRunTime { get; set; }
        
        /// <summary>
        /// Gets or sets the number of pages processed
        /// </summary>
        public int PagesProcessed { get; set; }
        
        /// <summary>
        /// Gets or sets the number of errors encountered
        /// </summary>
        public int ErrorsEncountered { get; set; }
        
        /// <summary>
        /// Gets or sets the last error message
        /// </summary>
        public string LastError { get; set; }
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
    /// ClassificationResult for content classification
    /// </summary>
    public class ClassificationResult
    {
        /// <summary>
        /// Gets or sets the primary category of the content
        /// </summary>
        public string PrimaryCategory { get; set; }
        
        /// <summary>
        /// Gets or sets the category of the content (legacy property)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence level of the classification (0.0-1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the list of all categories with their confidence scores
        /// </summary>
        public Dictionary<string, double> CategoryScores { get; set; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Gets or sets the regulatory impact of the content
        /// </summary>
        public RegulatoryImpact Impact { get; set; } = RegulatoryImpact.None;
        
        /// <summary>
        /// Gets or sets the list of keywords that contributed to the classification
        /// </summary>
        public List<string> MatchedKeywords { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the timestamp of the classification
        /// </summary>
        public DateTime ClassificationTime { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// DocumentMetadata for document processing
    /// </summary>
    public class DocumentMetadata
    {
        /// <summary>
        /// Gets or sets the document URL
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the document title
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the document author
        /// </summary>
        public string Author { get; set; }
        
        /// <summary>
        /// Gets or sets the document creation date
        /// </summary>
        public DateTime? CreationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the document last modified date
        /// </summary>
        public DateTime? LastModifiedDate { get; set; }
        
        /// <summary>
        /// Gets or sets the document page count
        /// </summary>
        public int PageCount { get; set; }
        
        /// <summary>
        /// Gets or sets the document word count
        /// </summary>
        public int WordCount { get; set; }
        
        /// <summary>
        /// Gets or sets the document file type
        /// </summary>
        public string FileType { get; set; }
        
        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Gets or sets the document file size in bytes
        /// </summary>
        public long FileSizeBytes { get; set; }
        
        /// <summary>
        /// Gets or sets the document keywords
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the document subject
        /// </summary>
        public string Subject { get; set; }
        
        /// <summary>
        /// Gets or sets the document categories
        /// </summary>
        public List<string> Categories { get; set; } = new List<string>();
        
        /// <summary>
        /// Gets or sets the document comments
        /// </summary>
        public string Comments { get; set; }
        
        /// <summary>
        /// Gets or sets the document content type
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Gets or sets the document language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the date the document was processed
        /// </summary>
        public DateTime ProcessedDate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets or sets the document publication date
        /// </summary>
        public DateTime? PublicationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the document publish date (alias for PublicationDate)
        /// </summary>
        public DateTime? PublishDate { get; set; }
        
        /// <summary>
        /// Gets or sets the path to the locally stored file
        /// </summary>
        public string LocalFilePath { get; set; }
        
        /// <summary>
        /// Gets or sets the content hash of the document
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Gets or sets the text content of the document
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Gets or sets the document classification
        /// </summary>
        public ClassificationResult Classification { get; set; }
        
        /// <summary>
        /// Gets or sets additional metadata extracted from the document
        /// </summary>
        public Dictionary<string, object> ExtractedMetadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Gets or sets additional custom metadata
        /// </summary>
        public Dictionary<string, object> CustomMetadata { get; set; } = new Dictionary<string, object>();
    }
}