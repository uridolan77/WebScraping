using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating document processing components
    /// </summary>
    public class DocumentProcessingFactory
    {
        private readonly ILogger<DocumentProcessingFactory> _logger;

        public DocumentProcessingFactory(ILogger<DocumentProcessingFactory> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Creates a document processor based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A document processor implementation or null if not needed</returns>
        public IDocumentProcessor? CreateDocumentProcessor(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                if (!config.EnableDocumentProcessing)
                {
                    logAction("Document processing disabled, skipping document processor creation");
                    return null;
                }

                logAction("Creating default document processor");
                return new DefaultDocumentProcessor(config, _logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating document processor");
                logAction($"Error creating document processor: {ex.Message}");
                return null;
            }
        }

        // Internal implementation for basic document processing
        private class DefaultDocumentProcessor : IDocumentProcessor
        {
            private readonly ScraperConfig _config;
            private readonly ILogger _logger;

            public DefaultDocumentProcessor(ScraperConfig config, ILogger logger)
            {
                _config = config;
                _logger = logger;
            }

            public string ProcessDocument(string content, Uri url)
            {
                // Simple implementation - just returns the content unmodified
                return content;
            }

            // Fix return type to match the interface's expected type
            public async Task<WebScraper.RegulatoryFramework.Interfaces.DocumentMetadata> ProcessDocumentAsync(string url, string contentType, byte[] documentData)
            {
                try
                {
                    // Simple implementation
                    await Task.Delay(1); // Make it truly async
                    
                    var metadata = new WebScraper.RegulatoryFramework.Interfaces.DocumentMetadata
                    {
                        Url = url ?? string.Empty,
                        ContentType = contentType ?? string.Empty,
                        Size = documentData?.Length ?? 0,
                        ProcessedDate = DateTime.UtcNow,
                        DocumentType = string.Empty,
                        Content = string.Empty,
                        AdditionalMetadata = new Dictionary<string, string>()
                    };
                    
                    // Set content based on type
                    if (contentType?.Contains("text") == true || contentType?.Contains("html") == true)
                    {
                        metadata.DocumentType = "Text";
                        metadata.Content = documentData != null ? System.Text.Encoding.UTF8.GetString(documentData) : "";
                    }
                    else if (contentType?.Contains("pdf") == true)
                    {
                        metadata.DocumentType = "PDF";
                        metadata.Content = $"[PDF Content - {metadata.Size} bytes]";
                    }
                    else
                    {
                        metadata.DocumentType = "Binary";
                        metadata.Content = $"[Binary content of type {contentType}]";
                    }
                    
                    return metadata;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing document {url}");
                    
                    return new WebScraper.RegulatoryFramework.Interfaces.DocumentMetadata
                    {
                        Url = url ?? string.Empty,
                        ContentType = contentType ?? string.Empty,
                        Size = documentData?.Length ?? 0,
                        DocumentType = "Error",
                        ProcessedDate = DateTime.UtcNow,
                        Content = $"Error: {ex.Message}",
                        AdditionalMetadata = new Dictionary<string, string>()
                    };
                }
            }

            // Fix return type to match interface
            public async Task ProcessLinkedDocumentsAsync(string baseUrl, HtmlDocument document)
            {
                try
                {
                    // Simple implementation - find and log links to documents
                    if (document != null)
                    {
                        // Find all links to documents
                        var linkNodes = document.DocumentNode.SelectNodes("//a[@href]");
                        if (linkNodes != null)
                        {
                            var documentLinks = new List<string>();
                            
                            foreach (var linkNode in linkNodes)
                            {
                                var href = linkNode.GetAttributeValue("href", "");
                                if (!string.IsNullOrEmpty(href) && 
                                    (href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                     href.EndsWith(".doc", StringComparison.OrdinalIgnoreCase) ||
                                     href.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)))
                                {
                                    // Make the URL absolute if it's relative
                                    var absoluteUrl = href;
                                    if (!Uri.TryCreate(href, UriKind.Absolute, out _))
                                    {
                                        absoluteUrl = new Uri(new Uri(baseUrl), href).ToString();
                                    }
                                    
                                    documentLinks.Add(absoluteUrl);
                                }
                            }
                            
                            // Log the found documents
                            if (documentLinks.Count > 0)
                            {
                                _logger.LogInformation($"Found {documentLinks.Count} linked documents on {baseUrl}");
                                foreach (var link in documentLinks)
                                {
                                    _logger.LogInformation($"  Document link: {link}");
                                }
                            }
                        }
                    }
                    
                    await Task.Delay(1); // Make it truly async
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing linked documents for {baseUrl}");
                }
            }
        }
    }
}

// Define the DocumentMetadata class with initialized non-nullable properties
public class DocumentMetadata
{
    public DocumentMetadata()
    {
        // Initialize non-nullable properties with default values
        Url = string.Empty;
        ContentType = string.Empty;
        DocumentType = string.Empty;
        Content = string.Empty;
        AdditionalMetadata = new Dictionary<string, string>();
    }
    
    public string Url { get; set; }
    public string ContentType { get; set; }
    public string DocumentType { get; set; }
    public long Size { get; set; }
    public DateTime ProcessedDate { get; set; }
    public string Content { get; set; }
    public Dictionary<string, string> AdditionalMetadata { get; set; }
}