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
        public string WebhookUrl { get; set; }
        
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
    }
}