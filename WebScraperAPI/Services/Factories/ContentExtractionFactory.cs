using System;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Implementation;
using WebScraper.AdaptiveCrawling;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating content extraction components
    /// </summary>
    public class ContentExtractionFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        public ContentExtractionFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Creates a content extractor based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IContentExtractor implementation or null if not needed</returns>
        public IContentExtractor? CreateContentExtractor(ScraperConfig config, Action<string> logAction)
        {
            if (config.ExtractStructuredContent)
            {
                logAction("Setting up structured content extraction");
                return (IContentExtractor)new StructuredContentExtractor(logAction);
            }
            
            return null;
        }
        
        /// <summary>
        /// Creates a crawl strategy based on the provided configuration
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An ICrawlStrategy implementation</returns>
        public ICrawlStrategy CreateCrawlStrategy(ScraperConfig config, Action<string> logAction)
        {
            if (config.IsUKGCWebsite)
            {
                logAction("Setting up UK Gambling Commission specific crawl strategy");
                return (ICrawlStrategy)new UKGCCrawlStrategy(logAction);
            }
            else
            {
                logAction("Setting up adaptive crawling strategy");
                return (ICrawlStrategy)new AdaptiveCrawlStrategy(logAction);
            }
        }
        
        /// <summary>
        /// Creates a dynamic content renderer for JavaScript-heavy pages
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IDynamicContentRenderer implementation or null if not needed</returns>
        public IDynamicContentRenderer? CreateDynamicContentRenderer(ScraperConfig config, Action<string> logAction)
        {
            if (config.ProcessJsHeavyPages)
            {
                logAction("Setting up headless browser renderer for JavaScript-heavy pages");
                
                // Create a config object with appropriate settings
                var dynamicContentConfig = new DynamicContentConfig
                {
                    BrowserType = "chromium",
                    MaxConcurrentSessions = 2,
                    NavigationTimeout = 30000,
                    PostNavigationDelay = 1000,
                    DisableJavaScript = false
                };
                
                var rendererLogger = _loggerFactory.CreateLogger<PlaywrightRenderer>();
                
                return new PlaywrightRenderer(dynamicContentConfig, rendererLogger);
            }
            
            return null;
        }
    }
}