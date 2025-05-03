using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper;
using WebScraperApi.Models;
using WebScraperApi.Services.Analytics;
using WebScraperApi.Services.Common;
using WebScraperApi.Services.Configuration;
using WebScraperApi.Services.Execution;
using WebScraperApi.Services.Monitoring;
using WebScraperApi.Services.Scheduling;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Orchestrates scraper operations across multiple services
    /// </summary>
    public class ScraperManager
    {
        private readonly ILogger<ScraperManager> _logger;
        private readonly IScraperConfigurationService _configService;
        private readonly IScraperExecutionService _executionService;
        private readonly IScraperMonitoringService _monitoringService;
        private readonly IScraperStateService _stateService;
        private readonly IScraperAnalyticsService _analyticsService;
        private readonly IScraperSchedulingService _schedulingService;

        public ScraperManager(
            ILogger<ScraperManager> logger,
            IScraperConfigurationService configService,
            IScraperExecutionService executionService,
            IScraperMonitoringService monitoringService,
            IScraperStateService stateService,
            IScraperAnalyticsService analyticsService,
            IScraperSchedulingService schedulingService)
        {
            _logger = logger;
            _configService = configService;
            _executionService = executionService;
            _monitoringService = monitoringService;
            _stateService = stateService;
            _analyticsService = analyticsService;
            _schedulingService = schedulingService;

            // No longer using ServiceLocator to avoid circular dependencies
        }

        /// <summary>
        /// Initializes the scraper manager and loads configurations
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("Initializing ScraperManager...");

                // Load scraper configurations
                var configs = await _configService.LoadScraperConfigurationsAsync();
                _logger.LogInformation($"Loaded {configs.Count} scraper configurations.");

                // Initialize instances for each configuration
                foreach (var config in configs)
                {
                    var instance = new ScraperInstance
                    {
                        Config = config,
                        Status = new ScraperStatus
                        {
                            IsRunning = false,
                            LastStatusUpdate = DateTime.Now
                        }
                    };

                    _stateService.AddOrUpdateScraper(config.Id, instance);
                }

                // Run a monitoring check for all loaded scrapers
                await _monitoringService.RunAllMonitoringChecksAsync();

                _logger.LogInformation("ScraperManager initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing ScraperManager");
                throw;
            }
        }

        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        public async Task<IEnumerable<ScraperConfigModel>> GetAllScraperConfigsAsync()
        {
            return await _configService.GetAllScraperConfigsAsync();
        }

        /// <summary>
        /// Gets a specific scraper configuration
        /// </summary>
        public async Task<ScraperConfigModel> GetScraperConfigAsync(string id)
        {
            return await _configService.GetScraperConfigAsync(id);
        }

        /// <summary>
        /// Creates a new scraper configuration
        /// </summary>
        public async Task<ScraperConfigModel> CreateScraperConfigAsync(ScraperConfigModel config)
        {
            // Create the configuration
            var createdConfig = await _configService.CreateScraperConfigAsync(config);

            // Initialize an instance for the new configuration
            var instance = new ScraperInstance
            {
                Config = createdConfig,
                Status = new ScraperStatus
                {
                    IsRunning = false,
                    LastStatusUpdate = DateTime.Now
                }
            };

            // Add to state management
            _stateService.AddOrUpdateScraper(createdConfig.Id, instance);

            // Add initial log message
            _monitoringService.AddLogMessage(createdConfig.Id, "Scraper configuration created");

            return createdConfig;
        }

        /// <summary>
        /// Updates an existing scraper configuration
        /// </summary>
        public async Task<bool> UpdateScraperConfigAsync(string id, ScraperConfigModel config)
        {
            // Update the configuration
            var result = await _configService.UpdateScraperConfigAsync(id, config);

            if (result)
            {
                // Get current scraper instance
                var instance = _stateService.GetScraperInstance(id);
                if (instance == null)
                {
                    // Create a new instance if it doesn't exist
                    instance = new ScraperInstance
                    {
                        Config = config,
                        Status = new ScraperStatus
                        {
                            IsRunning = false,
                            LastStatusUpdate = DateTime.Now
                        }
                    };
                }
                else
                {
                    // Update the existing instance
                    instance.Config = config;
                }

                // Update state management
                _stateService.AddOrUpdateScraper(id, instance);

                // Add log message
                _monitoringService.AddLogMessage(id, "Scraper configuration updated");
            }

            return result;
        }

        /// <summary>
        /// Deletes a scraper configuration
        /// </summary>
        public async Task<bool> DeleteScraperConfigAsync(string id)
        {
            // Get the current instance from state
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot delete config: scraper {id} not found in state");
                return false;
            }

            // Validate that the scraper is not running
            if (instance.Status.IsRunning)
            {
                _logger.LogWarning($"Cannot delete config: scraper {id} is currently running");
                return false;
            }

            // Stop and dispose the scraper if needed
            if (instance.Scraper != null)
            {
                _executionService.StopScraper(instance.Scraper, message => _monitoringService.AddLogMessage(id, message));

                // Clean up resources
                instance.Scraper?.Dispose();
                instance.Scraper = null!; // Using null-forgiving operator
            }

            // Remove from state management
            _stateService.RemoveScraper(id);

            // Delete from configuration store
            return await _configService.DeleteScraperConfigAsync(id);
        }

        /// <summary>
        /// Starts a scraper execution
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>True if started successfully</returns>
        public async Task<bool> StartScraperAsync(string id)
        {
            try
            {
                _logger.LogInformation($"Starting scraper {id}...");
                _monitoringService.AddLogMessage(id, $"Attempting to start scraper with ID: {id}");

                var instance = _stateService.GetScraperInstance(id);
                if (instance == null)
                {
                    var errorMsg = $"Cannot start scraper {id}: scraper instance not found in state management";
                    _logger.LogWarning(errorMsg);
                    _monitoringService.AddLogMessage(id, errorMsg);
                    return false;
                }

                // Debug info about the scraper configuration
                _monitoringService.AddLogMessage(id, $"Scraper configuration: Name={instance.Config.Name}, BaseUrl={instance.Config.BaseUrl}");
                
                // Skip if already running
                if (instance.Status.IsRunning)
                {
                    _logger.LogInformation($"Scraper {id} is already running");
                    _monitoringService.AddLogMessage(id, "Scraper is already running");
                    return true;
                }

                // Update status
                instance.Status.IsRunning = true;
                instance.Status.StartTime = DateTime.Now;
                instance.Status.Message = "Starting scraper execution";

                // Add log message
                _monitoringService.AddLogMessage(id, "Starting scraper execution");

                // Update metrics
                if (instance.Metrics == null)
                {
                    _monitoringService.AddLogMessage(id, "Initializing metrics for first run");
                    instance.Metrics = new ScraperMetrics();
                }
                instance.Metrics.ExecutionCount++;

                _monitoringService.AddLogMessage(id, "Converting config model to scraper config");
                
                // Log scraper configuration details
                _monitoringService.AddLogMessage(id, $"Configuration details: MaxDepth={instance.Config.MaxDepth}, MaxConcurrentRequests={instance.Config.MaxConcurrentRequests}, DelayBetweenRequests={instance.Config.DelayBetweenRequests}ms");
                if (instance.Config.IsUKGCWebsite)
                {
                    _monitoringService.AddLogMessage(id, "This is a UKGC website configuration with special handling");
                }

                // Execute the scraper
                _monitoringService.AddLogMessage(id, "Calling scraper execution service");
                var success = await _executionService.StartScraperAsync(
                    instance.Config,
                    new WebScraperApi.Models.ScraperState { Id = id, Status = "Running" },
                    message => _monitoringService.AddLogMessage(id, message));

                if (success)
                {
                    _monitoringService.AddLogMessage(id, "Scraper started successfully");
                    return true;
                }
                else
                {
                    instance.Status.IsRunning = false;
                    instance.Status.HasErrors = true;
                    instance.Status.Message = "Failed to start scraper";
                    _monitoringService.AddLogMessage(id, "Scraper failed to start - execution service returned failure");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting scraper {id}");
                _monitoringService.AddLogMessage(id, $"Error starting scraper: {ex.Message}");
                _monitoringService.AddLogMessage(id, $"Stack trace: {ex.StackTrace}");
                
                if (ex.InnerException != null)
                {
                    _monitoringService.AddLogMessage(id, $"Inner exception: {ex.InnerException.Message}");
                }

                // Update status
                var instance = _stateService.GetScraperInstance(id);
                if (instance != null)
                {
                    instance.Status.IsRunning = false;
                    instance.Status.HasErrors = true;
                    instance.Status.LastError = ex.Message;
                    instance.Status.Message = $"Error: {ex.Message}";
                }

                return false;
            }
        }

        /// <summary>
        /// Stops a currently running scraper
        /// </summary>
        public async Task<object> StopScraperAsync(string id)
        {
            try
            {
                // Get the scraper instance
                var instance = _stateService.GetScraperInstance(id);
                if (instance == null)
                {
                    _logger.LogWarning($"Cannot stop scraper: scraper {id} not found");
                    return new { Success = false, Message = "Scraper not found" };
                }

                // Check if not running
                if (!instance.Status.IsRunning || instance.Scraper == null)
                {
                    _logger.LogWarning($"Cannot stop scraper: scraper {id} is not running");
                    return new { Success = false, Message = "Scraper is not running" };
                }

                // Define a log action that will be passed to the execution service
                Action<string> logAction = (message) => _monitoringService.AddLogMessage(id, message);

                // Stop the scraper
                _executionService.StopScraper(instance.Scraper, logAction);

                // Update status
                instance.Status.IsRunning = false;
                instance.Status.EndTime = DateTime.Now;
                instance.Status.LastStatusUpdate = DateTime.Now;
                instance.Status.Message = "Scraper stopped by user";

                return new { Success = true, Message = "Scraper stopped" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping scraper {id}");
                return new { Success = false, Message = $"Error: {ex.Message}" };
            }
        }

        /// <summary>
        /// Gets the current status of all scrapers
        /// </summary>
        public IEnumerable<object> GetAllScraperStatus()
        {
            var scrapers = _stateService.GetScrapers();

            return scrapers.Select(s => new
            {
                Id = s.Key,
                Name = s.Value.Config.Name,
                IsRunning = s.Value.Status.IsRunning,
                StartTime = s.Value.Status.StartTime,
                EndTime = s.Value.Status.EndTime,
                LastRun = s.Value.Config.LastRun,
                RunCount = s.Value.Config.RunCount,
                Message = s.Value.Status.Message,
                BaseUrl = s.Value.Config.BaseUrl
            });
        }

        /// <summary>
        /// Gets detailed status for a specific scraper
        /// </summary>
        public async Task<object> GetScraperStatusAsync(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return new { Error = "Scraper not found" };
            }

            var analytics = await _analyticsService.GetScraperAnalyticsAsync(id);

            return new
            {
                Id = id,
                Config = instance.Config,
                Status = new
                {
                    IsRunning = instance.Status.IsRunning,
                    StartTime = instance.Status.StartTime,
                    EndTime = instance.Status.EndTime,
                    UrlsProcessed = instance.Status.UrlsProcessed,
                    Message = instance.Status.Message,
                    LastStatusUpdate = instance.Status.LastStatusUpdate
                },
                Logs = _monitoringService.GetScraperLogs(id, 100).Select(log => new
                {
                    log.Timestamp,
                    log.Message
                }).ToList(),
                Analytics = analytics
            };
        }

        /// <summary>
        /// Schedules a scraper to run
        /// </summary>
        public async Task<object> ScheduleScraperAsync(string id, ScheduleOptions options)
        {
            // Check if the scraper exists
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot schedule scraper: scraper {id} not found");
                return new { Success = false, Message = "Scraper not found" };
            }

            // Schedule the scraper
            var result = await _schedulingService.ScheduleScraperAsync(id, options);

            if ((result as dynamic).Success)
            {
                _monitoringService.AddLogMessage(id, $"Scheduled scraper for {(result as dynamic).NextRunTime}");
            }

            return result;
        }

        /// <summary>
        /// Gets the change history for a scraper
        /// </summary>
        public async Task<object> GetChangeHistoryAsync(string id, DateTime? since = null, int limit = 100)
        {
            return await _analyticsService.GetDetectedChangesAsync(id, since, limit);
        }

        /// <summary>
        /// Gets scraper metrics
        /// </summary>
        public async Task<object> GetScraperMetricsAsync(string id)
        {
            return await _analyticsService.GetScraperMetricsAsync(id);
        }

        /// <summary>
        /// Gets schedules for a scraper
        /// </summary>
        public async Task<IEnumerable<object>> GetSchedulesAsync(string id)
        {
            return await _schedulingService.GetSchedulesAsync(id);
        }

        /// <summary>
        /// Removes a schedule for a scraper
        /// </summary>
        public async Task<bool> RemoveScheduleAsync(string scraperId, string scheduleId)
        {
            var result = await _schedulingService.RemoveScheduleAsync(scraperId, scheduleId);

            if (result)
            {
                _monitoringService.AddLogMessage(scraperId, $"Removed schedule {scheduleId}");
            }

            return result;
        }

        /// <summary>
        /// Compresses stored content for a specific scraper
        /// </summary>
        public async Task<object> CompressStoredContentAsync(string id)
        {
            return await _stateService.CompressStoredContentAsync(id);
        }

        /// <summary>
        /// Updates webhook configuration for a specific scraper
        /// </summary>
        public async Task<bool> UpdateWebhookConfigAsync(string id, WebhookConfig config)
        {
            var result = await _stateService.UpdateWebhookConfigAsync(id, config);

            if (result)
            {
                _monitoringService.AddLogMessage(id, "Updated webhook configuration");
            }

            return result;
        }
    }
}
