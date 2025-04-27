using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Configuration
{
    /// <summary>
    /// Interface for scraper configuration services
    /// </summary>
    public interface IScraperConfigurationService
    {
        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        /// <returns>A collection of scraper configurations</returns>
        Task<IEnumerable<ScraperConfigModel>> GetAllScraperConfigsAsync();
        
        /// <summary>
        /// Gets a specific scraper configuration by ID
        /// </summary>
        Task<ScraperConfigModel> GetScraperConfigAsync(string id);
        
        /// <summary>
        /// Creates a new scraper configuration
        /// </summary>
        Task<ScraperConfigModel> CreateScraperConfigAsync(ScraperConfigModel config);
        
        /// <summary>
        /// Updates an existing scraper configuration
        /// </summary>
        Task<bool> UpdateScraperConfigAsync(string id, ScraperConfigModel config);
        
        /// <summary>
        /// Deletes a scraper configuration
        /// </summary>
        Task<bool> DeleteScraperConfigAsync(string id);
        
        /// <summary>
        /// Loads scraper configurations from the database with fallback to file
        /// </summary>
        Task<List<ScraperConfigModel>> LoadScraperConfigurationsAsync();
        
        /// <summary>
        /// Saves scraper configurations to a file (used as backup)
        /// </summary>
        void SaveScraperConfigurationsToFile(IEnumerable<ScraperConfigModel> configs);
    }
}