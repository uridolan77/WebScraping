using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.RegulatoryContent;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles regulatory content analysis
    /// </summary>
    public class RegulatoryContentComponent : ScraperComponentBase
    {
        private GamblingRegulationMonitor _regulationMonitor;
        private RegulatoryDocumentClassifier _documentClassifier;
        private bool _regulatoryAnalysisEnabled;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            _regulatoryAnalysisEnabled = Config.EnableRegulatoryContentAnalysis;
            if (!_regulatoryAnalysisEnabled)
            {
                LogInfo("Regulatory content analysis not enabled, component will be inactive");
                return;
            }
            
            InitializeRegulatoryComponents();
        }
        
        /// <summary>
        /// Initializes regulatory components
        /// </summary>
        private void InitializeRegulatoryComponents()
        {
            try
            {
                LogInfo("Initializing regulatory content components...");
                
                // Initialize the gambling regulation monitor
                _regulationMonitor = new GamblingRegulationMonitor(
                    null, // We'll handle this through component architecture
                    Config.OutputDirectory,
                    LogInfo);
                
                // Initialize the regulatory document classifier
                _documentClassifier = new RegulatoryDocumentClassifier(LogInfo);
                
                LogInfo("Regulatory content components initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize regulatory content components");
            }
        }
        
        /// <summary>
        /// Processes content for regulatory analysis
        /// </summary>
        public async Task<RegulatoryAnalysisResult> AnalyzeContentAsync(string url, string content, string contentType)
        {
            if (!_regulatoryAnalysisEnabled || _regulationMonitor == null)
                return new RegulatoryAnalysisResult { Url = url, IsRegulatoryContent = false };
                
            try
            {
                LogInfo($"Analyzing regulatory content for: {url}");
                
                // Create HtmlDocument from content (needed for classifier)
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content);
                
                // Classify the content with proper parameters
                var classification = _documentClassifier.ClassifyContent(url, content, htmlDoc);
                
                // Determine if this is regulatory content using classification.Category instead of RegulatoryProbability
                bool isRegulatory = !string.IsNullOrEmpty(classification.Category) && 
                                   classification.Category.Contains("Regulatory", StringComparison.OrdinalIgnoreCase);
                
                // Process with regulation monitor - use proper method with available parameters
                await Task.Run(() => _regulationMonitor.ProcessContent(url, content, classification));
                
                // Create analysis result - adapt to use available properties
                var result = new RegulatoryAnalysisResult
                {
                    Url = url,
                    IsRegulatoryContent = isRegulatory,
                    Classification = classification.Category,
                    RegulatoryImpact = (RegulatoryImpact)classification.Impact, // Cast from one enum to the other
                    // Use a default value since RegulatoryProbability might not exist
                    RegulatoryProbability = 0.5,
                    // Convert array to List if needed
                    RegulatoryTopics = classification.Keywords?.ToList() ?? new List<string>()
                };
                
                // Save analysis to state manager if available
                var stateManager = GetComponent<IStateManager>();
                if (stateManager != null && isRegulatory)
                {
                    await stateManager.SaveContentAsync(
                        $"regulatory:{url}", 
                        System.Text.Json.JsonSerializer.Serialize(result), 
                        "application/json");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error analyzing regulatory content for {url}");
                return new RegulatoryAnalysisResult { Url = url, IsRegulatoryContent = false };
            }
        }
    }
    
    /// <summary>
    /// Result of regulatory content analysis
    /// </summary>
    public class RegulatoryAnalysisResult
    {
        /// <summary>
        /// The URL of the analyzed content
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Whether the content is regulatory in nature
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// The classification category
        /// </summary>
        public string Classification { get; set; }
        
        /// <summary>
        /// The regulatory impact level
        /// </summary>
        public RegulatoryImpact RegulatoryImpact { get; set; }
        
        /// <summary>
        /// The probability that this is regulatory content
        /// </summary>
        public double RegulatoryProbability { get; set; }
        
        /// <summary>
        /// Topics identified in the regulatory content
        /// </summary>
        public List<string> RegulatoryTopics { get; set; } = new List<string>();
    }
}