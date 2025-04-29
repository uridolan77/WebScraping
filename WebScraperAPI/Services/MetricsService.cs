using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Repositories;

namespace WebScraperApi.Services
{
    public class MetricsService : BaseService
    {
        public MetricsService(IScraperRepository repository, ILogger<MetricsService> logger)
            : base(repository, logger)
        {
        }

        public async Task<Dictionary<string, List<KeyValuePair<DateTime, double>>>> GetScraperMetricsAsync(
            string scraperId, 
            DateTime from, 
            DateTime to, 
            string[] metricNames = null)
        {
            try
            {
                var result = new Dictionary<string, List<KeyValuePair<DateTime, double>>>();
                
                // Get all metrics for the scraper in the time range
                var metrics = await _repository.GetScraperMetricsAsync(scraperId, null, from, to);
                
                // Group by metric name
                var groupedMetrics = metrics.GroupBy(m => m.MetricName);
                
                // Filter by metric names if provided
                if (metricNames != null && metricNames.Length > 0)
                {
                    groupedMetrics = groupedMetrics.Where(g => metricNames.Contains(g.Key));
                }
                
                // Convert to the desired format
                foreach (var group in groupedMetrics)
                {
                    var metricName = group.Key;
                    var values = group.Select(m => new KeyValuePair<DateTime, double>(m.Timestamp, m.MetricValue))
                                     .OrderBy(kv => kv.Key)
                                     .ToList();
                    
                    result[metricName] = values;
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metrics for scraper with ID {ScraperId}", scraperId);
                throw;
            }
        }
    }
}
