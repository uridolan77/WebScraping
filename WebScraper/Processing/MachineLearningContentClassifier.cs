using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Processing.Interfaces;
using WebScraper.Processing.Models;

namespace WebScraper.Processing
{
    /// <summary>
    /// Classifies content using basic NLP techniques
    /// </summary>
    public class MachineLearningContentClassifier
    {
        private readonly ILogger _logger;
        private readonly List<string> _positiveKeywords;
        private readonly List<string> _negativeKeywords;
        private readonly List<string> _regulatoryKeywords;
        private readonly Dictionary<string, double> _keywordWeights;

        // Advanced analyzers
        private readonly ITextAnalyzer _textAnalyzer;
        private readonly ISentimentAnalyzer _sentimentAnalyzer;
        private readonly IEntityRecognizer _entityRecognizer;

        /// <summary>
        /// Creates a new instance of the MachineLearningContentClassifier
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public MachineLearningContentClassifier(ILogger<MachineLearningContentClassifier> logger)
        {
            _logger = logger;
            _textAnalyzer = null;
            _sentimentAnalyzer = null;
            _entityRecognizer = null;

            // Initialize keyword lists
            _positiveKeywords = new List<string> {
                "regulation", "compliance", "policy", "requirement", "mandatory",
                "must", "shall", "required", "obligation", "enforce", "law",
                "legal", "statutory", "regulatory", "legislation", "directive"
            };

            _negativeKeywords = new List<string> {
                "optional", "suggested", "recommended", "may", "might",
                "consider", "possibly", "perhaps", "option", "alternative",
                "voluntary", "discretionary", "flexible", "non-mandatory"
            };

            _regulatoryKeywords = new List<string> {
                "gambling", "financial", "banking", "insurance", "credit",
                "loan", "mortgage", "investment", "securities", "trading",
                "compliance", "regulation", "regulatory", "license", "permit",
                "authorization", "approval", "certification", "registration",
                "audit", "inspection", "enforcement", "penalty", "fine",
                "sanction", "violation", "breach", "infringement", "non-compliance"
            };

            // Initialize keyword weights
            _keywordWeights = new Dictionary<string, double>();
            foreach (var keyword in _positiveKeywords)
            {
                _keywordWeights[keyword] = 1.0;
            }
            foreach (var keyword in _negativeKeywords)
            {
                _keywordWeights[keyword] = -0.5;
            }
            foreach (var keyword in _regulatoryKeywords)
            {
                _keywordWeights[keyword] = 1.5;
            }
        }

        /// <summary>
        /// Creates a new instance of the MachineLearningContentClassifier with advanced analyzers
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="textAnalyzer">Text analyzer</param>
        /// <param name="sentimentAnalyzer">Sentiment analyzer</param>
        /// <param name="entityRecognizer">Entity recognizer</param>
        public MachineLearningContentClassifier(
            ILogger<MachineLearningContentClassifier> logger,
            ITextAnalyzer textAnalyzer,
            ISentimentAnalyzer sentimentAnalyzer,
            IEntityRecognizer entityRecognizer)
        {
            _logger = logger;
            _textAnalyzer = textAnalyzer;
            _sentimentAnalyzer = sentimentAnalyzer;
            _entityRecognizer = entityRecognizer;

            // Initialize keyword lists (for fallback)
            _positiveKeywords = new List<string> {
                "regulation", "compliance", "policy", "requirement", "mandatory",
                "must", "shall", "required", "obligation", "enforce", "law",
                "legal", "statutory", "regulatory", "legislation", "directive"
            };

            _negativeKeywords = new List<string> {
                "optional", "suggested", "recommended", "may", "might",
                "consider", "possibly", "perhaps", "option", "alternative",
                "voluntary", "discretionary", "flexible", "non-mandatory"
            };

            _regulatoryKeywords = new List<string> {
                "gambling", "financial", "banking", "insurance", "credit",
                "loan", "mortgage", "investment", "securities", "trading",
                "compliance", "regulation", "regulatory", "license", "permit",
                "authorization", "approval", "certification", "registration",
                "audit", "inspection", "enforcement", "penalty", "fine",
                "sanction", "violation", "breach", "infringement", "non-compliance"
            };

            // Initialize keyword weights
            _keywordWeights = new Dictionary<string, double>();
            foreach (var keyword in _positiveKeywords)
            {
                _keywordWeights[keyword] = 1.0;
            }
            foreach (var keyword in _negativeKeywords)
            {
                _keywordWeights[keyword] = -0.5;
            }
            foreach (var keyword in _regulatoryKeywords)
            {
                _keywordWeights[keyword] = 1.5;
            }
        }

        /// <summary>
        /// Classifies content based on text analysis
        /// </summary>
        /// <param name="content">Content to classify</param>
        /// <returns>Classification result</returns>
        public ContentClassification ClassifyContent(string content)
        {
            var classification = new ContentClassification();

            try
            {
                // Calculate basic text metrics
                classification.ContentLength = content.Length;
                classification.SentenceCount = CountSentences(content);
                classification.ParagraphCount = CountParagraphs(content);
                classification.ReadabilityScore = CalculateReadabilityScore(content);

                // Calculate sentiment scores
                CalculateSentiment(content, classification);

                // Extract entities
                classification.Entities = ExtractEntities(content);

                // Determine document type
                classification.DocumentType = DetermineDocumentType(content, classification);

                // Calculate confidence
                classification.Confidence = CalculateConfidence(classification);

                _logger.LogInformation($"Classified content with {classification.Confidence:P0} confidence as {classification.DocumentType}");

                return classification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying content");
                classification.Error = ex.Message;
                return classification;
            }
        }

        /// <summary>
        /// Classifies content using machine learning techniques asynchronously
        /// </summary>
        /// <param name="content">The content to classify</param>
        /// <returns>Content classification result</returns>
        public async Task<Models.ContentClassification> ClassifyContentAsync(string content)
        {
            _logger.LogInformation("Classifying content using machine learning asynchronously");

            var classification = new Models.ContentClassification();

            try
            {
                // Check if advanced analyzers are available
                if (_textAnalyzer == null || _sentimentAnalyzer == null || _entityRecognizer == null)
                {
                    _logger.LogWarning("Advanced analyzers are not available. Using basic classification.");

                    // Use the basic classification method and convert the result
                    var basicResult = ClassifyContent(content);

                    // Map basic result to advanced model
                    classification.ContentLength = basicResult.ContentLength;
                    classification.SentenceCount = basicResult.SentenceCount;
                    classification.ParagraphCount = basicResult.ParagraphCount;
                    classification.ReadabilityScore = basicResult.ReadabilityScore;
                    classification.PositiveScore = basicResult.PositiveScore;
                    classification.NegativeScore = basicResult.NegativeScore;
                    classification.OverallSentiment = basicResult.OverallSentiment;
                    classification.DocumentType = basicResult.DocumentType;
                    classification.Confidence = basicResult.Confidence;
                    classification.Error = basicResult.Error;

                    // Map entities
                    classification.Entities = basicResult.Entities.Select(e => new Models.Entity
                    {
                        Type = e.Type,
                        Value = e.Value,
                        Position = e.Position,
                        Confidence = 0.7 // Default confidence for basic entities
                    }).ToList();

                    return classification;
                }

                // Use advanced analyzers

                // Analyze text features
                var textFeatures = await _textAnalyzer.AnalyzeAsync(content);
                classification.ContentLength = textFeatures.Length;
                classification.SentenceCount = textFeatures.SentenceCount;
                classification.ParagraphCount = textFeatures.ParagraphCount;
                classification.ReadabilityScore = textFeatures.ReadabilityScore;

                // Analyze sentiment
                var sentiment = await _sentimentAnalyzer.AnalyzeSentimentAsync(content);
                classification.PositiveScore = sentiment.PositiveScore;
                classification.NegativeScore = sentiment.NegativeScore;
                classification.OverallSentiment = sentiment.OverallSentiment;

                // Extract entities
                var recognizedEntities = await _entityRecognizer.RecognizeEntitiesAsync(content);
                classification.Entities = recognizedEntities.Select(e => new Models.Entity
                {
                    Type = e.Type,
                    Value = e.Value,
                    Position = e.Position,
                    Confidence = e.Confidence
                }).ToList();

                // Determine document type and confidence
                classification.DocumentType = DetermineDocumentType(textFeatures, sentiment, recognizedEntities);
                classification.Confidence = CalculateConfidence(textFeatures, sentiment, recognizedEntities);

                _logger.LogInformation($"Classified content with {classification.Confidence:P0} confidence as {classification.DocumentType} using advanced analyzers");

                return classification;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in advanced content classification");
                classification.Error = ex.Message;
                return classification;
            }
        }

        /// <summary>
        /// Counts sentences in text
        /// </summary>
        private int CountSentences(string text)
        {
            // Simple sentence counting based on punctuation
            return Regex.Matches(text, @"[.!?]+").Count;
        }

        /// <summary>
        /// Counts paragraphs in text
        /// </summary>
        private int CountParagraphs(string text)
        {
            // Count double line breaks as paragraph separators
            return Regex.Matches(text, @"\n\s*\n").Count + 1;
        }

        /// <summary>
        /// Calculates a basic readability score
        /// </summary>
        private double CalculateReadabilityScore(string text)
        {
            // Simple readability score based on average word and sentence length
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var sentences = Regex.Matches(text, @"[.!?]+");

            if (sentences.Count == 0 || words.Length == 0)
                return 0;

            double avgWordLength = words.Average(w => w.Length);
            double avgSentenceLength = words.Length / (double)sentences.Count;

            // Higher score means more complex text
            return (avgWordLength * 0.39) + (avgSentenceLength * 0.05);
        }

        /// <summary>
        /// Calculates sentiment scores
        /// </summary>
        private void CalculateSentiment(string text, ContentClassification classification)
        {
            // Calculate positive score
            int positiveScore = 0;
            foreach (var keyword in _positiveKeywords)
            {
                string pattern = $@"\b{Regex.Escape(keyword)}\b";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                positiveScore += matches.Count;
            }

            // Calculate negative score
            int negativeScore = 0;
            foreach (var keyword in _negativeKeywords)
            {
                string pattern = $@"\b{Regex.Escape(keyword)}\b";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                negativeScore += matches.Count;
            }

            // Set scores
            classification.PositiveScore = positiveScore;
            classification.NegativeScore = negativeScore;

            // Determine overall sentiment
            if (positiveScore > negativeScore * 2)
                classification.OverallSentiment = "Positive";
            else if (negativeScore > positiveScore * 2)
                classification.OverallSentiment = "Negative";
            else
                classification.OverallSentiment = "Neutral";
        }

        /// <summary>
        /// Extracts entities from text
        /// </summary>
        private List<Entity> ExtractEntities(string text)
        {
            var entities = new List<Entity>();

            // Extract dates
            var datePatterns = new[]
            {
                @"\b(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{1,2},\s+\d{4}\b",
                @"\b\d{1,2}\s+(January|February|March|April|May|June|July|August|September|October|November|December)\s+\d{4}\b",
                @"\b\d{1,2}/\d{1,2}/\d{4}\b",
                @"\b\d{4}-\d{2}-\d{2}\b"
            };

            foreach (var pattern in datePatterns)
            {
                var matches = Regex.Matches(text, pattern);
                foreach (Match match in matches)
                {
                    entities.Add(new Entity
                    {
                        Type = "Date",
                        Value = match.Value,
                        Position = match.Index
                    });
                }
            }

            // Extract monetary values
            var moneyPattern = @"\$\d+(?:\.\d{2})?|\d+(?:\.\d{2})?\s+(?:dollars|pounds|euros)";
            var moneyMatches = Regex.Matches(text, moneyPattern, RegexOptions.IgnoreCase);
            foreach (Match match in moneyMatches)
            {
                entities.Add(new Entity
                {
                    Type = "Money",
                    Value = match.Value,
                    Position = match.Index
                });
            }

            // Extract percentages
            var percentPattern = @"\d+(?:\.\d+)?\s*%";
            var percentMatches = Regex.Matches(text, percentPattern);
            foreach (Match match in percentMatches)
            {
                entities.Add(new Entity
                {
                    Type = "Percentage",
                    Value = match.Value,
                    Position = match.Index
                });
            }

            // Extract organizations (simple approach)
            var orgPattern = @"\b[A-Z][a-z]+(?:\s+[A-Z][a-z]+)+\b";
            var orgMatches = Regex.Matches(text, orgPattern);
            foreach (Match match in orgMatches)
            {
                // Filter out common non-organization phrases
                if (!match.Value.Contains("January") &&
                    !match.Value.Contains("February") &&
                    !match.Value.Contains("March") &&
                    !match.Value.Contains("April") &&
                    !match.Value.Contains("May") &&
                    !match.Value.Contains("June") &&
                    !match.Value.Contains("July") &&
                    !match.Value.Contains("August") &&
                    !match.Value.Contains("September") &&
                    !match.Value.Contains("October") &&
                    !match.Value.Contains("November") &&
                    !match.Value.Contains("December"))
                {
                    entities.Add(new Entity
                    {
                        Type = "Organization",
                        Value = match.Value,
                        Position = match.Index
                    });
                }
            }

            return entities;
        }

        /// <summary>
        /// Determines the document type based on content analysis
        /// </summary>
        private string DetermineDocumentType(string text, ContentClassification classification)
        {
            // Calculate scores for each document type
            double regulationScore = 0;
            double guidanceScore = 0;
            double newsScore = 0;
            double generalScore = 0;

            // Check for regulatory keywords
            foreach (var keyword in _regulatoryKeywords)
            {
                string pattern = $@"\b{Regex.Escape(keyword)}\b";
                var matches = Regex.Matches(text, pattern, RegexOptions.IgnoreCase);
                if (matches.Count > 0)
                {
                    regulationScore += matches.Count * _keywordWeights[keyword];
                }
            }

            // Check for guidance indicators
            if (text.Contains("guidance", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("guide", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("recommendation", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("best practice", StringComparison.OrdinalIgnoreCase))
            {
                guidanceScore += 5;
            }

            // Check for news indicators
            if (text.Contains("announced", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("published", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("released", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("today", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("yesterday", StringComparison.OrdinalIgnoreCase))
            {
                newsScore += 5;
            }

            // Adjust scores based on sentiment
            if (classification.PositiveScore > classification.NegativeScore * 2)
            {
                regulationScore += 2;
            }

            // Adjust scores based on readability
            if (classification.ReadabilityScore > 10)
            {
                regulationScore += 3;
                guidanceScore += 2;
            }
            else
            {
                newsScore += 2;
                generalScore += 3;
            }

            // Determine the highest score
            var scores = new Dictionary<string, double>
            {
                { "Regulation", regulationScore },
                { "Guidance", guidanceScore },
                { "News", newsScore },
                { "General", generalScore }
            };

            return scores.OrderByDescending(s => s.Value).First().Key;
        }

        /// <summary>
        /// Calculates the confidence score for the classification
        /// </summary>
        private double CalculateConfidence(ContentClassification classification)
        {
            // Base confidence
            double confidence = 0.5;

            // Adjust based on content length
            if (classification.ContentLength > 1000)
                confidence += 0.1;

            // Adjust based on sentiment strength
            double sentimentStrength = Math.Abs(classification.PositiveScore - classification.NegativeScore) /
                                      (double)(classification.PositiveScore + classification.NegativeScore + 1);
            confidence += sentimentStrength * 0.2;

            // Adjust based on entity count
            if (classification.Entities.Count > 5)
                confidence += 0.1;

            // Cap confidence at 0.95
            return Math.Min(confidence, 0.95);
        }

        /// <summary>
        /// Determines the document type based on advanced features
        /// </summary>
        private string DetermineDocumentType(
            Models.TextFeatures textFeatures,
            Models.SentimentResult sentiment,
            IEnumerable<Models.RecognizedEntity> entities)
        {
            // Calculate scores for each document type
            double regulationScore = 0;
            double guidanceScore = 0;
            double newsScore = 0;
            double generalScore = 0;

            // Adjust scores based on text features
            if (textFeatures.ReadabilityScore > 60)
            {
                regulationScore += 3;
                guidanceScore += 2;
            }
            else if (textFeatures.ReadabilityScore > 40)
            {
                guidanceScore += 3;
                newsScore += 1;
            }
            else
            {
                newsScore += 3;
                generalScore += 2;
            }

            if (textFeatures.AverageSentenceLength > 20)
            {
                regulationScore += 2;
                guidanceScore += 1;
            }
            else if (textFeatures.AverageSentenceLength > 15)
            {
                guidanceScore += 2;
            }
            else
            {
                newsScore += 2;
                generalScore += 1;
            }

            // Adjust scores based on sentiment
            if (sentiment.OverallSentiment == "Neutral")
            {
                regulationScore += 2;
                guidanceScore += 1;
            }
            else if (sentiment.OverallSentiment == "Positive")
            {
                guidanceScore += 2;
                newsScore += 1;
            }
            else
            {
                newsScore += 2;
                generalScore += 1;
            }

            // Adjust scores based on entities
            int organizationCount = entities.Count(e => e.Type == "Organization");
            int dateCount = entities.Count(e => e.Type == "Date");
            int moneyCount = entities.Count(e => e.Type == "Money");
            int percentageCount = entities.Count(e => e.Type == "Percentage");

            if (organizationCount > 3)
            {
                regulationScore += 2;
                guidanceScore += 1;
            }

            if (dateCount > 2)
            {
                regulationScore += 1;
                guidanceScore += 1;
                newsScore += 2;
            }

            if (moneyCount > 2 || percentageCount > 2)
            {
                regulationScore += 2;
                guidanceScore += 1;
                newsScore += 1;
            }

            // Determine the highest score
            var scores = new Dictionary<string, double>
            {
                { "Regulation", regulationScore },
                { "Guidance", guidanceScore },
                { "News", newsScore },
                { "General", generalScore }
            };

            return scores.OrderByDescending(s => s.Value).First().Key;
        }

        /// <summary>
        /// Calculates confidence in the classification using advanced features
        /// </summary>
        private double CalculateConfidence(
            Models.TextFeatures textFeatures,
            Models.SentimentResult sentiment,
            IEnumerable<Models.RecognizedEntity> entities)
        {
            // Base confidence
            double confidence = 0.6;

            // Adjust based on text features
            if (textFeatures.Length > 1000)
            {
                confidence += 0.05;
            }

            if (textFeatures.SentenceCount > 20)
            {
                confidence += 0.05;
            }

            // Adjust based on sentiment
            if (sentiment.Confidence > 0.7)
            {
                confidence += 0.05;
            }

            // Adjust based on entities
            int entityCount = entities.Count();
            if (entityCount > 10)
            {
                confidence += 0.1;
            }
            else if (entityCount > 5)
            {
                confidence += 0.05;
            }

            // Cap confidence at 0.95
            return Math.Min(0.95, confidence);
        }
    }

    /// <summary>
    /// Result of content classification
    /// </summary>
    public class ContentClassification
    {
        /// <summary>
        /// Length of the content in characters
        /// </summary>
        public int ContentLength { get; set; }

        /// <summary>
        /// Number of sentences in the content
        /// </summary>
        public int SentenceCount { get; set; }

        /// <summary>
        /// Number of paragraphs in the content
        /// </summary>
        public int ParagraphCount { get; set; }

        /// <summary>
        /// Readability score (higher means more complex)
        /// </summary>
        public double ReadabilityScore { get; set; }

        /// <summary>
        /// Score for positive sentiment
        /// </summary>
        public int PositiveScore { get; set; }

        /// <summary>
        /// Score for negative sentiment
        /// </summary>
        public int NegativeScore { get; set; }

        /// <summary>
        /// Overall sentiment (Positive, Negative, Neutral)
        /// </summary>
        public string OverallSentiment { get; set; }

        /// <summary>
        /// Type of document
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Confidence in the classification (0-1)
        /// </summary>
        public double Confidence { get; set; }

        /// <summary>
        /// Entities extracted from the content
        /// </summary>
        public List<Entity> Entities { get; set; } = new List<Entity>();

        /// <summary>
        /// Error message if classification failed
        /// </summary>
        public string Error { get; set; }
    }

    /// <summary>
    /// Entity extracted from content
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Type of entity (Date, Money, Organization, etc.)
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Value of the entity
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Position in the text
        /// </summary>
        public int Position { get; set; }
    }
}
