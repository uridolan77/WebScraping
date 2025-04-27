using System.Collections.Generic;
using System;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Configuration for document processing
    /// </summary>
    public class DocumentProcessingConfig
    {
        /// <summary>
        /// Whether to download documents
        /// </summary>
        public bool DownloadDocuments { get; set; } = true;
        
        /// <summary>
        /// Path to store downloaded documents
        /// </summary>
        public string DocumentStoragePath { get; set; } = "documents";
        
        /// <summary>
        /// Document types to process
        /// </summary>
        public List<string> DocumentTypes { get; set; } = new List<string> { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        
        /// <summary>
        /// Maximum file size to download (in bytes)
        /// </summary>
        public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10 MB
        
        /// <summary>
        /// Whether to extract full text from documents
        /// </summary>
        public bool ExtractFullText { get; set; } = true;
        
        /// <summary>
        /// Whether to extract text from documents
        /// </summary>
        public bool ExtractText { get; set; } = true;
        
        /// <summary>
        /// Whether to analyze document metadata
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;
        
        /// <summary>
        /// Metadata extraction patterns
        /// </summary>
        public Dictionary<string, string> MetadataPatterns { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Whether to store downloaded documents locally
        /// </summary>
        public bool StoreDocumentsLocally { get; set; } = true;
        
        /// <summary>
        /// Default requests per minute
        /// </summary>
        public int DefaultRequestsPerMinute { get; set; } = 60;
        
        /// <summary>
        /// Default adaptive rate factor
        /// </summary>
        public float DefaultAdaptiveRateFactor { get; set; } = 0.8f;
        
        /// <summary>
        /// Validates the configuration
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            
            if (DownloadDocuments && string.IsNullOrEmpty(DocumentStoragePath))
            {
                errors.Add("DocumentStoragePath is required when DownloadDocuments is enabled");
            }
            
            if (MaxFileSize <= 0)
            {
                errors.Add("MaxFileSize must be greater than 0");
            }
            
            return errors;
        }
    }
}