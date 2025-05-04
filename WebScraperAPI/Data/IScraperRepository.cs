using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperAPI.Data
{
    [Obsolete("This interface is deprecated. Use WebScraperApi.Data.Repositories.IScraperRepository instead.")]
    public interface IScraperRepository
    {
        // Scraper Status methods
        Task<ScraperStatusEntity?> GetScraperStatusAsync(string scraperId);
        Task UpdateScraperStatusAsync(ScraperStatusEntity status);
        
        // Scraper Metrics methods
        Task<IEnumerable<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to);
        Task AddScraperMetricAsync(ScraperMetricEntity metric);
        
        // Scraper Logs methods
        Task<IEnumerable<ScraperLogEntity>> GetScraperLogsAsync(string scraperId);
        Task AddScraperLogAsync(ScraperLogEntity log);
        
        // Scraped Pages methods
        Task<IEnumerable<ScrapedPageEntity>> GetScrapedPagesAsync(string scraperId);
        Task AddScrapedPageAsync(ScrapedPageEntity page);
    }
}