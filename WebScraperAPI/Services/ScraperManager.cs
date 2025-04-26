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
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    public class ScraperManager : IHostedService, IDisposable
    {
        private readonly ILogger<ScraperManager> _logger;
        private readonly string _configFilePath = "scraperConfigs.json";
        private readonly Dictionary<string, ScraperInstance> _scrapers = new();
        private Timer _monitoringTimer;
        
        public ScraperManager(ILogger<ScraperManager> logger)
        {
            _logger = logger;
        }
        
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Scraper Manager starting");
            
            // Load saved scraper configurations
            LoadScraperConfigurations();
            
            // Set up monitoring timer for checking changes
            _monitoringTimer = new Timer(CheckAllMonitoredScrapers, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            
            return Task.CompletedTask;
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
        }
        
        #region Scraper Configuration Management
        
        public IEnumerable<ScraperConfigModel> GetAllScraperConfigs()
        {
            lock (_scrapers)
            {
                return _scrapers.Values.Select(s => s.Config).ToList();
            }
        }
        
        public ScraperConfigModel GetScraperConfig(string id)
        {
            lock (_scrapers)
            {
                if (_scrapers.TryGetValue(id, out var scraper))
                {
                    return scraper.Config;
                }
                return null;
            }
        }
        
        public ScraperConfigModel CreateScraperConfig(ScraperConfigModel config)
        {
            // Generate a new ID if none provided
            if (string.IsNullOrEmpty(config.Id))
            {
                config.Id = Guid.NewGuid().ToString();
            }
            
            lock (_scrapers)
            {
                var scraperInstance = new ScraperInstance
                {
                    Config = config,
                    Status = new ScraperStatus 
                    { 
                        IsRunning = false,
                        LastMonitorCheck = null
                    }
                };
                
                _scrapers[config.Id] = scraperInstance;
                SaveScraperConfigurations();
            }
            
            return config;
        }
        
        public bool UpdateScraperConfig(string id, ScraperConfigModel config)
        {
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
                
                // Ensure the ID stays the same
                config.Id = id;
                
                // Update the configuration
                _scrapers[id].Config = config;
                SaveScraperConfigurations();
                
                return true;
            }
        }
        
        public bool DeleteScraperConfig(string id)
        {
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
                
                _scrapers.Remove(id);
                SaveScraperConfigurations();
                
                return true;
            }
        }
        
        private void LoadScraperConfigurations()
        {
            lock (_scrapers)
            {
                if (File.Exists(_configFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_configFilePath);
                        var configs = JsonConvert.DeserializeObject<List<ScraperConfigModel>>(json);
                        
                        if (configs != null)
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
                            
                            _logger.LogInformation($"Loaded {configs.Count} scraper configurations");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error loading scraper configurations");
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
                        
                        // Create and initialize the scraper
                        scraperInstance.Scraper = new Scraper(config, message => AddLogMessage(id, message));
                        await scraperInstance.Scraper.InitializeAsync();
                        
                        // Start scraping
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
                LastMonitorCheck = this.LastMonitorCheck
            };
        }
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