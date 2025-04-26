using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Handles downloading, extracting text, and managing metadata for PDF documents
    /// which are common on regulatory websites
    /// </summary>
    public class PdfDocumentHandler
    {
        private readonly HttpClient _httpClient;
        private readonly string _storageDirectory;
        private readonly Action<string> _logger;
        private readonly Dictionary<string, PdfMetadata> _pdfMetadata = new Dictionary<string, PdfMetadata>();

        public PdfDocumentHandler(string storageDirectory = null, Action<string> logger = null)
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 WebScraper PDF Handler/1.0");
            
            _storageDirectory = storageDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PdfDocuments");
            _logger = logger ?? (_ => { });
            
            // Ensure storage directory exists
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }
            
            // Load any existing metadata
            LoadPdfMetadata();
        }
        
        /// <summary>
        /// Information about a PDF document
        /// </summary>
        public class PdfMetadata
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public string LocalFilePath { get; set; }
            public DateTime? PublishDate { get; set; }
            public string Category { get; set; }
            public DateTime DownloadDate { get; set; } = DateTime.Now;
            public long FileSizeBytes { get; set; }
            public int PageCount { get; set; }
            public string DocumentType { get; set; }
            public Dictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();
        }
        
        /// <summary>
        /// Downloads a PDF and extracts its text content
        /// </summary>
        public async Task<string> ExtractPdfText(string pdfUrl)
        {
            try
            {
                // Download the PDF if not already downloaded
                var localPath = await DownloadPdfIfNeeded(pdfUrl);
                
                // Extract text using PdfPig
                string extractedText = await ExtractTextFromPdfFile(localPath);
                
                _logger($"Extracted {extractedText.Length} characters from PDF: {pdfUrl}");
                
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from PDF {pdfUrl}: {ex.Message}");
                return $"[PDF EXTRACTION ERROR: {ex.Message}]";
            }
        }
        
        /// <summary>
        /// Saves metadata about a PDF document
        /// </summary>
        public async Task SavePdfMetadata(string pdfUrl, string title, DateTime? publishDate = null, string category = null, Dictionary<string, string> additionalMetadata = null)
        {
            try
            {
                var localPath = await DownloadPdfIfNeeded(pdfUrl);
                
                // Get or create the metadata object
                if (!_pdfMetadata.TryGetValue(pdfUrl, out var metadata))
                {
                    var fileInfo = new FileInfo(localPath);
                    
                    metadata = new PdfMetadata
                    {
                        Url = pdfUrl,
                        LocalFilePath = localPath,
                        DownloadDate = DateTime.Now,
                        FileSizeBytes = fileInfo.Length
                    };
                    
                    _pdfMetadata[pdfUrl] = metadata;
                }
                
                // Update metadata properties
                metadata.Title = title;
                metadata.PublishDate = publishDate;
                metadata.Category = category;
                
                if (additionalMetadata != null)
                {
                    foreach (var kvp in additionalMetadata)
                    {
                        metadata.AdditionalMetadata[kvp.Key] = kvp.Value;
                    }
                }
                
                // Try to extract page count
                await TryExtractPdfProperties(metadata);
                
                // Persist metadata to disk
                SavePdfMetadataToDisk();
                
                _logger($"Saved metadata for PDF: {pdfUrl}");
            }
            catch (Exception ex)
            {
                _logger($"Error saving PDF metadata for {pdfUrl}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Gets all PDF documents metadata
        /// </summary>
        public Dictionary<string, PdfMetadata> GetAllPdfMetadata()
        {
            return new Dictionary<string, PdfMetadata>(_pdfMetadata);
        }
        
        /// <summary>
        /// Gets metadata for a specific PDF document
        /// </summary>
        public PdfMetadata GetPdfMetadata(string pdfUrl)
        {
            _pdfMetadata.TryGetValue(pdfUrl, out var metadata);
            return metadata;
        }
        
        /// <summary>
        /// Searches through all PDF documents for a specific term
        /// </summary>
        public async Task<Dictionary<string, string>> SearchPdfsForTerm(string searchTerm, bool caseSensitive = false)
        {
            var results = new Dictionary<string, string>();
            var regexOptions = caseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
            var searchRegex = new Regex($"(?:^|\\s|[.,:;])({Regex.Escape(searchTerm)})(?:\\s|[.,:;]|$)", regexOptions);
            
            foreach (var pdf in _pdfMetadata.Values)
            {
                try
                {
                    var text = await ExtractPdfText(pdf.Url);
                    var matches = searchRegex.Matches(text);
                    
                    if (matches.Count > 0)
                    {
                        // Extract a snippet around the first match for context
                        var match = matches[0];
                        int start = Math.Max(0, match.Index - 50);
                        int length = Math.Min(text.Length - start, match.Index + match.Length + 50 - start);
                        
                        string context = text.Substring(start, length);
                        results[pdf.Url] = $"{pdf.Title}: ...{context}...";
                    }
                }
                catch (Exception ex)
                {
                    _logger($"Error searching PDF {pdf.Url}: {ex.Message}");
                }
            }
            
            _logger($"Found {results.Count} PDFs containing term '{searchTerm}'");
            return results;
        }
        
        private async Task<string> DownloadPdfIfNeeded(string pdfUrl)
        {
            string filename = GetSafeFilenameFromUrl(pdfUrl);
            string localPath = Path.Combine(_storageDirectory, filename);
            
            // Download only if we don't have it already
            if (!File.Exists(localPath))
            {
                _logger($"Downloading PDF from {pdfUrl}");
                
                try
                {
                    var response = await _httpClient.GetAsync(pdfUrl);
                    response.EnsureSuccessStatusCode();
                    
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(localPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    
                    _logger($"PDF downloaded to {localPath}");
                }
                catch (Exception ex)
                {
                    _logger($"Error downloading PDF {pdfUrl}: {ex.Message}");
                    throw;
                }
            }
            
            return localPath;
        }
        
        private string GetSafeFilenameFromUrl(string url)
        {
            // Extract filename from URL
            string filename = Path.GetFileName(new Uri(url).LocalPath);
            
            // Fallback if no filename can be determined
            if (string.IsNullOrWhiteSpace(filename) || !filename.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                filename = $"{Guid.NewGuid()}.pdf";
            }
            
            // Replace invalid characters with underscores
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            
            return filename;
        }
        
        private void LoadPdfMetadata()
        {
            string metadataPath = Path.Combine(_storageDirectory, "pdf_metadata.json");
            
            if (File.Exists(metadataPath))
            {
                try
                {
                    string json = File.ReadAllText(metadataPath);
                    var metadata = JsonConvert.DeserializeObject<Dictionary<string, PdfMetadata>>(json);
                    
                    if (metadata != null)
                    {
                        _pdfMetadata.Clear();
                        foreach (var entry in metadata)
                        {
                            _pdfMetadata[entry.Key] = entry.Value;
                        }
                        
                        _logger($"Loaded metadata for {_pdfMetadata.Count} PDF documents");
                    }
                }
                catch (Exception ex)
                {
                    _logger($"Error loading PDF metadata: {ex.Message}");
                }
            }
        }
        
        private void SavePdfMetadataToDisk()
        {
            string metadataPath = Path.Combine(_storageDirectory, "pdf_metadata.json");
            
            try
            {
                string json = JsonConvert.SerializeObject(_pdfMetadata, Formatting.Indented);
                File.WriteAllText(metadataPath, json);
            }
            catch (Exception ex)
            {
                _logger($"Error saving PDF metadata: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Uses PdfPig to extract text from PDF files
        /// </summary>
        private async Task<string> ExtractTextFromPdfFile(string pdfFilePath)
        {
            var result = new StringBuilder();
            
            try
            {
                // Use PdfPig to extract text from the PDF
                using (var document = PdfDocument.Open(pdfFilePath))
                {
                    // Add document information if available
                    if (document.Information.Title != null)
                    {
                        result.AppendLine($"Title: {document.Information.Title}");
                    }
                    if (document.Information.Author != null)
                    {
                        result.AppendLine($"Author: {document.Information.Author}");
                    }
                    if (document.Information.Subject != null)
                    {
                        result.AppendLine($"Subject: {document.Information.Subject}");
                    }
                    if (document.Information.CreationDate != null)
                    {
                        result.AppendLine($"Creation Date: {document.Information.CreationDate}");
                    }
                    if (result.Length > 0)
                    {
                        result.AppendLine();
                        result.AppendLine("---");
                        result.AppendLine();
                    }
                    
                    // Extract text from each page
                    for (var i = 1; i <= document.NumberOfPages; i++)
                    {
                        var page = document.GetPage(i);
                        
                        // Use simple text extraction
                        string pageText = ContentOrderTextExtractor.GetText(page);
                        
                        // Add page number and content
                        result.AppendLine($"Page {i}:");
                        result.AppendLine(pageText);
                        result.AppendLine();
                    }
                }
                
                _logger($"Extracted {result.Length} characters from PDF: {pdfFilePath}");
                return result.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error in PDF text extraction: {ex.Message}");
                return $"[PDF EXTRACTION ERROR: {ex.Message}]";
            }
        }
        
        /// <summary>
        /// Extract PDF properties using PdfPig
        /// </summary>
        private async Task TryExtractPdfProperties(PdfMetadata metadata)
        {
            try
            {
                using (var document = PdfDocument.Open(metadata.LocalFilePath))
                {
                    // Extract basic document properties
                    metadata.PageCount = document.NumberOfPages;
                    
                    // Try to extract title if not already set
                    if (string.IsNullOrEmpty(metadata.Title) && !string.IsNullOrEmpty(document.Information.Title))
                    {
                        metadata.Title = document.Information.Title;
                    }
                    
                    // Try to set publication date from document creation date
                    if (metadata.PublishDate == null && document.Information.CreationDate != null)
                    {
                        metadata.PublishDate = document.Information.CreationDate;
                    }
                    
                    // Add other metadata from the PDF
                    if (!string.IsNullOrEmpty(document.Information.Author))
                    {
                        metadata.AdditionalMetadata["Author"] = document.Information.Author;
                    }
                    
                    if (!string.IsNullOrEmpty(document.Information.Keywords))
                    {
                        metadata.AdditionalMetadata["Keywords"] = document.Information.Keywords;
                    }
                    
                    if (!string.IsNullOrEmpty(document.Information.Subject))
                    {
                        metadata.AdditionalMetadata["Subject"] = document.Information.Subject;
                    }
                    
                    if (!string.IsNullOrEmpty(document.Information.Producer))
                    {
                        metadata.AdditionalMetadata["Producer"] = document.Information.Producer;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error extracting PDF properties: {ex.Message}");
                
                // Set basic file info even if extraction fails
                var fileInfo = new FileInfo(metadata.LocalFilePath);
                metadata.FileSizeBytes = fileInfo.Length;
                
                if (metadata.PageCount == 0)
                {
                    metadata.PageCount = -1; // Unknown
                }
            }
        }
    }
}