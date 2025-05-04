using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IScraperRunRepository
    {
        // Scraper Run operations
        Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10);
        Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId);
        Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run);
        Task<ScraperRunEntity> UpdateScraperRunAsync(ScraperRunEntity run);

        // Log Entry operations
        Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100);
        Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry);
        
        // Scraper Log operations
        Task<List<ScraperLogEntity>> GetScraperLogsAsync(string scraperId, int limit = 100);
        Task<ScraperLogEntity> AddScraperLogAsync(ScraperLogEntity logEntry);
        
        // Content Change Record operations
        Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50);
        Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change);
    }
}