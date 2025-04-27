using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents rules for extracting content from web pages
    /// </summary>
    public class ContentExtractionRules
    {
        /// <summary>
        /// CSS selectors to include in content extraction
        /// </summary>
        public List<string> IncludeSelectors { get; set; } = new List<string>();
        
        /// <summary>
        /// CSS selectors to exclude from content extraction
        /// </summary>
        public List<string> ExcludeSelectors { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether to extract metadata
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;
        
        /// <summary>
        /// Whether to extract structured data
        /// </summary>
        public bool ExtractStructuredData { get; set; } = false;
        
        /// <summary>
        /// Custom JavaScript extractor code
        /// </summary>
        public string CustomJsExtractor { get; set; }
    }

    /// <summary>
    /// Represents regulatory monitoring configuration
    /// </summary>
    public class RegulatoryConfigModel
    {
        /// <summary>
        /// Whether regulatory content analysis is enabled
        /// </summary>
        public bool EnableRegulatoryContentAnalysis { get; set; } = false;
        
        /// <summary>
        /// Whether to track regulatory changes
        /// </summary>
        public bool TrackRegulatoryChanges { get; set; } = false;
        
        /// <summary>
        /// Whether to classify regulatory documents
        /// </summary>
        public bool ClassifyRegulatoryDocuments { get; set; } = false;
        
        /// <summary>
        /// Whether to extract structured content from documents
        /// </summary>
        public bool ExtractStructuredContent { get; set; } = false;
        
        /// <summary>
        /// Whether to process PDF documents
        /// </summary>
        public bool ProcessPdfDocuments { get; set; } = false;
        
        /// <summary>
        /// Whether to monitor high impact changes
        /// </summary>
        public bool MonitorHighImpactChanges { get; set; } = false;
        
        /// <summary>
        /// Whether this is a UKGC website
        /// </summary>
        public bool IsUKGCWebsite { get; set; } = false;
        
        /// <summary>
        /// List of keywords to alert on
        /// </summary>
        public List<string> KeywordAlertList { get; set; } = new List<string>();
        
        /// <summary>
        /// Notification endpoint for regulatory alerts
        /// </summary>
        public string NotificationEndpoint { get; set; }
    }

    /// <summary>
    /// Represents webhook configuration for a scraper
    /// </summary>
    public class WebhookConfig
    {
        /// <summary>
        /// Whether webhooks are enabled
        /// </summary>
        public bool Enabled { get; set; } = false;
        
        /// <summary>
        /// URL to send webhooks to
        /// </summary>
        [Url]
        public string WebhookUrl { get; set; }
        
        /// <summary>
        /// Whether to notify on content changes
        /// </summary>
        public bool NotifyOnContentChanges { get; set; } = true;
        
        /// <summary>
        /// Whether to notify when a document is processed
        /// </summary>
        public bool NotifyOnDocumentProcessed { get; set; } = false;
        
        /// <summary>
        /// Whether to notify on scraper status changes
        /// </summary>
        public bool NotifyOnScraperStatusChange { get; set; } = true;
    }

    /// <summary>
    /// Options for exporting scraped data
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Format to export data in (json, csv)
        /// </summary>
        [Required]
        public string Format { get; set; } = "json";
        
        /// <summary>
        /// Path to output the exported data
        /// </summary>
        public string OutputPath { get; set; }
        
        /// <summary>
        /// Whether to include raw HTML in the export
        /// </summary>
        public bool IncludeRawHtml { get; set; } = false;
        
        /// <summary>
        /// Whether to include processed content in the export
        /// </summary>
        public bool IncludeProcessedContent { get; set; } = true;
        
        /// <summary>
        /// Whether to include metadata in the export
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;
        
        /// <summary>
        /// Start date for filtering data to export
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// End date for filtering data to export
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

    /// <summary>
    /// Options for scheduling scrapers
    /// </summary>
    public class ScheduleOptions
    {
        /// <summary>
        /// Name of the schedule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Description of the schedule
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Whether this is a recurring schedule
        /// </summary>
        public bool IsRecurring { get; set; } = false;
        
        /// <summary>
        /// Cron expression for recurring schedules
        /// </summary>
        public string CronExpression { get; set; }
        
        /// <summary>
        /// Execution date for one-time schedules
        /// </summary>
        public DateTime? OneTimeExecutionDate { get; set; }
        
        /// <summary>
        /// Expiry date for recurring schedules
        /// </summary>
        public DateTime? ExpiryDate { get; set; }
    }

    /// <summary>
    /// Configuration for rate limiting
    /// </summary>
    public class RateLimitingConfig
    {
        /// <summary>
        /// Whether to use adaptive rate limiting
        /// </summary>
        public bool EnableAdaptiveRateLimiting { get; set; } = true;
        
        /// <summary>
        /// Maximum requests per minute
        /// </summary>
        public int MaxRequestsPerMinute { get; set; } = 60;
        
        /// <summary>
        /// Minimum delay between requests in milliseconds
        /// </summary>
        public int MinDelayBetweenRequests { get; set; } = 500;
        
        /// <summary>
        /// Maximum delay between requests in milliseconds
        /// </summary>
        public int MaxDelayBetweenRequests { get; set; } = 5000;
        
        /// <summary>
        /// Whether to respect robots.txt
        /// </summary>
        public bool RespectRobotsTxt { get; set; } = true;
        
        /// <summary>
        /// User agent string
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 WebScraper Bot";
        
        /// <summary>
        /// Whether to back off on errors
        /// </summary>
        public bool BackOffOnErrors { get; set; } = true;
        
        /// <summary>
        /// Domain-specific rate limits
        /// </summary>
        public Dictionary<string, DomainRateLimit> DomainRateLimits { get; set; } = new Dictionary<string, DomainRateLimit>();
    }

    /// <summary>
    /// Rate limiting configuration for a specific domain
    /// </summary>
    public class DomainRateLimit
    {
        /// <summary>
        /// Maximum requests per minute for this domain
        /// </summary>
        public int MaxRequestsPerMinute { get; set; } = 30;
        
        /// <summary>
        /// Minimum delay between requests in milliseconds for this domain
        /// </summary>
        public int MinDelayBetweenRequests { get; set; } = 1000;
    }

    /// <summary>
    /// Information about a proxy server
    /// </summary>
    public class ProxyInfo
    {
        /// <summary>
        /// Host address of the proxy
        /// </summary>
        [Required]
        public string Host { get; set; }
        
        /// <summary>
        /// Port number
        /// </summary>
        [Required]
        public int Port { get; set; }
        
        /// <summary>
        /// Username for authentication
        /// </summary>
        public string Username { get; set; }
        
        /// <summary>
        /// Password for authentication
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Protocol (http, https, socks5)
        /// </summary>
        public string Protocol { get; set; } = "http";
        
        /// <summary>
        /// Country code for geo-targeting
        /// </summary>
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// Proxy configuration for a scraper
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// Whether to use proxies
        /// </summary>
        public bool UseProxies { get; set; } = false;
        
        /// <summary>
        /// Rotation strategy (RoundRobin, Random, Sequential)
        /// </summary>
        public string RotationStrategy { get; set; } = "RoundRobin";
        
        /// <summary>
        /// Whether to test proxies before using them
        /// </summary>
        public bool TestProxiesBeforeUse { get; set; } = true;
        
        /// <summary>
        /// Maximum failures before removing a proxy
        /// </summary>
        public int MaxFailuresBeforeRemoval { get; set; } = 3;
        
        /// <summary>
        /// List of proxy servers
        /// </summary>
        public List<ProxyInfo> Proxies { get; set; } = new List<ProxyInfo>();
    }

    /// <summary>
    /// Options for test running a scraper
    /// </summary>
    public class TestRunOptions
    {
        /// <summary>
        /// URLs to test
        /// </summary>
        public List<string> TestUrls { get; set; } = new List<string>();
        
        /// <summary>
        /// Maximum depth for the test run
        /// </summary>
        public int MaxDepth { get; set; } = 2;
        
        /// <summary>
        /// Maximum pages for the test run
        /// </summary>
        public int MaxPages { get; set; } = 10;
        
        /// <summary>
        /// Whether to follow links during the test
        /// </summary>
        public bool FollowLinks { get; set; } = true;
        
        /// <summary>
        /// Whether to validate extraction rules
        /// </summary>
        public bool ValidateExtractionRules { get; set; } = true;
    }

    /// <summary>
    /// Request for a batch operation on multiple scrapers
    /// </summary>
    public class BatchOperationRequest
    {
        /// <summary>
        /// Operation to perform (start, stop, delete, compress)
        /// </summary>
        [Required]
        public string Operation { get; set; }
        
        /// <summary>
        /// IDs of scrapers to operate on
        /// </summary>
        [Required]
        public List<string> ScraperIds { get; set; } = new List<string>();
    }
}