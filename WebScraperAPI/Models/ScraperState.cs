using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper;
using WebScraper.RegulatoryFramework.Implementation;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents the current execution state of a scraper
    /// </summary>
    public class ScraperState
    {
        // Basic identification
        public string Id { get; set; } = string.Empty;
        public string ScraperId { get; set; } = string.Empty; // Alias for Id for compatibility

        // Status information
        public bool IsRunning { get; set; }
        public string Status { get; set; } = "Idle"; // Status string representation
        public string Message { get; set; } = string.Empty; // Status message
        public bool HasErrors { get; set; } = false; // Indicates if the scraper has encountered errors
        public string LastError { get; set; } = string.Empty; // The last error encountered by the scraper

        // Timing information
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? LastRunStartTime { get => StartTime; set => StartTime = value; } // Alias for StartTime
        public DateTime? LastRunEndTime { get => EndTime; set => EndTime = value; } // Alias for EndTime
        public string ElapsedTime { get; set; } = string.Empty;
        public DateTime? LastUpdate { get; set; } = DateTime.Now; // Last time the state was updated

        // Progress information
        public int UrlsProcessed { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        public string ProgressData { get; set; } = "{}"; // JSON serialized progress data
        public DateTime? LastMonitorCheck { get; set; }
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();

        // Active scraper instance
        public EnhancedScraper? Scraper { get; set; }

        /// <summary>
        /// Indicates whether this is a test run
        /// </summary>
        public bool IsTestRun { get; set; } = false;

        /// <summary>
        /// Maximum number of pages to process in this run
        /// </summary>
        public int MaxPages { get; set; } = 0;

        /// <summary>
        /// Update elapsed time based on start time
        /// </summary>
        public void UpdateElapsedTime()
        {
            if (IsRunning && StartTime.HasValue)
            {
                var elapsed = DateTime.Now - StartTime.Value;
                ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
            }
        }

        /// <summary>
        /// Add a log message
        /// </summary>
        public void AddLogMessage(string message)
        {
            LogMessages.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message
            });

            // Keep only the last 1000 messages
            if (LogMessages.Count > 1000)
            {
                LogMessages.RemoveAt(0);
            }
        }

        /// <summary>
        /// Get recent log messages
        /// </summary>
        public IEnumerable<LogEntry> GetRecentLogs(int limit = 100)
        {
            return LogMessages.Count <= limit
                ? LogMessages.ToList()
                : LogMessages.Skip(LogMessages.Count - limit).ToList();
        }
    }
}