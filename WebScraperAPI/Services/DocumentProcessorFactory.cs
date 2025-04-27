using System;
using System.IO;
using System.Net.Http;
using WebScraper;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Factory for creating document processors based on scraper configuration
    /// </summary>
    public class DocumentProcessorFactory
    {
        /// <summary>
        /// Creates a document processor based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IDocumentProcessor implementation or null if not needed</returns>
        public static IDocumentProcessor CreateDocumentProcessor(ScraperConfig config, Action<string> logAction)
        {
            // Check if any document processing is needed
            if (config.ProcessPdfDocuments || config.ProcessOfficeDocuments)
            {
                // Create a properly configured HttpClient
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                
                // Prepare the document storage directory
                string documentStoragePath = Path.Combine(config.OutputDirectory, "Documents");
                if (!Directory.Exists(documentStoragePath))
                {
                    Directory.CreateDirectory(documentStoragePath);
                }
                
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
                if (config.ProcessOfficeDocuments)
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
    }
}