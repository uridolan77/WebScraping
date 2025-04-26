using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Configuration for the regulatory scraper
    /// </summary>
    public class RegulatoryScraperConfig
    {
        /// <summary>
        /// Name of the regulatory domain being scraped
        /// </summary>
        public string DomainName { get; set; } = "GenericRegulatorySite";
        
        /// <summary>
        /// Base URL for the regulatory site
        /// </summary>
        public string BaseUrl { get; set; }
        
        /// <summary>
        /// User agent string to use for requests
        /// </summary>
        public string UserAgent { get; set; } = "RegulatoryScraperBot/1.0";
        
        /// <summary>
        /// Maximum number of concurrent requests
        /// </summary>
        public int MaxConcurrentRequests { get; set; } = 5;
        
        /// <summary>
        /// Request timeout in seconds
        /// </summary>
        public int RequestTimeoutSeconds { get; set; } = 30;
        
        /// <summary>
        /// Type of state store to use
        /// </summary>
        public StateStoreType StateStoreType { get; set; } = StateStoreType.Memory;
        
        /// <summary>
        /// Path to store state information (for file-based state store)
        /// </summary>
        public string StateStorePath { get; set; } = "regulatory_state";
        
        /// <summary>
        /// Connection string (for database state store)
        /// </summary>
        public string StateStoreConnectionString { get; set; }
        
        /// <summary>
        /// Enable/disable feature flags
        /// </summary>
        public bool EnablePriorityCrawling { get; set; } = true;
        public bool EnableHierarchicalExtraction { get; set; } = true;
        public bool EnableDocumentProcessing { get; set; } = true;
        public bool EnableComplianceChangeDetection { get; set; } = true;
        public bool EnableDomainClassification { get; set; } = true;
        public bool EnableDynamicContentRendering { get; set; } = false;
        public bool EnableAlertSystem { get; set; } = true;
        
        /// <summary>
        /// Feature-specific configurations
        /// </summary>
        public PriorityCrawlingConfig PriorityCrawlingConfig { get; set; } = new PriorityCrawlingConfig();
        public HierarchicalExtractionConfig HierarchicalExtractionConfig { get; set; } = new HierarchicalExtractionConfig();
        public DocumentProcessingConfig DocumentProcessingConfig { get; set; } = new DocumentProcessingConfig();
        public ChangeDetectionConfig ChangeDetectionConfig { get; set; } = new ChangeDetectionConfig();
        public ClassificationConfig ClassificationConfig { get; set; } = new ClassificationConfig();
        public DynamicContentConfig DynamicContentConfig { get; set; } = new DynamicContentConfig();
        public AlertSystemConfig AlertSystemConfig { get; set; } = new AlertSystemConfig();
        
        /// <summary>
        /// Validates the configuration
        /// </summary>
        public List<string> Validate()
        {
            var errors = new List<string>();
            
            // Basic validation
            if (string.IsNullOrEmpty(BaseUrl))
            {
                errors.Add("BaseUrl is required");
            }
            else if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out _))
            {
                errors.Add($"BaseUrl '{BaseUrl}' is not a valid URL");
            }
            
            if (MaxConcurrentRequests < 1)
            {
                errors.Add("MaxConcurrentRequests must be at least 1");
            }
            
            if (RequestTimeoutSeconds < 5)
            {
                errors.Add("RequestTimeoutSeconds must be at least 5 seconds");
            }
            
            // Feature-specific validation
            if (EnableDocumentProcessing)
            {
                errors.AddRange(DocumentProcessingConfig.Validate());
            }
            
            if (EnableDynamicContentRendering)
            {
                errors.AddRange(DynamicContentConfig.Validate());
            }
            
            if (EnableAlertSystem)
            {
                errors.AddRange(AlertSystemConfig.Validate());
            }
            
            // State store validation
            if (StateStoreType == StateStoreType.File && string.IsNullOrEmpty(StateStorePath))
            {
                errors.Add("StateStorePath is required for file-based state store");
            }
            
            if (StateStoreType == StateStoreType.Database && string.IsNullOrEmpty(StateStoreConnectionString))
            {
                errors.Add("StateStoreConnectionString is required for database state store");
            }
            
            return errors;
        }
        
        /// <summary>
        /// Loads configuration from a JSON file
        /// </summary>
        public static RegulatoryScraperConfig FromJson(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<RegulatoryScraperConfig>(json) ?? new RegulatoryScraperConfig();
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to parse configuration: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Converts the configuration to JSON
        /// </summary>
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}