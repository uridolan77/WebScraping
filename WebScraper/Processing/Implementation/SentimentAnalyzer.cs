using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Processing.Interfaces;
using WebScraper.Processing.Models;

namespace WebScraper.Processing.Implementation
{
    /// <summary>
    /// Analyzes sentiment in text
    /// </summary>
    public class SentimentAnalyzer : ISentimentAnalyzer
    {
        private readonly ILogger<SentimentAnalyzer> _logger;
        private readonly Dictionary<string, double> _sentimentLexicon;
        private readonly Dictionary<string, string> _emotionalTones;
        
        /// <summary>
        /// Creates a new instance of the SentimentAnalyzer
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public SentimentAnalyzer(ILogger<SentimentAnalyzer> logger)
        {
            _logger = logger;
            
            // Initialize sentiment lexicon
            _sentimentLexicon = InitializeSentimentLexicon();
            
            // Initialize emotional tones
            _emotionalTones = InitializeEmotionalTones();
        }
        
        /// <summary>
        /// Analyzes sentiment in text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Sentiment analysis results</returns>
        public Task<SentimentResult> AnalyzeSentimentAsync(string text)
        {
            _logger.LogInformation("Analyzing sentiment");
            
            try
            {
                var result = new SentimentResult();
                
                // Extract words
                var words = ExtractWords(text);
                
                // Calculate sentiment scores
                CalculateSentimentScores(words, result);
                
                // Determine emotional tone
                result.EmotionalTone = DetermineEmotionalTone(words);
                
                // Calculate confidence
                result.Confidence = CalculateConfidence(words);
                
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment");
                throw;
            }
        }
        
        /// <summary>
        /// Extracts words from text
        /// </summary>
        private List<string> ExtractWords(string text)
        {
            // Extract words using regex
            var matches = Regex.Matches(text, @"\b[a-zA-Z0-9_']+\b");
            return matches.Select(m => m.Value.ToLower()).ToList();
        }
        
        /// <summary>
        /// Calculates sentiment scores
        /// </summary>
        private void CalculateSentimentScores(List<string> words, SentimentResult result)
        {
            double positiveScore = 0;
            double negativeScore = 0;
            int matchedWords = 0;
            
            foreach (var word in words)
            {
                if (_sentimentLexicon.TryGetValue(word, out double score))
                {
                    if (score > 0)
                    {
                        positiveScore += score;
                    }
                    else
                    {
                        negativeScore += Math.Abs(score);
                    }
                    
                    matchedWords++;
                }
            }
            
            // Normalize scores to 0-100 range
            if (matchedWords > 0)
            {
                double totalScore = positiveScore + negativeScore;
                if (totalScore > 0)
                {
                    result.PositiveScore = (int)Math.Round((positiveScore / totalScore) * 100);
                    result.NegativeScore = (int)Math.Round((negativeScore / totalScore) * 100);
                }
            }
            
            // Determine overall sentiment
            if (result.PositiveScore > result.NegativeScore * 2)
            {
                result.OverallSentiment = "Positive";
            }
            else if (result.NegativeScore > result.PositiveScore * 2)
            {
                result.OverallSentiment = "Negative";
            }
            else
            {
                result.OverallSentiment = "Neutral";
            }
        }
        
        /// <summary>
        /// Determines emotional tone
        /// </summary>
        private string DetermineEmotionalTone(List<string> words)
        {
            var toneScores = new Dictionary<string, int>();
            
            foreach (var word in words)
            {
                if (_emotionalTones.TryGetValue(word, out string tone))
                {
                    if (!toneScores.ContainsKey(tone))
                    {
                        toneScores[tone] = 0;
                    }
                    
                    toneScores[tone]++;
                }
            }
            
            if (toneScores.Count > 0)
            {
                return toneScores.OrderByDescending(t => t.Value).First().Key;
            }
            
            return "Neutral";
        }
        
        /// <summary>
        /// Calculates confidence in sentiment analysis
        /// </summary>
        private double CalculateConfidence(List<string> words)
        {
            // Base confidence
            double confidence = 0.5;
            
            // Adjust based on number of matched words
            int matchedWords = words.Count(w => _sentimentLexicon.ContainsKey(w));
            double matchRatio = words.Count > 0 ? matchedWords / (double)words.Count : 0;
            
            confidence += matchRatio * 0.3;
            
            // Cap confidence at 0.95
            return Math.Min(0.95, confidence);
        }
        
        /// <summary>
        /// Initializes sentiment lexicon
        /// </summary>
        private Dictionary<string, double> InitializeSentimentLexicon()
        {
            var lexicon = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            
            // Positive regulatory terms
            lexicon["compliance"] = 0.7;
            lexicon["approved"] = 0.8;
            lexicon["authorized"] = 0.7;
            lexicon["permitted"] = 0.6;
            lexicon["legal"] = 0.5;
            lexicon["valid"] = 0.6;
            lexicon["certified"] = 0.7;
            lexicon["registered"] = 0.6;
            lexicon["licensed"] = 0.7;
            lexicon["compliant"] = 0.8;
            lexicon["transparent"] = 0.6;
            lexicon["ethical"] = 0.7;
            lexicon["responsible"] = 0.6;
            lexicon["fair"] = 0.5;
            lexicon["secure"] = 0.6;
            lexicon["safe"] = 0.7;
            lexicon["protected"] = 0.6;
            lexicon["benefit"] = 0.5;
            lexicon["advantage"] = 0.5;
            lexicon["improve"] = 0.6;
            lexicon["enhance"] = 0.6;
            
            // Negative regulatory terms
            lexicon["violation"] = -0.8;
            lexicon["breach"] = -0.7;
            lexicon["infringement"] = -0.7;
            lexicon["non-compliance"] = -0.8;
            lexicon["illegal"] = -0.8;
            lexicon["prohibited"] = -0.7;
            lexicon["restricted"] = -0.5;
            lexicon["banned"] = -0.8;
            lexicon["revoked"] = -0.7;
            lexicon["suspended"] = -0.6;
            lexicon["penalty"] = -0.6;
            lexicon["fine"] = -0.6;
            lexicon["sanction"] = -0.7;
            lexicon["warning"] = -0.5;
            lexicon["risk"] = -0.5;
            lexicon["hazard"] = -0.6;
            lexicon["danger"] = -0.7;
            lexicon["threat"] = -0.6;
            lexicon["problem"] = -0.5;
            lexicon["issue"] = -0.4;
            lexicon["concern"] = -0.4;
            
            // Neutral regulatory terms (slightly positive or negative)
            lexicon["regulation"] = 0.2;
            lexicon["requirement"] = 0.1;
            lexicon["standard"] = 0.2;
            lexicon["guideline"] = 0.2;
            lexicon["policy"] = 0.1;
            lexicon["procedure"] = 0.1;
            lexicon["rule"] = 0.0;
            lexicon["law"] = 0.0;
            lexicon["legislation"] = 0.0;
            lexicon["directive"] = 0.0;
            lexicon["statute"] = 0.0;
            lexicon["code"] = 0.1;
            lexicon["framework"] = 0.2;
            lexicon["oversight"] = -0.1;
            lexicon["monitoring"] = 0.1;
            lexicon["inspection"] = -0.2;
            lexicon["audit"] = -0.1;
            lexicon["review"] = 0.0;
            lexicon["assessment"] = 0.0;
            lexicon["evaluation"] = 0.0;
            
            return lexicon;
        }
        
        /// <summary>
        /// Initializes emotional tones
        /// </summary>
        private Dictionary<string, string> InitializeEmotionalTones()
        {
            var tones = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            // Authoritative tone
            tones["must"] = "Authoritative";
            tones["shall"] = "Authoritative";
            tones["required"] = "Authoritative";
            tones["mandatory"] = "Authoritative";
            tones["obligation"] = "Authoritative";
            tones["enforce"] = "Authoritative";
            tones["comply"] = "Authoritative";
            tones["adhere"] = "Authoritative";
            tones["ensure"] = "Authoritative";
            tones["prohibit"] = "Authoritative";
            
            // Informative tone
            tones["inform"] = "Informative";
            tones["advise"] = "Informative";
            tones["guide"] = "Informative";
            tones["explain"] = "Informative";
            tones["describe"] = "Informative";
            tones["detail"] = "Informative";
            tones["outline"] = "Informative";
            tones["clarify"] = "Informative";
            tones["note"] = "Informative";
            tones["reference"] = "Informative";
            
            // Cautionary tone
            tones["caution"] = "Cautionary";
            tones["warning"] = "Cautionary";
            tones["alert"] = "Cautionary";
            tones["attention"] = "Cautionary";
            tones["careful"] = "Cautionary";
            tones["risk"] = "Cautionary";
            tones["danger"] = "Cautionary";
            tones["hazard"] = "Cautionary";
            tones["avoid"] = "Cautionary";
            tones["prevent"] = "Cautionary";
            
            // Suggestive tone
            tones["recommend"] = "Suggestive";
            tones["suggest"] = "Suggestive";
            tones["consider"] = "Suggestive";
            tones["may"] = "Suggestive";
            tones["might"] = "Suggestive";
            tones["could"] = "Suggestive";
            tones["should"] = "Suggestive";
            tones["encourage"] = "Suggestive";
            tones["propose"] = "Suggestive";
            tones["advise"] = "Suggestive";
            
            return tones;
        }
    }
}
