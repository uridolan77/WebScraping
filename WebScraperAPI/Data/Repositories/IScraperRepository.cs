using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    /// <summary>
    /// Repository interface for scraper operations
    /// </summary>
    public interface IScraperRepository
    {
        // Database operations
        bool TestDatabaseConnection();
        // Scraper Config operations
        Task<List<ScraperConfigEntity>> GetAllScrapersAsync();
        Task<ScraperConfigEntity> GetScraperByIdAsync(string id);
        Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper);
        Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper);
        Task<bool> DeleteScraperAsync(string id);

        // Related entity operations
        Task<List<ScraperStartUrlEntity>> GetScraperStartUrlsAsync(string scraperId);
        Task<List<ContentExtractorSelectorEntity>> GetContentExtractorSelectorsAsync(string scraperId);
        Task<List<KeywordAlertEntity>> GetKeywordAlertsAsync(string scraperId);
        Task<List<DomainRateLimitEntity>> GetDomainRateLimitsAsync(string scraperId);
        Task<List<ProxyConfigurationEntity>> GetProxyConfigurationsAsync(string scraperId);
        Task<List<WebhookTriggerEntity>> GetWebhookTriggersAsync(string scraperId);
        Task<List<ScraperScheduleEntity>> GetScraperSchedulesAsync(string scraperId);

        // Scraper Status operations
        Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId);
        Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status);
        Task<List<ScraperStatusEntity>> GetAllRunningScrapersAsync();

        // Scraper Log operations - Added missing methods
        Task<List<ScraperLogEntity>> GetScraperLogsAsync(string scraperId, int limit = 100);
        Task<ScraperLogEntity> AddScraperLogAsync(ScraperLogEntity logEntry);

        // Scraped Page operations - Added missing method
        Task<ScrapedPageEntity> AddScrapedPageAsync(ScrapedPageEntity page);

        // Scraper Run operations
        Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10);
        Task<ScraperRunEntity?> GetScraperRunByIdAsync(string runId);
        Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run);
        Task<ScraperRunEntity> UpdateScraperRunAsync(ScraperRunEntity run);

        // Log Entry operations
        Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100);
        Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry);

        // Content Change Record operations
        Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50);
        Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change);

        // Processed Document operations
        Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50);
        Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId);
        Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document);

        // Metrics operations
        Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId); // Added overload
        Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to);
        Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric);

        // Pipeline Metrics operations
        Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId);
        Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric);

        // Content Classification operations - temporarily using object instead of ContentClassificationEntity
        Task<object> GetContentClassificationAsync(string scraperId, string url);
        Task<List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50);
        Task<object> SaveContentClassificationAsync(object classification);
    }
}
