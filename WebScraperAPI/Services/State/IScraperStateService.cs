using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.StateManagement;
using WebScraperApi.Models;

namespace WebScraperApi.Services.State
{
    /// <summary>
    /// Interface for scraper state services
    /// </summary>
    public interface IScraperStateService
    {
        /// <summary>
        /// Gets the current scraper instances
        /// </summary>
        Dictionary<string, ScraperInstance> GetScrapers();
        
        /// <summary>
        /// Gets a specific scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>Scraper instance if found, null otherwise</returns>
        ScraperInstance GetScraperInstance(string id);
        
        /// <summary>
        /// Adds or updates a scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="instance">Scraper instance</param>
        void AddOrUpdateScraper(string id, ScraperInstance instance);
        
        /// <summary>
        /// Removes a scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>True if successful, false otherwise</returns>
        bool RemoveScraper(string id);
        
        /// <summary>
        /// Compresses stored content for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>Operation result</returns>
        Task<object> CompressStoredContentAsync(string id);
        
        /// <summary>
        /// Updates webhook configuration for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="config">Webhook configuration</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateWebhookConfigAsync(string id, WebhookConfig config);
        
        /// <summary>
        /// Gets analytics data from a state manager
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <returns>Dictionary of analytics data</returns>
        Task<Dictionary<string, object>> GetStateManagerAnalyticsAsync(PersistentStateManager stateManager);
        
        /// <summary>
        /// Gets detected content changes for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="since">Optional date filter</param>
        /// <param name="limit">Max number of changes to return</param>
        /// <returns>List of detected changes</returns>
        Task<List<object>> GetDetectedChangesAsync(string id, DateTime? since = null, int limit = 100);
        
        /// <summary>
        /// Gets processed documents for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="documentType">Optional document type filter</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged list of documents</returns>
        Task<PagedDocumentResult> GetProcessedDocumentsAsync(string id, string documentType = null, int page = 1, int pageSize = 20);
    }
    
    /// <summary>
    /// Result object for paged document queries
    /// </summary>
    public class PagedDocumentResult
    {
        /// <summary>
        /// Total count of all documents matching the filter
        /// </summary>
        public int TotalCount { get; set; }
        
        /// <summary>
        /// The documents in the current page
        /// </summary>
        public List<object> Documents { get; set; } = new List<object>();
    }
}