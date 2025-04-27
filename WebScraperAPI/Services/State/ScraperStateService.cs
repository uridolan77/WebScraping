using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.StateManagement;
using WebScraperApi.Models;
using WebScraper.ContentChange;

namespace WebScraperApi.Services.State
{
    /// <summary>
    /// Service for managing scraper state
    /// </summary>
    public class ScraperStateService : IScraperStateService
    {
        private readonly ILogger<ScraperStateService> _logger;
        private readonly Dictionary<string, ScraperInstance> _scrapers;
        private readonly object _lock = new object();
        
        public ScraperStateService(ILogger<ScraperStateService> logger)
        {
            _logger = logger;
            _scrapers = new Dictionary<string, ScraperInstance>();
        }
        
        /// <summary>
        /// Gets the current scraper instances
        /// </summary>
        public Dictionary<string, ScraperInstance> GetScrapers()
        {
            lock (_lock)
            {
                return _scrapers;
            }
        }
        
        /// <summary>
        /// Gets a specific scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>Scraper instance if found, null otherwise</returns>
        public ScraperInstance GetScraperInstance(string id)
        {
            lock (_lock)
            {
                if (_scrapers.TryGetValue(id, out var instance))
                {
                    return instance;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Adds or updates a scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="instance">Scraper instance</param>
        public void AddOrUpdateScraper(string id, ScraperInstance instance)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Scraper ID cannot be null or empty", nameof(id));
            }
            
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            
            lock (_lock)
            {
                _scrapers[id] = instance;
            }
            
            _logger.LogInformation($"Added or updated scraper instance: {id}");
        }
        
        /// <summary>
        /// Removes a scraper instance
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool RemoveScraper(string id)
        {
            lock (_lock)
            {
                if (_scrapers.ContainsKey(id))
                {
                    _scrapers.Remove(id);
                    _logger.LogInformation($"Removed scraper instance: {id}");
                    return true;
                }
            }
            
            _logger.LogWarning($"Failed to remove scraper instance: {id} (not found)");
            return false;
        }
        
        /// <summary>
        /// Compresses stored content for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>Operation result</returns>
        public async Task<object> CompressStoredContentAsync(string id)
        {
            var instance = GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot compress content: scraper {id} not found");
                return new { Success = false, Message = "Scraper not found" };
            }
            
            if (instance.StateManager == null)
            {
                _logger.LogWarning($"Cannot compress content: scraper {id} has no state manager");
                return new { Success = false, Message = "Scraper has no state manager" };
            }
            
            try
            {
                // Use the persistent state manager to compress content
                var compressedFile = await CompressContentFolderAsync(id, instance.Config.OutputDirectory);
                
                return new
                {
                    Success = true,
                    Message = $"Content compressed successfully",
                    CompressedFile = compressedFile
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error compressing content for scraper {id}");
                return new { Success = false, Message = $"Error compressing content: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Helper method to compress a folder
        /// </summary>
        private async Task<string> CompressContentFolderAsync(string id, string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                _logger.LogWarning($"Content folder not found: {folderPath}");
                return null;
            }
            
            var compressedFile = Path.Combine(
                Path.GetDirectoryName(folderPath),
                $"scraper_{id}_content_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
            
            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(folderPath, compressedFile);
            });
            
            return compressedFile;
        }
        
        /// <summary>
        /// Updates webhook configuration for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="config">Webhook configuration</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UpdateWebhookConfigAsync(string id, WebhookConfig config)
        {
            var instance = GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot update webhook config: scraper {id} not found");
                return false;
            }
            
            try
            {
                // Update the webhook config in the instance
                instance.Config.WebhookUrl = config.WebhookUrl;
                instance.Config.WebhookEnabled = config.Enabled;
                instance.Config.WebhookFormat = config.Format;
                instance.Config.WebhookTriggers = config.Triggers;
                
                _logger.LogInformation($"Updated webhook configuration for scraper {id}");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating webhook configuration for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets analytics data from a state manager
        /// </summary>
        /// <param name="stateManager">The state manager</param>
        /// <returns>Dictionary of analytics data</returns>
        public async Task<Dictionary<string, object>> GetStateManagerAnalyticsAsync(PersistentStateManager stateManager)
        {
            if (stateManager == null)
            {
                return new Dictionary<string, object>();
            }
            
            try
            {
                var result = new Dictionary<string, object>();
                
                // Get stats about stored content
                var contentStats = await Task.Run(() => 
                {
                    try
                    {
                        var stats = stateManager.GetStorageStatistics();
                        return stats;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting storage statistics");
                        return null;
                    }
                });
                
                if (contentStats != null)
                {
                    result["totalContentItems"] = contentStats.TotalContentItems;
                    result["totalContentVersions"] = contentStats.TotalContentVersions;
                    result["averageVersionsPerItem"] = contentStats.AverageVersionsPerItem;
                    result["totalStorageSizeBytes"] = contentStats.TotalStorageSizeBytes;
                    result["lastContentUpdate"] = contentStats.LastContentUpdate;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting state manager analytics");
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }
        
        /// <summary>
        /// Gets detected content changes for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="since">Optional date filter</param>
        /// <param name="limit">Max number of changes to return</param>
        /// <returns>List of detected changes</returns>
        public async Task<List<object>> GetDetectedChangesAsync(string id, DateTime? since = null, int limit = 100)
        {
            var instance = GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get detected changes: scraper {id} not found");
                return new List<object>();
            }
            
            if (instance.StateManager == null)
            {
                _logger.LogWarning($"Cannot get detected changes: scraper {id} has no state manager");
                return new List<object>();
            }
            
            try
            {
                // Use the persistent state manager to get change history
                var changes = await Task.Run(() => 
                {
                    try
                    {
                        return instance.StateManager.GetContentChanges(since, limit);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting content changes for scraper {id}");
                        return new List<ContentChangeRecord>();
                    }
                });
                
                // Convert to anonymous objects
                var result = changes.Select(c => new
                {
                    url = c.Url,
                    changeType = c.ChangeType.ToString(),
                    detectedAt = c.DetectedAt,
                    significance = c.Significance,
                    changeDetails = c.ChangeDetails
                }).Cast<object>().ToList();
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting detected changes for scraper {id}");
                return new List<object>();
            }
        }
        
        /// <summary>
        /// Gets processed documents for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="documentType">Optional document type filter</param>
        /// <param name="page">Page number (1-based)</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged list of documents</returns>
        public async Task<PagedDocumentResult> GetProcessedDocumentsAsync(string id, string documentType = null, int page = 1, int pageSize = 20)
        {
            var instance = GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get processed documents: scraper {id} not found");
                return new PagedDocumentResult { TotalCount = 0 };
            }
            
            if (instance.StateManager == null)
            {
                _logger.LogWarning($"Cannot get processed documents: scraper {id} has no state manager");
                return new PagedDocumentResult { TotalCount = 0 };
            }
            
            try
            {
                // Use the persistent state manager to get processed documents
                var result = await Task.Run(() => 
                {
                    try
                    {
                        var docs = instance.StateManager.GetProcessedDocuments(documentType, page, pageSize);
                        
                        // Convert to a format suitable for API
                        return new PagedDocumentResult
                        {
                            TotalCount = docs.TotalCount,
                            Documents = docs.Documents.Select(d => new
                            {
                                id = d.Id,
                                url = d.Url,
                                title = d.Title,
                                type = d.DocumentType,
                                processedAt = d.ProcessedAt,
                                size = d.ContentSizeBytes,
                                metadata = d.Metadata
                            }).Cast<object>().ToList()
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error getting processed documents for scraper {id}");
                        return new PagedDocumentResult { TotalCount = 0 };
                    }
                });
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting processed documents for scraper {id}");
                return new PagedDocumentResult { TotalCount = 0 };
            }
        }
    }
}