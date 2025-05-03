using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data
{
    // Entity classes for database tables

    // ScraperConfigEntity moved to WebScraperApi.Data.Entities namespace
    // ScraperStartUrlEntity moved to WebScraperApi.Data.Entities namespace

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

    // ScraperStatusEntity moved to WebScraperApi.Data.Entities namespace

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
