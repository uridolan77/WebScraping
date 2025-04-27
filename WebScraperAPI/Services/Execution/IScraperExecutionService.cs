using System;
using System.Threading.Tasks;
using WebScraper;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Execution
{
    /// <summary>
    /// Interface for scraper execution services
    /// </summary>
    public interface IScraperExecutionService
    {
        /// <summary>
        /// Starts a scraper with the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="scraperState">The current scraper state to update during execution</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task<bool> StartScraperAsync(
            ScraperConfigModel config, 
            ScraperState scraperState,
            Action<string> logAction);
        
        /// <summary>
        /// Creates an enhanced scraper with components from factories
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A configured EnhancedScraper instance</returns>
        Task<EnhancedScraper> CreateEnhancedScraperAsync(ScraperConfig config, Action<string> logAction);
        
        /// <summary>
        /// Stops a running scraper
        /// </summary>
        /// <param name="scraper">The scraper to stop</param>
        /// <param name="logAction">Action for logging messages</param>
        void StopScraper(Scraper scraper, Action<string> logAction);
    }
}