using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Adapter class that wraps PersistentStateManager to properly implement IStateStore interface
    /// </summary>
    public class StateStoreAdapter : IStateStore
    {
        private readonly PersistentStateManager _stateManager;
        
        public StateStoreAdapter(PersistentStateManager stateManager)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }
        
        public Task<T> GetAsync<T>(string key)
        {
            return _stateManager.GetAsync<T>(key);
        }
        
        public Task SetAsync<T>(string key, T value)
        {
            return _stateManager.SetAsync<T>(key, value);
        }
        
        public async Task<WebScraper.RegulatoryFramework.Interfaces.PageVersion> GetLatestVersionAsync(string url)
        {
            // Convert from internal to interface type
            var internalVersion = await _stateManager.GetLatestContentVersionAsync(url);
            if (!internalVersion.Found || internalVersion.Version == null)
            {
                return null;
            }
            
            // Convert from ContentItem to PageVersion
            return new WebScraper.RegulatoryFramework.Interfaces.PageVersion
            {
                Url = internalVersion.Version.Url,
                Hash = internalVersion.Version.ContentHash,
                CapturedAt = internalVersion.Version.CapturedAt,
                FullContent = internalVersion.Version.RawContent,
                TextContent = internalVersion.Version.RawContent, // Best approximation
                ContentSummary = internalVersion.Version.Title,
                Metadata = new Dictionary<string, object>
                {
                    ["title"] = internalVersion.Version.Title,
                    ["scraperId"] = internalVersion.Version.ScraperId
                }
            };
        }
        
        public Task SaveVersionAsync(WebScraper.RegulatoryFramework.Interfaces.PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            // Convert from PageVersion to ContentItem
            var contentItem = new ContentItem
            {
                Url = version.Url,
                Title = version.Metadata.ContainsKey("title") ? version.Metadata["title"].ToString() : "Untitled",
                ScraperId = version.Metadata.ContainsKey("scraperId") ? version.Metadata["scraperId"].ToString() : "unknown",
                CapturedAt = version.CapturedAt,
                IsReachable = true,
                ContentType = "text/html",
                RawContent = version.FullContent,
                ContentHash = version.Hash
            };
            
            // Use max versions = 10 as default
            return _stateManager.SaveContentVersionAsync(contentItem, 10);
        }
        
        public async Task<List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            // Get the history of content versions
            var results = new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
            
            // We don't have a direct equivalent, so we need to fake this
            var latestVersion = await GetLatestVersionAsync(url);
            if (latestVersion != null)
            {
                results.Add(latestVersion);
            }
            
            return results;
        }
    }
}