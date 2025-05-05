using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;
using WebScraper;
using WebScraper.Monitoring;

namespace WebScraperApi.Services.Monitoring
{
    /// <summary>
    /// Service for managing scraper metrics
    /// </summary>
    public class ScraperMetricsService : IScraperMetricsService
    {
        private readonly IScraperRepository _scraperRepository;
        private readonly ILogger<ScraperMetricsService> _logger;

        public ScraperMetricsService(IScraperRepository scraperRepository, ILogger<ScraperMetricsService> logger)
        {
            _scraperRepository = scraperRepository ?? throw new ArgumentNullException(nameof(scraperRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Updates metrics for a running scraper
        /// </summary>
        public async Task UpdateScraperMetricsFromRuntimeAsync(string id, Scraper scraper)
        {
            try
            {
                _logger.LogInformation($"Updating metrics for scraper {id}");

                // Get scraper name and config
                string scraperName = "Unknown";
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig != null)
                {
                    scraperName = scraperConfig.Name;
                }

                // Try to find the current run ID for this scraper
                string currentRunId = null;

                // First check if we can get the run ID from the scraper object
                if (scraper != null)
                {
                    // Check if we have the run ID stored in the scraper object (using reflection)
                    var runIdField = typeof(Scraper).GetField("_runId", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (runIdField != null)
                    {
                        currentRunId = runIdField.GetValue(scraper) as string;
                    }
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

                // Check if the scraper is running in a separate process
                bool isRunningInSeparateProcess = false;

                // If scraper is null or we can't access its core, it might be running in a separate process
                if (scraper == null)
                {
                    isRunningInSeparateProcess = true;
                    _logger.LogInformation("Scraper object is null, assuming it's running in a separate process");
                }
                else
                {
                    // Try to get the ScraperCore instance using reflection
                    var coreField = typeof(Scraper).GetField("_core", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (coreField == null)
                    {
                        isRunningInSeparateProcess = true;
                        _logger.LogInformation("Could not access scraper core field, assuming it's running in a separate process");
                    }
                    else
                    {
                        var core = coreField.GetValue(scraper);
                        if (core == null)
                        {
                            isRunningInSeparateProcess = true;
                            _logger.LogInformation("Scraper core is null, assuming it's running in a separate process");
                        }
                        else
                        {
                            // Try to get the metrics object using reflection
                            var metricsProperty = core.GetType().GetProperty("Metrics");
                            if (metricsProperty == null)
                            {
                                isRunningInSeparateProcess = true;
                                _logger.LogInformation("Could not find Metrics property, assuming it's running in a separate process");
                            }
                        }
                    }
                }

                if (isRunningInSeparateProcess)
                {
                    // For scrapers running in separate processes, get metrics from the database
                    await UpdateMetricsFromDatabaseAsync(id, scraperName, currentRunId);
                }
                else
                {
                    // For scrapers running in the same process, get metrics directly
                    await UpdateMetricsFromScraperObjectAsync(id, scraper, scraperName, currentRunId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metrics for scraper {id}");
            }
        }

        /// <summary>
        /// Updates metrics for a scraper running in the same process
        /// </summary>
        private async Task UpdateMetricsFromScraperObjectAsync(string id, Scraper scraper, string scraperName, string currentRunId)
        {
            try
            {
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

                // Log the metrics to console so we can see their values
                Console.WriteLine($"DIRECT-CONSOLE: PagesProcessed={metrics.PagesProcessed}, DocumentsProcessed={metrics.DocumentsProcessed}");

                // Update key metrics in the database
                await UpdateMetricAsync(id, scraperName, "PagesProcessed", metrics.PagesProcessed, currentRunId);
                await UpdateMetricAsync(id, scraperName, "ProcessedUrls", metrics.ProcessedUrls, currentRunId);
                await UpdateMetricAsync(id, scraperName, "SuccessfulUrls", metrics.SuccessfulUrls, currentRunId);
                await UpdateMetricAsync(id, scraperName, "FailedUrls", metrics.FailedUrls, currentRunId);
                await UpdateMetricAsync(id, scraperName, "DocumentsProcessed", metrics.DocumentsProcessed, currentRunId);
                await UpdateMetricAsync(id, scraperName, "TotalLinksExtracted", metrics.TotalLinksExtracted, currentRunId);
                await UpdateMetricAsync(id, scraperName, "ContentItemsExtracted", metrics.ContentItemsExtracted, currentRunId);
                await UpdateMetricAsync(id, scraperName, "TotalBytesDownloaded", metrics.TotalBytesDownloaded, currentRunId);
                await UpdateMetricAsync(id, scraperName, "ClientErrors", metrics.ClientErrors, currentRunId);
                await UpdateMetricAsync(id, scraperName, "ServerErrors", metrics.ServerErrors, currentRunId);
                await UpdateMetricAsync(id, scraperName, "AveragePageProcessingTimeMs", metrics.AveragePageProcessingTimeMs, currentRunId);
                await UpdateMetricAsync(id, scraperName, "AverageLinksPerPage", metrics.AverageLinksPerPage, currentRunId);
                await UpdateMetricAsync(id, scraperName, "CurrentMemoryUsageMB", metrics.CurrentMemoryUsageMB, currentRunId);
                await UpdateMetricAsync(id, scraperName, "PeakMemoryUsageMB", metrics.PeakMemoryUsageMB, currentRunId);

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
                await UpdateRunWithMetricsAsync(id, currentRunId, (int)metrics.PagesProcessed, (int)metrics.DocumentsProcessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metrics from scraper object for {id}");
            }
        }

        /// <summary>
        /// Updates metrics for a scraper running in a separate process by querying the database
        /// </summary>
        private async Task UpdateMetricsFromDatabaseAsync(string id, string scraperName, string currentRunId)
        {
            try
            {
                _logger.LogInformation($"Getting metrics from database for scraper {id} running in separate process");

                // Get the current status from the database
                var status = await _scraperRepository.GetScraperStatusAsync(id);
                if (status == null)
                {
                    _logger.LogWarning($"No status found in database for scraper {id}");
                    return;
                }

                // Log the metrics to console so we can see their values
                Console.WriteLine($"DB-METRICS: UrlsProcessed={status.UrlsProcessed}, DocumentsProcessed={status.DocumentsProcessed}, UrlsQueued={status.UrlsQueued}");

                // Update key metrics in the database based on the status
                await UpdateMetricAsync(id, scraperName, "PagesProcessed", status.UrlsProcessed, currentRunId);
                await UpdateMetricAsync(id, scraperName, "ProcessedUrls", status.UrlsProcessed, currentRunId);
                await UpdateMetricAsync(id, scraperName, "DocumentsProcessed", status.DocumentsProcessed, currentRunId);

                // Get additional metrics from the database if available
                var recentMetrics = await _scraperRepository.GetScraperMetricsAsync(id);
                if (recentMetrics != null && recentMetrics.Count > 0)
                {
                    // Group metrics by name and get the most recent value for each metric
                    var latestMetrics = recentMetrics
                        .GroupBy(m => m.MetricName)
                        .Select(g => g.OrderByDescending(m => m.Timestamp).First())
                        .ToList();

                    foreach (var metric in latestMetrics)
                    {
                        // Skip metrics we've already updated
                        if (metric.MetricName == "PagesProcessed" || metric.MetricName == "ProcessedUrls" || metric.MetricName == "DocumentsProcessed")
                            continue;

                        await UpdateMetricAsync(id, scraperName, metric.MetricName, metric.MetricValue, currentRunId);
                    }
                }

                // If we have a run ID, update the run with summary metrics
                await UpdateRunWithMetricsAsync(id, currentRunId, status.UrlsProcessed, status.DocumentsProcessed);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metrics from database for {id}");
            }
        }

        /// <summary>
        /// Updates a run with summary metrics
        /// </summary>
        private async Task UpdateRunWithMetricsAsync(string scraperId, string runId, int urlsProcessed, int documentsProcessed)
        {
            if (string.IsNullOrEmpty(runId))
                return;

            try
            {
                // Log the operation for debugging
                Console.WriteLine($"UpdateRunWithMetricsAsync: Updating run {runId} with metrics: UrlsProcessed={urlsProcessed}, DocumentsProcessed={documentsProcessed}");

                // Get the run with AsNoTracking to avoid entity tracking conflicts
                var run = await _scraperRepository.GetScraperRunByIdAsync(runId);
                if (run != null)
                {
                    // Create a new instance to avoid tracking conflicts
                    var updatedRun = new WebScraperApi.Data.Entities.ScraperRunEntity
                    {
                        Id = run.Id,
                        ScraperId = run.ScraperId,
                        StartTime = run.StartTime,
                        EndTime = run.EndTime,
                        UrlsProcessed = urlsProcessed,
                        DocumentsProcessed = documentsProcessed,
                        Successful = run.Successful,
                        ErrorMessage = run.ErrorMessage ?? string.Empty
                    };

                    // Get the current status to update the elapsed time
                    var currentStatus = await _scraperRepository.GetScraperStatusAsync(scraperId);
                    if (currentStatus != null)
                    {
                        updatedRun.ElapsedTime = currentStatus.ElapsedTime ?? string.Empty;
                    }
                    else
                    {
                        updatedRun.ElapsedTime = run.ElapsedTime ?? string.Empty;
                    }

                    // Update the run with the new instance
                    await _scraperRepository.UpdateScraperRunAsync(updatedRun);
                    _logger.LogInformation($"Updated run {runId} with latest metrics");
                    Console.WriteLine($"UpdateRunWithMetricsAsync: Successfully updated run {runId}");
                }
                else
                {
                    _logger.LogWarning($"Run {runId} not found for metrics update");
                    Console.WriteLine($"UpdateRunWithMetricsAsync: Run {runId} not found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating run {runId} with metrics summary");
                Console.WriteLine($"UpdateRunWithMetricsAsync ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        /// <summary>
        /// Updates a single metric for a scraper
        /// </summary>
        public async Task UpdateMetricAsync(string scraperId, string scraperName, string metricName, double metricValue, string runId = null)
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
                            // Create a new instance to avoid tracking conflicts
                            var updatedRun = new WebScraperApi.Data.Entities.ScraperRunEntity
                            {
                                Id = run.Id,
                                ScraperId = run.ScraperId,
                                StartTime = run.StartTime,
                                EndTime = run.EndTime,
                                UrlsProcessed = run.UrlsProcessed,
                                DocumentsProcessed = run.DocumentsProcessed,
                                Successful = run.Successful,
                                ErrorMessage = run.ErrorMessage ?? string.Empty,
                                ElapsedTime = run.ElapsedTime ?? string.Empty
                            };

                            // Update run metrics based on metric name
                            switch (metricName)
                            {
                                case "PagesProcessed":
                                case "ProcessedUrls":
                                    updatedRun.UrlsProcessed = (int)metricValue;
                                    break;
                                case "DocumentsProcessed":
                                    updatedRun.DocumentsProcessed = (int)metricValue;
                                    break;
                            }

                            await _scraperRepository.UpdateScraperRunAsync(updatedRun);
                            _logger.LogInformation($"Updated run {runId} with metric {metricName}={metricValue}");
                            Console.WriteLine($"UpdateMetricAsync: Successfully updated run {runId} with {metricName}={metricValue}");
                        }
                        else
                        {
                            _logger.LogWarning($"Run {runId} not found for metric update: {metricName}");
                            Console.WriteLine($"UpdateMetricAsync: Run {runId} not found for metric update: {metricName}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error updating run {runId} with metric {metricName}");
                        Console.WriteLine($"UpdateMetricAsync ERROR: {ex.Message}");
                        if (ex.InnerException != null)
                        {
                            Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating metric {metricName} for scraper {scraperId}");
                Console.WriteLine($"DIRECT-METRIC-CRITICAL-ERROR: Failed to update {metricName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates metrics for all active scrapers
        /// </summary>
        public async Task UpdateMetricsForAllScrapersAsync(Dictionary<string, Scraper> activeScrapers)
        {
            if (activeScrapers == null || activeScrapers.Count == 0)
            {
                Console.WriteLine("METRICS-TIMER: No active scrapers found");
                return;
            }

            Console.WriteLine($"METRICS-TIMER: Processing {activeScrapers.Count} active scrapers");

            foreach (var kvp in activeScrapers)
            {
                string scraperId = kvp.Key;
                WebScraper.Scraper scraper = kvp.Value;

                _logger.LogInformation($"Updating metrics for active scraper {scraperId}");
                Console.WriteLine($"METRICS-TIMER: Updating metrics for {scraperId}");

                await UpdateScraperMetricsFromRuntimeAsync(scraperId, scraper);
            }

            _logger.LogInformation("Completed periodic metrics reporting");
            Console.WriteLine("METRICS-TIMER: Completed metrics update");
        }
    }
}
