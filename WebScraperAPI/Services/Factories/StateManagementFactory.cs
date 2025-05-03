using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryFramework.Interfaces;
using WebScraper.StateManagement;
using IRF = WebScraper.RegulatoryFramework.Interfaces; // Add alias for clarity

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating state management components
    /// </summary>
    public class StateManagementFactory
    {
        private readonly ILogger<StateManagementFactory> _logger;

        public StateManagementFactory(ILogger<StateManagementFactory> logger)
        {
            _logger = logger;
        }

        public IStateStore CreateStateStore(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Creating default state store");
                return new DefaultStateStore(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating state store");
                logAction($"Error creating state store: {ex.Message}");
                return new DefaultStateStore(_logger);
            }
        }

        // Simple implementation of IStateStore
        private class DefaultStateStore : IStateStore
        {
            private readonly Dictionary<string, object> _store = new Dictionary<string, object>();
            private readonly Dictionary<string, List<IRF.PageVersion>> _versions = new Dictionary<string, List<IRF.PageVersion>>();
            private readonly ILogger _logger;

            public DefaultStateStore(ILogger logger)
            {
                _logger = logger;
            }

            // Fix the constraints to match the interface exactly - remove the constraint
            public async Task<T> GetAsync<T>(string key)
            {
                try
                {
                    await Task.Delay(1); // Make it truly async
                    if (string.IsNullOrEmpty(key))
                    {
                        _logger.LogWarning("Attempt to get value with null or empty key");
                        return default;
                    }
                    
                    if (_store.TryGetValue(key, out var value) && value is T typedValue)
                    {
                        return typedValue;
                    }
                    return default;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting value for key {key}");
                    return default;
                }
            }

            public async Task SetAsync<T>(string key, T value)
            {
                try
                {
                    await Task.Delay(1); // Make it truly async
                    
                    if (string.IsNullOrEmpty(key))
                    {
                        _logger.LogWarning("Attempt to store value with null or empty key");
                        return;
                    }
                    
                    // Ensure we're not trying to store a null value
                    if (value != null)
                    {
                        _store[key] = value;
                    }
                    else
                    {
                        _logger.LogWarning($"Attempt to store null value for key {key}");
                        // Remove the key if value is null to avoid null reference issues
                        if (_store.ContainsKey(key))
                        {
                            _store.Remove(key);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error setting value for key {key}");
                }
            }

            public async Task<IRF.PageVersion> GetLatestVersionAsync(string url)
            {
                await Task.Delay(1); // Make it truly async
                
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("Attempt to get latest version with null or empty URL");
                    return new IRF.PageVersion { 
                        Url = string.Empty, 
                        Id = Guid.NewGuid().ToString(),
                        CaptureDate = DateTime.UtcNow,
                        Content = string.Empty,
                        ContentType = "text/html",
                        Metadata = new Dictionary<string, string>()
                    };
                }
                
                if (_versions.TryGetValue(url, out var versions) && versions.Count > 0)
                {
                    return versions[versions.Count - 1];
                }
                
                // Return an empty version rather than null
                return new IRF.PageVersion { 
                    Url = url,
                    Id = Guid.NewGuid().ToString(),
                    CaptureDate = DateTime.UtcNow,
                    Content = string.Empty,
                    ContentType = "text/html",
                    Metadata = new Dictionary<string, string>()
                };
            }

            public async Task SaveVersionAsync(IRF.PageVersion version)
            {
                await Task.Delay(1); // Make it truly async
                
                if (version == null)
                {
                    _logger.LogWarning("Attempt to save null PageVersion");
                    return;
                }
                
                if (string.IsNullOrEmpty(version.Url))
                {
                    _logger.LogWarning("Attempt to save PageVersion with null or empty URL");
                    return;
                }
                
                if (!_versions.TryGetValue(version.Url, out var versions))
                {
                    versions = new List<IRF.PageVersion>();
                    _versions[version.Url] = versions;
                }
                versions.Add(version);
            }

            public async Task<List<IRF.PageVersion>> GetVersionHistoryAsync(string url, int limit = 10)
            {
                await Task.Delay(1); // Make it truly async
                
                if (string.IsNullOrEmpty(url))
                {
                    _logger.LogWarning("Attempt to get version history with null or empty URL");
                    return new List<IRF.PageVersion>();
                }
                
                if (_versions.TryGetValue(url, out var versions))
                {
                    if (versions.Count <= limit)
                    {
                        return new List<IRF.PageVersion>(versions);
                    }
                    else
                    {
                        return versions.GetRange(versions.Count - limit, limit);
                    }
                }
                return new List<IRF.PageVersion>();
            }
        }
    }
}