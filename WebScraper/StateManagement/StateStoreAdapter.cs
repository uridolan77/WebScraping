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
        
        public async Task<PageVersion> GetLatestVersionAsync(string url)
        {
            // Convert from internal to interface type
            var version = await _stateManager.GetLatestVersionAsync(url);
            if (version == null)
            {
                return null;
            }
            
            // Already returning the correct type
            return version;
        }
        
        public Task SaveVersionAsync(PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            return _stateManager.SaveVersionAsync(version);
        }
        
        public async Task<List<PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            // The internal implementation returns the correct type already
            var history = await _stateManager.GetVersionHistoryAsync(url, maxVersions);
            return history.ToList(); // Ensure we return a List<T> not just IEnumerable<T>
        }
    }
}