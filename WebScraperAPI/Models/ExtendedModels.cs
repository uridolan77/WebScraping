using System;
using System.Collections.Generic;

namespace WebScraperApi.Models
{
    #region Content Change Detection Models

    /// <summary>
    /// Represents a version of a webpage's content
    /// </summary>
    public class PageVersion
    {
        /// <summary>
        /// When this version was created
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// Type of change compared to previous version
        /// </summary>
        public ChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Summary of what changed
        /// </summary>
        public string ChangeSummary { get; set; }
        
        /// <summary>
        /// Level of impact this change represents
        /// </summary>
        public ImpactLevel ImpactLevel { get; set; }
        
        /// <summary>
        /// Hash of the content for quick comparison
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Reference to the stored content
        /// </summary>
        public string ContentReference { get; set; }
    }

    /// <summary>
    /// Type of change detected between versions
    /// </summary>
    public enum ChangeType
    {
        Initial,
        Minor,
        Moderate,
        Major,
        Structure,
        Format,
        Removed
    }

    /// <summary>
    /// Impact level of a detected change
    /// </summary>
    public enum ImpactLevel
    {
        None,
        Low,
        Medium,
        High,
        Critical
    }

    /// <summary>
    /// Model for a detected content change
    /// </summary>
    public class ContentChangeModel
    {
        public string Url { get; set; }
        public DateTime PreviousVersion { get; set; }
        public DateTime CurrentVersion { get; set; }
        public string ChangeType { get; set; }
        public string ChangeSummary { get; set; }
        public string ImpactLevel { get; set; }
        public bool ContentHashChanged { get; set; }
    }

    #endregion

    #region Analytics Models

    /// <summary>
    /// Model for scraper analytics data
    /// </summary>
    public class ScraperAnalyticsModel
    {
        public int TotalRuns { get; set; }
        public string LastRunDuration { get; set; }
        public int TotalUrlsProcessed { get; set; }
        public Dictionary<string, int> DocumentTypes { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, object> AdvancedMetrics { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Document Processing Models

    /// <summary>
    /// Model for processed document information
    /// </summary>
    public class ProcessedDocumentModel
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public string FileType { get; set; }
    }

    /// <summary>
    /// Model for document processing results
    /// </summary>
    public class DocumentProcessingResultsModel
    {
        public List<ProcessedDocumentModel> Documents { get; set; } = new List<ProcessedDocumentModel>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    #endregion

    #region Pattern Learning Models

    /// <summary>
    /// Model for pattern learning data
    /// </summary>
    public class PatternModel
    {
        public string PatternType { get; set; }
        public string PatternValue { get; set; }
        public double Confidence { get; set; }
        public int OccurrenceCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    #endregion

    #region Regulatory Content Models
    
    /// <summary>
    /// Model for regulatory alerts
    /// </summary>
    public class RegulatoryAlertModel
    {
        public string Url { get; set; }
        public DateTime Timestamp { get; set; }
        public string AlertType { get; set; }
        public string Description { get; set; }
        public string Importance { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    #endregion

    #region Export Models
    
    /// <summary>
    /// Model for export result
    /// </summary>
    public class ExportResultModel
    {
        public string Message { get; set; }
        public string FilePath { get; set; }
        public int RecordCount { get; set; }
        public string Format { get; set; }
    }

    #endregion

    #region PersistentStateManager Extension

    /// <summary>
    /// Extension methods for PersistentStateManager
    /// </summary>
    public static class PersistentStateManagerExtensions
    {
        /// <summary>
        /// Gets analytics data from the state manager
        /// </summary>
        public static async System.Threading.Tasks.Task<Dictionary<string, object>> GetAnalyticsAsync(
            this WebScraper.StateManagement.PersistentStateManager manager)
        {
            // This would be implemented to actually query the database
            // For now, we'll return a mock implementation
            return new Dictionary<string, object>
            {
                ["TotalPagesProcessed"] = 0,
                ["TotalErrors"] = 0,
                ["AverageProcessingTimeMs"] = 0,
                ["CrawlRatePerMinute"] = 0
            };
        }
    }

    #endregion
}