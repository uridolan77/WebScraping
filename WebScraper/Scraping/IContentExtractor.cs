using System.Threading.Tasks;

namespace WebScraper.Scraping
{
    /// <summary>
    /// Interface for components that extract content from HTML
    /// </summary>
    public interface IContentExtractor : IScraperComponent
    {
        /// <summary>
        /// Extracts text content from HTML
        /// </summary>
        /// <param name="html">The HTML content</param>
        /// <returns>The extracted text</returns>
        Task<string> ExtractTextContentAsync(string html);
        
        /// <summary>
        /// Extracts structured content from HTML
        /// </summary>
        /// <param name="html">The HTML content</param>
        /// <returns>The structured content</returns>
        Task<object> ExtractStructuredContentAsync(string html);
    }
}