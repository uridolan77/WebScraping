using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper.StateManagement;
using WebScraperApi.Models;

namespace WebScraperApi.Services.State
{
    /// <summary>
    /// Extension methods for PersistentStateManager
    /// </summary>
    public static class PersistentStateManagerExtensions
    {
        /// <summary>
        /// Gets storage statistics for the state manager
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <returns>Storage statistics</returns>
        public static StorageStatistics GetStorageStatistics(this PersistentStateManager stateManager)
        {
            // This is a mock implementation since we don't have access to the actual implementation
            return new StorageStatistics
            {
                TotalContentItems = 0,
                TotalContentVersions = 0,
                AverageVersionsPerItem = 0,
                TotalStorageSizeBytes = 0,
                LastContentUpdate = DateTime.Now
            };
        }
        
        /// <summary>
        /// Gets content changes from the state manager
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <param name="since">Optional date filter</param>
        /// <param name="limit">Max number of changes to return</param>
        /// <returns>List of content changes</returns>
        public static List<ContentChangeRecord> GetContentChanges(this PersistentStateManager stateManager, DateTime? since = null, int limit = 100)
        {
            // This is a mock implementation since we don't have access to the actual implementation
            return new List<ContentChangeRecord>();
        }
        
        /// <summary>
        /// Gets processed documents from the state manager
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <param name="documentType">Optional document type filter</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged document result</returns>
        public static PagedDocumentResult GetProcessedDocuments(this PersistentStateManager stateManager, string documentType = null, int page = 1, int pageSize = 20)
        {
            // This is a mock implementation since we don't have access to the actual implementation
            return new PagedDocumentResult
            {
                TotalCount = 0,
                Documents = new List<object>()
            };
        }
    }
    
    /// <summary>
    /// Storage statistics for a state manager
    /// </summary>
    public class StorageStatistics
    {
        /// <summary>
        /// Total number of content items stored
        /// </summary>
        public int TotalContentItems { get; set; }
        
        /// <summary>
        /// Total number of content versions stored
        /// </summary>
        public int TotalContentVersions { get; set; }
        
        /// <summary>
        /// Average number of versions per content item
        /// </summary>
        public double AverageVersionsPerItem { get; set; }
        
        /// <summary>
        /// Total storage size in bytes
        /// </summary>
        public long TotalStorageSizeBytes { get; set; }
        
        /// <summary>
        /// When content was last updated
        /// </summary>
        public DateTime LastContentUpdate { get; set; }
    }
}
