using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using WebScraper.ContentChange;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Enhanced change detector specialized for regulatory content
    /// </summary>
    public class RegulatoryChangeDetector
    {
        private readonly ContentChangeDetector _baseDetector;
        private readonly Action<string> _logger;
        
        // Regex patterns for identifying important regulatory content
        private static readonly Dictionary<string, string> RegulatoryPatterns = new Dictionary<string, string>
        {
            { "RequirementChange", @"(?i)(must|required|mandatory|shall|condition|obligation)" },
            { "DateChange", @"(?i)(effective|from|by|deadline|due date)" },
            { "FeeChange", @"(?i)(fee|payment|cost|charge|price|amount|rate|percentage)" },
            { "PenaltyChange", @"(?i)(penalty|fine|sanction|enforcement|action)" },
            { "ProcessChange", @"(?i)(process|procedure|steps|method|application|submission)" }
        };
        
        // Keywords that indicate important regulatory changes
        private static readonly List<string> ImportantKeywords = new List<string>
        {
            "new requirement", "updated requirement", "policy change", "regulation change",
            "amendment", "update to", "revision of", "compliance deadline", "effective date",
            "license condition", "code of practice"
        };
        
        public RegulatoryChangeDetector(Action<string> logger = null)
        {
            _baseDetector = new ContentChangeDetector(logger: logger);
            _logger = logger ?? (_ => {});
        }
        
        /// <summary>
        /// Detects regulatory changes between old and new content versions
        /// </summary>
        public RegulatoryChangeResult DetectRegulatoryChanges(string url, string oldContent, string newContent)
        {
            // First, use the base detector to find changed sections
            var baseChangeType = _baseDetector.AnalyzeChanges(oldContent, newContent);
            var changedSections = _baseDetector.ExtractChangedSections(oldContent, newContent);
            
            var result = new RegulatoryChangeResult
            {
                Url = url,
                ChangeType = baseChangeType,
                DetectedAt = DateTime.Now,
                ChangedSections = changedSections,
                RegulatoryImpact = RegulatoryImpact.None
            };
            
            // If no significant changes, return early
            if (baseChangeType == ChangeType.None || baseChangeType == ChangeType.Minor)
            {
                return result;
            }
            
            // Check for specific regulatory patterns in the added content
            if (changedSections.TryGetValue("Added", out var addedContent))
            {
                foreach (var pattern in RegulatoryPatterns)
                {
                    var matches = Regex.Matches(addedContent, pattern.Value);
                    if (matches.Count > 0)
                    {
                        result.RegulatoryPatterns.Add(pattern.Key, matches.Count);
                        _logger($"Found {matches.Count} instances of {pattern.Key} pattern in added content");
                        
                        // Extract sentences containing the matches
                        foreach (Match match in matches.Take(3)) // Limit to 3 examples per pattern
                        {
                            var sentence = ExtractSentence(addedContent, match.Index);
                            result.RegulatoryExcerpts.Add(sentence);
                        }
                    }
                }
            }
            
            // Check for important keywords
            foreach (var keyword in ImportantKeywords)
            {
                if (addedContent != null && addedContent.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    result.ImportantKeywordsFound.Add(keyword);
                    _logger($"Found important keyword: {keyword} in added content");
                }
            }
            
            // Calculate regulatory impact
            result.RegulatoryImpact = CalculateRegulatoryImpact(result);
            
            // Generate impact summary
            result.ImpactSummary = GenerateImpactSummary(result);
            
            return result;
        }
        
        /// <summary>
        /// Extracts the full sentence containing a matched term
        /// </summary>
        private string ExtractSentence(string content, int matchIndex)
        {
            // Find the start of the sentence (previous period or beginning of text)
            int sentenceStart = content.LastIndexOf('.', matchIndex);
            sentenceStart = sentenceStart == -1 ? 0 : sentenceStart + 1;
            
            // Find the end of the sentence (next period or end of text)
            int sentenceEnd = content.IndexOf('.', matchIndex);
            sentenceEnd = sentenceEnd == -1 ? content.Length : sentenceEnd + 1;
            
            // Extract and trim the sentence
            return content.Substring(sentenceStart, sentenceEnd - sentenceStart).Trim();
        }
        
        /// <summary>
        /// Calculates the regulatory impact based on detected patterns and keywords
        /// </summary>
        private RegulatoryImpact CalculateRegulatoryImpact(RegulatoryChangeResult result)
        {
            int totalPatternMatches = result.RegulatoryPatterns.Values.Sum();
            int keywordCount = result.ImportantKeywordsFound.Count;
            
            if (totalPatternMatches > 10 || keywordCount > 3 || 
                result.RegulatoryPatterns.ContainsKey("RequirementChange") && result.RegulatoryPatterns["RequirementChange"] > 5)
            {
                return RegulatoryImpact.High;
            }
            else if (totalPatternMatches > 5 || keywordCount > 1)
            {
                return RegulatoryImpact.Medium;
            }
            else if (totalPatternMatches > 0 || keywordCount > 0)
            {
                return RegulatoryImpact.Low;
            }
            
            return RegulatoryImpact.None;
        }
        
        /// <summary>
        /// Generates a summary of the regulatory impact
        /// </summary>
        private string GenerateImpactSummary(RegulatoryChangeResult result)
        {
            var summary = new StringBuilder();
            
            summary.AppendLine($"Regulatory Change Analysis for: {result.Url}");
            summary.AppendLine($"Change Type: {result.ChangeType}");
            summary.AppendLine($"Regulatory Impact: {result.RegulatoryImpact}");
            summary.AppendLine();
            
            if (result.RegulatoryPatterns.Any())
            {
                summary.AppendLine("Regulatory Patterns Detected:");
                foreach (var pattern in result.RegulatoryPatterns)
                {
                    summary.AppendLine($"- {pattern.Key}: {pattern.Value} instances");
                }
                summary.AppendLine();
            }
            
            if (result.ImportantKeywordsFound.Any())
            {
                summary.AppendLine("Important Regulatory Keywords:");
                foreach (var keyword in result.ImportantKeywordsFound)
                {
                    summary.AppendLine($"- {keyword}");
                }
                summary.AppendLine();
            }
            
            if (result.RegulatoryExcerpts.Any())
            {
                summary.AppendLine("Key Regulatory Excerpts:");
                foreach (var excerpt in result.RegulatoryExcerpts.Take(5)) // Limit to 5 excerpts
                {
                    summary.AppendLine($"- \"{excerpt}\"");
                }
            }
            
            return summary.ToString();
        }
    }
    
   
    /// <summary>
    /// Results of a regulatory change analysis
    /// </summary>
    public class RegulatoryChangeResult
    {
        public string Url { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime DetectedAt { get; set; }
        public Dictionary<string, string> ChangedSections { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, int> RegulatoryPatterns { get; set; } = new Dictionary<string, int>();
        public List<string> ImportantKeywordsFound { get; set; } = new List<string>();
        public List<string> RegulatoryExcerpts { get; set; } = new List<string>();
        public RegulatoryImpact RegulatoryImpact { get; set; }
        public string ImpactSummary { get; set; }
    }
}