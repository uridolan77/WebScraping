using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WebScraperApi.Models;
using WebScraperApi.Services;
using WebScraperApi.Services.Analytics;
using WebScraperApi.Services.Common;
using WebScraperApi.Services.Configuration;
using WebScraperApi.Services.Execution;
using WebScraperApi.Services.Monitoring;
using WebScraperApi.Services.Notifications;
using WebScraperApi.Services.Scheduling;
using WebScraperApi.Services.State;
using WebScraper.StateManagement; // Add this for WebScraper.StateManagement.ScraperState

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperManager _scraperManager;
        private readonly IScraperConfigurationService _configService;
        private readonly IScraperExecutionService _executionService;
        private readonly IScraperMonitoringService _monitoringService;
        private readonly IScraperStateService _stateService;
        private readonly IScraperAnalyticsService _analyticsService;
        private readonly IScraperSchedulingService _schedulingService;
        private readonly IWebhookNotificationService _notificationService;
        
        public ScraperController(
            ScraperManager scraperManager,
            IScraperConfigurationService configService,
            IScraperExecutionService executionService,
            IScraperMonitoringService monitoringService,
            IScraperStateService stateService,
            IScraperAnalyticsService analyticsService,
            IScraperSchedulingService schedulingService,
            IWebhookNotificationService notificationService)
        {
            _scraperManager = scraperManager;
            _configService = configService;
            _executionService = executionService;
            _monitoringService = monitoringService;
            _stateService = stateService;
            _analyticsService = analyticsService;
            _schedulingService = schedulingService;
            _notificationService = notificationService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllScrapers()
        {
            var scrapers = await _configService.GetAllScraperConfigsAsync();
            return Ok(scrapers);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScraper(string id)
        {
            var config = await _configService.GetScraperConfigAsync(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            return Ok(config);
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateScraper([FromBody] ScraperConfigModel config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var createdConfig = await _configService.CreateScraperConfigAsync(config);
            return CreatedAtAction(nameof(GetScraper), new { id = createdConfig.Id }, createdConfig);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScraper(string id, [FromBody] ScraperConfigModel config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var success = await _configService.UpdateScraperConfigAsync(id, config);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            // Update the state with the new configuration
            var instance = _stateService.GetScraperInstance(id);
            if (instance != null)
            {
                instance.Config = config;
                _stateService.AddOrUpdateScraper(id, instance);
            }
            
            return Ok(config);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScraper(string id)
        {
            var success = await _configService.DeleteScraperConfigAsync(id);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            // Also remove from state
            _stateService.RemoveScraper(id);
            
            return NoContent();
        }
        
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetScraperStatus(string id)
        {
            var status = _monitoringService.GetScraperStatus(id);
            if (status == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            return Ok(new
            {
                ScraperId = id,
                ScraperName = instance.Config.Name,
                status.IsRunning,
                status.StartTime,
                status.EndTime,
                status.ElapsedTime,
                status.UrlsProcessed,
                LastMonitorCheck = status.LastMonitorCheck,
                IsMonitoring = instance.Config.EnableContinuousMonitoring,
                MonitoringInterval = instance.Config.MonitoringIntervalMinutes
            });
        }
        
        [HttpGet("{id}/logs")]
        public IActionResult GetScraperLogs(string id, [FromQuery] int limit = 100)
        {
            var logs = _monitoringService.GetScraperLogs(id, limit);
            return Ok(new { Logs = logs });
        }
        
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartScraper(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            if (instance.Status.IsRunning)
            {
                return BadRequest("Scraper is already running");
            }
            
            // Create log action to capture logs
            void LogAction(string message) => _monitoringService.AddLogMessage(id, message);
            
            // Start the scraper using the execution service
            // Create a new WebScraper.StateManagement.ScraperState to match what the service expects
            var scraperState = new WebScraper.StateManagement.ScraperState { 
                ScraperId = id,
                Status = "Starting" 
            };
            
            var success = await _executionService.StartScraperAsync(
                instance.Config, 
                scraperState,
                LogAction);
                
            if (!success)
            {
                return BadRequest("Failed to start scraper");
            }
            
            // Update status in the state
            instance.Status.IsRunning = true;
            instance.Status.StartTime = DateTime.Now;
            instance.Status.Message = "Scraper started successfully";
            _stateService.AddOrUpdateScraper(id, instance);
            
            // Send notification about status change
            await _notificationService.NotifyScraperStatusAsync(id, instance.Status);
            
            return Ok(new { Message = $"Scraper '{instance.Config.Name}' started successfully" });
        }
        
        [HttpPost("{id}/stop")]
        public IActionResult StopScraper(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            if (!instance.Status.IsRunning)
            {
                return BadRequest("Scraper is not running");
            }
            
            // Create log action
            void LogAction(string message) => _monitoringService.AddLogMessage(id, message);
            
            // Stop the scraper
            _executionService.StopScraper(instance.Scraper, LogAction);
            
            // Update status
            instance.Status.IsRunning = false;
            instance.Status.EndTime = DateTime.Now;
            instance.Status.Message = "Stopped by user";
            _stateService.AddOrUpdateScraper(id, instance);
            
            // Send notification about status change
            _notificationService.NotifyScraperStatusAsync(id, instance.Status);
            
            return Ok(new { Message = $"Scraper '{instance.Config.Name}' stopped successfully" });
        }
        
        [HttpPost("{id}/monitor")]
        public async Task<IActionResult> SetMonitoring(string id, [FromBody] Models.MonitoringSettings settings)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            // Update monitoring settings
            instance.Config.EnableContinuousMonitoring = settings.Enabled;
            instance.Config.MonitoringIntervalMinutes = settings.IntervalMinutes;
            instance.Config.NotifyOnChanges = settings.NotifyOnChanges;
            instance.Config.NotificationEmail = settings.NotificationEmail;
            instance.Config.TrackChangesHistory = settings.TrackChangesHistory;
            
            // Update the config
            var success = await _configService.UpdateScraperConfigAsync(id, instance.Config);
            if (!success)
            {
                return BadRequest("Failed to update monitoring settings");
            }
            
            _stateService.AddOrUpdateScraper(id, instance);
            
            return Ok(new 
            { 
                Message = settings.Enabled 
                    ? $"Monitoring enabled for '{instance.Config.Name}' with {settings.IntervalMinutes} minute interval" 
                    : $"Monitoring disabled for '{instance.Config.Name}'"
            });
        }
        
        [HttpGet("results")]
        public async Task<IActionResult> GetResults([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null, [FromQuery] string scraperId = null)
        {
            if (!string.IsNullOrEmpty(scraperId))
            {
                var results = await _stateService.GetProcessedDocumentsAsync(scraperId, null, page, pageSize);
                return Ok(results);
            }
            else
            {
                // If no specific scraper ID, return a summary list of results
                return Ok(new
                {
                    Results = new Dictionary<string, object>(),
                    TotalCount = 0,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = 0
                });
            }
        }
        
        #region API endpoints for additional functionality
        
        [HttpGet("{id}/changes")]
        public async Task<IActionResult> GetDetectedChanges(string id, [FromQuery] DateTime? since = null, [FromQuery] int limit = 100)
        {
            var changes = await _stateService.GetDetectedChangesAsync(id, since, limit);
            return Ok(new { Changes = changes });
        }
        
        [HttpGet("{id}/documents")]
        public async Task<IActionResult> GetProcessedDocuments(string id, [FromQuery] string documentType = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var documents = await _stateService.GetProcessedDocumentsAsync(id, documentType, page, pageSize);
            return Ok(documents);
        }
        
        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetScraperAnalytics(string id)
        {
            var analytics = await _analyticsService.GetScraperAnalyticsAsync(id);
            return Ok(analytics);
        }
        
        [HttpPost("{id}/webhook-test")]
        public async Task<IActionResult> TestWebhook(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            // Send a test notification
            var testData = new
            {
                message = "This is a test notification",
                timestamp = DateTime.Now
            };
            
            var success = await _notificationService.SendCustomNotificationAsync(id, "webhook_test", testData);
            
            if (!success)
            {
                return BadRequest("Failed to send webhook notification. Please check the webhook URL and configuration.");
            }
            
            return Ok(new { Message = "Test webhook notification sent successfully" });
        }

        [HttpPost("{id}/compress")]
        public async Task<IActionResult> CompressStoredContent(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var result = await _stateService.CompressStoredContentAsync(id);
            return Ok(result);
        }

        [HttpGet("{id}/metrics")]
        public async Task<IActionResult> GetScraperMetrics(string id)
        {
            var metrics = await _analyticsService.GetScraperMetricsAsync(id);
            return Ok(metrics);
        }
        
        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> ScheduleScraper(string id, [FromBody] Models.ScheduleOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var scheduleConfig = new ScheduleConfig
            {
                Name = options.ScheduleName,
                CronExpression = options.CronExpression,
                Enabled = true
            };
            
            try
            {
                var scheduleItem = await _schedulingService.ScheduleScraper(id, scheduleConfig);
                
                return Ok(new { 
                    Message = $"Scraper '{instance.Config.Name}' scheduled successfully", 
                    ScheduleId = scheduleItem.Id,
                    NextRunTime = scheduleItem.NextRunTime
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        
        [HttpGet("{id}/schedules")]
        public IActionResult GetScraperSchedules(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var schedules = _schedulingService.GetScheduledItems(id);
            return Ok(new { Schedules = schedules });
        }
        
        [HttpDelete("{id}/schedules/{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(string id, string scheduleId)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _schedulingService.RemoveSchedule(scheduleId);
            if (!success)
            {
                return NotFound($"Schedule with ID {scheduleId} not found");
            }
            
            return NoContent();
        }
        
        [HttpPut("{id}/webhook-config")]
        public async Task<IActionResult> UpdateWebhookConfig(string id, [FromBody] Models.WebhookConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _stateService.UpdateWebhookConfigAsync(id, config);
            if (!success)
            {
                return BadRequest("Failed to update webhook configuration");
            }
            
            return Ok(new { Message = "Webhook configuration updated successfully" });
        }
        
        #endregion
    }
}
