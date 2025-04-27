using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Scraping;
using WebScraper.Scraping.Components;

namespace WebScraper
{
    /// <summary>
    /// Enhanced scraper implementation that uses the modular component architecture
    /// while maintaining backward compatibility with the original EnhancedScraper
    /// </summary>
    public class ModularEnhancedScraper : IDisposable
    {
        private readonly ScraperCore _scraperCore;
        private readonly ILogger _logger;
        private readonly ScraperConfig _config;
        private bool _isDisposed;

        /// <summary>
        /// Initializes a new instance of the ModularEnhancedScraper class
        /// </summary>
        /// <param name="config">Scraper configuration</param>
        /// <param name="logger">Logger for the scraper</param>
        public ModularEnhancedScraper(ScraperConfig config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _scraperCore = new ScraperCore(config, logger);
            
            // Register components based on configuration
            RegisterComponents();
        }

        /// <summary>
        /// Initializes a new instance of the ModularEnhancedScraper class with a log action
        /// </summary>
        /// <param name="config">Scraper configuration</param>
        /// <param name="logAction">Action to log messages</param>
        public ModularEnhancedScraper(ScraperConfig config, Action<string> logAction)
            : this(config, new ActionLogger(logAction ?? (msg => Console.WriteLine(msg))))
        {
        }

        /// <summary>
        /// Registers components with the scraper core based on configuration
        /// </summary>
        private void RegisterComponents()
        {
            // Always add the URL processor
            _scraperCore.AddComponent(new HttpUrlProcessor());

            // Add state manager if persistent state is enabled
            if (_config.EnablePersistentState)
            {
                _scraperCore.AddComponent(new StateManagerComponent());
            }

            // Add headless browser if needed
            if (_config.ProcessJsHeavyPages || _config.IsUKGCWebsite)
            {
                _scraperCore.AddComponent(new HeadlessBrowserComponent());
            }

            // Add document processor if needed
            if (_config.ProcessPdfDocuments || _config.ProcessOfficeDocuments)
            {
                _scraperCore.AddComponent(new DocumentProcessingComponent());
            }

            // Add change detector if enabled
            if (_config.EnableChangeDetection)
            {
                _scraperCore.AddComponent(new ChangeDetectionComponent());
            }
            
            // Add regulatory content component if enabled
            if (_config.EnableRegulatoryContentAnalysis)
            {
                _scraperCore.AddComponent(new RegulatoryContentComponent());
            }
        }

        /// <summary>
        /// Initializes the scraper
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                await _scraperCore.InitializeAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing scraper");
                throw;
            }
        }

        /// <summary>
        /// Starts scraping
        /// </summary>
        public async Task StartScrapingAsync()
        {
            try
            {
                await _scraperCore.StartScrapingAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during scraping");
                throw;
            }
        }

        /// <summary>
        /// Stops scraping
        /// </summary>
        public void StopScraping()
        {
            try
            {
                _scraperCore.StopScraping();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraper");
            }
        }
        
        /// <summary>
        /// Sets up continuous scraping with the specified interval
        /// </summary>
        /// <param name="interval">The interval between scraping runs</param>
        /// <param name="cancellationToken">Token to cancel the continuous scraping</param>
        public async Task SetupContinuousScrapingAsync(TimeSpan interval, System.Threading.CancellationToken? cancellationToken = null)
        {
            _logger.LogInformation($"Setting up continuous scraping with interval of {interval.TotalMinutes} minutes");
            
            var token = cancellationToken ?? System.Threading.CancellationToken.None;
            
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
        /// Stops the continuous scraping
        /// </summary>
        public void StopContinuousScraping()
        {
            _logger.LogInformation("Stopping continuous scraping");
            StopScraping();
        }
        
        /// <summary>
        /// Processes a specific URL
        /// </summary>
        public async Task ProcessUrlAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                _logger.LogWarning("Attempted to process null or empty URL");
                return;
            }

            try
            {
                _logger.LogInformation($"Processing URL: {url}");
                var urlProcessor = _scraperCore.GetComponent<IUrlProcessor>();
                
                if (urlProcessor != null)
                {
                    await urlProcessor.ProcessUrlAsync(url);
                }
                else
                {
                    _logger.LogWarning("No URL processor component found");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing URL: {url}");
            }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _scraperCore.Dispose();
        }

        /// <summary>
        /// Simple logger that wraps a log action
        /// </summary>
        private class ActionLogger : ILogger
        {
            private readonly Action<string> _logAction;

            public ActionLogger(Action<string> logAction)
            {
                _logAction = logAction;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

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