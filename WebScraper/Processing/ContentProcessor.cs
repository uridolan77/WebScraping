using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraper.Interfaces;
using WebScraper.RegulatoryContent;

namespace WebScraper.Processing
{
    /// <summary>
    /// Processes content from various sources
    /// </summary>
    public class ContentProcessor
    {
        private readonly Action<string> _logger;
        private readonly RegulatoryDocumentHandler _documentHandler;
        private readonly RegulatoryDocumentClassifier _classifier;

        // Fix constructor parameter types
        public ContentProcessor(Action<string> logger = null)
        {
            _logger = logger ?? (msg => { });
            _documentHandler = new RegulatoryDocumentHandler(_logger);
            _classifier = new RegulatoryDocumentClassifier(_logger);
        }

        /// <summary>
        /// Process HTML document and extract content
        /// </summary>
        public async Task<ProcessingResult<WebScraper.ContentItem>> ProcessHtmlAsync(HtmlDocument htmlDoc, string url)
        {
            try
            {
                _logger($"Processing HTML content from URL: {url}");

                // Extract text content
                string textContent = htmlDoc.DocumentNode.InnerText;

                // Create content item using the canonical implementation
                var contentItem = new WebScraper.ContentItem
                {
                    Url = url,
                    Title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? "Untitled",
                    ScraperId = "contentprocessor",
                    ContentType = "text/html",
                    IsReachable = true,
                    RawContent = htmlDoc.DocumentNode.OuterHtml,
                    TextContent = textContent,
                    ContentHash = ComputeHash(htmlDoc.DocumentNode.OuterHtml),
                    CapturedAt = DateTime.Now
                };

                // Determine if this is regulatory content
                var isRegulatory = await _documentHandler.IsRegulatoryDocument(url, htmlDoc);
                contentItem.IsRegulatoryContent = isRegulatory;

                return new ProcessingResult<WebScraper.ContentItem>
                {
                    ContentItem = contentItem,
                    Success = true
                };
            }
            catch (Exception ex)
            {
                _logger($"Error processing HTML content: {ex.Message}");
                return new ProcessingResult<WebScraper.ContentItem> { Success = false };
            }
        }

        /// <summary>
        /// Process HTML string and extract content
        /// </summary>
        public async Task<ProcessingResult<WebScraper.ContentItem>> ProcessHtmlStringAsync(string htmlContent, string url)
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                return await ProcessHtmlAsync(htmlDoc, url);
            }
            catch (Exception ex)
            {
                _logger($"Error processing HTML string: {ex.Message}");
                return new ProcessingResult<WebScraper.ContentItem> { Success = false };
            }
        }

        private string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    public class ProcessingResult<T>
    {
        public T ContentItem { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, object> Metrics { get; set; } = new Dictionary<string, object>();
    }
}