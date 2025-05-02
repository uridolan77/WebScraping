using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraperApi.Models;
using WebScraperApi.Services.Common;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services.Monitoring
{
    /// <summary>
    /// Service for monitoring scraper status and logs
    /// </summary>
    public class ScraperMonitoringService : IScraperMonitoringService
    {
        private readonly ILogger<ScraperMonitoringService> _logger;
        private readonly IScraperStateService _stateService;
        private readonly Dictionary<string, List<LogEntry>> _logStore;
        
        /// <summary>
        /// Maximum number of log entries to keep per scraper
        /// </summary>
        private const int MaxLogEntries = 1000;
        
        public ScraperMonitoringService(
            ILogger<ScraperMonitoringService> logger,
            IScraperStateService stateService)
        {
            _logger = logger;
            _stateService = stateService;
            _logStore = new Dictionary<string, List<LogEntry>>();
        }
        
        /// <summary>
        /// Gets the current status of a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <returns>The current status of the scraper</returns>
        public ScraperStatus GetScraperStatus(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            return instance?.Status ?? new ScraperStatus 
            {
                IsRunning = false,
                Message = "Scraper not found"
            };
        }
        
        /// <summary>
        /// Gets the logs for a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="limit">Maximum number of log entries to return</param>
        /// <returns>Collection of log entries</returns>
        public IEnumerable<LogEntry> GetScraperLogs(string id, int limit = 100)
        {
            if (!_logStore.ContainsKey(id))
            {
                return new List<LogEntry>();
            }
            
            lock (_logStore)
            {
                return _logStore[id]
                    .OrderByDescending(log => log.Timestamp)
                    .Take(limit)
                    .ToList();
            }
        }
        
        /// <summary>
        /// Adds a log message for a scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        /// <param name="message">Message to log</param>
        public void AddLogMessage(string id, string message)
        {
            var logEntry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message
            };
            
            // Add to log store
            lock (_logStore)
            {
                if (!_logStore.ContainsKey(id))
                {
                    _logStore[id] = new List<LogEntry>();
                }
                
                _logStore[id].Add(logEntry);
                
                // Trim if exceeds max entries
                if (_logStore[id].Count > MaxLogEntries)
                {
                    _logStore[id] = _logStore[id].OrderByDescending(log => log.Timestamp).Take(MaxLogEntries).ToList();
                }
            }
            
            // Update status in the instance if available
            var instance = _stateService.GetScraperInstance(id);
            if (instance != null)
            {
                instance.Status.LastStatusUpdate = DateTime.Now;
                instance.Status.Message = message;
                
                // Add log to status log messages too
                if (instance.Status.LogMessages == null)
                {
                    instance.Status.LogMessages = new List<LogEntry>();
                }
                
                instance.Status.LogMessages.Add(logEntry);
                
                // If log message contains "error" or "fail", mark the status as having errors
                if (message.ToLower().Contains("error") || message.ToLower().Contains("fail"))
                {
                    instance.Status.HasErrors = true;
                    instance.Status.LastError = message;
                }
            }
            
            // Log to the application logs as well
            _logger.LogInformation($"Scraper {id}: {message}");
        }
        
        /// <summary>
        /// Run a monitoring check for all scrapers that have monitoring enabled
        /// </summary>
        public async Task RunAllMonitoringChecksAsync()
        {
            _logger.LogInformation("Running monitoring checks for all scrapers");
            
            foreach (var scraper in _stateService.GetScrapers())
            {
                try
                {
                    if (scraper.Value.Config.EnableContinuousMonitoring)
                    {
                        await RunMonitoringCheckAsync(scraper.Key);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error running monitoring check for scraper {scraper.Key}");
                }
            }
            
            _logger.LogInformation("Completed monitoring checks for all scrapers");
        }
        
        /// <summary>
        /// Runs a monitoring check for a specific scraper
        /// </summary>
        /// <param name="id">Scraper ID</param>
        public async Task RunMonitoringCheckAsync(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot run monitoring check for scraper {id}: not found");
                return;
            }
            
            // Skip if the scraper is already running
            if (instance.Status.IsRunning)
            {
                _logger.LogInformation($"Skipping monitoring check for scraper {id}: already running");
                return;
            }
            
            // Skip if not yet time for a check based on interval
            if (instance.Status.LastMonitorCheck.HasValue)
            {
                var interval = TimeSpan.FromMinutes(instance.Config.GetMonitoringInterval());
                var nextCheckTime = instance.Status.LastMonitorCheck.Value.Add(interval);
                
                if (DateTime.Now < nextCheckTime)
                {
                    _logger.LogInformation($"Skipping monitoring check for scraper {id}: not yet time for next check (next check: {nextCheckTime})");
                    return;
                }
            }
            
            // Update last check time
            instance.Status.LastMonitorCheck = DateTime.Now;
            
            _logger.LogInformation($"Running monitoring check for scraper {id}");
            AddLogMessage(id, "Running scheduled monitoring check");
            
            // If continuous monitoring is enabled, start the scraper
            if (instance.Config.EnableContinuousMonitoring && !instance.Status.IsRunning)
            {
                try
                {
                    // Get the scraper manager to start the scraper
                    // This is a temporary approach - in a more advanced design, 
                    // we would use a message queue or event system to avoid circular dependencies
                    var scraperManager = (ScraperManager)ServiceLocator.GetService(typeof(ScraperManager));
                    if (scraperManager != null)
                    {
                        await scraperManager.StartScraperAsync(id);
                        _logger.LogInformation($"Started scraper {id} from monitoring check");
                    }
                    else
                    {
                        _logger.LogError($"Could not start scraper {id} from monitoring check: ScraperManager not available");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error starting scraper {id} from monitoring check");
                }
            }
        }
    }
}