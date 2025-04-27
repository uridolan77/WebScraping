using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Linq;
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

        // Constructor with all parameters
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

        // Constructor that accepts just logger - needed for compatibility with RegulatoryDocumentHandler
        public PdfDocumentHandler(Action<string> logger = null)
        {
            _storageDirectory = Path.Combine(Path.GetTempPath(), "PdfDocuments");
            _httpClient = new HttpClient();
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

        // Add ExtractTextAsync method to match the interface expected by DocumentProcessingComponent
        public async Task<DocumentProcessingResult> ExtractTextAsync(string filePath)
        {
            try
            {
                var text = await ExtractTextFromPdfFile(filePath);
                string title = Path.GetFileNameWithoutExtension(filePath);
                string author = null;
                DateTime? creationDate = null;
                DateTime? modificationDate = null;
                int pageCount = 0;
                List<string> keywords = new List<string>();
                
                // Try to extract metadata from the PDF
                using (var pdfReader = new PdfReader(filePath))
                using (var pdfDocument = new PdfDocument(pdfReader))
                {
                    pageCount = pdfDocument.GetNumberOfPages();
                    
                    var info = pdfDocument.GetDocumentInfo();
                    if (info != null)
                    {
                        if (!string.IsNullOrEmpty(info.GetTitle()))
                            title = info.GetTitle();
                        
                        if (!string.IsNullOrEmpty(info.GetAuthor()))
                            author = info.GetAuthor();
                        
                        if (!string.IsNullOrEmpty(info.GetKeywords()))
                            keywords = new List<string>(info.GetKeywords().Split(',').Select(k => k.Trim()));
                        
                        // Parse creation date if present
                        string creationDateStr = info.GetMoreInfo("CreationDate");
                        if (!string.IsNullOrEmpty(creationDateStr))
                        {
                            // PDF dates are often in format: D:YYYYMMDDHHmmSSOHH'mm'
                            // Try to extract a valid date
                            if (creationDateStr.StartsWith("D:") && creationDateStr.Length >= 15)
                            {
                                try
                                {
                                    string dateStr = creationDateStr.Substring(2);
                                    int year = int.Parse(dateStr.Substring(0, 4));
                                    int month = int.Parse(dateStr.Substring(4, 2));
                                    int day = int.Parse(dateStr.Substring(6, 2));
                                    int hour = int.Parse(dateStr.Substring(8, 2));
                                    int minute = int.Parse(dateStr.Substring(10, 2));
                                    
                                    creationDate = new DateTime(year, month, day, hour, minute, 0);
                                }
                                catch
                                {
                                    _logger($"Failed to parse creation date: {creationDateStr}");
                                }
                            }
                            else if (DateTime.TryParse(creationDateStr, out var parsedDate))
                            {
                                creationDate = parsedDate;
                            }
                        }
                        
                        // Parse modification date if present
                        string modDateStr = info.GetMoreInfo("ModDate");
                        if (!string.IsNullOrEmpty(modDateStr))
                        {
                            if (DateTime.TryParse(modDateStr, out var parsedDate))
                            {
                                modificationDate = parsedDate;
                            }
                        }
                    }
                }
                
                return new DocumentProcessingResult
                {
                    Text = text,
                    Title = title,
                    Author = author,
                    CreationDate = creationDate,
                    ModificationDate = modificationDate,
                    PageCount = pageCount,
                    Keywords = keywords
                };
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text and metadata from PDF {filePath}: {ex.Message}");
                return new DocumentProcessingResult
                {
                    Text = $"[PDF EXTRACTION ERROR: {ex.Message}]",
                    Title = Path.GetFileNameWithoutExtension(filePath)
                };
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

    // Add DocumentProcessingResult class if it doesn't exist elsewhere
    // This should match the class in OfficeDocumentHandler
    public class DocumentProcessingResult
    {
        public string Text { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public int PageCount { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }
}