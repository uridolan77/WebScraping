using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using WebScraperApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using WebScraper; // Added import for Scraper class

namespace WebScraperAPI.Controllers
{
    public partial class ScraperController
    {
        [HttpGet("{id}/detailed-status")]
        public async Task<IActionResult> GetScraperDetailedStatus(string id)
        {
            try
            {
                _logger.LogInformation($"Getting status for scraper {id}");
                bool isRunning = _activeScrapers.ContainsKey(id);
                var errors = new List<object>();
                var stats = new Dictionary<string, object>();

                // Get data from the active scraper if it's running
                if (isRunning && _activeScrapers.TryGetValue(id, out var scraper))
                {
                    _logger.LogInformation($"Scraper {id} is currently running");

                    // Access the ScraperCore to get more details
                    var coreField = typeof(Scraper).GetField("_core", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (coreField != null)
                    {
                        var core = coreField.GetValue(scraper);
                        if (core != null)
                        {
                            // Get stats using reflection
                            var props = core.GetType().GetProperties();
                            foreach (var prop in props)
                            {
                                try
                                {
                                    if (prop.Name != "Errors" && prop.CanRead)
                                    {
                                        var value = prop.GetValue(core);
                                        if (value != null)
                                        {
                                            stats[prop.Name] = value;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogDebug($"Could not get property {prop.Name}: {ex.Message}");
                                }
                            }

                            // Get the Errors property specifically
                            var errorsProperty = core.GetType().GetProperty("Errors");
                            if (errorsProperty != null)
                            {
                                var scraperErrors = errorsProperty.GetValue(core) as System.Collections.IEnumerable;
                                if (scraperErrors != null)
                                {
                                    foreach (var error in scraperErrors)
                                    {
                                        // Extract the properties from the error object
                                        var urlProperty = error.GetType().GetProperty("Url");
                                        var messageProperty = error.GetType().GetProperty("Message");
                                        var timestampProperty = error.GetType().GetProperty("Timestamp");

                                        if (urlProperty != null && messageProperty != null && timestampProperty != null)
                                        {
                                            errors.Add(new
                                            {
                                                Url = urlProperty.GetValue(error)?.ToString(),
                                                Message = messageProperty.GetValue(error)?.ToString(),
                                                Timestamp = timestampProperty.GetValue(error) is DateTime ts ? ts : DateTime.Now
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation($"Scraper {id} is not currently running");
                }

                // Try to get status from the database
                ScraperStatusEntity dbStatus = null;
                try
                {
                    dbStatus = await _scraperRepository.GetScraperStatusAsync(id);
                    if (dbStatus != null)
                    {
                        _logger.LogInformation($"Found status in database for scraper {id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not get scraper status from database: {ex.Message}");
                }

                // Use the ScraperMonitoringService to get recent logs
                var monitoringService = HttpContext.RequestServices.GetService<WebScraperApi.Services.Monitoring.IScraperMonitoringService>();
                var recentLogs = new List<object>();
                var allLogs = new List<object>();

                if (monitoringService != null)
                {
                    try
                    {
                        // Get error logs
                        recentLogs = monitoringService.GetScraperLogs(id, 10)
                            .Where(log => log.Message.ToLower().Contains("error") || log.Message.ToLower().Contains("fail"))
                            .Select(log => new { log.Timestamp, log.Message })
                            .ToList<object>();

                        // Get all recent logs (limited to 20)
                        allLogs = monitoringService.GetScraperLogs(id, 20)
                            .Select(log => new { log.Timestamp, log.Message })
                            .ToList<object>();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not get logs from monitoring service: {ex.Message}");
                    }
                }

                // Extract processed pages from logs
                var processedPages = new List<object>();
                if (allLogs.Count > 0)
                {
                    foreach (dynamic log in allLogs)
                    {
                        if (log.Message is string message && message.StartsWith("PAGE_PROCESSED|"))
                        {
                            try
                            {
                                var parts = message.Split('|');
                                if (parts.Length >= 6)
                                {
                                    processedPages.Add(new
                                    {
                                        Url = parts[1],
                                        Title = parts[2],
                                        ContentLength = int.TryParse(parts[3], out int contentLength) ? contentLength : 0,
                                        TextLength = int.TryParse(parts[4], out int textLength) ? textLength : 0,
                                        Timestamp = DateTime.TryParse(parts[5], out DateTime timestamp) ? timestamp : DateTime.Now
                                    });
                                }
                            }
                            catch
                            {
                                // Ignore parsing errors
                            }
                        }
                    }
                }

                // Get the name of the scraper
                string scraperName = "Unknown";
                try
                {
                    var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                    if (scraperConfig != null)
                    {
                        scraperName = scraperConfig.Name;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Could not get scraper name: {ErrorMessage}", ex.Message);
                }

                // Create response object with all the information we've gathered
                var response = new
                {
                    Id = id,
                    Name = scraperName,
                    IsRunning = isRunning,
                    Status = isRunning ? "Running" : "Idle",
                    LastUpdate = DateTime.Now,
                    Errors = errors,
                    RecentErrorLogs = recentLogs,
                    RecentLogs = allLogs,
                    ProcessedPages = processedPages,
                    Stats = stats,
                    HasErrors = errors.Count > 0 || recentLogs.Count > 0,
                    DatabaseStatus = dbStatus != null ? new
                    {
                        IsRunning = dbStatus.IsRunning,
                        StartTime = dbStatus.StartTime,
                        EndTime = dbStatus.EndTime,
                        UrlsProcessed = dbStatus.UrlsProcessed,
                        UrlsQueued = dbStatus.UrlsQueued,
                        Message = dbStatus.Message,
                        ElapsedTime = dbStatus.ElapsedTime,
                        LastStatusUpdate = dbStatus.LastStatusUpdate,
                        LastUpdate = dbStatus.LastUpdate
                    } : null,
                    // Add a summary section for quick overview
                    Summary = new
                    {
                        TotalPagesProcessed = dbStatus?.UrlsProcessed ?? 0,
                        RecentlyProcessedPages = processedPages.Count,
                        ErrorCount = errors.Count + recentLogs.Count,
                        RunningTime = dbStatus?.StartTime != null ? (DateTime.Now - dbStatus.StartTime.Value).ToString(@"hh\:mm\:ss") : "00:00:00",
                        CurrentStatus = dbStatus?.Message ?? (isRunning ? "Running" : "Idle")
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for scraper {ScraperId}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting status for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("{id}/logs")]
        public IActionResult GetScraperLogs(string id, [FromQuery] int limit = 100)
        {
            try
            {
                var monitoringService = HttpContext.RequestServices.GetService<WebScraperApi.Services.Monitoring.IScraperMonitoringService>();
                if (monitoringService == null)
                {
                    return StatusCode(500, new
                    {
                        Message = "Monitoring service is not available"
                    });
                }

                var logs = monitoringService.GetScraperLogs(id, limit);

                return Ok(logs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting logs for scraper with ID {Id}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting logs for scraper {id}",
                    Error = ex.Message
                });
            }
        }
    }
}