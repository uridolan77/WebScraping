using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;
using WebScraper.RegulatoryFramework.Implementation;
using HtmlAgilityPack;
using System.Net.Http;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Adapter class that implements IDocumentProcessor interface to handle multiple document types
    /// </summary>
    public class DocumentProcessorAdapter : IDocumentProcessor
    {
        private PdfDocumentHandler? _pdfHandler;
        private OfficeDocumentHandler? _officeHandler;

        public DocumentProcessorAdapter()
        {
            // Default constructor with no handlers - they can be registered later
        }

        /// <summary>
        /// Register a PDF document handler
        /// </summary>
        /// <param name="pdfHandler">The PDF document handler instance</param>
        public void RegisterPdfHandler(PdfDocumentHandler pdfHandler)
        {
            _pdfHandler = pdfHandler ?? throw new ArgumentNullException(nameof(pdfHandler));
        }

        /// <summary>
        /// Register an Office document handler
        /// </summary>
        /// <param name="officeHandler">The Office document handler instance</param>
        public void RegisterOfficeHandler(OfficeDocumentHandler officeHandler)
        {
            _officeHandler = officeHandler ?? throw new ArgumentNullException(nameof(officeHandler));
        }

        /// <summary>
        /// Process a document based on its URL
        /// </summary>
        /// <param name="documentUrl">URL of the document to process</param>
        /// <returns>Extracted text from the document</returns>
        public async Task<string> ProcessDocumentAsync(string documentUrl)
        {
            // Determine the document type and call the appropriate handler
            if (IsPdfDocument(documentUrl))
            {
                if (_pdfHandler == null)
                    throw new InvalidOperationException("PDF handler not registered but attempted to process PDF document");

                return await _pdfHandler.ExtractPdfText(documentUrl);
            }
            else if (IsOfficeDocument(documentUrl))
            {
                if (_officeHandler == null)
                    throw new InvalidOperationException("Office handler not registered but attempted to process Office document");

                return await _officeHandler.ExtractTextFromDocument(documentUrl);
            }

            throw new NotSupportedException($"Document format not supported for URL: {documentUrl}");
        }

        /// <summary>
        /// Process a document with provided content
        /// </summary>
        /// <param name="documentUrl">URL of the document</param>
        /// <param name="documentType">Type of document (pdf, docx, etc.)</param>
        /// <param name="content">Raw binary content of the document</param>
        /// <returns>Document metadata including extracted text</returns>
        public async Task<WebScraper.RegulatoryFramework.Implementation.DocumentMetadata> ProcessDocumentAsync(string documentUrl, string documentType, byte[] content)
        {
            // If content is provided directly, we would need to implement direct processing logic here
            // For now, we'll fall back to URL-based processing
            var text = await ProcessDocumentAsync(documentUrl);

            // Return document metadata with the extracted text
            return new WebScraper.RegulatoryFramework.Implementation.DocumentMetadata
            {
                Url = documentUrl,
                DocumentType = documentType ?? DetermineDocumentType(documentUrl),
                ExtractedMetadata = new Dictionary<string, object>
                {
                    ["FullText"] = text
                },
                ProcessedDate = DateTime.UtcNow,
                Title = Path.GetFileNameWithoutExtension(documentUrl),
                TextContent = text
            };
        }

        /// <summary>
        /// Process all document links in an HTML page
        /// </summary>
        /// <param name="baseUrl">Base URL of the page</param>
        /// <param name="document">HTML document containing links</param>
        public async Task ProcessLinkedDocumentsAsync(string baseUrl, HtmlDocument document)
        {
            try
            {
                // Find all links in the document
                var linkNodes = document?.DocumentNode?.SelectNodes("//a[@href]");
                if (linkNodes != null)
                {
                    foreach (var linkNode in linkNodes)
                    {
                        string href = linkNode.GetAttributeValue("href", "");

                        // Skip empty links
                        if (string.IsNullOrEmpty(href))
                            continue;

                        // Check if it's a supported document format
                        if (IsSupportedDocumentFormat(href))
                        {
                            // Convert relative URLs to absolute
                            string absoluteUrl = ConvertToAbsoluteUrl(href, baseUrl);

                            // Process the document
                            try {
                                await ProcessDocumentAsync(absoluteUrl);
                            }
                            catch (Exception ex) {
                                System.Diagnostics.Debug.WriteLine($"Error processing document {absoluteUrl}: {ex.Message}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log but don't throw to avoid breaking the scraping process
                System.Diagnostics.Debug.WriteLine($"Error processing linked documents: {ex.Message}");
            }
        }

        /// <summary>
        /// Check if this adapter can process a given document URL
        /// </summary>
        /// <param name="documentUrl">URL of the document</param>
        /// <returns>True if document can be processed, false otherwise</returns>
        public bool CanProcessDocument(string documentUrl)
        {
            return (IsPdfDocument(documentUrl) && _pdfHandler != null) ||
                   (IsOfficeDocument(documentUrl) && _officeHandler != null);
        }

        /// <summary>
        /// Dispose of any resources used by this adapter
        /// </summary>
        public void Dispose()
        {
            // Most handlers don't implement IDisposable, but call Dispose if they did
        }

        #region Helper Methods

        /// <summary>
        /// Determine if a URL points to a PDF document
        /// </summary>
        private bool IsPdfDocument(string url)
        {
            return url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Determine if a URL points to an Office document
        /// </summary>
        private bool IsOfficeDocument(string url)
        {
            var lowercaseUrl = url.ToLowerInvariant();
            return lowercaseUrl.EndsWith(".docx") || lowercaseUrl.EndsWith(".doc") ||
                   lowercaseUrl.EndsWith(".xlsx") || lowercaseUrl.EndsWith(".xls") ||
                   lowercaseUrl.EndsWith(".pptx") || lowercaseUrl.EndsWith(".ppt");
        }

        /// <summary>
        /// Check if the URL points to a supported document format
        /// </summary>
        private bool IsSupportedDocumentFormat(string url)
        {
            return IsPdfDocument(url) || IsOfficeDocument(url);
        }

        /// <summary>
        /// Determine the document type based on URL extension
        /// </summary>
        private string DetermineDocumentType(string url)
        {
            string extension = Path.GetExtension(url).ToLowerInvariant();

            if (extension == ".pdf") return "pdf";
            if (extension == ".docx" || extension == ".doc") return "word";
            if (extension == ".xlsx" || extension == ".xls") return "excel";
            if (extension == ".pptx" || extension == ".ppt") return "powerpoint";

            return "unknown";
        }

        /// <summary>
        /// Convert a relative URL to an absolute URL
        /// </summary>
        private string ConvertToAbsoluteUrl(string href, string baseUrl)
        {
            if (href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                return href;

            try {
                Uri baseUri = new Uri(baseUrl);

                if (href.StartsWith("/"))
                {
                    // Absolute path from domain root
                    Uri absoluteUri = new Uri(baseUri.Scheme + "://" + baseUri.Host + href);
                    return absoluteUri.ToString();
                }
                else
                {
                    // Relative path from current URL
                    Uri absoluteUri = new Uri(baseUri, href);
                    return absoluteUri.ToString();
                }
            }
            catch {
                // If URL parsing fails, return the original URL
                return href;
            }
        }

        #endregion
    }
}