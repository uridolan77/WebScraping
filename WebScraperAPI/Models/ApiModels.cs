using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Model for content extraction rules configuration
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
        /// Whether to extract metadata from the page
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;

        /// <summary>
        /// Whether to extract structured data (JSON-LD, microdata, etc.)
        /// </summary>
        public bool ExtractStructuredData { get; set; } = false;

        /// <summary>
        /// Custom JavaScript code to extract content
        /// </summary>
        public string CustomJsExtractor { get; set; }
    }

    /// <summary>
    /// Model for regulatory-specific configuration
    /// </summary>
    public class RegulatoryConfigModel
    {
        /// <summary>
        /// Whether to enable regulatory content analysis
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
        /// Whether to extract structured content
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
        /// Whether the site is a UK Gambling Commission website
        /// </summary>
        public bool IsUKGCWebsite { get; set; } = false;

        /// <summary>
        /// Keywords to alert on when found in content
        /// </summary>
        public List<string> KeywordAlertList { get; set; } = new List<string>();

        /// <summary>
        /// Endpoint to notify for regulatory changes
        /// </summary>
        public string NotificationEndpoint { get; set; }
    }

    /// <summary>
    /// Model for data export options
    /// </summary>
    public class ExportOptions
    {
        /// <summary>
        /// Format to export data in (json, csv)
        /// </summary>
        [Required]
        public string Format { get; set; } = "json";

        /// <summary>
        /// Starting date for filtered export
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Ending date for filtered export
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Whether to include raw HTML in export
        /// </summary>
        public bool IncludeRawHtml { get; set; } = false;

        /// <summary>
        /// Whether to include processed content in export
        /// </summary>
        public bool IncludeProcessedContent { get; set; } = true;

        /// <summary>
        /// Whether to include metadata in export
        /// </summary>
        public bool IncludeMetadata { get; set; } = true;

        /// <summary>
        /// Output directory path for export
        /// </summary>
        public string OutputPath { get; set; }
    }

    /// <summary>
    /// Model for monitoring settings
    /// </summary>
    public class MonitoringSettings
    {
        /// <summary>
        /// Whether monitoring is enabled
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Interval in minutes between monitoring checks
        /// </summary>
        public int IntervalMinutes { get; set; } = 1440; // 24 hours by default

        /// <summary>
        /// Whether to notify on changes
        /// </summary>
        public bool NotifyOnChanges { get; set; } = false;

        /// <summary>
        /// Email to notify on changes
        /// </summary>
        public string NotificationEmail { get; set; }

        /// <summary>
        /// Whether to track content version history
        /// </summary>
        public bool TrackChangesHistory { get; set; } = true;
    }

    /// <summary>
    /// Configuration for webhook notifications
    /// </summary>
    public class WebhookConfig
    {
        /// <summary>
        /// Gets or sets whether webhook notifications are enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the webhook URL to send notifications to
        /// </summary>
        public string WebhookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the format of webhook notifications (json, form)
        /// </summary>
        public string Format { get; set; } = "json";

        /// <summary>
        /// Gets or sets the triggers for webhook notifications
        /// </summary>
        public string[] Triggers { get; set; } = new string[] { "all" };

        /// <summary>
        /// Gets or sets whether to notify on content changes
        /// </summary>
        public bool NotifyOnContentChanges { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to notify when documents are processed
        /// </summary>
        public bool NotifyOnDocumentProcessed { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to notify on scraper status changes
        /// </summary>
        public bool NotifyOnScraperStatusChange { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to send a test notification
        /// </summary>
        public bool SendTestNotification { get; set; } = false;
    }

    /// <summary>
    /// Options for scheduling a scraper job
    /// </summary>
    public class ScheduleOptions
    {
        /// <summary>
        /// One-time execution date, if not recurring
        /// </summary>
        public DateTime? OneTimeExecutionDate { get; set; }

        /// <summary>
        /// Whether this is a recurring schedule
        /// </summary>
        public bool IsRecurring { get; set; }

        /// <summary>
        /// Cron expression for recurring schedules
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// Human-readable description of the schedule
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional expiry date for recurring schedules
        /// </summary>
        public DateTime? ExpiryDate { get; set; }

        /// <summary>
        /// Name for the schedule
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Alias for Name to maintain backward compatibility
        /// </summary>
        public string ScheduleName
        {
            get => Name;
            set => Name = value;
        }
    }

    /// <summary>
    /// Rate limiting configuration for a scraper
    /// </summary>
    public class RateLimitingConfig
    {
        /// <summary>
        /// Whether adaptive rate limiting is enabled
        /// </summary>
        public bool EnableAdaptiveRateLimiting { get; set; } = true;

        /// <summary>
        /// Maximum requests per minute
        /// </summary>
        [Range(1, 1000)]
        public int MaxRequestsPerMinute { get; set; } = 60;

        /// <summary>
        /// Minimum delay between requests in milliseconds
        /// </summary>
        [Range(0, 60000)]
        public int MinDelayBetweenRequests { get; set; } = 1000;

        /// <summary>
        /// Maximum delay between requests in milliseconds
        /// </summary>
        [Range(0, 60000)]
        public int MaxDelayBetweenRequests { get; set; } = 5000;

        /// <summary>
        /// Whether to respect robots.txt rules
        /// </summary>
        public bool RespectRobotsTxt { get; set; } = true;

        /// <summary>
        /// User-agent string to use
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Whether to back off on HTTP errors
        /// </summary>
        public bool BackOffOnErrors { get; set; } = true;

        /// <summary>
        /// Domain-specific rate limits
        /// </summary>
        public Dictionary<string, DomainRateLimit> DomainRateLimits { get; set; } = new Dictionary<string, DomainRateLimit>();
    }

    /// <summary>
    /// Domain-specific rate limit
    /// </summary>
    public class DomainRateLimit
    {
        /// <summary>
        /// Maximum requests per minute for this domain
        /// </summary>
        public int MaxRequestsPerMinute { get; set; }

        /// <summary>
        /// Minimum delay between requests in milliseconds for this domain
        /// </summary>
        public int MinDelayBetweenRequests { get; set; }
    }

    /// <summary>
    /// Proxy configuration
    /// </summary>
    public class ProxyConfig
    {
        /// <summary>
        /// Whether to use proxies
        /// </summary>
        public bool UseProxies { get; set; } = false;

        /// <summary>
        /// Type of proxy rotation strategy
        /// </summary>
        public string RotationStrategy { get; set; } = "RoundRobin";

        /// <summary>
        /// List of proxies to use
        /// </summary>
        public List<ProxyInfo> Proxies { get; set; } = new List<ProxyInfo>();

        /// <summary>
        /// Whether to test proxies before using them
        /// </summary>
        public bool TestProxiesBeforeUse { get; set; } = true;

        /// <summary>
        /// Maximum failures before removing a proxy
        /// </summary>
        public int MaxFailuresBeforeRemoval { get; set; } = 3;
    }

    /// <summary>
    /// Information about a proxy
    /// </summary>
    public class ProxyInfo
    {
        /// <summary>
        /// Host address of the proxy
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Port of the proxy
        /// </summary>
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
        /// Protocol of the proxy (http, https, socks5)
        /// </summary>
        public string Protocol { get; set; } = "http";

        /// <summary>
        /// Country code of the proxy
        /// </summary>
        public string CountryCode { get; set; }
    }

    /// <summary>
    /// Options for running a test on a scraper
    /// </summary>
    public class TestRunOptions
    {
        /// <summary>
        /// List of URLs to test
        /// </summary>
        public List<string> TestUrls { get; set; } = new List<string>();

        /// <summary>
        /// Maximum number of pages to process
        /// </summary>
        public int MaxPages { get; set; } = 5;

        /// <summary>
        /// Maximum depth to crawl
        /// </summary>
        public int MaxDepth { get; set; } = 2;

        /// <summary>
        /// Whether to follow links during test
        /// </summary>
        public bool FollowLinks { get; set; } = false;

        /// <summary>
        /// Whether to validate extraction rules
        /// </summary>
        public bool ValidateExtractionRules { get; set; } = true;
    }

    /// <summary>
    /// Request for performing batch operations on multiple scrapers
    /// </summary>
    public class BatchOperationRequest
    {
        /// <summary>
        /// Type of operation to perform
        /// </summary>
        [Required]
        public string Operation { get; set; }

        /// <summary>
        /// List of scraper IDs to operate on
        /// </summary>
        [Required]
        public List<string> ScraperIds { get; set; } = new List<string>();

        /// <summary>
        /// Optional operation-specific parameters
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}