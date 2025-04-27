using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Base class for all feature configurations
    /// </summary>
    public abstract class FeatureConfigBase
    {
        /// <summary>
        /// Returns a list of validation errors (empty if valid)
        /// </summary>
        public virtual List<string> Validate()
        {
            return new List<string>();
        }
    }
    
    /// <summary>
    /// Configuration for the priority crawling feature
    /// </summary>
    public class PriorityCrawlingConfig : FeatureConfigBase
    {
        /// <summary>
        /// URL patterns with their priority scores (0.0-1.0)
        /// </summary>
        public List<UrlPriorityPattern> UrlPatterns { get; set; } = new List<UrlPriorityPattern>();
        
        /// <summary>
        /// Keywords that indicate important content
        /// </summary>
        public List<string> ContentKeywords { get; set; } = new List<string>();
        
        /// <summary>
        /// Maximum depth to crawl
        /// </summary>
        public int MaxDepth { get; set; } = 5;
        
        /// <summary>
        /// Maximum number of links to follow per page
        /// </summary>
        public int MaxLinksPerPage { get; set; } = 20;
        
        /// <summary>
        /// Respect robots.txt rules
        /// </summary>
        public bool RespectRobotsTxt { get; set; } = true;
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (MaxDepth <= 0)
            {
                errors.Add("MaxDepth must be greater than 0");
            }
            
            if (MaxLinksPerPage <= 0)
            {
                errors.Add("MaxLinksPerPage must be greater than 0");
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// URL pattern and priority for crawling
    /// </summary>
    public class UrlPriorityPattern
    {
        /// <summary>
        /// URL pattern (can be a regex or simple substring)
        /// </summary>
        public string Pattern { get; set; }
        
        /// <summary>
        /// Priority (0.0-1.0)
        /// </summary>
        public double Priority { get; set; }
        
        /// <summary>
        /// Whether the pattern is a regex
        /// </summary>
        public bool IsRegex { get; set; } = false;
    }
    
    /// <summary>
    /// Configuration for hierarchical content extraction
    /// </summary>
    public class HierarchicalExtractionConfig
    {
        /// <summary>
        /// CSS selector for content elements
        /// </summary>
        public string ContentSelector { get; set; } = "p, .content";
        
        /// <summary>
        /// CSS selector for parent container elements
        /// </summary>
        public string ParentSelector { get; set; } = "section, article, div.section";
        
        /// <summary>
        /// CSS selector for title elements
        /// </summary>
        public string TitleSelector { get; set; } = "h1, h2, h3, .title";
        
        /// <summary>
        /// CSS selector for elements to exclude
        /// </summary>
        public string ExcludeSelector { get; set; } = "nav, footer, header, .ads";
        
        /// <summary>
        /// Maximum depth for the content hierarchy
        /// </summary>
        public int MaxHierarchyDepth { get; set; } = 5;
        
        /// <summary>
        /// Keywords to look for in content to determine relevance
        /// </summary>
        public List<string> KeywordPatterns { get; set; } = new List<string>();
        
        /// <summary>
        /// CSS classes that indicate relevant content
        /// </summary>
        public List<string> RelevantClasses { get; set; } = new List<string>();
        
        /// <summary>
        /// Whether to extract links from content
        /// </summary>
        public bool ExtractLinks { get; set; } = true;
        
        /// <summary>
        /// Whether to extract metadata from content
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;
    }
    
    /// <summary>
    /// Configuration for document processing
    /// </summary>
    public class DocumentProcessingConfig : FeatureConfigBase
    {
        /// <summary>
        /// Directory to store downloaded documents
        /// </summary>
        public string DocumentStoragePath { get; set; } = "regulatory_documents";
        
        /// <summary>
        /// Whether to download documents
        /// </summary>
        public bool DownloadDocuments { get; set; } = true;
        
        /// <summary>
        /// Extract metadata from documents
        /// </summary>
        public bool ExtractMetadata { get; set; } = true;
        
        /// <summary>
        /// Extract full text from documents
        /// </summary>
        public bool ExtractFullText { get; set; } = true;
        
        /// <summary>
        /// Document types to process
        /// </summary>
        public List<string> DocumentTypes { get; set; } = new List<string> { ".pdf", ".docx", ".xlsx" };
        
        /// <summary>
        /// Regular expression patterns to extract metadata from documents
        /// </summary>
        public Dictionary<string, string> MetadataPatterns { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Maximum file size to download (in bytes)
        /// </summary>
        public long MaxFileSize { get; set; } = 20 * 1024 * 1024; // 20 MB
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (DownloadDocuments && string.IsNullOrEmpty(DocumentStoragePath))
            {
                errors.Add("DocumentStoragePath is required when DownloadDocuments is enabled");
            }
            
            if (MaxFileSize <= 0)
            {
                errors.Add("MaxFileSize must be greater than 0");
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// Configuration for change detection
    /// </summary>
    public class ChangeDetectionConfig : FeatureConfigBase
    {
        /// <summary>
        /// Keywords that indicate significant changes
        /// </summary>
        public List<string> SignificantKeywords { get; set; } = new List<string>();
        
        /// <summary>
        /// Minimum content length to process
        /// </summary>
        public int MinContentLength { get; set; } = 100;
        
        /// <summary>
        /// Minimum threshold for detecting significant changes (0.0-1.0)
        /// </summary>
        public double SignificanceThreshold { get; set; } = 0.3;
        
        /// <summary>
        /// Store historical versions
        /// </summary>
        public bool StoreVersionHistory { get; set; } = true;
        
        /// <summary>
        /// Maximum number of versions to store per URL
        /// </summary>
        public int MaxVersionsPerUrl { get; set; } = 10;
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (SignificanceThreshold < 0 || SignificanceThreshold > 1)
            {
                errors.Add("SignificanceThreshold must be between 0.0 and 1.0");
            }
            
            if (MaxVersionsPerUrl <= 0)
            {
                errors.Add("MaxVersionsPerUrl must be greater than 0");
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// Configuration for content classification
    /// </summary>
    public class ClassificationConfig : FeatureConfigBase
    {
        /// <summary>
        /// Categories and their associated keywords
        /// </summary>
        public Dictionary<string, List<string>> Categories { get; set; } = new Dictionary<string, List<string>>();
        
        /// <summary>
        /// Minimum confidence threshold for classification (0.0-1.0)
        /// </summary>
        public double ConfidenceThreshold { get; set; } = 0.5;
        
        /// <summary>
        /// Use machine learning for classification
        /// </summary>
        public bool UseMachineLearning { get; set; } = false;
        
        /// <summary>
        /// Path to ML model file
        /// </summary>
        public string ModelPath { get; set; }
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (Categories.Count == 0)
            {
                errors.Add("At least one category must be defined");
            }
            
            if (ConfidenceThreshold < 0 || ConfidenceThreshold > 1)
            {
                errors.Add("ConfidenceThreshold must be between 0.0 and 1.0");
            }
            
            if (UseMachineLearning && string.IsNullOrEmpty(ModelPath))
            {
                errors.Add("ModelPath is required when UseMachineLearning is enabled");
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// Configuration for dynamic content rendering
    /// </summary>
    public class DynamicContentConfig : FeatureConfigBase
    {
        /// <summary>
        /// Browser type to use
        /// </summary>
        public string BrowserType { get; set; } = "chromium";
        
        /// <summary>
        /// Maximum number of concurrent browser sessions
        /// </summary>
        public int MaxConcurrentSessions { get; set; } = 3;
        
        /// <summary>
        /// CSS selector to wait for before considering page loaded
        /// </summary>
        public string WaitForSelector { get; set; }
        
        /// <summary>
        /// CSS selector for element to click (e.g., cookie banner)
        /// </summary>
        public string AutoClickSelector { get; set; }
        
        /// <summary>
        /// Delay after navigation (in milliseconds)
        /// </summary>
        public int PostNavigationDelay { get; set; } = 1000;
        
        /// <summary>
        /// Navigation timeout (in milliseconds)
        /// </summary>
        public int NavigationTimeout { get; set; } = 30000;
        
        /// <summary>
        /// Custom user agent for browser
        /// </summary>
        public string BrowserUserAgent { get; set; }
        
        /// <summary>
        /// Whether to disable JavaScript
        /// </summary>
        public bool DisableJavaScript { get; set; } = false;
        
        /// <summary>
        /// Proxy to use
        /// </summary>
        public string Proxy { get; set; }
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (string.IsNullOrEmpty(BrowserType) || 
                (BrowserType != "chromium" && BrowserType != "firefox" && BrowserType != "webkit"))
            {
                errors.Add("BrowserType must be one of: chromium, firefox, webkit");
            }
            
            if (MaxConcurrentSessions <= 0)
            {
                errors.Add("MaxConcurrentSessions must be greater than 0");
            }
            
            if (NavigationTimeout <= 0)
            {
                errors.Add("NavigationTimeout must be greater than 0");
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// Configuration for the alert system
    /// </summary>
    public class AlertSystemConfig : FeatureConfigBase
    {
        /// <summary>
        /// Whether to enable email alerts
        /// </summary>
        public bool EnableEmailAlerts { get; set; } = false;
        
        /// <summary>
        /// SMTP server address
        /// </summary>
        public string SmtpServer { get; set; }
        
        /// <summary>
        /// SMTP port
        /// </summary>
        public int SmtpPort { get; set; } = 25;
        
        /// <summary>
        /// SMTP username
        /// </summary>
        public string SmtpUsername { get; set; }
        
        /// <summary>
        /// SMTP password
        /// </summary>
        public string SmtpPassword { get; set; }
        
        /// <summary>
        /// Email sender
        /// </summary>
        public string EmailSender { get; set; }
        
        /// <summary>
        /// Email recipient
        /// </summary>
        public string EmailRecipient { get; set; }
        
        /// <summary>
        /// Alert rules
        /// </summary>
        public List<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
        
        /// <summary>
        /// Cooldown period between alerts (in minutes)
        /// </summary>
        public int AlertCooldownMinutes { get; set; } = 60;
        
        public override List<string> Validate()
        {
            var errors = new List<string>();
            
            if (EnableEmailAlerts)
            {
                if (string.IsNullOrEmpty(SmtpServer))
                {
                    errors.Add("SmtpServer is required when EnableEmailAlerts is enabled");
                }
                
                if (string.IsNullOrEmpty(EmailSender))
                {
                    errors.Add("EmailSender is required when EnableEmailAlerts is enabled");
                }
                
                if (string.IsNullOrEmpty(EmailRecipient))
                {
                    errors.Add("EmailRecipient is required when EnableEmailAlerts is enabled");
                }
            }
            
            return errors;
        }
    }
    
    /// <summary>
    /// Rule for generating alerts
    /// </summary>
    public class AlertRule
    {
        /// <summary>
        /// Name of the rule
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Keywords that trigger the rule
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();
        
        /// <summary>
        /// URL patterns that trigger the rule
        /// </summary>
        public List<string> UrlPatterns { get; set; } = new List<string>();
        
        /// <summary>
        /// Importance of the alert
        /// </summary>
        public string Importance { get; set; } = "Medium";
    }
}