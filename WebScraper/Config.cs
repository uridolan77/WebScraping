namespace WebScraper
{
    public class ScraperConfig
    {
        // Multi-scraper support
        public string ScraperId { get; set; }
        public string ScraperName { get; set; } = "Default Scraper";
        public string Name { get; set; } = "Default Scraper"; // For backward compatibility
        public string ScraperType { get; set; } = "Standard"; // Added missing property
        
        // Creation metadata
        public System.DateTime CreatedAt { get; set; } = System.DateTime.Now;
        
        public string StartUrl { get; set; }
        public string BaseUrl { get; set; }
        public string OutputDirectory { get; set; }
        public int DelayBetweenRequests { get; set; }
        public int MaxConcurrentRequests { get; set; }
        public int MaxDepth { get; set; }
        public int MaxCrawlDepth { get; set; } // Added missing property (same as MaxDepth for compatibility)
        public bool FollowExternalLinks { get; set; }
        public bool RespectRobotsTxt { get; set; }
        public int RequestTimeoutSeconds { get; set; } = 30;
        public string UserAgent { get; set; } = "Mozilla/5.0 WebScraper/1.0";
        public bool AppendToExistingData { get; set; } = false;

        // Browser automation
        public bool ProcessJsHeavyPages { get; set; } = false;
        public int BrowserTimeout { get; set; } = 60000; // milliseconds
        public bool UseHeadlessBrowser { get; set; } = true;
        
        // Header/footer pattern learning
        public bool AutoLearnHeaderFooter { get; set; } = true;
        public int LearningPagesCount { get; set; } = 5;

        // Content Change Detection options
        public bool EnableChangeDetection { get; set; } = true;
        public bool TrackContentVersions { get; set; } = true;
        public int MaxVersionsToKeep { get; set; } = 5;
        public bool DetectContentChanges { get; set; } = true; // Added missing property
        
        // Notifications
        public bool NotifyOnChanges { get; set; } = false;
        public string NotificationEmail { get; set; }

        // Continuous monitoring options
        public bool EnableContinuousMonitoring { get; set; } = false;
        public int MonitoringIntervalMinutes { get; set; } = 60;
        
        // Adaptive Crawling options
        public bool EnableAdaptiveCrawling { get; set; } = true;
        public int PriorityQueueSize { get; set; } = 100;
        public bool AdjustDepthBasedOnQuality { get; set; } = true;

        // Smart Rate Limiting options
        public bool EnableAdaptiveRateLimiting { get; set; } = true;
        public bool EnableRateLimiting { get; set; } = true; // Added missing property
        public int MinDelayBetweenRequests { get; set; } = 500; // milliseconds
        public int MaxDelayBetweenRequests { get; set; } = 5000; // milliseconds
        public bool MonitorResponseTimes { get; set; } = true;
        
        // Content extraction options
        public System.Collections.Generic.List<string> ContentExtractorSelectors { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> ContentExtractorExcludeSelectors { get; set; } = new System.Collections.Generic.List<string>();
        public bool ExtractMetadata { get; set; } = true;
        public bool ExtractStructuredData { get; set; } = false;
        public string CustomJsExtractor { get; set; }
        
        // Regulatory content monitoring options
        public bool EnableRegulatoryContentAnalysis { get; set; } = false;
        public bool TrackRegulatoryChanges { get; set; } = false;
        public bool ClassifyRegulatoryDocuments { get; set; } = false;
        public bool ExtractStructuredContent { get; set; } = false;
        public bool ProcessPdfDocuments { get; set; } = false;
        public bool ProcessOfficeDocuments { get; set; } = false;  // Added support for Office documents
        public bool MonitorHighImpactChanges { get; set; } = false;
        public bool EnableDocumentProcessing { get; set; } = false; // Added missing property
        
        // State management
        public bool EnablePersistentState { get; set; } = true; // Added missing property
        
        // Regulatory alert options
        public System.Collections.Generic.List<string> KeywordAlertList { get; set; } = new System.Collections.Generic.List<string>();
        public string NotificationEndpoint { get; set; }
        
        // UKGC specific options
        public bool IsUKGCWebsite { get; set; } = false;
        public bool PrioritizeEnforcementActions { get; set; } = true;
        public bool PrioritizeLCCP { get; set; } = true;  // License Conditions and Codes of Practice
        public bool PrioritizeAML { get; set; } = true;   // Anti-Money Laundering

        // URL filtering options
        public System.Collections.Generic.List<string> AllowedDomains { get; set; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> ExcludeUrlPatterns { get; set; } = new System.Collections.Generic.List<string>();
        
        // Validation and error handling
        public bool StrictValidation { get; set; } = false;
        public int MaxRetries { get; set; } = 3;
        public bool ContinueOnError { get; set; } = true;
        
        // Additional properties needed by EnhancedScraper
        public string ScheduleExpression { get; set; } // For CRON-based scheduling
        public bool IsActive { get; set; } = true;
        public string StateStorePath { get; set; } = "state";
        public bool UseCustomContentProcessor { get; set; } = false;
        public string CustomContentProcessorType { get; set; }
        public int MaxConcurrentProcessing { get; set; } = 2;
        public bool CompressOldVersions { get; set; } = false;
        public int DataRetentionDays { get; set; } = 90;
        public bool EnableAnalytics { get; set; } = true;
        
        // Content relevance filtering options
        public double ContentRelevanceThreshold { get; set; } = 0.5;
        public System.Collections.Generic.List<string> KeyTermFilters { get; set; } = new System.Collections.Generic.List<string>();
        
        // Document handling options
        public bool ProcessDocumentLinks { get; set; } = false;
        public string DocumentStoragePath { get; set; } = "documents";
        public System.Collections.Generic.List<string> DocumentTypes { get; set; } = new System.Collections.Generic.List<string> { ".pdf", ".doc", ".docx" };
        
        // Webhook notifications
        public bool EnableWebhookNotifications { get; set; } = false;
        public string WebhookEndpoint { get; set; }
        public string WebhookAuthToken { get; set; }
        public bool NotifyOnContentChanges { get; set; } = true;
    }
}