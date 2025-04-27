// filepath: c:\dev\WebScraping\WebScraper\StateManagement\PersistentStateManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Manages persistent state for the scraper across runs
    /// </summary>
    public class PersistentStateManager : IStateStore
    {
        private readonly string _stateDirectory;
        private readonly Action<string> _logAction;

        public PersistentStateManager(string baseDirectory, Action<string> logAction = null)
        {
            _stateDirectory = Path.Combine(baseDirectory, "state");
            _logAction = logAction ?? (msg => Console.WriteLine(msg));
            
            if (!Directory.Exists(_stateDirectory))
            {
                Directory.CreateDirectory(_stateDirectory);
            }
        }

        /// <summary>
        /// Initialize the state manager
        /// </summary>
        public Task InitializeAsync()
        {
            _logAction("Initializing PersistentStateManager...");
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the state for a scraper by ID
        /// </summary>
        public async Task<ScraperState> GetScraperStateAsync(string scraperId)
        {
            var stateFile = Path.Combine(_stateDirectory, $"{scraperId}.json");
            
            if (!File.Exists(stateFile))
            {
                return new ScraperState
                {
                    ScraperId = scraperId,
                    Status = "New"
                };
            }

            try
            {
                var json = await File.ReadAllTextAsync(stateFile);
                return JsonSerializer.Deserialize<ScraperState>(json) ?? 
                    new ScraperState { ScraperId = scraperId, Status = "Error" };
            }
            catch (Exception ex)
            {
                _logAction($"Error loading scraper state: {ex.Message}");
                return new ScraperState
                {
                    ScraperId = scraperId,
                    Status = "Error"
                };
            }
        }

        /// <summary>
        /// Updates the state for a scraper
        /// </summary>
        public async Task SaveScraperStateAsync(ScraperState state)
        {
            var stateFile = Path.Combine(_stateDirectory, $"{state.ScraperId}.json");
            
            try
            {
                var json = JsonSerializer.Serialize(state);
                await File.WriteAllTextAsync(stateFile, json);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving scraper state: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates the state for a scraper - kept for backward compatibility
        /// </summary>
        public Task UpdateScraperStateAsync(ScraperState state)
        {
            return SaveScraperStateAsync(state);
        }

        /// <summary>
        /// Check if a URL has been visited
        /// </summary>
        public async Task<bool> HasUrlBeenVisitedAsync(string scraperId, string url)
        {
            var state = await GetScraperStateAsync(scraperId);
            return state.ProcessedUrls?.Contains(url) ?? false;
        }

        /// <summary>
        /// Mark a URL as visited
        /// </summary>
        public async Task MarkUrlVisitedAsync(string scraperId, string url, int statusCode, int responseTimeMs)
        {
            var state = await GetScraperStateAsync(scraperId);
            
            if (state.ProcessedUrls == null)
            {
                state.ProcessedUrls = new HashSet<string>();
            }
            
            state.ProcessedUrls.Add(url);
            state.PagesScraped = state.ProcessedUrls.Count;
            
            if (statusCode >= 400)
            {
                state.ErrorCount++;
                state.LastError = $"Error {statusCode} at {url}";
            }
            
            await SaveScraperStateAsync(state);
        }

        /// <summary>
        /// Saves a content item
        /// </summary>
        public async Task<bool> SaveContentVersionAsync(ContentItem item)
        {
            // Call the version with maxVersions parameter, using a default value of 10
            return await SaveContentVersionAsync(item, 10);
        }

        /// <summary>
        /// Gets the latest content version for a URL - implementation for IStateStore
        /// </summary>
        public async Task<RegulatoryFramework.Interfaces.PageVersion> GetLatestVersionAsync(string url)
        {
            var contentFile = Path.Combine(_stateDirectory, $"{ComputeHash(url)}_latest.json");
            
            if (!File.Exists(contentFile))
            {
                return null;
            }

            try
            {
                var json = await File.ReadAllTextAsync(contentFile);
                var localVersion = JsonSerializer.Deserialize<WebScraper.StateManagement.PageVersion>(json);
                
                // Convert to the interface's PageVersion type
                if (localVersion != null)
                {
                    return new RegulatoryFramework.Interfaces.PageVersion
                    {
                        Url = localVersion.Url,
                        Hash = localVersion.ContentHash,
                        CapturedAt = localVersion.VersionDate,
                        FullContent = localVersion.Content,
                        TextContent = localVersion.TextContent,
                        ContentSummary = localVersion.TextContent?.Substring(0, Math.Min(200, localVersion.TextContent?.Length ?? 0)) ?? "",
                        Metadata = new Dictionary<string, object>(localVersion.Metadata ?? 
                            new Dictionary<string, object>())
                    };
                }
                return null;
            }
            catch (Exception ex)
            {
                _logAction($"Error getting latest version: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Gets the latest content version tuple for backwards compatibility
        /// </summary>
        public async Task<(bool Found, ContentItem Version)> GetLatestContentVersionAsync(string url)
        {
            var version = await GetLatestVersionAsync(url);
            
            if (version == null)
            {
                return (false, null);
            }
            
            // Convert the PageVersion to ContentItem
            var contentItem = new ContentItem
            {
                Url = version.Url,
                ContentHash = version.Hash,
                RawContent = version.FullContent,
                Title = version.Metadata?.ContainsKey("title") == true ? version.Metadata["title"]?.ToString() : "",
                ScraperId = version.Metadata?.ContainsKey("scraperId") == true ? version.Metadata["scraperId"]?.ToString() : ""
            };
            
            return (true, contentItem);
        }

        /// <summary>
        /// Saves a content version - implementation for IStateStore
        /// </summary>
        public async Task SaveVersionAsync(RegulatoryFramework.Interfaces.PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            // Convert to our internal PageVersion structure
            var localVersion = new WebScraper.StateManagement.PageVersion
            {
                Url = version.Url,
                ContentHash = version.Hash,
                VersionDate = version.CapturedAt,
                Content = version.FullContent,
                TextContent = version.TextContent,
                Metadata = version.Metadata
            };
            
            var contentFile = Path.Combine(_stateDirectory, $"{ComputeHash(version.Url)}_latest.json");
            
            try
            {
                var json = JsonSerializer.Serialize(localVersion);
                await File.WriteAllTextAsync(contentFile, json);

                // Store historical version
                var historyFile = Path.Combine(_stateDirectory, $"{ComputeHash(version.Url)}_{DateTime.Now.Ticks}.json");
                await File.WriteAllTextAsync(historyFile, json);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving content version: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets version history for a URL - implementation for IStateStore
        /// </summary>
        public async Task<List<RegulatoryFramework.Interfaces.PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            var prefix = ComputeHash(url);
            var historyFiles = Directory.GetFiles(_stateDirectory, $"{prefix}_*.json")
                .Where(f => !f.EndsWith("_latest.json"))
                .OrderByDescending(f => f)
                .Take(maxVersions);
                
            var result = new List<RegulatoryFramework.Interfaces.PageVersion>();
            
            foreach (var file in historyFiles)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file);
                    var localVersion = JsonSerializer.Deserialize<WebScraper.StateManagement.PageVersion>(json);
                    
                    if (localVersion != null)
                    {
                        result.Add(new RegulatoryFramework.Interfaces.PageVersion
                        {
                            Url = localVersion.Url,
                            Hash = localVersion.ContentHash,
                            CapturedAt = localVersion.VersionDate,
                            FullContent = localVersion.Content,
                            TextContent = localVersion.TextContent,
                            ContentSummary = localVersion.TextContent?.Substring(0, Math.Min(200, localVersion.TextContent?.Length ?? 0)) ?? "",
                            Metadata = new Dictionary<string, object>(localVersion.Metadata ?? 
                                new Dictionary<string, object>())
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logAction($"Error loading version history: {ex.Message}");
                }
            }
            
            return result;
        }

        /// <summary>
        /// Get value from store by key - implementation for IStateStore
        /// </summary>
        public async Task<T> GetAsync<T>(string key)
        {
            var keyFile = Path.Combine(_stateDirectory, $"key_{ComputeHash(key)}.json");
            
            if (!File.Exists(keyFile))
            {
                return default;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(keyFile);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logAction($"Error getting value for key {key}: {ex.Message}");
                return default;
            }
        }

        /// <summary>
        /// Set value in store by key - implementation for IStateStore
        /// </summary>
        public async Task SetAsync<T>(string key, T value)
        {
            var keyFile = Path.Combine(_stateDirectory, $"key_{ComputeHash(key)}.json");
            
            try
            {
                var json = JsonSerializer.Serialize(value);
                await File.WriteAllTextAsync(keyFile, json);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving value for key {key}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Computes a safe filename hash from a string
        /// </summary>
        private string ComputeHash(string input)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return Convert.ToBase64String(bytes)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .Replace("=", "");
            }
        }
        
        /// <summary>
        /// Add CapturedAt property check and conversion
        /// </summary>
        private DateTime GetCapturedAt(ContentItem item)
        {
            // Handle legacy ContentItem vs Processing.ContentItem
            if (item is Processing.ContentItem processingItem)
            {
                return processingItem.CapturedAt;
            }
            
            // Use reflection to check for property
            var capturedAtProperty = item.GetType().GetProperty("CapturedAt");
            if (capturedAtProperty != null)
            {
                return (DateTime)capturedAtProperty.GetValue(item);
            }
            
            // Fallback to current time
            return DateTime.Now;
        }
        
        /// <summary>
        /// Saves a content item to the storage
        /// </summary>
        public async Task<bool> SaveContentItemAsync(WebScraper.Interfaces.ContentItem item)
        {
            try
            {
                if (item == null)
                {
                    _logAction($"Cannot save null content item");
                    return false;
                }
                
                // Convert to our internal ContentItem type
                var contentItem = new ContentItem
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
                    CapturedAt = GetCapturedAt(item)
                };
                
                // Use the existing method to save the content
                return await SaveContentVersionAsync(contentItem) != null;
            }
            catch (Exception ex)
            {
                _logAction($"Error saving content item: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Saves a content version with max versions parameter
        /// </summary>
        public async Task SaveContentVersionAsync(ContentItem contentItem, int maxVersions = 10)
        {
            string versionFolder = GetVersionFolder(contentItem.Url);
            Directory.CreateDirectory(versionFolder);

            // Create a PageVersion object
            PageVersion version = new PageVersion
            {
                Url = contentItem.Url,
                ContentHash = contentItem.ContentHash,
                Timestamp = DateTime.Now,
                Content = contentItem.RawContent,
                TextContent = contentItem.TextContent
            };

            // Maintain a history of versions
            await SaveVersionHistoryAsync(contentItem.Url, version, maxVersions);

            // Store version to disk
            string versionFilePath = GetVersionFilePath(version);
            string versionFileName = Path.GetFileName(versionFilePath);
            string metaFilePath = Path.Combine(versionFolder, $"{versionFileName}.meta.json");
            
            File.WriteAllText(versionFilePath, contentItem.RawContent);
            
            // Save metadata
            string json = System.Text.Json.JsonSerializer.Serialize(version);
            File.WriteAllText(metaFilePath, json);
            
            _logAction?.Invoke($"Saved version {versionFilePath}");
        }
    }

    /// <summary>
    /// Represents the current state of a scraper
    /// </summary>
    public class ScraperState
    {
        public string ScraperId { get; set; }
        public string Status { get; set; }
        public DateTime LastRunStartTime { get; set; }
        public DateTime? LastRunEndTime { get; set; }
        public DateTime? LastSuccessfulRunTime { get; set; }
        public int PagesScraped { get; set; }
        public int ErrorCount { get; set; }
        public string LastError { get; set; }
        public string ProgressData { get; set; }
        public string ConfigSnapshot { get; set; }
        public HashSet<string> ProcessedUrls { get; set; } = new HashSet<string>();
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
    
    /// <summary>
    /// Internal PageVersion class for state management
    /// </summary>
    internal class PageVersion
    {
        public string Url { get; set; }
        public string Content { get; set; }
        public string TextContent { get; set; }
        public string ContentHash { get; set; }
        public DateTime VersionDate { get; set; }
        public string VersionId { get; set; } = Guid.NewGuid().ToString();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}