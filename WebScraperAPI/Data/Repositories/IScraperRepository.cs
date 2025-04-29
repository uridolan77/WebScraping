using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IScraperRepository
    {
        // Scraper Config operations
        Task<List<ScraperConfigEntity>> GetAllScrapersAsync();
        Task<ScraperConfigEntity> GetScraperByIdAsync(string id);
        Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper);
        Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper);
        Task<bool> DeleteScraperAsync(string id);
        
        // Scraper Status operations
        Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId);
        Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status);
        
        // Scraper Run operations
        Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10);
        Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId);
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
        Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to);
        Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric);
        
        // Pipeline Metrics operations
        Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId);
        Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric);
    }
}
