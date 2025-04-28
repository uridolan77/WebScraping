using System;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.RegulatoryContent
{
    // Extension class for StructuredContentExtractor to add missing methods
    public static class StructuredContentExtractorExtension
    {
        // Add async version of ExtractStructuredContent that accepts string content
        public static async Task<StructuredContentResult> ExtractStructuredContent(this StructuredContentExtractor extractor, string url, string htmlContent)
        {
            try
            {
                // Parse HTML into document
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);
                
                // Extract structured content
                var contentSection = extractor.ExtractStructuredContentFromHtml(htmlContent, url);
                
                // Convert to StructuredContentResult
                var result = new StructuredContentResult
                {
                    Title = contentSection.Title ?? "Untitled",
                    Author = "Unknown", // Default value
                    PublicationDate = contentSection.PublishedDate,
                    Summary = GetSummaryFromContentSection(contentSection),
                    KeyPoints = GetKeyPointsFromContentSection(contentSection)
                };
                
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting structured content: {ex.Message}");
                
                // Return a minimal result on error
                return new StructuredContentResult
                {
                    Title = "Error extracting content",
                    Author = "Unknown",
                    Summary = $"Error: {ex.Message}",
                    KeyPoints = new string[0]
                };
            }
        }
        
        // Helper method to extract summary from content section
        private static string GetSummaryFromContentSection(StructuredContentExtractor.ContentSection contentSection)
        {
            if (contentSection.ContentNodes == null || contentSection.ContentNodes.Count == 0)
                return string.Empty;
                
            // Try to find the first paragraph
            foreach (var node in contentSection.ContentNodes)
            {
                if (node.Type == "paragraph" && !string.IsNullOrEmpty(node.Content))
                {
                    return node.Content;
                }
            }
            
            // Fallback to first content node
            return contentSection.ContentNodes[0].Content ?? string.Empty;
        }
        
        // Helper method to extract key points from content section
        private static string[] GetKeyPointsFromContentSection(StructuredContentExtractor.ContentSection contentSection)
        {
            if (contentSection.ContentNodes == null || contentSection.ContentNodes.Count == 0)
                return new string[0];
                
            var keyPoints = new System.Collections.Generic.List<string>();
            
            // Look for list items or headings
            foreach (var node in contentSection.ContentNodes)
            {
                if ((node.Type == "listItem" || node.Type == "heading") && !string.IsNullOrEmpty(node.Content))
                {
                    keyPoints.Add(node.Content);
                }
                
                // Limit to 5 key points
                if (keyPoints.Count >= 5)
                    break;
            }
            
            return keyPoints.ToArray();
        }
    }
}
