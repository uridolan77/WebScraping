using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;

namespace WebScraper.Monitoring
{
    /// <summary>
    /// Provides enhanced telemetry for scraper operations with metrics collection
    /// </summary>
    public class ScraperMetrics : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, long> _counters = new();
        private readonly ConcurrentDictionary<string, ConcurrentQueue<double>> _histograms = new();
        private readonly ConcurrentDictionary<string, Stopwatch> _timers = new();
        private readonly ConcurrentDictionary<string, (double Sum, int Count)> _gauges = new();
        private readonly Timer _reportingTimer;
        private readonly string _scraperId;
        private bool _disposed = false;

        public ScraperMetrics(ILogger logger, string scraperId, int reportingIntervalSeconds = 60)
        {
            _logger = logger;
            _scraperId = scraperId;
            
            // Initialize reporting timer to periodically log metrics
            _reportingTimer = new Timer(
                _ => ReportMetrics(), 
                null, 
                TimeSpan.FromSeconds(reportingIntervalSeconds), 
                TimeSpan.FromSeconds(reportingIntervalSeconds));
        }

        /// <summary>
        /// Records time taken to process a page
        /// </summary>
        public void RecordPageProcessingTime(string url, TimeSpan processingTime, bool success = true)
        {
            try
            {
                var domain = new Uri(url).Host;
                string metricName = $"page.processing_time.{(success ? "success" : "failure")}";
                
                RecordHistogram(metricName, processingTime.TotalMilliseconds, new[] { $"domain:{domain}" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to record page processing time for {Url}", url);
            }
        }

        /// <summary>
        /// Increments the count of pages processed
        /// </summary>
        public void IncrementPageProcessed(string url, bool success = true)
        {
            try
            {
                var domain = new Uri(url).Host;
                string metricName = $"page.processed.{(success ? "success" : "failure")}";
                
                IncrementCounter(metricName, 1, new[] { $"domain:{domain}" });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to increment page processed count for {Url}", url);
            }
        }

        /// <summary>
        /// Records content size metrics
        /// </summary>
        public void RecordContentSize(string contentType, long sizeInBytes)
        {
            RecordHistogram($"content.size", sizeInBytes, new[] { $"type:{contentType}" });
        }

        /// <summary>
        /// Records document processing time
        /// </summary>
        public void RecordDocumentProcessingTime(string documentType, TimeSpan processingTime)
        {
            RecordHistogram($"document.processing_time", processingTime.TotalMilliseconds, new[] { $"type:{documentType}" });
        }
        
        /// <summary>
        /// Starts measuring processing time for a specific operation
        /// </summary>
        public void StartTimer(string name, params string[] tags)
        {
            var key = FormatMetricName(name, tags);
            var timer = new Stopwatch();
            timer.Start();
            _timers[key] = timer;
        }
        
        /// <summary>
        /// Stops measuring processing time for a specific operation and records the result
        /// </summary>
        public TimeSpan StopTimer(string name, params string[] tags)
        {
            var key = FormatMetricName(name, tags);
            
            if (_timers.TryRemove(key, out var timer))
            {
                timer.Stop();
                var elapsed = timer.Elapsed;
                RecordHistogram(name, elapsed.TotalMilliseconds, tags);
                return elapsed;
            }
            
            return TimeSpan.Zero;
        }

        /// <summary>
        /// Records a histogram value (for statistical distribution)
        /// </summary>
        public void RecordHistogram(string name, double value, params string[] tags)
        {
            var key = FormatMetricName(name, tags);
            _histograms.GetOrAdd(key, _ => new ConcurrentQueue<double>()).Enqueue(value);
        }

        /// <summary>
        /// Increments a counter by the specified amount
        /// </summary>
        public void IncrementCounter(string name, long amount = 1, params string[] tags)
        {
            var key = FormatMetricName(name, tags);
            _counters.AddOrUpdate(key, amount, (_, current) => current + amount);
        }
        
        /// <summary>
        /// Sets or updates a gauge value (for current state)
        /// </summary>
        public void RecordGauge(string name, double value, params string[] tags)
        {
            var key = FormatMetricName(name, tags);
            _gauges[key] = (value, 1);
        }

        /// <summary>
        /// Formats the metric name with tags
        /// </summary>
        private static string FormatMetricName(string name, params string[] tags)
        {
            if (tags == null || tags.Length == 0)
                return name;
                
            return $"{name}:{string.Join(",", tags)}";
        }

        /// <summary>
        /// Reports all collected metrics
        /// </summary>
        private void ReportMetrics()
        {
            var report = new Dictionary<string, object>();

            // Include scraper ID
            report["ScraperId"] = _scraperId;
            report["Timestamp"] = DateTime.UtcNow;
            
            // Process counters
            var counterData = new Dictionary<string, long>();
            foreach (var counter in _counters)
            {
                counterData[counter.Key] = counter.Value;
            }
            report["Counters"] = counterData;

            // Process histograms
            var histogramData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var histogram in _histograms)
            {
                var values = histogram.Value.ToArray();
                if (values.Length > 0)
                {
                    histogramData[histogram.Key] = new Dictionary<string, object>
                    {
                        ["Count"] = values.Length,
                        ["Min"] = values.Min(),
                        ["Max"] = values.Max(),
                        ["Avg"] = values.Average(),
                        ["Sum"] = values.Sum(),
                        ["P95"] = CalculatePercentile(values, 95)
                    };
                }
            }
            report["Histograms"] = histogramData;
            
            // Process gauges
            var gaugeData = new Dictionary<string, double>();
            foreach (var gauge in _gauges)
            {
                gaugeData[gauge.Key] = gauge.Value.Sum / gauge.Value.Count;
            }
            report["Gauges"] = gaugeData;

            // Log the report
            _logger.LogInformation("Metrics: {MetricsReport}", 
                System.Text.Json.JsonSerializer.Serialize(report));
            
            // Optional: Send metrics to external monitoring system
            // This would be implemented based on the specific monitoring system used
        }

        /// <summary>
        /// Gets the current metrics snapshot
        /// </summary>
        public Dictionary<string, object> GetMetricsSnapshot()
        {
            var snapshot = new Dictionary<string, object>();
            
            // Include counters
            snapshot["Counters"] = new Dictionary<string, long>(_counters);
            
            // Process histograms
            var histogramData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var histogram in _histograms)
            {
                var values = histogram.Value.ToArray();
                if (values.Length > 0)
                {
                    histogramData[histogram.Key] = new Dictionary<string, object>
                    {
                        ["Count"] = values.Length,
                        ["Min"] = values.Min(),
                        ["Max"] = values.Max(),
                        ["Avg"] = values.Average(),
                        ["Sum"] = values.Sum(),
                        ["P95"] = CalculatePercentile(values, 95)
                    };
                }
            }
            snapshot["Histograms"] = histogramData;
            
            // Include gauges
            var gaugeData = new Dictionary<string, double>();
            foreach (var gauge in _gauges)
            {
                gaugeData[gauge.Key] = gauge.Value.Sum / gauge.Value.Count;
            }
            snapshot["Gauges"] = gaugeData;
            
            return snapshot;
        }

        /// <summary>
        /// Calculates percentile value for an array of values
        /// </summary>
        private static double CalculatePercentile(double[] values, int percentile)
        {
            if (values.Length == 0)
                return 0;
                
            var sortedValues = values.OrderBy(v => v).ToArray();
            var index = (int)Math.Ceiling((percentile / 100.0) * sortedValues.Length) - 1;
            return sortedValues[Math.Max(0, Math.Min(index, sortedValues.Length - 1))];
        }

        /// <summary>
        /// Disposes the metrics reporting timer
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _reportingTimer?.Dispose();
                _disposed = true;
            }
        }
    }
}