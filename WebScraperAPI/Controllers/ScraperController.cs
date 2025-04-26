using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using WebScraper;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private static Scraper _activeScraper;
        private static ScraperConfig _config = new ScraperConfig();
        private static bool _isRunning = false;
        private static DateTime? _startTime;
        private static DateTime? _endTime;
        private static List<string> _logMessages = new List<string>();
        private static readonly object _lock = new object();
        private static Dictionary<string, ScrapedPage> _scrapedPages = new Dictionary<string, ScrapedPage>();

        // Add a logger to capture console output
        public static void LogMessage(string message)
        {
            lock (_lock)
            {
                _logMessages.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
                // Keep only the last 1000 messages
                if (_logMessages.Count > 1000)
                {
                    _logMessages.RemoveAt(0);
                }
            }
        }

        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            return Ok(new
            {
                IsRunning = _isRunning,
                StartTime = _startTime,
                EndTime = _endTime,
                UrlsProcessed = _scrapedPages.Count,
                ElapsedTime = _startTime.HasValue ? (DateTime.Now - _startTime.Value).ToString(@"hh\:mm\:ss") : null,
                CurrentConfig = _config
            });
        }

        [HttpGet("logs")]
        public IActionResult GetLogs([FromQuery] int limit = 100)
        {
            lock (_lock)
            {
                var logs = _logMessages.Count <= limit
                    ? _logMessages
                    : _logMessages.GetRange(_logMessages.Count - limit, limit);

                return Ok(new { Logs = logs });
            }
        }

        [HttpGet("results")]
        public IActionResult GetResults([FromQuery] int limit = 20)
        {
            lock (_lock)
            {
                var results = _scrapedPages.Values
                    .OrderByDescending(p => p.ScrapedDateTime)
                    .Take(limit)
                    .ToList();

                return Ok(new { Results = results });
            }
        }

        [HttpPost("start")]
        public IActionResult StartScraping([FromBody] ScraperConfig config)
        {
            if (_isRunning)
            {
                return BadRequest("Scraper is already running");
            }

            try
            {
                // Apply default values if not provided
                if (config == null)
                {
                    config = new ScraperConfig();
                }

                if (string.IsNullOrEmpty(config.StartUrl))
                {
                    return BadRequest("Start URL is required");
                }

                _config = config;
                _startTime = DateTime.Now;
                _endTime = null;
                _isRunning = true;

                // Clear previous results if not in append mode
                if (!config.AppendToExistingData)
                {
                    _scrapedPages.Clear();
                    _logMessages.Clear();
                }

                LogMessage($"Starting scraping from {config.StartUrl}");
                
                // Initialize scraper with the LogMessage function
                _activeScraper = new Scraper(_config, LogMessage);

                // Start scraping in a background thread
                Task.Run(async () =>
                {
                    try
                    {
                        await _activeScraper.StartScrapingAsync();
                        _isRunning = false;
                        _endTime = DateTime.Now;
                        LogMessage("Scraping completed successfully");
                    }
                    catch (Exception ex)
                    {
                        _isRunning = false;
                        _endTime = DateTime.Now;
                        LogMessage($"Scraping failed: {ex.Message}");
                    }
                });

                return Ok(new { Message = "Scraper started successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to start scraper: {ex.Message}");
            }
        }

        [HttpPost("stop")]
        public IActionResult StopScraping()
        {
            if (!_isRunning)
            {
                return BadRequest("Scraper is not running");
            }

            try
            {
                // Gracefully stop the scraper
                _isRunning = false;
                _endTime = DateTime.Now;
                LogMessage("Scraping stopped by user");

                return Ok(new { Message = "Scraper stopped successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to stop scraper: {ex.Message}");
            }
        }
    }
}