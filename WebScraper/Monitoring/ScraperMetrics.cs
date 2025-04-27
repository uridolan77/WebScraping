using System;
using System.Collections.Generic;

namespace WebScraper.Monitoring
{
    /// <summary>
    /// Class to track and manage metrics for web scraping operations
    /// </summary>
    public class ScraperMetrics
    {
        /// <summary>
        /// When the scraper first ran
        /// </summary>
        public DateTime FirstRunTime { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// When the scraper last ran
        /// </summary>
        public DateTime LastRunTime { get; set; } = DateTime.MinValue;
        
        /// <summary>
        /// Total number of scraping runs
        /// </summary>
        public int TotalRuns { get; set; }
        
        /// <summary>
        /// Number of failed scraping runs
        /// </summary>
        public int FailedRuns { get; set; }
        
        /// <summary>
        /// Number of URLs successfully processed
        /// </summary>
        public int ProcessedUrls { get; set; }
        
        /// <summary>
        /// Number of URLs that failed processing
        /// </summary>
        public int FailedUrls { get; set; }
        
        /// <summary>
        /// Number of bytes downloaded
        /// </summary>
        public long BytesDownloaded { get; set; }
        
        /// <summary>
        /// Number of documents processed
        /// </summary>
        public int DocumentsProcessed { get; set; }
        
        /// <summary>
        /// Time spent processing in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Dictionary of custom metrics
        /// </summary>
        public Dictionary<string, double> CustomMetrics { get; } = new Dictionary<string, double>();
        
        /// <summary>
        /// Session metrics
        /// </summary>
        public DateTime SessionStartTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Duration of the last session in ms
        /// </summary>
        public long LastSessionDurationMs { get; set; }
        
        /// <summary>
        /// Total scraping time in ms
        /// </summary>
        public long TotalScrapingTimeMs { get; set; }
        
        /// <summary>
        /// Total number of URLs that were successfully processed
        /// </summary>
        public int SuccessfulUrls { get; set; }
        
        /// <summary>
        /// Count of pages processed in this session
        /// </summary>
        public int PagesProcessed { get; set; }
        
        /// <summary>
        /// Total number of links extracted
        /// </summary>
        public int TotalLinksExtracted { get; set; }
        
        /// <summary>
        /// Number of content items extracted
        /// </summary>
        public int ContentItemsExtracted { get; set; }
        
        /// <summary>
        /// Total time spent on page processing in ms
        /// </summary>
        public long TotalPageProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Total time spent on document processing in ms
        /// </summary>
        public long TotalDocumentProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Total size of all documents in bytes
        /// </summary>
        public long TotalDocumentSizeBytes { get; set; }
        
        /// <summary>
        /// Client errors (4xx)
        /// </summary>
        public int ClientErrors { get; set; }
        
        /// <summary>
        /// Server errors (5xx)
        /// </summary>
        public int ServerErrors { get; set; }
        
        /// <summary>
        /// Rate limit errors
        /// </summary>
        public int RateLimitErrors { get; set; }
        
        /// <summary>
        /// Network errors
        /// </summary>
        public int NetworkErrors { get; set; }
        
        /// <summary>
        /// Timeout errors
        /// </summary>
        public int TimeoutErrors { get; set; }
        
        /// <summary>
        /// Number of pending requests
        /// </summary>
        public int PendingRequests { get; set; }
        
        /// <summary>
        /// Total bytes downloaded
        /// </summary>
        public long TotalBytesDownloaded { get; set; }
        
        /// <summary>
        /// Current memory usage in MB
        /// </summary>
        public double CurrentMemoryUsageMB { get; set; }
        
        /// <summary>
        /// Peak memory usage in MB
        /// </summary>
        public double PeakMemoryUsageMB { get; set; }
        
        /// <summary>
        /// Average page processing time in ms
        /// </summary>
        public double AveragePageProcessingTimeMs => PagesProcessed > 0 ? 
            (double)TotalPageProcessingTimeMs / PagesProcessed : 0;
            
        /// <summary>
        /// Average links per page
        /// </summary>
        public double AverageLinksPerPage => PagesProcessed > 0 ? 
            (double)TotalLinksExtracted / PagesProcessed : 0;
        
        /// <summary>
        /// Dictionary of per-domain metrics
        /// </summary>
        public Dictionary<string, DomainMetrics> DomainMetrics { get; } = new Dictionary<string, DomainMetrics>();
        
        /// <summary>
        /// Dictionary of document type metrics
        /// </summary>
        public Dictionary<string, DocumentTypeMetrics> DocumentTypeMetrics { get; } = new Dictionary<string, DocumentTypeMetrics>();
        
        /// <summary>
        /// Average processing time per URL in milliseconds
        /// </summary>
        public double AverageProcessingTimePerUrlMs => ProcessedUrls > 0 ? 
            (double)ProcessingTimeMs / ProcessedUrls : 0;
        
        /// <summary>
        /// Success rate (0.0 to 1.0)
        /// </summary>
        public double SuccessRate => (ProcessedUrls + FailedUrls) > 0 ? 
            (double)ProcessedUrls / (ProcessedUrls + FailedUrls) : 0;
        
        /// <summary>
        /// Reset metrics to their initial state
        /// </summary>
        public void Reset()
        {
            ProcessedUrls = 0;
            FailedUrls = 0;
            BytesDownloaded = 0;
            DocumentsProcessed = 0;
            ProcessingTimeMs = 0;
            CustomMetrics.Clear();
            SuccessfulUrls = 0;
            ClientErrors = 0;
            ServerErrors = 0;
            RateLimitErrors = 0;
            NetworkErrors = 0;
            TimeoutErrors = 0;
            PendingRequests = 0;
            TotalBytesDownloaded = 0;
            TotalLinksExtracted = 0;
            ContentItemsExtracted = 0;
            TotalPageProcessingTimeMs = 0;
            TotalDocumentProcessingTimeMs = 0;
            TotalDocumentSizeBytes = 0;
            PagesProcessed = 0;
            DomainMetrics.Clear();
            DocumentTypeMetrics.Clear();
        }
        
        /// <summary>
        /// Reset session-specific metrics
        /// </summary>
        public void ResetSessionMetrics()
        {
            SessionStartTime = DateTime.Now;
            LastSessionDurationMs = 0;
            PagesProcessed = 0;
            SuccessfulUrls = 0;
            ClientErrors = 0;
            ServerErrors = 0;
            RateLimitErrors = 0;
            NetworkErrors = 0;
            TimeoutErrors = 0;
            PendingRequests = 0;
            TotalLinksExtracted = 0;
            ContentItemsExtracted = 0;
            TotalPageProcessingTimeMs = 0;
            CurrentMemoryUsageMB = 0;
        }
        
        /// <summary>
        /// Increment a custom metric by the specified amount
        /// </summary>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value to increment by</param>
        public void IncrementMetric(string name, double value = 1)
        {
            if (CustomMetrics.ContainsKey(name))
            {
                CustomMetrics[name] += value;
            }
            else
            {
                CustomMetrics[name] = value;
            }
        }
        
        /// <summary>
        /// Set a custom metric to the specified value
        /// </summary>
        /// <param name="name">Name of the metric</param>
        /// <param name="value">Value to set</param>
        public void SetMetric(string name, double value)
        {
            CustomMetrics[name] = value;
        }
        
        /// <summary>
        /// Get a string representation of the metrics
        /// </summary>
        public override string ToString()
        {
            return $"Metrics: URLs(Success={ProcessedUrls}, Failed={FailedUrls}), " +
                   $"Runs={TotalRuns}, SuccessRate={SuccessRate:P2}, " +
                   $"Downloaded={FormatBytes(BytesDownloaded)}";
        }
        
        /// <summary>
        /// Format bytes to a human-readable string
        /// </summary>
        private string FormatBytes(long bytes)
        {
            string[] suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblBytes = bytes;
            
            for (i = 0; i < suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblBytes = bytes / 1024.0;
            }
            
            return $"{dblBytes:0.##} {suffix[i]}";
        }
    }
    
    /// <summary>
    /// Metrics for a specific domain
    /// </summary>
    public class DomainMetrics
    {
        /// <summary>
        /// Domain name
        /// </summary>
        public string Domain { get; set; }
        
        /// <summary>
        /// Number of successful requests
        /// </summary>
        public int SuccessfulRequests { get; set; }
        
        /// <summary>
        /// Number of failed requests
        /// </summary>
        public int FailedRequests { get; set; }
        
        /// <summary>
        /// Total bytes downloaded
        /// </summary>
        public long BytesDownloaded { get; set; }
        
        /// <summary>
        /// Average response time in ms
        /// </summary>
        public double AverageResponseTimeMs { get; set; }
        
        /// <summary>
        /// Number of rate limiting errors
        /// </summary>
        public int RateLimitErrors { get; set; }
        
        /// <summary>
        /// Last successful request time
        /// </summary>
        public DateTime LastSuccessfulRequest { get; set; }
        
        /// <summary>
        /// Total number of requests (successful + failed)
        /// </summary>
        public int RequestCount => SuccessfulRequests + FailedRequests;
        
        /// <summary>
        /// Total response time in milliseconds
        /// </summary>
        public long TotalResponseTimeMs { get; set; }
        
        /// <summary>
        /// Total bytes downloaded
        /// </summary>
        public long TotalBytesDownloaded { get; set; }
        
        /// <summary>
        /// Number of client errors (4xx)
        /// </summary>
        public int ClientErrors { get; set; }
        
        /// <summary>
        /// Number of server errors (5xx)
        /// </summary>
        public int ServerErrors { get; set; }
        
        /// <summary>
        /// Number of rate limit hits (429)
        /// </summary>
        public int RateLimitHits { get; set; }
        
        /// <summary>
        /// Number of timeout errors
        /// </summary>
        public int Timeouts { get; set; }
        
        /// <summary>
        /// Number of network errors
        /// </summary>
        public int NetworkErrors { get; set; }
    }
    
    /// <summary>
    /// Metrics for a specific document type
    /// </summary>
    public class DocumentTypeMetrics
    {
        /// <summary>
        /// Document type
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Number of documents processed
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// Total size in bytes
        /// </summary>
        public long TotalSizeBytes { get; set; }
        
        /// <summary>
        /// Total processing time in ms
        /// </summary>
        public long TotalProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Number of processing errors
        /// </summary>
        public int ProcessingErrors { get; set; }
    }
}