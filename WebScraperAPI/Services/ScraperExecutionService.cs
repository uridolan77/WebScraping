using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.Validation;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service for managing the execution of scrapers
    /// </summary>
    public class ScraperExecutionService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        
        public ScraperExecutionService(ILogger logger, ILoggerFactory loggerFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Starts a scraper with the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="scraperState">The current scraper state to update during execution</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>A task representing the asynchronous operation</returns>
        public async Task<bool> StartScraperAsync(
            ScraperConfigModel config, 
            ScraperState scraperState,
            Action<string> logAction)
        {
            try
            {
                logAction($"Starting scraper: {config.Name}");
                
                // Get the scraper configuration
                var scraperConfig = config.ToScraperConfig();
                
                // Create an HttpClient for validation
                var httpClient = new System.Net.Http.HttpClient();
                
                // First validate the configuration using our validator
                var validator = new ConfigurationValidator(httpClient, logAction);
                var validationResult = await validator.ValidateConfigurationAsync(scraperConfig);
                
                if (!validationResult.IsValid && !validationResult.CanRunWithWarnings)
                {
                    logAction("Configuration validation failed");
                    foreach (var error in validationResult.Errors)
                    {
                        logAction($"Error: {error}");
                    }
                    
                    logAction("Scraping aborted due to configuration errors");
                    return false;
                }
                else if (validationResult.Warnings.Any())
                {
                    logAction("Configuration has warnings:");
                    foreach (var warning in validationResult.Warnings)
                    {
                        logAction($"Warning: {warning}");
                    }
                    logAction("Continuing with warnings...");
                }
                
                // Check if enhanced features are needed
                bool useEnhancedScraper = IsEnhancedScraperRequired(scraperConfig);
                
                // Create a scraper instance
                Scraper scraper;
                if (useEnhancedScraper)
                {
                    logAction("Using enhanced scraper with advanced capabilities");
                    
                    // Create the logger for the enhanced scraper
                    var scraperLogger = _loggerFactory.CreateLogger<EnhancedScraper>();
                    
                    // Create document processor if needed
                    var documentProcessor = CreateDocumentProcessor(scraperConfig, logAction);
                    
                    // Create the enhanced scraper with our components
                    scraper = new EnhancedScraper(
                        scraperConfig, 
                        scraperLogger,
                        crawlStrategy: null!, // Using null-forgiving operator
                        contentExtractor: null!, // Using null-forgiving operator
                        documentProcessor: documentProcessor!); // Using null-forgiving operator
                }
                else
                {
                    // Create and initialize the standard scraper
                    scraper = new Scraper(scraperConfig, logAction);
                }
                
                // Initialize the scraper
                logAction("Initializing scraper...");
                await scraper.InitializeAsync();
                
                // Start scraping
                logAction("Starting scraping process...");
                await scraper.StartScrapingAsync();
                
                // Set up continuous monitoring if enabled
                if (config.EnableContinuousMonitoring)
                {
                    var interval = config.GetMonitoringInterval();
                    await scraper.SetupContinuousScrapingAsync(interval);
                    logAction($"Continuous monitoring enabled with interval: {interval.TotalHours:F1} hours");
                }
                
                logAction("Scraping operation completed successfully");
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error executing scraper {config.Name}");
                logAction($"Error during scraping: {ex.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Stops a running scraper
        /// </summary>
        /// <param name="scraper">The scraper to stop</param>
        /// <param name="logAction">Action for logging messages</param>
        public void StopScraper(Scraper scraper, Action<string> logAction)
        {
            if (scraper != null)
            {
                try
                {
                    // Stop the scraper's continuous monitoring if it's running
                    scraper.StopContinuousScraping();
                    logAction("Scraping stopped by user");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error stopping scraper");
                    logAction($"Error stopping scraper: {ex.Message}");
                }
            }
        }
        
        /// <summary>
        /// Determines if the enhanced scraper should be used based on configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <returns>True if enhanced scraper is required, otherwise false</returns>
        private bool IsEnhancedScraperRequired(ScraperConfig config)
        {
            // Check if regulatory features are enabled
            bool useEnhancedScraper = IsRegulatoryFeaturesEnabled(config);
            
            // Also check if other enhanced features are enabled
            useEnhancedScraper = useEnhancedScraper || 
                                config.ProcessPdfDocuments || 
                                config.ProcessJsHeavyPages;
                                

            return useEnhancedScraper;
        }
        
        /// <summary>
        /// Determines if regulatory features are enabled in the scraper configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <returns>True if regulatory features are enabled, otherwise false</returns>
        private bool IsRegulatoryFeaturesEnabled(ScraperConfig config)
        {
            // Check if any regulatory-specific features are enabled
            return config.EnableRegulatoryContentAnalysis ||
                   config.TrackRegulatoryChanges ||
                   config.ClassifyRegulatoryDocuments ||
                   config.ExtractStructuredContent ||
                   config.ProcessPdfDocuments ||
                   config.MonitorHighImpactChanges ||
                   config.IsUKGCWebsite;
        }
        
        /// <summary>
        /// Creates a document processor based on the provided configuration
        /// </summary>
        private WebScraper.RegulatoryFramework.Interfaces.IDocumentProcessor? CreateDocumentProcessor(ScraperConfig config, Action<string> logAction)
        {
            // Create document processor if needed
            if (config.ProcessPdfDocuments)
            {
                // Create a properly configured HttpClient
                var httpClient = new System.Net.Http.HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(2)
                };
                
                // Create the PDF document handler with proper parameters
                var pdfHandler = new WebScraper.RegulatoryContent.PdfDocumentHandler(
                    config.OutputDirectory,
                    httpClient,
                    logAction);
                
                // Create adapter that implements IDocumentProcessor interface
                return new DocumentProcessorAdapter(pdfHandler);
            }
            
            return null;
        }
    }
    
    /// <summary>
    /// Represents the current execution state of a scraper
    /// </summary>
    public class ScraperState
    {
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ElapsedTime { get; set; } = string.Empty; // Initialize with empty string
        public int UrlsProcessed { get; set; }
        public PipelineMetrics PipelineMetrics { get; set; } = new PipelineMetrics();
        public DateTime? LastMonitorCheck { get; set; }
        public List<LogEntry> LogMessages { get; set; } = new List<LogEntry>();
        
        /// <summary>
        /// Update elapsed time based on start time
        /// </summary>
        public void UpdateElapsedTime()
        {
            if (IsRunning && StartTime.HasValue)
            {
                var elapsed = DateTime.Now - StartTime.Value;
                ElapsedTime = elapsed.ToString(@"hh\:mm\:ss");
            }
        }
        
        /// <summary>
        /// Add a log message
        /// </summary>
        public void AddLogMessage(string message)
        {
            LogMessages.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Message = message
            });
            
            // Keep only the last 1000 messages
            if (LogMessages.Count > 1000)
            {
                LogMessages.RemoveAt(0);
            }
        }
        
        /// <summary>
        /// Get recent log messages
        /// </summary>
        public IEnumerable<LogEntry> GetRecentLogs(int limit = 100)
        {
            return LogMessages.Count <= limit
                ? LogMessages.ToList()
                : LogMessages.Skip(LogMessages.Count - limit).ToList();
        }
    }
}