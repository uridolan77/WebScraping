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

                // Save only to the scrapermetric table
                try
                {
                    _context.ScraperMetric.Add(metric);
                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Successfully added metric to ScraperMetric table: {metric.MetricName} = {metric.MetricValue}");
                }
                catch (Exception primaryEx)
                {
                    Console.WriteLine($"Error adding to ScraperMetric table: {primaryEx.Message}");
                    if (primaryEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {primaryEx.InnerException.Message}");
                    }
                    Console.WriteLine($"Stack trace: {primaryEx.StackTrace}");

                    // Try to manually execute the insert using raw SQL if EF Core fails
                    try
                    {
                        Console.WriteLine("Attempting direct SQL insert to scrapermetric table");
                        var connection = _context.Database.GetDbConnection();
                        await connection.OpenAsync();

                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = @"
                                INSERT INTO scrapermetric (scraper_id, metric_name, metric_value, timestamp)
                                VALUES (@scraperId, @metricName, @metricValue, @timestamp)";

                            var scraperIdParam = command.CreateParameter();
                            scraperIdParam.ParameterName = "@scraperId";
                            scraperIdParam.Value = metric.ScraperId;
                            command.Parameters.Add(scraperIdParam);

                            var metricNameParam = command.CreateParameter();
                            metricNameParam.ParameterName = "@metricName";
                            metricNameParam.Value = metric.MetricName;
                            command.Parameters.Add(metricNameParam);

                            var metricValueParam = command.CreateParameter();
                            metricValueParam.ParameterName = "@metricValue";
                            metricValueParam.Value = metric.MetricValue;
                            command.Parameters.Add(metricValueParam);

                            var timestampParam = command.CreateParameter();
                            timestampParam.ParameterName = "@timestamp";
                            timestampParam.Value = metric.Timestamp;
                            command.Parameters.Add(timestampParam);

                            var result = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"Direct SQL insert result: {result} rows affected");
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