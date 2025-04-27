using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebScraper;
using WebScraper.HeadlessBrowser;
using WebScraper.RegulatoryContent;
using WebScraper.StateManagement;
using WebScraper.Validation;
using WebScraperApi.Models;
using WebScraperAPI.Data.Entities;
using WebScraperAPI.Data.Repositories;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Manages scraper configurations, execution, and monitoring
    /// </summary>
    public class ScraperManager : IHostedService, IDisposable
    {
        private readonly ILogger<ScraperManager> _logger;
        private readonly IScraperConfigRepository _configRepository;
        private readonly IScrapedContentRepository _contentRepository;
        private readonly ICacheRepository _cacheRepository;
        private readonly string _configFilePath = "scraperConfigs.json";
        private readonly Dictionary<string, ScraperInstance> _scrapers = new();
        private readonly string _stateDbPath;
        private Timer _monitoringTimer;
        private readonly ILoggerFactory _loggerFactory;
        
        /// <summary>
        /// Initializes a new instance of the ScraperManager class
        /// </summary>
        /// <param name="logger">The logger</param>
        /// <param name="configRepository">The scraper configuration repository</param>
        /// <param name="contentRepository">The scraped content repository</param>
        /// <param name="cacheRepository">The cache repository</param>
        /// <param name="loggerFactory">The logger factory</param>
        public ScraperManager(
            ILogger<ScraperManager> logger,
            IScraperConfigRepository configRepository,
            IScrapedContentRepository contentRepository,
            ICacheRepository cacheRepository,
            ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _configRepository = configRepository;
            _contentRepository = contentRepository;
            _cacheRepository = cacheRepository;
            _loggerFactory = loggerFactory;
            
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
                var configs = await _configRepository.GetAllAsync();
                var models = configs.Select(ToModel).ToList();
                
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
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return null;
                }

                var entity = await _configRepository.GetByIdAsync(guidId);
                if (entity == null)
                {
                    return null;
                }

                var model = ToModel(entity);

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
                // Generate a new ID if none provided
                if (string.IsNullOrEmpty(config.Id))
                {
                    config.Id = Guid.NewGuid().ToString();
                }
                
                // Ensure creation date is set
                if (config.CreatedAt == default)
                {
                    config.CreatedAt = DateTime.Now;
                }
                
                // Convert to entity for the repository
                var entity = ToEntity(config);
                
                // Save to repository
                await _configRepository.CreateAsync(entity);
                // No need to call SaveChangesAsync as it's handled internally by CreateAsync
                
                // Get the saved entity with generated ID
                config.Id = entity.Id.ToString();
                
                // Add to in-memory cache
                lock (_scrapers)
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
                
                _logger.LogInformation($"Created scraper configuration: {config.Name} ({config.Id})");
                return config;
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
                // Ensure the ID stays the same
                config.Id = id;

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

                // Convert to entity for the repository
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return false;
                }

                // Get the existing entity
                var existingEntity = await _configRepository.GetByIdAsync(guidId);
                if (existingEntity == null)
                {
                    _logger.LogWarning($"Scraper with ID {id} not found in database");
                    return false;
                }

                // Update entity properties
                var entity = ToEntity(config);
                
                // Update in repository
                await _configRepository.UpdateAsync(entity);
                // No need to call SaveChangesAsync as it's handled internally by UpdateAsync

                // Update in-memory cache
                lock (_scrapers)
                {
                    if (_scrapers.TryGetValue(id, out var instance))
                    {
                        // Update config but preserve status
                        instance.Config = config;
                    }
                }

                _logger.LogInformation($"Updated scraper configuration: {config.Name} ({config.Id})");
                return true;
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

                // Parse the ID to GUID
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return false;
                }

                // Delete from repository
                var entity = await _configRepository.GetByIdAsync(guidId);
                if (entity == null)
                {
                    _logger.LogWarning($"Scraper with ID {id} not found in database");
                    return false;
                }

                await _configRepository.DeleteAsync(guidId);
                // No need to call SaveChangesAsync as it's handled internally by DeleteAsync

                // Remove from in-memory cache
                lock (_scrapers)
                {
                    _scrapers.Remove(id);
                }

                _logger.LogInformation($"Deleted scraper configuration with ID {id}");
                return true;
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
                var configs = await _configRepository.GetAllAsync();
                var configModels = configs.Select(ToModel).ToList();
                
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
                
                _logger.LogInformation($"Loaded {configModels.Count} scraper configurations from database");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scraper configurations from database");
                
                // Fallback to file-based configuration if database fails
                if (File.Exists(_configFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_configFilePath);
                        var configs = JsonConvert.DeserializeObject<List<ScraperConfigModel>>(json);
                        
                        if (configs != null)
                        {
                            lock (_scrapers)
                            {
                                foreach (var config in configs)
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
                            
                            _logger.LogInformation($"Loaded {configs.Count} scraper configurations from file");
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "Error loading scraper configurations from file");
                    }
                }
            }
        }
        
        private void SaveScraperConfigurations()
        {
            lock (_scrapers)
            {
                try
                {
                    var configs = _scrapers.Values.Select(s => s.Config).ToList();
                    var json = JsonConvert.SerializeObject(configs, Formatting.Indented);
                    File.WriteAllText(_configFilePath, json);
                    
                    _logger.LogInformation($"Saved {configs.Count} scraper configurations");
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
                
                // Get the scraper configuration
                var config = scraperInstance.Config.ToScraperConfig();
                
                // Start scraping in a background task
                Task.Run(async () =>
                {
                    try
                    {
                        _logger.LogInformation($"Starting scraper {id}: {scraperInstance.Config.Name}");
                        AddLogMessage(id, $"Starting scraper: {scraperInstance.Config.Name}");
                        
                        // First validate the configuration using our new validator
                        var validator = new ConfigurationValidator(message => AddLogMessage(id, message));
                        var validationResult = await validator.ValidateConfigurationAsync(config);
                        
                        if (!validationResult.IsValid && !validationResult.CanRunWithWarnings)
                        {
                            AddLogMessage(id, "Configuration validation failed");
                            foreach (var error in validationResult.Errors)
                            {
                                AddLogMessage(id, $"Error: {error}");
                            }
                            
                            // Update status
                            scraperInstance.Status.IsRunning = false;
                            scraperInstance.Status.EndTime = DateTime.Now;
                            AddLogMessage(id, "Scraping aborted due to configuration errors");
                            return;
                        }
                        else if (validationResult.Warnings.Any())
                        {
                            AddLogMessage(id, "Configuration has warnings:");
                            foreach (var warning in validationResult.Warnings)
                            {
                                AddLogMessage(id, $"Warning: {warning}");
                            }
                            AddLogMessage(id, "Continuing with warnings...");
                        }
                        
                        // Check if regulatory features are enabled
                        bool useEnhancedScraper = IsRegulatoryFeaturesEnabled(config);
                        
                        // Also check if other enhanced features are enabled
                        useEnhancedScraper = useEnhancedScraper || 
                                            config.ProcessPdfDocuments || 
                                            config.ProcessJsHeavyPages;
                        
                        // Create a persistent state manager for this scraper
                        var stateDbConnectionString = _stateDbPath;
                        scraperInstance.StateManager = new PersistentStateManager(
                            stateDbConnectionString, 
                            message => AddLogMessage(id, message));
                        
                        await scraperInstance.StateManager.InitializeAsync();
                        
                        if (useEnhancedScraper)
                        {
                            AddLogMessage(id, "Using enhanced scraper with advanced capabilities");
                            
                            // Create the logger for the enhanced scraper
                            var scraperLogger = _loggerFactory.CreateLogger<EnhancedScraper>();
                            
                            // Create document processor if PDF processing is enabled
                            IDocumentProcessor documentProcessor = null;
                            if (config.ProcessPdfDocuments)
                            {
                                documentProcessor = new PdfDocumentHandler(
                                    config.OutputDirectory,
                                    message => AddLogMessage(id, message));
                            }
                            
                            // Create the enhanced scraper with our new components
                            scraperInstance.Scraper = new EnhancedScraper(
                                config, 
                                scraperLogger,
                                crawlStrategy: null,
                                contentExtractor: null,
                                documentProcessor: documentProcessor);
                        }
                        else
                        {
                            // Create and initialize the standard scraper
                            scraperInstance.Scraper = new Scraper(
                                config, 
                                message => AddLogMessage(id, message));
                        }
                        
                        // Initialize the scraper
                        AddLogMessage(id, "Initializing scraper...");
                        await scraperInstance.Scraper.InitializeAsync();
                        
                        // Start scraping
                        AddLogMessage(id, "Starting scraping process...");
                        await scraperInstance.Scraper.StartScrapingAsync();
                        
                        // Set up continuous monitoring if enabled
                        if (scraperInstance.Config.EnableContinuousMonitoring)
                        {
                            var interval = scraperInstance.Config.GetMonitoringInterval();
                            await scraperInstance.Scraper.SetupContinuousScrapingAsync(interval);
                            AddLogMessage(id, $"Continuous monitoring enabled with interval: {interval.TotalHours:F1} hours");
                        }
                        
                        // Update status once complete
                        scraperInstance.Status.IsRunning = false;
                        scraperInstance.Status.EndTime = DateTime.Now;
                        
                        // If enhanced scraper, get pipeline statistics
                        if (scraperInstance.Scraper is EnhancedScraper enhancedScraper)
                        {
                            var pipelineStatus = enhancedScraper.GetPipelineStatus();
                            if (pipelineStatus != null)
                            {
                                AddLogMessage(id, "Pipeline statistics:");
                                AddLogMessage(id, $" - Processed Items: {pipelineStatus.CompletedItems}");
                                AddLogMessage(id, $" - Failed Items: {pipelineStatus.FailedItems}");
                                AddLogMessage(id, $" - Average Processing Time: {pipelineStatus.AverageProcessingTimeMs:F1}ms");
                            }
                            
                            // Get persistent state information
                            var scraperState = await enhancedScraper.GetStateAsync();
                            if (scraperState != null)
                            {
                                scraperInstance.Status.UrlsProcessed = scraperState.UrlsProcessed;
                            }
                        }
                        
                        AddLogMessage(id, "Scraping completed successfully");
                        
                        // Get and log regulatory statistics if applicable
                        if (useEnhancedScraper && scraperInstance.Scraper is EnhancedScraper enhScraper)
                        {
                            var stats = enhScraper.GetRegulatoryStatistics();
                            AddLogMessage(id, "Regulatory Statistics:");
                            AddLogMessage(id, stats);
                            
                            // Get high importance regulatory documents
                            var highImportanceDocuments = enhScraper.GetHighImportanceDocuments();
                            if (highImportanceDocuments.Any())
                            {
                                AddLogMessage(id, $"Found {highImportanceDocuments.Count} high importance regulatory documents:");
                                foreach (var doc in highImportanceDocuments.Take(5)) // Show only top 5 in log
                                {
                                    AddLogMessage(id, $" - {doc.Title} ({doc.Importance})");
                                }
                            }
                        }
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
                
                // Stop the scraper's continuous monitoring if it's running
                scraperInstance.Scraper?.StopContinuousScraping();
                
                AddLogMessage(id, "Scraping stopped by user");
                
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
                                        QueuedItems = metrics.QueuedItems,
                                        CompletedItems = metrics.CompletedItems,
                                        FailedItems = metrics.FailedItems,
                                        AverageProcessingTimeMs = metrics.AverageProcessingTimeMs
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
        
        /// <summary>
        /// Determines if regulatory features are enabled in the scraper configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <returns>True if regulatory features are enabled, otherwise false</returns>
        private bool IsRegulatoryFeaturesEnabled(ScraperConfig config)
        {
            // Check if any regulatory-specific features are enabled
            return config.EnableRegulatoryContentAnalysis ||
                   config.TrackRegulatoryChanges ||
                   config.ClassifyRegulatoryDocuments ||
                   config.ExtractStructuredContent ||
                   config.ProcessPdfDocuments ||
                   config.MonitorHighImpactChanges ||
                   config.IsUKGCWebsite;
        }
        
        #endregion
        
        #region Mapping Methods
        
        /// <summary>
        /// Converts a ScraperConfig entity to a ScraperConfigModel
        /// </summary>
        /// <param name="entity">The entity to convert</param>
        /// <returns>The converted model</returns>
        private ScraperConfigModel ToModel(WebScraperAPI.Data.Entities.ScraperConfig entity)
        {
            return new ScraperConfigModel
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                LastRun = entity.LastRun,
                StartUrl = entity.StartUrl,
                BaseUrl = entity.BaseUrl,
                OutputDirectory = entity.OutputDirectory,
                DelayBetweenRequests = entity.DelayBetweenRequests,
                MaxConcurrentRequests = entity.MaxConcurrentRequests,
                MaxDepth = entity.MaxDepth,
                FollowExternalLinks = entity.FollowExternalLinks,
                RespectRobotsTxt = entity.RespectRobotsTxt,
                AutoLearnHeaderFooter = entity.AutoLearnHeaderFooter,
                LearningPagesCount = entity.LearningPagesCount,
                EnableChangeDetection = entity.EnableChangeDetection,
                TrackContentVersions = entity.TrackContentVersions,
                MaxVersionsToKeep = entity.MaxVersionsToKeep,
                EnableAdaptiveCrawling = entity.EnableAdaptiveCrawling,
                PriorityQueueSize = entity.PriorityQueueSize,
                AdjustDepthBasedOnQuality = entity.AdjustDepthBasedOnQuality,
                EnableAdaptiveRateLimiting = entity.EnableAdaptiveRateLimiting,
                MinDelayBetweenRequests = entity.MinDelayBetweenRequests,
                MaxDelayBetweenRequests = entity.MaxDelayBetweenRequests,
                MonitorResponseTimes = entity.MonitorResponseTimes,
                EnableContinuousMonitoring = entity.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = entity.MonitoringIntervalMinutes,
                NotifyOnChanges = entity.NotifyOnChanges,
                NotificationEmail = entity.NotificationEmail,
                TrackChangesHistory = entity.TrackChangesHistory
            };
        }
        
        /// <summary>
        /// Converts a ScraperConfigModel to a ScraperConfig entity
        /// </summary>
        /// <param name="model">The model to convert</param>
        /// <returns>The converted entity</returns>
        private WebScraperAPI.Data.Entities.ScraperConfig ToEntity(ScraperConfigModel model)
        {
            return new WebScraperAPI.Data.Entities.ScraperConfig
            {
                Id = Guid.Parse(model.Id),
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                LastRun = model.LastRun,
                StartUrl = model.StartUrl,
                BaseUrl = model.BaseUrl,
                OutputDirectory = model.OutputDirectory,
                DelayBetweenRequests = model.DelayBetweenRequests,
                MaxConcurrentRequests = model.MaxConcurrentRequests,
                MaxDepth = model.MaxDepth,
                FollowExternalLinks = model.FollowExternalLinks,
                RespectRobotsTxt = model.RespectRobotsTxt,
                AutoLearnHeaderFooter = model.AutoLearnHeaderFooter,
                LearningPagesCount = model.LearningPagesCount,
                EnableChangeDetection = model.EnableChangeDetection,
                TrackContentVersions = model.TrackContentVersions,
                MaxVersionsToKeep = model.MaxVersionsToKeep,
                EnableAdaptiveCrawling = model.EnableAdaptiveCrawling,
                PriorityQueueSize = model.PriorityQueueSize,
                AdjustDepthBasedOnQuality = model.AdjustDepthBasedOnQuality,
                EnableAdaptiveRateLimiting = model.EnableAdaptiveRateLimiting,
                MinDelayBetweenRequests = model.MinDelayBetweenRequests,
                MaxDelayBetweenRequests = model.MaxDelayBetweenRequests,
                MonitorResponseTimes = model.MonitorResponseTimes,
                EnableContinuousMonitoring = model.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = model.MonitoringIntervalMinutes,
                NotifyOnChanges = model.NotifyOnChanges,
                NotificationEmail = model.NotificationEmail,
                TrackChangesHistory = model.TrackChangesHistory
            };
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
    
    public class ScraperStatus
    {
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ElapsedTime { get; set; }
        public int UrlsProcessed { get; set; }
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();
        public DateTime? LastMonitorCheck { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        
        // Clone the status (to avoid locking issues)
        public ScraperStatus Clone()
        {
            return new ScraperStatus
            {
                IsRunning = this.IsRunning,
                StartTime = this.StartTime,
                EndTime = this.EndTime,
                ElapsedTime = this.ElapsedTime,
                UrlsProcessed = this.UrlsProcessed,
                LogMessages = this.LogMessages.ToList(),
                LastMonitorCheck = this.LastMonitorCheck,
                PipelineMetrics = new PipelineMetrics
                {
                    ProcessingItems = this.PipelineMetrics?.ProcessingItems ?? 0,
                    QueuedItems = this.PipelineMetrics?.QueuedItems ?? 0,
                    CompletedItems = this.PipelineMetrics?.CompletedItems ?? 0,
                    FailedItems = this.PipelineMetrics?.FailedItems ?? 0,
                    AverageProcessingTimeMs = this.PipelineMetrics?.AverageProcessingTimeMs ?? 0
                }
            };
        }
    }
    
    public class PipelineMetrics
    {
        public int ProcessingItems { get; set; }
        public int QueuedItems { get; set; }
        public int CompletedItems { get; set; }
        public int FailedItems { get; set; }
        public double AverageProcessingTimeMs { get; set; }
    }
    
    public class LogEntry
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
    }
}
