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
    }
}