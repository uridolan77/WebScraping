using System.Collections.Generic;

namespace WebScraper.RegulatoryFramework.Configuration
{
    /// <summary>
    /// Sample configuration for the UK Gambling Commission website
    /// </summary>
    public static class UkgcConfiguration
    {
        /// <summary>
        /// Creates a configuration for the UK Gambling Commission website
        /// </summary>
        public static RegulatoryScraperConfig CreateUkgcConfig()
        {
            return new RegulatoryScraperConfig
            {
                DomainName = "UKGamblingCommission",
                BaseUrl = "https://www.gamblingcommission.gov.uk/",
                UserAgent = "RegulatoryScraperBot/1.0 (compliance@example.com)",
                MaxConcurrentRequests = 3,
                RequestTimeoutSeconds = 30,
                StateStoreType = Interfaces.StateStoreType.File,
                StateStorePath = "ukgc_state",
                
                // Enable all features
                EnablePriorityCrawling = true,
                EnableHierarchicalExtraction = true,
                EnableDocumentProcessing = true,
                EnableComplianceChangeDetection = true,
                EnableDomainClassification = true,
                EnableDynamicContentRendering = true,
                EnableAlertSystem = true,
                
                // Configure priority crawling
                PriorityCrawlingConfig = new PriorityCrawlingConfig
                {
                    UrlPatterns = new List<UrlPriorityPattern>
                    {
                        new UrlPriorityPattern { Pattern = "/licensees-and-businesses/lccp", Priority = 0.9 },
                        new UrlPriorityPattern { Pattern = "/licensees-and-businesses/compliance", Priority = 0.8 },
                        new UrlPriorityPattern { Pattern = "/licensees-and-businesses/aml", Priority = 0.8 },
                        new UrlPriorityPattern { Pattern = "/news/enforcement-action", Priority = 0.8 },
                        new UrlPriorityPattern { Pattern = "/guidance/", Priority = 0.7 },
                        new UrlPriorityPattern { Pattern = ".pdf", Priority = 0.7 }
                    },
                    ContentKeywords = new List<string>
                    {
                        "licence", "license", "requirement", "compliance", "condition",
                        "enforcement", "penalty", "fine", "money laundering"
                    },
                    MaxDepth = 5,
                    MaxLinksPerPage = 30,
                    RespectRobotsTxt = true
                },
                
                // Configure hierarchical extraction
                HierarchicalExtractionConfig = new HierarchicalExtractionConfig
                {
                    ParentSelector = "section, article, div.gcweb-card, div.gcweb-panel",
                    TitleSelector = "h1, h2, h3, p.gcweb-heading-m",
                    ContentSelector = "p.gc-card__description, p.gcweb-body, .gcweb-body-l",
                    ExcludeSelector = "nav, footer, header, aside, #breadcrumb, .gcweb-header, .gcweb-footer",
                    MaxHierarchyDepth = 4,
                    PreserveHtml = false
                },
                
                // Configure document processing
                DocumentProcessingConfig = new DocumentProcessingConfig
                {
                    DocumentStoragePath = "ukgc_documents",
                    DownloadDocuments = true,
                    ExtractMetadata = true,
                    ExtractFullText = true,
                    DocumentTypes = new List<string> { ".pdf", ".docx", ".xlsx" },
                    MetadataPatterns = new Dictionary<string, string>
                    {
                        ["EffectiveDate"] = @"(?i)effective\s*(?:from|date)?\s*:\s*(\\d{1,2}\\s+\\w+\\s+\\d{4})",
                        ["LicenceType"] = @"(?i)(remote|non-remote|gambling software|gaming machine)\s+licen[cs]e",
                        ["RegulatorySection"] = @"(?i)(social responsibility|ordinary)\s+code\s+(\\d+\\.\\d+\\.\\d+)"
                    },
                    MaxFileSize = 20 * 1024 * 1024 // 20 MB
                },
                
                // Configure change detection
                ChangeDetectionConfig = new ChangeDetectionConfig
                {
                    SignificantKeywords = new List<string>
                    {
                        "must", "required", "mandatory", "shall", "condition", "obligation",
                        "effective", "from", "by", "deadline", "due date",
                        "fee", "payment", "cost", "charge", "price", "amount", "rate",
                        "penalty", "fine", "sanction", "enforcement", "action"
                    },
                    MinContentLength = 100,
                    SignificanceThreshold = 0.3,
                    StoreVersionHistory = true,
                    MaxVersionsPerUrl = 10
                },
                
                // Configure content classification
                ClassificationConfig = new ClassificationConfig
                {
                    Categories = new Dictionary<string, List<string>>
                    {
                        ["Licensing"] = new List<string> 
                        { 
                            "licence", "license", "application", "personal licence", "operating licence" 
                        },
                        ["AML"] = new List<string> 
                        { 
                            "anti-money laundering", "aml", "money laundering", "terrorist financing" 
                        },
                        ["ResponsibleGambling"] = new List<string> 
                        { 
                            "responsible gambling", "player protection", "self-exclusion" 
                        },
                        ["Compliance"] = new List<string> 
                        { 
                            "compliance", "regulatory returns", "key event" 
                        },
                        ["Enforcement"] = new List<string> 
                        { 
                            "enforcement", "regulatory action", "sanction", "penalty", "fine" 
                        },
                        ["LCCP"] = new List<string> 
                        { 
                            "lccp", "licence conditions and codes of practice", "code of practice" 
                        }
                    },
                    ConfidenceThreshold = 0.5,
                    UseMachineLearning = false
                },
                
                // Configure dynamic content rendering
                DynamicContentConfig = new DynamicContentConfig
                {
                    BrowserType = "chromium",
                    MaxConcurrentSessions = 2,
                    WaitForSelector = ".gcweb-card",
                    AutoClickSelector = "#cocc-banner-accept",
                    PostNavigationDelay = 2000,
                    NavigationTimeout = 30000,
                    DisableJavaScript = false
                },
                
                // Configure alert system
                AlertSystemConfig = new AlertSystemConfig
                {
                    EnableEmailAlerts = true,
                    SmtpServer = "smtp.example.com",
                    SmtpPort = 587,
                    SmtpUsername = "alerts@example.com",
                    SmtpPassword = "password",
                    EmailSender = "alerts@example.com",
                    EmailRecipient = "regulatory-alerts@example.com",
                    AlertRules = new List<AlertRule>
                    {
                        new AlertRule
                        {
                            Name = "EnforcementAction",
                            Keywords = new List<string> { "sanction", "fine", "penalty", "regulatory action" },
                            UrlPatterns = new List<string> { "/news/enforcement-action" },
                            Importance = "High"
                        },
                        new AlertRule
                        {
                            Name = "RequirementChange",
                            Keywords = new List<string> { "must", "shall", "requirement", "obligation", "condition" },
                            UrlPatterns = new List<string> { "/licensees-and-businesses/lccp" },
                            Importance = "Medium"
                        }
                    },
                    AlertCooldownMinutes = 60
                }
            };
        }
        
        /// <summary>
        /// Exports the UKGC configuration to JSON
        /// </summary>
        public static string ExportConfigToJson()
        {
            var config = CreateUkgcConfig();
            return config.ToJson();
        }
    }
}