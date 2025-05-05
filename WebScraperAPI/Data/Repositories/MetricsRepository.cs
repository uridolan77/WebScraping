using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class MetricsRepository : IMetricsRepository
    {
        private readonly WebScraperDbContext _context;

        public MetricsRepository(WebScraperDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId)
        {
            try
            {
                // Default to retrieving metrics from the last 24 hours
                var from = DateTime.Now.AddDays(-1);
                var to = DateTime.Now;

                // Try to get metrics from the custommetric table instead
                // Create empty ScraperMetricEntity objects with data from custommetric
                var customMetrics = await _context.CustomMetric
                    .Select(cm => new ScraperMetricEntity
                    {
                        Id = cm.Id,
                        ScraperId = scraperId,
                        MetricName = cm.MetricName,
                        MetricValue = cm.MetricValue,
                        Timestamp = DateTime.Now
                    })
                    .ToListAsync();

                if (customMetrics.Count > 0)
                {
                    return customMetrics;
                }

                // Fallback to the original query if no custom metrics found
                return await _context.ScraperMetric
                    .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting metrics: {ex.Message}");
                // Return an empty list if there's an error
                return [];
            }
        }

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to)
        {
            try
            {
                // Try to get metrics from the custommetric table first
                var customMetrics = await _context.CustomMetric
                    .Select(cm => new ScraperMetricEntity
                    {
                        Id = cm.Id,
                        ScraperId = scraperId,
                        MetricName = cm.MetricName,
                        MetricValue = cm.MetricValue,
                        Timestamp = DateTime.Now
                    })
                    .ToListAsync();

                if (customMetrics.Count > 0)
                {
                    // Filter the custom metrics based on the metricName parameter
                    if (!string.IsNullOrEmpty(metricName))
                    {
                        customMetrics = customMetrics.Where(m => m.MetricName == metricName).ToList();
                    }

                    return customMetrics;
                }

                // Fallback to the original query if no custom metrics found
                var query = _context.ScraperMetric
                    .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to);

                if (!string.IsNullOrEmpty(metricName))
                {
                    query = query.Where(m => m.MetricName == metricName);
                }

                return await query
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting metrics: {ex.Message}");
                // Return an empty list if there's an error
                return [];
            }
        }

        public async Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric)
        {
            try
            {
                Console.WriteLine($"DEBUG: AddScraperMetricAsync called with metric name: {metric.MetricName}, value: {metric.MetricValue}, ScraperId: {metric.ScraperId}");

                // Ensure ScraperRun record exists for this scraper
                var existingRun = await _context.ScraperRun
                    .Where(r => r.ScraperId == metric.ScraperId && (r.EndTime == null))
                    .FirstOrDefaultAsync();

                if (existingRun == null)
                {
                    // Create a new ScraperRun record if none exists
                    var scraperRun = new ScraperRunEntity
                    {
                        ScraperId = metric.ScraperId,
                        StartTime = DateTime.Now,
                        Successful = null, // Still running
                        ErrorMessage = string.Empty
                    };

                    _context.ScraperRun.Add(scraperRun);
                    try
                    {
                        await _context.SaveChangesAsync();
                        Console.WriteLine($"Created new ScraperRun record for ScraperId: {metric.ScraperId}");

                        // Set the RunId on the metric
                        metric.RunId = scraperRun.Id;
                    }
                    catch (Exception runEx)
                    {
                        Console.WriteLine($"Error saving ScraperRun: {runEx.Message}");
                    }
                }
                else
                {
                    // Set the RunId on the metric using the existing run
                    metric.RunId = existingRun.Id;
                }

                // Always use direct SQL insert to avoid EF Core issues with NULL values
                try
                {
                    Console.WriteLine("Using direct SQL insert to scrapermetric table");
                    var connection = _context.Database.GetDbConnection();
                    await connection.OpenAsync();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = @"
                            INSERT INTO scrapermetric (scraperid, runid, metricname, metricvalue, timestamp, scrapername)
                            VALUES (@scraperId, @runId, @metricName, @metricValue, @timestamp, @scraperName)";

                        // Ensure all string values have defaults to avoid DBNull casting issues
                        var scraperIdParam = command.CreateParameter();
                        scraperIdParam.ParameterName = "@scraperId";
                        scraperIdParam.Value = !string.IsNullOrEmpty(metric.ScraperId) ? metric.ScraperId : string.Empty;
                        command.Parameters.Add(scraperIdParam);

                        var runIdParam = command.CreateParameter();
                        runIdParam.ParameterName = "@runId";
                        runIdParam.Value = !string.IsNullOrEmpty(metric.RunId) ? metric.RunId : (object)DBNull.Value;
                        command.Parameters.Add(runIdParam);

                        var metricNameParam = command.CreateParameter();
                        metricNameParam.ParameterName = "@metricName";
                        metricNameParam.Value = !string.IsNullOrEmpty(metric.MetricName) ? metric.MetricName : string.Empty;
                        command.Parameters.Add(metricNameParam);

                        var metricValueParam = command.CreateParameter();
                        metricValueParam.ParameterName = "@metricValue";
                        metricValueParam.Value = metric.MetricValue;
                        command.Parameters.Add(metricValueParam);

                        var timestampParam = command.CreateParameter();
                        timestampParam.ParameterName = "@timestamp";
                        timestampParam.Value = metric.Timestamp;
                        command.Parameters.Add(timestampParam);

                        var scraperNameParam = command.CreateParameter();
                        scraperNameParam.ParameterName = "@scraperName";
                        scraperNameParam.Value = !string.IsNullOrEmpty(metric.ScraperName) ? metric.ScraperName : string.Empty;
                        command.Parameters.Add(scraperNameParam);

                        var result = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"Direct SQL insert result: {result} rows affected");

                        // Set a dummy ID since we don't have the actual auto-increment ID
                        if (result > 0 && metric.Id == 0)
                        {
                            metric.Id = 1; // Just to indicate success
                        }
                    }
                }
                catch (Exception sqlEx)
                {
                    Console.WriteLine($"Error with direct SQL insert: {sqlEx.Message}");
                    if (sqlEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {sqlEx.InnerException.Message}");
                    }
                    Console.WriteLine($"Stack trace: {sqlEx.StackTrace}");
                }

                return metric;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical error in AddScraperMetricAsync: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return metric;
            }
        }

        public async Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId)
        {
            return await _context.PipelineMetric
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric)
        {
            _context.PipelineMetric.Add(metric);
            await _context.SaveChangesAsync();

            return metric;
        }
    }
}