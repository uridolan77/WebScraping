using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebScraperApi.Models;
using WebScraperApi.Services.Analytics;
using WebScraperApi.Services.Configuration;
using WebScraperApi.Services.Execution;
using WebScraperApi.Services.Monitoring;
using WebScraperApi.Services.Scheduling;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Manages scraper configurations, execution, and monitoring.
    /// This is a refactored version that delegates responsibilities to specialized services.
    /// </summary>
    public class RefactoredScraperManager : IHostedService, IDisposable
    {
        private readonly ILogger<RefactoredScraperManager> _logger;
        private readonly IScraperConfigurationService _configService;
        private readonly IScraperExecutionService _executionService;
        private readonly IScraperMonitoringService _monitoringService;
        private readonly IScraperAnalyticsService _analyticsService;
        private readonly IScraperSchedulingService _schedulingService;
        private readonly IScraperStateService _stateService;
        private Timer _monitoringTimer;
        private bool _disposed = false;
        
        /// <summary>
        /// Initializes a new instance of the ScraperManager class
        /// </summary>
        public RefactoredScraperManager(
            ILogger<RefactoredScraperManager> logger,
            IScraperConfigurationService configService,
            IScraperExecutionService executionService,
            IScraperMonitoringService monitoringService,
            IScraperAnalyticsService analyticsService,
            IScraperSchedulingService schedulingService,
            IScraperStateService stateService)
        {
            _logger = logger;
            _configService = configService;
            _executionService = executionService;
            _monitoringService = monitoringService;
            _analyticsService = analyticsService;
            _schedulingService = schedulingService;
            _stateService = stateService;
        }

        #region IHostedService Implementation
        
        /// <summary>
        /// Starts the scraper manager
        /// </summary>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refactored Scraper Manager starting");
            
            try
            {
                // Load scraper configurations
                var configs = await _configService.LoadScraperConfigurationsAsync();
                
                // Initialize scraper instances
                foreach (var config in configs)
                {
                    _stateService.AddOrUpdateScraper(config.Id, new ScraperInstance
                    {
                        Config = config,
                        Status = new ScraperStatus
                        {
                            IsRunning = false,
                            LastMonitorCheck = null
                        }
                    });
                }
                
                // Start monitoring timer to periodically check for scrapers that need to run
                _monitoringTimer = new Timer(
                    CheckScrapersAsync, 
                    null, 
                    TimeSpan.FromSeconds(30), // Initial delay
                    TimeSpan.FromMinutes(1)); // Interval
                
                _logger.LogInformation("Refactored Scraper Manager started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting Refactored Scraper Manager");
                throw;
            }
        }
        
        /// <summary>
        /// Stops the scraper manager
        /// </summary>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Refactored Scraper Manager stopping");
            _monitoringTimer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }
        
        #endregion
        
        #region Monitoring
        
        /// <summary>
        /// Periodically checks all scrapers that have monitoring enabled
        /// </summary>
        private async void CheckScrapersAsync(object state)
        {
            try
            {
                _logger.LogDebug("Running scheduled monitoring checks");
                await _monitoringService.RunAllMonitoringChecksAsync();
                
                // Find scrapers that need to run according to schedule
                var scrapersToRun = await _schedulingService.GetScrapersToRun();
                foreach (var scraperId in scrapersToRun)
                {
                    // Start each scraper
                    await StartScraperAsync(scraperId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in monitoring timer callback");
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Starts a scraper by ID
        /// </summary>
        public async Task<bool> StartScraperAsync(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot start scraper {id}: not found");
                return false;
            }
            
            if (instance.Status.IsRunning)
            {
                _logger.LogWarning($"Scraper {id} is already running");
                return false;
            }
            
            try
            {
                // Update status
                instance.Status.IsRunning = true;
                instance.Status.StartTime = DateTime.Now;
                instance.Status.Message = "Starting...";
                
                // Create log collector
                var logs = new List<string>();
                void LogAction(string message) 
                {
                    _monitoringService.AddLogMessage(id, message);
                    logs.Add(message);
                }
                
                // Start the scraper
                var success = await _executionService.StartScraperAsync(
                    instance.Config, 
                    new ScraperState { Id = id },
                    LogAction);
                
                if (success)
                {
                    // Update status
                    instance.Status.Message = "Running successfully";
                    _logger.LogInformation($"Scraper {id} ({instance.Config.Name}) started successfully");
                    return true;
                }
                else
                {
                    // Update status
                    instance.Status.IsRunning = false;
                    instance.Status.EndTime = DateTime.Now;
                    instance.Status.HasErrors = true;
                    instance.Status.Message = $"Failed to start: {logs.LastOrDefault() ?? "Unknown error"}";
                    _logger.LogError($"Failed to start scraper {id}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Update status
                instance.Status.IsRunning = false;
                instance.Status.EndTime = DateTime.Now;
                instance.Status.HasErrors = true;
                instance.Status.LastError = ex.Message;
                instance.Status.Message = $"Error: {ex.Message}";
                
                _logger.LogError(ex, $"Error starting scraper {id}");
                return false;
            }
        }
        
        /// <summary>
        /// Stops a running scraper
        /// </summary>
        public bool StopScraperInstance(string id)
        {
            try
            {
                var instance = _stateService.GetScraperInstance(id);
                if (instance == null)
                {
                    _logger.LogWarning($"Cannot stop scraper {id}: not found");
                    return false;
                }
                
                if (!instance.Status.IsRunning)
                {
                    _logger.LogWarning($"Cannot stop scraper {id}: not running");
                    return false;
                }
                
                if (instance.Scraper == null)
                {
                    _logger.LogWarning($"Cannot stop scraper {id}: no active scraper instance");
                    instance.Status.IsRunning = false;
                    instance.Status.Message = "Stopped (no active instance found)";
                    instance.Status.EndTime = DateTime.Now;
                    return true;
                }
                
                // Create log collector
                void LogAction(string message) => _monitoringService.AddLogMessage(id, message);
                
                // Stop the scraper
                _executionService.StopScraper(instance.Scraper, LogAction);
                
                // Update status
                instance.Status.IsRunning = false;
                instance.Status.Message = "Stopped by user";
                instance.Status.EndTime = DateTime.Now;
                
                _logger.LogInformation($"Scraper {id} stopped successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping scraper {id}");
                return false;
            }
        }
        
        #endregion
        
        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes the scraper manager
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        /// <summary>
        /// Releases the unmanaged resources used by the scraper manager
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;
                
            if (disposing)
            {
                _monitoringTimer?.Dispose();
            }
            
            _disposed = true;
        }
        
        #endregion
    }
}