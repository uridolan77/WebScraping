using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;
using WebScraperApi.Services;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly ScraperManager _scraperManager;
        
        public ScraperController(ScraperManager scraperManager)
        {
            _scraperManager = scraperManager;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllScrapers()
        {
            var scrapers = await _scraperManager.GetAllScraperConfigsAsync();
            return Ok(scrapers);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScraper(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
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
            
            var createdConfig = await _scraperManager.CreateScraperConfig(config);
            return CreatedAtAction(nameof(GetScraper), new { id = createdConfig.Id }, createdConfig);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScraper(string id, [FromBody] ScraperConfigModel config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var success = await _scraperManager.UpdateScraperConfig(id, config);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            return Ok(config);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScraper(string id)
        {
            var success = await _scraperManager.DeleteScraperConfig(id);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            return NoContent();
        }
        
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetScraperStatus(string id)
        {
            var status = _scraperManager.GetScraperStatus(id);
            if (status == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var config = await _scraperManager.GetScraperConfig(id);
            
            return Ok(new
            {
                ScraperId = id,
                ScraperName = config.Name,
                status.IsRunning,
                status.StartTime,
                status.EndTime,
                status.ElapsedTime,
                status.UrlsProcessed,
                LastMonitorCheck = status.LastMonitorCheck,
                IsMonitoring = config.EnableContinuousMonitoring,
                MonitoringInterval = config.MonitoringIntervalMinutes
            });
        }
        
        [HttpGet("{id}/logs")]
        public IActionResult GetScraperLogs(string id, [FromQuery] int limit = 100)
        {
            var logs = _scraperManager.GetScraperLogs(id, limit);
            return Ok(new { Logs = logs });
        }
        
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartScraper(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _scraperManager.StartScraper(id);
            if (!success)
            {
                return BadRequest("Scraper is already running");
            }
            
            return Ok(new { Message = $"Scraper '{config.Name}' started successfully" });
        }
        
        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopScraper(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = _scraperManager.StopScraperInstance(id);
            if (!success)
            {
                return BadRequest("Scraper is not running");
            }
            
            return Ok(new { Message = $"Scraper '{config.Name}' stopped successfully" });
        }
        
        [HttpPost("{id}/monitor")]
        public async Task<IActionResult> SetMonitoring(string id, [FromBody] Models.MonitoringSettings settings)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            // Update monitoring settings
            config.EnableContinuousMonitoring = settings.Enabled;
            config.MonitoringIntervalMinutes = settings.IntervalMinutes;
            config.NotifyOnChanges = settings.NotifyOnChanges;
            config.NotificationEmail = settings.NotificationEmail;
            config.TrackChangesHistory = settings.TrackChangesHistory;
            
            // Update the config
            var success = await _scraperManager.UpdateScraperConfig(id, config);
            if (!success)
            {
                return BadRequest("Failed to update monitoring settings");
            }
            
            return Ok(new 
            { 
                Message = settings.Enabled 
                    ? $"Monitoring enabled for '{config.Name}' with {settings.IntervalMinutes} minute interval" 
                    : $"Monitoring disabled for '{config.Name}'"
            });
        }
        
        [HttpGet("results")]
        public IActionResult GetResults([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string search = null, [FromQuery] string scraperId = null)
        {
            // This is a placeholder - in a real implementation,
            // we'd query the actual results from disk or database
            // filtered by the scraper ID
            
            return Ok(new
            {
                Results = new Dictionary<string, object>(),
                TotalCount = 0,
                Page = page,
                PageSize = pageSize,
                TotalPages = 0,
                ScraperId = scraperId
            });
        }
        
        #region API endpoints for additional functionality
        
        [HttpGet("{id}/changes")]
        public async Task<IActionResult> GetDetectedChanges(string id, [FromQuery] DateTime? since = null, [FromQuery] int limit = 100)
        {
            var changes = await _scraperManager.GetDetectedChanges(id, since, limit);
            if (changes == null)
            {
                return NotFound($"Scraper with ID {id} not found or has no changes detected");
            }
            
            return Ok(new { Changes = changes });
        }
        
        [HttpGet("{id}/documents")]
        public async Task<IActionResult> GetProcessedDocuments(string id, [FromQuery] string documentType = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var documents = await _scraperManager.GetProcessedDocuments(id, documentType, page, pageSize);
            if (documents == null)
            {
                return NotFound($"Scraper with ID {id} not found or has no processed documents");
            }
            
            return Ok(documents);
        }
        
        [HttpGet("{id}/analytics")]
        public async Task<IActionResult> GetScraperAnalytics(string id)
        {
            var analytics = await _scraperManager.GetScraperAnalytics(id);
            if (analytics == null)
            {
                return NotFound($"Scraper with ID {id} not found or analytics not available");
            }
            
            return Ok(analytics);
        }
        
        [HttpPut("{id}/content-extraction")]
        public async Task<IActionResult> UpdateContentExtractionRules(string id, [FromBody] Models.ContentExtractionRules rules)
        {
            var success = await _scraperManager.UpdateContentExtractionRules(id, rules);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            return Ok(new { Message = "Content extraction rules updated successfully" });
        }
        
        [HttpGet("{id}/patterns")]
        public async Task<IActionResult> GetLearnedPatterns(string id)
        {
            var patterns = await _scraperManager.GetLearnedPatterns(id);
            if (patterns == null)
            {
                return NotFound($"Scraper with ID {id} not found or has no learned patterns");
            }
            
            return Ok(patterns);
        }
        
        [HttpGet("{id}/regulatory-alerts")]
        public async Task<IActionResult> GetRegulatoryAlerts(string id, [FromQuery] DateTime? since = null, [FromQuery] string importance = null)
        {
            var alerts = await _scraperManager.GetRegulatoryAlerts(id, since, importance);
            if (alerts == null)
            {
                return NotFound($"Scraper with ID {id} not found or has no regulatory alerts");
            }
            
            return Ok(new { Alerts = alerts });
        }
        
        [HttpPut("{id}/regulatory-config")]
        public async Task<IActionResult> UpdateRegulatoryConfig(string id, [FromBody] Models.RegulatoryConfigModel config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var success = await _scraperManager.UpdateRegulatoryConfig(id, config);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            return Ok(new { Message = "Regulatory configuration updated successfully" });
        }
        
        [HttpPost("{id}/export-data")]
        public async Task<IActionResult> ExportScrapedData(string id, [FromBody] Models.ExportOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _scraperManager.ExportScrapedData(id, options);
            if (result == null)
            {
                return NotFound($"Scraper with ID {id} not found or has no data to export");
            }
            
            return Ok(result);
        }
        
        [HttpPut("{id}/webhook-config")]
        public async Task<IActionResult> UpdateWebhookConfig(string id, [FromBody] Models.WebhookConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var success = await _scraperManager.UpdateWebhookConfig(id, config);
            if (!success)
            {
                return NotFound($"Scraper with ID {id} not found or is currently running");
            }
            
            return Ok(new { Message = "Webhook configuration updated successfully" });
        }

        [HttpPost("{id}/compress")]
        public async Task<IActionResult> CompressStoredContent(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var result = await _scraperManager.CompressStoredContent(id);
            if (result == null)
            {
                return BadRequest("Failed to compress content");
            }
            
            return Ok(result);
        }

        [HttpGet("{id}/metrics")]
        public async Task<IActionResult> GetScraperMetrics(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var metrics = await _scraperManager.GetScraperMetrics(id);
            if (metrics == null)
            {
                return NotFound($"Scraper metrics not available for {id}");
            }
            
            return Ok(metrics);
        }
        
        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> ScheduleScraper(string id, [FromBody] Models.ScheduleOptions options)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var result = await _scraperManager.ScheduleScraper(id, options);
            if (!result.Success)
            {
                return BadRequest(result.Message);
            }
            
            return Ok(new { 
                Message = $"Scraper '{config.Name}' scheduled successfully", 
                ScheduleId = result.ScheduleId,
                NextRunTime = result.NextRunTime
            });
        }
        
        [HttpGet("{id}/schedules")]
        public async Task<IActionResult> GetScraperSchedules(string id)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var schedules = await _scraperManager.GetScraperSchedules(id);
            return Ok(new { Schedules = schedules });
        }
        
        [HttpDelete("{id}/schedules/{scheduleId}")]
        public async Task<IActionResult> DeleteSchedule(string id, string scheduleId)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _scraperManager.DeleteSchedule(id, scheduleId);
            if (!success)
            {
                return NotFound($"Schedule with ID {scheduleId} not found");
            }
            
            return NoContent();
        }
        
        [HttpPut("{id}/rate-limiting")]
        public async Task<IActionResult> UpdateRateLimitingConfig(string id, [FromBody] Models.RateLimitingConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var scraperConfig = await _scraperManager.GetScraperConfig(id);
            if (scraperConfig == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _scraperManager.UpdateRateLimitingConfig(id, config);
            if (!success)
            {
                return BadRequest("Failed to update rate limiting configuration");
            }
            
            return Ok(new { Message = "Rate limiting configuration updated successfully" });
        }
        
        [HttpPut("{id}/proxy-config")]
        public async Task<IActionResult> UpdateProxyConfig(string id, [FromBody] Models.ProxyConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var scraperConfig = await _scraperManager.GetScraperConfig(id);
            if (scraperConfig == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var success = await _scraperManager.UpdateProxyConfig(id, config);
            if (!success)
            {
                return BadRequest("Failed to update proxy configuration");
            }
            
            return Ok(new { Message = "Proxy configuration updated successfully" });
        }
        
        [HttpPost("{id}/test-run")]
        public async Task<IActionResult> RunScraperTest(string id, [FromBody] Models.TestRunOptions options)
        {
            var config = await _scraperManager.GetScraperConfig(id);
            if (config == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var result = await _scraperManager.RunScraperTest(id, options);
            if (result == null)
            {
                return BadRequest("Test run failed to start");
            }
            
            return Ok(result);
        }
        
        [HttpPost("batch-operation")]
        public async Task<IActionResult> BatchOperation([FromBody] Models.BatchOperationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var result = await _scraperManager.PerformBatchOperation(request);
            
            // Cast the dynamic result to access its properties
            dynamic dynamicResult = result;
            
            return Ok(new { 
                SuccessCount = dynamicResult.SuccessCount,
                FailureCount = dynamicResult.FailureCount,
                Results = dynamicResult.Results
            });
        }
        
        #endregion
    }
}
