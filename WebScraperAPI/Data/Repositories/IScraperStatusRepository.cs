using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IScraperStatusRepository
    {
        // Scraper Status operations
        Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId);
        Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status);
        Task<List<ScraperStatusEntity>> GetAllRunningScrapersAsync();
    }
}