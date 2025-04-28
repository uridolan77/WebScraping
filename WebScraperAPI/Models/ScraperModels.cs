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
        public string ElapsedTime { get; set; } = string.Empty;
        public int UrlsProcessed { get; set; }
        public int UrlsQueued { get; set; }
        public int DocumentsProcessed { get; set; }
        public bool HasErrors { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime? LastStatusUpdate { get; set; } = DateTime.Now;
        public DateTime? LastUpdate { get; set; } = DateTime.Now;
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();
        public DateTime? LastMonitorCheck { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        public string LastError { get; set; } = string.Empty;

        /// <summary>
        /// Clone the status (to avoid locking issues)
        /// </summary>
        public ScraperStatus Clone()
        {
            return new ScraperStatus
            {
                IsRunning = IsRunning,
                StartTime = StartTime,
                EndTime = EndTime,
                ElapsedTime = ElapsedTime,
                UrlsProcessed = UrlsProcessed,
                LogMessages = LogMessages?.ToList() ?? new List<LogEntry>(),
                LastMonitorCheck = LastMonitorCheck,
                PipelineMetrics = new PipelineMetrics
                {
                    ProcessingItems = PipelineMetrics?.ProcessingItems ?? 0,
                    QueuedItems = PipelineMetrics?.QueuedItems ?? 0,
                    CompletedItems = PipelineMetrics?.CompletedItems ?? 0,
                    FailedItems = PipelineMetrics?.FailedItems ?? 0,
                    AverageProcessingTimeMs = PipelineMetrics?.AverageProcessingTimeMs ?? 0
                },
                Message = Message,
                HasErrors = HasErrors,
                LastError = LastError,
                LastStatusUpdate = LastStatusUpdate,
                LastUpdate = LastUpdate,
                UrlsQueued = UrlsQueued,
                DocumentsProcessed = DocumentsProcessed
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
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "Info";

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
        }
    }
}