using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents the complete history of a scraper run
    /// </summary>
    public class ScraperRunHistory
    {
        // Basic information
        public string ScraperId { get; set; } = string.Empty;
        public string ScraperName { get; set; } = string.Empty;
        public string RunId { get; set; } = string.Empty;
        
        // File path information
        public string OutputDirectory { get; set; } = string.Empty;
        public string JsonFilePath { get; set; } = string.Empty;
        
        // Timing information
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string TotalElapsedTime { get; set; } = "00:00:00";
        
        // Configuration used for this run
        public ScraperRunConfiguration Configuration { get; set; } = new ScraperRunConfiguration();
        
        // Statistics
        public int TotalUrlsProcessed { get; set; }
        public int TotalUrlsQueued { get; set; }
        public int SuccessfulUrls { get; set; }
        public int FailedUrls { get; set; }
        public long TotalBytesDownloaded { get; set; }
        public int TotalDocumentsProcessed { get; set; }
        
        // Performance metrics
        public double AverageProcessingTimeMs { get; set; }
        public double PeakMemoryUsageMb { get; set; }
        public double RequestsPerSecond { get; set; }
        
        // Status and results
        public bool HasErrors { get; set; }
        public string FinalStatus { get; set; } = "Completed";
        
        // Detailed tracking of URLs
        public List<ProcessedUrlInfo> ProcessedUrls { get; set; } = new List<ProcessedUrlInfo>();
        
        // Log entries
        public List<LogEntry> LogEntries { get; set; } = new List<LogEntry>();
        
        // List of errors encountered
        public List<ErrorInfo> Errors { get; set; } = new List<ErrorInfo>();
    }
    
    /// <summary>
    /// Information about a processed URL
    /// </summary>
    public class ProcessedUrlInfo
    {
        public string Url { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public bool Success { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public int ContentSizeBytes { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Configuration used for a scraper run
    /// </summary>
    public class ScraperRunConfiguration
    {
        public string StartUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int MaxDepth { get; set; }
        public int MaxPages { get; set; }
        public int MaxConcurrentRequests { get; set; }
        public int DelayBetweenRequests { get; set; }
        public bool FollowLinks { get; set; }
        public bool FollowExternalLinks { get; set; }
        public bool RespectRobotsTxt { get; set; }
        public List<string> ContentExtractorSelectors { get; set; } = new List<string>();
        public List<string> ContentExtractorExcludeSelectors { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Information about an error
    /// </summary>
    public class ErrorInfo
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RelatedUrl { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
    }
}