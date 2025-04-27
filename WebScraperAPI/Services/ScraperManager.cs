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
    }
    
    public class ScraperInstance
    {
        public ScraperConfigModel Config { get; set; }
        public ScraperStatus Status { get; set; }
        public Scraper Scraper { get; set; }
        public PersistentStateManager StateManager { get; set; }
    }
}
