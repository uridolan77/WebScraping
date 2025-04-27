using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents the current execution state of a scraper
    /// </summary>
    public class ScraperState
    {
        // Added ID property
        public string Id { get; set; }
        
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ElapsedTime { get; set; } = string.Empty;
        public int UrlsProcessed { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        public DateTime? LastMonitorCheck { get; set; }
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();
        
        // Added property for active scraper instance
        public EnhancedScraper Scraper { get; set; }
        
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