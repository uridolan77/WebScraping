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
        public async Task<IActionResult> SetMonitoring(string id, [FromBody] MonitoringSettings settings)
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
        
        #region New API endpoints for additional functionality
        
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
        public async Task<IActionResult> UpdateContentExtractionRules(string id, [FromBody] ContentExtractionRules rules)
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
        public async Task<IActionResult> UpdateRegulatoryConfig(string id, [FromBody] RegulatoryConfigModel config)
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
        public async Task<IActionResult> ExportScrapedData(string id, [FromBody] ExportOptions options)
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
        
        #endregion
    }
    
    public class MonitoringSettings
    {
        public bool Enabled { get; set; } = true;
        public int IntervalMinutes { get; set; } = 1440; // 24 hours by default
        public bool NotifyOnChanges { get; set; } = false;
        public string NotificationEmail { get; set; }
        public bool TrackChangesHistory { get; set; } = true;
    }
    
    #region New model classes for additional endpoints
    
    public class ContentExtractionRules
    {
        public List<string> IncludeSelectors { get; set; } = new List<string>();
        public List<string> ExcludeSelectors { get; set; } = new List<string>();
        public bool ExtractMetadata { get; set; } = true;
        public bool ExtractStructuredData { get; set; } = false;
        public string CustomJsExtractor { get; set; }
    }
    
    public class RegulatoryConfigModel
    {
        public bool EnableRegulatoryContentAnalysis { get; set; } = false;
        public bool TrackRegulatoryChanges { get; set; } = false;
        public bool ClassifyRegulatoryDocuments { get; set; } = false;
        public bool ExtractStructuredContent { get; set; } = false;
        public bool ProcessPdfDocuments { get; set; } = false;
        public bool MonitorHighImpactChanges { get; set; } = false;
        public bool IsUKGCWebsite { get; set; } = false;
        public List<string> KeywordAlertList { get; set; } = new List<string>();
        public string NotificationEndpoint { get; set; }
    }
    
    public class ExportOptions
    {
        public string Format { get; set; } = "json";
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IncludeRawHtml { get; set; } = false;
        public bool IncludeProcessedContent { get; set; } = true;
        public bool IncludeMetadata { get; set; } = true;
        public string OutputPath { get; set; }
    }
    
    #endregion
}
