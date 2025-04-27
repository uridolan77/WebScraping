using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Base class for scraper components
    /// </summary>
    public abstract class ScraperComponentBase : IScraperComponent
    {
        /// <summary>
        /// The scraper core instance
        /// </summary>
        protected ScraperCore Core { get; private set; }
        
        /// <summary>
        /// The scraper configuration
        /// </summary>
        protected ScraperConfig Config => Core?.Config;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        /// <param name="core">The scraper core instance</param>
        public virtual Task InitializeAsync(ScraperCore core)
        {
            Core = core ?? throw new ArgumentNullException(nameof(core));
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public virtual Task OnScrapingStartedAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public virtual Task OnScrapingCompletedAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        public virtual Task OnScrapingStoppedAsync()
        {
            return Task.CompletedTask;
        }
        
        /// <summary>
        /// Logs an information message
        /// </summary>
        protected void LogInfo(string message)
        {
            Core?.Log(message);
        }
        
        /// <summary>
        /// Logs an error
        /// </summary>
        protected void LogError(Exception ex, string message)
        {
            Core?.LogError(ex, message);
        }
        
        /// <summary>
        /// Logs a warning
        /// </summary>
        protected void LogWarning(string message)
        {
            Core?.LogWarning(message);
        }
        
        /// <summary>
        /// Gets a component of a specific type
        /// </summary>
        protected T GetComponent<T>() where T : class, IScraperComponent
        {
            return Core?.GetComponent<T>();
        }
    }
}