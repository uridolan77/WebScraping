using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WebScraper;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Default implementation of IScraperFactory that creates Scraper instances
    /// </summary>
    public class DefaultScraperFactory : IScraperFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public DefaultScraperFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>
        /// Creates a new Scraper instance with the specified configuration
        /// </summary>
        public WebScraper.Scraper CreateScraper(ScraperConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            
            var logger = _loggerFactory.CreateLogger($"WebScraper.Scraper.{config.Name}");
            return new WebScraper.Scraper(config, logger);
        }

        /// <summary>
        /// Creates a new Scraper instance with a default configuration
        /// </summary>
        public WebScraper.Scraper CreateDefaultScraper()
        {
            var config = new ScraperConfig
            {
                StartUrl = "https://default-placeholder-url.com",
                Name = "Default Scraper Instance",
                MaxDepth = 5,
                MaxConcurrentRequests = 3,
                DelayBetweenRequests = 1000,
                EnablePersistentState = true,
                OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData")
            };
            
            var logger = _loggerFactory.CreateLogger("WebScraper.Scraper.Default");
            return new WebScraper.Scraper(config, logger);
        }

        /// <summary>
        /// Creates a new Scraper instance for the specified URL
        /// </summary>
        public WebScraper.Scraper CreateScraperForUrl(string startUrl)
        {
            if (string.IsNullOrEmpty(startUrl)) throw new ArgumentException("Start URL cannot be null or empty", nameof(startUrl));
            
            var config = new ScraperConfig
            {
                StartUrl = startUrl,
                Name = $"URL Scraper - {Path.GetFileNameWithoutExtension(startUrl)}",
                MaxDepth = 3,
                MaxConcurrentRequests = 2,
                DelayBetweenRequests = 1000,
                EnablePersistentState = true,
                OutputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData", 
                    $"url-{DateTime.Now:yyyyMMdd-HHmmss}")
            };
            
            var logger = _loggerFactory.CreateLogger($"WebScraper.Scraper.URL.{Path.GetFileNameWithoutExtension(startUrl)}");
            return new WebScraper.Scraper(config, logger);
        }
    }
}