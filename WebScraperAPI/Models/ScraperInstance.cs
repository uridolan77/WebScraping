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
        /// Scraper configuration
        /// </summary>
        public ScraperConfigModel Config { get; set; }
        
        /// <summary>
        /// Scraper status
        /// </summary>
        public ScraperStatus Status { get; set; }
        
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