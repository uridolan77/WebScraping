using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    public interface IScraperService
    {
        // Scraper configuration operations
        Task<List<ScraperConfigModel>> GetAllScrapersAsync();
        Task<ScraperConfigModel> GetScraperByIdAsync(string id);
        Task<ScraperConfigModel> CreateScraperAsync(ScraperConfigModel scraper);
        Task<ScraperConfigModel> UpdateScraperAsync(string id, ScraperConfigModel scraper);
        Task<bool> DeleteScraperAsync(string id);
        
        // Scraper execution operations
        Task<bool> StartScraperAsync(string id);
        Task<bool> StopScraperAsync(string id);
        Task<ScraperStatus> GetScraperStatusAsync(string id);
        
        // Scraper run operations
        Task<List<ScraperRun>> GetScraperRunsAsync(string scraperId, int limit = 10);
        Task<ScraperRun> GetScraperRunByIdAsync(string runId);
        
        // Content change operations
        Task<List<ContentChangeRecord>> GetContentChangesAsync(string scraperId, int limit = 50);
        
        // Document operations
        Task<List<ProcessedDocument>> GetProcessedDocumentsAsync(string scraperId, int limit = 50);
        Task<ProcessedDocument> GetDocumentByIdAsync(string documentId);
        
        // Metrics operations
        Task<Dictionary<string, List<KeyValuePair<DateTime, double>>>> GetScraperMetricsAsync(
            string scraperId, 
            DateTime from, 
            DateTime to, 
            string[] metricNames = null);
    }
}
