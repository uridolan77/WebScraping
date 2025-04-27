using System;
using System.IO;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating document processing components
    /// </summary>
    public class DocumentProcessingFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        public DocumentProcessingFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Creates a document processor based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A document processor implementation or null if not needed</returns>
        public IDocumentProcessor? CreateDocumentProcessor(ScraperConfig config, Action<string> logAction)
        {
            // Create properly configured HttpClient
            var httpClient = new System.Net.Http.HttpClient
            {
                Timeout = TimeSpan.FromMinutes(2)
            };

            // Prepare the document storage directory
            string documentStoragePath = Path.Combine(config.OutputDirectory, "Documents");
            if (!Directory.Exists(documentStoragePath))
            {
                Directory.CreateDirectory(documentStoragePath);
            }
            
            // Create document handlers based on configuration
            if (config.ProcessPdfDocuments || IsOfficeDocumentsProcessingEnabled(config))
            {
                logAction("Initializing document processing capabilities...");
                
                // Create an adapter that implements IDocumentProcessor interface
                var adapter = new DocumentProcessorAdapter();
                
                // Add PDF handler if enabled
                if (config.ProcessPdfDocuments)
                {
                    logAction("Setting up PDF document handler");
                    var pdfHandler = new PdfDocumentHandler(
                        documentStoragePath,
                        httpClient,
                        logAction);
                    
                    adapter.RegisterPdfHandler(pdfHandler);
                }
                
                // Add Office documents handler if enabled
                if (IsOfficeDocumentsProcessingEnabled(config))
                {
                    logAction("Setting up Office document handler");
                    var officeHandler = new OfficeDocumentHandler(
                        documentStoragePath,
                        httpClient,
                        logAction);
                    
                    adapter.RegisterOfficeHandler(officeHandler);
                }
                
                return adapter;
            }
            
            return null;
        }
        
        /// <summary>
        /// Determines if Office document processing is enabled in the configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <returns>True if Office document processing is enabled, otherwise false</returns>
        private bool IsOfficeDocumentsProcessingEnabled(ScraperConfig config)
        {
            // Check if Office document processing is explicitly enabled
            return config.ProcessOfficeDocuments || 
                  (config.EnableRegulatoryContentAnalysis && config.ClassifyRegulatoryDocuments);
        }
    }
}