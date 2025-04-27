using System;
using System.Collections.Generic;

namespace WebScraper
{
    public class ScrapedPage
    {
        public string Url { get; set; }
        public DateTime ScrapedDateTime { get; set; }
        public string TextContent { get; set; }
        public int Depth { get; set; }
    }

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

    // Processing pipeline result model
    public class ProcessingResult
    {
        public string Url { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }

    // Headless browser configuration
    public enum BrowserType
    {
        Chromium,
        Firefox,
        WebKit
    }

    public class HeadlessBrowserOptions
    {
        public bool Headless { get; set; } = true;
        public string ScreenshotDirectory { get; set; }
        public BrowserType BrowserType { get; set; } = BrowserType.Chromium;
        public bool TakeScreenshots { get; set; }
        public string UserAgent { get; set; }
        public bool JavaScriptEnabled { get; set; } = true;
        public int NavigationTimeout { get; set; } = 30000;
        public int WaitTimeout { get; set; } = 10000;
    }

    public enum NavigationWaitUntil
    {
        Load,
        DOMContentLoaded,
        NetworkIdle,
        None
    }

    // State management models
    public class ContentItem
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string ScraperId { get; set; }
        public int LastStatusCode { get; set; }
        public string ContentType { get; set; }
        public bool IsReachable { get; set; }
        public string RawContent { get; set; }
        public string ContentHash { get; set; }
        public bool IsRegulatoryContent { get; set; }
    }

    public class ScraperState
    {
        public string ScraperId { get; set; }
        public string Status { get; set; }
        public DateTime LastRunStartTime { get; set; }
        public DateTime? LastRunEndTime { get; set; }
        public DateTime? LastSuccessfulRunTime { get; set; }
        public string ProgressData { get; set; }
        public string ConfigSnapshot { get; set; }
    }

    // Regulatory impact enum
    public enum RegulatoryImpact
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }

    // Model classes for regulatory framework interfaces
    public class PageVersion
    {
        public string Url { get; set; }
        public string ContentHash { get; set; }
        public DateTime VersionDate { get; set; }
        public string Content { get; set; }
        public string TextContent { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class ChangeAnalysisResult
    {
        public bool HasChanges { get; set; }
        public double ChangePercentage { get; set; }
        public List<string> AddedContent { get; set; } = new List<string>();
        public List<string> RemovedContent { get; set; } = new List<string>();
        public List<string> ModifiedContent { get; set; } = new List<string>();
    }

    public class ClassificationResult
    {
        public bool IsRegulatoryContent { get; set; }
        public double ConfidenceScore { get; set; }
        public string Category { get; set; }
        public RegulatoryImpact Impact { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    public class ContentNode
    {
        public string NodeType { get; set; }
        public string Content { get; set; }
        public int Depth { get; set; }
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        public double RelevanceScore { get; set; }
    }

    public class DocumentMetadata
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string DocumentType { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Author { get; set; }
        public int PageCount { get; set; }
        public string TextContent { get; set; }
        public string ContentHash { get; set; }
        public ClassificationResult Classification { get; set; }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public bool CanRunWithWarnings { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
    }

    public class PipelineStatus
    {
        public bool IsRunning { get; set; }
        public int TotalItems { get; set; }
        public int ProcessedItems { get; set; } = 0;
        public int FailedItems { get; set; } = 0;
        public string CurrentOperation { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan ElapsedTime => StartTime.HasValue 
            ? (EndTime ?? DateTime.UtcNow) - StartTime.Value 
            : TimeSpan.Zero;
    }
}