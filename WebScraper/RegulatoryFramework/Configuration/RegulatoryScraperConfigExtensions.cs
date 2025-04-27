using System;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Extensions for RegulatoryScraperConfig to provide compatibility with the standard ScraperConfig
    /// </summary>
    public static class RegulatoryScraperConfigExtensions
    {
        /// <summary>
        /// Converts a RegulatoryScraperConfig to a standard ScraperConfig
        /// </summary>
        public static ScraperConfig ToScraperConfig(this RegulatoryScraperConfig regulatoryConfig)
        {
            if (regulatoryConfig == null)
                throw new ArgumentNullException(nameof(regulatoryConfig));

            return new ScraperConfig
            {
                // Basic settings
                ScraperId = Guid.NewGuid().ToString(),
                ScraperName = regulatoryConfig.DomainName,
                StartUrl = regulatoryConfig.BaseUrl,
                BaseUrl = regulatoryConfig.BaseUrl,
                OutputDirectory = regulatoryConfig.StateStorePath ?? "ScrapedData",
                UserAgent = regulatoryConfig.UserAgent,
                MaxConcurrentRequests = regulatoryConfig.MaxConcurrentRequests,
                RequestTimeoutSeconds = regulatoryConfig.RequestTimeoutSeconds,
                
                // Map regulatory features to ScraperConfig features
                EnableRegulatoryContentAnalysis = true,
                TrackRegulatoryChanges = regulatoryConfig.EnableComplianceChangeDetection,
                ClassifyRegulatoryDocuments = regulatoryConfig.EnableDomainClassification,
                ExtractStructuredContent = regulatoryConfig.EnableHierarchicalExtraction,
                ProcessPdfDocuments = regulatoryConfig.EnableDocumentProcessing,
                MonitorHighImpactChanges = regulatoryConfig.EnableAlertSystem,
                
                // Set reasonable defaults for other features
                EnableChangeDetection = regulatoryConfig.EnableComplianceChangeDetection,
                TrackContentVersions = true,
                EnableAdaptiveCrawling = regulatoryConfig.EnablePriorityCrawling,
                DelayBetweenRequests = 1000,
                MaxDepth = 5,
                NotifyOnChanges = regulatoryConfig.EnableAlertSystem
            };
        }
    }
}