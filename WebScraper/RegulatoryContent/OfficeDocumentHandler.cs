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
            string filename = GetSafeFilenameFromUrl(documentUrl);
            string localPath = Path.Combine(_storageDirectory, filename);
            
            // Download only if we don't have it already
            if (!File.Exists(localPath))
            {
                _logger($"Downloading document from {documentUrl}");
                
                try
                {
                    var response = await _httpClient.GetAsync(documentUrl);
                    response.EnsureSuccessStatusCode();
                    
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(localPath))
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                    
                    _logger($"Document downloaded to {localPath}");
                }
                catch (Exception ex)
                {
                    _logger($"Error downloading document {documentUrl}: {ex.Message}");
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
                           "_" + timeStamp + Path.GetExtension(fileName);
            }
            
            return fileName;
        }

        public async Task<string> ExtractTextFromDocument(string documentUrl)
        {
            try
            {
                // Download the document if not already downloaded
                var localPath = await DownloadDocumentIfNeeded(documentUrl);
                string extension = Path.GetExtension(localPath).ToLowerInvariant();
                
                // Extract text based on document type
                string extractedText;
                
                switch (extension)
                {
                    case ".docx":
                        extractedText = ExtractTextFromWordDocument(localPath);
                        break;
                    case ".xlsx":
                        extractedText = ExtractTextFromExcelDocument(localPath);
                        break;
                    case ".pptx":
                        extractedText = ExtractTextFromPowerPointDocument(localPath);
                        break;
                    case ".doc":
                    case ".xls":
                    case ".ppt":
                        extractedText = $"[LEGACY OFFICE FORMAT NOT SUPPORTED: {extension}]";
                        _logger($"Legacy Office format not supported: {extension}");
                        break;
                    default:
                        extractedText = $"[UNSUPPORTED DOCUMENT FORMAT: {extension}]";
                        _logger($"Unsupported document format: {extension}");
                        break;
                }
                
                _logger($"Extracted {extractedText.Length} characters from document: {documentUrl}");
                
                // Update metadata
                if (!_docMetadata.TryGetValue(documentUrl, out var metadata))
                {
                    metadata = new OfficeDocumentMetadata
                    {
                        Url = documentUrl,
                        LocalFilePath = localPath,
                        DownloadDate = DateTime.Now,
                        DocumentType = GetDocumentType(extension),
                        FileSizeBytes = new FileInfo(localPath).Length
                    };
                    _docMetadata[documentUrl] = metadata;
                }
                
                SaveDocumentMetadataToDisk();
                
                return extractedText;
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from document {documentUrl}: {ex.Message}");
                return $"[DOCUMENT EXTRACTION ERROR: {ex.Message}]";
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
                        
                        // Extract tables
                        foreach (var table in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Table>())
                        {
                            foreach (var row in table.Descendants<TableRow>())
                            {
                                var rowText = string.Join("\t", row.Descendants<TableCell>().Select(cell => cell.InnerText));
                                text.AppendLine(rowText);
                            }
                            text.AppendLine();
                        }
                    }
                }
                
                _logger($"Successfully extracted text from Word document: {filePath}");
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from Word document {filePath}: {ex.Message}");
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
                    
                    // Get all worksheets
                    var sheets = doc.WorkbookPart.Workbook.Descendants<Sheet>();
                    
                    foreach (var sheet in sheets)
                    {
                        text.AppendLine($"--- Sheet: {sheet.Name} ---");
                        
                        // Get the worksheet part
                        WorksheetPart worksheetPart = (WorksheetPart)doc.WorkbookPart.GetPartById(sheet.Id);
                        Worksheet worksheet = worksheetPart.Worksheet;
                        
                        // Get all cells with data
                        var cells = worksheet.Descendants<Cell>().Where(c => c.CellValue != null);
                        
                        // Group cells by row
                        var rows = cells.GroupBy(c => GetRowIndex(c.CellReference));
                        
                        foreach (var row in rows.OrderBy(r => r.Key))
                        {
                            var rowText = new StringBuilder();
                            
                            foreach (var cell in row.OrderBy(c => GetColumnIndex(c.CellReference)))
                            {
                                string cellValue = GetCellValue(cell, sharedStrings);
                                rowText.Append(cellValue + "\t");
                            }
                            
                            text.AppendLine(rowText.ToString().TrimEnd('\t'));
                        }
                        
                        text.AppendLine();
                    }
                }
                
                _logger($"Successfully extracted text from Excel document: {filePath}");
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from Excel document {filePath}: {ex.Message}");
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
                    
                    // Get presentation part
                    var presentationPart = doc.PresentationPart;
                    var presentation = presentationPart.Presentation;
                    
                    // Get all slides
                    var slideIds = presentation.SlideIdList.ChildElements;
                    int slideNumber = 1;
                    
                    foreach (SlideId slideId in slideIds)
                    {
                        text.AppendLine($"--- Slide {slideNumber++} ---");
                        
                        // Get the slide part
                        SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);
                        
                        // Get all text elements
                        var textElements = slidePart.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
                        
                        foreach (var textElement in textElements)
                        {
                            text.AppendLine(textElement.Text);
                        }
                        
                        text.AppendLine();
                    }
                }
                
                _logger($"Successfully extracted text from PowerPoint document: {filePath}");
                return text.ToString();
            }
            catch (Exception ex)
            {
                _logger($"Error extracting text from PowerPoint document {filePath}: {ex.Message}");
                return $"[POWERPOINT DOCUMENT EXTRACTION ERROR: {ex.Message}]";
            }
        }

        private string GetCellValue(Cell cell, SharedStringTable sharedStrings)
        {
            if (cell.DataType == null)
            {
                // Numeric cell value
                return cell.CellValue?.Text ?? "";
            }
            
            if (cell.DataType.Value == CellValues.SharedString && sharedStrings != null)
            {
                // Shared string value
                int ssid = int.Parse(cell.CellValue.Text);
                return sharedStrings.ChildElements[ssid].InnerText;
            }
            
            // Other cell value types
            return cell.CellValue?.Text ?? "";
        }

        private string GetRowIndex(string cellReference)
        {
            // Extract the row index from cell reference like "A1", "B12", etc.
            return new string(cellReference.Where(c => char.IsDigit(c)).ToArray());
        }

        private string GetColumnIndex(string cellReference)
        {
            // Extract the column index from cell reference like "A1", "B12", etc.
            return new string(cellReference.Where(c => char.IsLetter(c)).ToArray());
        }

        private string GetDocumentType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".docx" => "Word Document",
                ".xlsx" => "Excel Spreadsheet",
                ".pptx" => "PowerPoint Presentation",
                ".doc" => "Legacy Word Document",
                ".xls" => "Legacy Excel Spreadsheet",
                ".ppt" => "Legacy PowerPoint Presentation",
                _ => "Unknown Office Document"
            };
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
}