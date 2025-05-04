using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Services;
using WebScraperApi.Data.Entities;
using Microsoft.Extensions.Logging;

namespace WebScraperAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContentClassificationController : ControllerBase
    {
        private readonly ContentClassificationService _classificationService;
        private readonly ILogger<ContentClassificationController> _logger;

        public ContentClassificationController(
            ContentClassificationService classificationService,
            ILogger<ContentClassificationController> logger)
        {
            _classificationService = classificationService;
            _logger = logger;
        }

        [HttpGet("scraper/{scraperId}")]
        public async Task<IActionResult> GetContentClassifications(string scraperId, [FromQuery] int limit = 50)
        {
            try
            {
                var classifications = await _classificationService.GetContentClassificationsAsync(scraperId, limit);
                return Ok(classifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content classifications for scraper with ID {ScraperId}", scraperId);
                return StatusCode(500, new { Message = "An error occurred while retrieving content classifications", Error = ex.Message });
            }
        }

        [HttpGet("scraper/{scraperId}/url")]
        public async Task<IActionResult> GetContentClassification(string scraperId, [FromQuery] string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return BadRequest("URL parameter is required");
                }

                var classification = await _classificationService.GetContentClassificationAsync(scraperId, url);
                
                if (classification == null)
                {
                    return NotFound($"No classification found for URL: {url}");
                }

                return Ok(classification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content classification for URL {Url}", url);
                return StatusCode(500, new { Message = "An error occurred while retrieving content classification", Error = ex.Message });
            }
        }

        [HttpGet("scraper/{scraperId}/statistics")]
        public async Task<IActionResult> GetClassificationStatistics(string scraperId)
        {
            try
            {
                var statistics = await _classificationService.GetClassificationStatisticsAsync(scraperId);
                return Ok(statistics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classification statistics for scraper with ID {ScraperId}", scraperId);
                return StatusCode(500, new { Message = "An error occurred while retrieving classification statistics", Error = ex.Message });
            }
        }

        [HttpPost("scraper/{scraperId}/classify")]
        public async Task<IActionResult> ClassifyContent(string scraperId, [FromBody] ClassifyContentRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Url) || string.IsNullOrEmpty(request.Content))
                {
                    return BadRequest("URL and content are required");
                }

                var classification = await _classificationService.ClassifyContentAsync(scraperId, request.Url, request.Content);
                return Ok(classification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying content for URL {Url}", request?.Url);
                return StatusCode(500, new { Message = "An error occurred while classifying content", Error = ex.Message });
            }
        }
    }

    public class ClassifyContentRequest
    {
        public string Url { get; set; }
        public string Content { get; set; }
    }
}
