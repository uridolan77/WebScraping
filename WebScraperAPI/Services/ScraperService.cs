using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Models;
using WebScraperApi.Services.Execution;

namespace WebScraperApi.Services
{
    public class ScraperService : IScraperService
    {
        private readonly ScraperConfigService _configService;
        private readonly IScraperExecutionService _executionService;
        private readonly ScraperRunService _runService;
        private readonly ContentChangeService _contentChangeService;
        private readonly DocumentService _documentService;
        private readonly MetricsService _metricsService;
        private readonly ILogger<ScraperService> _logger;

        public ScraperService(
            ScraperConfigService configService,
            IScraperExecutionService executionService,
            ScraperRunService runService,
            ContentChangeService contentChangeService,
            DocumentService documentService,
            MetricsService metricsService,
            ILogger<ScraperService> logger)
        {
            _configService = configService ?? throw new ArgumentNullException(nameof(configService));
            _executionService = executionService ?? throw new ArgumentNullException(nameof(executionService));
            _runService = runService ?? throw new ArgumentNullException(nameof(runService));
            _contentChangeService = contentChangeService ?? throw new ArgumentNullException(nameof(contentChangeService));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _metricsService = metricsService ?? throw new ArgumentNullException(nameof(metricsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Scraper Configuration Operations

        public async Task<List<ScraperConfigModel>> GetAllScrapersAsync()
        {
            return await _configService.GetAllScrapersAsync();
        }

        public async Task<ScraperConfigModel> GetScraperByIdAsync(string id)
        {
            return await _configService.GetScraperByIdAsync(id);
        }

        public async Task<ScraperConfigModel> CreateScraperAsync(ScraperConfigModel scraper)
        {
            return await _configService.CreateScraperAsync(scraper);
        }

        public async Task<ScraperConfigModel> UpdateScraperAsync(string id, ScraperConfigModel scraper)
        {
            return await _configService.UpdateScraperAsync(id, scraper);
        }

        public async Task<bool> DeleteScraperAsync(string id)
        {
            return await _configService.DeleteScraperAsync(id);
        }

        #endregion

        #region Scraper Execution Operations

        public async Task<bool> StartScraperAsync(string id)
        {
            // Get the scraper configuration
            var config = await _configService.GetScraperByIdAsync(id);
            if (config == null)
            {
                _logger.LogWarning("Cannot start scraper: scraper {ScraperId} not found", id);
                return false;
            }

            // Create a new scraper state
            var scraperState = new ScraperState { Id = id, Status = "Running" };

            // Start the scraper with the configuration and state
            return await _executionService.StartScraperAsync(
                config,
                scraperState,
                message => _logger.LogInformation("Scraper {ScraperId}: {Message}", id, message));
        }

        public async Task<bool> StopScraperAsync(string id)
        {
            return await _executionService.StopScraperAsync(id);
        }

        public async Task<ScraperStatus> GetScraperStatusAsync(string id)
        {
            return await _executionService.GetScraperStatusAsync(id);
        }

        #endregion

        #region Scraper Run Operations

        public async Task<List<ScraperRun>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            return await _runService.GetScraperRunsAsync(scraperId, limit);
        }

        public async Task<ScraperRun> GetScraperRunByIdAsync(string runId)
        {
            return await _runService.GetScraperRunByIdAsync(runId);
        }

        #endregion

        #region Content Change Operations

        public async Task<List<ContentChangeRecord>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            return await _contentChangeService.GetContentChangesAsync(scraperId, limit);
        }

        #endregion

        #region Document Operations

        public async Task<List<ProcessedDocument>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            return await _documentService.GetProcessedDocumentsAsync(scraperId, limit);
        }

        public async Task<ProcessedDocument> GetDocumentByIdAsync(string documentId)
        {
            return await _documentService.GetDocumentByIdAsync(documentId);
        }

        #endregion

        #region Metrics Operations

        public async Task<Dictionary<string, List<KeyValuePair<DateTime, double>>>> GetScraperMetricsAsync(
            string scraperId,
            DateTime from,
            DateTime to,
            string[] metricNames = null)
        {
            return await _metricsService.GetScraperMetricsAsync(scraperId, from, to, metricNames);
        }

        #endregion

        #region Monitoring Operations

        public async Task<ScraperMonitorData> GetScraperMonitorDataAsync(string id)
        {
            try
            {
                // Get the current status
                var status = await GetScraperStatusAsync(id);
                if (status == null || !status.IsRunning)
                {
                    return null;
                }

                // Get the scraper configuration
                var config = await GetScraperByIdAsync(id);
                if (config == null)
                {
                    return null;
                }

                // Calculate metrics
                var elapsedTime = status.StartTime.HasValue ?
                    DateTime.Now - status.StartTime.Value : TimeSpan.Zero;

                double requestsPerSecond = 0;
                if (elapsedTime.TotalSeconds > 0)
                {
                    requestsPerSecond = Math.Round(status.UrlsProcessed / elapsedTime.TotalSeconds, 2);
                }

                // Create monitor data
                return new ScraperMonitorData
                {
                    CurrentUrl = $"{config.BaseUrl}/page-{status.UrlsProcessed + 1}",
                    PercentComplete = CalculatePercentComplete(config, status),
                    EstimatedTimeRemaining = CalculateEstimatedTimeRemaining(config, status),
                    CurrentDepth = Math.Min((status.UrlsProcessed / 10) + 1, config.MaxDepth),
                    RequestsPerSecond = requestsPerSecond,
                    MemoryUsage = "128 MB", // This would come from real metrics in a production system
                    CpuUsage = "15%",       // This would come from real metrics in a production system
                    ActiveThreads = config.MaxConcurrentRequests,
                    RecentActivity = new List<object>() // This would come from a real activity log in a production system
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting monitor data for scraper {ScraperId}", id);
                return null;
            }
        }

        private int CalculatePercentComplete(ScraperConfigModel config, ScraperStatus status)
        {
            if (!status.IsRunning || config.MaxPages <= 0)
                return 0;

            int percent = (int)Math.Min(100, (status.UrlsProcessed * 100.0) / config.MaxPages);
            return percent;
        }

        private string CalculateEstimatedTimeRemaining(ScraperConfigModel config, ScraperStatus status)
        {
            if (!status.IsRunning || !status.StartTime.HasValue || status.UrlsProcessed <= 0 || config.MaxPages <= 0)
                return "Unknown";

            TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
            if (elapsed.TotalSeconds <= 0)
                return "Unknown";

            double pagesPerSecond = status.UrlsProcessed / elapsed.TotalSeconds;
            if (pagesPerSecond <= 0)
                return "Unknown";

            int pagesRemaining = Math.Max(0, config.MaxPages - status.UrlsProcessed);
            double secondsRemaining = pagesRemaining / pagesPerSecond;

            TimeSpan remaining = TimeSpan.FromSeconds(secondsRemaining);
            return remaining.ToString(@"hh\:mm\:ss");
        }

        #endregion
    }
}
