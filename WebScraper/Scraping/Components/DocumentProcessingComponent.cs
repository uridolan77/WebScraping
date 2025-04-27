using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using WebScraper.RegulatoryContent;
using WebScraper.Interfaces;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles document processing
    /// </summary>
    public class DocumentProcessingComponent : ScraperComponentBase, IDocumentProcessor
    {
        private PdfDocumentHandler _pdfDocumentHandler;
        private OfficeDocumentHandler _officeDocumentHandler;
        private string _documentsDirectory;
        private bool _documentProcessingEnabled;
        private readonly HttpClient _httpClient;
        
        public DocumentProcessingComponent()
        {
            _httpClient = new HttpClient();
        }
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            _documentProcessingEnabled = Config.ProcessPdfDocuments || Config.ProcessOfficeDocuments;
            if (!_documentProcessingEnabled)
            {
                LogInfo("Document processing not enabled, component will be inactive");
                return;
            }
            
            InitializeDocumentHandlers();
        }
        
        /// <summary>
        /// Initializes the document handlers
        /// </summary>
        private void InitializeDocumentHandlers()
        {
            try
            {
                LogInfo("Initializing document handlers...");
                
                // Create documents directory
                _documentsDirectory = Path.Combine(Config.OutputDirectory, "documents");
                if (!Directory.Exists(_documentsDirectory))
                {
                    Directory.CreateDirectory(_documentsDirectory);
                }
                
                // Initialize PDF document handler if enabled
                if (Config.ProcessPdfDocuments)
                {
                    LogInfo("Initializing PDF document handler...");
                    // Fix: Pass _httpClient as the second parameter
                    _pdfDocumentHandler = new PdfDocumentHandler(_documentsDirectory, _httpClient, LogInfo);
                    LogInfo("PDF document handler initialized");
                }
                
                // Initialize Office document handler if enabled
                if (Config.ProcessOfficeDocuments)
                {
                    LogInfo("Initializing Office document handler...");
                    // Fix: Pass _httpClient as the second parameter
                    _officeDocumentHandler = new OfficeDocumentHandler(_documentsDirectory, _httpClient, LogInfo);
                    LogInfo("Office document handler initialized");
                }
                
                LogInfo("Document handlers initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize document handlers");
            }
        }
        
        /// <summary>
        /// Processes a document
        /// </summary>
        public async Task ProcessDocumentAsync(string url, byte[] content, string contentType)
        {
            if (!_documentProcessingEnabled || content == null || content.Length == 0)
                return;
                
            try
            {
                LogInfo($"Processing document: {url} ({contentType})");
                
                // Determine file extension from content type
                string fileExtension = GetFileExtensionFromContentType(contentType);
                
                // Create a safe filename from the URL
                string fileName = CreateSafeFileName(url, fileExtension);
                
                // Full file path
                string filePath = Path.Combine(_documentsDirectory, fileName);
                
                // Save document to disk
                await File.WriteAllBytesAsync(filePath, content);
                
                // Process document based on its type
                if (IsPdfContentType(contentType) && _pdfDocumentHandler != null)
                {
                    await ProcessPdfDocumentAsync(url, filePath);
                }
                else if (IsOfficeContentType(contentType) && _officeDocumentHandler != null)
                {
                    await ProcessOfficeDocumentAsync(url, filePath);
                }
                else
                {
                    LogInfo($"No handler available for content type: {contentType}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing document: {url}");
            }
        }
        
        /// <summary>
        /// Processes a PDF document
        /// </summary>
        private async Task ProcessPdfDocumentAsync(string url, string filePath)
        {
            try
            {
                // Extract text from PDF
                var result = await _pdfDocumentHandler.ExtractTextAsync(filePath);
                
                // Save extracted text
                string textFilePath = Path.ChangeExtension(filePath, ".txt");
                await File.WriteAllTextAsync(textFilePath, result.Text);
                
                // Extract and save metadata
                var metadata = new DocumentMetadata
                {
                    Title = result.Title,
                    Author = result.Author,
                    CreationDate = result.CreationDate,
                    LastModifiedDate = result.ModificationDate,
                    PageCount = result.PageCount,
                    Keywords = result.Keywords
                };
                
                string metadataFilePath = Path.ChangeExtension(filePath, ".metadata.json");
                await File.WriteAllTextAsync(
                    metadataFilePath,
                    System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
                );
                
                LogInfo($"Processed PDF document: {filePath} ({result.PageCount} pages)");
                
                // Optional: Store in state manager
                var stateManager = GetComponent<IStateManager>();
                if (stateManager != null)
                {
                    await stateManager.SaveContentAsync(url, result.Text, "text/plain");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing PDF: {filePath}");
            }
        }
        
        /// <summary>
        /// Processes an Office document
        /// </summary>
        private async Task ProcessOfficeDocumentAsync(string url, string filePath)
        {
            try
            {
                // Extract text from Office document
                var result = await _officeDocumentHandler.ExtractTextAsync(filePath);
                
                // Save extracted text
                string textFilePath = Path.ChangeExtension(filePath, ".txt");
                await File.WriteAllTextAsync(textFilePath, result.Text);
                
                // Extract and save metadata
                var metadata = new DocumentMetadata
                {
                    Title = result.Title,
                    Author = result.Author,
                    CreationDate = result.CreationDate,
                    LastModifiedDate = result.ModificationDate,
                    PageCount = result.PageCount,
                    Keywords = result.Keywords
                };
                
                string metadataFilePath = Path.ChangeExtension(filePath, ".metadata.json");
                await File.WriteAllTextAsync(
                    metadataFilePath,
                    System.Text.Json.JsonSerializer.Serialize(metadata, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })
                );
                
                LogInfo($"Processed Office document: {filePath}");
                
                // Optional: Store in state manager
                var stateManager = GetComponent<IStateManager>();
                if (stateManager != null)
                {
                    await stateManager.SaveContentAsync(url, result.Text, "text/plain");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing Office document: {filePath}");
            }
        }
        
        /// <summary>
        /// Creates a safe filename from a URL
        /// </summary>
        private string CreateSafeFileName(string url, string extension)
        {
            try
            {
                // Remove scheme and domain
                Uri uri = new Uri(url);
                string path = uri.Host + uri.AbsolutePath;
                
                // Replace invalid characters
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    path = path.Replace(c, '_');
                }
                
                // Replace other problematic characters
                path = path.Replace('/', '_').Replace('\\', '_').Replace(':', '_');
                
                // Ensure extension
                if (string.IsNullOrEmpty(extension))
                {
                    extension = ".bin";
                }
                
                // Add extension if needed
                if (!path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    path += extension;
                }
                
                return path;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error creating safe filename from URL: {url}");
                return $"document_{DateTime.Now.Ticks}{extension}";
            }
        }
        
        /// <summary>
        /// Gets the file extension from a content type
        /// </summary>
        private string GetFileExtensionFromContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return ".bin";
                
            contentType = contentType.ToLowerInvariant();
            
            // Map content types to extensions
            if (contentType.Contains("pdf"))
                return ".pdf";
            if (contentType.Contains("msword") || contentType.Contains("wordprocessingml"))
                return ".docx";
            if (contentType.Contains("excel") || contentType.Contains("spreadsheetml"))
                return ".xlsx";
            if (contentType.Contains("powerpoint") || contentType.Contains("presentationml"))
                return ".pptx";
            if (contentType.Contains("opendocument.text"))
                return ".odt";
            if (contentType.Contains("opendocument.spreadsheet"))
                return ".ods";
            if (contentType.Contains("opendocument.presentation"))
                return ".odp";
                
            return ".bin";
        }
        
        /// <summary>
        /// Determines if a content type is PDF
        /// </summary>
        private bool IsPdfContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;
                
            contentType = contentType.ToLowerInvariant();
            return contentType.Contains("pdf");
        }
        
        /// <summary>
        /// Determines if a content type is an Office document
        /// </summary>
        private bool IsOfficeContentType(string contentType)
        {
            if (string.IsNullOrEmpty(contentType))
                return false;
                
            contentType = contentType.ToLowerInvariant();
            return contentType.Contains("msword") ||
                   contentType.Contains("wordprocessingml") ||
                   contentType.Contains("excel") ||
                   contentType.Contains("spreadsheetml") ||
                   contentType.Contains("powerpoint") ||
                   contentType.Contains("presentationml") ||
                   contentType.Contains("opendocument");
        }
    }
    
    /// <summary>
    /// Document metadata
    /// </summary>
    public class DocumentMetadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? CreationDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int PageCount { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }
}