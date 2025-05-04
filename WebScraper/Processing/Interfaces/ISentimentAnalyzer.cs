using System.Threading.Tasks;
using WebScraper.Processing.Models;

namespace WebScraper.Processing.Interfaces
{
    /// <summary>
    /// Interface for analyzing sentiment in text
    /// </summary>
    public interface ISentimentAnalyzer
    {
        /// <summary>
        /// Analyzes sentiment in text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Sentiment analysis results</returns>
        Task<SentimentResult> AnalyzeSentimentAsync(string text);
    }
}
