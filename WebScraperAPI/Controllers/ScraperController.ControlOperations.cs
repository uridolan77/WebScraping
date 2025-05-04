using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using WebScraper;
using WebScraperApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;

namespace WebScraperAPI.Controllers
{
    public partial class ScraperController
    {
        [HttpPost("{id}/start")]
        public async Task<IActionResult> StartScraper(string id)
        {
            try
            {
                _logger.LogInformation($"Starting scraper with ID {id}");

                // First, try to get the scraper from the database
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                ScraperConfigModel config;

                if (dbScraper != null)
                {
                    // Convert database entity to API model
                    config = new ScraperConfigModel
                    {
                        Id = dbScraper.Id,
                        Name = dbScraper.Name,
                        StartUrl = dbScraper.StartUrl,
                        BaseUrl = dbScraper.BaseUrl,
                        OutputDirectory = dbScraper.OutputDirectory,
                        MaxDepth = dbScraper.MaxDepth,
                        MaxPages = dbScraper.MaxPages,
                        DelayBetweenRequests = dbScraper.DelayBetweenRequests,
                        MaxConcurrentRequests = dbScraper.MaxConcurrentRequests,
                        FollowLinks = dbScraper.FollowLinks,
                        FollowExternalLinks = dbScraper.FollowExternalLinks,
                        CreatedAt = dbScraper.CreatedAt,
                        LastModified = dbScraper.LastModified,
                        LastRun = dbScraper.LastRun,
                        RunCount = dbScraper.RunCount
                    };

                    _logger.LogInformation($"Found scraper {id} in database, starting it");
                }
                else
                {
                    // Fallback to JSON file if not found in database
                    var configs = await LoadScraperConfigsAsync();
                    config = configs.FirstOrDefault(c => c.Id == id);

                    if (config == null)
                    {
                        return NotFound($"Scraper with ID {id} not found");
                    }

                    _logger.LogWarning($"Scraper with ID {id} not found in database, falling back to JSON file.");
                }

                // Check if the scraper is already running
                if (_activeScrapers.TryGetValue(id, out var existingScraper))
                {
                    _logger.LogInformation($"Scraper {id} is already running");
                    return Ok(new
                    {
                        Status = "AlreadyRunning",
                        Message = "Scraper is already running"
                    });
                }

                // Validate the configuration
                if (string.IsNullOrEmpty(config.StartUrl))
                {
                    _logger.LogError($"Scraper {id} has an invalid or empty start URL");
                    return BadRequest(new
                    {
                        Status = "ConfigurationError",
                        Message = "The scraper has an invalid or empty start URL"
                    });
                }

                _logger.LogInformation($"Creating scraper configuration for {id}. Start URL: {config.StartUrl}, MaxDepth: {config.MaxDepth}");

                // Create and configure scraper with all available settings
                var scraperConfig = new ScraperConfig
                {
                    Name = config.Name,
                    StartUrl = config.StartUrl,
                    BaseUrl = config.BaseUrl ?? new Uri(config.StartUrl).GetLeftPart(UriPartial.Authority),
                    OutputDirectory = config.OutputDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData", config.Id),
                    MaxDepth = config.MaxDepth,
                    DelayBetweenRequests = config.DelayBetweenRequests,
                    MaxConcurrentRequests = config.MaxConcurrentRequests,
                    EnablePersistentState = true,
                    FollowExternalLinks = config.FollowExternalLinks
                };

                // Get the monitoring service during the request
                var monitoringService = HttpContext.RequestServices.GetService<WebScraperApi.Services.Monitoring.IScraperMonitoringService>();

                // Create logger action that logs to console and monitoring service
                Action<string> logAction = (message) =>
                {
                    _logger.LogInformation($"Scraper {id}: {message}");
                    Console.WriteLine($"Scraper {id}: {message}");

                    // Also log to monitoring service if it was available
                    if (monitoringService != null)
                    {
                        try
                        {
                            monitoringService.AddLogMessage(id, message);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error logging to monitoring service: {ex.Message}");
                        }
                    }
                };

                _logger.LogInformation($"Creating scraper instance for {id}");

                // Create the scraper instance
                var scraper = new Scraper(scraperConfig, logAction);

                // Start the scraper in a separate process
                try
                {
                    _logger.LogInformation($"Starting scraper {id} in a separate process");

                    // Get the path to the current executable
                    string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string scraperRunnerPath = Path.Combine(currentDirectory, "WebScraperRunner.exe");

                    // Check if the runner exists
                    if (!System.IO.File.Exists(scraperRunnerPath))
                    {
                        _logger.LogWarning($"WebScraperRunner.exe not found at {scraperRunnerPath}, falling back to in-process execution");

                        // Fall back to in-process execution
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInformation($"Initializing scraper {id} in-process");
                                await scraper.InitializeAsync();

                                _logger.LogInformation($"Starting scraping process for {id}");
                                await scraper.StartScrapingAsync();

                                _logger.LogInformation($"Scraping completed for {id}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error running scraper {id}: {ex.Message}");

                                if (ex.InnerException != null)
                                {
                                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                                }
                            }
                            finally
                            {
                                // Remove from active scrapers when done
                                _logger.LogInformation($"Removing scraper {id} from active scrapers list");
                                _activeScrapers.Remove(id);
                            }
                        });
                    }
                    else
                    {
                        // Start the scraper runner process
                        var processStartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = scraperRunnerPath,
                            Arguments = $"--scraper-id {id} --connection-string \"{_configuration.GetConnectionString("DefaultConnection")}\"",
                            UseShellExecute = false,
                            CreateNoWindow = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true
                        };

                        var process = new System.Diagnostics.Process
                        {
                            StartInfo = processStartInfo,
                            EnableRaisingEvents = true
                        };

                        // Set up event handlers
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                _logger.LogInformation($"Scraper {id} output: {e.Data}");
                            }
                        };

                        process.ErrorDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                _logger.LogError($"Scraper {id} error: {e.Data}");
                            }
                        };

                        process.Exited += (sender, e) =>
                        {
                            _logger.LogInformation($"Scraper {id} process exited with code {process.ExitCode}");
                            _activeScrapers.Remove(id);
                        };

                        // Start the process
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        _logger.LogInformation($"Started scraper {id} in separate process with PID {process.Id}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error starting scraper {id} in separate process: {ex.Message}");

                    // Fall back to in-process execution
                    _logger.LogWarning($"Falling back to in-process execution for scraper {id}");

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            _logger.LogInformation($"Initializing scraper {id} in-process");
                            await scraper.InitializeAsync();

                            _logger.LogInformation($"Starting scraping process for {id}");
                            await scraper.StartScrapingAsync();

                            _logger.LogInformation($"Scraping completed for {id}");
                        }
                        catch (Exception runEx)
                        {
                            _logger.LogError(runEx, $"Error running scraper {id}: {runEx.Message}");

                            if (runEx.InnerException != null)
                            {
                                _logger.LogError($"Inner exception: {runEx.InnerException.Message}");
                            }
                        }
                        finally
                        {
                            // Remove from active scrapers when done
                            _logger.LogInformation($"Removing scraper {id} from active scrapers list");
                            _activeScrapers.Remove(id);
                        }
                    });
                }

                // Store the scraper in the active scrapers dictionary
                _activeScrapers[id] = scraper;

                // Update the scraper status
                if (dbScraper != null)
                {
                    // Update database record
                    _logger.LogInformation($"Updating database record for scraper {id}");
                    dbScraper.LastRun = DateTime.Now;
                    dbScraper.RunCount++;
                    await _scraperRepository.UpdateScraperAsync(dbScraper);
                }
                else
                {
                    // Update JSON file
                    _logger.LogInformation($"Updating JSON file for scraper {id}");
                    config.LastRun = DateTime.Now;
                    config.RunCount++;
                    var configs = await LoadScraperConfigsAsync();
                    var configIndex = configs.FindIndex(c => c.Id == id);
                    if (configIndex >= 0)
                    {
                        configs[configIndex] = config;
                        await SaveScraperConfigsAsync(configs);
                    }
                }

                // Try to update scraper status in database if monitoring service is available
                try
                {
                    var status = new WebScraperApi.Data.Entities.ScraperStatusEntity
                    {
                        ScraperId = id,
                        IsRunning = true,
                        StartTime = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        UrlsProcessed = 0,
                        HasErrors = false,
                        Message = "Scraper started successfully"
                    };

                    await _scraperRepository.UpdateScraperStatusAsync(status);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Could not update scraper status in database: {ex.Message}");
                }

                _logger.LogInformation($"Scraper {id} started successfully");

                return Ok(new
                {
                    Status = "Started",
                    Message = "Scraper started successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting scraper {id}: {ex.Message}");
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
                _logger.LogInformation($"Attempting to stop scraper with ID {id}");

                bool scraperWasActive = false;

                // Check if the scraper is in the active scrapers dictionary
                if (_activeScrapers.TryGetValue(id, out var scraper))
                {
                    _logger.LogInformation($"Found active scraper with ID {id}, stopping it");

                    // Stop the scraper
                    scraper.StopScraping();
                    _activeScrapers.Remove(id);

                    _logger.LogInformation($"Successfully stopped scraper with ID {id}");
                    scraperWasActive = true;
                }
                else
                {
                    _logger.LogWarning($"No active scraper instance found with ID {id}, checking for running process");

                    // Check if there's a running process for this scraper
                    try
                    {
                        var processes = System.Diagnostics.Process.GetProcessesByName("WebScraperRunner");
                        foreach (var process in processes)
                        {
                            try
                            {
                                // Try to kill the process
                                _logger.LogInformation($"Found WebScraperRunner process with ID {process.Id}, attempting to kill it");
                                process.Kill();
                                _logger.LogInformation($"Successfully killed WebScraperRunner process with ID {process.Id}");
                                scraperWasActive = true;
                            }
                            catch (Exception processEx)
                            {
                                _logger.LogError(processEx, $"Error killing WebScraperRunner process with ID {process.Id}");
                            }
                        }
                    }
                    catch (Exception processEx)
                    {
                        _logger.LogError(processEx, "Error getting WebScraperRunner processes");
                    }

                    if (!scraperWasActive)
                    {
                        _logger.LogWarning($"No active scraper process found for ID {id}, but will update database status anyway");
                    }
                }

                // First, check if the scraper exists in the database
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);

                if (dbScraper != null)
                {
                    // Update the database record
                    _logger.LogInformation($"Updating status for scraper {id} in database");
                    await _scraperRepository.UpdateScraperAsync(dbScraper);

                    // Update scraper status
                    try
                    {
                        // Get current status to preserve any fields we don't want to change
                        var currentStatus = await _scraperRepository.GetScraperStatusAsync(id);

                        // Create new status object
                        var status = new WebScraperApi.Data.Entities.ScraperStatusEntity
                        {
                            ScraperId = id,
                            IsRunning = false,
                            EndTime = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            Message = scraperWasActive ? "Scraper stopped by user" : "Scraper marked as stopped (no active instance found)"
                        };

                        // If we have current status, preserve some fields
                        if (currentStatus != null)
                        {
                            status.StartTime = currentStatus.StartTime;
                            status.UrlsProcessed = currentStatus.UrlsProcessed;
                            status.UrlsQueued = currentStatus.UrlsQueued;
                            status.DocumentsProcessed = currentStatus.DocumentsProcessed;
                            status.HasErrors = currentStatus.HasErrors;
                        }

                        await _scraperRepository.UpdateScraperStatusAsync(status);
                        _logger.LogInformation($"Updated scraper status in database for {id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Could not update scraper status in database: {ex.Message}");
                    }
                }
                else
                {
                    // Fallback to JSON file
                    _logger.LogWarning($"Scraper with ID {id} not found in database, updating JSON file");
                    var configs = await LoadScraperConfigsAsync();
                    var config = configs.FirstOrDefault(c => c.Id == id);

                    if (config != null)
                    {
                        config.LastModified = DateTime.Now;
                        await SaveScraperConfigsAsync(configs);
                        _logger.LogInformation($"Updated scraper configuration in JSON file for {id}");
                    }
                }

                return Ok(new
                {
                    Status = "Stopped",
                    Message = scraperWasActive ? "Scraper stopped successfully" : "Scraper marked as stopped (no active instance found)"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error stopping scraper {id}: {ex.Message}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while stopping scraper {id}",
                    Error = ex.Message
                });
            }
        }
    }
}