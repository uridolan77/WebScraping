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
                ScraperMetrics metrics = instance.Metrics;
                
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
    }
}