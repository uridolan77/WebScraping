using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.Processing.Models;

namespace WebScraper.Processing.Interfaces
{
    /// <summary>
    /// Interface for recognizing entities in text
    /// </summary>
    public interface IEntityRecognizer
    {
        /// <summary>
        /// Recognizes entities in text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>List of recognized entities</returns>
        Task<IEnumerable<RecognizedEntity>> RecognizeEntitiesAsync(string text);
    }
}
