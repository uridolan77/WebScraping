using System.Threading.Tasks;
using WebScraper.Processing.Models;

namespace WebScraper.Processing.Interfaces
{
    /// <summary>
    /// Interface for analyzing text features
    /// </summary>
    public interface ITextAnalyzer
    {
        /// <summary>
        /// Analyzes text and extracts features
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Text features</returns>
        Task<TextFeatures> AnalyzeAsync(string text);
    }
}
