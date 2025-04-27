using System.Threading.Tasks;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Interface for basic scraper functionality
    /// </summary>
    public interface IScraper
    {
        /// <summary>
        /// Initialize the scraper and its components
        /// </summary>
        Task InitializeAsync();

        /// <summary>
        /// Start the scraping process
        /// </summary>
        Task StartScrapingAsync();

        /// <summary>
        /// Process a specific URL
        /// </summary>
        Task ProcessUrlAsync(string url);
    }
}