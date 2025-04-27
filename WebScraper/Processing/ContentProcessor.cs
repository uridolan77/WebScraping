using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using WebScraper.ContentChange;
using WebScraper.Interfaces;
using WebScraper.RegulatoryContent;
using WebScraper.RegulatoryFramework.Interfaces;
// Explicitly use the ContentChange namespace versions to resolve ambiguity
using SignificantChangesResult = WebScraper.ContentChange.SignificantChangesResult;
using ChangeType = WebScraper.ContentChange.ChangeType;

namespace WebScraper.Processing
{
    /// <summary>
    /// Implementation of content processor that handles regulatory content
    /// </summary>
    public class RegulatoryContentProcessor : IContentProcessor
    {
        private readonly ContentChangeDetector _changeDetector;
        private readonly RegulatoryDocumentClassifier _documentClassifier;
        private readonly Action<string> _logAction;
        private readonly string[] _keyTerms;

        public RegulatoryContentProcessor(Action<string> logAction, string[] keyTerms = null)
        {
            _logAction = logAction ?? (msg => { });
            _changeDetector = new ContentChangeDetector(logAction);
            _documentClassifier = new RegulatoryDocumentClassifier(logAction);
            _keyTerms = keyTerms ?? new[] { "regulation", "compliance", "gambling", "license", "requirement" };
        }

        public async Task<ProcessedContentResult> ProcessContentAsync(string url, PageContent content)
        {
            var stopwatch = Stopwatch.StartNew();
            var result = new ProcessedContentResult
            {
                ContentItem = new ContentItem
                {
                    Url = url,
                    Title = content.Title,
                    ScraperId = null, // Will be set by caller if needed
                    LastStatusCode = 200, // Assume success
                    ContentType = "text/html",
                    IsReachable = true,
                    RawContent = content.Content,
                    ContentHash = ComputeHash(content.Content)
                }
            };

            try
            {
                // Check if content is regulatory in nature
                bool isRegulatory = await _documentClassifier.IsRegulatoryDocument(url, content.Content);
                
                // Check if content contains key terms
                int keyTermsFound = _keyTerms.Count(term => 
                    content.Content.Contains(term, StringComparison.OrdinalIgnoreCase));
                
                result.IsRelevant = isRegulatory || keyTermsFound > 0;
                result.Metrics = new ContentProcessingMetrics
                {
                    TextLength = content.Content.Length,
                    ProcessingTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    KeyTermsFound = keyTermsFound
                };

                if (result.IsRelevant)
                {
                    // Extract any additional structured information if content is relevant
                    var extractor = new StructuredContentExtractor(_logAction);
                    var structuredContent = await extractor.ExtractStructuredContent(url, content.Content);
                    
                    // Since we're using ContentItem from Models.cs (not ContentItem from ContentModels.cs), 
                    // we need to handle the metadata differently as it doesn't have the same structure
                    result.ContentItem.IsRegulatoryContent = isRegulatory;
                }
            }
            catch (Exception ex)
            {
                _logAction($"Error processing content from {url}: {ex.Message}");
                result.IsRelevant = false;
            }

            return result;
        }

        public async Task<SignificantChangesResult> DetectChangesAsync(ContentItem previousVersion, ContentItem currentVersion)
        {
            if (previousVersion == null || currentVersion == null)
            {
                return new SignificantChangesResult
                {
                    HasSignificantChanges = false,
                    ChangeType = ChangeType.None,
                    Summary = "Unable to compare versions - one or both versions are missing."
                };
            }

            try
            {
                // Detect changes using the content change detector
                var changeType = _changeDetector.AnalyzeChanges(previousVersion.RawContent, currentVersion.RawContent);
                
                // Create a significantChanges result based on the change type
                var result = new SignificantChangesResult
                {
                    HasSignificantChanges = changeType != ChangeType.None,
                    ChangeType = changeType,
                    DetectedAt = DateTime.UtcNow
                };

                if (result.HasSignificantChanges)
                {
                    // Extract changed sections
                    var changedSections = _changeDetector.ExtractChangedSections(
                        previousVersion.RawContent, 
                        currentVersion.RawContent);

                    // Build a summary of changes
                    result.ChangedSections = changedSections;
                    result.Summary = BuildChangeSummary(changeType, changedSections);

                    // Look for important regulatory terms in added content
                    if (changedSections.TryGetValue("Added", out var addedContent))
                    {
                        foreach (var term in _keyTerms)
                        {
                            if (addedContent.Contains(term, StringComparison.OrdinalIgnoreCase))
                            {
                                result.AddedTerms.Add(term);
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logAction($"Error detecting changes: {ex.Message}");
                return new SignificantChangesResult
                {
                    HasSignificantChanges = false,
                    ChangeType = ChangeType.None,
                    Summary = $"Error during change detection: {ex.Message}"
                };
            }
        }

        private string BuildChangeSummary(ChangeType changeType, Dictionary<string, string> changedSections)
        {
            var summary = $"Change type: {changeType}\n\n";

            if (changedSections.TryGetValue("Added", out var added) && !string.IsNullOrEmpty(added))
            {
                summary += $"Content added ({added.Length} chars)\n";
            }

            if (changedSections.TryGetValue("Removed", out var removed) && !string.IsNullOrEmpty(removed))
            {
                summary += $"Content removed ({removed.Length} chars)\n";
            }

            return summary;
        }

        private string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}