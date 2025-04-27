using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebScraper.Interfaces;
using WebScraper.Processing;
using WebScraper.RegulatoryFramework.Interfaces;

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
        
        public async Task SaveScraperStateAsync(ScraperState state)
        {
            try
            {
                // The WebScraperApi.Models.ScraperState is already the correct type
                // since we're passing in a WebScraper.StateManagement.ScraperState
                await _stateManager.SaveScraperStateAsync(state);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving scraper state: {ex.Message}");
            }
        }
        
        public async Task<ScraperState> GetScraperStateAsync(string scraperId)
        {
            try
            {
                var state = await _stateManager.GetScraperStateAsync(scraperId);
                return state;
            }
            catch (Exception ex)
            {
                _logAction($"Error getting scraper state: {ex.Message}");
                return new ScraperState { ScraperId = scraperId, Status = "Error" };
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
        
        public async Task SaveContentVersionAsync(ContentItem content, int maxVersions)
        {
            try
            {
                await _stateManager.SaveContentVersionAsync(content, maxVersions);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving content version: {ex.Message}");
            }
        }
        
        public async Task<(bool Found, ContentItem Version)> GetLatestContentVersionAsync(string url)
        {
            try
            {
                return await _stateManager.GetLatestContentVersionAsync(url);
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
    }
}