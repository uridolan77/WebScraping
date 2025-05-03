using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using WebScraper;

namespace WebScraperAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly ILogger<ScraperController> _logger;
        private readonly IConfiguration _configuration;
        private readonly Dictionary<string, Scraper> _activeScrapers = new Dictionary<string, Scraper>();
        private readonly string _configFilePath;
        
        public ScraperController(
            ILogger<ScraperController> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scraperConfigs.json");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllScrapers()
        {
            try
            {
                var configs = await LoadScraperConfigsAsync();
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scrapers");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving scrapers",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScraper(string id)
        {
            try
            {
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraper");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while retrieving scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartScraper(string id)
        {
            try
            {
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                // Check if the scraper is already running
                if (_activeScrapers.TryGetValue(id, out var existingScraper))
                {
                    return Ok(new
                    {
                        Status = "AlreadyRunning",
                        Message = "Scraper is already running"
                    });
                }
                
                // Create and configure scraper
                var scraperConfig = new ScraperConfig
                {
                    Name = config.Name,
                    StartUrl = config.StartUrl,
                    MaxDepth = config.MaxDepth,
                    DelayBetweenRequests = config.DelayBetweenRequests,
                    MaxConcurrentRequests = config.MaxConcurrentRequests
                };

                // Create logger action that logs to console
                Action<string> logAction = (message) =>
                {
                    _logger.LogInformation($"Scraper {id}: {message}");
                    Console.WriteLine($"Scraper {id}: {message}");
                };

                // Create the scraper instance
                var scraper = new Scraper(scraperConfig, logAction);
                
                // Initialize and start scraping in a background task
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await scraper.InitializeAsync();
                        await scraper.StartScrapingAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error running scraper {id}");
                    }
                    finally
                    {
                        // Remove from active scrapers when done
                        _activeScrapers.Remove(id);
                    }
                });

                // Store the scraper in the active scrapers dictionary
                _activeScrapers[id] = scraper;
                
                // Update the scraper status
                config.LastRun = DateTime.Now;
                config.RunCount++;
                await SaveScraperConfigsAsync(configs);
                
                return Ok(new
                {
                    Status = "Started",
                    Message = "Scraper started successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while starting scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpPost("{id}/stop")]
        public async Task<IActionResult> StopScraper(string id)
        {
            try
            {
                if (!_activeScrapers.TryGetValue(id, out var scraper))
                {
                    return NotFound($"No running scraper found with ID {id}");
                }
                
                // Stop the scraper
                scraper.StopScraping();
                _activeScrapers.Remove(id);
                
                // Update configs
                var configs = await LoadScraperConfigsAsync();
                await SaveScraperConfigsAsync(configs);
                
                return Ok(new
                {
                    Status = "Stopped",
                    Message = "Scraper stopped successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while stopping scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}/status")]
        public IActionResult GetScraperStatus(string id)
        {
            try
            {
                bool isRunning = _activeScrapers.ContainsKey(id);
                
                return Ok(new
                {
                    Id = id,
                    IsRunning = isRunning,
                    Status = isRunning ? "Running" : "Idle",
                    LastUpdate = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting status for scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting status for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}/logs")]
        public async Task<IActionResult> GetScraperLogs(string id, [FromQuery] int limit = 500)
        {
            try
            {
                // Use the ScraperMonitoringService to get detailed logs from the internal monitoring services
                var monitoringService = HttpContext.RequestServices.GetService<WebScraperApi.Services.Monitoring.IScraperMonitoringService>();
                
                if (monitoringService == null)
                {
                    _logger.LogWarning("Monitoring service not available");
                    return StatusCode(500, new { Message = "Monitoring service not available" });
                }

                // Get logs from the monitoring service
                var logs = monitoringService.GetScraperLogs(id, limit)
                    .Select(log => new 
                    { 
                        Timestamp = log.Timestamp, 
                        Message = log.Message,
                        Level = log.Message.ToLower().Contains("error") || log.Message.ToLower().Contains("fail") 
                            ? "error" 
                            : (log.Message.ToLower().Contains("warn") ? "warning" : "info")
                    })
                    .ToList();

                return Ok(new
                {
                    ScraperId = id,
                    LogCount = logs.Count,
                    Logs = logs
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving logs for scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while retrieving logs for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        private async Task<List<ScraperConfigModel>> LoadScraperConfigsAsync()
        {
            if (!System.IO.File.Exists(_configFilePath))
            {
                return new List<ScraperConfigModel>();
            }
            
            string json = await System.IO.File.ReadAllTextAsync(_configFilePath);
            return JsonSerializer.Deserialize<List<ScraperConfigModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ScraperConfigModel>();
        }
        
        private async Task SaveScraperConfigsAsync(List<ScraperConfigModel> configs)
        {
            string json = JsonSerializer.Serialize(configs, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            await System.IO.File.WriteAllTextAsync(_configFilePath, json);
        }
    }
    
    // Simple model class for scraper configuration
    public class ScraperConfigModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string StartUrl { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public string OutputDirectory { get; set; } = string.Empty;
        public int MaxDepth { get; set; } = 5;
        public int MaxPages { get; set; } = 1000;
        public int DelayBetweenRequests { get; set; } = 1000;
        public int MaxConcurrentRequests { get; set; } = 5;
        public bool FollowLinks { get; set; } = true;
        public bool FollowExternalLinks { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime LastModified { get; set; } = DateTime.Now;
        public DateTime? LastRun { get; set; }
        public int RunCount { get; set; } = 0;
        public List<string>? StartUrls { get; set; }
        public List<string>? ContentExtractorSelectors { get; set; }
        public List<string>? ContentExtractorExcludeSelectors { get; set; }
    }
}
