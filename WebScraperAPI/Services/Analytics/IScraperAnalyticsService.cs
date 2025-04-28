using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebScraperApi.Services.Analytics
{
    /// <summary>
    /// Interface for scraper analytics services
    /// </summary>
    public interface IScraperAnalyticsService
    {
        /// <summary>
        /// Gets analytics data for a specific scraper
        /// </summary>
        Task<Dictionary<string, object>> GetScraperAnalyticsAsync(string id);

        /// <summary>
        /// Gets detected content changes for a specific scraper
        /// </summary>
        Task<object> GetDetectedChangesAsync(string id, DateTime? since = null, int limit = 100);

        /// <summary>
        /// Gets processed documents for a specific scraper
        /// </summary>
        Task<object> GetProcessedDocumentsAsync(string id, string documentType = null, int page = 1, int pageSize = 20);

        /// <summary>
        /// Gets detailed telemetry metrics for a specific scraper
        /// </summary>
        Task<object> GetScraperMetricsAsync(string id);

        /// <summary>
        /// Gets a summary of analytics across all scrapers
        /// </summary>
        Task<object> GetAnalyticsSummaryAsync();

        /// <summary>
        /// Gets performance metrics for a specific scraper within a date range
        /// </summary>
        Task<object> GetScraperPerformanceAsync(string id, DateTime? start = null, DateTime? end = null);

        /// <summary>
        /// Gets the most popular domains being scraped
        /// </summary>
        Task<IEnumerable<object>> GetPopularDomainsAsync(int count = 10);

        /// <summary>
        /// Gets the frequency of content changes across all scrapers
        /// </summary>
        Task<object> GetContentChangeFrequencyAsync(DateTime? since = null);

        /// <summary>
        /// Gets usage statistics within a date range
        /// </summary>
        Task<object> GetUsageStatisticsAsync(DateTime start, DateTime end);

        /// <summary>
        /// Gets the distribution of errors across all scrapers
        /// </summary>
        Task<object> GetErrorDistributionAsync(DateTime? since = null);
    }
}