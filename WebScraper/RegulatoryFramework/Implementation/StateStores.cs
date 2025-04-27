using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Implementation
{
    /// <summary>
    /// In-memory implementation of the state store
    /// </summary>
    public class InMemoryStateStore : IStateStore
    {
        private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<string, List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>> _versionHistory = new ConcurrentDictionary<string, List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>>();
        
        public Task<T> GetAsync<T>(string key)
        {
            if (_store.TryGetValue(key, out var json))
            {
                return Task.FromResult(JsonConvert.DeserializeObject<T>(json));
            }
            
            return Task.FromResult<T>(default);
        }
        
        public Task SetAsync<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            _store[key] = json;
            return Task.CompletedTask;
        }
        
        public Task<WebScraper.RegulatoryFramework.Interfaces.PageVersion> GetLatestVersionAsync(string url)
        {
            if (_versionHistory.TryGetValue(url, out var versions) && versions.Count > 0)
            {
                return Task.FromResult(versions.OrderByDescending(v => v.CapturedAt).First());
            }
            
            return Task.FromResult<WebScraper.RegulatoryFramework.Interfaces.PageVersion>(null);
        }
        
        public Task SaveVersionAsync(WebScraper.RegulatoryFramework.Interfaces.PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            var versions = _versionHistory.GetOrAdd(version.Url, _ => new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>());
            
            // Add new version
            versions.Add(version);
            
            // Keep only the most recent versions (default: 10)
            if (versions.Count > 10)
            {
                versions = versions.OrderByDescending(v => v.CapturedAt).Take(10).ToList();
                _versionHistory[version.Url] = versions;
            }
            
            return Task.CompletedTask;
        }
        
        public Task<List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            if (_versionHistory.TryGetValue(url, out var versions))
            {
                return Task.FromResult(versions.OrderByDescending(v => v.CapturedAt).Take(maxVersions).ToList());
            }
            
            return Task.FromResult(new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>());
        }
    }
    
    /// <summary>
    /// File-system based implementation of the state store
    /// </summary>
    public class FileSystemStateStore : IStateStore
    {
        private readonly string _basePath;
        private readonly ILogger<FileSystemStateStore> _logger;
        private readonly string _dataPath;
        private readonly string _versionsPath;
        
        public FileSystemStateStore(string basePath, ILogger<FileSystemStateStore> logger)
        {
            _basePath = basePath;
            _logger = logger;
            
            _dataPath = Path.Combine(_basePath, "data");
            _versionsPath = Path.Combine(_basePath, "versions");
            
            // Ensure directories exist
            Directory.CreateDirectory(_dataPath);
            Directory.CreateDirectory(_versionsPath);
        }
        
        public async Task<T> GetAsync<T>(string key)
        {
            var path = GetPathForKey(key);
            
            if (!File.Exists(path))
            {
                return default;
            }
            
            try
            {
                var json = await File.ReadAllTextAsync(path);
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading data for key {Key}", key);
                return default;
            }
        }
        
        public async Task SetAsync<T>(string key, T value)
        {
            var path = GetPathForKey(key);
            
            try
            {
                var json = JsonConvert.SerializeObject(value);
                await File.WriteAllTextAsync(path, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing data for key {Key}", key);
            }
        }
        
        public async Task<WebScraper.RegulatoryFramework.Interfaces.PageVersion> GetLatestVersionAsync(string url)
        {
            var versions = await GetVersionHistoryAsync(url, 1);
            return versions.FirstOrDefault();
        }
        
        public async Task SaveVersionAsync(WebScraper.RegulatoryFramework.Interfaces.PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            var urlHash = GetSafeFilename(version.Url);
            var versionPath = Path.Combine(_versionsPath, urlHash);
            
            // Ensure version directory exists
            Directory.CreateDirectory(versionPath);
            
            // Save version file
            var timestamp = version.CapturedAt.ToString("yyyyMMddHHmmss");
            var versionFile = Path.Combine(versionPath, $"{timestamp}.json");
            
            try
            {
                // Create a copy without the full content for storage
                var storageVersion = new WebScraper.RegulatoryFramework.Interfaces.PageVersion
                {
                    Url = version.Url,
                    Hash = version.Hash,
                    CapturedAt = version.CapturedAt,
                    ChangeFromPrevious = version.ChangeFromPrevious,
                    ContentSummary = version.ContentSummary,
                    Metadata = version.Metadata
                };
                
                var json = JsonConvert.SerializeObject(storageVersion);
                await File.WriteAllTextAsync(versionFile, json);
                
                // If there's full content, save it separately
                if (!string.IsNullOrEmpty(version.FullContent))
                {
                    var contentFile = Path.Combine(versionPath, $"{timestamp}.content.txt");
                    await File.WriteAllTextAsync(contentFile, version.FullContent);
                }
                
                // If there's text content, save it separately
                if (!string.IsNullOrEmpty(version.TextContent))
                {
                    var textFile = Path.Combine(versionPath, $"{timestamp}.text.txt");
                    await File.WriteAllTextAsync(textFile, version.TextContent);
                }
                
                // Manage version history size
                await PruneVersionHistoryAsync(version.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving version for URL {Url}", version.Url);
            }
        }
        
        public async Task<List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            var urlHash = GetSafeFilename(url);
            var versionPath = Path.Combine(_versionsPath, urlHash);
            
            if (!Directory.Exists(versionPath))
            {
                return new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
            }
            
            try
            {
                var versionFiles = Directory.GetFiles(versionPath, "*.json")
                    .OrderByDescending(f => f)
                    .Take(maxVersions);
                
                var versions = new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
                
                foreach (var file in versionFiles)
                {
                    var json = await File.ReadAllTextAsync(file);
                    var version = JsonConvert.DeserializeObject<WebScraper.RegulatoryFramework.Interfaces.PageVersion>(json);
                    
                    // Try to load full content if it exists
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var contentFile = Path.Combine(versionPath, $"{fileName}.content.txt");
                    if (File.Exists(contentFile))
                    {
                        version.FullContent = await File.ReadAllTextAsync(contentFile);
                    }
                    
                    // Try to load text content if it exists
                    var textFile = Path.Combine(versionPath, $"{fileName}.text.txt");
                    if (File.Exists(textFile))
                    {
                        version.TextContent = await File.ReadAllTextAsync(textFile);
                    }
                    
                    versions.Add(version);
                }
                
                return versions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for URL {Url}", url);
                return new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
            }
        }
        
        private string GetPathForKey(string key)
        {
            var safeKey = GetSafeFilename(key);
            return Path.Combine(_dataPath, $"{safeKey}.json");
        }
        
        private string GetSafeFilename(string input)
        {
            // Convert to Base64 for simplicity and safety
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(input))
                .Replace('/', '_')
                .Replace('+', '-')
                .Replace('=', '.');
        }
        
        private async Task PruneVersionHistoryAsync(string url, int maxVersions = 10)
        {
            var urlHash = GetSafeFilename(url);
            var versionPath = Path.Combine(_versionsPath, urlHash);
            
            if (!Directory.Exists(versionPath))
            {
                return;
            }
            
            try
            {
                var versionFiles = Directory.GetFiles(versionPath, "*.json")
                    .OrderByDescending(f => f)
                    .Skip(maxVersions)
                    .ToList();
                
                foreach (var file in versionFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    
                    // Remove JSON version file
                    File.Delete(file);
                    
                    // Remove content file if it exists
                    var contentFile = Path.Combine(versionPath, $"{fileName}.content.txt");
                    if (File.Exists(contentFile))
                    {
                        File.Delete(contentFile);
                    }
                    
                    // Remove text file if it exists
                    var textFile = Path.Combine(versionPath, $"{fileName}.text.txt");
                    if (File.Exists(textFile))
                    {
                        File.Delete(textFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pruning version history for URL {Url}", url);
            }
        }
    }
    
    /// <summary>
    /// SQLite-based implementation of the state store
    /// </summary>
    public class DatabaseStateStore : IStateStore
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseStateStore> _logger;
        
        public DatabaseStateStore(string connectionString, ILogger<DatabaseStateStore> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
            
            InitializeDatabase().GetAwaiter().GetResult();
        }
        
        public async Task<T> GetAsync<T>(string key)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT Value FROM KeyValueData WHERE [Key] = @Key";
                command.Parameters.AddWithValue("@Key", key);
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    return JsonConvert.DeserializeObject<T>((string)result);
                }
                
                return default;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value for key {Key}", key);
                return default;
            }
        }
        
        public async Task SetAsync<T>(string key, T value)
        {
            try
            {
                var json = JsonConvert.SerializeObject(value);
                
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT OR REPLACE INTO KeyValueData ([Key], Value, UpdatedAt)
                    VALUES (@Key, @Value, @UpdatedAt)";
                command.Parameters.AddWithValue("@Key", key);
                command.Parameters.AddWithValue("@Value", json);
                command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow);
                
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value for key {Key}", key);
            }
        }
        
        public async Task<WebScraper.RegulatoryFramework.Interfaces.PageVersion> GetLatestVersionAsync(string url)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Metadata, FullContent, TextContent
                    FROM PageVersions
                    WHERE Url = @Url
                    ORDER BY CapturedAt DESC
                    LIMIT 1";
                command.Parameters.AddWithValue("@Url", url);
                
                using var reader = await command.ExecuteReaderAsync();
                
                if (reader.Read())
                {
                    var metadata = reader.GetString(0);
                    var version = JsonConvert.DeserializeObject<WebScraper.RegulatoryFramework.Interfaces.PageVersion>(metadata);
                    
                    // Load content only if needed
                    if (!reader.IsDBNull(1))
                    {
                        version.FullContent = reader.GetString(1);
                    }
                    
                    if (!reader.IsDBNull(2))
                    {
                        version.TextContent = reader.GetString(2);
                    }
                    
                    return version;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting latest version for URL {Url}", url);
                return null;
            }
        }
        
        public async Task SaveVersionAsync(WebScraper.RegulatoryFramework.Interfaces.PageVersion version)
        {
            if (version == null) throw new ArgumentNullException(nameof(version));
            
            try
            {
                // Prepare metadata without full content
                var storageVersion = new WebScraper.RegulatoryFramework.Interfaces.PageVersion
                {
                    Url = version.Url,
                    Hash = version.Hash,
                    CapturedAt = version.CapturedAt,
                    ChangeFromPrevious = version.ChangeFromPrevious,
                    ContentSummary = version.ContentSummary,
                    Metadata = version.Metadata
                };
                
                var metadata = JsonConvert.SerializeObject(storageVersion);
                
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO PageVersions (Url, Hash, CapturedAt, ChangeType, Metadata, FullContent, TextContent)
                    VALUES (@Url, @Hash, @CapturedAt, @ChangeType, @Metadata, @FullContent, @TextContent)";
                command.Parameters.AddWithValue("@Url", version.Url);
                command.Parameters.AddWithValue("@Hash", version.Hash);
                command.Parameters.AddWithValue("@CapturedAt", version.CapturedAt);
                command.Parameters.AddWithValue("@ChangeType", version.ChangeFromPrevious.ToString());
                command.Parameters.AddWithValue("@Metadata", metadata);
                command.Parameters.AddWithValue("@FullContent", version.FullContent ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@TextContent", version.TextContent ?? (object)DBNull.Value);
                
                await command.ExecuteNonQueryAsync();
                
                // Prune older versions
                await PruneVersionHistoryAsync(version.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving version for URL {Url}", version.Url);
            }
        }
        
        public async Task<List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT Metadata, FullContent, TextContent
                    FROM PageVersions
                    WHERE Url = @Url
                    ORDER BY CapturedAt DESC
                    LIMIT @MaxVersions";
                command.Parameters.AddWithValue("@Url", url);
                command.Parameters.AddWithValue("@MaxVersions", maxVersions);
                
                using var reader = await command.ExecuteReaderAsync();
                
                var versions = new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
                
                while (reader.Read())
                {
                    var metadata = reader.GetString(0);
                    var version = JsonConvert.DeserializeObject<WebScraper.RegulatoryFramework.Interfaces.PageVersion>(metadata);
                    
                    // Load content only if needed
                    if (!reader.IsDBNull(1))
                    {
                        version.FullContent = reader.GetString(1);
                    }
                    
                    if (!reader.IsDBNull(2))
                    {
                        version.TextContent = reader.GetString(2);
                    }
                    
                    versions.Add(version);
                }
                
                return versions;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting version history for URL {Url}", url);
                return new List<WebScraper.RegulatoryFramework.Interfaces.PageVersion>();
            }
        }
        
        private async Task InitializeDatabase()
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                // Create key-value table
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS KeyValueData (
                            [Key] TEXT PRIMARY KEY,
                            Value TEXT,
                            UpdatedAt TEXT
                        )";
                    await command.ExecuteNonQueryAsync();
                }
                
                // Create page versions table
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS PageVersions (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Url TEXT,
                            Hash TEXT,
                            CapturedAt TEXT,
                            ChangeType TEXT,
                            Metadata TEXT,
                            FullContent TEXT,
                            TextContent TEXT
                        )";
                    await command.ExecuteNonQueryAsync();
                }
                
                // Create index on Url and CapturedAt
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE INDEX IF NOT EXISTS IDX_PageVersions_Url_CapturedAt
                        ON PageVersions (Url, CapturedAt)";
                    await command.ExecuteNonQueryAsync();
                }
                
                _logger.LogInformation("Database initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing database");
                throw;
            }
        }
        
        private async Task PruneVersionHistoryAsync(string url, int maxVersions = 10)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                await connection.OpenAsync();
                
                // Get the IDs to keep (most recent versions)
                using var selectCommand = connection.CreateCommand();
                selectCommand.CommandText = @"
                    SELECT Id FROM PageVersions
                    WHERE Url = @Url
                    ORDER BY CapturedAt DESC
                    LIMIT @MaxVersions";
                selectCommand.Parameters.AddWithValue("@Url", url);
                selectCommand.Parameters.AddWithValue("@MaxVersions", maxVersions);
                
                var idsToKeep = new List<long>();
                using (var reader = await selectCommand.ExecuteReaderAsync())
                {
                    while (reader.Read())
                    {
                        idsToKeep.Add(reader.GetInt64(0));
                    }
                }
                
                if (idsToKeep.Count == 0)
                {
                    return;
                }
                
                // Delete all versions for this URL except the ones to keep
                using var deleteCommand = connection.CreateCommand();
                deleteCommand.CommandText = $@"
                    DELETE FROM PageVersions
                    WHERE Url = @Url
                    AND Id NOT IN ({string.Join(",", idsToKeep)})";
                deleteCommand.Parameters.AddWithValue("@Url", url);
                
                await deleteCommand.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pruning version history for URL {Url}", url);
            }
        }
    }
}