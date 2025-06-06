using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Reflection;
using WebScraperApi.Data.Entities;
using Microsoft.Extensions.DependencyInjection;
using WebScraper; // Added import for Scraper class
using WebScraper.Monitoring; // Added import for ScraperMetrics

namespace WebScraperAPI.Controllers
{
    public partial class ScraperController
    {
        // New method to update metrics from the scraper to the database
        private async Task UpdateScraperMetricsFromRuntime(string id, Scraper scraper)
        {
            try
            {
                _logger.LogInformation($"Updating metrics for scraper {id}");

                // Get the ScraperCore instance using reflection
                var coreField = typeof(Scraper).GetField("_core", BindingFlags.NonPublic | BindingFlags.Instance);
                if (coreField == null || scraper == null)
                {
                    _logger.LogWarning("Could not access scraper core field");
                    return;
                }

                var core = coreField.GetValue(scraper);
                if (core == null)
                {
                    _logger.LogWarning("Scraper core is null");
                    return;
                }

                // Get the metrics object using reflection
                var metricsProperty = core.GetType().GetProperty("Metrics");
                if (metricsProperty == null)
                {
                    _logger.LogWarning("Could not find Metrics property");
                    return;
                }

                var metrics = metricsProperty.GetValue(core) as ScraperMetrics;
                if (metrics == null)
                {
                    _logger.LogWarning("Metrics object is null");
                    return;
                }

                // Get scraper name
                string scraperName = "Unknown";
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig != null)
                {
                    scraperName = scraperConfig.Name;
                }

                // Try to find the current run ID for this scraper
                string currentRunId = null;

                // Check if we have the run ID stored in the scraper object (using reflection)
                var runIdField = typeof(Scraper).GetField("_runId", BindingFlags.NonPublic | BindingFlags.Instance);
                if (runIdField != null)
                {
                    currentRunId = runIdField.GetValue(scraper) as string;
                }

                // If we couldn't get the run ID directly, try to find the most recent active run
                if (string.IsNullOrEmpty(currentRunId))
                {
                    var activeRuns = await _scraperRepository.GetScraperRunsAsync(id, 1);
                    if (activeRuns.Count > 0 && activeRuns[0].EndTime == null)
                    {
                        currentRunId = activeRuns[0].Id;
                    }
                }

                _logger.LogDebug($"Using run ID {currentRunId ?? "none"} for metrics update");

                // Log the metrics to console so we can see their values
                Console.WriteLine($"DIRECT-CONSOLE: PagesProcessed={metrics.PagesProcessed}, DocumentsProcessed={metrics.DocumentsProcessed}");

                // Update key metrics in the database
                await UpdateMetric(id, scraperName, "PagesProcessed", metrics.PagesProcessed, currentRunId);
                await UpdateMetric(id, scraperName, "ProcessedUrls", metrics.ProcessedUrls, currentRunId);
                await UpdateMetric(id, scraperName, "SuccessfulUrls", metrics.SuccessfulUrls, currentRunId);
                await UpdateMetric(id, scraperName, "FailedUrls", metrics.FailedUrls, currentRunId);
                await UpdateMetric(id, scraperName, "DocumentsProcessed", metrics.DocumentsProcessed, currentRunId);
                await UpdateMetric(id, scraperName, "TotalLinksExtracted", metrics.TotalLinksExtracted, currentRunId);
                await UpdateMetric(id, scraperName, "ContentItemsExtracted", metrics.ContentItemsExtracted, currentRunId);
                await UpdateMetric(id, scraperName, "TotalBytesDownloaded", metrics.TotalBytesDownloaded, currentRunId);
                await UpdateMetric(id, scraperName, "ClientErrors", metrics.ClientErrors, currentRunId);
                await UpdateMetric(id, scraperName, "ServerErrors", metrics.ServerErrors, currentRunId);
                await UpdateMetric(id, scraperName, "AveragePageProcessingTimeMs", metrics.AveragePageProcessingTimeMs, currentRunId);
                await UpdateMetric(id, scraperName, "AverageLinksPerPage", metrics.AverageLinksPerPage, currentRunId);
                await UpdateMetric(id, scraperName, "CurrentMemoryUsageMB", metrics.CurrentMemoryUsageMB, currentRunId);
                await UpdateMetric(id, scraperName, "PeakMemoryUsageMB", metrics.PeakMemoryUsageMB, currentRunId);

                // Get the queue count directly from the core if possible
                int queuedUrlsCount = 0;
                try {
                    var queuedUrlsProperty = core.GetType().GetProperty("QueuedUrls");
                    if (queuedUrlsProperty != null) {
                        var queuedUrls = queuedUrlsProperty.GetValue(core);
                        if (queuedUrls != null) {
                            // Try to get the count from the collection
                            if (queuedUrls is System.Collections.ICollection collection) {
                                queuedUrlsCount = collection.Count;
                            }
                        }
                    }

                    // Update scraper status with the latest metrics
                    var status = new WebScraperApi.Data.Entities.ScraperStatusEntity {
                        ScraperId = id,
                        IsRunning = true,
                        UrlsProcessed = (int)metrics.PagesProcessed,
                        DocumentsProcessed = (int)metrics.DocumentsProcessed,
                        UrlsQueued = queuedUrlsCount,
                        LastUpdate = DateTime.Now
                    };

                    await _scraperRepository.UpdateScraperStatusAsync(status);
                    _logger.LogInformation($"Updated scraper status for {id}");
                } catch (Exception ex) {
                    _logger.LogWarning($"Error updating scraper status: {ex.Message}");
                }

                // If we have a run ID, update the run with summary metrics
                if (!string.IsNullOrEmpty(currentRunId))
                {
                    try
                    {
                        var run = await _scraperRepository.GetScraperRunByIdAsync(currentRunId);
                        if (run != null)
                        {
                            run.UrlsProcessed = (int)metrics.PagesProcessed;
                            run.DocumentsProcessed = (int)metrics.DocumentsProcessed;

                            // Get the current status to update the elapsed time
                            var currentStatus = await _scraperRepository.GetScraperStatusAsync(id);
                            if (currentStatus != null)
                            {
                                run.ElapsedTime = currentStatus.ElapsedTime;
                            }

                            await _scraperRepository.UpdateScraperRunAsync(run);
                            _logger.LogInformation($"Updated run {currentRunId} with latest metrics");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating run {currentRunId} with metrics summary");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metrics for scraper {id}");
            }
        }

        // Helper method to update a single metric
        private async Task UpdateMetric(string scraperId, string scraperName, string metricName, double metricValue, string runId = null)
        {
            try
            {
                _logger.LogInformation($"=== METRIC SAVE ATTEMPT: {metricName}={metricValue} for scraper {scraperId} ===");
                // Add direct console output that will show up regardless of log level settings
                Console.WriteLine($"DIRECT-METRIC: {metricName}={metricValue} for {scraperName} ({scraperId})");

                // Get the scraperConfigId first, which is needed for the database schema
                string scraperConfigId = null;
                try
                {
                    var scraperConfig = await _scraperRepository.GetScraperByIdAsync(scraperId);
                    if (scraperConfig != null)
                    {
                        scraperConfigId = scraperConfig.Id;
                        _logger.LogInformation($"Found scraperConfigId: {scraperConfigId}");
                    }
                    else
                    {
                        _logger.LogWarning($"Could not find scraper config for ID {scraperId}");
                        Console.WriteLine($"DIRECT-METRIC-WARNING: Could not find scraper config for ID {scraperId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Could not get scraperConfigId for scraper {scraperId}");
                    Console.WriteLine($"DIRECT-METRIC-ERROR: Could not get scraperConfigId for scraper {scraperId} - {ex.Message}");
                }

                // Create metric entity with the correct fields from the schema
                var metric = new WebScraperApi.Data.ScraperMetricEntity
                {
                    ScraperId = scraperId,
                    ScraperConfigId = scraperConfigId,
                    ScraperName = scraperName ?? "Unknown", // Ensure ScraperName is not null
                    MetricName = metricName,
                    MetricValue = metricValue,
                    Timestamp = DateTime.Now,
                    RunId = runId
                };

                _logger.LogInformation($"Calling repository AddScraperMetricAsync with metric: {metric.MetricName}={metric.MetricValue}, ScraperId={metric.ScraperId}, ScraperName={metric.ScraperName}");
                Console.WriteLine($"DIRECT-METRIC-SAVE: Sending {metric.MetricName}={metric.MetricValue} to repository");

                try
                {
                    // Persist the metric
                    var savedMetric = await _scraperRepository.AddScraperMetricAsync(metric);

                    if (savedMetric != null && savedMetric.Id > 0)
                    {
                        _logger.LogInformation($"SUCCESS! Metric saved with ID: {savedMetric.Id}");
                        Console.WriteLine($"DIRECT-METRIC-SUCCESS: {metric.MetricName}={metric.MetricValue} saved with ID: {savedMetric.Id}");
                    }
                    else
                    {
                        _logger.LogWarning($"SavedMetric returned but possibly no rows added. ID: {savedMetric?.Id}");
                        Console.WriteLine($"DIRECT-METRIC-WARNING: {metric.MetricName} returned with ID: {savedMetric?.Id}, but may not have been saved");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Repository call failed for saving metric {metricName}");
                    Console.WriteLine($"DIRECT-METRIC-ERROR: Failed to save {metricName} to database: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"DIRECT-METRIC-ERROR-INNER: {ex.InnerException.Message}");
                    }
                    // Don't rethrow so we can continue with other metrics
                }

                // If we have a run ID, update the run with the latest metrics
                if (!string.IsNullOrEmpty(runId))
                {
                    try
                    {
                        var run = await _scraperRepository.GetScraperRunByIdAsync(runId);
                        if (run != null)
                        {
                            // Update run metrics based on metric name
                            switch (metricName)
                            {
                                case "PagesProcessed":
                                case "ProcessedUrls":
                                    run.UrlsProcessed = (int)metricValue;
                                    break;
                                case "DocumentsProcessed":
                                    run.DocumentsProcessed = (int)metricValue;
                                    break;
                            }

                            await _scraperRepository.UpdateScraperRunAsync(run);
                            _logger.LogInformation($"Updated run {runId} with metric {metricName}={metricValue}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating run {runId} with metric {metricName}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metric {metricName} for scraper {scraperId}");
                Console.WriteLine($"DIRECT-METRIC-CRITICAL-ERROR: Failed to update {metricName}: {ex.Message}");
            }
        }

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

                    // Update metrics from the running scraper to the database using the metrics service
                    var metricsService = HttpContext.RequestServices.GetRequiredService<WebScraperApi.Services.Monitoring.IScraperMetricsService>();
                    await metricsService.UpdateScraperMetricsFromRuntimeAsync(id, scraper);

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
                ScraperStatusEntity? dbStatus = null;
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
        public async Task<IActionResult> GetScraperLogs(string id, [FromQuery] int limit = 100)
        {
            try
            {
                _logger.LogInformation("Getting logs for scraper with ID {Id}, limit {Limit}", id, limit);

                // Get logs directly from the scraperlog table
                var dbLogs = await _scraperRepository.GetScraperLogsAsync(id, limit);

                // Map database entities to response model
                var logEntries = dbLogs.Select(log => new
                {
                    timestamp = log.Timestamp,
                    level = log.LogLevel?.ToLower(), // Convert to lowercase to match frontend expectations
                    message = log.Message
                }).ToList();

                _logger.LogInformation("Returning {Count} log entries for scraper {Id}", logEntries.Count, id);
                return Ok(new { logs = logEntries });
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

        [HttpGet("{id}/metrics")]
        public async Task<IActionResult> GetScraperMetrics(string id)
        {
            try
            {
                _logger.LogInformation("Getting metrics for scraper with ID {Id}", id);

                // Check if scraper is currently running
                bool isRunning = _activeScrapers.ContainsKey(id);

                // If the scraper is running, update the metrics from runtime first
                if (isRunning && _activeScrapers.TryGetValue(id, out var scraper))
                {
                    await UpdateScraperMetricsFromRuntime(id, scraper);
                }

                // Get metrics from the database
                var from = DateTime.Now.AddHours(-24); // Last 24 hours by default
                var to = DateTime.Now;

                var metrics = await _scraperRepository.GetScraperMetricsAsync(id, string.Empty, from, to);

                // Process metrics into a more usable format
                var groupedMetrics = metrics
                    .GroupBy(m => m.MetricName)
                    .Select(g => new
                    {
                        name = g.Key,
                        latestValue = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.MetricValue ?? 0,
                        history = g.OrderBy(m => m.Timestamp)
                            .Select(m => new
                            {
                                timestamp = m.Timestamp,
                                value = m.MetricValue
                            }).ToList()
                    })
                    .ToList();

                // Get scraper status
                var status = await _scraperRepository.GetScraperStatusAsync(id);

                // Return combined metrics and status info
                return Ok(new
                {
                    status = status != null ? new
                    {
                        isRunning = status.IsRunning,
                        urlsProcessed = status.UrlsProcessed,
                        documentsProcessed = status.DocumentsProcessed,
                        startTime = status.StartTime,
                        endTime = status.EndTime,
                        elapsedTime = status.ElapsedTime,
                        lastUpdate = status.LastUpdate,
                        hasErrors = status.HasErrors,
                        message = status.Message
                    } : null,
                    metrics = groupedMetrics,
                    metricSummaries = new
                    {
                        performance = GetMetricValueDirect(metrics, "AveragePageProcessingTimeMs"),
                        pagesProcessed = GetMetricValueDirect(metrics, "PagesProcessed"),
                        urlsProcessed = GetMetricValueDirect(metrics, "ProcessedUrls"),
                        successfulUrls = GetMetricValueDirect(metrics, "SuccessfulUrls"),
                        failedUrls = GetMetricValueDirect(metrics, "FailedUrls"),
                        documentsProcessed = GetMetricValueDirect(metrics, "DocumentsProcessed"),
                        linksExtracted = GetMetricValueDirect(metrics, "TotalLinksExtracted"),
                        contentExtracted = GetMetricValueDirect(metrics, "ContentItemsExtracted"),
                        bytesDownloaded = GetMetricValueDirect(metrics, "TotalBytesDownloaded"),
                        clientErrors = GetMetricValueDirect(metrics, "ClientErrors"),
                        serverErrors = GetMetricValueDirect(metrics, "ServerErrors"),
                        memoryUsage = GetMetricValueDirect(metrics, "CurrentMemoryUsageMB")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for scraper with ID {Id}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting metrics for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("runs/{runId}/metrics")]
        public async Task<IActionResult> GetScraperRunMetrics(string runId)
        {
            try
            {
                _logger.LogInformation("Getting metrics for scraper run with ID {RunId}", runId);

                // First get the run information
                var run = await _scraperRepository.GetScraperRunByIdAsync(runId);
                if (run == null)
                {
                    return NotFound($"Scraper run with ID {runId} not found");
                }

                // Get metrics from the database for this run
                // Since we don't have direct RunId column yet, we'll get metrics within the run's time range
                var from = run.StartTime;
                var to = run.EndTime ?? DateTime.Now;

                var metrics = await _scraperRepository.GetScraperMetricsAsync(run.ScraperId, string.Empty, from, to);

                // Process metrics into a more usable format
                var groupedMetrics = metrics
                    .GroupBy(m => m.MetricName)
                    .Select(g => new
                    {
                        name = g.Key,
                        // For a run, we're most interested in the last value (end of run)
                        finalValue = g.OrderByDescending(m => m.Timestamp).FirstOrDefault()?.MetricValue ?? 0,
                        // Initial value (start of run)
                        initialValue = g.OrderBy(m => m.Timestamp).FirstOrDefault()?.MetricValue ?? 0,
                        // History
                        history = g.OrderBy(m => m.Timestamp)
                            .Select(m => new
                            {
                                timestamp = m.Timestamp,
                                value = m.MetricValue
                            }).ToList()
                    })
                    .ToList();

                // Return run details and metrics
                return Ok(new
                {
                    runInfo = new
                    {
                        id = run.Id,
                        scraperId = run.ScraperId,
                        startTime = run.StartTime,
                        endTime = run.EndTime,
                        urlsProcessed = run.UrlsProcessed,
                        documentsProcessed = run.DocumentsProcessed,
                        successful = run.Successful,
                        errorMessage = run.ErrorMessage,
                        elapsedTime = run.ElapsedTime
                    },
                    metrics = groupedMetrics,
                    metricSummaries = new
                    {
                        performance = GetMetricValueDirect(metrics, "AveragePageProcessingTimeMs"),
                        pagesProcessed = GetMetricValueDirect(metrics, "PagesProcessed"),
                        urlsProcessed = GetMetricValueDirect(metrics, "ProcessedUrls"),
                        successfulUrls = GetMetricValueDirect(metrics, "SuccessfulUrls"),
                        failedUrls = GetMetricValueDirect(metrics, "FailedUrls"),
                        documentsProcessed = GetMetricValueDirect(metrics, "DocumentsProcessed"),
                        linksExtracted = GetMetricValueDirect(metrics, "TotalLinksExtracted"),
                        contentExtracted = GetMetricValueDirect(metrics, "ContentItemsExtracted"),
                        bytesDownloaded = GetMetricValueDirect(metrics, "TotalBytesDownloaded"),
                        clientErrors = GetMetricValueDirect(metrics, "ClientErrors"),
                        serverErrors = GetMetricValueDirect(metrics, "ServerErrors"),
                        memoryUsage = GetMetricValueDirect(metrics, "CurrentMemoryUsageMB")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for scraper run with ID {RunId}", runId);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting metrics for scraper run {runId}",
                    Error = ex.Message
                });
            }
        }

        // Modified helper method that doesn't rely on specific namespace
        private double GetMetricValueDirect(IEnumerable<dynamic> metrics, string metricName)
        {
            return metrics
                .Where(m => m.MetricName == metricName)
                .OrderByDescending(m => m.Timestamp)
                .FirstOrDefault()?.MetricValue ?? 0;
        }

        // New endpoint to match frontend expectation for /status
        [HttpGet("{id}/status")]
        public async Task<IActionResult> GetScraperStatus(string id)
        {
            // This is a simplified version of the detailed status to match what the frontend expects
            try
            {
                // Check if scraper is running
                bool isRunning = _activeScrapers.ContainsKey(id);

                // Try to get status from the database
                var dbStatus = await _scraperRepository.GetScraperStatusAsync(id);

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
                catch
                {
                    // Silently fail if we can't get the name
                }

                // Create a simplified response
                var response = new
                {
                    Id = id,
                    Name = scraperName,
                    IsRunning = isRunning || (dbStatus?.IsRunning ?? false),
                    Status = isRunning ? "Running" : (dbStatus?.IsRunning ?? false ? "Running" : "Idle"),
                    LastUpdate = dbStatus?.LastUpdate ?? DateTime.Now,
                    UrlsProcessed = dbStatus?.UrlsProcessed ?? 0,
                    DocumentsProcessed = dbStatus?.DocumentsProcessed ?? 0,
                    Message = dbStatus?.Message ?? (isRunning ? "Running" : "Idle"),
                    StartTime = dbStatus?.StartTime,
                    EndTime = dbStatus?.EndTime,
                    ElapsedTime = dbStatus?.ElapsedTime
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting simple status for scraper {ScraperId}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting status for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        // Direct GET by ID endpoint for the base route
        [HttpGet("{id}")]
        public async Task<IActionResult> GetScraperByIdBase(string id)
        {
            try
            {
                _logger.LogInformation($"Getting scraper with ID {id}");

                // First try to get the scraper from the database
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);

                if (dbScraper != null)
                {
                    // Convert the database entity to API model
                    var scraperModel = new ScraperConfigModel
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
                        RunCount = dbScraper.RunCount,
                        // Include related entity collections
                        StartUrls = dbScraper.StartUrls?.Select(u => u.Url)?.ToList(),
                        ContentExtractorSelectors = dbScraper.ContentExtractorSelectors?.Where(c => !c.IsExclude)?.Select(c => c.Selector)?.ToList(),
                        ContentExtractorExcludeSelectors = dbScraper.ContentExtractorSelectors?.Where(c => c.IsExclude)?.Select(c => c.Selector)?.ToList()
                    };

                    return Ok(scraperModel);
                }

                // If not found, return 404
                return NotFound($"Scraper with ID {id} not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraper with ID {Id}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while retrieving scraper {id}",
                    Error = ex.Message
                });
            }
        }

        // Add monitor endpoint to match frontend expectations
        [HttpGet("{id}/monitor")]
        public async Task<IActionResult> GetScraperMonitor(string id)
        {
            try
            {
                _logger.LogInformation($"Getting monitor data for scraper {id}");
                bool isRunning = _activeScrapers.ContainsKey(id);
                var errors = new List<object>();
                var stats = new Dictionary<string, object>();

                // Get data from the active scraper if it's running
                if (isRunning && _activeScrapers.TryGetValue(id, out var scraper))
                {
                    _logger.LogInformation($"Scraper {id} is currently running, fetching real-time data");

                    // Update metrics from the running scraper to the database
                    await UpdateScraperMetricsFromRuntime(id, scraper);

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

                            // Ensure key counters are included in stats if they exist in the core
                            EnsureCounterIncluded(stats, core, "UrlsProcessed", "pagesProcessed");
                            EnsureCounterIncluded(stats, core, "DocumentsProcessed", "documentsProcessed");
                            EnsureCounterIncluded(stats, core, "QueuedUrls", "urlsQueued");
                        }
                    }
                }

                // Try to get status from the database
                var dbStatus = await _scraperRepository.GetScraperStatusAsync(id);

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

                // Get recent metrics
                var from = DateTime.Now.AddHours(-1); // Last hour
                var to = DateTime.Now;
                var metrics = await _scraperRepository.GetScraperMetricsAsync(id, string.Empty, from, to);

                // Process metrics
                var processedMetrics = new Dictionary<string, double>();
                foreach (var metric in metrics.OrderByDescending(m => m.Timestamp).GroupBy(m => m.MetricName))
                {
                    if (metric.Any())
                    {
                        processedMetrics[metric.Key] = metric.First().MetricValue;
                    }
                }

                // Get recent logs - increase to 100 for more comprehensive display
                var dbLogs = await _scraperRepository.GetScraperLogsAsync(id, 100);
                var recentLogs = dbLogs.Select(log => new
                {
                    timestamp = log.Timestamp,
                    level = log.LogLevel?.ToLower() ?? "info",
                    message = log.Message
                }).ToList();

                // Determine counters from all available sources (active scraper, metrics, db status)
                double pagesProcessed = 0;
                double documentsProcessed = 0;
                double urlsQueued = 0;

                // First check stats from active scraper (most up-to-date)
                if (stats.ContainsKey("UrlsProcessed") && stats["UrlsProcessed"] is IConvertible)
                    pagesProcessed = Convert.ToDouble(stats["UrlsProcessed"]);
                else if (stats.ContainsKey("pagesProcessed") && stats["pagesProcessed"] is IConvertible)
                    pagesProcessed = Convert.ToDouble(stats["pagesProcessed"]);

                if (stats.ContainsKey("DocumentsProcessed") && stats["DocumentsProcessed"] is IConvertible)
                    documentsProcessed = Convert.ToDouble(stats["DocumentsProcessed"]);
                else if (stats.ContainsKey("documentsProcessed") && stats["documentsProcessed"] is IConvertible)
                    documentsProcessed = Convert.ToDouble(stats["documentsProcessed"]);

                if (stats.ContainsKey("QueuedUrls") && stats["QueuedUrls"] is IConvertible)
                    urlsQueued = Convert.ToDouble(stats["QueuedUrls"]);
                else if (stats.ContainsKey("urlsQueued") && stats["urlsQueued"] is IConvertible)
                    urlsQueued = Convert.ToDouble(stats["urlsQueued"]);

                // Then check metrics (next most up-to-date)
                if (pagesProcessed == 0 && processedMetrics.ContainsKey("PagesProcessed"))
                    pagesProcessed = processedMetrics["PagesProcessed"];
                else if (pagesProcessed == 0 && processedMetrics.ContainsKey("ProcessedUrls"))
                    pagesProcessed = processedMetrics["ProcessedUrls"];

                if (documentsProcessed == 0 && processedMetrics.ContainsKey("DocumentsProcessed"))
                    documentsProcessed = processedMetrics["DocumentsProcessed"];

                // Finally check db status (least up-to-date)
                if (pagesProcessed == 0 && dbStatus != null)
                    pagesProcessed = dbStatus.UrlsProcessed;

                if (documentsProcessed == 0 && dbStatus != null)
                    documentsProcessed = dbStatus.DocumentsProcessed;

                if (urlsQueued == 0 && dbStatus != null)
                    urlsQueued = dbStatus.UrlsQueued;

                // Create the monitor response
                var response = new
                {
                    id = id,
                    name = scraperName,
                    isRunning = isRunning || (dbStatus?.IsRunning ?? false),
                    status = isRunning ? "Running" : (dbStatus?.IsRunning ?? false ? "Running" : "Idle"),
                    lastUpdate = DateTime.Now,
                    statistics = stats,
                    metrics = processedMetrics,
                    errors = errors,
                    recentLogs = recentLogs,  // Format exactly matching the example response
                    hasErrors = errors.Count > 0,
                    databaseStatus = dbStatus != null ? new
                    {
                        isRunning = dbStatus.IsRunning,
                        startTime = dbStatus.StartTime,
                        endTime = dbStatus.EndTime,
                        urlsProcessed = dbStatus.UrlsProcessed,
                        urlsQueued = dbStatus.UrlsQueued,
                        message = dbStatus.Message,
                        elapsedTime = dbStatus.ElapsedTime,
                        lastStatusUpdate = dbStatus.LastStatusUpdate,
                        lastUpdate = dbStatus.LastUpdate
                    } : null,
                    summary = new
                    {
                        pagesProcessed = pagesProcessed,
                        documentsProcessed = documentsProcessed,
                        urlsQueued = urlsQueued,
                        errorCount = errors.Count,
                        runningTime = dbStatus?.StartTime != null ? (DateTime.Now - dbStatus.StartTime.Value).ToString(@"hh\:mm\:ss") : "00:00:00",
                        currentStatus = dbStatus?.Message ?? (isRunning ? "Running" : "Idle")
                    },
                    // Include the logs array in the format shown in the example
                    logs = dbLogs.Select(log => new
                    {
                        timestamp = log.Timestamp,
                        level = log.LogLevel?.ToLower() ?? "info",
                        message = log.Message
                    }).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitor data for scraper {ScraperId}", id);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while getting monitor data for scraper {id}",
                    Error = ex.Message
                });
            }
        }

        // Helper method to ensure important counters are included in stats
        private void EnsureCounterIncluded(Dictionary<string, object> stats, object core, string propertyName, string alternateName)
        {
            try
            {
                if (!stats.ContainsKey(propertyName) && !stats.ContainsKey(alternateName))
                {
                    var prop = core.GetType().GetProperty(propertyName);
                    if (prop != null && prop.CanRead)
                    {
                        var value = prop.GetValue(core);
                        if (value != null)
                        {
                            stats[propertyName] = value;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently fail if we can't access the property
            }
        }
    }
}