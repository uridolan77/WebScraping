using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    /// <summary>
    /// Repository interface for scraper configurations
    /// </summary>
    public interface IScraperConfigRepository
    {
        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        /// <returns>A collection of scraper configurations</returns>
        Task<IEnumerable<ScraperConfigEntity>> GetAllAsync();

        /// <summary>
        /// Gets a scraper configuration by ID
        /// </summary>
        /// <param name="id">The scraper configuration ID</param>
        /// <returns>The scraper configuration if found, or null</returns>
        Task<ScraperConfigEntity> GetByIdAsync(Guid id);

        /// <summary>
        /// Creates a new scraper configuration
        /// </summary>
        /// <param name="config">The scraper configuration to create</param>
        /// <returns>The created scraper configuration</returns>
        Task<ScraperConfigEntity> CreateAsync(ScraperConfigEntity config);

        /// <summary>
        /// Updates an existing scraper configuration
        /// </summary>
        /// <param name="config">The scraper configuration to update</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        Task<bool> UpdateAsync(ScraperConfigEntity config);

        /// <summary>
        /// Deletes a scraper configuration
        /// </summary>
        /// <param name="id">The ID of the scraper configuration to delete</param>
        /// <returns>True if the deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets logs for a scraper configuration
        /// </summary>
        /// <param name="scraperId">The scraper configuration ID</param>
        /// <param name="limit">The maximum number of logs to return</param>
        /// <returns>A collection of scraper logs</returns>
        Task<IEnumerable<LogEntryEntity>> GetLogsAsync(Guid scraperId, int limit = 100);

        /// <summary>
        /// Adds a log entry for a scraper configuration
        /// </summary>
        /// <param name="log">The log entry to add</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task AddLogAsync(LogEntryEntity log);

        /// <summary>
        /// Gets runs for a scraper configuration
        /// </summary>
        /// <param name="scraperId">The scraper configuration ID</param>
        /// <param name="limit">The maximum number of runs to return</param>
        /// <returns>A collection of scraper runs</returns>
        Task<IEnumerable<ScraperRunEntity>> GetRunsAsync(Guid scraperId, int limit = 10);

        /// <summary>
        /// Adds a run entry for a scraper configuration
        /// </summary>
        /// <param name="run">The run entry to add</param>
        /// <returns>The created scraper run</returns>
        Task<ScraperRunEntity> AddRunAsync(ScraperRunEntity run);

        /// <summary>
        /// Updates a run entry
        /// </summary>
        /// <param name="run">The run entry to update</param>
        /// <returns>True if the update was successful, false otherwise</returns>
        Task<bool> UpdateRunAsync(ScraperRunEntity run);
    }
}
