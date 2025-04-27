using System.Threading.Tasks;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Interface for handling different document types
    /// </summary>
    public interface IDocumentHandler
    {
        /// <summary>
        /// Check if this handler can process the specified URL
        /// </summary>
        bool CanHandle(string url);

        /// <summary>
        /// Extract text content from a document URL
        /// </summary>
        Task<string> ExtractTextFromUrl(string url);
    }
}