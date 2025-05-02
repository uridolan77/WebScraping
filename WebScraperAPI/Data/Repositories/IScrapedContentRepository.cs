using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    /// <summary>
    /// Repository interface for scraped content stored in MongoDB
    /// </summary>
    public interface IScrapedContentRepository
    {
        /// <summary>
        /// Gets scraped content by URL
        /// </summary>
        /// <param name="url">The URL of the scraped content</param>
        /// <returns>The scraped content if found, or null</returns>
        Task<ScrapedContentEntity> GetByUrlAsync(string url);

        /// <summary>
        /// Gets scraped content by ID
        /// </summary>
        /// <param name="id">The ID of the scraped content</param>
        /// <returns>The scraped content if found, or null</returns>
        Task<ScrapedContentEntity> GetByIdAsync(string id);

        /// <summary>
        /// Gets all scraped content for a scraper configuration
        /// </summary>
        /// <param name="scraperId">The scraper configuration ID</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A collection of scraped content</returns>
        Task<(IEnumerable<ScrapedContentEntity> Items, int TotalCount)> GetByScraperIdAsync(
            Guid scraperId, int page = 1, int pageSize = 20);

        /// <summary>
        /// Saves scraped content
        /// </summary>
        /// <param name="content">The scraped content to save</param>
        /// <returns>The saved scraped content</returns>
        Task<ScrapedContentEntity> SaveContentAsync(ScrapedContentEntity content);

        /// <summary>
        /// Adds a new version of content
        /// </summary>
        /// <param name="version">The content version to add</param>
        /// <returns>The added content version</returns>
        Task<ContentVersionEntity> AddVersionAsync(ContentVersionEntity version);

        /// <summary>
        /// Gets versions of content
        /// </summary>
        /// <param name="contentId">The content ID</param>
        /// <param name="limit">The maximum number of versions to return</param>
        /// <returns>A collection of content versions</returns>
        Task<IEnumerable<ContentVersionEntity>> GetVersionsAsync(string contentId, int limit = 5);

        /// <summary>
        /// Searches for content based on a query
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="scraperId">Optional scraper ID to filter by</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <returns>A collection of scraped content matching the query</returns>
        Task<(IEnumerable<ScrapedContentEntity> Items, int TotalCount)> SearchAsync(
            string query, Guid? scraperId = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Gets content that has changed since a specific date
        /// </summary>
        /// <param name="since">The date to check changes from</param>
        /// <param name="scraperId">Optional scraper ID to filter by</param>
        /// <returns>A collection of content that has changed</returns>
        Task<IEnumerable<ScrapedContentEntity>> GetChangedContentSinceAsync(
            DateTime since, Guid? scraperId = null);

        /// <summary>
        /// Deletes scraped content
        /// </summary>
        /// <param name="id">The ID of the content to delete</param>
        /// <returns>True if the deletion was successful, false otherwise</returns>
        Task<bool> DeleteAsync(string id);

        /// <summary>
        /// Deletes all content for a scraper configuration
        /// </summary>
        /// <param name="scraperId">The scraper configuration ID</param>
        /// <returns>The number of deleted items</returns>
        Task<int> DeleteByScraperIdAsync(Guid scraperId);
    }
}
