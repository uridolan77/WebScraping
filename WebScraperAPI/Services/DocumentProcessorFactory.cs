using System;
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
            if (config.ProcessPdfDocuments)
            {
                // Create a properly configured HttpClient
                var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                
                // Create the PDF document handler with proper parameters
                var pdfHandler = new PdfDocumentHandler(
                    config.OutputDirectory, 
                    httpClient,
                    logAction);
                
                // Create adapter that implements IDocumentProcessor interface
                return new DocumentProcessorAdapter(pdfHandler);
            }
            
            return null;
        }
    }
}