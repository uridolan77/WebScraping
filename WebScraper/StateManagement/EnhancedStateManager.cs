using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.Interfaces;
using WebScraper.Processing;
using WebScraper.StateManagement;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Implementation of IStateManager that wraps the PersistentStateManager
    /// </summary>
    public class EnhancedStateManager : IStateManager
    {
        private readonly PersistentStateManager _stateManager;
        private readonly Action<string> _logAction;
        private readonly string _connectionString;
        
        public EnhancedStateManager(string connectionString, Action<string> logAction)
        {
            _connectionString = connectionString;
            _logAction = logAction ?? (msg => { });
            _stateManager = new PersistentStateManager(Path.GetDirectoryName(connectionString), logAction);
        }
        
        public async Task InitializeAsync()
        {
            try
            {
                await _stateManager.InitializeAsync();
                _logAction("State manager initialized successfully");
            }
            catch (Exception ex)
            {
                _logAction($"Error initializing state manager: {ex.Message}");
                throw;
            }
        }
        
        public Task SaveScraperStateAsync(WebScraper.StateManagement.ScraperState state)
        {
            try
            {
                return _stateManager.SaveScraperStateAsync(state);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving scraper state: {ex.Message}");
                return Task.CompletedTask;
            }
        }
        
        public Task<WebScraper.StateManagement.ScraperState> GetScraperStateAsync(string scraperId)
        {
            try
            {
                return _stateManager.GetScraperStateAsync(scraperId);
            }
            catch (Exception ex)
            {
                _logAction($"Error getting scraper state: {ex.Message}");
                return Task.FromResult(new WebScraper.StateManagement.ScraperState { ScraperId = scraperId, Status = "Error" });
            }
        }
        
        public async Task<bool> HasUrlBeenVisitedAsync(string scraperId, string url)
        {
            try
            {
                return await _stateManager.HasUrlBeenVisitedAsync(scraperId, url);
            }
            catch (Exception ex)
            {
                _logAction($"Error checking URL visited status: {ex.Message}");
                return false;
            }
        }
        
        public async Task MarkUrlVisitedAsync(string scraperId, string url, int statusCode, int responseTimeMs)
        {
            try
            {
                await _stateManager.MarkUrlVisitedAsync(scraperId, url, statusCode, responseTimeMs);
            }
            catch (Exception ex)
            {
                _logAction($"Error marking URL as visited: {ex.Message}");
            }
        }
        
        public async Task<bool> SaveContentItemAsync(Processing.ContentItem item)
        {
            if (_stateManager != null)
            {
                try
                {
                    // Convert the Processing.ContentItem to StateManagement.ContentItem for compatibility
                    var convertedItem = new WebScraper.StateManagement.ContentItem
                    {
                        Url = item.Url,
                        Title = item.Title,
                        ScraperId = item.ScraperId,
                        LastStatusCode = item.LastStatusCode,
                        ContentType = item.ContentType,
                        IsReachable = item.IsReachable,
                        RawContent = item.RawContent,
                        ContentHash = item.ContentHash,
                        IsRegulatoryContent = item.IsRegulatoryContent
                        // Note: CapturedAt is handled implicitly by the implementation
                    };
                    
                    return await _stateManager.SaveContentItemAsync(convertedItem);
                }
                catch (Exception ex)
                {
                    _logAction($"Error saving content item: {ex.Message}");
                    return false;
                }
            }
            
            return false;
        }
        
        public async Task<(bool Found, WebScraper.Interfaces.ContentItem Version)> GetLatestContentVersionAsync(string url)
        {
            try
            {
                var result = await _stateManager.GetLatestContentVersionAsync(url);
                
                if (!result.Found || result.Version == null)
                {
                    return (false, null);
                }
                
                // StateManagement.ContentItem already implements Interfaces.ContentItem,
                // so we can just return it directly as the interface type
                return (true, result.Version);
            }
            catch (Exception ex)
            {
                _logAction($"Error getting content version: {ex.Message}");
                return (false, null);
            }
        }
        
        public async Task<Dictionary<string, object>> GetStorageStatisticsAsync()
        {
            try
            {
                // Implement basic statistics for the storage
                var stats = new Dictionary<string, object>
                {
                    ["totalVersions"] = 0,
                    ["totalUrls"] = 0,
                    ["lastUpdated"] = DateTime.Now
                };
                
                // Count the number of version files
                if (_stateManager != null && Directory.Exists(Path.GetDirectoryName(_connectionString)))
                {
                    var stateDir = Path.Combine(Path.GetDirectoryName(_connectionString), "state");
                    if (Directory.Exists(stateDir))
                    {
                        var versionFiles = Directory.GetFiles(stateDir, "*_*.json");
                        stats["totalVersions"] = versionFiles.Length;
                        
                        // Estimate number of unique URLs
                        var uniqueUrlPrefixes = new HashSet<string>();
                        foreach (var file in versionFiles)
                        {
                            var fileName = Path.GetFileNameWithoutExtension(file);
                            var parts = fileName.Split('_');
                            if (parts.Length > 0)
                            {
                                uniqueUrlPrefixes.Add(parts[0]);
                            }
                        }
                        stats["totalUrls"] = uniqueUrlPrefixes.Count;
                        
                        // Get storage size
                        var directoryInfo = new DirectoryInfo(stateDir);
                        var size = directoryInfo.GetFiles("*.json", SearchOption.AllDirectories)
                            .Sum(file => file.Length);
                        stats["storageSize"] = size;
                        stats["storageSizeMB"] = Math.Round(size / 1024.0 / 1024.0, 2);
                    }
                }
                
                return stats;
            }
            catch (Exception ex)
            {
                _logAction($"Error getting storage statistics: {ex.Message}");
                return new Dictionary<string, object>
                {
                    ["error"] = ex.Message
                };
            }
        }

        // Fix for method signature - Changing return type to Task instead of Task<bool>
        public Task SaveContentVersionAsync(WebScraper.Interfaces.ContentItem item, int maxVersions = 10)
        {
            if (_stateManager != null)
            {
                try
                {
                    // Convert interface ContentItem to concrete ContentItem
                    var contentItem = new WebScraper.StateManagement.ContentItem
                    {
                        Url = item.Url,
                        Title = item.Title,
                        ScraperId = item.ScraperId,
                        LastStatusCode = item.LastStatusCode,
                        ContentType = item.ContentType,
                        IsReachable = item.IsReachable,
                        RawContent = item.RawContent,
                        ContentHash = item.ContentHash,
                        IsRegulatoryContent = item.IsRegulatoryContent,
                        CapturedAt = DateTime.Now // Set current time as capture time
                    };
                    
                    return _stateManager.SaveContentVersionAsync(contentItem, maxVersions);
                }
                catch (Exception ex)
                {
                    _logAction($"Error saving content version: {ex.Message}");
                    return Task.CompletedTask;
                }
            }
            
            return Task.CompletedTask;
        }
    }
}