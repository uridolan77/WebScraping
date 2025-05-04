// filepath: c:\dev\WebScraping\WebScraper\StateManagement\PersistentStateManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
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

        public PersistentStateManager(string baseDirectory, Action<string>? logAction = null)
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
            // Create a timestamped filename for the state file
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var stateFile = Path.Combine(_stateDirectory, $"{state.ScraperId}_{timestamp}.json");

            _logAction($"Saving scraper state to timestamped file: {stateFile}");

            try
            {
                var json = JsonSerializer.Serialize(state);
                await TrySaveFileWithRetryAsync(stateFile, json);

                // Also save a "latest" version for backward compatibility
                var latestStateFile = Path.Combine(_stateDirectory, $"{state.ScraperId}_latest.json");
                await TrySaveFileWithRetryAsync(latestStateFile, json);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving scraper state: {ex.Message}");
            }
        }

        /// <summary>
        /// Tries to save a file with retry logic and alternative file creation if needed
        /// </summary>
        private async Task TrySaveFileWithRetryAsync(string filePath, string content, int maxRetries = 3)
        {
            Exception lastException = null;

            // Try a few times with the original file
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, content);
                    return; // Success
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    lastException = ex;
                    _logAction($"File {filePath} is locked, retrying in 100ms... (Attempt {i+1}/{maxRetries})");
                    await Task.Delay(100); // Wait a bit before retrying
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _logAction($"Error writing to file {filePath}: {ex.Message}");
                    break; // Don't retry for other types of exceptions
                }
            }

            // If we couldn't save to the original file, create an alternative file
            try
            {
                string directory = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string alternativeFilePath = Path.Combine(directory, $"{fileName}_{timestamp}{extension}");

                _logAction($"Creating alternative file: {alternativeFilePath}");
                await File.WriteAllTextAsync(alternativeFilePath, content);

                // Log success with alternative file
                _logAction($"Successfully saved to alternative file: {alternativeFilePath}");
            }
            catch (Exception ex)
            {
                _logAction($"Failed to create alternative file: {ex.Message}");
                throw new AggregateException($"Failed to save file after retries and alternative file creation", lastException, ex);
            }
        }

        /// <summary>
        /// Checks if the exception is due to a locked file
        /// </summary>
        private bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & 0xFFFF;
            return errorCode == 32 || errorCode == 33 || errorCode == 0x20; // 32=ERROR_SHARING_VIOLATION, 33=ERROR_LOCK_VIOLATION
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
            await SaveContentVersionAsync(item, 10);
            return true;
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
        public async Task<(bool Found, WebScraper.ContentItem? Version)> GetLatestContentVersionAsync(string url)
        {
            var version = await GetLatestVersionAsync(url);

            if (version == null)
            {
                return (false, null);
            }

            // Convert the PageVersion to ContentItem
            var contentItem = new WebScraper.ContentItem
            {
                Url = version.Url,
                ContentHash = version.Hash,
                RawContent = version.FullContent,
                TextContent = version.TextContent,
                Title = version.Metadata?.ContainsKey("title") == true ? version.Metadata["title"]?.ToString() ?? string.Empty : string.Empty,
                ScraperId = version.Metadata?.ContainsKey("scraperId") == true ? version.Metadata["scraperId"]?.ToString() ?? string.Empty : string.Empty
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
                await TrySaveFileWithRetryAsync(contentFile, json);

                // Store historical version
                var historyFile = Path.Combine(_stateDirectory, $"{ComputeHash(version.Url)}_{DateTime.Now.Ticks}.json");
                await TrySaveFileWithRetryAsync(historyFile, json);
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
                return default!; // Use non-null assertion to match interface contract
            }

            try
            {
                var json = await File.ReadAllTextAsync(keyFile);
                // Use non-null assertion to match interface contract
                return JsonSerializer.Deserialize<T>(json) ?? default!;
            }
            catch (Exception ex)
            {
                _logAction($"Error getting value for key {key}: {ex.Message}");
                return default!; // Use non-null assertion to match interface contract
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
                await TrySaveFileWithRetryAsync(keyFile, json);
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
        /// Get CapturedAt property from ContentItem
        /// </summary>
        private DateTime GetCapturedAt(WebScraper.ContentItem item)
        {
            if (item == null)
                return DateTime.Now;

            return item.CapturedAt;
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

                // Convert to canonical ContentItem if needed
                WebScraper.ContentItem contentItem;

                if (item is WebScraper.ContentItem canonicalItem)
                {
                    contentItem = canonicalItem;
                }
                else
                {
                    contentItem = WebScraper.ContentItem.FromInterface(item);
                }

                // Use the existing method to save the content
                return await SaveContentVersionAsync(contentItem);
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
        public async Task SaveContentVersionAsync(WebScraper.ContentItem contentItem, int maxVersions = 10)
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

            try
            {
                // Use async methods with retry
                await TrySaveFileWithRetryAsync(versionFilePath, contentItem.RawContent);

                // Save metadata
                string json = System.Text.Json.JsonSerializer.Serialize(version);
                await TrySaveFileWithRetryAsync(metaFilePath, json);
            }
            catch (Exception ex)
            {
                _logAction($"Error saving version files: {ex.Message}");
            }

            _logAction?.Invoke($"Saved version {versionFilePath}");
        }

        /// <summary>
        /// Gets version folder path for a URL
        /// </summary>
        private string GetVersionFolder(string url)
        {
            string urlHash = ComputeHash(url);
            return Path.Combine(_stateDirectory, urlHash);
        }

        /// <summary>
        /// Gets version file path for a page version
        /// </summary>
        private string GetVersionFilePath(PageVersion version)
        {
            string versionFolder = GetVersionFolder(version.Url);
            return Path.Combine(versionFolder, $"{version.VersionDate.Ticks}_{version.ContentHash}.html");
        }

        /// <summary>
        /// Saves version history, keeping up to maxVersions
        /// </summary>
        private async Task SaveVersionHistoryAsync(string url, PageVersion version, int maxVersions)
        {
            try
            {
                string versionFolder = GetVersionFolder(url);

                // Get existing versions sorted by date (newest first)
                var versionFiles = Directory.GetFiles(versionFolder, "*.html")
                    .OrderByDescending(f => Path.GetFileName(f))
                    .Skip(maxVersions - 1) // Keep the newest (maxVersions - 1)
                    .ToList();

                // Delete old versions beyond the limit
                foreach (var oldFile in versionFiles)
                {
                    try
                    {
                        // Use async file operations to make this method properly use await
                        await Task.Run(() => {
                            File.Delete(oldFile);
                            // Also delete the metadata file if it exists
                            string metaFile = $"{oldFile}.meta.json";
                            if (File.Exists(metaFile))
                            {
                                File.Delete(metaFile);
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        _logAction($"Error cleaning up old version {oldFile}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logAction($"Error managing version history: {ex.Message}");
                // Add proper await to ensure the method actually uses async
                await Task.CompletedTask;
            }
        }
    }

    /// <summary>
    /// Represents the current state of a scraper
    /// </summary>
    public class ScraperState
    {
        public string ScraperId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime LastRunStartTime { get; set; }
        public DateTime? LastRunEndTime { get; set; }
        public DateTime? LastSuccessfulRunTime { get; set; }
        public int PagesScraped { get; set; }
        public int ErrorCount { get; set; }
        public string LastError { get; set; } = string.Empty;
        public string ProgressData { get; set; } = string.Empty;
        public string ConfigSnapshot { get; set; } = string.Empty;
        public HashSet<string> ProcessedUrls { get; set; } = new HashSet<string>();
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Internal PageVersion class for state management
    /// </summary>
    internal class PageVersion
    {
        public string Url { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public string ContentHash { get; set; } = string.Empty;
        public DateTime VersionDate { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string VersionId { get; set; } = Guid.NewGuid().ToString();
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}