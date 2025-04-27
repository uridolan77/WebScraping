using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Classifies regulatory documents based on their content and structure
    /// </summary>
    public class RegulatoryDocumentClassifier
    {
        private readonly Action<string> _logger;
        private readonly Dictionary<DocumentType, List<string>> _keywordMap;
        private readonly Dictionary<DocumentType, List<Regex>> _patternMap;

        public RegulatoryDocumentClassifier(Action<string> logger = null)
        {
            _logger = logger ?? (_ => { });
            
            // Initialize keyword maps for classification
            _keywordMap = new Dictionary<DocumentType, List<string>>
            {
                { DocumentType.Guidance, new List<string> { "guidance", "guide", "how to", "guidelines", "best practice", "procedures" } },
                { DocumentType.Regulation, new List<string> { "regulation", "regulatory", "requirement", "legislation", "statute", "act", "rule", "law", "directive", "mandate" } },
                { DocumentType.Consultation, new List<string> { "consultation", "feedback", "proposal", "draft", "comment", "response", "seeking views" } },
                { DocumentType.EnforcementAction, new List<string> { "enforcement", "penalty", "fine", "sanction", "breach", "compliance", "violation", "action taken" } },
                { DocumentType.PressRelease, new List<string> { "press release", "announcement", "news", "published", "statement" } },
                { DocumentType.Statistics, new List<string> { "statistics", "data", "figures", "report", "quarterly", "annual", "numbers" } },
                { DocumentType.LicensingInfo, new List<string> { "license", "licensing", "permit", "application", "approval", "authorized", "certificate" } }
            };
            
            // Initialize regex patterns for more precise matching
            _patternMap = new Dictionary<DocumentType, List<Regex>>
            {
                { 
                    DocumentType.Guidance, 
                    new List<Regex> { 
                        new Regex(@"\bguidance\s+(?:on|for|about)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\bguide\s+to\b", RegexOptions.IgnoreCase),
                        new Regex(@"\bhow\s+to\b", RegexOptions.IgnoreCase)
                    } 
                },
                { 
                    DocumentType.EnforcementAction, 
                    new List<Regex> { 
                        new Regex(@"\b(?:fined|penalized|sanctioned)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\bpenalty\s+of\s+[£$]?[\d,.]+\b", RegexOptions.IgnoreCase),
                        new Regex(@"\benforcement\s+action\b", RegexOptions.IgnoreCase)
                    } 
                },
                { 
                    DocumentType.Consultation, 
                    new List<Regex> { 
                        new Regex(@"\bconsultation\s+(?:on|about)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(?:seeking|request|invite)\s+(?:views|feedback|comments)\b", RegexOptions.IgnoreCase),
                        new Regex(@"\b(?:response|replies)\s+to\s+consultation\b", RegexOptions.IgnoreCase)
                    } 
                }
            };
        }

        /// <summary>
        /// Regulatory document types
        /// </summary>
        public enum DocumentType
        {
            Guidance,
            Regulation,
            Consultation,
            EnforcementAction,
            PressRelease,
            Statistics,
            LicensingInfo,
            Other
        }

        /// <summary>
        /// Classification result with confidence score and reasons
        /// </summary>
        public class ClassificationResult
        {
            public DocumentType PrimaryType { get; set; }
            public DocumentType? SecondaryType { get; set; }
            public double Confidence { get; set; }
            public Dictionary<DocumentType, double> TypeScores { get; set; } = new Dictionary<DocumentType, double>();
            public List<string> MatchedKeywords { get; set; } = new List<string>();
        }

        /// <summary>
        /// Classifies document content by analyzing text and URL patterns
        /// </summary>
        public ClassificationResult ClassifyContent(string url, string content, HtmlDocument document = null)
        {
            // Initialize scores for each document type
            var scores = Enum.GetValues(typeof(DocumentType))
                .Cast<DocumentType>()
                .ToDictionary(type => type, type => 0.0);
            
            var matchedKeywords = new List<string>();
            
            // Process URL for clues (worth 30% of the total score)
            AnalyzeUrl(url, scores);
            
            // Process content for keywords and patterns (worth 50% of the total score)
            AnalyzeContent(content, scores, matchedKeywords);
            
            // Process HTML structure if available (worth 20% of the total score)
            if (document != null)
            {
                AnalyzeStructure(document, scores);
            }
            
            // Determine primary and secondary types
            var sortedTypes = scores.OrderByDescending(s => s.Value).ToList();
            var primaryType = sortedTypes[0].Key;
            var primaryScore = sortedTypes[0].Value;
            
            DocumentType? secondaryType = null;
            if (sortedTypes.Count > 1 && sortedTypes[1].Value > 0)
            {
                secondaryType = sortedTypes[1].Key;
            }
            
            // Calculate confidence score (normalized to 0-1 range)
            double confidence = primaryScore / 10.0; // Assuming 10 is max possible score
            if (confidence > 1.0) confidence = 1.0;
            
            _logger($"Classified document as {primaryType} with {confidence:P0} confidence");
            
            return new ClassificationResult
            {
                PrimaryType = primaryType,
                SecondaryType = secondaryType,
                Confidence = confidence,
                TypeScores = scores,
                MatchedKeywords = matchedKeywords
            };
        }

        /// <summary>
        /// Adds custom keywords for document type classification
        /// </summary>
        public void AddCustomKeywords(DocumentType documentType, IEnumerable<string> keywords)
        {
            if (!_keywordMap.ContainsKey(documentType))
            {
                _keywordMap[documentType] = new List<string>();
            }
            
            _keywordMap[documentType].AddRange(keywords);
            _logger($"Added {keywords.Count()} custom keywords for {documentType}");
        }

        /// <summary>
        /// Determines whether a document is regulatory in nature based on its content
        /// </summary>
        /// <param name="url">URL of the document</param>
        /// <param name="content">Document content</param>
        /// <returns>True if the document is regulatory, false otherwise</returns>
        public async Task<bool> IsRegulatoryDocument(string url, string content)
        {
            // Use the classification engine to determine if this is regulatory
            var result = ClassifyContent(url, content);
            
            // Consider it regulatory if it's classified as a regulation or has high confidence
            // in being guidance, enforcement action, or licensing info
            bool isRegulatory = result.PrimaryType == DocumentType.Regulation ||
                              (result.Confidence > 0.7 && 
                               (result.PrimaryType == DocumentType.Guidance ||
                                result.PrimaryType == DocumentType.EnforcementAction ||
                                result.PrimaryType == DocumentType.LicensingInfo));
                                
            _logger($"Document at {url} is {(isRegulatory ? "regulatory" : "non-regulatory")} with confidence {result.Confidence:P0}");
            
            return await Task.FromResult(isRegulatory);
        }

        private void AnalyzeUrl(string url, Dictionary<DocumentType, double> scores)
        {
            url = url.ToLower();
            
            // Check for URL path segments that indicate document type
            foreach (var type in _keywordMap.Keys)
            {
                foreach (var keyword in _keywordMap[type])
                {
                    if (url.Contains("/" + keyword) || url.Contains("-" + keyword) || url.Contains(keyword + "/"))
                    {
                        scores[type] += 1.5; // URL structure is a strong indicator
                    }
                    else if (url.Contains(keyword))
                    {
                        scores[type] += 0.5; // Keyword appears in URL
                    }
                }
            }
            
            // Check for file extensions
            if (url.EndsWith(".pdf"))
            {
                // PDFs are often formal regulatory documents
                scores[DocumentType.Regulation] += 0.5;
                scores[DocumentType.Guidance] += 0.5;
            }
            
            // Check for specific URL patterns common in regulatory sites
            if (Regex.IsMatch(url, @"\/regulation|\/legislation|\/laws"))
                scores[DocumentType.Regulation] += 2.0;
                
            if (Regex.IsMatch(url, @"\/guide|\/guidance|\/help"))
                scores[DocumentType.Guidance] += 2.0;
                
            if (Regex.IsMatch(url, @"\/consult|\/consultation|\/feedback"))
                scores[DocumentType.Consultation] += 2.0;
                
            if (Regex.IsMatch(url, @"\/enforcement|\/action|\/penalty|\/fine"))
                scores[DocumentType.EnforcementAction] += 2.0;
                
            if (Regex.IsMatch(url, @"\/press|\/news|\/media|\/announcement"))
                scores[DocumentType.PressRelease] += 2.0;
                
            if (Regex.IsMatch(url, @"\/stat|\/data|\/figures|\/report"))
                scores[DocumentType.Statistics] += 2.0;
                
            if (Regex.IsMatch(url, @"\/licens|\/permit|\/application|\/apply"))
                scores[DocumentType.LicensingInfo] += 2.0;
        }

        private void AnalyzeContent(string content, Dictionary<DocumentType, double> scores, List<string> matchedKeywords)
        {
            content = content.ToLower();
            
            // Check for keywords in content
            foreach (var type in _keywordMap.Keys)
            {
                foreach (var keyword in _keywordMap[type])
                {
                    // Simple keyword counting (with word boundary check)
                    string pattern = $@"\b{Regex.Escape(keyword)}\b";
                    var matches = Regex.Matches(content, pattern);
                    
                    if (matches.Count > 0)
                    {
                        // Add more weight to keywords that appear multiple times
                        double occurrenceScore = Math.Min(matches.Count * 0.2, 2.0); // Cap at 2.0
                        scores[type] += occurrenceScore;
                        
                        // Only add each keyword once to the matched list
                        if (!matchedKeywords.Contains(keyword))
                        {
                            matchedKeywords.Add(keyword);
                        }
                    }
                }
                
                // Check for regex patterns if defined for this type
                if (_patternMap.ContainsKey(type))
                {
                    foreach (var pattern in _patternMap[type])
                    {
                        var matches = pattern.Matches(content);
                        if (matches.Count > 0)
                        {
                            // Patterns are stronger indicators than simple keywords
                            scores[type] += Math.Min(matches.Count * 0.5, 2.5); // Cap at 2.5
                        }
                    }
                }
            }
            
            // Look for dates (common in regulated documents)
            var dateMatches = Regex.Matches(content, @"\b\d{1,2}\/\d{1,2}\/\d{2,4}\b|\b\d{1,2}\s+(?:January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{4}\b");
            
            if (dateMatches.Count > 0)
            {
                // Documents with many dates are often regulatory or statistical
                if (dateMatches.Count > 5)
                {
                    scores[DocumentType.Regulation] += 0.5;
                    scores[DocumentType.Statistics] += 1.0;
                }
            }
            
            // Look for monetary amounts (common in enforcement actions)
            var moneyMatches = Regex.Matches(content, @"[£$€]\s*[\d,]+(?:\.\d+)?|\b\d+\s*(?:pounds|dollars|euros)\b");
            
            if (moneyMatches.Count > 0)
            {
                scores[DocumentType.EnforcementAction] += Math.Min(moneyMatches.Count * 0.3, 1.5);
            }
            
            // Check for percentage figures (common in statistics and reports)
            var percentageMatches = Regex.Matches(content, @"\b\d+(?:\.\d+)?\s*%");
            
            if (percentageMatches.Count > 3)
            {
                scores[DocumentType.Statistics] += Math.Min(percentageMatches.Count * 0.2, 1.0);
            }
        }

        private void AnalyzeStructure(HtmlDocument document, Dictionary<DocumentType, double> scores)
        {
            // Title is often a strong indicator
            var titleNode = document.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null)
            {
                string titleText = titleNode.InnerText.ToLower();
                
                foreach (var type in _keywordMap.Keys)
                {
                    foreach (var keyword in _keywordMap[type])
                    {
                        if (titleText.Contains(keyword))
                        {
                            scores[type] += 1.5; // Title keywords are strong indicators
                        }
                    }
                }
            }
            
            // Check for tables (common in statistical documents)
            var tables = document.DocumentNode.SelectNodes("//table");
            if (tables != null && tables.Count > 0)
            {
                scores[DocumentType.Statistics] += Math.Min(tables.Count * 0.5, 1.5);
            }
            
            // Check for forms (common in licensing information)
            var forms = document.DocumentNode.SelectNodes("//form");
            if (forms != null && forms.Count > 0)
            {
                scores[DocumentType.LicensingInfo] += 1.0;
            }
            
            // Check for PDF links (common in regulatory documents)
            var pdfLinks = document.DocumentNode.SelectNodes("//a[contains(@href, '.pdf')]");
            if (pdfLinks != null && pdfLinks.Count > 0)
            {
                scores[DocumentType.Regulation] += 0.5;
                scores[DocumentType.Guidance] += 0.5;
            }
            
            // Meta description can be insightful
            var metaDescription = document.DocumentNode.SelectSingleNode("//meta[@name='description']");
            if (metaDescription != null)
            {
                string description = metaDescription.GetAttributeValue("content", "").ToLower();
                
                foreach (var type in _keywordMap.Keys)
                {
                    foreach (var keyword in _keywordMap[type])
                    {
                        if (description.Contains(keyword))
                        {
                            scores[type] += 0.5;
                        }
                    }
                }
            }
        }
    }
}