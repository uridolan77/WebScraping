using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Monitoring
{
    /// <summary>
    /// Interface for scraper monitoring services
    /// </summary>
    public interface IScraperMonitoringService
    {
        /// <summary>
        /// Gets the current status of a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>The current status of the scraper</returns>
        ScraperStatus GetScraperStatus(string id);
        
        /// <summary>
        /// Gets the logs for a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="limit">Maximum number of log entries to return</param>
        /// <returns>Collection of log entries</returns>
        IEnumerable<LogEntry> GetScraperLogs(string id, int limit = 100);
        
        /// <summary>
        /// Adds a log message for a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="message">Message to log</param>
        void AddLogMessage(string id, string message);
        
        /// <summary>
        /// Run a monitoring check for all scrapers that have monitoring enabled
        /// </summary>
        Task RunAllMonitoringChecksAsync();
        
        /// <summary>
        /// Runs a monitoring check for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        Task RunMonitoringCheckAsync(string id);
    }
}