using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data
{
    // Entity classes for database tables

    public class ScraperConfigEntity
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime LastModified { get; set; }
        
        public DateTime? LastRun { get; set; }
        
        public int RunCount { get; set; }
        
        [Required]
        public string StartUrl { get; set; }
        
        [Required]
        public string BaseUrl { get; set; }
        
        [Required]
        public string OutputDirectory { get; set; }
        
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
        
        public string UserAgent { get; set; }
        
        public bool BackOffOnErrors { get; set; }
        
        public bool UseProxies { get; set; }
        
        public string ProxyRotationStrategy { get; set; }
        
        public bool TestProxiesBeforeUse { get; set; }
        
        public int MaxProxyFailuresBeforeRemoval { get; set; }
        
        public bool EnableContinuousMonitoring { get; set; }
        
        public int MonitoringIntervalMinutes { get; set; }
        
        public bool NotifyOnChanges { get; set; }
        
        public string NotificationEmail { get; set; }
        
        public bool TrackChangesHistory { get; set; }
        
        public bool EnableRegulatoryContentAnalysis { get; set; }
        
        public bool TrackRegulatoryChanges { get; set; }
        
        public bool ClassifyRegulatoryDocuments { get; set; }
        
        public bool ExtractStructuredContent { get; set; }
        
        public bool ProcessPdfDocuments { get; set; }
        
        public bool MonitorHighImpactChanges { get; set; }
        
        public bool ExtractMetadata { get; set; }
        
        public bool ExtractStructuredData { get; set; }
        
        public string CustomJsExtractor { get; set; }
        
        public string WaitForSelector { get; set; }
        
        public bool IsUKGCWebsite { get; set; }
        
        public bool PrioritizeEnforcementActions { get; set; }
        
        public bool PrioritizeLCCP { get; set; }
        
        public bool PrioritizeAML { get; set; }
        
        public string NotificationEndpoint { get; set; }
        
        public bool WebhookEnabled { get; set; }
        
        public string WebhookUrl { get; set; }
        
        public bool NotifyOnContentChanges { get; set; }
        
        public bool NotifyOnDocumentProcessed { get; set; }
        
        public bool NotifyOnScraperStatusChange { get; set; }
        
        public string WebhookFormat { get; set; }
        
        public bool EnableContentCompression { get; set; }
        
        public int CompressionThresholdBytes { get; set; }
        
        public bool CollectDetailedMetrics { get; set; }
        
        public int MetricsReportingIntervalSeconds { get; set; }
        
        public bool TrackDomainMetrics { get; set; }
        
        public string ScraperType { get; set; }

        // Navigation properties
        public virtual ICollection<ScraperStartUrlEntity> StartUrls { get; set; }
        public virtual ICollection<ContentExtractorSelectorEntity> ContentExtractorSelectors { get; set; }
        public virtual ICollection<KeywordAlertEntity> KeywordAlerts { get; set; }
        public virtual ICollection<WebhookTriggerEntity> WebhookTriggers { get; set; }
        public virtual ICollection<DomainRateLimitEntity> DomainRateLimits { get; set; }
        public virtual ICollection<ProxyConfigurationEntity> ProxyConfigurations { get; set; }
        public virtual ICollection<ScraperScheduleEntity> Schedules { get; set; }
        public virtual ICollection<ScraperRunEntity> Runs { get; set; }
        public virtual ScraperStatusEntity Status { get; set; }
        public virtual ICollection<PipelineMetricEntity> PipelineMetrics { get; set; }
        public virtual ICollection<LogEntryEntity> LogEntries { get; set; }
        public virtual ICollection<ContentChangeRecordEntity> ContentChangeRecords { get; set; }
        public virtual ICollection<ProcessedDocumentEntity> ProcessedDocuments { get; set; }
        public virtual ICollection<ScraperMetricEntity> Metrics { get; set; }
    }

    public class ScraperStartUrlEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Url { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class ContentExtractorSelectorEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Selector { get; set; }
        
        public bool IsExclude { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class KeywordAlertEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Keyword { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class WebhookTriggerEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string TriggerName { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class DomainRateLimitEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Domain { get; set; }
        
        public int MaxRequestsPerMinute { get; set; }
        
        public int DelayBetweenRequests { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class ProxyConfigurationEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Protocol { get; set; }
        
        public bool IsActive { get; set; }
        
        public int FailureCount { get; set; }
        
        public DateTime? LastUsed { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class ScraperScheduleEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string CronExpression { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LastRun { get; set; }
        
        public DateTime? NextRun { get; set; }
        
        public int? MaxRuntimeMinutes { get; set; }
        
        public string NotificationEmail { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class ScraperRunEntity
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public int UrlsProcessed { get; set; }
        
        public int DocumentsProcessed { get; set; }
        
        public bool? Successful { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string ElapsedTime { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ICollection<LogEntryEntity> LogEntries { get; set; }
        public virtual ICollection<ContentChangeRecordEntity> ContentChangeRecords { get; set; }
        public virtual ICollection<ProcessedDocumentEntity> ProcessedDocuments { get; set; }
    }

    public class ScraperStatusEntity
    {
        [Key]
        public string ScraperId { get; set; }
        
        public bool IsRunning { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public string ElapsedTime { get; set; }
        
        public int UrlsProcessed { get; set; }
        
        public int UrlsQueued { get; set; }
        
        public int DocumentsProcessed { get; set; }
        
        public bool HasErrors { get; set; }
        
        public string Message { get; set; }
        
        public DateTime? LastStatusUpdate { get; set; }
        
        public DateTime? LastUpdate { get; set; }
        
        public DateTime? LastMonitorCheck { get; set; }
        
        public string LastError { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class PipelineMetricEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        public int ProcessingItems { get; set; }
        
        public int QueuedItems { get; set; }
        
        public int CompletedItems { get; set; }
        
        public int FailedItems { get; set; }
        
        public double AverageProcessingTimeMs { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }

    public class LogEntryEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public string Level { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
    }

    public class ContentChangeRecordEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        [Required]
        public string ChangeType { get; set; }
        
        [Required]
        public DateTime DetectedAt { get; set; }
        
        public int Significance { get; set; }
        
        public string ChangeDetails { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
    }

    public class ProcessedDocumentEntity
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        public string Title { get; set; }
        
        [Required]
        public string DocumentType { get; set; }
        
        [Required]
        public DateTime ProcessedAt { get; set; }
        
        public long ContentSizeBytes { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
        public virtual ICollection<DocumentMetadataEntity> Metadata { get; set; }
    }

    public class DocumentMetadataEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string DocumentId { get; set; }
        
        [Required]
        public string MetaKey { get; set; }
        
        public string MetaValue { get; set; }

        // Navigation property
        public virtual ProcessedDocumentEntity ProcessedDocument { get; set; }
    }

    public class ScraperMetricEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string MetricName { get; set; }
        
        public double MetricValue { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
