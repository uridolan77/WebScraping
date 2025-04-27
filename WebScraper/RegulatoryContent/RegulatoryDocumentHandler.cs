using System;
using System.Threading.Tasks;
using System.IO;
using WebScraper.Interfaces;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Document handler for regulatory content
    /// </summary>
    public class RegulatoryDocumentHandler : IDocumentHandler
    {
        private readonly PdfDocumentHandler _pdfHandler;
        private readonly OfficeDocumentHandler _officeHandler;
        private readonly Action<string> _logAction;
        private readonly RegulatoryDocumentClassifier _classifier;

        public RegulatoryDocumentHandler(Action<string> logAction)
        {
            _logAction = logAction ?? (msg => { });
            _pdfHandler = new PdfDocumentHandler(logAction);
            _officeHandler = new OfficeDocumentHandler(logAction);
            _classifier = new RegulatoryDocumentClassifier(logAction);
        }

        public bool CanHandle(string url)
        {
            var extension = Path.GetExtension(url).ToLowerInvariant();
            return extension == ".pdf" 
                || extension == ".doc" 
                || extension == ".docx" 
                || extension == ".xlsx" 
                || extension == ".xls";
        }

        public async Task<string> ExtractTextFromUrl(string url)
        {
            try
            {
                if (url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return await _pdfHandler.ExtractTextAsync(url);
                }
                else if (url.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) || 
                        url.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                        url.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                        url.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    return await _officeHandler.ExtractTextAsync(url);
                }
                
                _logAction($"No suitable handler found for URL: {url}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logAction($"Error extracting text from {url}: {ex.Message}");
                return string.Empty;
            }
        }
        
        public async Task<bool> IsRegulatoryDocument(string url, string content)
        {
            return await _classifier.IsRegulatoryDocument(url, content);
        }
        
        public async Task<StructuredContentResult> ExtractStructuredContent(string url, string content)
        {
            var extractor = new StructuredContentExtractor(_logAction);
            return await extractor.ExtractStructuredContent(url, content);
        }
    }
    
    public class StructuredContentResult
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? PublicationDate { get; set; }
        public string Summary { get; set; }
        public string[] KeyPoints { get; set; }
    }
}