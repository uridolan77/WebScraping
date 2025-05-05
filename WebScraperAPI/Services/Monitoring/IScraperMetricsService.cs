using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper;
using WebScraper.Monitoring;

namespace WebScraperApi.Services.Monitoring
{
    /// <summary>
    /// Service for managing scraper metrics
    /// </summary>
    public interface IScraperMetricsService
    {
        /// <summary>
        /// Updates metrics for a running scraper
        /// </summary>
        /// <param name="id">The scraper ID</param>
        /// <param name="scraper">The scraper instance</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateScraperMetricsFromRuntimeAsync(string id, Scraper scraper);

        /// <summary>
        /// Updates a single metric for a scraper
        /// </summary>
        /// <param name="scraperId">The scraper ID</param>
        /// <param name="scraperName">The scraper name</param>
        /// <param name="metricName">The name of the metric</param>
        /// <param name="metricValue">The value of the metric</param>
        /// <param name="runId">Optional run ID</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateMetricAsync(string scraperId, string scraperName, string metricName, double metricValue, string runId = null);

        /// <summary>
        /// Updates metrics for all active scrapers
        /// </summary>
        /// <param name="activeScrapers">Dictionary of active scrapers</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateMetricsForAllScrapersAsync(Dictionary<string, Scraper> activeScrapers);
    }
}
