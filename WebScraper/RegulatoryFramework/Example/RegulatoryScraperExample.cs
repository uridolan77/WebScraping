using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Implementation;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Example
{
    /// <summary>
    /// Example of how to use the regulatory scraping framework
    /// </summary>
    public class RegulatoryScraperExample
    {
        /// <summary>
        /// Runs a simple example of the regulatory scraper
        /// </summary>
        public static async Task RunExampleAsync()
        {
            Console.WriteLine("Starting Regulatory Scraper Example");
            
            // Set up dependency injection
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(configure => 
            {
                configure.AddConsole();
                configure.SetMinimumLevel(LogLevel.Information);
            });
            
            // Create configuration
            var config = UkgcConfiguration.CreateUkgcConfig();
            
            // Configure file paths for running in example mode
            config.StateStorePath = Path.Combine("examples", "ukgc_state");
            config.DocumentProcessingConfig.DocumentStoragePath = Path.Combine("examples", "ukgc_documents");
            
            // Add regulatory scraper with UKGC configuration
            services.AddRegulatoryScraper(config);
            
            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            
            // Get the enhanced scraper
            var scraper = serviceProvider.GetRequiredService<EnhancedScraper>();
            
            // Example URLs to process
            var urlsToProcess = new List<string>
            {
                "https://www.gamblingcommission.gov.uk/licensees-and-businesses/lccp/online",
                "https://www.gamblingcommission.gov.uk/licensees-and-businesses/guide/anti-money-laundering"
            };
            
            // Process each URL
            foreach (var url in urlsToProcess)
            {
                Console.WriteLine($"Processing URL: {url}");
                await scraper.ProcessUrl(url);
            }
            
            // Get the current state of the scraper
            var state = await scraper.GetStateAsync();
            
            Console.WriteLine($"Scraper configured for: {state.ConfiguredDomain}");
            Console.WriteLine("Enabled features:");
            
            foreach (var feature in state.EnabledFeatures)
            {
                Console.WriteLine($"  - {feature.Key}: {feature.Value}");
            }
            
            Console.WriteLine("Example completed successfully");
        }
        
        /// <summary>
        /// Example of using the regulatory scraper with direct component instantiation
        /// </summary>
        public static async Task RunManualExampleAsync()
        {
            Console.WriteLine("Starting Manual Regulatory Scraper Example");
            
            // Create logger factory and logger
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });
            
            var logger = loggerFactory.CreateLogger<EnhancedScraper>();
            
            // Create configuration
            var config = UkgcConfiguration.CreateUkgcConfig();
            
            // Configure file paths for running in example mode
            config.StateStorePath = Path.Combine("examples", "manual_state");
            config.DocumentProcessingConfig.DocumentStoragePath = Path.Combine("examples", "manual_documents");
            
            // Create components manually
            var stateStore = new FileSystemStateStore(
                config.StateStorePath,
                loggerFactory.CreateLogger<FileSystemStateStore>());
            
            var contentExtractor = new StructureAwareExtractor(
                config.HierarchicalExtractionConfig,
                loggerFactory.CreateLogger<StructureAwareExtractor>());
            
            /*
            var changeDetector = new ComplianceChangeDetector(
                config.ChangeDetectionConfig,
                loggerFactory.CreateLogger<ComplianceChangeDetector>());
            */
            
            var documentProcessor = new DocumentProcessor(
                config.DocumentProcessingConfig,
                loggerFactory.CreateLogger<DocumentProcessor>());
            
            // Create the scraper with only the components we need
            var scraper = new EnhancedScraper(
                config, // Pass the RegulatoryScraperConfig directly, removed the conversion
                logger,
                contentExtractor: contentExtractor,
                changeDetector: null, // Comment out: changeDetector
                documentProcessor: documentProcessor,
                stateStore: stateStore);
            
            // Process a URL
            await scraper.ProcessUrl("https://www.gamblingcommission.gov.uk/licensees-and-businesses/lccp/online");
            
            Console.WriteLine("Manual example completed successfully");
        }
    }
}