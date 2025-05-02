using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.Validation;
using WebScraperApi.Models;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Implementation;
using WebScraperApi.Services.Factories;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service for managing the execution of scrapers
    /// </summary>
    public class ScraperExecutionService
    {
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ScraperComponentFactory _componentFactory;

        public ScraperExecutionService(
            ILogger logger,
            ILoggerFactory loggerFactory,
            ScraperComponentFactory componentFactory)
        {
            _logger = logger;
            _loggerFactory = loggerFactory;
            _componentFactory = componentFactory;
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
            WebScraperApi.Models.ScraperState scraperState,
            Action<string> logAction)
        {
            try
            {
                logAction($"Starting scraper: {config.Name}");

                // Get the scraper configuration
                var scraperConfig = config.ToScraperConfig();

                // Validate the configuration
                if (!await ValidateConfigurationAsync(scraperConfig, logAction))
                {
                    return false;
                }

                // Always use EnhancedScraper for all scrapers
                var enhancedScraper = await CreateEnhancedScraperAsync(scraperConfig, logAction);
                // Create a standard Scraper that we can use with the existing code
                var scraper = new Scraper(scraperConfig, _logger);

                // Initialize the scraper
                logAction("Initializing scraper...");
                await scraper.InitializeAsync();

                // Start scraping
                logAction("Starting scraping process...");
                await scraper.StartScrapingAsync();

                // Set up continuous monitoring if enabled
                if (config.EnableContinuousMonitoring)
                {
                    var interval = TimeSpan.FromHours(config.GetMonitoringInterval());
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
        /// Validate the scraper configuration
        /// </summary>
        private async Task<bool> ValidateConfigurationAsync(ScraperConfig config, Action<string> logAction)
        {
            // Create an HttpClient for validation
            using var httpClient = new System.Net.Http.HttpClient();

            // First validate the configuration using our validator
            var validator = new ConfigurationValidator(httpClient, logAction);
            var validationResult = await validator.ValidateConfigurationAsync(config);

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

            return true;
        }

        /// <summary>
        /// Create an enhanced scraper with components from factories
        /// </summary>
        private async Task<EnhancedScraper> CreateEnhancedScraperAsync(ScraperConfig config, Action<string> logAction)
        {
            logAction("Using enhanced scraper with advanced capabilities");

            // Create the logger for the enhanced scraper
            var scraperLogger = _loggerFactory.CreateLogger<EnhancedScraper>();

            // Create all required components for the enhanced scraper using the factory
            var crawlStrategy = _componentFactory.CreateCrawlStrategy(config, logAction);
            var contentExtractor = _componentFactory.CreateContentExtractor(config, logAction);
            var documentProcessor = _componentFactory.CreateDocumentProcessor(config, logAction);
            var changeDetector = _componentFactory.CreateChangeDetector(config, logAction);
            var contentClassifier = _componentFactory.CreateContentClassifier(config, logAction);
            var dynamicRenderer = _componentFactory.CreateDynamicContentRenderer(config, logAction);
            var stateStore = _componentFactory.CreateStateStore(config, logAction);
            var alertService = _componentFactory.CreateAlertService(config, logAction);

            // Convert ScraperConfig to RegulatoryScraperConfig
            var regulatoryConfig = new WebScraper.RegulatoryFramework.Configuration.RegulatoryScraperConfig
            {
                DomainName = config.Name,
                BaseUrl = config.BaseUrl,
                UserAgent = "WebScraperAPI/1.0",
                MaxConcurrentRequests = config.MaxConcurrentRequests,
                RequestTimeoutSeconds = 30,
                EnablePriorityCrawling = config.EnableAdaptiveCrawling,
                EnableHierarchicalExtraction = config.ExtractStructuredContent,
                EnableDocumentProcessing = config.ProcessPdfDocuments,
                EnableComplianceChangeDetection = config.EnableChangeDetection,
                EnableDomainClassification = config.ClassifyRegulatoryDocuments,
                EnableDynamicContentRendering = false,
                EnableAlertSystem = config.NotifyOnChanges
            };

            // Create the enhanced scraper with all components
            var enhancedScraper = new EnhancedScraper(
                regulatoryConfig,
                scraperLogger,
                crawlStrategy,
                contentExtractor,
                documentProcessor,
                changeDetector,
                contentClassifier,
                dynamicRenderer,
                alertService,
                stateStore);

            // Wait a moment to make this truly async
            await Task.Delay(1);

            return enhancedScraper;
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
    }
}