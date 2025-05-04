using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IScraperConfigRepository
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
    }
}
