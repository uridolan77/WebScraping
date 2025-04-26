using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.Extensions.Logging;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Implementation
{
    /// <summary>
    /// Processes documents like PDFs, Office files, etc.
    /// </summary>
    public class DocumentProcessor : IDocumentProcessor
    {
        private readonly DocumentProcessingConfig _config;
        private readonly ILogger<DocumentProcessor> _logger;
        private readonly HttpClient _httpClient;
        private readonly Dictionary<string, Func<byte[], Task<string>>> _extractors;
        
        public DocumentProcessor(DocumentProcessingConfig config, ILogger<DocumentProcessor> logger)
        {
            _config = config;
            _logger = logger;
            
            // Initialize HTTP client
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 WebScraper Document Processor/1.0");
            
            // Ensure storage directory exists
            if (!Directory.Exists(_config.DocumentStoragePath))
            {
                Directory.CreateDirectory(_config.DocumentStoragePath);
            }
            
            // Set up content extractors for different document types
            _extractors = new Dictionary<string, Func<byte[], Task<string>>>(StringComparer.OrdinalIgnoreCase)
            {
                [".pdf"] = ExtractPdfTextAsync,
                [".docx"] = ExtractDocxTextAsync,
                [".xlsx"] = ExtractXlsxTextAsync
            };
        }
        
        /// <summary>
        /// Processes a document and extracts metadata and content
        /// </summary>
        public async Task<DocumentMetadata> ProcessDocumentAsync(string url, string title, byte[] content)
        {
            try
            {
                string extension = Path.GetExtension(url);
                
                // Ensure this is a supported document type
                if (!_config.DocumentTypes.Contains(extension, StringComparer.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("Unsupported document type: {Extension} for {Url}", extension, url);
                    return null;
                }
                
                // Create document metadata
                var metadata = new DocumentMetadata
                {
                    Url = url,
                    Title = title ?? Path.GetFileNameWithoutExtension(url),
                    DocumentType = extension.TrimStart('.').ToUpper(),
                    ProcessedDate = DateTime.Now
                };
                
                // Save document to disk if enabled
                if (_config.DownloadDocuments)
                {
                    var localPath = await SaveDocumentAsync(url, content);
                    metadata.LocalFilePath = localPath;
                }
                
                // Extract metadata if enabled
                if (_config.ExtractMetadata)
                {
                    await ExtractDocumentMetadataAsync(metadata, content);
                }
                
                // Extract full text if enabled
                if (_config.ExtractFullText && _extractors.TryGetValue(extension, out var extractor))
                {
                    var text = await extractor(content);
                    metadata.ExtractedMetadata["FullText"] = text;
                    
                    // Apply metadata extraction patterns
                    foreach (var pattern in _config.MetadataPatterns)
                    {
                        var matches = Regex.Matches(text, pattern.Value);
                        if (matches.Count > 0 && matches[0].Groups.Count > 1)
                        {
                            metadata.ExtractedMetadata[pattern.Key] = matches[0].Groups[1].Value.Trim();
                        }
                    }
                    
                    // Try to extract effective date from patterns
                    if (metadata.ExtractedMetadata.TryGetValue("EffectiveDate", out var effectiveDateStr))
                    {
                        if (DateTime.TryParse(effectiveDateStr, out var effectiveDate))
                        {
                            metadata.PublishDate = effectiveDate;
                        }
                    }
                }
                
                _logger.LogInformation("Processed document: {Url}, Type: {DocumentType}", url, metadata.DocumentType);
                
                return metadata;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing document: {Url}", url);
                return null;
            }
        }
        
        /// <summary>
        /// Processes all linked documents in an HTML page
        /// </summary>
        public async Task ProcessLinkedDocumentsAsync(string pageUrl, HtmlDocument document)
        {
            try
            {
                var baseUri = new Uri(pageUrl);
                var links = document.DocumentNode.SelectNodes("//a[@href]");
                
                if (links == null)
                {
                    return;
                }
                
                var documentLinks = links
                    .Select(a => a.GetAttributeValue("href", ""))
                    .Where(href => !string.IsNullOrEmpty(href))
                    .Select(href => NormalizeUrl(href, baseUri))
                    .Where(url => _config.DocumentTypes.Any(ext => url.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                    .Distinct()
                    .ToList();
                
                _logger.LogInformation("Found {Count} document links on {Url}", documentLinks.Count, pageUrl);
                
                // Process each document link
                foreach (var url in documentLinks)
                {
                    try
                    {
                        // Get document title from link text
                        var linkNode = links.FirstOrDefault(a => 
                            NormalizeUrl(a.GetAttributeValue("href", ""), baseUri) == url);
                        
                        string title = linkNode?.InnerText.Trim() ?? Path.GetFileNameWithoutExtension(url);
                        
                        // Download document
                        var content = await _httpClient.GetByteArrayAsync(url);
                        
                        // Skip if file is too large
                        if (content.Length > _config.MaxFileSize)
                        {
                            _logger.LogWarning(
                                "Document too large ({Size} bytes) - skipping: {Url}", 
                                content.Length, url);
                            continue;
                        }
                        
                        // Process document
                        await ProcessDocumentAsync(url, title, content);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing linked document: {Url}", url);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing linked documents for page: {Url}", pageUrl);
            }
        }
        
        /// <summary>
        /// Saves a document to disk
        /// </summary>
        private async Task<string> SaveDocumentAsync(string url, byte[] content)
        {
            try
            {
                string filename = GetSafeFilenameFromUrl(url);
                string localPath = Path.Combine(_config.DocumentStoragePath, filename);
                
                await File.WriteAllBytesAsync(localPath, content);
                
                _logger.LogInformation("Saved document to {Path}", localPath);
                
                return localPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving document: {Url}", url);
                return null;
            }
        }
        
        /// <summary>
        /// Extracts metadata from a document
        /// </summary>
        private async Task ExtractDocumentMetadataAsync(DocumentMetadata metadata, byte[] content)
        {
            string extension = Path.GetExtension(metadata.Url);
            
            try
            {
                switch (extension.ToLower())
                {
                    case ".pdf":
                        await ExtractPdfMetadataAsync(metadata, content);
                        break;
                    case ".docx":
                        ExtractDocxMetadata(metadata, content);
                        break;
                    case ".xlsx":
                        ExtractXlsxMetadata(metadata, content);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from document: {Url}", metadata.Url);
            }
        }
        
        /// <summary>
        /// Extracts text from a PDF document
        /// </summary>
        private async Task<string> ExtractPdfTextAsync(byte[] content)
        {
            try
            {
                using var memoryStream = new MemoryStream(content);
                using var pdfReader = new PdfReader(memoryStream);
                using var pdfDocument = new PdfDocument(pdfReader);
                
                var text = new System.Text.StringBuilder();
                
                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var strategy = new SimpleTextExtractionStrategy();
                    var pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                    
                    text.AppendLine($"--- Page {i} ---");
                    text.AppendLine(pageText);
                    text.AppendLine();
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text from PDF");
                return "[PDF TEXT EXTRACTION ERROR]";
            }
        }
        
        /// <summary>
        /// Extracts metadata from a PDF document
        /// </summary>
        private async Task ExtractPdfMetadataAsync(DocumentMetadata metadata, byte[] content)
        {
            try
            {
                using var memoryStream = new MemoryStream(content);
                using var pdfReader = new PdfReader(memoryStream);
                using var pdfDocument = new PdfDocument(pdfReader);
                
                // Get document info
                var info = pdfDocument.GetDocumentInfo();
                
                // Set page count
                metadata.PageCount = pdfDocument.GetNumberOfPages();
                
                // Extract metadata fields
                if (info.GetTitle() != null)
                {
                    metadata.ExtractedMetadata["Title"] = info.GetTitle();
                    
                    // If no title was provided, use the one from the PDF
                    if (string.IsNullOrEmpty(metadata.Title))
                    {
                        metadata.Title = info.GetTitle();
                    }
                }
                
                if (info.GetAuthor() != null)
                {
                    metadata.ExtractedMetadata["Author"] = info.GetAuthor();
                }
                
                if (info.GetSubject() != null)
                {
                    metadata.ExtractedMetadata["Subject"] = info.GetSubject();
                }
                
                if (info.GetKeywords() != null)
                {
                    metadata.ExtractedMetadata["Keywords"] = info.GetKeywords();
                }
                
                if (info.GetCreator() != null)
                {
                    metadata.ExtractedMetadata["Creator"] = info.GetCreator();
                }
                
                if (info.GetProducer() != null)
                {
                    metadata.ExtractedMetadata["Producer"] = info.GetProducer();
                }
                
                if (info.GetCreationDate() != null)
                {
                    var creationDate = info.GetCreationDate();
                    if (creationDate != null)
                    {
                        metadata.ExtractedMetadata["CreationDate"] = creationDate.ToString();
                        
                        // If no publish date was found, use creation date
                        if (!metadata.PublishDate.HasValue)
                        {
                            metadata.PublishDate = creationDate.GetTime();
                        }
                    }
                }
                
                _logger.LogInformation("Extracted metadata from PDF: {Url}", metadata.Url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting metadata from PDF: {Url}", metadata.Url);
            }
        }
        
        /// <summary>
        /// Extracts text from a DOCX document (simplified implementation)
        /// </summary>
        private async Task<string> ExtractDocxTextAsync(byte[] content)
        {
            // In a real implementation, use DocumentFormat.OpenXml to extract text
            return "[DOCX TEXT EXTRACTION NOT IMPLEMENTED]";
        }
        
        /// <summary>
        /// Extracts metadata from a DOCX document (simplified implementation)
        /// </summary>
        private void ExtractDocxMetadata(DocumentMetadata metadata, byte[] content)
        {
            // In a real implementation, use DocumentFormat.OpenXml to extract metadata
            metadata.ExtractedMetadata["Format"] = "DOCX";
        }
        
        /// <summary>
        /// Extracts text from an XLSX document (simplified implementation)
        /// </summary>
        private async Task<string> ExtractXlsxTextAsync(byte[] content)
        {
            // In a real implementation, use DocumentFormat.OpenXml to extract text
            return "[XLSX TEXT EXTRACTION NOT IMPLEMENTED]";
        }
        
        /// <summary>
        /// Extracts metadata from an XLSX document (simplified implementation)
        /// </summary>
        private void ExtractXlsxMetadata(DocumentMetadata metadata, byte[] content)
        {
            // In a real implementation, use DocumentFormat.OpenXml to extract metadata
            metadata.ExtractedMetadata["Format"] = "XLSX";
        }
        
        /// <summary>
        /// Gets a safe filename from a URL
        /// </summary>
        private string GetSafeFilenameFromUrl(string url)
        {
            try
            {
                // Extract filename from URL
                string filename = Path.GetFileName(new Uri(url).LocalPath);
                
                // Fallback if no filename can be determined
                if (string.IsNullOrWhiteSpace(filename))
                {
                    var extension = Path.GetExtension(url);
                    filename = $"{Guid.NewGuid()}{extension}";
                }
                
                // Replace invalid characters with underscores
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    filename = filename.Replace(c, '_');
                }
                
                return filename;
            }
            catch
            {
                // If all else fails, create a unique filename with the URL's hash
                string extension = Path.GetExtension(url);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".bin";
                }
                
                return $"{url.GetHashCode():X8}{extension}";
            }
        }
        
        /// <summary>
        /// Normalizes a URL to an absolute URL
        /// </summary>
        private string NormalizeUrl(string href, Uri baseUri)
        {
            try
            {
                var uri = new Uri(baseUri, href);
                return uri.AbsoluteUri;
            }
            catch
            {
                return href;
            }
        }
    }
}