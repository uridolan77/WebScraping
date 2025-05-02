using System;
using System.Collections.Generic;
using System.Linq;

namespace WebScraper
{
    /// <summary>
    /// Configuration for a web scraper
    /// </summary>
    public class ScraperConfig
    {
        // Multi-scraper support
        /// <summary>
        /// Gets or sets the unique identifier for this scraper
        /// </summary>
        public string ScraperId { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Gets or sets the human-readable name for this scraper
        /// </summary>
        public string ScraperName { get; set; } = "Default Scraper";

        /// <summary>
        /// Gets or sets the name (legacy property)
        /// </summary>
        public string Name
        {
            get => ScraperName;
            set => ScraperName = value;
        }

        /// <summary>
        /// Gets or sets the type of scraper
        /// </summary>
        public string ScraperType { get; set; } = "Standard";

        // Creation metadata
        /// <summary>
        /// Gets or sets when this scraper configuration was created
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Basic configuration
        /// <summary>
        /// Gets or sets the starting URL for the scraper
        /// </summary>
        public string StartUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the base URL for relative URL resolution
        /// </summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the directory where output will be stored
        /// </summary>
        public string OutputDirectory { get; set; } = "output";

        /// <summary>
        /// Gets or sets the delay between requests in milliseconds
        /// </summary>
        public int DelayBetweenRequests { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the maximum number of concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 4;

        /// <summary>
        /// Gets or sets the maximum depth to crawl
        /// </summary>
        public int MaxDepth { get; set; } = 3;

        /// <summary>
        /// Gets or sets the maximum crawl depth (alias for MaxDepth)
        /// </summary>
        public int MaxCrawlDepth
        {
            get => MaxDepth;
            set => MaxDepth = value;
        }

        /// <summary>
        /// Gets or sets whether to follow external links
        /// </summary>
        public bool FollowExternalLinks { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to respect robots.txt
        /// </summary>
        public bool RespectRobotsTxt { get; set; } = true;

        /// <summary>
        /// Gets or sets the request timeout in seconds
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 30;

        /// <summary>
        /// Gets or sets the User-Agent string to use for requests
        /// </summary>
        public string UserAgent { get; set; } = "Mozilla/5.0 WebScraper/1.0";

        /// <summary>
        /// Gets or sets whether to append to existing data
        /// </summary>
        public bool AppendToExistingData { get; set; } = false;

        // Browser automation
        /// <summary>
        /// Gets or sets whether to process JavaScript-heavy pages
        /// </summary>
        public bool ProcessJsHeavyPages { get; set; } = false;

        /// <summary>
        /// Gets or sets the browser timeout in milliseconds
        /// </summary>
        public int BrowserTimeout { get; set; } = 60000;

        /// <summary>
        /// Gets or sets whether to use a headless browser
        /// </summary>
        public bool UseHeadlessBrowser { get; set; } = true;

        // Header/footer pattern learning
        /// <summary>
        /// Gets or sets whether to automatically learn header/footer patterns
        /// </summary>
        public bool AutoLearnHeaderFooter { get; set; } = true;

        /// <summary>
        /// Gets or sets the number of pages to use for learning
        /// </summary>
        public int LearningPagesCount { get; set; } = 5;

        // Content Change Detection options
        /// <summary>
        /// Gets or sets whether to enable change detection
        /// </summary>
        public bool EnableChangeDetection { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to track content versions
        /// </summary>
        public bool TrackContentVersions { get; set; } = true;

        /// <summary>
        /// Gets or sets the maximum number of versions to keep
        /// </summary>
        public int MaxVersionsToKeep { get; set; } = 5;

        /// <summary>
        /// Gets or sets whether to detect content changes (alias for EnableChangeDetection)
        /// </summary>
        public bool DetectContentChanges
        {
            get => EnableChangeDetection;
            set => EnableChangeDetection = value;
        }

        // Notifications
        /// <summary>
        /// Gets or sets whether to notify on changes
        /// </summary>
        public bool NotifyOnChanges { get; set; } = false;

        /// <summary>
        /// Gets or sets the email address to send notifications to
        /// </summary>
        public string NotificationEmail { get; set; } = string.Empty;

        // Continuous monitoring options
        /// <summary>
        /// Gets or sets whether to enable continuous monitoring
        /// </summary>
        public bool EnableContinuousMonitoring { get; set; } = false;

        /// <summary>
        /// Gets or sets the monitoring interval in minutes
        /// </summary>
        public int MonitoringIntervalMinutes { get; set; } = 60;

        // Adaptive Crawling options
        /// <summary>
        /// Gets or sets whether to enable adaptive crawling
        /// </summary>
        public bool EnableAdaptiveCrawling { get; set; } = true;

        /// <summary>
        /// Gets or sets the size of the priority queue
        /// </summary>
        public int PriorityQueueSize { get; set; } = 100;

        /// <summary>
        /// Gets or sets whether to adjust depth based on quality
        /// </summary>
        public bool AdjustDepthBasedOnQuality { get; set; } = true;

        // Smart Rate Limiting options
        /// <summary>
        /// Gets or sets whether to enable adaptive rate limiting
        /// </summary>
        public bool EnableAdaptiveRateLimiting { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to enable rate limiting
        /// </summary>
        public bool EnableRateLimiting { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum delay between requests in milliseconds
        /// </summary>
        public int MinDelayBetweenRequests { get; set; } = 500;

        /// <summary>
        /// Gets or sets the maximum delay between requests in milliseconds
        /// </summary>
        public int MaxDelayBetweenRequests { get; set; } = 5000;

        /// <summary>
        /// Gets or sets whether to monitor response times
        /// </summary>
        public bool MonitorResponseTimes { get; set; } = true;

        /// <summary>
        /// Gets or sets the default requests per minute
        /// </summary>
        public double DefaultRequestsPerMinute { get; set; } = 60;

        /// <summary>
        /// Gets or sets the default adaptive rate factor
        /// </summary>
        public float DefaultAdaptiveRateFactor { get; set; } = 0.8f;

        // Content extraction options
        /// <summary>
        /// Gets or sets the CSS selectors to use for content extraction
        /// </summary>
        public List<string> ContentExtractorSelectors { get; set; } = new();

        /// <summary>
        /// Gets or sets the CSS selectors to exclude from content extraction
        /// </summary>
        public List<string> ContentExtractorExcludeSelectors { get; set; } = new();

        /// <summary>
        /// Gets or sets whether to extract metadata
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to extract structured data
        /// </summary>
        public bool ExtractStructuredData { get; set; } = false;

        /// <summary>
        /// Gets or sets the custom JavaScript extractor
        /// </summary>
        public string CustomJsExtractor { get; set; } = string.Empty;

        // Regulatory content monitoring options
        /// <summary>
        /// Gets or sets whether to enable regulatory content analysis
        /// </summary>
        public bool EnableRegulatoryContentAnalysis { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to track regulatory changes
        /// </summary>
        public bool TrackRegulatoryChanges { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to classify regulatory documents
        /// </summary>
        public bool ClassifyRegulatoryDocuments { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to extract structured content
        /// </summary>
        public bool ExtractStructuredContent { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to process PDF documents
        /// </summary>
        public bool ProcessPdfDocuments { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to process Office documents
        /// </summary>
        public bool ProcessOfficeDocuments { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to monitor high impact changes
        /// </summary>
        public bool MonitorHighImpactChanges { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to enable document processing
        /// </summary>
        public bool EnableDocumentProcessing { get; set; } = false;

        // State management
        /// <summary>
        /// Gets or sets whether to enable persistent state
        /// </summary>
        public bool EnablePersistentState { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to store content in a database
        /// </summary>
        public bool StoreContentInDatabase { get; set; } = false;

        // Analytics and metrics
        /// <summary>
        /// Gets or sets whether to enable metrics tracking
        /// </summary>
        public bool EnableMetricsTracking { get; set; } = true;

        // Regulatory alert options
        /// <summary>
        /// Gets or sets the list of keywords to alert on
        /// </summary>
        public List<string> KeywordAlertList { get; set; } = new();

        /// <summary>
        /// Gets or sets the notification endpoint
        /// </summary>
        public string NotificationEndpoint { get; set; } = string.Empty;

        // UKGC specific options
        /// <summary>
        /// Gets or sets whether this is a UKGC website
        /// </summary>
        public bool IsUKGCWebsite { get; set; } = false;

        /// <summary>
        /// Gets or sets whether to prioritize enforcement actions
        /// </summary>
        public bool PrioritizeEnforcementActions { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to prioritize License Conditions and Codes of Practice
        /// </summary>
        public bool PrioritizeLCCP { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to prioritize Anti-Money Laundering
        /// </summary>
        public bool PrioritizeAML { get; set; } = true;

        // URL filtering options
        /// <summary>
        /// Gets or sets the list of allowed domains
        /// </summary>
        public List<string> AllowedDomains { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of URL patterns to exclude
        /// </summary>
        public List<string> ExcludeUrlPatterns { get; set; } = new();

        // Validation and error handling
        /// <summary>
        /// Gets or sets whether to use strict validation
        /// </summary>
        public bool StrictValidation { get; set; } = false;

        /// <summary>
        /// Gets or sets the maximum number of retries
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets whether to continue on error
        /// </summary>
        public bool ContinueOnError { get; set; } = true;

        // Additional properties needed by EnhancedScraper
        /// <summary>
        /// Gets or sets the schedule expression (CRON format)
        /// </summary>
        public string ScheduleExpression { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this scraper is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to store state
        /// </summary>
        public string StateStorePath { get; set; } = "state";

        /// <summary>
        /// Gets or sets whether to use a custom content processor
        /// </summary>
        public bool UseCustomContentProcessor { get; set; } = false;

        /// <summary>
        /// Gets or sets the type of custom content processor
        /// </summary>
        public string CustomContentProcessorType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the maximum number of concurrent processing tasks
        /// </summary>
        public int MaxConcurrentProcessing { get; set; } = 2;

        /// <summary>
        /// Gets or sets whether to compress old versions
        /// </summary>
        public bool CompressOldVersions { get; set; } = false;

        /// <summary>
        /// Gets or sets the number of days to retain data
        /// </summary>
        public int DataRetentionDays { get; set; } = 90;

        /// <summary>
        /// Gets or sets whether to enable analytics
        /// </summary>
        public bool EnableAnalytics { get; set; } = true;

        // Content relevance filtering options
        /// <summary>
        /// Gets or sets the content relevance threshold
        /// </summary>
        public double ContentRelevanceThreshold { get; set; } = 0.5;

        /// <summary>
        /// Gets or sets the list of key term filters
        /// </summary>
        public List<string> KeyTermFilters { get; set; } = new();

        // Document handling options
        /// <summary>
        /// Gets or sets whether to process document links
        /// </summary>
        public bool ProcessDocumentLinks { get; set; } = false;

        /// <summary>
        /// Gets or sets the path to store documents
        /// </summary>
        public string DocumentStoragePath { get; set; } = "documents";

        /// <summary>
        /// Gets or sets the list of document types to process
        /// </summary>
        public List<string> DocumentTypes { get; set; } = new() { ".pdf", ".doc", ".docx" };

        // Webhook notifications
        /// <summary>
        /// Gets or sets whether to enable webhook notifications
        /// </summary>
        public bool EnableWebhookNotifications { get; set; } = false;

        /// <summary>
        /// Gets or sets the webhook endpoint
        /// </summary>
        public string WebhookEndpoint { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the webhook authentication token
        /// </summary>
        public string WebhookAuthToken { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether to notify on content changes
        /// </summary>
        public bool NotifyOnContentChanges { get; set; } = true;

        /// <summary>
        /// Validates the configuration and returns a list of validation errors
        /// </summary>
        /// <returns>A list of validation errors, or an empty list if valid</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            // Validate required fields
            if (string.IsNullOrEmpty(ScraperId))
                errors.Add("ScraperId is required");

            if (string.IsNullOrEmpty(ScraperName))
                errors.Add("ScraperName is required");

            if (string.IsNullOrEmpty(StartUrl))
                errors.Add("StartUrl is required");

            // Validate numeric ranges
            if (DelayBetweenRequests < 0)
                errors.Add("DelayBetweenRequests must be non-negative");

            if (MaxConcurrentRequests <= 0)
                errors.Add("MaxConcurrentRequests must be positive");

            if (MaxDepth < 0)
                errors.Add("MaxDepth must be non-negative");

            if (RequestTimeoutSeconds <= 0)
                errors.Add("RequestTimeoutSeconds must be positive");

            if (BrowserTimeout <= 0)
                errors.Add("BrowserTimeout must be positive");

            if (MaxVersionsToKeep <= 0)
                errors.Add("MaxVersionsToKeep must be positive");

            if (MonitoringIntervalMinutes <= 0)
                errors.Add("MonitoringIntervalMinutes must be positive");

            if (MinDelayBetweenRequests < 0)
                errors.Add("MinDelayBetweenRequests must be non-negative");

            if (MaxDelayBetweenRequests <= 0)
                errors.Add("MaxDelayBetweenRequests must be positive");

            if (MinDelayBetweenRequests > MaxDelayBetweenRequests)
                errors.Add("MinDelayBetweenRequests must be less than or equal to MaxDelayBetweenRequests");

            if (DefaultRequestsPerMinute <= 0)
                errors.Add("DefaultRequestsPerMinute must be positive");

            if (DefaultAdaptiveRateFactor <= 0)
                errors.Add("DefaultAdaptiveRateFactor must be positive");

            if (MaxConcurrentProcessing <= 0)
                errors.Add("MaxConcurrentProcessing must be positive");

            if (DataRetentionDays <= 0)
                errors.Add("DataRetentionDays must be positive");

            if (ContentRelevanceThreshold < 0 || ContentRelevanceThreshold > 1)
                errors.Add("ContentRelevanceThreshold must be between 0 and 1");

            // Validate notification settings
            if (NotifyOnChanges && string.IsNullOrEmpty(NotificationEmail))
                errors.Add("NotificationEmail is required when NotifyOnChanges is true");

            if (EnableWebhookNotifications && string.IsNullOrEmpty(WebhookEndpoint))
                errors.Add("WebhookEndpoint is required when EnableWebhookNotifications is true");

            return errors;
        }

        /// <summary>
        /// Validates the configuration and throws an exception if invalid
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the configuration is invalid</exception>
        public void Validate()
        {
            var errors = GetValidationErrors();

            if (errors.Count > 0)
                throw new ArgumentException($"Invalid configuration: {string.Join(", ", errors)}");
        }

        /// <summary>
        /// Creates a default configuration for the specified URL
        /// </summary>
        /// <param name="url">The URL to scrape</param>
        /// <returns>A default configuration</returns>
        public static ScraperConfig CreateDefault(string url)
        {
            var config = new ScraperConfig
            {
                StartUrl = url,
                ScraperName = $"Scraper for {url}"
            };

            // Try to extract the domain from the URL
            try
            {
                var uri = new Uri(url);
                config.BaseUrl = $"{uri.Scheme}://{uri.Host}";

                // Add the domain to allowed domains
                config.AllowedDomains.Add(uri.Host);
            }
            catch
            {
                // Ignore URL parsing errors
            }

            return config;
        }
    }
}