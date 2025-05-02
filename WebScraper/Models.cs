using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace WebScraper
{
    /// <summary>
    /// Scraped page data
    /// </summary>
    public record ScrapedPage
    {
        /// <summary>
        /// URL of the scraped page
        /// </summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>
        /// Date and time when the page was scraped
        /// </summary>
        public DateTime ScrapedDateTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Textual content of the page
        /// </summary>
        public string TextContent { get; init; } = string.Empty;

        /// <summary>
        /// Depth of the page in the site hierarchy
        /// </summary>
        public int Depth { get; init; }
    }

    /// <summary>
    /// Custom string builder with logging
    /// </summary>
    public class CustomStringBuilder
    {
        private readonly StringBuilder _stringBuilder = new();
        private readonly Action<string> _logAction;

        /// <summary>
        /// Creates a new CustomStringBuilder with logging
        /// </summary>
        /// <param name="logAction">Action to log the string when ToString() is called</param>
        public CustomStringBuilder(Action<string> logAction)
        {
            _logAction = logAction ?? (_ => { }); // Default no-op logger if none provided
        }

        /// <summary>
        /// Property to expose the internal StringBuilder
        /// </summary>
        public StringBuilder InternalBuilder => _stringBuilder;

        /// <summary>
        /// Appends a string to the builder
        /// </summary>
        /// <param name="value">String to append</param>
        /// <returns>This CustomStringBuilder for chaining</returns>
        public CustomStringBuilder Append(string value)
        {
            _stringBuilder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends a string followed by a line terminator to the builder
        /// </summary>
        /// <param name="value">String to append</param>
        /// <returns>This CustomStringBuilder for chaining</returns>
        public CustomStringBuilder AppendLine(string value)
        {
            _stringBuilder.AppendLine(value);
            return this;
        }

        /// <summary>
        /// Converts the value of this instance to a string, logs it, and clears the builder
        /// </summary>
        /// <returns>The string representation of this instance</returns>
        public override string ToString()
        {
            var result = _stringBuilder.ToString();
            _logAction(result);
            _stringBuilder.Clear();
            return result;
        }

        /// <summary>
        /// Clears the contents of the builder
        /// </summary>
        public void Clear() => _stringBuilder.Clear();
    }

    /// <summary>
    /// Result of processing a pipeline
    /// </summary>
    public record PipelineProcessingResult
    {
        /// <summary>
        /// URL of the processed item
        /// </summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>
        /// Whether processing was successful
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Error message if processing failed
        /// </summary>
        public string Error { get; init; } = string.Empty;

        /// <summary>
        /// When the processing was completed
        /// </summary>
        public DateTime CompletedAt { get; init; } = DateTime.Now;

        /// <summary>
        /// Duration of the processing in milliseconds
        /// </summary>
        public long DurationMs { get; init; }

        /// <summary>
        /// Creates a successful result
        /// </summary>
        /// <param name="url">URL of the processed item</param>
        /// <param name="durationMs">Duration of the processing in milliseconds</param>
        /// <returns>A successful result</returns>
        public static PipelineProcessingResult CreateSuccess(string url, long durationMs = 0) =>
            new() { Url = url, Success = true, DurationMs = durationMs };

        /// <summary>
        /// Creates a failed result
        /// </summary>
        /// <param name="url">URL of the processed item</param>
        /// <param name="error">Error message</param>
        /// <param name="durationMs">Duration of the processing in milliseconds</param>
        /// <returns>A failed result</returns>
        public static PipelineProcessingResult CreateFailure(string url, string error, long durationMs = 0) =>
            new() { Url = url, Success = false, Error = error, DurationMs = durationMs };
    }

    /// <summary>
    /// Unified ContentNode implementation that consolidates functionality from all versions
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Type of the node
        /// </summary>
        public string NodeType { get; set; } = string.Empty;

        /// <summary>
        /// Type property for backward compatibility
        /// </summary>
        public string Type
        {
            get => NodeType;
            set => NodeType = value;
        }

        /// <summary>
        /// Content of the node
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Depth in the hierarchy
        /// </summary>
        public int Depth { get; set; }

        /// <summary>
        /// Level property for backward compatibility
        /// </summary>
        public int Level
        {
            get => Depth;
            set => Depth = value;
        }

        /// <summary>
        /// Title of the node
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Relevance score of the node
        /// </summary>
        public double RelevanceScore { get; set; }

        /// <summary>
        /// Attributes of the node
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Children of the node
        /// </summary>
        public List<ContentNode> Children { get; set; } = new();

        /// <summary>
        /// Metadata of the node
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Additional metadata as string values for backward compatibility
        /// </summary>
        public Dictionary<string, string> MetadataStrings
        {
            get
            {
                var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var kvp in Metadata)
                {
                    result[kvp.Key] = kvp.Value?.ToString() ?? string.Empty;
                }
                return result;
            }
            set
            {
                if (value == null) return;

                foreach (var kvp in value)
                {
                    Metadata[kvp.Key] = kvp.Value;
                }
            }
        }

        /// <summary>
        /// URL associated with this node, if any
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Source of this content node
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// When this node was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Creates a deep clone of this ContentNode
        /// </summary>
        public ContentNode Clone()
        {
            var clone = new ContentNode
            {
                NodeType = this.NodeType,
                Content = this.Content,
                Depth = this.Depth,
                Title = this.Title,
                RelevanceScore = this.RelevanceScore,
                Url = this.Url,
                Source = this.Source,
                CreatedAt = this.CreatedAt
            };

            // Copy attributes (using dictionary initializer for better performance)
            clone.Attributes = new Dictionary<string, string>(this.Attributes, StringComparer.OrdinalIgnoreCase);

            // Copy metadata (using dictionary initializer for better performance)
            clone.Metadata = new Dictionary<string, object>(this.Metadata, StringComparer.OrdinalIgnoreCase);

            // Clone children recursively (with capacity for better performance)
            clone.Children = new List<ContentNode>(this.Children.Count);
            foreach (var child in this.Children)
            {
                clone.Children.Add(child.Clone());
            }

            return clone;
        }

        /// <summary>
        /// Gets all descendant nodes as a flattened list
        /// </summary>
        public List<ContentNode> GetAllDescendants()
        {
            // Estimate capacity to reduce reallocations
            var result = new List<ContentNode>(Children.Count * 2);
            CollectDescendants(result);
            return result;
        }

        /// <summary>
        /// Helper method to collect descendants recursively
        /// </summary>
        private void CollectDescendants(List<ContentNode> result)
        {
            foreach (var child in Children)
            {
                result.Add(child);
                child.CollectDescendants(result);
            }
        }

        /// <summary>
        /// Gets the text content of this node and all its descendants
        /// </summary>
        public string GetFullTextContent()
        {
            // Estimate capacity to reduce reallocations
            var sb = new StringBuilder(EstimateTextCapacity());

            // Add this node's content
            if (!string.IsNullOrEmpty(Content))
            {
                sb.AppendLine(Content);
            }

            // Add children's content
            foreach (var child in Children)
            {
                sb.AppendLine(child.GetFullTextContent());
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// Estimates the capacity needed for the text content
        /// </summary>
        private int EstimateTextCapacity()
        {
            int estimate = Content?.Length ?? 0;

            // Add a rough estimate for each child
            foreach (var child in Children)
            {
                estimate += child.Content?.Length ?? 0;
            }

            // Add some extra for line breaks and potential growth
            return Math.Max(estimate, 1024);
        }

        /// <summary>
        /// Creates a new ContentNode with the specified type and content
        /// </summary>
        public static ContentNode Create(string nodeType, string content) =>
            new() { NodeType = nodeType, Content = content };

        /// <summary>
        /// Creates a new ContentNode with the specified type, content, and title
        /// </summary>
        public static ContentNode Create(string nodeType, string content, string title) =>
            new() { NodeType = nodeType, Content = content, Title = title };
    }

    /// <summary>
    /// PipelineStatus for legacy compatibility
    /// </summary>
    public record PipelineStatus
    {
        /// <summary>
        /// Whether the pipeline is currently running
        /// </summary>
        public bool IsRunning { get; init; }

        /// <summary>
        /// Total number of items processed through the pipeline
        /// </summary>
        public int TotalItems { get; init; }

        /// <summary>
        /// Number of successfully processed items
        /// </summary>
        public int ProcessedItems { get; init; }

        /// <summary>
        /// Number of failed items
        /// </summary>
        public int FailedItems { get; init; }

        /// <summary>
        /// Current operation description
        /// </summary>
        public string CurrentOperation { get; init; } = string.Empty;

        /// <summary>
        /// Time when the pipeline started processing
        /// </summary>
        public DateTime StartTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Time when the pipeline finished processing
        /// </summary>
        public DateTime? EndTime { get; init; }

        /// <summary>
        /// Elapsed time since the pipeline started
        /// </summary>
        public TimeSpan ElapsedTime => EndTime.HasValue
            ? EndTime.Value - StartTime
            : DateTime.Now - StartTime;

        /// <summary>
        /// Percentage of completion (0-100)
        /// </summary>
        public double CompletionPercentage => TotalItems > 0
            ? Math.Min(100, (ProcessedItems + FailedItems) * 100.0 / TotalItems)
            : 0;

        /// <summary>
        /// Creates a new PipelineStatus for a running pipeline
        /// </summary>
        public static PipelineStatus CreateRunning(string operation = "Starting") =>
            new() {
                IsRunning = true,
                CurrentOperation = operation
            };

        /// <summary>
        /// Creates a new PipelineStatus for a completed pipeline
        /// </summary>
        public static PipelineStatus CreateCompleted(int processed, int failed) =>
            new() {
                IsRunning = false,
                ProcessedItems = processed,
                FailedItems = failed,
                TotalItems = processed + failed,
                EndTime = DateTime.Now,
                CurrentOperation = "Completed"
            };
    }

    /// <summary>
    /// State information for a scraper
    /// </summary>
    public record ScraperState
    {
        /// <summary>
        /// Gets or sets the configured domain
        /// </summary>
        public string ConfiguredDomain { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the enabled features
        /// </summary>
        public Dictionary<string, bool> EnabledFeatures { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the crawl strategy metadata
        /// </summary>
        public Dictionary<string, object> CrawlStrategyMetadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the last run time
        /// </summary>
        public DateTime? LastRunTime { get; init; }

        /// <summary>
        /// Gets or sets the number of pages processed
        /// </summary>
        public int PagesProcessed { get; init; }

        /// <summary>
        /// Gets or sets the number of errors encountered
        /// </summary>
        public int ErrorsEncountered { get; init; }

        /// <summary>
        /// Gets or sets the last error message
        /// </summary>
        public string LastError { get; init; } = string.Empty;

        /// <summary>
        /// Gets the success rate (0.0-1.0)
        /// </summary>
        public double SuccessRate => PagesProcessed > 0
            ? Math.Max(0, Math.Min(1, 1 - (double)ErrorsEncountered / PagesProcessed))
            : 1.0;

        /// <summary>
        /// Gets whether the scraper is currently running
        /// </summary>
        public bool IsRunning { get; init; }

        /// <summary>
        /// Gets the current status of the scraper
        /// </summary>
        public string Status => IsRunning ? "Running" : (LastRunTime.HasValue ? "Completed" : "Not Started");

        /// <summary>
        /// Creates a new ScraperState for a running scraper
        /// </summary>
        public static ScraperState CreateRunning(string domain) =>
            new() {
                ConfiguredDomain = domain,
                IsRunning = true,
                LastRunTime = DateTime.Now
            };

        /// <summary>
        /// Creates a new ScraperState for a completed scraper
        /// </summary>
        public static ScraperState CreateCompleted(string domain, int processed, int errors, string lastError = "") =>
            new() {
                ConfiguredDomain = domain,
                IsRunning = false,
                LastRunTime = DateTime.Now,
                PagesProcessed = processed,
                ErrorsEncountered = errors,
                LastError = lastError
            };
    }

    /// <summary>
    /// Canonical implementation of ContentItem that will be used throughout the project
    /// </summary>
    public class ContentItem : Interfaces.ContentItem
    {
        /// <summary>
        /// Gets or sets the URL the content was scraped from
        /// </summary>
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ID of the scraper that scraped this content
        /// </summary>
        public string ScraperId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP status code from the last request
        /// </summary>
        public int LastStatusCode { get; set; }

        /// <summary>
        /// Gets or sets the content type of the response
        /// </summary>
        public string ContentType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether the URL is reachable
        /// </summary>
        public bool IsReachable { get; set; }

        /// <summary>
        /// Gets or sets the raw content of the page
        /// </summary>
        public string RawContent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the extracted text content
        /// </summary>
        public string TextContent { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the hash of the content
        /// </summary>
        public string ContentHash { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this is regulatory content
        /// </summary>
        public bool IsRegulatoryContent { get; set; }

        /// <summary>
        /// Gets or sets when this content was captured
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Gets or sets additional metadata for this content
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the folder path for versions of this content
        /// </summary>
        /// <returns>The folder path</returns>
        public string GetVersionFolder() =>
            System.IO.Path.Combine("content", ComputeUrlHash(Url));

        /// <summary>
        /// Compute a hash of the URL for folder names
        /// </summary>
        private string ComputeUrlHash(string url)
        {
            if (string.IsNullOrEmpty(url))
                return "empty_url";

            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(url));
            return Convert.ToBase64String(bytes)
                .Replace("/", "_")
                .Replace("+", "-")
                .Replace("=", "");
        }

        /// <summary>
        /// Creates a copy of this ContentItem
        /// </summary>
        public ContentItem Clone()
        {
            var clone = new ContentItem
            {
                Url = this.Url,
                Title = this.Title,
                ScraperId = this.ScraperId,
                LastStatusCode = this.LastStatusCode,
                ContentType = this.ContentType,
                IsReachable = this.IsReachable,
                RawContent = this.RawContent,
                TextContent = this.TextContent,
                ContentHash = this.ContentHash,
                IsRegulatoryContent = this.IsRegulatoryContent,
                CapturedAt = this.CapturedAt
            };

            // Copy metadata (using dictionary initializer for better performance)
            clone.Metadata = new Dictionary<string, object>(this.Metadata, StringComparer.OrdinalIgnoreCase);

            return clone;
        }

        /// <summary>
        /// Creates a ContentItem from an interface implementation
        /// </summary>
        public static ContentItem FromInterface(Interfaces.ContentItem item)
        {
            if (item == null)
                return null;

            // If it's already a ContentItem, just return it
            if (item is ContentItem contentItem)
                return contentItem;

            var result = new ContentItem
            {
                Url = item.Url ?? string.Empty,
                Title = item.Title ?? string.Empty,
                ScraperId = item.ScraperId ?? string.Empty,
                LastStatusCode = item.LastStatusCode,
                ContentType = item.ContentType ?? string.Empty,
                IsReachable = item.IsReachable,
                RawContent = item.RawContent ?? string.Empty,
                ContentHash = item.ContentHash ?? string.Empty,
                IsRegulatoryContent = item.IsRegulatoryContent,
                CapturedAt = DateTime.Now
            };

            // Handle TextContent if available through reflection
            try
            {
                var textContentProperty = item.GetType().GetProperty("TextContent");
                if (textContentProperty != null)
                {
                    result.TextContent = textContentProperty.GetValue(item) as string ?? string.Empty;
                }

                // Try to copy metadata if available
                var metadataProperty = item.GetType().GetProperty("Metadata");
                if (metadataProperty != null &&
                    metadataProperty.GetValue(item) is Dictionary<string, object> metadata)
                {
                    foreach (var kvp in metadata)
                    {
                        result.Metadata[kvp.Key] = kvp.Value;
                    }
                }
            }
            catch (Exception)
            {
                // Ignore reflection errors
            }

            return result;
        }

        /// <summary>
        /// Creates a new ContentItem with the specified URL
        /// </summary>
        public static ContentItem Create(string url) =>
            new() { Url = url };

        /// <summary>
        /// Creates a new ContentItem with the specified URL and title
        /// </summary>
        public static ContentItem Create(string url, string title) =>
            new() { Url = url, Title = title };
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
    public record ClassificationResult
    {
        /// <summary>
        /// Gets or sets the primary category of the content
        /// </summary>
        public string PrimaryCategory { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the category of the content (legacy property)
        /// </summary>
        public string Category { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the confidence level of the classification (0.0-1.0)
        /// </summary>
        public double Confidence { get; init; }

        /// <summary>
        /// Gets or sets the list of all categories with their confidence scores
        /// </summary>
        public Dictionary<string, double> CategoryScores { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets the regulatory impact of the content
        /// </summary>
        public RegulatoryImpact Impact { get; init; } = RegulatoryImpact.None;

        /// <summary>
        /// Gets or sets the list of keywords that contributed to the classification
        /// </summary>
        public List<string> MatchedKeywords { get; init; } = new();

        /// <summary>
        /// Gets or sets the timestamp of the classification
        /// </summary>
        public DateTime ClassificationTime { get; init; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the list of topics related to the content
        /// </summary>
        public List<string> Topics { get; init; } = new();

        /// <summary>
        /// Gets the primary type of the content (alias for PrimaryCategory)
        /// </summary>
        public string PrimaryType => PrimaryCategory;

        /// <summary>
        /// Creates a new ClassificationResult with the specified category and confidence
        /// </summary>
        public static ClassificationResult Create(string category, double confidence) =>
            new() {
                PrimaryCategory = category,
                Category = category,
                Confidence = confidence
            };

        /// <summary>
        /// Creates a new ClassificationResult with the specified category, confidence, and impact
        /// </summary>
        public static ClassificationResult Create(string category, double confidence, RegulatoryImpact impact) =>
            new() {
                PrimaryCategory = category,
                Category = category,
                Confidence = confidence,
                Impact = impact
            };
    }

    /// <summary>
    /// DocumentMetadata for document processing
    /// </summary>
    public record DocumentMetadata
    {
        /// <summary>
        /// Gets or sets the document URL
        /// </summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document title
        /// </summary>
        public string Title { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document author
        /// </summary>
        public string Author { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document creation date
        /// </summary>
        public DateTime? CreationDate { get; init; }

        /// <summary>
        /// Gets or sets the document last modified date
        /// </summary>
        public DateTime? LastModifiedDate { get; init; }

        /// <summary>
        /// Gets or sets the document page count
        /// </summary>
        public int PageCount { get; init; }

        /// <summary>
        /// Gets or sets the document word count
        /// </summary>
        public int WordCount { get; init; }

        /// <summary>
        /// Gets or sets the document file type
        /// </summary>
        public string FileType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public string DocumentType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document file size in bytes
        /// </summary>
        public long FileSizeBytes { get; init; }

        /// <summary>
        /// Gets or sets the document keywords
        /// </summary>
        public List<string> Keywords { get; init; } = new();

        /// <summary>
        /// Gets or sets the document subject
        /// </summary>
        public string Subject { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document categories
        /// </summary>
        public List<string> Categories { get; init; } = new();

        /// <summary>
        /// Gets or sets the document comments
        /// </summary>
        public string Comments { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document content type
        /// </summary>
        public string ContentType { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document language
        /// </summary>
        public string Language { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the date the document was processed
        /// </summary>
        public DateTime ProcessedDate { get; init; } = DateTime.Now;

        /// <summary>
        /// Gets or sets the document publication date
        /// </summary>
        public DateTime? PublicationDate { get; init; }

        /// <summary>
        /// Gets or sets the document publish date (alias for PublicationDate)
        /// </summary>
        public DateTime? PublishDate
        {
            get => PublicationDate;
            init => PublicationDate = value;
        }

        /// <summary>
        /// Gets or sets the path to the locally stored file
        /// </summary>
        public string LocalFilePath { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the content hash of the document
        /// </summary>
        public string ContentHash { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the text content of the document
        /// </summary>
        public string TextContent { get; init; } = string.Empty;

        /// <summary>
        /// Gets or sets the document classification
        /// </summary>
        public ClassificationResult Classification { get; init; }

        /// <summary>
        /// Gets or sets additional metadata extracted from the document
        /// </summary>
        public Dictionary<string, object> ExtractedMetadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets or sets additional custom metadata
        /// </summary>
        public Dictionary<string, object> CustomMetadata { get; init; } = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the file size in a human-readable format
        /// </summary>
        public string FileSizeFormatted => FormatFileSize(FileSizeBytes);

        /// <summary>
        /// Gets whether the document has been classified
        /// </summary>
        public bool IsClassified => Classification != null;

        /// <summary>
        /// Gets the age of the document in days
        /// </summary>
        public int DocumentAgeDays => CreationDate.HasValue
            ? (int)(DateTime.Now - CreationDate.Value).TotalDays
            : 0;

        /// <summary>
        /// Creates a new DocumentMetadata with the specified URL
        /// </summary>
        public static DocumentMetadata Create(string url) =>
            new() { Url = url };

        /// <summary>
        /// Creates a new DocumentMetadata with the specified URL and title
        /// </summary>
        public static DocumentMetadata Create(string url, string title) =>
            new() { Url = url, Title = title };

        /// <summary>
        /// Formats a file size in bytes to a human-readable string
        /// </summary>
        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {suffixes[suffixIndex]}";
        }
    }
}