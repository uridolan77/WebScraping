using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WebScraper;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Interface for creating Scraper instances
    /// </summary>
    public interface IScraperFactory
    {
        /// <summary>
        /// Creates a new Scraper instance with the specified configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <returns>A new Scraper instance</returns>
        WebScraper.Scraper CreateScraper(ScraperConfig config);
        
        /// <summary>
        /// Creates a new Scraper instance with a default configuration
        /// </summary>
        /// <returns>A new Scraper instance with default configuration</returns>
        WebScraper.Scraper CreateDefaultScraper();
        
        /// <summary>
        /// Creates a new Scraper instance for the specified URL
        /// </summary>
        /// <param name="startUrl">The starting URL for the scraper</param>
        /// <returns>A new Scraper instance configured for the specified URL</returns>
        WebScraper.Scraper CreateScraperForUrl(string startUrl);
    }
}