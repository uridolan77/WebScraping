using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.StateManagement;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Core scraper class that coordinates all scraping activities
    /// </summary>
    public class ScraperCore : IDisposable
    {
        private readonly ScraperConfig _config;
        private readonly ILogger _logger;
        private readonly List<IScraperComponent> _components = new List<IScraperComponent>();
        private bool _isInitialized = false;
        private bool _isDisposed = false;
        private CancellationTokenSource _cancellationTokenSource;

        /// <summary>
        /// Gets the configuration for this scraper
        /// </summary>
        public ScraperConfig Config => _config;

        /// <summary>
        /// Initializes a new instance of the ScraperCore class
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logger">Logger for scraper events</param>
        public ScraperCore(ScraperConfig config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Adds a component to the scraper
        /// </summary>
        /// <param name="component">The component to add</param>
        public void AddComponent(IScraperComponent component)
        {
            if (_isInitialized)
                throw new InvalidOperationException("Cannot add components after initialization");

            _components.Add(component ?? throw new ArgumentNullException(nameof(component)));
            _logger.LogInformation($"Component {component.GetType().Name} added to scraper");
        }

        /// <summary>
        /// Initializes all components
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ScraperCore));

            if (_isInitialized)
                return;

            _logger.LogInformation("Initializing scraper components...");

            foreach (var component in _components)
            {
                try
                {
                    await component.InitializeAsync(this);
                    _logger.LogInformation($"Component {component.GetType().Name} initialized");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize component {component.GetType().Name}");
                    throw;
                }
            }

            _isInitialized = true;
            _logger.LogInformation("Scraper initialization completed");
        }

        /// <summary>
        /// Starts the scraping process
        /// </summary>
        public async Task StartScrapingAsync()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ScraperCore));

            if (!_isInitialized)
                await InitializeAsync();

            _logger.LogInformation("Starting scraping process...");
            
            // Create a new cancellation token source for this run
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Notify components that scraping is starting
            foreach (var component in _components)
            {
                try
                {
                    await component.OnScrapingStartedAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in component {component.GetType().Name} during scraping start");
                }
            }

            // Find URL processor component and start processing the start URL
            var urlProcessor = GetComponent<IUrlProcessor>();
            if (urlProcessor != null)
            {
                try
                {
                    await urlProcessor.ProcessUrlAsync(_config.StartUrl);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing start URL: {_config.StartUrl}");
                }
            }
            else
            {
                _logger.LogWarning("No URL processor component found, unable to process URLs");
            }

            // Notify components that scraping has completed
            foreach (var component in _components)
            {
                try
                {
                    await component.OnScrapingCompletedAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error in component {component.GetType().Name} during scraping completion");
                }
            }

            _logger.LogInformation("Scraping process completed");
        }

        /// <summary>
        /// Stops the scraping process
        /// </summary>
        public void StopScraping()
        {
            if (_isDisposed)
                return;

            _logger.LogInformation("Stopping scraping process...");
            
            try
            {
                _cancellationTokenSource?.Cancel();
                
                // Notify components that scraping is being stopped
                foreach (var component in _components)
                {
                    try
                    {
                        component.OnScrapingStoppedAsync().GetAwaiter().GetResult();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error in component {component.GetType().Name} during scraping stop");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraping process");
            }

            _logger.LogInformation("Scraping process stopped");
        }

        /// <summary>
        /// Gets a component of the specified type
        /// </summary>
        /// <typeparam name="T">The type of component to get</typeparam>
        /// <returns>The component, or null if not found</returns>
        public T GetComponent<T>() where T : class, IScraperComponent
        {
            foreach (var component in _components)
            {
                if (component is T typedComponent)
                {
                    return typedComponent;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the cancellation token for the current scraping operation
        /// </summary>
        public CancellationToken GetCancellationToken()
        {
            return _cancellationTokenSource?.Token ?? CancellationToken.None;
        }

        /// <summary>
        /// Logs a message
        /// </summary>
        /// <param name="message">The message to log</param>
        public void Log(string message)
        {
            _logger.LogInformation(message);
        }

        /// <summary>
        /// Logs an error
        /// </summary>
        /// <param name="ex">The exception</param>
        /// <param name="message">The error message</param>
        public void LogError(Exception ex, string message)
        {
            _logger.LogError(ex, message);
        }

        /// <summary>
        /// Logs a warning
        /// </summary>
        /// <param name="message">The warning message</param>
        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
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
                // Cancel any ongoing operations
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();

                // Dispose components
                foreach (var component in _components)
                {
                    try
                    {
                        if (component is IDisposable disposable)
                        {
                            disposable.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error disposing component {component.GetType().Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing scraper");
            }

            _logger.LogInformation("Scraper disposed");
        }
    }
}