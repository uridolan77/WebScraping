using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScraper.RegulatoryFramework.Interfaces;
using WebScraper.RegulatoryFramework.Implementation;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Extension methods for registering regulatory framework services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds regulatory scraper services to the service collection
        /// </summary>
        public static IServiceCollection AddRegulatoryScraper(
            this IServiceCollection services, 
            RegulatoryScraperConfig config)
        {
            // Validate configuration
            var validationErrors = config.Validate();
            if (validationErrors.Count > 0)
            {
                throw new ArgumentException(
                    $"Invalid configuration: {string.Join(", ", validationErrors)}");
            }
            
            // Base services always registered
            services.AddSingleton(config);
            services.AddSingleton<EnhancedScraper>();
            
            // Register each component based on feature flags
            /*
            if (config.EnablePriorityCrawling)
            {
                services.AddSingleton<ICrawlStrategy>(sp => 
                    new PriorityCrawler(config.PriorityCrawlingConfig, sp.GetService<ILogger<PriorityCrawler>>()));
            }
            */
            
            if (config.EnableHierarchicalExtraction)
            {
                services.AddSingleton<IContentExtractor>(sp => 
                    new StructureAwareExtractor(config.HierarchicalExtractionConfig, sp.GetService<ILogger<StructureAwareExtractor>>()));
            }
            
            if (config.EnableDocumentProcessing)
            {
                services.AddSingleton<IDocumentProcessor>(sp => 
                    new DocumentProcessor(config.DocumentProcessingConfig, sp.GetService<ILogger<DocumentProcessor>>()));
            }
            
            /*
            if (config.EnableComplianceChangeDetection)
            {
                services.AddSingleton<IChangeDetector>(sp => 
                    new ComplianceChangeDetector(config.ChangeDetectionConfig, sp.GetService<ILogger<ComplianceChangeDetector>>()));
            }
            
            if (config.EnableDomainClassification)
            {
                services.AddSingleton<IContentClassifier>(sp => 
                    new ContentClassifier(config.ClassificationConfig, sp.GetService<ILogger<ContentClassifier>>()));
            }
            */
            
            if (config.EnableDynamicContentRendering)
            {
                services.AddSingleton<IDynamicContentRenderer>(sp => 
                    new PlaywrightRenderer(config.DynamicContentConfig, sp.GetService<ILogger<PlaywrightRenderer>>()));
            }
            
            /*
            if (config.EnableAlertSystem)
            {
                services.AddSingleton<IAlertService>(sp => 
                    new AlertService(config.AlertSystemConfig, sp.GetService<ILogger<AlertService>>()));
            }
            */
            
            // Add state store based on configuration
            switch (config.StateStoreType)
            {
                case StateStoreType.Memory:
                    services.AddSingleton<IStateStore, InMemoryStateStore>();
                    break;
                case StateStoreType.File:
                    services.AddSingleton<IStateStore>(sp => 
                        new FileSystemStateStore(config.StateStorePath, sp.GetService<ILogger<FileSystemStateStore>>()));
                    break;
                case StateStoreType.Database:
                    services.AddSingleton<IStateStore>(sp => 
                        new DatabaseStateStore(config.StateStoreConnectionString, sp.GetService<ILogger<DatabaseStateStore>>()));
                    break;
                default:
                    services.AddSingleton<IStateStore, InMemoryStateStore>();
                    break;
            }
            
            return services;
        }
    }
}