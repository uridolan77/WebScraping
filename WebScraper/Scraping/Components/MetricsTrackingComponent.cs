using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WebScraper.Monitoring;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that tracks metrics for scraper performance monitoring
    /// </summary>
    public class MetricsTrackingComponent : ScraperComponentBase
    {
        private readonly ScraperMetrics _metrics;
        private readonly Stopwatch _sessionStopwatch = new Stopwatch();
        private readonly Dictionary<string, Stopwatch> _domainStopwatches = new Dictionary<string, Stopwatch>();
        
        /// <summary>
        /// Initializes a new instance of the MetricsTrackingComponent class
        /// </summary>
        /// <param name="metrics">The metrics object to update</param>
        public MetricsTrackingComponent(ScraperMetrics metrics)
        {
            _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            _metrics.ResetSessionMetrics();
            LogInfo("Metrics tracking component initialized");
        }
        
        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public override Task OnScrapingStartedAsync()
        {
            // Start timing the scraping session
            _sessionStopwatch.Restart();
            _metrics.SessionStartTime = DateTime.Now;
            
            LogInfo("Started metrics tracking for scraping session");
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override Task OnScrapingCompletedAsync()
        {
            // Stop the session timer
            _sessionStopwatch.Stop();
            _metrics.LastSessionDurationMs = _sessionStopwatch.ElapsedMilliseconds;
            _metrics.TotalScrapingTimeMs += _sessionStopwatch.ElapsedMilliseconds;
            
            // Log metrics
            LogInfo($"Scraping session completed in {_metrics.LastSessionDurationMs}ms");
            LogInfo($"URLs processed: {_metrics.ProcessedUrls} (success: {_metrics.SuccessfulUrls}, failed: {_metrics.FailedUrls})");
            LogInfo($"Content items: {_metrics.ContentItemsExtracted} (pages: {_metrics.PagesProcessed}, documents: {_metrics.DocumentsProcessed})");
            if (_metrics.PagesProcessed > 0)
            {
                LogInfo($"Average page processing time: {_metrics.TotalPageProcessingTimeMs / _metrics.PagesProcessed}ms");
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        public override Task OnScrapingStoppedAsync()
        {
            // Handle early stopping metrics
            if (_sessionStopwatch.IsRunning)
            {
                _sessionStopwatch.Stop();
                _metrics.LastSessionDurationMs = _sessionStopwatch.ElapsedMilliseconds;
                _metrics.TotalScrapingTimeMs += _sessionStopwatch.ElapsedMilliseconds;
                
                LogInfo($"Scraping session stopped early after {_metrics.LastSessionDurationMs}ms");
            }
            
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Records the start of a URL request
        /// </summary>
        /// <param name="url">The URL being requested</param>
        public void StartUrlRequest(string url)
        {
            try
            {
                string domain = ExtractDomain(url);
                
                lock (_domainStopwatches)
                {
                    if (!_domainStopwatches.ContainsKey(domain))
                    {
                        _domainStopwatches[domain] = new Stopwatch();
                    }
                    
                    _domainStopwatches[domain].Restart();
                }
                
                _metrics.PendingRequests++;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error starting URL request tracking for {url}");
            }
        }
        
        /// <summary>
        /// Records the completion of a URL request
        /// </summary>
        /// <param name="url">The URL that was requested</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="contentSizeBytes">The size of the content in bytes</param>
        public void CompleteUrlRequest(string url, int statusCode, long contentSizeBytes)
        {
            try
            {
                string domain = ExtractDomain(url);
                long elapsedMs = 0;
                
                lock (_domainStopwatches)
                {
                    if (_domainStopwatches.TryGetValue(domain, out var stopwatch))
                    {
                        stopwatch.Stop();
                        elapsedMs = stopwatch.ElapsedMilliseconds;
                    }
                }
                
                _metrics.PendingRequests--;
                _metrics.ProcessedUrls++;
                _metrics.TotalBytesDownloaded += contentSizeBytes;
                
                // Update domain-specific metrics
                if (!_metrics.DomainMetrics.ContainsKey(domain))
                {
                    _metrics.DomainMetrics[domain] = new DomainMetrics();
                }
                
                var domainMetrics = _metrics.DomainMetrics[domain];
                domainMetrics.RequestCount++;
                domainMetrics.TotalResponseTimeMs += elapsedMs;
                domainMetrics.AverageResponseTimeMs = domainMetrics.TotalResponseTimeMs / domainMetrics.RequestCount;
                domainMetrics.TotalBytesDownloaded += contentSizeBytes;
                
                // Track status code
                if (statusCode >= 200 && statusCode < 300)
                {
                    _metrics.SuccessfulUrls++;
                    domainMetrics.SuccessfulRequests++;
                }
                else if (statusCode >= 400 && statusCode < 500)
                {
                    _metrics.ClientErrors++;
                    domainMetrics.ClientErrors++;
                }
                else if (statusCode >= 500)
                {
                    _metrics.ServerErrors++;
                    domainMetrics.ServerErrors++;
                }
                
                if (statusCode == 429)
                {
                    _metrics.RateLimitErrors++;
                    domainMetrics.RateLimitHits++;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error completing URL request tracking for {url}");
            }
        }
        
        /// <summary>
        /// Records a failed URL request
        /// </summary>
        /// <param name="url">The URL that failed</param>
        /// <param name="exception">The exception that occurred</param>
        public void RecordFailedRequest(string url, Exception exception)
        {
            try
            {
                string domain = ExtractDomain(url);
                
                _metrics.PendingRequests--;
                _metrics.ProcessedUrls++;
                _metrics.FailedUrls++;
                
                // Update domain-specific metrics
                if (!_metrics.DomainMetrics.ContainsKey(domain))
                {
                    _metrics.DomainMetrics[domain] = new DomainMetrics();
                }
                
                var domainMetrics = _metrics.DomainMetrics[domain];
                domainMetrics.RequestCount++;
                domainMetrics.FailedRequests++;
                
                // Track error type
                if (exception is TimeoutException)
                {
                    _metrics.TimeoutErrors++;
                    domainMetrics.Timeouts++;
                }
                else if (exception is System.Net.WebException)
                {
                    _metrics.NetworkErrors++;
                    domainMetrics.NetworkErrors++;
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error recording failed request for {url}");
            }
        }
        
        /// <summary>
        /// Records a processed page
        /// </summary>
        /// <param name="url">The URL of the page</param>
        /// <param name="processingTimeMs">The time taken to process the page in milliseconds</param>
        /// <param name="linksExtracted">The number of links extracted from the page</param>
        public void RecordProcessedPage(string url, long processingTimeMs, int linksExtracted)
        {
            try
            {
                _metrics.PagesProcessed++;
                _metrics.TotalPageProcessingTimeMs += processingTimeMs;
                _metrics.TotalLinksExtracted += linksExtracted;
                _metrics.ContentItemsExtracted++;
                
                // Calculate averages
                _metrics.AveragePageProcessingTimeMs = _metrics.TotalPageProcessingTimeMs / _metrics.PagesProcessed;
                _metrics.AverageLinksPerPage = _metrics.PagesProcessed > 0 ? 
                    (double)_metrics.TotalLinksExtracted / _metrics.PagesProcessed : 0;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error recording processed page for {url}");
            }
        }
        
        /// <summary>
        /// Records a processed document
        /// </summary>
        /// <param name="url">The URL of the document</param>
        /// <param name="documentType">The type of document</param>
        /// <param name="sizeBytes">The size of the document in bytes</param>
        /// <param name="processingTimeMs">The time taken to process the document</param>
        public void RecordProcessedDocument(string url, string documentType, long sizeBytes, long processingTimeMs)
        {
            try
            {
                _metrics.DocumentsProcessed++;
                _metrics.TotalDocumentProcessingTimeMs += processingTimeMs;
                _metrics.TotalDocumentSizeBytes += sizeBytes;
                _metrics.ContentItemsExtracted++;
                
                // Update document type statistics
                if (!_metrics.DocumentTypeMetrics.ContainsKey(documentType))
                {
                    _metrics.DocumentTypeMetrics[documentType] = new DocumentTypeMetrics();
                }
                
                var docMetrics = _metrics.DocumentTypeMetrics[documentType];
                docMetrics.Count++;
                docMetrics.TotalSizeBytes += sizeBytes;
                docMetrics.TotalProcessingTimeMs += processingTimeMs;
                docMetrics.AverageSizeBytes = docMetrics.TotalSizeBytes / docMetrics.Count;
                docMetrics.AverageProcessingTimeMs = docMetrics.TotalProcessingTimeMs / docMetrics.Count;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error recording processed document for {url}");
            }
        }
        
        /// <summary>
        /// Records memory usage
        /// </summary>
        public void RecordMemoryUsage()
        {
            try
            {
                var process = Process.GetCurrentProcess();
                long workingSetMB = process.WorkingSet64 / (1024 * 1024);
                
                _metrics.CurrentMemoryUsageMB = workingSetMB;
                _metrics.PeakMemoryUsageMB = Math.Max(_metrics.PeakMemoryUsageMB, workingSetMB);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error recording memory usage");
            }
        }
        
        /// <summary>
        /// Extracts domain from a URL
        /// </summary>
        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "unknown";
            }
        }
    }
}