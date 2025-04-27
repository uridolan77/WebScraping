using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.Processing;
using WebScraper.StateManagement;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Interface for managing scraper state
    /// </summary>
    public interface IStateManager
    {
        /// <summary>
        /// Initialize the state manager
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Save the state of the scraper
        /// </summary>
        Task SaveScraperStateAsync(WebScraper.StateManagement.ScraperState state);
        
        /// <summary>
        /// Get the state of the scraper
        /// </summary>
        Task<WebScraper.StateManagement.ScraperState> GetScraperStateAsync(string scraperId);

        /// <summary>
        /// Check if a URL has been visited
        /// </summary>
        Task<bool> HasUrlBeenVisitedAsync(string scraperId, string url);

        /// <summary>
        /// Mark a URL as visited
        /// </summary>
        Task MarkUrlVisitedAsync(string scraperId, string url, int statusCode, int responseTimeMs);

        /// <summary>
        /// Save a content version
        /// </summary>
        Task SaveContentVersionAsync(ContentItem content, int maxVersions);

        /// <summary>
        /// Get the latest content version for a URL
        /// </summary>
        Task<(bool Found, ContentItem Version)> GetLatestContentVersionAsync(string url);
        
        /// <summary>
        /// Get storage statistics
        /// </summary>
        Task<Dictionary<string, object>> GetStorageStatisticsAsync();
    }
}