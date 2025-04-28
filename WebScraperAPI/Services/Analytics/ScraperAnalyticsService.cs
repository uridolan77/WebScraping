using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Monitoring;
using WebScraperApi.Models;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services.Analytics
{
    /// <summary>
    /// Service for scraper analytics operations
    /// </summary>
    public class ScraperAnalyticsService : IScraperAnalyticsService
    {
        private readonly ILogger<ScraperAnalyticsService> _logger;
        private readonly IScraperStateService _stateService;

        public ScraperAnalyticsService(
            ILogger<ScraperAnalyticsService> logger,
            IScraperStateService stateService)
        {
            _logger = logger;
            _stateService = stateService;
        }

        /// <summary>
        /// Gets analytics data for a specific scraper
        /// </summary>
        public async Task<Dictionary<string, object>> GetScraperAnalyticsAsync(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get analytics for scraper {id}: not found");
                return new Dictionary<string, object>
                {
                    ["error"] = "Scraper not found"
                };
            }

            var result = new Dictionary<string, object>
            {
                ["id"] = id,
                ["name"] = instance.Config.Name,
                ["totalRuns"] = instance.Config.RunCount,
                ["lastRun"] = instance.Config.LastRun,
                ["isRunning"] = instance.Status.IsRunning,
                ["startTime"] = instance.Status.StartTime,
                ["endTime"] = instance.Status.EndTime,
                ["urlsProcessed"] = instance.Status.UrlsProcessed,
                ["urlsQueued"] = instance.Status.UrlsQueued,
                ["documentsProcessed"] = instance.Status.DocumentsProcessed,
                ["hasErrors"] = instance.Status.HasErrors
            };

            // If we have a state manager, get additional analytics
            if (instance.StateManager != null)
            {
                try
                {
                    var stateAnalytics = await _stateService.GetStateManagerAnalyticsAsync(instance.StateManager);

                    foreach (var item in stateAnalytics)
                    {
                        result[$"state_{item.Key}"] = item.Value;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting state analytics for scraper {id}");
                    result["stateError"] = ex.Message;
                }
            }

            // If we have metrics, add those too
            if (instance.Metrics != null)
            {
                try
                {
                    result["metrics_avgResponseTime"] = instance.Metrics.AverageResponseTimeMs;
                    result["metrics_totalRequests"] = instance.Metrics.TotalRequests;
                    result["metrics_successfulRequests"] = instance.Metrics.SuccessfulRequests;
                    result["metrics_failedRequests"] = instance.Metrics.FailedRequests;
                    result["metrics_contentChangesDetected"] = instance.Metrics.ContentChangesDetected;
                    result["metrics_documentsCrawled"] = instance.Metrics.DocumentsCrawled;
                    result["metrics_documentsProcessed"] = instance.Metrics.DocumentsProcessed;
                    result["metrics_totalCrawlTimeMs"] = instance.Metrics.TotalCrawlTimeMs;

                    if (instance.Metrics.RateLimitsEncountered > 0)
                    {
                        result["metrics_rateLimitsEncountered"] = instance.Metrics.RateLimitsEncountered;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error getting metrics for scraper {id}");
                    result["metricsError"] = ex.Message;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets detected content changes for a specific scraper
        /// </summary>
        public async Task<object> GetDetectedChangesAsync(string id, DateTime? since = null, int limit = 100)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get detected changes for scraper {id}: not found");
                return new { error = "Scraper not found" };
            }

            try
            {
                var changes = await _stateService.GetDetectedChangesAsync(id, since, limit);

                return new
                {
                    scraperId = id,
                    scraperName = instance.Config.Name,
                    totalChanges = changes.Count,
                    since = since ?? DateTime.MinValue,
                    limit = limit,
                    changes = changes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting detected changes for scraper {id}");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets processed documents for a specific scraper
        /// </summary>
        public async Task<object> GetProcessedDocumentsAsync(string id, string documentType = null, int page = 1, int pageSize = 20)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get processed documents for scraper {id}: not found");
                return new { error = "Scraper not found" };
            }

            try
            {
                var documentsResult = await _stateService.GetProcessedDocumentsAsync(id, documentType, page, pageSize);

                return new
                {
                    scraperId = id,
                    scraperName = instance.Config.Name,
                    page = page,
                    pageSize = pageSize,
                    documentType = documentType ?? "all",
                    totalCount = documentsResult.TotalCount,
                    documents = documentsResult.Documents
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting processed documents for scraper {id}");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets detailed telemetry metrics for a specific scraper
        /// </summary>
        public async Task<object> GetScraperMetricsAsync(string id)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get metrics for scraper {id}: not found");
                return new { error = "Scraper not found" };
            }

            if (instance.Metrics == null)
            {
                return new
                {
                    scraperId = id,
                    scraperName = instance.Config.Name,
                    message = "No metrics available for this scraper"
                };
            }

            try
            {
                WebScraperApi.Models.ScraperMetrics metrics = instance.Metrics;

                return new
                {
                    scraperId = id,
                    scraperName = instance.Config.Name,
                    averageResponseTimeMs = metrics.AverageResponseTimeMs,
                    totalRequests = metrics.TotalRequests,
                    successfulRequests = metrics.SuccessfulRequests,
                    failedRequests = metrics.FailedRequests,
                    contentChangesDetected = metrics.ContentChangesDetected,
                    documentsCrawled = metrics.DocumentsCrawled,
                    documentsProcessed = metrics.DocumentsProcessed,
                    totalCrawlTimeMs = metrics.TotalCrawlTimeMs,
                    rateLimitsEncountered = metrics.RateLimitsEncountered,
                    lastMetricsUpdate = metrics.LastMetricsUpdate
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting metrics for scraper {id}");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets a summary of analytics across all scrapers
        /// </summary>
        public async Task<object> GetAnalyticsSummaryAsync()
        {
            try
            {
                var scrapers = _stateService.GetAllScraperInstances();

                int totalScrapers = scrapers.Count;
                int activeScrapers = 0;
                int scrapersWithErrors = 0;
                int totalUrlsProcessed = 0;
                int totalDocumentsProcessed = 0;
                int totalContentChanges = 0;

                // Aggregate metrics
                foreach (var scraper in scrapers)
                {
                    if (scraper.Status?.IsRunning == true)
                    {
                        activeScrapers++;
                    }

                    if (scraper.Status?.HasErrors == true)
                    {
                        scrapersWithErrors++;
                    }

                    totalUrlsProcessed += scraper.Status?.UrlsProcessed ?? 0;
                    totalDocumentsProcessed += scraper.Status?.DocumentsProcessed ?? 0;
                    totalContentChanges += scraper.Metrics?.ContentChangesDetected ?? 0;
                }

                // Get the most recently updated scrapers
                var recentScrapers = scrapers
                    .Where(s => s.Status != null)
                    .OrderByDescending(s => s.Status.LastUpdate)
                    .Take(5)
                    .Select(s => new
                    {
                        id = s.Config?.Id ?? "",
                        name = s.Config?.Name ?? "",
                        lastUpdate = s.Status?.LastUpdate,
                        isRunning = s.Status?.IsRunning ?? false,
                        urlsProcessed = s.Status?.UrlsProcessed ?? 0
                    })
                    .ToList();

                return new
                {
                    totalScrapers,
                    activeScrapers,
                    scrapersWithErrors,
                    totalUrlsProcessed,
                    totalDocumentsProcessed,
                    totalContentChanges,
                    recentScrapers
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting analytics summary");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets performance metrics for a specific scraper within a date range
        /// </summary>
        public async Task<object> GetScraperPerformanceAsync(string id, DateTime? start = null, DateTime? end = null)
        {
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                _logger.LogWarning($"Cannot get performance metrics for scraper {id}: not found");
                return new { error = "Scraper not found" };
            }

            try
            {
                // Default to last 7 days if not specified
                if (!start.HasValue)
                {
                    start = DateTime.Now.AddDays(-7);
                }

                if (!end.HasValue)
                {
                    end = DateTime.Now;
                }

                // Get performance data from state service
                var performanceData = await _stateService.GetScraperPerformanceAsync(id, start.Value, end.Value);

                return new
                {
                    scraperId = id,
                    scraperName = instance.Config.Name,
                    startDate = start.Value,
                    endDate = end.Value,
                    performanceData
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting performance metrics for scraper {id}");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets the most popular domains being scraped
        /// </summary>
        public async Task<IEnumerable<object>> GetPopularDomainsAsync(int count = 10)
        {
            try
            {
                var scrapers = _stateService.GetAllScraperInstances();

                // Group by domain and count
                var domains = scrapers
                    .GroupBy(s => new Uri(s.Config.BaseUrl).Host)
                    .Select(g => new
                    {
                        domain = g.Key,
                        scraperCount = g.Count(),
                        totalUrlsProcessed = g.Sum(s => s.Status.UrlsProcessed),
                        totalDocumentsProcessed = g.Sum(s => s.Status.DocumentsProcessed),
                        totalContentChanges = g.Sum(s => s.Metrics?.ContentChangesDetected ?? 0)
                    })
                    .OrderByDescending(d => d.scraperCount)
                    .ThenByDescending(d => d.totalUrlsProcessed)
                    .Take(count)
                    .ToList();

                return domains;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting popular domains");
                return new[] { new { error = ex.Message } };
            }
        }

        /// <summary>
        /// Gets the frequency of content changes across all scrapers
        /// </summary>
        public async Task<object> GetContentChangeFrequencyAsync(DateTime? since = null)
        {
            try
            {
                // Default to last 30 days if not specified
                if (!since.HasValue)
                {
                    since = DateTime.Now.AddDays(-30);
                }

                var scrapers = _stateService.GetAllScraperInstances();

                // Get all content changes
                var allChanges = new List<object>();
                foreach (var scraper in scrapers)
                {
                    try
                    {
                        var changes = await _stateService.GetDetectedChangesAsync(scraper.Id, since, 1000);
                        foreach (var change in changes)
                        {
                            allChanges.Add(new
                            {
                                scraperId = scraper.Id,
                                scraperName = scraper.Config.Name,
                                changeTimestamp = change.GetType().GetProperty("Timestamp")?.GetValue(change),
                                url = change.GetType().GetProperty("Url")?.GetValue(change),
                                changeType = change.GetType().GetProperty("ChangeType")?.GetValue(change)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error getting changes for scraper {scraper.Id}");
                    }
                }

                // Group by day
                var changesByDay = allChanges
                    .GroupBy(c => ((DateTime?)c.GetType().GetProperty("changeTimestamp")?.GetValue(c))?.Date ?? DateTime.MinValue)
                    .Select(g => new
                    {
                        date = g.Key,
                        count = g.Count()
                    })
                    .OrderBy(g => g.date)
                    .ToList();

                return new
                {
                    since = since.Value,
                    totalChanges = allChanges.Count,
                    changesByDay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content change frequency");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets usage statistics within a date range
        /// </summary>
        public async Task<object> GetUsageStatisticsAsync(DateTime start, DateTime end)
        {
            try
            {
                var scrapers = _stateService.GetAllScraperInstances();

                // Filter runs within the date range
                var runsInRange = scrapers
                    .SelectMany(s => s.Runs ?? new List<ScraperRun>())
                    .Where(r => r.StartTime >= start && r.StartTime <= end)
                    .ToList();

                // Group by day
                var runsByDay = runsInRange
                    .GroupBy(r => r.StartTime.Date)
                    .Select(g => new
                    {
                        date = g.Key,
                        runCount = g.Count(),
                        totalUrlsProcessed = g.Sum(r => r.UrlsProcessed),
                        totalDocumentsProcessed = g.Sum(r => r.DocumentsProcessed),
                        averageDuration = g.Average(r => r.Duration.HasValue ? r.Duration.Value.TotalMinutes : 0)
                    })
                    .OrderBy(g => g.date)
                    .ToList();

                return new
                {
                    startDate = start,
                    endDate = end,
                    totalRuns = runsInRange.Count,
                    totalUrlsProcessed = runsInRange.Sum(r => r.UrlsProcessed),
                    totalDocumentsProcessed = runsInRange.Sum(r => r.DocumentsProcessed),
                    averageDuration = runsInRange.Count > 0 ? runsInRange.Average(r => r.Duration.HasValue ? r.Duration.Value.TotalMinutes : 0) : 0,
                    runsByDay
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting usage statistics");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Gets the distribution of errors across all scrapers
        /// </summary>
        public async Task<object> GetErrorDistributionAsync(DateTime? since = null)
        {
            try
            {
                // Default to last 30 days if not specified
                if (!since.HasValue)
                {
                    since = DateTime.Now.AddDays(-30);
                }

                var scrapers = _stateService.GetAllScraperInstances();

                // Get all errors
                var allErrors = new List<object>();
                foreach (var scraper in scrapers)
                {
                    try
                    {
                        var errors = scraper.LogMessages
                            .Where(l => l.Timestamp >= since && l.Level == "Error")
                            .Select(l => new
                            {
                                scraperId = scraper.Id,
                                scraperName = scraper.Config.Name,
                                timestamp = l.Timestamp,
                                message = l.Message
                            })
                            .ToList();

                        allErrors.AddRange(errors);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, $"Error getting errors for scraper {scraper.Id}");
                    }
                }

                // Group by error type
                var errorsByType = allErrors
                    .GroupBy(e => GetErrorType(e.GetType().GetProperty("message")?.GetValue(e)?.ToString() ?? ""))
                    .Select(g => new
                    {
                        errorType = g.Key,
                        count = g.Count(),
                        examples = g.Take(3).Select(e => e.GetType().GetProperty("message")?.GetValue(e)?.ToString()).ToList()
                    })
                    .OrderByDescending(g => g.count)
                    .ToList();

                return new
                {
                    since = since.Value,
                    totalErrors = allErrors.Count,
                    errorsByType
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting error distribution");
                return new { error = ex.Message };
            }
        }

        /// <summary>
        /// Helper method to categorize errors
        /// </summary>
        private string GetErrorType(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage))
            {
                return "Unknown";
            }

            if (errorMessage.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            {
                return "Timeout";
            }

            if (errorMessage.Contains("404") || errorMessage.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return "Not Found";
            }

            if (errorMessage.Contains("403") || errorMessage.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            {
                return "Access Denied";
            }

            if (errorMessage.Contains("429") || errorMessage.Contains("rate limit", StringComparison.OrdinalIgnoreCase))
            {
                return "Rate Limited";
            }

            if (errorMessage.Contains("500") || errorMessage.Contains("server error", StringComparison.OrdinalIgnoreCase))
            {
                return "Server Error";
            }

            if (errorMessage.Contains("connection", StringComparison.OrdinalIgnoreCase))
            {
                return "Connection Error";
            }

            return "Other";
        }
    }
}