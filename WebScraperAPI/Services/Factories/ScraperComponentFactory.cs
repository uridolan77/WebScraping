using System;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Central factory for creating all scraper components
    /// </summary>
    public class ScraperComponentFactory
    {
        private readonly ContentExtractionFactory _contentFactory;
        private readonly DocumentProcessingFactory _documentFactory;
        private readonly RegulatoryMonitorFactory _regulatoryFactory;
        private readonly StateManagementFactory _stateFactory;
        
        public ScraperComponentFactory(ILoggerFactory loggerFactory)
        {
            _contentFactory = new ContentExtractionFactory(loggerFactory);
            _documentFactory = new DocumentProcessingFactory(loggerFactory);
            _regulatoryFactory = new RegulatoryMonitorFactory(loggerFactory);
            _stateFactory = new StateManagementFactory(loggerFactory);
        }
        
        // Content extraction components
        public ICrawlStrategy CreateCrawlStrategy(ScraperConfig config, Action<string> logAction) => 
            _contentFactory.CreateCrawlStrategy(config, logAction);
            
        public IContentExtractor? CreateContentExtractor(ScraperConfig config, Action<string> logAction) => 
            _contentFactory.CreateContentExtractor(config, logAction);
            
        public IDynamicContentRenderer? CreateDynamicContentRenderer(ScraperConfig config, Action<string> logAction) =>
            _contentFactory.CreateDynamicContentRenderer(config, logAction);
        
        // Document processing components
        public IDocumentProcessor? CreateDocumentProcessor(ScraperConfig config, Action<string> logAction) => 
            _documentFactory.CreateDocumentProcessor(config, logAction);
        
        // Regulatory components
        public IChangeDetector? CreateChangeDetector(ScraperConfig config, Action<string> logAction) =>
            _regulatoryFactory.CreateChangeDetector(config, logAction);
            
        public IContentClassifier? CreateContentClassifier(ScraperConfig config, Action<string> logAction) =>
            _regulatoryFactory.CreateContentClassifier(config, logAction);
            
        public IAlertService? CreateAlertService(ScraperConfig config, Action<string> logAction) =>
            _regulatoryFactory.CreateAlertService(config, logAction);
        
        // State management components
        public IStateStore CreateStateStore(ScraperConfig config, Action<string> logAction) =>
            _stateFactory.CreateStateStore(config, logAction);
    }
}