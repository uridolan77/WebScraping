// filepath: c:\dev\WebScraping\WebScraper\StateManagement\PersistentStateManager.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using WebScraper.ContentChange;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Manages persistent storage of scraper state including content versions
    /// </summary>
    public class PersistentStateManager : IDisposable
    {
        private readonly StateDbContext _dbContext;
        private readonly Action<string> _logger;
        private readonly string _connectionString;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private bool _initialized;
        
        /// <summary>
        /// Initializes a new instance of the PersistentStateManager class
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        /// <param name="logger">Logger callback function</param>
        public PersistentStateManager(string connectionString, Action<string> logger = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _logger = logger ?? Console.WriteLine;
            
            var options = new DbContextOptionsBuilder<StateDbContext>()
                .UseSqlite(_connectionString)
                .Options;
                
            _dbContext = new StateDbContext(options);
        }

        /// <summary>
        /// Initializes the database
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task InitializeAsync()
        {
            if (_initialized)
                return;
                
            await _semaphore.WaitAsync();
            
            try
            {
                if (_initialized)
                    return;
                    
                _logger("Initializing persistent state database...");
                await _dbContext.Database.MigrateAsync();
                _initialized = true;
                _logger("Persistent state database initialized");
            }
            catch (Exception ex)
            {
                _logger($"Error initializing persistent state database: {ex.Message}");
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// Saves or updates the state of a scraper
        /// </summary>
        /// <param name="state">The state to save or update</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task SaveScraperStateAsync(ScraperState state)
        {
            await EnsureInitializedAsync();
            
            var existingState = await _dbContext.ScraperStates
                .FirstOrDefaultAsync(s => s.ScraperId == state.ScraperId);
                
            if (existingState != null)
            {
                // Update existing state
                existingState.Status = state.Status;
                existingState.LastRunStartTime = state.LastRunStartTime;
                existingState.LastRunEndTime = state.LastRunEndTime;
                existingState.LastSuccessfulRunTime = state.LastSuccessfulRunTime;
                existingState.ProgressData = state.ProgressData;
                existingState.ConfigSnapshot = state.ConfigSnapshot;
                existingState.UpdatedAt = DateTime.UtcNow;
                existingState.Statistics = state.Statistics;
                existingState.ErrorMessage = state.ErrorMessage;
                existingState.UrlsProcessed = state.UrlsProcessed;
            }
            else
            {
                // Create new state
                state.CreatedAt = DateTime.UtcNow;
                state.UpdatedAt = DateTime.UtcNow;
                await _dbContext.ScraperStates.AddAsync(state);
            }
            
            await _dbContext.SaveChangesAsync();
            _logger($"Saved scraper state for {state.ScraperId}");
        }
        
        /// <summary>
        /// Gets the state of a scraper
        /// </summary>
        /// <param name="scraperId">The ID of the scraper</param>
        /// <returns>The scraper state, or null if not found</returns>
        public async Task<ScraperState> GetScraperStateAsync(string scraperId)
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.ScraperStates
                .FirstOrDefaultAsync(s => s.ScraperId == scraperId);
        }
        
        /// <summary>
        /// Gets all scraper states
        /// </summary>
        /// <returns>A collection of scraper states</returns>
        public async Task<IEnumerable<ScraperState>> GetAllScraperStatesAsync()
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.ScraperStates.ToListAsync();
        }

        /// <summary>
        /// Saves content data with versioning
        /// </summary>
        /// <param name="contentItem">The content item to save</param>
        /// <param name="maxVersionsToKeep">Maximum number of versions to keep</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<ContentVersion> SaveContentVersionAsync(ContentItem contentItem, int maxVersionsToKeep = 5)
        {
            if (contentItem == null)
                throw new ArgumentNullException(nameof(contentItem));
                
            await EnsureInitializedAsync();
            
            var url = contentItem.Url;
            
            // Check if we already have this content
            var existingContent = await _dbContext.ContentItems
                .FirstOrDefaultAsync(c => c.Url == url);
                
            var contentVersion = new ContentVersion
            {
                VersionId = Guid.NewGuid().ToString(),
                ContentHash = contentItem.ContentHash,
                VersionTimestamp = DateTime.UtcNow,
                RawContent = contentItem.RawContent,
                ContentLength = contentItem.RawContent?.Length ?? 0,
                DiffFromPrevious = null, // Will set this later if needed
                ChangeDetected = false,  // Will set this later
                ChangeMetadata = null    // Will set this later
            };
            
            if (existingContent == null)
            {
                // New content
                contentItem.ContentKey = Guid.NewGuid().ToString();
                contentItem.FirstFetchTime = DateTime.UtcNow;
                contentItem.LastFetchTime = DateTime.UtcNow;
                contentItem.VersionCount = 1;
                
                // Add the version
                contentItem.Versions = new List<ContentVersion> { contentVersion };
                
                // Add to database
                await _dbContext.ContentItems.AddAsync(contentItem);
                _logger($"Added new content for URL: {url} (first version)");
            }
            else
            {
                // Existing content, update metadata
                existingContent.LastFetchTime = DateTime.UtcNow;
                existingContent.Title = contentItem.Title;
                existingContent.LastStatusCode = contentItem.LastStatusCode;
                existingContent.ContentType = contentItem.ContentType;
                existingContent.IsReachable = contentItem.IsReachable;
                
                // Get the most recent version to compare
                var latestVersion = await _dbContext.ContentVersions
                    .Where(v => v.ContentItemId == existingContent.ContentKey)
                    .OrderByDescending(v => v.VersionTimestamp)
                    .FirstOrDefaultAsync();
                    
                if (latestVersion != null)
                {
                    // Check if content has changed
                    if (latestVersion.ContentHash != contentVersion.ContentHash)
                    {
                        // Content has changed, calculate diff
                        if (latestVersion.RawContent != null && contentVersion.RawContent != null)
                        {
                            var differ = new DiffCalculator();
                            var diff = differ.CalculateDiff(latestVersion.RawContent, contentVersion.RawContent);
                            contentVersion.DiffFromPrevious = JsonConvert.SerializeObject(diff);
                            
                            contentVersion.ChangeDetected = true;
                            contentVersion.ChangeMetadata = JsonConvert.SerializeObject(new
                            {
                                AddedLineCount = diff.Count(c => c.Action == DiffAction.Add),
                                RemovedLineCount = diff.Count(c => c.Action == DiffAction.Remove),
                                ModifiedLineCount = diff.Count(c => c.Action == DiffAction.Modify),
                                PreviousVersionId = latestVersion.VersionId,
                                PreviousVersionTimestamp = latestVersion.VersionTimestamp
                            });
                            
                            _logger($"Content changed for URL: {url}");
                        }
                    }
                    else
                    {
                        // No change in content
                        _logger($"No content change for URL: {url}");
                        contentVersion.ChangeDetected = false;
                    }
                }
                
                // Link to the existing content
                contentVersion.ContentItemId = existingContent.ContentKey;
                
                // Add the new version
                await _dbContext.ContentVersions.AddAsync(contentVersion);
                
                // Increment version count
                existingContent.VersionCount += 1;
            }
            
            await _dbContext.SaveChangesAsync();
            
            // Clean up old versions if needed
            if (maxVersionsToKeep > 0)
            {
                string contentKey = existingContent?.ContentKey ?? contentItem.ContentKey;
                await CleanupOldVersionsAsync(contentKey, maxVersionsToKeep);
            }
            
            return contentVersion;
        }
        
        /// <summary>
        /// Gets the latest version of content for a specific URL
        /// </summary>
        /// <param name="url">The URL</param>
        /// <returns>The latest content version, or null if not found</returns>
        public async Task<(ContentItem ContentItem, ContentVersion Version)> GetLatestContentVersionAsync(string url)
        {
            await EnsureInitializedAsync();
            
            var content = await _dbContext.ContentItems
                .FirstOrDefaultAsync(c => c.Url == url);
                
            if (content == null)
                return (null, null);
                
            var version = await _dbContext.ContentVersions
                .Where(v => v.ContentItemId == content.ContentKey)
                .OrderByDescending(v => v.VersionTimestamp)
                .FirstOrDefaultAsync();
                
            return (content, version);
        }
        
        /// <summary>
        /// Gets all versions of content for a specific URL
        /// </summary>
        /// <param name="url">The URL</param>
        /// <returns>A list of content versions</returns>
        public async Task<(ContentItem ContentItem, List<ContentVersion> Versions)> GetAllContentVersionsAsync(string url)
        {
            await EnsureInitializedAsync();
            
            var content = await _dbContext.ContentItems
                .FirstOrDefaultAsync(c => c.Url == url);
                
            if (content == null)
                return (null, null);
                
            var versions = await _dbContext.ContentVersions
                .Where(v => v.ContentItemId == content.ContentKey)
                .OrderByDescending(v => v.VersionTimestamp)
                .ToListAsync();
                
            return (content, versions);
        }
        
        /// <summary>
        /// Gets a specific version of content
        /// </summary>
        /// <param name="versionId">The version ID</param>
        /// <returns>The content version, or null if not found</returns>
        public async Task<ContentVersion> GetContentVersionByIdAsync(string versionId)
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.ContentVersions
                .FirstOrDefaultAsync(v => v.VersionId == versionId);
        }
        
        /// <summary>
        /// Gets all URLs that have had content changes within a time period
        /// </summary>
        /// <param name="since">The start time</param>
        /// <returns>A list of URLs with changes</returns>
        public async Task<List<string>> GetUrlsWithChangesAsync(DateTime since)
        {
            await EnsureInitializedAsync();
            
            var changedVersions = await _dbContext.ContentVersions
                .Where(v => v.ChangeDetected && v.VersionTimestamp >= since)
                .Select(v => v.ContentItemId)
                .Distinct()
                .ToListAsync();
                
            if (changedVersions.Count == 0)
                return new List<string>();
                
            var urls = await _dbContext.ContentItems
                .Where(c => changedVersions.Contains(c.ContentKey))
                .Select(c => c.Url)
                .ToListAsync();
                
            return urls;
        }

        /// <summary>
        /// Cleans up old versions of content
        /// </summary>
        /// <param name="contentKey">The content key</param>
        /// <param name="maxVersionsToKeep">Maximum number of versions to keep</param>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task CleanupOldVersionsAsync(string contentKey, int maxVersionsToKeep)
        {
            // Get all versions for this content, ordered by timestamp (newest first)
            var versions = await _dbContext.ContentVersions
                .Where(v => v.ContentItemId == contentKey)
                .OrderByDescending(v => v.VersionTimestamp)
                .ToListAsync();
                
            if (versions.Count <= maxVersionsToKeep)
                return;
                
            // Keep at least the first version and the newest maxVersionsToKeep versions
            var versionsToKeep = versions
                .Take(maxVersionsToKeep)
                .Select(v => v.VersionId)
                .ToList();
                
            // Get the first version (oldest)
            var firstVersion = await _dbContext.ContentVersions
                .Where(v => v.ContentItemId == contentKey)
                .OrderBy(v => v.VersionTimestamp)
                .FirstOrDefaultAsync();
                
            if (firstVersion != null && !versionsToKeep.Contains(firstVersion.VersionId))
            {
                versionsToKeep.Add(firstVersion.VersionId);
            }
            
            // Delete versions we don't need to keep
            var versionsToDelete = await _dbContext.ContentVersions
                .Where(v => v.ContentItemId == contentKey && !versionsToKeep.Contains(v.VersionId))
                .ToListAsync();
                
            if (versionsToDelete.Any())
            {
                _dbContext.ContentVersions.RemoveRange(versionsToDelete);
                await _dbContext.SaveChangesAsync();
                _logger($"Cleaned up {versionsToDelete.Count} old versions for content key {contentKey}");
            }
        }
        
        /// <summary>
        /// Marks a URL as having been visited
        /// </summary>
        /// <param name="scraperId">The ID of the scraper</param>
        /// <param name="url">The URL</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="depth">The depth at which the URL was visited</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task MarkUrlVisitedAsync(string scraperId, string url, int statusCode, int depth)
        {
            await EnsureInitializedAsync();
            
            var visitLog = new UrlVisitLog
            {
                ScraperId = scraperId,
                Url = url,
                VisitTime = DateTime.UtcNow,
                StatusCode = statusCode,
                Depth = depth
            };
            
            await _dbContext.UrlVisitLogs.AddAsync(visitLog);
            await _dbContext.SaveChangesAsync();
        }
        
        /// <summary>
        /// Checks if a URL has been visited by a scraper
        /// </summary>
        /// <param name="scraperId">The ID of the scraper</param>
        /// <param name="url">The URL</param>
        /// <returns>True if the URL has been visited, false otherwise</returns>
        public async Task<bool> HasUrlBeenVisitedAsync(string scraperId, string url)
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.UrlVisitLogs
                .AnyAsync(v => v.ScraperId == scraperId && v.Url == url);
        }

        /// <summary>
        /// Gets a list of URLs that have been visited by a scraper
        /// </summary>
        /// <param name="scraperId">The ID of the scraper</param>
        /// <returns>A list of visited URLs</returns>
        public async Task<List<string>> GetVisitedUrlsAsync(string scraperId)
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.UrlVisitLogs
                .Where(v => v.ScraperId == scraperId)
                .Select(v => v.Url)
                .ToListAsync();
        }

        /// <summary>
        /// Gets a full history of visits to a URL
        /// </summary>
        /// <param name="url">The URL</param>
        /// <returns>A list of visit logs</returns>
        public async Task<List<UrlVisitLog>> GetUrlVisitHistoryAsync(string url)
        {
            await EnsureInitializedAsync();
            
            return await _dbContext.UrlVisitLogs
                .Where(v => v.Url == url)
                .OrderByDescending(v => v.VisitTime)
                .ToListAsync();
        }

        /// <summary>
        /// Ensures the database has been initialized
        /// </summary>
        private async Task EnsureInitializedAsync()
        {
            if (!_initialized)
            {
                await InitializeAsync();
            }
        }
        
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _dbContext?.Dispose();
            _semaphore?.Dispose();
        }

        /// <summary>
        /// Calculates differences between two text documents
        /// </summary>
        private class DiffCalculator
        {
            public List<DiffItem> CalculateDiff(string oldText, string newText)
            {
                var oldLines = oldText.Split('\n');
                var newLines = newText.Split('\n');
                
                var result = new List<DiffItem>();
                
                // This is a very simple diff implementation
                // In a real application, use a proper diff algorithm
                int maxLen = Math.Max(oldLines.Length, newLines.Length);
                
                for (int i = 0; i < maxLen; i++)
                {
                    if (i >= oldLines.Length)
                    {
                        // Line added
                        result.Add(new DiffItem
                        {
                            Action = DiffAction.Add,
                            Content = newLines[i],
                            LineNumber = i
                        });
                    }
                    else if (i >= newLines.Length)
                    {
                        // Line removed
                        result.Add(new DiffItem
                        {
                            Action = DiffAction.Remove,
                            Content = oldLines[i],
                            LineNumber = i
                        });
                    }
                    else if (oldLines[i] != newLines[i])
                    {
                        // Line modified
                        result.Add(new DiffItem
                        {
                            Action = DiffAction.Modify,
                            Content = newLines[i],
                            OldContent = oldLines[i],
                            LineNumber = i
                        });
                    }
                }
                
                return result;
            }
        }
    }
    
    /// <summary>
    /// DbContext for the state database
    /// </summary>
    public class StateDbContext : DbContext
    {
        public DbSet<ScraperState> ScraperStates { get; set; }
        public DbSet<ContentItem> ContentItems { get; set; }
        public DbSet<ContentVersion> ContentVersions { get; set; }
        public DbSet<UrlVisitLog> UrlVisitLogs { get; set; }
        
        public StateDbContext(DbContextOptions<StateDbContext> options) : base(options)
        {
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure ScraperState
            modelBuilder.Entity<ScraperState>()
                .HasKey(s => s.Id);
                
            modelBuilder.Entity<ScraperState>()
                .HasIndex(s => s.ScraperId)
                .IsUnique();
                
            // Configure ContentItem
            modelBuilder.Entity<ContentItem>()
                .HasKey(c => c.ContentKey);
                
            modelBuilder.Entity<ContentItem>()
                .HasIndex(c => c.Url)
                .IsUnique();
                
            modelBuilder.Entity<ContentItem>()
                .HasMany(c => c.Versions)
                .WithOne()
                .HasForeignKey(v => v.ContentItemId)
                .OnDelete(DeleteBehavior.Cascade);
                
            // Configure ContentVersion
            modelBuilder.Entity<ContentVersion>()
                .HasKey(v => v.VersionId);
                
            modelBuilder.Entity<ContentVersion>()
                .HasIndex(v => new { v.ContentItemId, v.VersionTimestamp });
                
            // Configure UrlVisitLog
            modelBuilder.Entity<UrlVisitLog>()
                .HasKey(v => v.Id);
                
            modelBuilder.Entity<UrlVisitLog>()
                .HasIndex(v => new { v.ScraperId, v.Url });
                
            modelBuilder.Entity<UrlVisitLog>()
                .HasIndex(v => v.VisitTime);
        }
    }
    
    /// <summary>
    /// State information for a scraper
    /// </summary>
    public class ScraperState
    {
        /// <summary>
        /// Database ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID of the scraper
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// Current status of the scraper
        /// </summary>
        public string Status { get; set; }
        
        /// <summary>
        /// Time the scraper state was created
        /// </summary>
        public DateTime CreatedAt { get; set; }
        
        /// <summary>
        /// Time the scraper state was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
        
        /// <summary>
        /// Time the scraper was last started
        /// </summary>
        public DateTime? LastRunStartTime { get; set; }
        
        /// <summary>
        /// Time the scraper last finished
        /// </summary>
        public DateTime? LastRunEndTime { get; set; }
        
        /// <summary>
        /// Time the scraper last completed successfully
        /// </summary>
        public DateTime? LastSuccessfulRunTime { get; set; }
        
        /// <summary>
        /// Number of URLs processed
        /// </summary>
        public int UrlsProcessed { get; set; }
        
        /// <summary>
        /// JSON serialized progress data
        /// </summary>
        public string ProgressData { get; set; }
        
        /// <summary>
        /// JSON serialized snapshot of the configuration
        /// </summary>
        public string ConfigSnapshot { get; set; }
        
        /// <summary>
        /// JSON serialized statistics
        /// </summary>
        public string Statistics { get; set; }
        
        /// <summary>
        /// Error message if the scraper failed
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// Content item from a web page
    /// </summary>
    public class ContentItem
    {
        /// <summary>
        /// Unique key for the content
        /// </summary>
        public string ContentKey { get; set; }
        
        /// <summary>
        /// URL where the content was found
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Title of the page
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// ID of the scraper that found the content
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// First time the content was fetched
        /// </summary>
        public DateTime FirstFetchTime { get; set; }
        
        /// <summary>
        /// Most recent time the content was fetched
        /// </summary>
        public DateTime LastFetchTime { get; set; }
        
        /// <summary>
        /// HTTP status code from the most recent fetch
        /// </summary>
        public int LastStatusCode { get; set; }
        
        /// <summary>
        /// Content type of the page
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Whether the URL is currently reachable
        /// </summary>
        public bool IsReachable { get; set; }
        
        /// <summary>
        /// Number of versions stored for this content
        /// </summary>
        public int VersionCount { get; set; }
        
        /// <summary>
        /// Whether this content relates to regulatory information
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// JSON serialized metadata about the content
        /// </summary>
        public string Metadata { get; set; }
        
        /// <summary>
        /// Navigation property for content versions
        /// </summary>
        public List<ContentVersion> Versions { get; set; }
        
        /// <summary>
        /// Hash of the content based on its raw content
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Raw content of the page
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        public string RawContent { get; set; }
    }
    
    /// <summary>
    /// Version of content from a web page
    /// </summary>
    public class ContentVersion
    {
        /// <summary>
        /// Unique ID for this version
        /// </summary>
        public string VersionId { get; set; }
        
        /// <summary>
        /// ID of the content this version belongs to
        /// </summary>
        public string ContentItemId { get; set; }
        
        /// <summary>
        /// Hash of the content
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Raw content of the page
        /// </summary>
        public string RawContent { get; set; }
        
        /// <summary>
        /// Time this version was created
        /// </summary>
        public DateTime VersionTimestamp { get; set; }
        
        /// <summary>
        /// Length of the content in characters
        /// </summary>
        public int ContentLength { get; set; }
        
        /// <summary>
        /// JSON serialized diff from the previous version
        /// </summary>
        public string DiffFromPrevious { get; set; }
        
        /// <summary>
        /// Whether a change was detected from the previous version
        /// </summary>
        public bool ChangeDetected { get; set; }
        
        /// <summary>
        /// JSON serialized metadata about the change
        /// </summary>
        public string ChangeMetadata { get; set; }
    }
    
    /// <summary>
    /// Log of a URL visit
    /// </summary>
    public class UrlVisitLog
    {
        /// <summary>
        /// Database ID
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// ID of the scraper
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// URL that was visited
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Time the URL was visited
        /// </summary>
        public DateTime VisitTime { get; set; }
        
        /// <summary>
        /// HTTP status code returned
        /// </summary>
        public int StatusCode { get; set; }
        
        /// <summary>
        /// Depth at which the URL was visited
        /// </summary>
        public int Depth { get; set; }
    }
    
    /// <summary>
    /// Diff action for content changes
    /// </summary>
    public enum DiffAction
    {
        /// <summary>
        /// Line was added
        /// </summary>
        Add,
        
        /// <summary>
        /// Line was removed
        /// </summary>
        Remove,
        
        /// <summary>
        /// Line was modified
        /// </summary>
        Modify
    }
    
    /// <summary>
    /// Item in a diff
    /// </summary>
    public class DiffItem
    {
        /// <summary>
        /// Action performed
        /// </summary>
        public DiffAction Action { get; set; }
        
        /// <summary>
        /// Content of the line
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Original content of the line (for modifications)
        /// </summary>
        public string OldContent { get; set; }
        
        /// <summary>
        /// Line number in the document
        /// </summary>
        public int LineNumber { get; set; }
    }
}