using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    /// <summary>
    /// Facade implementation that delegates to specialized repositories while still supporting the original interface
    /// </summary>
    public class ScraperRepositoryFacade : IScraperRepository
    {
        private readonly IScraperConfigRepository _scraperConfigRepository;
        private readonly IScraperStatusRepository _scraperStatusRepository;
        private readonly IScraperRunRepository _scraperRunRepository;
        private readonly IScrapedPageRepository _scrapedPageRepository;
        private readonly IMetricsRepository _metricsRepository;

        public ScraperRepositoryFacade(
            IScraperConfigRepository scraperConfigRepository,
            IScraperStatusRepository scraperStatusRepository,
            IScraperRunRepository scraperRunRepository,
            IScrapedPageRepository scrapedPageRepository,
            IMetricsRepository metricsRepository)
        {
            _scraperConfigRepository = scraperConfigRepository ?? throw new ArgumentNullException(nameof(scraperConfigRepository));
            _scraperStatusRepository = scraperStatusRepository ?? throw new ArgumentNullException(nameof(scraperStatusRepository));
            _scraperRunRepository = scraperRunRepository ?? throw new ArgumentNullException(nameof(scraperRunRepository));
            _scrapedPageRepository = scrapedPageRepository ?? throw new ArgumentNullException(nameof(scrapedPageRepository));
            _metricsRepository = metricsRepository ?? throw new ArgumentNullException(nameof(metricsRepository));
        }

        // Database operations
        public bool TestDatabaseConnection() => _scraperConfigRepository.TestDatabaseConnection();

        // Scraper Config operations
        public Task<List<ScraperConfigEntity>> GetAllScrapersAsync() => _scraperConfigRepository.GetAllScrapersAsync();
        public Task<ScraperConfigEntity> GetScraperByIdAsync(string id) => _scraperConfigRepository.GetScraperByIdAsync(id);
        public Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper) => _scraperConfigRepository.CreateScraperAsync(scraper);
        public Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper) => _scraperConfigRepository.UpdateScraperAsync(scraper);
        public Task<bool> DeleteScraperAsync(string id) => _scraperConfigRepository.DeleteScraperAsync(id);

        // Related entity operations
        public Task<List<ScraperStartUrlEntity>> GetScraperStartUrlsAsync(string scraperId) => _scraperConfigRepository.GetScraperStartUrlsAsync(scraperId);
        public Task<List<ContentExtractorSelectorEntity>> GetContentExtractorSelectorsAsync(string scraperId) => _scraperConfigRepository.GetContentExtractorSelectorsAsync(scraperId);
        public Task<List<KeywordAlertEntity>> GetKeywordAlertsAsync(string scraperId) => _scraperConfigRepository.GetKeywordAlertsAsync(scraperId);
        public Task<List<DomainRateLimitEntity>> GetDomainRateLimitsAsync(string scraperId) => _scraperConfigRepository.GetDomainRateLimitsAsync(scraperId);
        public Task<List<ProxyConfigurationEntity>> GetProxyConfigurationsAsync(string scraperId) => _scraperConfigRepository.GetProxyConfigurationsAsync(scraperId);
        public Task<List<WebhookTriggerEntity>> GetWebhookTriggersAsync(string scraperId) => _scraperConfigRepository.GetWebhookTriggersAsync(scraperId);
        public Task<List<ScraperScheduleEntity>> GetScraperSchedulesAsync(string scraperId) => _scraperConfigRepository.GetScraperSchedulesAsync(scraperId);

        // Scraper Status operations
        public Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId) => _scraperStatusRepository.GetScraperStatusAsync(scraperId);
        public Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status) => _scraperStatusRepository.UpdateScraperStatusAsync(status);
        public Task<List<ScraperStatusEntity>> GetAllRunningScrapersAsync() => _scraperStatusRepository.GetAllRunningScrapersAsync();

        // Scraper Log operations
        public Task<List<ScraperLogEntity>> GetScraperLogsAsync(string scraperId, int limit = 100) => _scraperRunRepository.GetScraperLogsAsync(scraperId, limit);
        public Task<ScraperLogEntity> AddScraperLogAsync(ScraperLogEntity logEntry) => _scraperRunRepository.AddScraperLogAsync(logEntry);

        // Scraped Page operations
        public Task<ScrapedPageEntity> AddScrapedPageAsync(ScrapedPageEntity page) => _scrapedPageRepository.AddScrapedPageAsync(page);

        // Scraper Run operations
        public Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10) => _scraperRunRepository.GetScraperRunsAsync(scraperId, limit);
        public Task<ScraperRunEntity?> GetScraperRunByIdAsync(string runId) => _scraperRunRepository.GetScraperRunByIdAsync(runId);
        public Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run) => _scraperRunRepository.CreateScraperRunAsync(run);
        public Task<ScraperRunEntity> UpdateScraperRunAsync(ScraperRunEntity run) => _scraperRunRepository.UpdateScraperRunAsync(run);

        // Log Entry operations
        public Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100) => _scraperRunRepository.GetLogEntriesAsync(scraperId, limit);
        public Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry) => _scraperRunRepository.AddLogEntryAsync(logEntry);

        // Content Change Record operations
        public Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50) => _scraperRunRepository.GetContentChangesAsync(scraperId, limit);
        public Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change) => _scraperRunRepository.AddContentChangeAsync(change);

        // Processed Document operations
        public Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50) => _scrapedPageRepository.GetProcessedDocumentsAsync(scraperId, limit);
        public Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId) => _scrapedPageRepository.GetDocumentByIdAsync(documentId);
        public Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document) => _scrapedPageRepository.AddProcessedDocumentAsync(document);

        // Metrics operations
        public Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId) => _metricsRepository.GetScraperMetricsAsync(scraperId);
        public Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to)
            => _metricsRepository.GetScraperMetricsAsync(scraperId, metricName, from, to);
        public Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric) => _metricsRepository.AddScraperMetricAsync(metric);

        // Pipeline Metrics operations
        public Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId) => _metricsRepository.GetLatestPipelineMetricAsync(scraperId);
        public Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric) => _metricsRepository.AddPipelineMetricAsync(metric);

        // Content Classification operations
        public Task<object> GetContentClassificationAsync(string scraperId, string url) => _scrapedPageRepository.GetContentClassificationAsync(scraperId, url);
        public Task<List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50) => _scrapedPageRepository.GetContentClassificationsAsync(scraperId, limit);
        public Task<object> SaveContentClassificationAsync(object classification) => _scrapedPageRepository.SaveContentClassificationAsync(classification);
    }
}