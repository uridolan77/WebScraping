using System;
using System.Threading.Tasks;
using WebScraper.ContentChange;

namespace WebScraper.Interfaces
{
    /// <summary>
    /// Interface for processing content extracted from web pages
    /// </summary>
    public interface IContentProcessor
    {
        /// <summary>
        /// Process content from a web page
        /// </summary>
        /// <param name="url">The URL of the content</param>
        /// <param name="content">The content to process</param>
        /// <returns>The processed content result</returns>
        Task<ProcessedContentResult> ProcessContentAsync(string url, PageContent content);
        
        /// <summary>
        /// Detect changes between two versions of content
        /// </summary>
        /// <param name="previousVersion">The previous version of the content</param>
        /// <param name="currentVersion">The current version of the content</param>
        /// <returns>Details about the detected changes</returns>
        Task<SignificantChangesResult> DetectChangesAsync(ContentItem previousVersion, ContentItem currentVersion);
    }
    
    /// <summary>
    /// Result of content processing
    /// </summary>
    public class ProcessedContentResult
    {
        /// <summary>
        /// The content item after processing
        /// </summary>
        public ContentItem ContentItem { get; set; }
        
        /// <summary>
        /// Whether the content is relevant based on processing logic
        /// </summary>
        public bool IsRelevant { get; set; }
        
        /// <summary>
        /// Metrics related to the content processing
        /// </summary>
        public ContentProcessingMetrics Metrics { get; set; }
    }
    
    /// <summary>
    /// Metrics collected during content processing
    /// </summary>
    public class ContentProcessingMetrics
    {
        public int TextLength { get; set; }
        public int ProcessingTimeMs { get; set; }
        public int KeyTermsFound { get; set; }
    }
}