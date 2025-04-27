using System;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating regulatory monitoring components
    /// </summary>
    public class RegulatoryMonitorFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        public RegulatoryMonitorFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Creates an alert service for significant regulatory changes
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IAlertService implementation or null if not needed</returns>
        public IAlertService? CreateAlertService(ScraperConfig config, Action<string> logAction)
        {
            bool enableNotifications = config.GetType().GetProperty("EnableNotifications")?.GetValue(config) as bool? ?? false;
            
            if (config.MonitorHighImpactChanges || enableNotifications)
            {
                logAction("Setting up regulatory alert service");
                
                // Fix constructor call by using the existing scraper and correct parameters
                var scraper = new Scraper(config, logAction);
                var monitor = new GamblingRegulationMonitor(scraper, config.OutputDirectory, logAction);
                
                // Cast to the required interface
                return (IAlertService)monitor;
            }
            
            return null;
        }
        
        /// <summary>
        /// Creates a change detector based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IChangeDetector implementation or null if not needed</returns>
        public IChangeDetector? CreateChangeDetector(ScraperConfig config, Action<string> logAction)
        {
            if (config.TrackRegulatoryChanges || config.MonitorHighImpactChanges)
            {
                logAction("Setting up regulatory change detection");
                // Add explicit cast to fix conversion error
                return (IChangeDetector)new RegulatoryChangeDetector(logAction);
            }
            
            return null;
        }
        
        /// <summary>
        /// Creates a content classifier based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IContentClassifier implementation or null if not needed</returns>
        public IContentClassifier? CreateContentClassifier(ScraperConfig config, Action<string> logAction)
        {
            if (config.ClassifyRegulatoryDocuments || config.EnableRegulatoryContentAnalysis)
            {
                logAction("Setting up regulatory document classification");
                // Add explicit cast to fix conversion error
                return (IContentClassifier)new RegulatoryDocumentClassifier(logAction);
            }
            
            return null;
        }
    }
}