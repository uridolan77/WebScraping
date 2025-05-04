using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperConfigEntity
    {
        [Key]
        public string Id { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime LastModified { get; set; }

        public DateTime? LastRun { get; set; }

        public int RunCount { get; set; }

        [Required]
        public string StartUrl { get; set; } = string.Empty;

        [Required]
        public string BaseUrl { get; set; } = string.Empty;

        [Required]
        public string OutputDirectory { get; set; } = string.Empty;

        public int DelayBetweenRequests { get; set; }

        public int MaxConcurrentRequests { get; set; }

        public int MaxDepth { get; set; }

        public int MaxPages { get; set; }

        public bool FollowLinks { get; set; }

        public bool FollowExternalLinks { get; set; }

        public bool RespectRobotsTxt { get; set; }

        public bool AutoLearnHeaderFooter { get; set; }

        public int LearningPagesCount { get; set; }

        public bool EnableChangeDetection { get; set; }

        public bool TrackContentVersions { get; set; }

        public int MaxVersionsToKeep { get; set; }

        public bool EnableAdaptiveCrawling { get; set; }

        public int PriorityQueueSize { get; set; }

        public bool AdjustDepthBasedOnQuality { get; set; }

        public bool EnableAdaptiveRateLimiting { get; set; }

        public int MinDelayBetweenRequests { get; set; }

        public int MaxDelayBetweenRequests { get; set; }

        public bool MonitorResponseTimes { get; set; }

        public int MaxRequestsPerMinute { get; set; }

        public string? UserAgent { get; set; }

        public bool BackOffOnErrors { get; set; }

        public bool UseProxies { get; set; }

        public string? ProxyRotationStrategy { get; set; }

        public bool TestProxiesBeforeUse { get; set; }

        public int MaxProxyFailuresBeforeRemoval { get; set; }

        public bool EnableContinuousMonitoring { get; set; }

        public int MonitoringIntervalMinutes { get; set; }

        public bool NotifyOnChanges { get; set; }

        public string? NotificationEmail { get; set; }

        public bool TrackChangesHistory { get; set; }

        public bool EnableRegulatoryContentAnalysis { get; set; }

        public bool TrackRegulatoryChanges { get; set; }

        public bool ClassifyRegulatoryDocuments { get; set; }

        public bool ExtractStructuredContent { get; set; }

        public bool ProcessPdfDocuments { get; set; }

        public bool MonitorHighImpactChanges { get; set; }

        public bool ExtractMetadata { get; set; }

        public bool ExtractStructuredData { get; set; }

        public string? CustomJsExtractor { get; set; }

        public string? WaitForSelector { get; set; }

        public bool IsUKGCWebsite { get; set; }

        public bool PrioritizeEnforcementActions { get; set; }

        public bool PrioritizeLCCP { get; set; }

        public bool PrioritizeAML { get; set; }

        public string? NotificationEndpoint { get; set; }

        public bool WebhookEnabled { get; set; }

        public string? WebhookUrl { get; set; }

        public bool NotifyOnContentChanges { get; set; }

        public bool NotifyOnDocumentProcessed { get; set; }

        public bool NotifyOnScraperStatusChange { get; set; }

        public string? WebhookFormat { get; set; }

        public bool EnableContentCompression { get; set; }

        public int CompressionThresholdBytes { get; set; }

        public bool CollectDetailedMetrics { get; set; }

        public int MetricsReportingIntervalSeconds { get; set; }

        public bool TrackDomainMetrics { get; set; }

        public string? ScraperType { get; set; }

        // Navigation properties
        public virtual ICollection<ScraperStartUrlEntity> StartUrls { get; set; } = new List<ScraperStartUrlEntity>();
        public virtual ICollection<ContentExtractorSelectorEntity> ContentExtractorSelectors { get; set; } = new List<ContentExtractorSelectorEntity>();
        public virtual ICollection<KeywordAlertEntity> KeywordAlerts { get; set; } = new List<KeywordAlertEntity>();
        public virtual ICollection<WebhookTriggerEntity> WebhookTriggers { get; set; } = new List<WebhookTriggerEntity>();
        public virtual ICollection<DomainRateLimitEntity> DomainRateLimits { get; set; } = new List<DomainRateLimitEntity>();
        public virtual ICollection<ProxyConfigurationEntity> ProxyConfigurations { get; set; } = new List<ProxyConfigurationEntity>();
        public virtual ICollection<ScraperScheduleEntity> Schedules { get; set; } = new List<ScraperScheduleEntity>();
        public virtual ICollection<ScraperRunEntity> Runs { get; set; } = new List<ScraperRunEntity>();
        public virtual ScraperStatusEntity? Status { get; set; }
        public virtual ICollection<PipelineMetricEntity> PipelineMetrics { get; set; } = new List<PipelineMetricEntity>();
        public virtual ICollection<LogEntryEntity> LogEntries { get; set; } = new List<LogEntryEntity>();
        public virtual ICollection<ContentChangeRecordEntity> ContentChangeRecords { get; set; } = new List<ContentChangeRecordEntity>();
        public virtual ICollection<ProcessedDocumentEntity> ProcessedDocuments { get; set; } = new List<ProcessedDocumentEntity>();
        public virtual ICollection<ScraperMetricEntity> Metrics { get; set; } = new List<ScraperMetricEntity>();
        public virtual ICollection<ScraperLogEntity> Logs { get; set; } = new List<ScraperLogEntity>();
        public virtual ICollection<ScrapedPageEntity> ScrapedPages { get; set; } = new List<ScrapedPageEntity>();
    }
}
