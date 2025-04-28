using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.ContentChange;
using WebScraper.RegulatoryFramework.Implementation;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Specialized monitor for gambling regulation websites that integrates
    /// all the regulatory components
    /// </summary>
    public class GamblingRegulationMonitor
    {
        private readonly Scraper? _scraper;
        private readonly UKGCCrawlStrategy _crawlStrategy;
        private readonly RegulatoryDocumentClassifier _documentClassifier;
        private readonly RegulatoryChangeDetector _changeDetector;
        private readonly StructuredContentExtractor _contentExtractor;
        private readonly PdfDocumentHandler _pdfHandler;
        private readonly Action<string> _logger;
        
        // Track important regulatory documents and changes
        private readonly Dictionary<string, RegulatoryDocument> _regulatoryDocuments = new Dictionary<string, RegulatoryDocument>();
        private readonly List<RegulatoryChangeResult> _regulatoryChanges = new List<RegulatoryChangeResult>();
        
        public GamblingRegulationMonitor(
            Scraper? scraper = null,
            string? outputDirectory = null,
            Action<string>? logger = null)
        {
            _scraper = scraper;
            _logger = logger ?? (_ => { });
            
            // Initialize specialized components
            _crawlStrategy = new UKGCCrawlStrategy(_logger);
            _documentClassifier = new RegulatoryDocumentClassifier(_logger);
            _changeDetector = new RegulatoryChangeDetector(_logger);
            _contentExtractor = new StructuredContentExtractor(_logger);
            _pdfHandler = new PdfDocumentHandler(outputDirectory ?? "documents", logger: _logger);
            
            // Add gambling industry specific keywords to the document classifier
            AddGamblingSpecificKeywords();
        }
        
        /// <summary>
        /// Represents a regulatory document with its classification and structure
        /// </summary>
        public class RegulatoryDocument
        {
            public string Url { get; set; } = string.Empty;
            public string Title { get; set; } = string.Empty;
            public RegulatoryDocumentClassifier.DocumentType DocumentType { get; set; }
            public double ClassificationConfidence { get; set; }
            public DateTime? PublishedDate { get; set; }
            public string Category { get; set; } = string.Empty;
            public StructuredContentExtractor.ContentSection? StructuredContent { get; set; }
            public List<string> KeyTerms { get; set; } = new List<string>();
            public bool IsPdf { get; set; }
            public RegulatoryImportance Importance { get; set; }
            public DateTime ProcessedDate { get; set; } = DateTime.Now;
        }
        
        /// <summary>
        /// Importance level of a regulatory document
        /// </summary>
        public enum RegulatoryImportance
        {
            Low,
            Medium,
            High,
            Critical
        }
        
        /// <summary>
        /// Process a regulatory page with enhanced handling
        /// </summary>
        public async Task<RegulatoryDocument?> ProcessRegulatoryPage(string url, HtmlDocument? document, string content, string textContent)
        {
            _logger($"Processing regulatory page: {url}");
            
            try
            {
                var isPdf = url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase);
                
                // Extract structured content (for HTML pages)
                StructuredContentExtractor.ContentSection? structuredContent = null;
                if (!isPdf && document != null)
                {
                    structuredContent = _contentExtractor.ExtractStructuredContent(document, url);
                }
                
                // Get page title
                string? title = structuredContent?.Title;
                if (string.IsNullOrEmpty(title) && document != null)
                {
                    title = document.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? 
                            document.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim() ??
                            "Untitled Document";
                }
                
                // Classify document
                var classification = _documentClassifier.ClassifyContent(url, textContent, document);
                
                // Extract publication date from structured content
                var publishedDate = structuredContent?.PublishedDate;
                
                // Create regulatory document
                var regulatoryDoc = new RegulatoryDocument
                {
                    Url = url,
                    Title = title ?? "Untitled Document",
                    DocumentType = classification.PrimaryType,
                    ClassificationConfidence = classification.Confidence,
                    PublishedDate = publishedDate,
                    Category = structuredContent?.Category ?? "Uncategorized",
                    StructuredContent = structuredContent,
                    KeyTerms = classification.MatchedKeywords,
                    IsPdf = isPdf,
                    Importance = DetermineImportance(classification, url, structuredContent)
                };
                
                // Store the document
                _regulatoryDocuments[url] = regulatoryDoc;
                
                // If it's a PDF document, save metadata
                if (isPdf)
                {
                    await _pdfHandler.SavePdfMetadata(
                        url, 
                        title ?? "Untitled Document", 
                        publishedDate,
                        regulatoryDoc.Category,
                        new Dictionary<string, string>
                        {
                            ["DocumentType"] = classification.PrimaryType.ToString(),
                            ["Importance"] = regulatoryDoc.Importance.ToString(),
                            ["ProcessedDate"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                        }
                    );
                }
                
                _logger($"Classified {url} as {classification.PrimaryType} with {classification.Confidence:P0} confidence");
                
                return regulatoryDoc;
            }
            catch (Exception ex)
            {
                _logger($"Error processing regulatory page {url}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Process content for regulatory analysis
        /// </summary>
        public async Task ProcessContent(string url, string content, RegulatoryDocumentClassifier.ClassificationResult classification)
        {
            _logger($"Processing regulatory content from: {url}");
            
            try
            {
                // Parse as HTML if possible
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(content);
                
                // Extract text content
                string textContent = htmlDoc.DocumentNode.InnerText;
                
                // Create a regulatory document from the content - note we're using the classification passed in
                // not calling ClassifyContent again which would require a document parameter
                var regulatoryDoc = new RegulatoryDocument
                {
                    Url = url,
                    Title = htmlDoc.DocumentNode.SelectSingleNode("//title")?.InnerText?.Trim() ?? 
                           htmlDoc.DocumentNode.SelectSingleNode("//h1")?.InnerText?.Trim() ?? 
                           "Untitled Document",
                    DocumentType = classification.PrimaryType,
                    ClassificationConfidence = classification.Confidence,
                    Category = classification.Category ?? "Uncategorized",
                    KeyTerms = classification.MatchedKeywords,
                    IsPdf = url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase),
                    Importance = DetermineImportance(classification, url, null)
                };
                
                // Store the document
                _regulatoryDocuments[url] = regulatoryDoc;
                
                _logger($"Processed regulatory content from {url} as {classification.PrimaryType} with {classification.Confidence:P0} confidence");
                
                // Add an await operation to properly use the async keyword
                await Task.Delay(1); // Minimal delay to make the async keyword meaningful
            }
            catch (Exception ex)
            {
                _logger($"Error processing content from {url}: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Monitor for changes in a regulatory page
        /// </summary>
        public async Task<RegulatoryChangeResult?> MonitorForChanges(string url, string oldContent, string newContent)
        {
            try
            {
                var changeResult = _changeDetector.DetectRegulatoryChanges(url, oldContent, newContent);
                
                if (changeResult.RegulatoryImpact > RegulatoryImpact.None)
                {
                    _regulatoryChanges.Add(changeResult);
                    _logger($"Detected {changeResult.RegulatoryImpact} impact change at {url}");
                    
                    // For high impact changes, log detailed information
                    if (changeResult.RegulatoryImpact >= RegulatoryImpact.Medium)
                    {
                        _logger($"Significant regulatory change detected!\n{changeResult.ImpactSummary}");
                    }
                    
                    // Add an await operation to properly use the async keyword
                    await Task.Delay(1); // Minimal delay to make the async keyword meaningful
                }
                
                return changeResult;
            }
            catch (Exception ex)
            {
                _logger($"Error monitoring changes for {url}: {ex.Message}");
                return null;
            }
        }
        
        /// <summary>
        /// Return statistics about monitored regulatory content
        /// </summary>
        public string GetRegulatoryStatistics()
        {
            var stats = new StringBuilder();
            stats.AppendLine("Gambling Regulation Monitoring Statistics:");
            stats.AppendLine($"Total regulatory documents: {_regulatoryDocuments.Count}");
            
            // Count by document type
            var docTypeCount = new Dictionary<RegulatoryDocumentClassifier.DocumentType, int>();
            var importanceCount = new Dictionary<RegulatoryImportance, int>();
            
            foreach (var doc in _regulatoryDocuments.Values)
            {
                // Count document types
                if (!docTypeCount.ContainsKey(doc.DocumentType))
                {
                    docTypeCount[doc.DocumentType] = 0;
                }
                docTypeCount[doc.DocumentType]++;
                
                // Count importance levels
                if (!importanceCount.ContainsKey(doc.Importance))
                {
                    importanceCount[doc.Importance] = 0;
                }
                importanceCount[doc.Importance]++;
            }
            
            // Add document types to statistics
            stats.AppendLine("\nDocument Types:");
            foreach (var type in docTypeCount)
            {
                stats.AppendLine($"- {type.Key}: {type.Value}");
            }
            
            // Add importance levels to statistics
            stats.AppendLine("\nImportance Levels:");
            foreach (var importance in importanceCount)
            {
                stats.AppendLine($"- {importance.Key}: {importance.Value}");
            }
            
            // Add change statistics
            stats.AppendLine($"\nRegulatory changes detected: {_regulatoryChanges.Count}");
            var impactCounts = new Dictionary<RegulatoryImpact, int>();
            foreach (var change in _regulatoryChanges)
            {
                if (!impactCounts.ContainsKey(change.RegulatoryImpact))
                {
                    impactCounts[change.RegulatoryImpact] = 0;
                }
                impactCounts[change.RegulatoryImpact]++;
            }
            
            stats.AppendLine("\nRegulatory Impact Levels:");
            foreach (var impact in impactCounts)
            {
                stats.AppendLine($"- {impact.Key}: {impact.Value}");
            }
            
            return stats.ToString();
        }

        /// <summary>
        /// Get high importance regulatory documents
        /// </summary>
        public List<RegulatoryDocument> GetHighImportanceDocuments()
        {
            var result = new List<RegulatoryDocument>();
            
            foreach (var doc in _regulatoryDocuments.Values)
            {
                if (doc.Importance == RegulatoryImportance.High || doc.Importance == RegulatoryImportance.Critical)
                {
                    result.Add(doc);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Get latest regulatory changes with high impact
        /// </summary>
        public List<RegulatoryChangeResult> GetHighImpactChanges()
        {
            return _regulatoryChanges.FindAll(c => 
                c.RegulatoryImpact == RegulatoryImpact.High || 
                c.RegulatoryImpact == RegulatoryImpact.Medium);
        }
        
        /// <summary>
        /// Get the crawl strategy specialized for gambling regulation
        /// </summary>
        public UKGCCrawlStrategy GetCrawlStrategy()
        {
            return _crawlStrategy;
        }
        
        /// <summary>
        /// Determine importance of a regulatory document
        /// </summary>
        private RegulatoryImportance DetermineImportance(
            RegulatoryDocumentClassifier.ClassificationResult classification, 
            string url, 
            StructuredContentExtractor.ContentSection? content)
        {
            // Critical importance for enforcement actions 
            if (classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.EnforcementAction &&
                classification.Confidence > 0.7)
            {
                return RegulatoryImportance.Critical;
            }
            
            // Critical importance for LCCP (License Conditions and Codes of Practice)
            if (url.Contains("/lccp/") || 
                (content?.Title?.Contains("LCCP") == true) ||
                (classification.MatchedKeywords.Contains("license conditions") || 
                 classification.MatchedKeywords.Contains("code of practice")))
            {
                return RegulatoryImportance.Critical;
            }
            
            // High importance for regulatory documents with high confidence
            if ((classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.Regulation ||
                classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.Guidance) &&
                classification.Confidence > 0.8)
            {
                return RegulatoryImportance.High;
            }
            
            // High importance for money laundering related content
            if (url.Contains("/aml/") || 
                (content?.Title?.Contains("money laundering") == true) ||
                classification.MatchedKeywords.Contains("anti-money laundering"))
            {
                return RegulatoryImportance.High;
            }
            
            // Medium importance for other regulatory content with moderate confidence
            if ((classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.Regulation ||
                classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.Guidance ||
                classification.PrimaryType == RegulatoryDocumentClassifier.DocumentType.Consultation) &&
                classification.Confidence > 0.5)
            {
                return RegulatoryImportance.Medium;
            }
            
            // Low importance for everything else
            return RegulatoryImportance.Low;
        }
        
        /// <summary>
        /// Add gambling industry specific keywords to the document classifier
        /// </summary>
        private void AddGamblingSpecificKeywords()
        {
            var gamblingKeywords = _crawlStrategy.GetGamblingRegulationKeywords();
            
            // Add AML-related keywords
            _documentClassifier.AddCustomKeywords(
                RegulatoryDocumentClassifier.DocumentType.Regulation,
                gamblingKeywords["AML"]);
                
            // Add licensing-related keywords
            _documentClassifier.AddCustomKeywords(
                RegulatoryDocumentClassifier.DocumentType.LicensingInfo,
                gamblingKeywords["Licensing"]);
                
            // Add responsible gambling keywords
            _documentClassifier.AddCustomKeywords(
                RegulatoryDocumentClassifier.DocumentType.Guidance,
                gamblingKeywords["Responsible Gambling"]);
                
            // Add LCCP-related keywords
            _documentClassifier.AddCustomKeywords(
                RegulatoryDocumentClassifier.DocumentType.Regulation,
                gamblingKeywords["LCCP"]);
                
            // Add enforcement-related keywords
            _documentClassifier.AddCustomKeywords(
                RegulatoryDocumentClassifier.DocumentType.EnforcementAction,
                gamblingKeywords["Enforcement"]);
        }
    }
}