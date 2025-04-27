using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Interface for components that process URLs
    /// </summary>
    public interface IUrlProcessor : IScraperComponent
    {
        /// <summary>
        /// Processes a URL
        /// </summary>
        /// <param name="url">The URL to process</param>
        Task ProcessUrlAsync(string url);
        
        /// <summary>
        /// Processes a batch of URLs
        /// </summary>
        /// <param name="urls">The URLs to process</param>
        Task ProcessUrlBatchAsync(IEnumerable<string> urls);
    }
}