using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Model for scraper configuration
    /// </summary>
    public class ScraperConfigModel
    {
        // Unique identifier for this scraper configuration
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is required")]
        public string Name { get; set; } = "New Scraper";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime LastModified { get; set; } = DateTime.Now;

        public DateTime? LastRun { get; set; }

        // Track run count for analytics
        public int RunCount { get; set; } = 0;

        // Basic settings
        [Required(AllowEmptyStrings = false, ErrorMessage = "StartUrl is required")]
        public string StartUrl { get; set; } = string.Empty;

        // List of start URLs for more complex configurations
        public List<string>? StartUrls { get; set; } = new List<string>();

        [Required(AllowEmptyStrings = false, ErrorMessage = "BaseUrl is required")]
        public string BaseUrl { get; set; } = string.Empty;

        public string OutputDirectory { get; set; } = "ScrapedData";

        public int DelayBetweenRequests { get; set; } = 1000;

        public int MaxConcurrentRequests { get; set; } = 5;

        public int MaxDepth { get; set; } = 5;

        // Maximum number of pages to scrape
        public int MaxPages { get; set; } = 1000;

        // Whether to follow links at all
        public bool FollowLinks { get; set; } = true;

        public bool FollowExternalLinks { get; set; } = false;

        public bool RespectRobotsTxt { get; set; } = true;

        // Header/footer pattern learning
        public bool AutoLearnHeaderFooter { get; set; } = true;

        public int LearningPagesCount { get; set; } = 5;

        // Content Change Detection options
        public bool EnableChangeDetection { get; set; } = true;

        public bool TrackContentVersions { get; set; } = true;

        public int MaxVersionsToKeep { get; set; } = 5;

        // Adaptive Crawling options
        public bool EnableAdaptiveCrawling { get; set; } = true;

        public int PriorityQueueSize { get; set; } = 100;

        public bool AdjustDepthBasedOnQuality { get; set; } = true;

        // Smart Rate Limiting options
        public bool EnableAdaptiveRateLimiting { get; set; } = true;

        public int MinDelayBetweenRequests { get; set; } = 500;

        public int MaxDelayBetweenRequests { get; set; } = 5000;

        public bool MonitorResponseTimes { get; set; } = true;

        public int MaxRequestsPerMinute { get; set; } = 60;

        public string UserAgent { get; set; } = "WebStraction Crawler/1.0";

        public bool BackOffOnErrors { get; set; } = true;

        // Proxy support
        public bool UseProxies { get; set; } = false;

        public string ProxyRotationStrategy { get; set; } = "RoundRobin";

        public bool TestProxiesBeforeUse { get; set; } = true;

        public int MaxProxyFailuresBeforeRemoval { get; set; } = 3;

        // Monitoring options
        public bool EnableContinuousMonitoring { get; set; } = false;

        public int MonitoringIntervalMinutes { get; set; } = 60;

        public bool NotifyOnChanges { get; set; } = false;

        public string? NotificationEmail { get; set; }

        public bool TrackChangesHistory { get; set; } = true;

        // Regulatory content analysis options
        public bool EnableRegulatoryContentAnalysis { get; set; } = false;

        public bool TrackRegulatoryChanges { get; set; } = false;

        public bool ClassifyRegulatoryDocuments { get; set; } = false;

        public bool ExtractStructuredContent { get; set; } = false;

        public bool ProcessPdfDocuments { get; set; } = false;

        public bool MonitorHighImpactChanges { get; set; } = false;

        public List<string> KeywordAlertList { get; set; } = new List<string>();

        public string? NotificationEndpoint { get; set; }

        // Content extraction options
        public List<string> ContentExtractorSelectors { get; set; } = new List<string>();

        public List<string> ContentExtractorExcludeSelectors { get; set; } = new List<string>();

        public bool ExtractMetadata { get; set; } = true;

        public bool ExtractStructuredData { get; set; } = false;

        public bool EnableEnhancedContentExtraction { get; set; } = false;

        public string? CustomJsExtractor { get; set; }

        // Page processing options
        public string? WaitForSelector { get; set; }

        // UKGC specific options
        public bool IsUKGCWebsite { get; set; } = false;

        public bool PrioritizeEnforcementActions { get; set; } = true;

        public bool PrioritizeLCCP { get; set; } = true;

        public bool PrioritizeAML { get; set; } = true;

        // Webhook options
        public bool WebhookEnabled { get; set; } = false;

        public string? WebhookUrl { get; set; }

        public List<string> WebhookTriggers { get; set; } = new List<string>();

        public bool NotifyOnContentChanges { get; set; } = false;

        public bool NotifyOnDocumentProcessed { get; set; } = false;

        public bool NotifyOnScraperStatusChange { get; set; } = false;

        public string WebhookFormat { get; set; } = "JSON";

        // Performance and storage options
        public bool EnableContentCompression { get; set; } = false;

        public int CompressionThresholdBytes { get; set; } = 102400; // 100KB

        public bool CollectDetailedMetrics { get; set; } = true;

        public int MetricsReportingIntervalSeconds { get; set; } = 60;

        public bool TrackDomainMetrics { get; set; } = false;

        public string ScraperType { get; set; } = "Standard";

        // Schedule information (optional)
        public List<Dictionary<string, object>> Schedules { get; set; } = new List<Dictionary<string, object>>();

        /// <summary>
        /// Gets the monitoring interval in minutes
        /// </summary>
        /// <returns>The monitoring interval in minutes, or 60 if monitoring is not enabled</returns>
        public int GetMonitoringInterval()
        {
            return EnableContinuousMonitoring ? MonitoringIntervalMinutes : 60;
        }

        // Convert to WebScraper.ScraperConfig for the actual scraper
        public WebScraper.ScraperConfig ToScraperConfig()
        {
            return new WebScraper.ScraperConfig
            {
                ScraperId = this.Id,
                ScraperName = this.Name,
                StartUrl = this.StartUrl,
                BaseUrl = this.BaseUrl,
                OutputDirectory = $"{this.OutputDirectory}/{this.Id}",
                DelayBetweenRequests = this.DelayBetweenRequests,
                MaxConcurrentRequests = this.MaxConcurrentRequests,
                MaxDepth = this.MaxDepth,
                FollowExternalLinks = this.FollowExternalLinks,
                RespectRobotsTxt = this.RespectRobotsTxt,
                AutoLearnHeaderFooter = this.AutoLearnHeaderFooter,
                LearningPagesCount = this.LearningPagesCount,
                EnableChangeDetection = this.EnableChangeDetection,
                TrackContentVersions = this.TrackContentVersions,
                MaxVersionsToKeep = this.MaxVersionsToKeep,
                EnableAdaptiveCrawling = this.EnableAdaptiveCrawling,
                PriorityQueueSize = this.PriorityQueueSize,
                AdjustDepthBasedOnQuality = this.AdjustDepthBasedOnQuality,
                EnableAdaptiveRateLimiting = this.EnableAdaptiveRateLimiting,
                MinDelayBetweenRequests = this.MinDelayBetweenRequests,
                MaxDelayBetweenRequests = this.MaxDelayBetweenRequests,
                MonitorResponseTimes = this.MonitorResponseTimes,
                NotifyOnChanges = this.NotifyOnChanges,
                NotificationEmail = this.NotificationEmail,  // Already nullable

                // Content extraction options
                ContentExtractorSelectors = this.ContentExtractorSelectors,
                ContentExtractorExcludeSelectors = this.ContentExtractorExcludeSelectors,
                ExtractMetadata = this.ExtractMetadata,
                ExtractStructuredData = this.ExtractStructuredData,
                EnableEnhancedContentExtraction = this.EnableEnhancedContentExtraction,
                CustomJsExtractor = this.CustomJsExtractor,

                // Add regulatory options
                EnableRegulatoryContentAnalysis = this.EnableRegulatoryContentAnalysis,
                TrackRegulatoryChanges = this.TrackRegulatoryChanges,
                ClassifyRegulatoryDocuments = this.ClassifyRegulatoryDocuments,
                ExtractStructuredContent = this.ExtractStructuredContent,
                ProcessPdfDocuments = this.ProcessPdfDocuments,
                MonitorHighImpactChanges = this.MonitorHighImpactChanges,
                KeywordAlertList = this.KeywordAlertList,
                NotificationEndpoint = this.NotificationEndpoint,

                // UKGC specific options
                IsUKGCWebsite = this.IsUKGCWebsite,
                PrioritizeEnforcementActions = this.PrioritizeEnforcementActions,
                PrioritizeLCCP = this.PrioritizeLCCP,
                PrioritizeAML = this.PrioritizeAML,

                // Added common configurable flags
                MaxCrawlDepth = this.MaxDepth,
                EnableRateLimiting = this.EnableAdaptiveRateLimiting,
                EnablePersistentState = true,
                EnableDocumentProcessing = this.ProcessPdfDocuments || this.ExtractStructuredContent,
                DetectContentChanges = this.EnableChangeDetection
            };
        }
    }
}