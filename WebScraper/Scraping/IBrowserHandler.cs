using System.Threading.Tasks;
using WebScraper.HeadlessBrowser;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Interface for components that handle browser automation
    /// </summary>
    public interface IBrowserHandler : IScraperComponent
    {
        /// <summary>
        /// Navigates to a URL using the headless browser
        /// </summary>
        /// <param name="url">The URL to navigate to</param>
        /// <returns>The result of the navigation</returns>
        Task<BrowserPageResult> NavigateToUrlAsync(string url);
    }
}