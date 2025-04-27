using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using WebScraperApi.Services.Analytics;
using WebScraperApi.Services.State;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IScraperAnalyticsService _analyticsService;
        private readonly IScraperStateService _stateService;
        
        public AnalyticsController(
            IScraperAnalyticsService analyticsService,
            IScraperStateService stateService)
        {
            _analyticsService = analyticsService;
            _stateService = stateService;
        }
        
        [HttpGet("summary")]
        public async Task<IActionResult> GetAnalyticsSummary()
        {
            var summary = await _analyticsService.GetAnalyticsSummaryAsync();
            return Ok(summary);
        }
        
        [HttpGet("scrapers/{id}")]
        public async Task<IActionResult> GetScraperAnalytics(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var analytics = await _analyticsService.GetScraperAnalyticsAsync(id);
            return Ok(analytics);
        }
        
        [HttpGet("scrapers/{id}/metrics")]
        public async Task<IActionResult> GetScraperMetrics(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var metrics = await _analyticsService.GetScraperMetricsAsync(id);
            return Ok(metrics);
        }
        
        [HttpGet("scrapers/{id}/performance")]
        public async Task<IActionResult> GetScraperPerformance(string id, [FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            var performance = await _analyticsService.GetScraperPerformanceAsync(id, start, end);
            return Ok(performance);
        }
        
        [HttpGet("popular-domains")]
        public async Task<IActionResult> GetPopularDomains([FromQuery] int count = 10)
        {
            var domains = await _analyticsService.GetPopularDomainsAsync(count);
            return Ok(new { PopularDomains = domains });
        }
        
        [HttpGet("content-change-frequency")]
        public async Task<IActionResult> GetContentChangeFrequency([FromQuery] DateTime? since = null)
        {
            var frequency = await _analyticsService.GetContentChangeFrequencyAsync(since);
            return Ok(new { ChangeFrequency = frequency });
        }
        
        [HttpGet("usage-statistics")]
        public async Task<IActionResult> GetUsageStatistics([FromQuery] DateTime? start = null, [FromQuery] DateTime? end = null)
        {
            if (!start.HasValue)
            {
                start = DateTime.Now.AddDays(-30);
            }
            
            if (!end.HasValue)
            {
                end = DateTime.Now;
            }
            
            var statistics = await _analyticsService.GetUsageStatisticsAsync(start.Value, end.Value);
            return Ok(statistics);
        }
        
        [HttpGet("error-distribution")]
        public async Task<IActionResult> GetErrorDistribution([FromQuery] DateTime? since = null)
        {
            var distribution = await _analyticsService.GetErrorDistributionAsync(since);
            return Ok(new { ErrorDistribution = distribution });
        }
    }
}