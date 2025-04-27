// filepath: c:\dev\WebScraping\WebScraper\RegulatoryContent\OfficeDocumentHandler.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Presentation;
using Newtonsoft.Json;
using System.Linq;
using System.Xml;
using System.Text.RegularExpressions;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Handles processing of Office documents (Word, Excel, PowerPoint)
    /// </summary>
    public class OfficeDocumentHandler
    {
        private readonly string _storageDirectory;
        private readonly HttpClient _httpClient;
        private readonly Action<string> _logger;
        private Dictionary<string, OfficeDocumentMetadata> _docMetadata = new Dictionary<string, OfficeDocumentMetadata>();

        public OfficeDocumentHandler(string storageDirectory, HttpClient httpClient = null, Action<string> logger = null)
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
            LoadDocumentMetadata();
        }

        // Add constructor overload that accepts just the logger
        public OfficeDocumentHandler(Action<string> logger = null)
        {
            _storageDirectory = Path.Combine(Path.GetTempPath(), "OfficeDocuments");
            _httpClient = new HttpClient();
            _logger = logger ?? Console.WriteLine;

            // Create storage directory if it doesn't exist
            if (!Directory.Exists(_storageDirectory))
            {
                Directory.CreateDirectory(_storageDirectory);
            }

            // Load existing metadata if available
            LoadDocumentMetadata();
        }

        private void SaveDocumentMetadataToDisk()
        {
            string metadataPath = Path.Combine(_storageDirectory, "office_metadata.json");
            
            try
            {
                string json = JsonConvert.SerializeObject(_docMetadata, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(metadataPath, json);
            }
            catch (Exception ex)
            {
                _logger($"Error saving Office document metadata: {ex.Message}");
            }
        }

        private void LoadDocumentMetadata()
        {
            string metadataPath = Path.Combine(_storageDirectory, "office_metadata.json");
            
            if (File.Exists(metadataPath))
            {
                try
                {
                    string json = File.ReadAllText(metadataPath);
                    _docMetadata = JsonConvert.DeserializeObject<Dictionary<string, OfficeDocumentMetadata>>(json) ?? 
                                  new Dictionary<string, OfficeDocumentMetadata>();
                    _logger($"Loaded metadata for {_docMetadata.Count} Office documents");
                }
                catch (Exception ex)
                {
                    _logger($"Error loading Office document metadata: {ex.Message}");
                    _docMetadata = new Dictionary<string, OfficeDocumentMetadata>();
                }
            }
        }

        private async Task<string> DownloadDocumentIfNeeded(string documentUrl)
        {
            string localPath = Path.Combine(_storageDirectory, GetSafeFilenameFromUrl(documentUrl));
            
            // Check if we have already downloaded this file
            if (File.Exists(localPath))
            {
                _logger($"Using cached version of {documentUrl}");
                return localPath;
            }
            
            // Download the file
            _logger($"Downloading {documentUrl}");
            try
            {
                byte[] data = await _httpClient.GetByteArrayAsync(documentUrl);
                File.WriteAllBytes(localPath, data);
                
                // Add a metadata entry
                if (!_docMetadata.ContainsKey(documentUrl))
                {
                    _docMetadata[documentUrl] = new OfficeDocumentMetadata
                    {
                        Url = documentUrl,
                        LocalFilePath = localPath,
                        DownloadDate = DateTime.Now,
                        FileSizeBytes = data.Length,
                        DocumentType = GetDocumentType(Path.GetExtension(documentUrl))
                    };
                    
                    SaveDocumentMetadataToDisk();
                }
                
                return localPath;
            }
            catch (Exception ex)
            {
                _logger($"Error downloading {documentUrl}: {ex.Message}");
                return null;
            }
        }

        private string GetSafeFilenameFromUrl(string url)
        {
            // Extract filename from URL
            string filename = Path.GetFileName(url);
            
            // If URL doesn't have a clear filename, hash it
            if (string.IsNullOrEmpty(filename))
            {
                filename = url.GetHashCode().ToString("X8");
                filename += Path.GetExtension(url);
            }
            
            // Replace invalid characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }
            
            return filename;
        }

        public async Task<string> ExtractTextFromDocument(string documentUrl)
        {
            try
            {
                // Download the document if needed
                string filePath = await DownloadDocumentIfNeeded(documentUrl);
                if (string.IsNullOrEmpty(filePath))
                {
                    return "[DOCUMENT DOWNLOAD FAILED]";
                }
                
                // Get the file extension
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                
                // Extract text based on the file type
                string text;
                switch (extension)
                {
                    case ".docx":
                    case ".doc":
                        text = ExtractTextFromWordDocument(filePath);
                        break;
                    case ".xlsx":
                    case ".xls":
                        text = ExtractTextFromExcelDocument(filePath);
                        break;
                    case ".pptx":
                    case ".ppt":
                        text = ExtractTextFromPowerPointDocument(filePath);
                        break;
                    default:
                        text = $"[UNSUPPORTED DOCUMENT FORMAT: {extension}]";
                        break;
                }
                
                return text;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from {documentUrl}: {ex.Message}");
                return $"[DOCUMENT PROCESSING ERROR: {ex.Message}]";
            }
        }
        
        // Add ExtractTextAsync method which is being called in DocumentProcessingComponent.cs
        public async Task<DocumentProcessingResult> ExtractTextAsync(string filePath)
        {
            try
            {
                string extension = Path.GetExtension(filePath).ToLowerInvariant();
                string text;
                string title = Path.GetFileNameWithoutExtension(filePath);
                string author = null;
                DateTime? creationDate = null;
                DateTime? modificationDate = null;
                int pageCount = 0;
                List<string> keywords = new List<string>();
                
                switch (extension)
                {
                    case ".docx":
                    case ".doc":
                        text = ExtractTextFromWordDocument(filePath);
                        using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                        {
                            var props = doc.PackageProperties;
                            if (props != null)
                            {
                                if (!string.IsNullOrEmpty(props.Title))
                                    title = props.Title;
                                if (!string.IsNullOrEmpty(props.Creator))
                                    author = props.Creator;
                                if (props.Created != null)
                                    creationDate = props.Created;
                                if (props.Modified != null)
                                    modificationDate = props.Modified;
                                if (!string.IsNullOrEmpty(props.Keywords))
                                    keywords = props.Keywords.Split(',').Select(k => k.Trim()).ToList();
                            }
                        }
                        break;
                    case ".xlsx":
                    case ".xls":
                        text = ExtractTextFromExcelDocument(filePath);
                        using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                        {
                            var props = doc.PackageProperties;
                            if (props != null)
                            {
                                if (!string.IsNullOrEmpty(props.Title))
                                    title = props.Title;
                                if (!string.IsNullOrEmpty(props.Creator))
                                    author = props.Creator;
                                if (props.Created != null)
                                    creationDate = props.Created;
                                if (props.Modified != null)
                                    modificationDate = props.Modified;
                                if (!string.IsNullOrEmpty(props.Keywords))
                                    keywords = props.Keywords.Split(',').Select(k => k.Trim()).ToList();
                            }
                        }
                        break;
                    case ".pptx":
                    case ".ppt":
                        text = ExtractTextFromPowerPointDocument(filePath);
                        using (PresentationDocument doc = PresentationDocument.Open(filePath, false))
                        {
                            var props = doc.PackageProperties;
                            if (props != null)
                            {
                                if (!string.IsNullOrEmpty(props.Title))
                                    title = props.Title;
                                if (!string.IsNullOrEmpty(props.Creator))
                                    author = props.Creator;
                                if (props.Created != null)
                                    creationDate = props.Created;
                                if (props.Modified != null)
                                    modificationDate = props.Modified;
                                if (!string.IsNullOrEmpty(props.Keywords))
                                    keywords = props.Keywords.Split(',').Select(k => k.Trim()).ToList();
                            }
                            // Try to count slides for page count
                            var presentationPart = doc.PresentationPart;
                            if (presentationPart?.Presentation?.SlideIdList != null)
                            {
                                pageCount = presentationPart.Presentation.SlideIdList.Count();
                            }
                        }
                        break;
                    default:
                        text = $"[UNSUPPORTED DOCUMENT FORMAT: {extension}]";
                        break;
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
                _logger($"Error extracting text from {filePath}: {ex.Message}");
                return new DocumentProcessingResult
                {
                    Text = $"[DOCUMENT PROCESSING ERROR: {ex.Message}]",
                    Title = Path.GetFileNameWithoutExtension(filePath)
                };
            }
        }

        // Add ExtractTextAsync method overload for byte[] parameter
        public async Task<string> ExtractTextAsync(byte[] documentBytes)
        {
            try
            {
                // Create a temporary file to save the bytes
                string tempPath = Path.Combine(Path.GetTempPath(), $"tempoffice_{Guid.NewGuid()}.docx");
                File.WriteAllBytes(tempPath, documentBytes);
                
                // Process the file
                var result = await ExtractTextAsync(tempPath);
                
                // Clean up the temp file
                try { File.Delete(tempPath); } catch { }
                
                return result.Text;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from document bytes: {ex.Message}");
                return $"[DOCUMENT PROCESSING ERROR: {ex.Message}]";
            }
        }

        private string ExtractTextFromWordDocument(string filePath)
        {
            var text = new StringBuilder();
            
            try
            {
                using (WordprocessingDocument doc = WordprocessingDocument.Open(filePath, false))
                {
                    // Extract document properties
                    if (_docMetadata.TryGetValue(filePath, out var metadata))
                    {
                        var props = doc.PackageProperties;
                        if (props != null)
                        {
                            if (!string.IsNullOrEmpty(props.Title))
                                metadata.Title = props.Title;
                            
                            if (!string.IsNullOrEmpty(props.Creator))
                                metadata.AdditionalMetadata["Author"] = props.Creator;
                            
                            if (props.Created != null)
                                metadata.PublishDate = props.Created;
                        }
                    }
                    
                    // Extract document body text
                    var body = doc.MainDocumentPart?.Document?.Body;
                    if (body != null)
                    {
                        // Extract paragraphs
                        foreach (var para in body.Descendants<Paragraph>())
                        {
                            text.AppendLine(para.InnerText);
                        }
                    }
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from Word document: {ex.Message}");
                return $"[WORD DOCUMENT EXTRACTION ERROR: {ex.Message}]";
            }
        }

        private string ExtractTextFromExcelDocument(string filePath)
        {
            var text = new StringBuilder();
            
            try
            {
                using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filePath, false))
                {
                    // Extract document properties
                    if (_docMetadata.TryGetValue(filePath, out var metadata))
                    {
                        var props = doc.PackageProperties;
                        if (props != null)
                        {
                            if (!string.IsNullOrEmpty(props.Title))
                                metadata.Title = props.Title;
                            
                            if (!string.IsNullOrEmpty(props.Creator))
                                metadata.AdditionalMetadata["Author"] = props.Creator;
                            
                            if (props.Created != null)
                                metadata.PublishDate = props.Created;
                        }
                    }
                    
                    // Get shared string table
                    SharedStringTable sharedStrings = doc.WorkbookPart.SharedStringTablePart?.SharedStringTable;
                    
                    // Process each worksheet
                    foreach (var worksheetPart in doc.WorkbookPart.WorksheetParts)
                    {
                        var sheetName = doc.WorkbookPart.Workbook.Descendants<Sheet>()
                            .FirstOrDefault(s => s.Id == doc.WorkbookPart.GetIdOfPart(worksheetPart))?.Name;
                            
                        if (!string.IsNullOrEmpty(sheetName))
                        {
                            text.AppendLine($"[Sheet: {sheetName}]");
                        }
                        
                        Worksheet worksheet = worksheetPart.Worksheet;
                        SheetData sheetData = worksheet.GetFirstChild<SheetData>();
                        
                        if (sheetData != null)
                        {
                            string currentRowIndex = null;
                            StringBuilder rowText = new StringBuilder();
                            
                            foreach (var row in sheetData.Descendants<Row>())
                            {
                                rowText.Clear();
                                
                                foreach (var cell in row.Descendants<Cell>())
                                {
                                    string cellValue = GetCellValue(cell, sharedStrings);
                                    rowText.Append(cellValue + "\t");
                                }
                                
                                text.AppendLine(rowText.ToString().TrimEnd('\t'));
                            }
                        }
                        
                        text.AppendLine();
                    }
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from Excel document: {ex.Message}");
                return $"[EXCEL DOCUMENT EXTRACTION ERROR: {ex.Message}]";
            }
        }

        private string ExtractTextFromPowerPointDocument(string filePath)
        {
            var text = new StringBuilder();
            
            try
            {
                using (PresentationDocument doc = PresentationDocument.Open(filePath, false))
                {
                    // Extract document properties
                    if (_docMetadata.TryGetValue(filePath, out var metadata))
                    {
                        var props = doc.PackageProperties;
                        if (props != null)
                        {
                            if (!string.IsNullOrEmpty(props.Title))
                                metadata.Title = props.Title;
                            
                            if (!string.IsNullOrEmpty(props.Creator))
                                metadata.AdditionalMetadata["Author"] = props.Creator;
                            
                            if (props.Created != null)
                                metadata.PublishDate = props.Created;
                        }
                    }
                    
                    // Extract text from slides
                    var presentationPart = doc.PresentationPart;
                    if (presentationPart?.Presentation?.SlideIdList != null)
                    {
                        int slideNumber = 1;
                        
                        foreach (var slideId in presentationPart.Presentation.SlideIdList.ChildElements)
                        {
                            var slidePartId = ((SlideId)slideId).RelationshipId;
                            var slidePart = (SlidePart)presentationPart.GetPartById(slidePartId);
                            
                            text.AppendLine($"[Slide {slideNumber}]");
                            
                            // Extract text from text elements
                            foreach (var paragraph in slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Paragraph>())
                            {
                                text.AppendLine(paragraph.InnerText);
                            }
                            
                            text.AppendLine();
                            slideNumber++;
                        }
                    }
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from PowerPoint document: {ex.Message}");
                return $"[POWERPOINT DOCUMENT EXTRACTION ERROR: {ex.Message}]";
            }
        }

        private string GetCellValue(Cell cell, SharedStringTable sharedStrings)
        {
            if (cell == null)
                return string.Empty;
                
            string value = cell.InnerText;
            
            // If the cell contains a shared string
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                if (sharedStrings != null && int.TryParse(value, out int index))
                {
                    if (index < sharedStrings.ChildElements.Count)
                    {
                        return sharedStrings.ChildElements[index].InnerText;
                    }
                }
            }
            
            return value;
        }

        private string GetRowIndex(string cellReference)
        {
            // Remove column part
            return Regex.Replace(cellReference, "[A-Za-z]", "");
        }

        private string GetColumnIndex(string cellReference)
        {
            // Remove row part
            return Regex.Replace(cellReference, "[0-9]", "");
        }

        private string GetDocumentType(string extension)
        {
            extension = extension.ToLowerInvariant();
            
            switch (extension)
            {
                case ".docx":
                case ".doc":
                    return "Word";
                case ".xlsx":
                case ".xls":
                    return "Excel";
                case ".pptx":
                case ".ppt":
                    return "PowerPoint";
                default:
                    return "Unknown";
            }
        }

        public class OfficeDocumentMetadata
        {
            public string Url { get; set; }
            public string Title { get; set; }
            public string LocalFilePath { get; set; }
            public DateTime? PublishDate { get; set; }
            public string Category { get; set; }
            public DateTime DownloadDate { get; set; } = DateTime.Now;
            public long FileSizeBytes { get; set; }
            public string DocumentType { get; set; }
            public Dictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();
        }
    }
    
    // Add DocumentProcessingResult class for ExtractTextAsync method
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