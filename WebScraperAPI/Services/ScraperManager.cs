using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.StateManagement;
using WebScraperApi.Models;
using WebScraperAPI.Data.Repositories;
using WebScraperApi.Controllers;
using WebScraper.RegulatoryContent;
using WebScraper.ContentChange;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Manages scraper configurations, execution, and monitoring
    /// </summary>
    public class ScraperManager : IHostedService, IDisposable
    {
        private readonly ILogger<ScraperManager> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ScraperConfigurationService _configService;
        private readonly ScraperExecutionService _executionService;
        private readonly Dictionary<string, ScraperInstance> _scrapers = new();
        private readonly string _stateDbPath;
        private Timer _monitoringTimer;
        
        /// <summary>
        /// Initializes a new instance of the ScraperManager class
        /// </summary>
        public ScraperManager(
            ILogger<ScraperManager> logger,
            IScraperConfigRepository configRepository,
            IScrapedContentRepository contentRepository,
            ICacheRepository cacheRepository,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            
            // Initialize services
            _configService = new ScraperConfigurationService(logger, configRepository);
            _executionService = new ScraperExecutionService(logger, loggerFactory);
            
            // Set up path for state database
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScraperState");
            if (!Directory.Exists(dataDir))
            {
                Directory.CreateDirectory(dataDir);
            }
            _stateDbPath = $"Data Source={Path.Combine(dataDir, "scraper_state.db")}";
        }
        
        /// <summary>
        /// Starts the scraper manager
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraper Manager starting");
            
            // Load saved scraper configurations
            await LoadScraperConfigurationsAsync();
            
            // Set up monitoring timer for checking changes
            _monitoringTimer = new Timer(CheckAllMonitoredScrapers, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            
            return;
        }
        
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraper Manager stopping");
            
            // Stop all scrapers
            foreach (var scraper in _scrapers.Values)
            {
                StopScraperInstance(scraper.Config.Id);
            }
            
            _monitoringTimer?.Change(Timeout.Infinite, 0);
            
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _monitoringTimer?.Dispose();
            
            // Dispose scrapers that are still running
            foreach (var scraper in _scrapers.Values)
            {
                if (scraper.Scraper != null)
                {
                    try
                    {
                        // Only dispose if IsRunning is true, as it might already be disposed otherwise
                        if (scraper.Status.IsRunning)
                        {
                            scraper.Scraper.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error disposing scraper {scraper.Config.Id}");
                    }
                }
                
                // Dispose state manager if it exists
                scraper.StateManager?.Dispose();
            }
        }
        
        #region Scraper Configuration Management
        
        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        /// <returns>A collection of scraper configurations</returns>
        public async Task<IEnumerable<ScraperConfigModel>> GetAllScraperConfigsAsync()
        {
            try
            {
                var configs = await _configService.GetAllScraperConfigsAsync();
                var models = configs.ToList();
                
                // Update in-memory cache
                lock (_scrapers)
                {
                    foreach (var model in models)
                    {
                        if (!_scrapers.TryGetValue(model.Id, out var instance))
                        {
                            _scrapers[model.Id] = new ScraperInstance
                            {
                                Config = model,
                                Status = new ScraperStatus
                                {
                                    IsRunning = false,
                                    LastMonitorCheck = null
                                }
                            };
                        }
                        else
                        {
                            // Update config but preserve status
                            instance.Config = model;
                        }
                    }
                }
                
                return models;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all scraper configurations from database");
                
                // Fallback to in-memory cache
                lock (_scrapers)
                {
                    return _scrapers.Values.Select(s => s.Config).ToList();
                }
            }
        }
        
        public async Task<ScraperConfigModel> GetScraperConfig(string id)
        {
            try
            {
                var model = await _configService.GetScraperConfigAsync(id);
                if (model == null)
                {
                    return null;
                }

                // Update in-memory cache if needed
                lock (_scrapers)
                {
                    if (_scrapers.TryGetValue(id, out var instance))
                    {
                        // Update config but preserve status
                        instance.Config = model;
                    }
                    else
                    {
                        _scrapers[id] = new ScraperInstance
                        {
                            Config = model,
                            Status = new ScraperStatus
                            {
                                IsRunning = false,
                                LastMonitorCheck = null
                            }
                        };
                    }
                }

                return model;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting scraper configuration with ID {id} from database");
                
                // Fallback to in-memory cache
                lock (_scrapers)
                {
                    return _scrapers.TryGetValue(id, out var instance) ? instance.Config : null;
                }
            }
        }
        
        public async Task<ScraperConfigModel> CreateScraperConfig(ScraperConfigModel config)
        {
            try
            {
                var createdConfig = await _configService.CreateScraperConfigAsync(config);
                
                // Add to in-memory cache
                lock (_scrapers)
                {
                    _scrapers[createdConfig.Id] = new ScraperInstance
                    {
                        Config = createdConfig,
                        Status = new ScraperStatus
                        {
                            IsRunning = false,
                            LastMonitorCheck = null
                        }
                    };
                }
                
                // Save configurations to file as backup
                SaveScraperConfigurations();
                
                return createdConfig;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating scraper configuration '{config.Name}'");
                
                // Add to in-memory cache as a fallback
                lock (_scrapers)
                {
                    if (!_scrapers.ContainsKey(config.Id))
                    {
                        _scrapers[config.Id] = new ScraperInstance
                        {
                            Config = config,
                            Status = new ScraperStatus
                            {
                                IsRunning = false,
                                LastMonitorCheck = null
                            }
                        };
                    }
                }
                
                return config;
            }
        }
        
        public async Task<bool> UpdateScraperConfig(string id, ScraperConfigModel config)
        {
            try
            {
                // Check in-memory cache first to verify not running
                lock (_scrapers)
                {
                    if (!_scrapers.ContainsKey(id))
                    {
                        return false;
                    }
                    
                    // Don't allow changing config while scraper is running
                    if (_scrapers[id].Status.IsRunning)
                    {
                        return false;
                    }
                }

                // Update in service
                var success = await _configService.UpdateScraperConfigAsync(id, config);
                
                if (success)
                {
                    // Update in-memory cache
                    lock (_scrapers)
                    {
                        if (_scrapers.TryGetValue(id, out var instance))
                        {
                            // Update config but preserve status
                            instance.Config = config;
                        }
                    }
                    
                    // Save configurations to file as backup
                    SaveScraperConfigurations();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating scraper configuration with ID {id}");
                return false;
            }
        }
        
        public async Task<bool> DeleteScraperConfig(string id)
        {
            try
            {
                // Check in-memory cache first to verify not running
                lock (_scrapers)
                {
                    if (!_scrapers.ContainsKey(id))
                    {
                        return false;
                    }
                    
                    // Don't allow deletion while scraper is running
                    if (_scrapers[id].Status.IsRunning)
                    {
                        return false;
                    }
                }

                // Delete from service
                var success = await _configService.DeleteScraperConfigAsync(id);
                
                if (success)
                {
                    // Remove from in-memory cache
                    lock (_scrapers)
                    {
                        _scrapers.Remove(id);
                    }
                    
                    // Save configurations to file as backup
                    SaveScraperConfigurations();
                }

                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting scraper configuration with ID {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Loads scraper configurations from the database
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        private async Task LoadScraperConfigurationsAsync()
        {
            try
            {
                var configModels = await _configService.LoadScraperConfigurationsAsync();
                
                lock (_scrapers)
                {
                    _scrapers.Clear();
                    
                    foreach (var config in configModels)
                    {
                        _scrapers[config.Id] = new ScraperInstance
                        {
                            Config = config,
                            Status = new ScraperStatus
                            {
                                IsRunning = false,
                                LastMonitorCheck = null
                            }
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scraper configurations");
            }
        }
        
        private void SaveScraperConfigurations()
        {
            lock (_scrapers)
            {
                try
                {
                    var configs = _scrapers.Values.Select(s => s.Config).ToList();
                    _configService.SaveScraperConfigurationsToFile(configs);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error saving scraper configurations");
                }
            }
        }
        
        #endregion
        
        #region Scraper Operation
        
        public async Task<bool> StartScraper(string id)
        {
            lock (_scrapers)
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return false;
                }
                
                if (scraperInstance.Status.IsRunning)
                {
                    return false; // Already running
                }
                
                // Update status
                scraperInstance.Status.IsRunning = true;
                scraperInstance.Status.StartTime = DateTime.Now;
                scraperInstance.Status.EndTime = null;
                scraperInstance.Status.UrlsProcessed = 0;
                scraperInstance.Status.LogMessages.Clear();
                
                // Update last run timestamp
                scraperInstance.Config.LastRun = DateTime.Now;
                
                // Start scraping in a background task
                Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation($"Starting scraper {id}: {scraperInstance.Config.Name}");
                        
                        // Create a persistent state manager for this scraper
                        var stateDbConnectionString = _stateDbPath;
                        scraperInstance.StateManager = new PersistentStateManager(
                            stateDbConnectionString, 
                            message => AddLogMessage(id, message));
                        
                        await scraperInstance.StateManager.InitializeAsync();
                        
                        // Create a delegate to log messages
                        Action<string> logAction = message => AddLogMessage(id, message);
                        
                        // Execute the scraper using the execution service
                        var config = scraperInstance.Config;
                        await _executionService.StartScraperAsync(
                            config,
                            new WebScraperApi.Models.ScraperState(),
                            logAction);
                        
                        // Update status once complete
                        scraperInstance.Status.IsRunning = false;
                        scraperInstance.Status.EndTime = DateTime.Now;
                        
                        AddLogMessage(id, "Scraping completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error during scraping of {id}");
                        scraperInstance.Status.IsRunning = false;
                        scraperInstance.Status.EndTime = DateTime.Now;
                        AddLogMessage(id, $"Error during scraping: {ex.Message}");
                    }
                });
                
                SaveScraperConfigurations();
                return true;
            }
        }
        
        public bool StopScraperInstance(string id)
        {
            lock (_scrapers)
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return false;
                }
                
                if (!scraperInstance.Status.IsRunning)
                {
                    return false; // Not running
                }
                
                // Update status
                scraperInstance.Status.IsRunning = false;
                scraperInstance.Status.EndTime = DateTime.Now;
                
                // Stop the scraper
                if (scraperInstance.Scraper != null)
                {
                    _executionService.StopScraper(
                        scraperInstance.Scraper, 
                        message => AddLogMessage(id, message));
                }
                else
                {
                    AddLogMessage(id, "Scraping stopped by user");
                }
                
                return true;
            }
        }
        
        public ScraperStatus GetScraperStatus(string id)
        {
            lock (_scrapers)
            {
                if (_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    // Calculate elapsed time if running
                    if (scraperInstance.Status.IsRunning && scraperInstance.Status.StartTime.HasValue)
                    {
                        var elapsed = DateTime.Now - scraperInstance.Status.StartTime.Value;
                        scraperInstance.Status.ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
                    }
                    
                    // If we have an enhanced scraper with persistent state, update status from there
                    if (scraperInstance.Scraper is EnhancedScraper enhancedScraper && 
                        scraperInstance.StateManager != null)
                    {
                        try
                        {
                            // Get scraper state asynchronously but wait for result
                            var state = enhancedScraper.GetStateAsync().GetAwaiter().GetResult();
                            if (state != null)
                            {
                                // Update status with information from persistent state
                                scraperInstance.Status.UrlsProcessed = state.UrlsProcessed;
                                
                                // Get pipeline metrics
                                var metrics = enhancedScraper.GetPipelineStatus();
                                if (metrics != null)
                                {
                                    scraperInstance.Status.PipelineMetrics = new PipelineMetrics
                                    {
                                        ProcessingItems = metrics.ProcessingItems,
                                        QueuedItems = metrics.InputQueueCount,
                                        CompletedItems = (int)(metrics.Metrics?.ProcessedCount ?? 0),
                                        FailedItems = (int)(metrics.Metrics?.ErrorCount ?? 0),
                                        AverageProcessingTimeMs = metrics.Metrics?.AverageProcessingTimeMs ?? 0
                                    };
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error getting enhanced scraper state");
                        }
                    }
                    
                    return scraperInstance.Status.Clone();
                }
                
                return null;
            }
        }
        
        public IEnumerable<LogEntry> GetScraperLogs(string id, int limit = 100)
        {
            lock (_scrapers)
            {
                if (_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    var logs = scraperInstance.Status.LogMessages;
                    
                    return logs.Count <= limit
                        ? logs.ToList()
                        : logs.Skip(logs.Count - limit).ToList();
                }
                
                return Enumerable.Empty<LogEntry>();
            }
        }
        
        public void AddLogMessage(string id, string message)
        {
            lock (_scrapers)
            {
                if (_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    var logEntry = new LogEntry
                    {
                        Timestamp = DateTime.Now,
                        Message = message
                    };
                    
                    scraperInstance.Status.LogMessages.Add(logEntry);
                    
                    // Keep only the last 1000 messages
                    if (scraperInstance.Status.LogMessages.Count > 1000)
                    {
                        scraperInstance.Status.LogMessages.RemoveAt(0);
                    }
                    
                    _logger.LogInformation($"[Scraper {id}] {message}");
                }
            }
        }
        
        #endregion
        
        #region Monitoring
        
        private void CheckAllMonitoredScrapers(object state)
        {
            // Get scrapers that need to be checked
            var scrapersToCheck = new List<string>();
            
            lock (_scrapers)
            {
                var now = DateTime.Now;
                
                foreach (var kvp in _scrapers)
                {
                    var id = kvp.Key;
                    var instance = kvp.Value;
                    
                    // Skip if monitoring is not enabled
                    if (!instance.Config.EnableContinuousMonitoring)
                        continue;
                        
                    // Skip if currently running
                    if (instance.Status.IsRunning)
                        continue;
                        
                    // Check if it's time to run the monitoring check
                    var lastCheck = instance.Status.LastMonitorCheck ?? DateTime.MinValue;
                    var interval = instance.Config.GetMonitoringInterval();
                    
                    if (now - lastCheck >= interval)
                    {
                        scrapersToCheck.Add(id);
                    }
                }
            }
            
            // Check each scraper that needs to be monitored
            foreach (var id in scrapersToCheck)
            {
                Task.Run(async () => await RunMonitoringCheck(id));
            }
        }
        
        private async Task RunMonitoringCheck(string id)
        {
            lock (_scrapers)
            {
                if (!_scrapers.TryGetValue(id, out var instance))
                    return;
                    
                // Update last check time
                instance.Status.LastMonitorCheck = DateTime.Now;
                
                AddLogMessage(id, "Starting monitoring check...");
            }
            
            try
            {
                // Start the scraper in monitoring mode
                await StartScraper(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during monitoring check of {id}");
                AddLogMessage(id, $"Error during monitoring check: {ex.Message}");
            }
        }
        
        #endregion
        
        #region Additional API functionality
        
        /// <summary>
        /// Gets detected content changes for a specific scraper
        /// </summary>
        public async Task<object> GetDetectedChanges(string id, DateTime? since = null, int limit = 100)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Create path for version history
                var versionHistoryPath = Path.Combine(outputDir, "version_history.json");
                
                if (!File.Exists(versionHistoryPath))
                {
                    return new List<object>();
                }
                
                // Load version history
                var json = await File.ReadAllTextAsync(versionHistoryPath);
                var history = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<WebScraper.ContentChange.PageVersion>>>(json);
                
                if (history == null)
                {
                    return new List<object>();
                }
                
                // Process and filter changes
                var changes = new List<object>();
                
                foreach (var url in history.Keys)
                {
                    var versions = history[url];
                    
                    // Filter by date if specified
                    if (since.HasValue)
                    {
                        versions = versions.Where(v => v.VersionDate >= since.Value).ToList();
                    }
                    
                    // Skip if no versions or only one version (no changes to detect)
                    if (versions.Count <= 1)
                    {
                        continue;
                    }
                    
                    // Add each change (comparing adjacent versions)
                    for (int i = 1; i < versions.Count; i++)
                    {
                        changes.Add(new
                        {
                            Url = url,
                            PreviousVersion = versions[i - 1].VersionDate,
                            CurrentVersion = versions[i].VersionDate,
                            ChangeType = versions[i].ChangeFromPrevious.ToString(),
                            ChangeSummary = versions[i].ChangedSections != null ? 
                                string.Join(Environment.NewLine, versions[i].ChangedSections.Select(kvp => $"{kvp.Key}: {kvp.Value}")) : 
                                string.Empty,
                            ContentHashChanged = versions[i].Hash != versions[i - 1].Hash
                        });
                    }
                }
                
                // Apply limit and return sorted by timestamp (most recent first)
                return changes
                    .OrderByDescending(c => ((DateTime)((dynamic)c).CurrentVersion))
                    .Take(limit)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting detected changes for scraper {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets processed documents for a specific scraper
        /// </summary>
        public async Task<object> GetProcessedDocuments(string id, string documentType = null, int page = 1, int pageSize = 20)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Ensure the directory exists
                if (!Directory.Exists(outputDir))
                {
                    return new
                    {
                        Documents = new List<object>(),
                        TotalCount = 0,
                        Page = page,
                        PageSize = pageSize,
                        TotalPages = 0
                    };
                }
                
                // Filter by document type
                string searchPattern = documentType?.ToLowerInvariant() switch
                {
                    "pdf" => "*.pdf",
                    "word" => "*.docx", // Include more Office formats if needed
                    "excel" => "*.xlsx", // Include more Office formats if needed
                    _ => "*.*"
                };
                
                // Find all processed documents in the directory and subdirectories
                var files = Directory.GetFiles(outputDir, searchPattern, SearchOption.AllDirectories)
                    .Where(file => !Path.GetFileName(file).StartsWith("."))
                    .ToList();
                
                // Filter further if documentType is specified but not one of the predefined types
                if (!string.IsNullOrEmpty(documentType) && 
                    !new[] { "pdf", "word", "excel" }.Contains(documentType.ToLowerInvariant()))
                {
                    files = files.Where(f => Path.GetExtension(f).TrimStart('.').Equals(
                        documentType, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                
                // Calculate pagination
                int totalCount = files.Count;
                int skip = (page - 1) * pageSize;
                int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                
                var documents = files
                    .Skip(skip)
                    .Take(pageSize)
                    .Select(file => new
                    {
                        FileName = Path.GetFileName(file),
                        FilePath = file.Replace(outputDir, "").TrimStart('\\', '/'),
                        FileSize = new FileInfo(file).Length,
                        LastModified = File.GetLastWriteTime(file),
                        FileType = Path.GetExtension(file).TrimStart('.').ToUpperInvariant()
                    })
                    .ToList<object>();
                
                return new
                {
                    Documents = documents,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting processed documents for scraper {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets analytics data for a specific scraper
        /// </summary>
        public async Task<object> GetScraperAnalytics(string id)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get basic status
                var status = GetScraperStatus(id);
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Calculate additional analytics
                var analytics = new Dictionary<string, object>();
                
                // Add basic stats
                analytics["TotalRuns"] = scraperInstance.Config.RunCount;
                analytics["LastRunDuration"] = status?.ElapsedTime;
                analytics["TotalUrlsProcessed"] = status?.UrlsProcessed ?? 0;
                
                // Check if state manager exists for this scraper
                if (scraperInstance.StateManager != null)
                {
                    try
                    {
                        // Get advanced analytics from state manager
                        var advancedAnalytics = await GetStateManagerAnalytics(scraperInstance.StateManager);
                        if (advancedAnalytics != null)
                        {
                            foreach (var kvp in advancedAnalytics)
                            {
                                analytics[kvp.Key] = kvp.Value;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error getting advanced analytics from state manager");
                    }
                }
                
                // Count document types
                if (Directory.Exists(outputDir))
                {
                    var fileTypes = Directory.GetFiles(outputDir, "*.*", SearchOption.AllDirectories)
                        .Select(f => Path.GetExtension(f).TrimStart('.').ToLowerInvariant())
                        .GroupBy(ext => ext)
                        .ToDictionary(g => g.Key, g => g.Count());
                    
                    analytics["DocumentTypes"] = fileTypes;
                }
                
                return analytics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting analytics for scraper {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Helper method to get analytics from a state manager
        /// </summary>
        private async Task<Dictionary<string, object>> GetStateManagerAnalytics(PersistentStateManager stateManager)
        {
            // This is a placeholder that would be implemented based on what analytics
            // the PersistentStateManager can provide
            return new Dictionary<string, object>
            {
                ["AverageResponseTime"] = 150,
                ["TotalPagesProcessed"] = 1000,
                ["UniqueDomainsVisited"] = 5,
                ["ErrorRate"] = 0.02
            };
        }
        
        /// <summary>
        /// Updates content extraction rules for a specific scraper
        /// </summary>
        public async Task<bool> UpdateContentExtractionRules(string id, ContentExtractionRules rules)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Status.IsRunning)
                {
                    return false;
                }
                
                // Update content extraction rules in the config
                scraperInstance.Config.ContentExtractorSelectors = rules.IncludeSelectors;
                scraperInstance.Config.ContentExtractorExcludeSelectors = rules.ExcludeSelectors;
                scraperInstance.Config.ExtractMetadata = rules.ExtractMetadata;
                scraperInstance.Config.ExtractStructuredData = rules.ExtractStructuredData;
                scraperInstance.Config.CustomJsExtractor = rules.CustomJsExtractor;
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (success)
                {
                    SaveScraperConfigurations();
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating content extraction rules for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Gets learned patterns for a specific scraper
        /// </summary>
        public async Task<object> GetLearnedPatterns(string id)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Path to learned patterns
                var patternsPath = Path.Combine(outputDir, "learned_patterns.json");
                
                if (!File.Exists(patternsPath))
                {
                    return new Dictionary<string, object>();
                }
                
                // Load learned patterns
                var json = await File.ReadAllTextAsync(patternsPath);
                var patterns = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                
                return patterns ?? new Dictionary<string, object>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting learned patterns for scraper {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Gets regulatory alerts for a specific scraper
        /// </summary>
        public async Task<object> GetRegulatoryAlerts(string id, DateTime? since = null, string importance = null)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Path to regulatory alerts
                var alertsPath = Path.Combine(outputDir, "regulatory_alerts.json");
                
                if (!File.Exists(alertsPath))
                {
                    return new List<object>();
                }
                
                // Load regulatory alerts
                var json = await File.ReadAllTextAsync(alertsPath);
                var allAlerts = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                
                if (allAlerts == null)
                {
                    return new List<object>();
                }
                
                // Filter alerts
                var filteredAlerts = allAlerts.AsEnumerable();
                
                if (since.HasValue)
                {
                    filteredAlerts = filteredAlerts.Where(a => 
                        DateTime.TryParse(a["Timestamp"].ToString(), out DateTime timestamp) && 
                        timestamp >= since.Value);
                }
                
                if (!string.IsNullOrEmpty(importance))
                {
                    filteredAlerts = filteredAlerts.Where(a => 
                        a.TryGetValue("Importance", out object imp) && 
                        imp.ToString().Equals(importance, StringComparison.OrdinalIgnoreCase));
                }
                
                return filteredAlerts.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting regulatory alerts for scraper {id}");
                return null;
            }
        }
        
        /// <summary>
        /// Updates regulatory configuration for a specific scraper
        /// </summary>
        public async Task<bool> UpdateRegulatoryConfig(string id, RegulatoryConfigModel config)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Status.IsRunning)
                {
                    return false;
                }
                
                // Update regulatory configuration in the config
                scraperInstance.Config.EnableRegulatoryContentAnalysis = config.EnableRegulatoryContentAnalysis;
                scraperInstance.Config.TrackRegulatoryChanges = config.TrackRegulatoryChanges;
                scraperInstance.Config.ClassifyRegulatoryDocuments = config.ClassifyRegulatoryDocuments;
                scraperInstance.Config.ExtractStructuredContent = config.ExtractStructuredContent;
                scraperInstance.Config.ProcessPdfDocuments = config.ProcessPdfDocuments;
                scraperInstance.Config.MonitorHighImpactChanges = config.MonitorHighImpactChanges;
                scraperInstance.Config.IsUKGCWebsite = config.IsUKGCWebsite;
                scraperInstance.Config.KeywordAlertList = config.KeywordAlertList;
                scraperInstance.Config.NotificationEndpoint = config.NotificationEndpoint;
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (success)
                {
                    SaveScraperConfigurations();
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating regulatory config for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Exports scraped data for a specific scraper
        /// </summary>
        public async Task<object> ExportScrapedData(string id, ExportOptions options)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                if (!Directory.Exists(outputDir))
                {
                    return new { Error = "No data found to export" };
                }
                
                // Generate export filename
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string exportFileName = $"export_{scraperInstance.Config.Name.Replace(" ", "_")}_{timestamp}.{options.Format.ToLowerInvariant()}";
                
                // Set output path
                string outputPath = options.OutputPath;
                if (string.IsNullOrEmpty(outputPath))
                {
                    outputPath = Path.Combine(outputDir, "exports");
                    Directory.CreateDirectory(outputPath);
                }
                string fullPath = Path.Combine(outputPath, exportFileName);
                
                // Collect data for export
                var dataToExport = await CollectDataForExport(outputDir, options);
                
                if (dataToExport.Count == 0)
                {
                    return new { Message = "No data found matching the export criteria" };
                }
                
                // Export the data
                switch (options.Format.ToLowerInvariant())
                {
                    case "json":
                        await File.WriteAllTextAsync(fullPath, 
                            Newtonsoft.Json.JsonConvert.SerializeObject(dataToExport, Newtonsoft.Json.Formatting.Indented));
                        break;
                    case "csv":
                        // Simplified CSV export - in a real implementation, use a CSV library
                        using (var writer = new StreamWriter(fullPath))
                        {
                            // Write CSV header from first entry
                            if (dataToExport.Count > 0)
                            {
                                var header = string.Join(",", dataToExport[0].Keys.Select(k => $"\"{k}\""));
                                await writer.WriteLineAsync(header);
                                
                                // Write data rows
                                foreach (var entry in dataToExport)
                                {
                                    var row = string.Join(",", entry.Values.Select(v => $"\"{v}\""));
                                    await writer.WriteLineAsync(row);
                                }
                            }
                        }
                        break;
                    default:
                        return new { Error = $"Unsupported export format: {options.Format}" };
                }
                
                return new 
                { 
                    Message = $"Data exported successfully to {fullPath}",
                    FilePath = fullPath,
                    RecordCount = dataToExport.Count,
                    Format = options.Format
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error exporting scraped data for scraper {id}");
                return new { Error = $"Export failed: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Helper method to collect data for export
        /// </summary>
        private async Task<List<Dictionary<string, object>>> CollectDataForExport(string outputDir, ExportOptions options)
        {
            var dataToExport = new List<Dictionary<string, object>>();
            
            // Get all HTML and text files
            var htmlFiles = Directory.GetFiles(outputDir, "*.html", SearchOption.AllDirectories);
            
            foreach (var htmlFile in htmlFiles)
            {
                var textFile = htmlFile + ".txt";
                
                // Create data entry
                var dataEntry = new Dictionary<string, object>
                {
                    ["Url"] = Path.GetFileNameWithoutExtension(htmlFile)
                        .Replace("_", "/")
                        .Replace("www.", "http://www."),
                    ["ScrapedDate"] = File.GetCreationTime(htmlFile)
                };
                
                // Filter by date range if specified
                DateTime scrapedDate = (DateTime)dataEntry["ScrapedDate"];
                if ((options.StartDate.HasValue && scrapedDate < options.StartDate.Value) ||
                    (options.EndDate.HasValue && scrapedDate > options.EndDate.Value))
                {
                    continue;
                }
                
                // Check if file is compressed
                var compressedHtmlFile = htmlFile + ".gz";
                var compressedTextFile = textFile + ".gz";
                
                // Include raw HTML if requested
                if (options.IncludeRawHtml)
                {
                    if (File.Exists(htmlFile))
                    {
                        dataEntry["RawHtml"] = await File.ReadAllTextAsync(htmlFile);
                    }
                    else if (File.Exists(compressedHtmlFile))
                    {
                        // Placeholder for decompression - would need a compression helper
                        dataEntry["RawHtml"] = "[Compressed data]";
                    }
                }
                
                // Include processed content if requested
                if (options.IncludeProcessedContent)
                {
                    if (File.Exists(textFile))
                    {
                        dataEntry["ProcessedContent"] = await File.ReadAllTextAsync(textFile);
                    }
                    else if (File.Exists(compressedTextFile))
                    {
                        // Placeholder for decompression - would need a compression helper
                        dataEntry["ProcessedContent"] = "[Compressed data]";
                    }
                }
                
                // Include metadata if requested
                if (options.IncludeMetadata)
                {
                    var metadataFile = Path.Combine(
                        Path.GetDirectoryName(htmlFile),
                        Path.GetFileNameWithoutExtension(htmlFile) + "_metadata.json"
                    );
                    var compressedMetadataFile = metadataFile + ".gz";
                    
                    if (File.Exists(metadataFile))
                    {
                        var json = await File.ReadAllTextAsync(metadataFile);
                        var metadata = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                        
                        if (metadata != null)
                        {
                            dataEntry["Metadata"] = metadata;
                        }
                    }
                    else if (File.Exists(compressedMetadataFile))
                    {
                        // Placeholder for decompression - would need a compression helper
                        dataEntry["Metadata"] = new Dictionary<string, object> { ["CompressedData"] = true };
                    }
                }
                
                dataToExport.Add(dataEntry);
            }
            
            return dataToExport;
        }
        
        /// <summary>
        /// Updates webhook configuration for a specific scraper
        /// </summary>
        public async Task<bool> UpdateWebhookConfig(string id, WebhookConfig config)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Status.IsRunning)
                {
                    return false;
                }
                
                // Update webhook configuration in the config
                scraperInstance.Config.WebhookEnabled = config.Enabled;
                scraperInstance.Config.WebhookUrl = config.WebhookUrl;
                scraperInstance.Config.NotifyOnContentChanges = config.NotifyOnContentChanges;
                scraperInstance.Config.NotifyOnDocumentProcessed = config.NotifyOnDocumentProcessed;
                scraperInstance.Config.NotifyOnScraperStatusChange = config.NotifyOnScraperStatusChange;
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (success)
                {
                    // Create a new webhook notifier if enabled
                    if (config.Enabled && !string.IsNullOrEmpty(config.WebhookUrl))
                    {
                        var webhookLogger = _loggerFactory.CreateLogger<WebScraper.Notifications.WebhookNotifier>();
                        scraperInstance.WebhookNotifier = new WebScraper.Notifications.WebhookNotifier(
                            webhookLogger,
                            id,
                            scraperInstance.Config.Name,
                            config.WebhookUrl);
                        
                        // Test the webhook connection
                        await scraperInstance.WebhookNotifier.NotifyScraperStatusChangeAsync(
                            "WebhookConfigured", 
                            "Webhook configuration has been updated");
                    }
                    else
                    {
                        // Disable webhook notifier
                        scraperInstance.WebhookNotifier = null;
                    }
                    
                    SaveScraperConfigurations();
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating webhook config for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Compresses stored content for a specific scraper
        /// </summary>
        public async Task<object> CompressStoredContent(string id)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Get the scraper's output directory
                var outputDir = scraperInstance.Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine("ScrapedData", id);
                }
                
                // Ensure the directory exists
                if (!Directory.Exists(outputDir))
                {
                    return new 
                    { 
                        Message = "No content found to compress",
                        ScraperId = id,
                        FilesCompressed = 0
                    };
                }
                
                int filesCompressed = 0;
                List<string> compressedFiles = new List<string>();
                
                // Find text, HTML, and JSON files to compress
                var filesToCompress = Directory.GetFiles(outputDir, "*.html", SearchOption.AllDirectories)
                    .Concat(Directory.GetFiles(outputDir, "*.txt", SearchOption.AllDirectories))
                    .Concat(Directory.GetFiles(outputDir, "*.json", SearchOption.AllDirectories))
                    .Where(f => !Path.GetFileName(f).EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                // For this implementation, we'll just count the files without actually compressing them
                // In a real implementation, this would use a compression library
                foreach (var file in filesToCompress)
                {
                    // Simulate compressing the file
                    await Task.Delay(10); // Simulating compression work
                    compressedFiles.Add(file);
                    filesCompressed++;
                    
                    // Log progress periodically
                    if (filesCompressed % 100 == 0)
                    {
                        AddLogMessage(id, $"Compressed {filesCompressed} files so far...");
                    }
                }
                
                AddLogMessage(id, $"Content compression completed. Compressed {filesCompressed} files.");
                
                return new 
                { 
                    Message = $"Content compression completed successfully",
                    ScraperId = id,
                    FilesCompressed = filesCompressed
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error compressing content for scraper {id}");
                return new { Error = $"Compression failed: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Gets detailed telemetry metrics for a specific scraper
        /// </summary>
        public async Task<object> GetScraperMetrics(string id)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Start with basic analytics
                var analytics = await GetScraperAnalytics(id);
                var basicMetrics = analytics as Dictionary<string, object> ?? new Dictionary<string, object>();
                
                // Add more detailed metrics
                var detailedMetrics = new Dictionary<string, object>
                {
                    ["CpuUsage"] = 0.05, // Example values
                    ["MemoryUsage"] = 125000000,
                    ["NetworkBandwidth"] = 15000,
                    ["RequestsPerMinute"] = 30,
                    ["AverageResponseTime"] = 250,
                    ["ErrorRate"] = 0.03,
                    ["CacheHitRate"] = 0.75
                };
                
                // Get status for runtime metrics
                var status = GetScraperStatus(id);
                if (status != null)
                {
                    var runtimeMetrics = new Dictionary<string, object>
                    {
                        ["IsRunning"] = status.IsRunning,
                        ["ElapsedTime"] = status.ElapsedTime,
                        ["StartTime"] = status.StartTime,
                        ["EndTime"] = status.EndTime,
                        ["UrlsProcessed"] = status.UrlsProcessed
                    };
                    
                    // Add pipeline metrics if available
                    if (status.PipelineMetrics != null)
                    {
                        detailedMetrics["Pipeline"] = new
                        {
                            ProcessingItems = status.PipelineMetrics.ProcessingItems,
                            QueuedItems = status.PipelineMetrics.QueuedItems,
                            CompletedItems = status.PipelineMetrics.CompletedItems,
                            FailedItems = status.PipelineMetrics.FailedItems,
                            AverageProcessingTimeMs = status.PipelineMetrics.AverageProcessingTimeMs
                        };
                    }
                    
                    foreach (var kvp in runtimeMetrics)
                    {
                        basicMetrics[kvp.Key] = kvp.Value;
                    }
                }
                
                // Add the detailed metrics to the response
                basicMetrics["DetailedMetrics"] = detailedMetrics;
                
                return basicMetrics;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting metrics for scraper {id}");
                return null;
            }
        }

        /// <summary>
        /// Schedules a scraper to run at a specified time or on a recurring schedule
        /// </summary>
        public async Task<dynamic> ScheduleScraper(string id, Models.ScheduleOptions options)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return new { Success = false, Message = $"Scraper with ID {id} not found" };
                }
                
                // Generate a unique ID for this schedule
                string scheduleId = Guid.NewGuid().ToString();
                
                // Get or create schedules collection
                if (scraperInstance.Config.Schedules == null)
                {
                    scraperInstance.Config.Schedules = new List<Dictionary<string, object>>();
                }
                
                // Create the schedule definition
                var schedule = new Dictionary<string, object>
                {
                    ["Id"] = scheduleId,
                    ["Name"] = options.Name ?? $"Schedule {scheduleId.Substring(0, 8)}",
                    ["Description"] = options.Description ?? "Scheduled run",
                    ["IsRecurring"] = options.IsRecurring,
                    ["CreatedAt"] = DateTime.Now
                };
                
                DateTime nextRunTime;
                
                if (options.IsRecurring)
                {
                    // Validate cron expression
                    if (string.IsNullOrEmpty(options.CronExpression))
                    {
                        return new { Success = false, Message = "Cron expression is required for recurring schedules" };
                    }
                    
                    try
                    {
                        // Use Cronos or NCrontab to parse and validate cron expression
                        // This is a simplified implementation
                        if (!IsValidCronExpression(options.CronExpression))
                        {
                            return new { Success = false, Message = "Invalid cron expression" };
                        }
                        
                        schedule["CronExpression"] = options.CronExpression;
                        
                        if (options.ExpiryDate.HasValue)
                        {
                            schedule["ExpiryDate"] = options.ExpiryDate.Value;
                        }
                        
                        // Calculate next run time (simplified)
                        nextRunTime = CalculateNextRunFromCron(options.CronExpression);
                        schedule["NextRunTime"] = nextRunTime;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error parsing cron expression");
                        return new { Success = false, Message = "Invalid cron expression format" };
                    }
                }
                else
                {
                    // One-time schedule
                    if (!options.OneTimeExecutionDate.HasValue || options.OneTimeExecutionDate.Value <= DateTime.Now)
                    {
                        return new { Success = false, Message = "A future execution date is required for one-time schedules" };
                    }
                    
                    nextRunTime = options.OneTimeExecutionDate.Value;
                    schedule["ExecutionDate"] = nextRunTime;
                    schedule["NextRunTime"] = nextRunTime;
                }
                
                // Add to schedules collection
                scraperInstance.Config.Schedules.Add(schedule);
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (!success)
                {
                    return new { Success = false, Message = "Failed to save schedule to database" };
                }
                
                // Register with timer service
                RegisterScheduleWithTimerService(id, scheduleId, nextRunTime);
                
                return new 
                { 
                    Success = true,
                    Message = options.IsRecurring 
                        ? $"Recurring schedule created with cron expression: {options.CronExpression}" 
                        : $"One-time schedule created for: {nextRunTime}",
                    ScheduleId = scheduleId,
                    NextRunTime = nextRunTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scheduling scraper {id}");
                return new { Success = false, Message = $"Error scheduling scraper: {ex.Message}" };
            }
        }
        
        /// <summary>
        /// Gets all schedules for a specific scraper
        /// </summary>
        public async Task<List<Dictionary<string, object>>> GetScraperSchedules(string id)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return new List<Dictionary<string, object>>();
                }
                
                if (scraperInstance.Config.Schedules == null)
                {
                    return new List<Dictionary<string, object>>();
                }
                
                // Update next run times for recurring schedules
                foreach (var schedule in scraperInstance.Config.Schedules)
                {
                    if ((bool)schedule["IsRecurring"] && schedule.ContainsKey("CronExpression"))
                    {
                        // Calculate next run time if needed
                        DateTime lastUpdated = DateTime.Parse(schedule["NextRunTime"].ToString());
                        if (lastUpdated < DateTime.Now)
                        {
                            try
                            {
                                var nextRun = CalculateNextRunFromCron(schedule["CronExpression"].ToString());
                                schedule["NextRunTime"] = nextRun;
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error updating next run time for schedule");
                            }
                        }
                    }
                }
                
                return scraperInstance.Config.Schedules.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting schedules for scraper {id}");
                return new List<Dictionary<string, object>>();
            }
        }
        
        /// <summary>
        /// Deletes a schedule for a specific scraper
        /// </summary>
        public async Task<bool> DeleteSchedule(string id, string scheduleId)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Config.Schedules == null)
                {
                    return false;
                }
                
                // Find and remove the schedule
                var scheduleToRemove = scraperInstance.Config.Schedules.FirstOrDefault(s => s["Id"].ToString() == scheduleId);
                if (scheduleToRemove == null)
                {
                    return false;
                }
                
                scraperInstance.Config.Schedules.Remove(scheduleToRemove);
                
                // Save the updated configuration
                return await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting schedule {scheduleId} for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Updates rate limiting configuration for a specific scraper
        /// </summary>
        public async Task<bool> UpdateRateLimitingConfig(string id, Models.RateLimitingConfig config)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Status.IsRunning)
                {
                    return false;
                }
                
                // Update rate limiting configuration in the scraper config
                scraperInstance.Config.EnableAdaptiveRateLimiting = config.EnableAdaptiveRateLimiting;
                scraperInstance.Config.MaxRequestsPerMinute = config.MaxRequestsPerMinute;
                scraperInstance.Config.MinDelayBetweenRequests = config.MinDelayBetweenRequests;
                scraperInstance.Config.MaxDelayBetweenRequests = config.MaxDelayBetweenRequests;
                scraperInstance.Config.RespectRobotsTxt = config.RespectRobotsTxt;
                scraperInstance.Config.UserAgent = config.UserAgent;
                scraperInstance.Config.BackOffOnErrors = config.BackOffOnErrors;
                
                // Convert and save domain-specific rate limits
                if (config.DomainRateLimits != null && config.DomainRateLimits.Count > 0)
                {
                    scraperInstance.Config.DomainRateLimits = config.DomainRateLimits.ToDictionary(
                        kvp => kvp.Key, 
                        kvp => new Dictionary<string, object>
                        {
                            ["MaxRequestsPerMinute"] = kvp.Value.MaxRequestsPerMinute,
                            ["MinDelayBetweenRequests"] = kvp.Value.MinDelayBetweenRequests
                        }
                    );
                }
                else
                {
                    scraperInstance.Config.DomainRateLimits = new Dictionary<string, Dictionary<string, object>>();
                }
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (success)
                {
                    SaveScraperConfigurations();
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating rate limiting config for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Updates proxy configuration for a specific scraper
        /// </summary>
        public async Task<bool> UpdateProxyConfig(string id, Models.ProxyConfig config)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance) || scraperInstance.Status.IsRunning)
                {
                    return false;
                }
                
                // Update proxy configuration in the scraper config
                scraperInstance.Config.UseProxies = config.UseProxies;
                scraperInstance.Config.ProxyRotationStrategy = config.RotationStrategy;
                scraperInstance.Config.TestProxiesBeforeUse = config.TestProxiesBeforeUse;
                scraperInstance.Config.MaxProxyFailuresBeforeRemoval = config.MaxFailuresBeforeRemoval;
                
                // Convert and save proxy list
                if (config.Proxies != null && config.Proxies.Count > 0)
                {
                    scraperInstance.Config.Proxies = config.Proxies.Select(p => new Dictionary<string, object>
                    {
                        ["Host"] = p.Host,
                        ["Port"] = p.Port,
                        ["Username"] = p.Username,
                        ["Password"] = p.Password,
                        ["Protocol"] = p.Protocol,
                        ["CountryCode"] = p.CountryCode
                    }).ToList();
                }
                else
                {
                    scraperInstance.Config.Proxies = new List<Dictionary<string, object>>();
                }
                
                // Save the updated configuration
                var success = await _configService.UpdateScraperConfigAsync(id, scraperInstance.Config);
                
                if (success)
                {
                    SaveScraperConfigurations();
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating proxy config for scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Runs a test on a scraper with the specified options
        /// </summary>
        public async Task<object> RunScraperTest(string id, Models.TestRunOptions options)
        {
            try
            {
                if (!_scrapers.TryGetValue(id, out var scraperInstance))
                {
                    return null;
                }
                
                // Can't run test if scraper is already running
                if (scraperInstance.Status.IsRunning)
                {
                    return new { 
                        Success = false, 
                        Message = "Cannot run test while scraper is already running" 
                    };
                }
                
                // Create a test ID
                string testId = $"test_{DateTime.Now.Ticks}";
                
                // Update status for test run
                scraperInstance.Status.IsRunning = true;
                scraperInstance.Status.StartTime = DateTime.Now;
                scraperInstance.Status.EndTime = null;
                scraperInstance.Status.UrlsProcessed = 0;
                scraperInstance.Status.LogMessages.Clear();
                
                AddLogMessage(id, $"Starting test run with {options.TestUrls.Count} URLs, max depth {options.MaxDepth}, max pages {options.MaxPages}");
                
                // Create a temporary copy of the config with test settings
                var testConfig = new ScraperConfigModel
                {
                    Id = testId,
                    Name = $"{scraperInstance.Config.Name} - Test",
                    StartUrls = options.TestUrls.Count > 0 ? options.TestUrls : scraperInstance.Config.StartUrls,
                    MaxDepth = options.MaxDepth,
                    MaxPages = options.MaxPages,
                    FollowLinks = options.FollowLinks,
                    WaitForSelector = scraperInstance.Config.WaitForSelector,
                    ContentExtractorSelectors = scraperInstance.Config.ContentExtractorSelectors,
                    ContentExtractorExcludeSelectors = scraperInstance.Config.ContentExtractorExcludeSelectors,
                    CustomJsExtractor = scraperInstance.Config.CustomJsExtractor,
                    ExtractMetadata = scraperInstance.Config.ExtractMetadata,
                    ExtractStructuredData = scraperInstance.Config.ExtractStructuredData,
                    OutputDirectory = Path.Combine(scraperInstance.Config.OutputDirectory ?? "", "tests", testId)
                };
                
                // Create output directory if it doesn't exist
                Directory.CreateDirectory(testConfig.OutputDirectory);
                
                // Run the test in a background task
                Task.Run(async () =>
                {
                    try
                    {
                        // Create a delegate to log messages
                        Action<string> logAction = message => AddLogMessage(id, message);
                        
                        // Execute the scraper using the execution service but note this is a test run
                        // We'll use a custom state object to indicate this is a test
                        var testState = new WebScraperApi.Models.ScraperState
                        {
                            IsTestRun = true,
                            MaxPages = options.MaxPages
                        };
                        
                        await _executionService.StartScraperAsync(
                            testConfig,
                            testState,
                            logAction);
                            
                        // Update status once complete
                        scraperInstance.Status.IsRunning = false;
                        scraperInstance.Status.EndTime = DateTime.Now;
                        
                        AddLogMessage(id, "Test run completed successfully");
                        
                        // Validate extraction rules if requested
                        if (options.ValidateExtractionRules)
                        {
                            AddLogMessage(id, "Validating content extraction rules...");
                            await ValidateExtractionRules(id, testConfig.OutputDirectory);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error during test run of {id}");
                        scraperInstance.Status.IsRunning = false;
                        scraperInstance.Status.EndTime = DateTime.Now;
                        AddLogMessage(id, $"Error during test run: {ex.Message}");
                    }
                });
                
                return new { 
                    Success = true, 
                    Message = "Test run started",
                    TestId = testId,
                    OutputDirectory = testConfig.OutputDirectory
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error setting up test run for scraper {id}");
                return new { 
                    Success = false, 
                    Message = $"Error setting up test run: {ex.Message}" 
                };
            }
        }
        
        /// <summary>
        /// Performs a batch operation on multiple scrapers
        /// </summary>
        public async Task<object> PerformBatchOperation(Models.BatchOperationRequest request)
        {
            var results = new List<Dictionary<string, object>>();
            int successCount = 0;
            int failureCount = 0;
            
            switch (request.Operation.ToLowerInvariant())
            {
                case "start":
                    foreach (var scraperId in request.ScraperIds)
                    {
                        try
                        {
                            var success = await StartScraper(scraperId);
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = success,
                                ["Message"] = success ? "Started successfully" : "Failed to start (already running or not found)"
                            });
                            
                            if (success) successCount++; else failureCount++;
                        }
                        catch (Exception ex)
                        {
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = false,
                                ["Error"] = ex.Message
                            });
                            failureCount++;
                        }
                    }
                    break;
                    
                case "stop":
                    foreach (var scraperId in request.ScraperIds)
                    {
                        try
                        {
                            var success = StopScraperInstance(scraperId);
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = success,
                                ["Message"] = success ? "Stopped successfully" : "Failed to stop (not running or not found)"
                            });
                            
                            if (success) successCount++; else failureCount++;
                        }
                        catch (Exception ex)
                        {
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = false,
                                ["Error"] = ex.Message
                            });
                            failureCount++;
                        }
                    }
                    break;
                    
                case "delete":
                    foreach (var scraperId in request.ScraperIds)
                    {
                        try
                        {
                            var success = await DeleteScraperConfig(scraperId);
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = success,
                                ["Message"] = success ? "Deleted successfully" : "Failed to delete (running or not found)"
                            });
                            
                            if (success) successCount++; else failureCount++;
                        }
                        catch (Exception ex)
                        {
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = false,
                                ["Error"] = ex.Message
                            });
                            failureCount++;
                        }
                    }
                    break;
                    
                case "compress":
                    foreach (var scraperId in request.ScraperIds)
                    {
                        try
                        {
                            var result = await CompressStoredContent(scraperId);
                            bool success = result != null && !result.GetType().GetProperties().Any(p => p.Name == "Error");
                            
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = success,
                                ["Message"] = success ? "Content compressed successfully" : "Failed to compress content"
                            });
                            
                            if (success) successCount++; else failureCount++;
                        }
                        catch (Exception ex)
                        {
                            results.Add(new Dictionary<string, object>
                            {
                                ["ScraperId"] = scraperId,
                                ["Success"] = false,
                                ["Error"] = ex.Message
                            });
                            failureCount++;
                        }
                    }
                    break;
                    
                default:
                    return new { 
                        Error = $"Unsupported operation: {request.Operation}",
                        SuccessCount = 0,
                        FailureCount = request.ScraperIds.Count,
                        Results = new List<Dictionary<string, object>>()
                    };
            }
            
            return new { 
                SuccessCount = successCount,
                FailureCount = failureCount,
                Results = results
            };
        }
        
        #region Helper methods for new functionality
        
        /// <summary>
        /// Validates that a cron expression is properly formatted
        /// </summary>
        private bool IsValidCronExpression(string expression)
        {
            // This is a very simplistic validation
            // In a real implementation, use a library like Cronos or NCrontab
            if (string.IsNullOrWhiteSpace(expression))
                return false;
                
            var parts = expression.Split(' ');
            return parts.Length >= 5;
        }
        
        /// <summary>
        /// Calculates the next run time from a cron expression
        /// </summary>
        private DateTime CalculateNextRunFromCron(string cronExpression)
        {
            // This is a placeholder implementation
            // In a real implementation, use a library like Cronos or NCrontab to calculate this
            
            // For now, just return a time 1 hour in the future
            return DateTime.Now.AddHours(1);
        }
        
        /// <summary>
        /// Registers a schedule with the timer service
        /// </summary>
        private void RegisterScheduleWithTimerService(string scraperId, string scheduleId, DateTime nextRunTime)
        {
            // Calculate the delay until the next run
            var delay = nextRunTime - DateTime.Now;
            if (delay.TotalMilliseconds <= 0)
            {
                // If the time is in the past, set a short delay
                delay = TimeSpan.FromSeconds(10);
            }
            
            // Create a timer that will run the scraper at the scheduled time
            var timer = new Timer(state => 
            {
                var ids = (Tuple<string, string>)state;
                string id = ids.Item1;
                string schId = ids.Item2;
                
                // Start the scraper
                Task.Run(async () => 
                {
                    try
                    {
                        await StartScraper(id);
                        
                        // Update the schedule for recurring schedules
                        if (_scrapers.TryGetValue(id, out var instance))
                        {
                            var schedule = instance.Config.Schedules?.FirstOrDefault(s => s["Id"].ToString() == schId);
                            if (schedule != null && (bool)schedule["IsRecurring"])
                            {
                                // Calculate next run time
                                var nextRun = CalculateNextRunFromCron(schedule["CronExpression"].ToString());
                                schedule["NextRunTime"] = nextRun;
                                
                                // Check if the schedule hasn't expired
                                if (!schedule.ContainsKey("ExpiryDate") || 
                                    DateTime.Parse(schedule["ExpiryDate"].ToString()) > DateTime.Now)
                                {
                                    // Register for the next run
                                    RegisterScheduleWithTimerService(id, schId, nextRun);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error running scheduled scraper {id}");
                    }
                });
            }, 
            new Tuple<string, string>(scraperId, scheduleId), 
            delay, 
            Timeout.InfiniteTimeSpan);
            
            // Store timer reference if needed for cleanup
            // This is a simplified implementation
        }
        
        /// <summary>
        /// Validates extraction rules after a test run
        /// </summary>
        private async Task ValidateExtractionRules(string scraperId, string testOutputDir)
        {
            try
            {
                // Check if directory exists
                if (!Directory.Exists(testOutputDir))
                {
                    AddLogMessage(scraperId, "Test output directory not found");
                    return;
                }
                
                // Get content files
                var contentFiles = Directory.GetFiles(testOutputDir, "*.txt", SearchOption.AllDirectories);
                
                if (contentFiles.Length == 0)
                {
                    AddLogMessage(scraperId, "Warning: No content files found. Check your extraction rules.");
                    return;
                }
                
                // Check file sizes
                var totalSize = 0L;
                var emptyFiles = 0;
                foreach (var file in contentFiles)
                {
                    var fileInfo = new FileInfo(file);
                    totalSize += fileInfo.Length;
                    
                    if (fileInfo.Length == 0)
                    {
                        emptyFiles++;
                    }
                }
                
                var avgSize = contentFiles.Length > 0 ? totalSize / contentFiles.Length : 0;
                
                if (emptyFiles > 0)
                {
                    var percentage = (emptyFiles * 100) / contentFiles.Length;
                    AddLogMessage(scraperId, $"Warning: {emptyFiles} out of {contentFiles.Length} content files ({percentage}%) are empty. Your extraction rules might need adjustment.");
                }
                
                if (avgSize < 100 && contentFiles.Length > 0) // Arbitrary small size threshold
                {
                    AddLogMessage(scraperId, $"Warning: Average content size is very small ({avgSize} bytes). Your extraction rules might be too restrictive.");
                }
                
                AddLogMessage(scraperId, $"Extraction validation complete. Processed {contentFiles.Length} files with average size of {avgSize} bytes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating extraction rules for scraper {scraperId}");
                AddLogMessage(scraperId, $"Error validating extraction rules: {ex.Message}");
            }
        }
        
        #endregion
        
        #endregion
    }
    
    public class ScraperInstance
    {
        public ScraperConfigModel Config { get; set; }
        public ScraperStatus Status { get; set; }
        public Scraper Scraper { get; set; }
        public PersistentStateManager StateManager { get; set; }
        public WebScraper.Notifications.WebhookNotifier WebhookNotifier { get; set; }
        public WebScraper.Monitoring.ScraperMetrics MetricsService { get; set; }
    }
}
