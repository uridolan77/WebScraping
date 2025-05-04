using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IMetricsRepository
    {
        // Metrics operations
        Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId);
        Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to);
        Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric);

        // Pipeline Metrics operations
        Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId);
        Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric);
    }
}