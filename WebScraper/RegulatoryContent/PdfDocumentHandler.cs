using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace WebScraper.RegulatoryContent
{
    public class PdfDocumentHandler
    {
        private readonly string _storageDirectory;
        private readonly HttpClient _httpClient;
        private readonly Action<string> _logger;
        private Dictionary<string, PdfMetadata> _pdfMetadata = new Dictionary<string, PdfMetadata>();

        public PdfDocumentHandler(string storageDirectory, HttpClient httpClient = null, Action<string> logger = null)
        {
            _storageDirectory = storageDirectory;
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger ?? Console.WriteLine;

            // Create storage directory if it doesn't exist
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            // Load existing metadata if available
            LoadPdfMetadata();
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

        private void LoadPdfMetadata()
        {
            string metadataPath = Path.Combine(_storageDirectory, "pdf_metadata.json");
            
            if (File.Exists(metadataPath))
            {
                try
                {
                    string json = File.ReadAllText(metadataPath);
                    _pdfMetadata = JsonConvert.DeserializeObject<Dictionary<string, PdfMetadata>>(json) ?? 
                                  new Dictionary<string, PdfMetadata>();
                    _logger($"Loaded metadata for {_pdfMetadata.Count} PDFs");
                }
                catch (Exception ex)
                {
                    _logger($"Error loading PDF metadata: {ex.Message}");
                    _pdfMetadata = new Dictionary<string, PdfMetadata>();
                }
            }
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
            // Remove query string
            var baseUrl = url.Split('?')[0];
            
            // Get the last part of the URL
            var fileName = Path.GetFileName(baseUrl);
            
            // Ensure it has a PDF extension
            if (!fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".pdf";
            }
            
            // Replace invalid filename characters
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            
            // Ensure unique by adding a timestamp if needed
            if (File.Exists(Path.Combine(_storageDirectory, fileName)))
            {
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                fileName = Path.GetFileNameWithoutExtension(fileName) + 
                           "_" + timeStamp + ".pdf";
            }
            
            return fileName;
        }

        private async Task TryExtractPdfProperties(PdfMetadata metadata)
        {
            try
            {
                var fileInfo = new FileInfo(metadata.LocalFilePath);
                metadata.FileSizeBytes = fileInfo.Length;
                
                // Extract page count using iText7
                using (var pdfReader = new PdfReader(metadata.LocalFilePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    metadata.PageCount = pdfDocument.GetNumberOfPages();
                    
                    // Extract additional metadata if available
                    var info = pdfDocument.GetDocumentInfo();
                    if (info != null)
                    {
                        if (!string.IsNullOrEmpty(info.GetTitle()))
                            metadata.AdditionalMetadata["Title"] = info.GetTitle();
                        
                        if (!string.IsNullOrEmpty(info.GetAuthor()))
                            metadata.AdditionalMetadata["Author"] = info.GetAuthor();
                        
                        if (!string.IsNullOrEmpty(info.GetCreator()))
                            metadata.AdditionalMetadata["Creator"] = info.GetCreator();
                        
                        if (!string.IsNullOrEmpty(info.GetProducer()))
                            metadata.AdditionalMetadata["Producer"] = info.GetProducer();
                        
                        if (!string.IsNullOrEmpty(info.GetSubject()))
                            metadata.AdditionalMetadata["Subject"] = info.GetSubject();
                        
                        if (!string.IsNullOrEmpty(info.GetKeywords()))
                            metadata.AdditionalMetadata["Keywords"] = info.GetKeywords();
                    }
                }
                
                _logger($"Extracted PDF properties: {metadata.PageCount} pages, {metadata.FileSizeBytes} bytes");
            }
            catch (Exception ex)
            {
                _logger($"Error extracting PDF properties: {ex.Message}");
            }
        }

        private async Task<string> ExtractTextFromPdfFile(string pdfFilePath)
        {
            try
            {
                var extractedText = new System.Text.StringBuilder();
                
                using (var pdfReader = new PdfReader(pdfFilePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    int pageCount = pdfDocument.GetNumberOfPages();
                    
                    for (int i = 1; i <= pageCount; i++)
                    {
                        var page = pdfDocument.GetPage(i);
                        var textListener = new LocationTextExtractionStrategy();
                        string pageText = PdfTextExtractor.GetTextFromPage(page, textListener);
                        extractedText.AppendLine(pageText);
                        
                        // Add a page marker to help with document structure analysis
                        extractedText.AppendLine($"[PAGE_BREAK_{i}]");
                    }
                }
                
                string result = extractedText.ToString();
                _logger($"Successfully extracted {result.Length} characters from PDF: {pdfFilePath}");
                return result;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from PDF {pdfFilePath}: {ex.Message}");
                return $"[PDF EXTRACTION ERROR: {ex.Message}]";
            }
        }

        public async Task<string> ExtractPdfText(string pdfUrl)
        {
            try
            {
                // Download the PDF if not already downloaded
                var localPath = await DownloadPdfIfNeeded(pdfUrl);
                
                // Extract text from the PDF file
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

        public async Task<string> ExtractTextFromPdfUrl(string pdfUrl)
        {
            // This is an alias to match the method being called from EnhancedScraper
            return await ExtractPdfText(pdfUrl);
        }

        public async Task<string> ExtractTextAsync(byte[] document)
        {
            try
            {
                // Create a temporary file to process the PDF
                string tempFilePath = Path.Combine(_storageDirectory, $"temp_pdf_{Guid.NewGuid()}.pdf");
                
                try
                {
                    // Write the PDF bytes to a temp file
                    await File.WriteAllBytesAsync(tempFilePath, document);
                    
                    // Extract the text from the temp file
                    string extractedText = await ExtractTextFromPdfFile(tempFilePath);
                    
                    _logger($"Successfully extracted {extractedText.Length} characters from PDF");
                    return extractedText;
                }
                finally
                {
                    // Clean up the temp file
                    if (File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch (Exception ex)
                        {
                            _logger($"Warning: Failed to delete temporary PDF file: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from PDF: {ex.Message}");
                return $"[PDF EXTRACTION ERROR: {ex.Message}]";
            }
        }

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
    }
}