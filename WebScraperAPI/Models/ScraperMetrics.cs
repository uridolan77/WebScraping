using System;
using System.Collections.Generic;

namespace WebScraperApi.Models
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
        /// When the metrics were last updated
        /// </summary>
        public DateTime LastMetricsUpdate { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Total number of scraping runs
        /// </summary>
        public int TotalRuns { get; set; }
        
        /// <summary>
        /// Number of failed scraping runs
        /// </summary>
        public int FailedRuns { get; set; }
        
        /// <summary>
        /// Number of times the scraper has been executed
        /// </summary>
        public int ExecutionCount { get; set; }
        
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
        /// Number of documents crawled
        /// </summary>
        public int DocumentsCrawled { get; set; }
        
        /// <summary>
        /// Time spent processing in milliseconds
        /// </summary>
        public long ProcessingTimeMs { get; set; }
        
        /// <summary>
        /// Total time spent crawling in milliseconds
        /// </summary>
        public long TotalCrawlTimeMs { get; set; }
        
        /// <summary>
        /// Dictionary of custom metrics
        /// </summary>
        public Dictionary<string, double> CustomMetrics { get; set; } = new Dictionary<string, double>();
        
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
        /// Total number of successful requests
        /// </summary>
        public int SuccessfulRequests { get; set; }
        
        /// <summary>
        /// Total number of failed requests
        /// </summary>
        public int FailedRequests { get; set; }
        
        /// <summary>
        /// Total number of requests
        /// </summary>
        public int TotalRequests => SuccessfulRequests + FailedRequests;
        
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
        /// Number of content changes detected
        /// </summary>
        public int ContentChangesDetected { get; set; }
        
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
        /// Number of rate limits encountered
        /// </summary>
        public int RateLimitsEncountered { get; set; }
        
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
        /// Average response time in ms
        /// </summary>
        public double AverageResponseTimeMs => TotalRequests > 0 ? 
            (double)TotalCrawlTimeMs / TotalRequests : 0;
        
        /// <summary>
        /// Average processing time per URL in milliseconds
        /// </summary>
        public double AverageProcessingTimePerUrlMs => ProcessedUrls > 0 ? 
            (double)ProcessingTimeMs / ProcessedUrls : 0;
        
        /// <summary>
        /// Constructor with default values
        /// </summary>
        public ScraperMetrics()
        {
        }
        
        /// <summary>
        /// Constructor with scraper ID and name
        /// </summary>
        public ScraperMetrics(string scraperId, string scraperName)
        {
            ScraperId = scraperId;
            ScraperName = scraperName;
        }
        
        /// <summary>
        /// ID of the scraper
        /// </summary>
        public string ScraperId { get; set; } = string.Empty;
        
        /// <summary>
        /// Name of the scraper
        /// </summary>
        public string ScraperName { get; set; } = string.Empty;
        
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
            SuccessfulRequests = 0;
            FailedRequests = 0;
            ClientErrors = 0;
            ServerErrors = 0;
            RateLimitErrors = 0;
            RateLimitsEncountered = 0;
            NetworkErrors = 0;
            TimeoutErrors = 0;
            PendingRequests = 0;
            TotalBytesDownloaded = 0;
            TotalLinksExtracted = 0;
            ContentItemsExtracted = 0;
            ContentChangesDetected = 0;
            TotalPageProcessingTimeMs = 0;
            TotalDocumentProcessingTimeMs = 0;
            TotalDocumentSizeBytes = 0;
            PagesProcessed = 0;
            DocumentsCrawled = 0;
            TotalCrawlTimeMs = 0;
            LastMetricsUpdate = DateTime.Now;
        }
    }
}
