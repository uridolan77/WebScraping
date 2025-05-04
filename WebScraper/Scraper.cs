using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Monitoring;
using WebScraper.Scraping;
using WebScraper.Scraping.Components;

namespace WebScraper
{
    /// <summary>
    /// Comprehensive modular web scraper that handles all scraping functionality 
    /// through pluggable components
    /// </summary>
    public class Scraper : IDisposable
    {
        private readonly ScraperCore _core;
        private readonly ILogger _logger;
        private readonly ScraperConfig _config;
        private CancellationTokenSource _continuousScrapingCts;
        private bool _isInitialized;
        private bool _isDisposed;

        /// <summary>
        /// Gets the configuration for this scraper
        /// </summary>
        public ScraperConfig Config => _config;

        /// <summary>
        /// Gets current metrics for the scraper
        /// </summary>
        public ScraperMetrics Metrics { get; } = new ScraperMetrics();

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the Scraper class with an ILogger
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logger">Logger for the scraper</param>
        public Scraper(ScraperConfig config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _core = new ScraperCore(config, logger);
            
            RegisterComponents();
        }

        /// <summary>
        /// Initializes a new instance of the Scraper class with a log action
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        public Scraper(ScraperConfig config, Action<string> logAction)
            : this(config, new ActionLogger(logAction ?? (s => Console.WriteLine(s))))
        {
        }

        /// <summary>
        /// Initializes a new instance of the Scraper class with default logging to console
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        public Scraper(ScraperConfig config)
            : this(config, new ActionLogger(s => Console.WriteLine(s)))
        {
        }
        
        /// <summary>
        /// Initializes a new instance of the Scraper class with a default configuration
        /// </summary>
        /// <param name="startUrl">The URL to start scraping from</param>
        public Scraper(string startUrl)
            : this(new ScraperConfig { StartUrl = startUrl })
        {
        }

        #endregion

        /// <summary>
        /// Registers components with the scraper core based on configuration
        /// </summary>
        private void RegisterComponents()
        {
            // Core components
            _core.AddComponent(new HttpUrlProcessor());
            _core.AddComponent(new ContentExtractionComponent());
            
            // Optional components based on configuration
            if (_config.EnablePersistentState)
            {
                _core.AddComponent(new StateManagerComponent());
            }
            
            if (_config.ProcessJsHeavyPages || _config.IsUKGCWebsite)
            {
                _core.AddComponent(new HeadlessBrowserComponent());
            }
            
            if (_config.ProcessPdfDocuments || _config.ProcessOfficeDocuments)
            {
                _core.AddComponent(new DocumentProcessingComponent());
            }
            
            if (_config.EnableChangeDetection)
            {
                _core.AddComponent(new ChangeDetectionComponent());
            }
            
            if (_config.EnableRegulatoryContentAnalysis)
            {
                _core.AddComponent(new RegulatoryContentComponent());
            }
            
            if (_config.EnableAdaptiveCrawling)
            {
                _core.AddComponent(new AdaptiveCrawlingComponent());
            }
            
            if (_config.EnableRateLimiting)
            {
                _core.AddComponent(new RateLimitingComponent());
            }
            
            if (_config.EnableMetricsTracking)
            {
                _core.AddComponent(new MetricsTrackingComponent(Metrics));
            }
        }

        #region Public API

        /// <summary>
        /// Initializes the scraper and all components
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
                return;
                
            try
            {
                _logger.LogInformation("Starting scraper initialization...");
                _logger.LogInformation($"Scraper configuration: Name={_config.Name}, StartUrl={_config.StartUrl}");
                _logger.LogInformation($"Output directory: {_config.OutputDirectory}");

                // Ensure output directory exists
                if (!string.IsNullOrEmpty(_config.OutputDirectory) && !System.IO.Directory.Exists(_config.OutputDirectory))
                {
                    _logger.LogInformation($"Creating output directory: {_config.OutputDirectory}");
                    System.IO.Directory.CreateDirectory(_config.OutputDirectory);
                }

                // Initialize core components
                _logger.LogInformation("Initializing core components...");
                await _core.InitializeAsync();
                _isInitialized = true;
                _logger.LogInformation("Scraper initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize scraper");
                _logger.LogError($"Error details: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    _logger.LogError($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        /// <summary>
        /// Starts the scraping process
        /// </summary>
        public async Task StartScrapingAsync()
        {
            if (!_isInitialized)
            {
                _logger.LogInformation("Scraper not initialized, initializing now...");
                await InitializeAsync();
            }
            
            try
            {
                _logger.LogInformation("Starting scraping process...");
                _logger.LogInformation($"Configuration details: MaxDepth={_config.MaxDepth}, MaxConcurrentRequests={_config.MaxConcurrentRequests}");
                
                await _core.StartScrapingAsync();
                
                _logger.LogInformation("Scraping process completed successfully");
                
                // Update metrics
                Metrics.LastRunTime = DateTime.Now;
                if (Metrics.FirstRunTime == DateTime.MinValue)
                {
                    Metrics.FirstRunTime = DateTime.Now;
                }
                Metrics.TotalRuns++;
                _logger.LogInformation($"Updated metrics: TotalRuns={Metrics.TotalRuns}, FailedRuns={Metrics.FailedRuns}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scraping");
                _logger.LogError($"Error details: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                    _logger.LogError($"Inner exception stack trace: {ex.InnerException.StackTrace}");
                }
                
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                Metrics.FailedRuns++;
                throw;
            }
        }

        /// <summary>
        /// Stops the scraping process
        /// </summary>
        public void StopScraping()
        {
            try
            {
                _core.StopScraping();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraper");
            }
        }

        /// <summary>
        /// Processes a specific URL
        /// </summary>
        public async Task ProcessUrlAsync(string url)
        {
            if (!_isInitialized)
            {
                await InitializeAsync();
            }
            
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("Attempted to process null or empty URL");
                return;
            }

            try
            {
                _logger.LogInformation($"Processing URL: {url}");
                var urlProcessor = _core.GetComponent<IUrlProcessor>();
                
                if (urlProcessor != null)
                {
                    await urlProcessor.ProcessUrlAsync(url);
                    Metrics.ProcessedUrls++;
                }
                else
                {
                    _logger.LogWarning("No URL processor component found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing URL: {url}");
                Metrics.FailedUrls++;
            }
        }

        /// <summary>
        /// Sets up continuous scraping with the specified interval
        /// </summary>
        /// <param name="interval">The interval between scraping runs</param>
        public async Task SetupContinuousScrapingAsync(TimeSpan interval)
        {
            if (_continuousScrapingCts != null)
            {
                _logger.LogWarning("Continuous scraping already active, stopping previous instance");
                StopContinuousScraping();
            }
            
            _continuousScrapingCts = new CancellationTokenSource();
            var token = _continuousScrapingCts.Token;
            
            _logger.LogInformation($"Setting up continuous scraping with interval of {interval.TotalMinutes} minutes");
            
            try
            {
                while (!token.IsCancellationRequested)
                {
                    _logger.LogInformation("Starting scheduled scraping run");
                    await StartScrapingAsync();
                    _logger.LogInformation($"Scheduled scraping completed, waiting {interval.TotalMinutes} minutes until next run");
                    
                    try
                    {
                        await Task.Delay(interval, token);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger.LogInformation("Continuous scraping canceled");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in continuous scraping");
                throw;
            }
        }

        /// <summary>
        /// Stops continuous scraping if active
        /// </summary>
        public void StopContinuousScraping()
        {
            _logger.LogInformation("Stopping continuous scraping");
            _continuousScrapingCts?.Cancel();
            _continuousScrapingCts?.Dispose();
            _continuousScrapingCts = null;
        }

        /// <summary>
        /// Gets a component of the specified type
        /// </summary>
        /// <typeparam name="T">The type of component to get</typeparam>
        /// <returns>The component, or null if not found</returns>
        public T GetComponent<T>() where T : class, IScraperComponent
        {
            return _core.GetComponent<T>();
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;
                
            _isDisposed = true;
            
            try
            {
                StopContinuousScraping();
                _core.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing scraper");
            }
        }

        #endregion

        /// <summary>
        /// Simple logger that wraps a log action
        /// </summary>
        private class ActionLogger : ILogger
        {
            private readonly Action<string> _logAction;

            public ActionLogger(Action<string> logAction)
            {
                _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            }

            public IDisposable BeginScope<TState>(TState state) => null;
            
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);
                _logAction($"[{logLevel}] {message}");
                
                if (exception != null)
                {
                    _logAction($"[{logLevel}] Exception: {exception}");
                }
            }
        }
    }
}
