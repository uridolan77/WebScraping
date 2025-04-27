using System;
using System.Collections.Generic;
using System.Linq;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents the current status of a scraper
    /// </summary>
    public class ScraperStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ElapsedTime { get; set; }
        public int UrlsProcessed { get; set; }
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();
        public DateTime? LastMonitorCheck { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        
        // Added properties to fix compilation errors
        public string Message { get; set; }
        public bool HasErrors { get; set; }
        public string LastError { get; set; }
        public DateTime LastStatusUpdate { get; set; } = DateTime.Now;
        public int UrlsQueued { get; set; }
        public int DocumentsProcessed { get; set; }
        
        /// <summary>
        /// Clone the status (to avoid locking issues)
        /// </summary>
        public ScraperStatus Clone()
        {
            return new ScraperStatus
            {
                IsRunning = this.IsRunning,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                ElapsedTime = this.ElapsedTime,
                UrlsProcessed = this.UrlsProcessed,
                LogMessages = this.LogMessages.ToList(),
                LastMonitorCheck = this.LastMonitorCheck,
                PipelineMetrics = new PipelineMetrics
                {
                    ProcessingItems = this.PipelineMetrics?.ProcessingItems ?? 0,
                    QueuedItems = this.PipelineMetrics?.QueuedItems ?? 0,
                    CompletedItems = this.PipelineMetrics?.CompletedItems ?? 0,
                    FailedItems = this.PipelineMetrics?.FailedItems ?? 0,
                    AverageProcessingTimeMs = this.PipelineMetrics?.AverageProcessingTimeMs ?? 0
                },
                // Clone the new properties
                Message = this.Message,
                HasErrors = this.HasErrors,
                LastError = this.LastError,
                LastStatusUpdate = this.LastStatusUpdate,
                UrlsQueued = this.UrlsQueued,
                DocumentsProcessed = this.DocumentsProcessed
            };
        }
    }
    
    /// <summary>
    /// Represents metrics from the processing pipeline
    /// </summary>
    public class PipelineMetrics
    {
        public int ProcessingItems { get; set; }
        public int QueuedItems { get; set; }
        public int CompletedItems { get; set; }
        public int FailedItems { get; set; }
        public double AverageProcessingTimeMs { get; set; }
    }
    
    /// <summary>
    /// Represents a log entry
    /// </summary>
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
    }
}