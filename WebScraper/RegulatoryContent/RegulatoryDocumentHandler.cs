using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using System.Linq;
using WebScraper.Interfaces;
using HtmlAgilityPack;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Document handler for regulatory content
    /// </summary>
    public class RegulatoryDocumentHandler : IDocumentHandler
    {
        private readonly Action<string> _logger;
        private readonly RegulatoryDocumentClassifier _contentClassifier;
        private readonly OfficeDocumentHandler _officeDocumentHandler;

        public RegulatoryDocumentHandler(Action<string> logger = null)
        {
            _logger = logger ?? (msg => { });
            _contentClassifier = new RegulatoryDocumentClassifier(_logger);
            _officeDocumentHandler = new OfficeDocumentHandler(_logger);
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
                    byte[] pdfBytes = await DownloadFileAsync(url);
                    // Use the extension method we created to handle byte[] parameter
                    var pdfHandler = new PdfDocumentHandler(_logger);
                    return await PdfDocumentHandlerExtension.ExtractTextAsync(pdfHandler, pdfBytes);
                }
                else if (url.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) ||
                        url.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) ||
                        url.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) ||
                        url.EndsWith(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    byte[] officeBytes = await DownloadFileAsync(url);
                    return GetExtractedText(officeBytes);
                }

                _logger($"No suitable handler found for URL: {url}");
                return string.Empty;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from {url}: {ex.Message}");
                return string.Empty;
            }
        }

        public async Task<bool> IsRegulatoryDocument(string url, string content)
        {
            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);
                return await IsRegulatoryDocument(url, htmlDoc);
            }
            catch (Exception ex)
            {
                _logger($"Error determining if document is regulatory: {ex.Message}");
                return false;
            }
        }

        // Add method to handle byte array parameter
        public async Task<bool> IsRegulatoryDocument(byte[] contentBytes)
        {
            if (contentBytes == null || contentBytes.Length == 0)
                return false;

            try
            {
                // Convert bytes to string for HTML content
                string content = System.Text.Encoding.UTF8.GetString(contentBytes);
                return await IsRegulatoryDocument("bytecontent://unknown", content);
            }
            catch (Exception ex)
            {
                _logger($"Error determining if byte content is regulatory: {ex.Message}");
                return false;
            }
        }

        public async Task<StructuredContentResult> ExtractStructuredContent(string url, string content)
        {
            var extractor = new StructuredContentExtractor(_logger);
            // Use the extension method we created to handle string content
            return await StructuredContentExtractorExtension.ExtractStructuredContent(extractor, url, content);
        }

        private async Task<byte[]> DownloadFileAsync(string url)
        {
            using (var httpClient = new System.Net.Http.HttpClient())
            {
                return await httpClient.GetByteArrayAsync(url);
            }
        }

        // Add missing method overloads for HTML document handling
        public async Task<bool> IsRegulatoryDocument(string url, HtmlDocument htmlDoc)
        {
            try
            {
                string htmlContent = null;
                string textContent = null;

                if (htmlDoc != null)
                {
                    // Extract content from the provided HTML document
                    textContent = htmlDoc.DocumentNode.InnerText;
                    htmlContent = htmlDoc.DocumentNode.OuterHtml;
                }
                else
                {
                    // Fetch the HTML content
                    using (var httpClient = new System.Net.Http.HttpClient())
                    {
                        htmlContent = await httpClient.GetStringAsync(url);

                        // Parse the HTML
                        htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(htmlContent);
                        textContent = htmlDoc.DocumentNode.InnerText;
                    }
                }

                // Classify the content
                var classification = _contentClassifier.ClassifyContent(url, textContent, htmlDoc);
                return classification.IsRegulatoryContent;
            }
            catch (Exception ex)
            {
                _logger($"Error determining if document is regulatory: {ex.Message}");
                return false;
            }
        }

        public string GetExtractedText(byte[] officeDocBytes)
        {
            try
            {
                // Create a temporary file to save the bytes
                string tempPath = Path.Combine(Path.GetTempPath(), $"tempoffice_{Guid.NewGuid()}.docx");
                File.WriteAllBytes(tempPath, officeDocBytes);

                // Process the file
                var result = _officeDocumentHandler.ExtractTextFromDocument(tempPath).GetAwaiter().GetResult();

                // Clean up the temp file
                try { File.Delete(tempPath); } catch { }

                return result;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from office document bytes: {ex.Message}");
                return string.Empty;
            }
        }

        // Fix type conversion issues in method calls
        public async Task<DocumentMetadata> ProcessDocumentContent(string contentData)
        {
            try
            {
                // Fix parameter type - convert Action<string> to string
                return await ProcessHtmlDocument(contentData);
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error processing document content: {ex.Message}");
                return null;
            }
        }

        public async Task<DocumentMetadata> ProcessDocumentFile(byte[] fileData)
        {
            try
            {
                // Fix parameter type - convert byte[] to string
                string contentAsString = System.Text.Encoding.UTF8.GetString(fileData);
                return await ProcessHtmlDocument(contentAsString);
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error processing document file: {ex.Message}");
                return null;
            }
        }

        // Method to handle HtmlDocument parameter type
        public async Task<DocumentMetadata> ProcessHtmlDocument(HtmlDocument htmlDoc)
        {
            try
            {
                // Extract basic metadata and text content
                string textContent = htmlDoc.DocumentNode.InnerText;
                
                // Classify the content
                var classification = _contentClassifier.ClassifyContent("file://document", textContent, htmlDoc);
                
                // Extract publication date if available
                DateTime? publishDate = null;
                var dateNodes = htmlDoc.DocumentNode.SelectNodes("//meta[@name='date' or @property='article:published_time']");
                if (dateNodes != null && dateNodes.Count > 0)
                {
                    var dateString = dateNodes[0].GetAttributeValue("content", null);
                    if (!string.IsNullOrEmpty(dateString) && DateTime.TryParse(dateString, out var pubDate))
                    {
                        publishDate = pubDate;
                    }
                }
                
                // Create metadata with all properties initialized at once
                var metadata = new DocumentMetadata
                {
                    Title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText ?? "Unknown Title",
                    ContentHash = ComputeContentHash(htmlDoc.DocumentNode.OuterHtml),
                    TextContent = textContent,
                    Classification = new ClassificationResult
                    {
                        PrimaryCategory = classification.PrimaryCategory ?? "Unknown",
                        Category = classification.Category ?? "Unknown",
                        Confidence = classification.ConfidenceScore,
                        Impact = (WebScraper.RegulatoryImpact)classification.Impact,
                        MatchedKeywords = classification.Topics?.ToList() ?? new List<string>()
                    },
                    PublishDate = publishDate,
                    PublicationDate = publishDate
                };

                return metadata;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error processing HTML document: {ex.Message}");
                return null;
            }
        }

        // Add overload for string content
        public async Task<DocumentMetadata> ProcessHtmlDocument(string htmlContent)
        {
            if (string.IsNullOrEmpty(htmlContent))
                return null;

            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);
                return await ProcessHtmlDocument(htmlDoc);
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error processing HTML content: {ex.Message}");
                return null;
            }
        }

        // Helper method to compute content hash
        private string ComputeContentHash(string content)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var contentBytes = System.Text.Encoding.UTF8.GetBytes(content);
                var hashBytes = sha256.ComputeHash(contentBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        // Add IsRegulatoryContent property to ClassificationResult
        public bool IsRegulatoryContent(RegulatoryDocumentClassifier.ClassificationResult result)
        {
            return result?.ConfidenceScore > 0.7 || (result?.Impact >= RegulatoryImpact.Medium);
        }

        // Add the missing method to OfficeDocumentHandler
        public Task<string> ExtractTextFromDocumentFile(string filePath)
        {
            // Forward the call to the correct method
            if (_officeDocumentHandler != null)
            {
                return _officeDocumentHandler.ExtractTextFromDocument(filePath);
            }

            return Task.FromResult<string>(null);
        }
    }

    public class StructuredContentResult
    {
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime? PublicationDate { get; set; }
        public string Summary { get; set; } = string.Empty;
        public string[] KeyPoints { get; set; } = Array.Empty<string>();
    }
}