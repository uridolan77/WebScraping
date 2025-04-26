namespace WebScraper
{
    public class ScraperConfig
    {
        // Multi-scraper support
        public string ScraperId { get; set; }
        public string ScraperName { get; set; } = "Default Scraper";
        
        public string StartUrl { get; set; }
        public string BaseUrl { get; set; }
        public string OutputDirectory { get; set; }
        public int DelayBetweenRequests { get; set; }
        public int MaxConcurrentRequests { get; set; }
        public int MaxDepth { get; set; }
        public bool FollowExternalLinks { get; set; }
        public bool RespectRobotsTxt { get; set; }
        public int RequestTimeoutSeconds { get; set; } = 30;
        public string UserAgent { get; set; } = "Mozilla/5.0 WebScraper/1.0";
        public bool AppendToExistingData { get; set; } = false;

        // Header/footer pattern learning
        public bool AutoLearnHeaderFooter { get; set; } = true;
        public int LearningPagesCount { get; set; } = 5;

        // Content Change Detection options
        public bool EnableChangeDetection { get; set; } = true;
        public bool TrackContentVersions { get; set; } = true;
        public int MaxVersionsToKeep { get; set; } = 5;
        
        // Notifications
        public bool NotifyOnChanges { get; set; } = false;
        public string NotificationEmail { get; set; }

        // Adaptive Crawling options
        public bool EnableAdaptiveCrawling { get; set; } = true;
        public int PriorityQueueSize { get; set; } = 100;
        public bool AdjustDepthBasedOnQuality { get; set; } = true;

        // Smart Rate Limiting options
        public bool EnableAdaptiveRateLimiting { get; set; } = true;
        public int MinDelayBetweenRequests { get; set; } = 500; // milliseconds
        public int MaxDelayBetweenRequests { get; set; } = 5000; // milliseconds
        public bool MonitorResponseTimes { get; set; } = true;
    }
}