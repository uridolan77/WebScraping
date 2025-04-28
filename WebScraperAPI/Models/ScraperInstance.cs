using System;
using WebScraper;
using WebScraper.StateManagement;
using WebScraper.Monitoring;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents a scraper instance in memory
    /// </summary>
    public class ScraperInstance
    {
        /// <summary>
        /// Gets the ID of this scraper instance (derived from Config.Id)
        /// </summary>
        public string Id => Config?.Id ?? string.Empty;

        /// <summary>
        /// Scraper configuration
        /// </summary>
        public ScraperConfigModel Config { get; set; }

        /// <summary>
        /// Scraper status
        /// </summary>
        public ScraperStatus Status { get; set; }

        /// <summary>
        /// List of log messages for this scraper
        /// </summary>
        public List<LogEntry> LogMessages => Status?.LogMessages ?? new List<LogEntry>();

        /// <summary>
        /// List of runs for this scraper
        /// </summary>
        public List<ScraperRun> Runs { get; set; } = new List<ScraperRun>();

        /// <summary>
        /// Reference to the scraper object if running
        /// </summary>
        public Scraper Scraper { get; set; }

        /// <summary>
        /// State manager for storing persistent state
        /// </summary>
        public PersistentStateManager StateManager { get; set; }

        /// <summary>
        /// Metrics for this scraper instance
        /// </summary>
        public ScraperMetrics Metrics { get; set; }

        /// <summary>
        /// Last activity timestamp
        /// </summary>
        public DateTime LastActivity { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructor
        /// </summary>
        public ScraperInstance()
        {
            Status = new ScraperStatus();
            Metrics = new ScraperMetrics(null, "default");
        }
    }
}