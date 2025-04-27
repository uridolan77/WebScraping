using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;
using HtmlAgilityPack;
using System.Net.Http;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Adapter class to make PdfDocumentHandler compatible with IDocumentProcessor interface
    /// </summary>
    public class DocumentProcessorAdapter : IDocumentProcessor
    {
        private readonly PdfDocumentHandler _pdfHandler;
        
        public DocumentProcessorAdapter(PdfDocumentHandler pdfHandler)
        {
            _pdfHandler = pdfHandler ?? throw new ArgumentNullException(nameof(pdfHandler));
        }
        
        public async Task<string> ProcessDocumentAsync(string documentUrl)
        {
            // Call the appropriate method in PdfDocumentHandler
            return await _pdfHandler.ExtractPdfText(documentUrl);
        }
        
        // Fix the return type to match the interface
        public async Task<WebScraper.RegulatoryFramework.Interfaces.DocumentMetadata> ProcessDocumentAsync(string documentUrl, string documentType, byte[] content)
        {
            // If content is provided, we could potentially process it directly
            var text = await ProcessDocumentAsync(documentUrl);
            
            // Return document metadata with the extracted text using the correct type
            return new WebScraper.RegulatoryFramework.Interfaces.DocumentMetadata
            {
                Url = documentUrl,
                DocumentType = documentType ?? "pdf",
                ExtractedMetadata = new Dictionary<string, string>
                {
                    ["FullText"] = text
                },
                ProcessedDate = DateTime.UtcNow,
                Title = Path.GetFileNameWithoutExtension(documentUrl)
            };
        }
        
        // Fix the return type to match the interface
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
                            
                        // Check if it's a PDF link
                        if (href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                        {
                            // Convert relative URLs to absolute
                            string absoluteUrl = href;
                            if (!href.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                            {
                                // Handle relative paths
                                if (href.StartsWith("/"))
                                {
                                    // Absolute path from domain root
                                    Uri baseUri = new Uri(baseUrl);
                                    Uri absoluteUri = new Uri(baseUri.Scheme + "://" + baseUri.Host + href);
                                    absoluteUrl = absoluteUri.ToString();
                                }
                                else
                                {
                                    // Relative path from current URL
                                    Uri baseUri = new Uri(baseUrl);
                                    Uri absoluteUri = new Uri(baseUri, href);
                                    absoluteUrl = absoluteUri.ToString();
                                }
                            }
                            
                            // Process the PDF
                            await ProcessDocumentAsync(absoluteUrl);
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
        
        public bool CanProcessDocument(string documentUrl)
        {
            // Simple check to determine if this can process the document
            return documentUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
        }
        
        public void Dispose()
        {
            // PdfDocumentHandler doesn't have Dispose method, but if it did we would call it here
        }
    }
}