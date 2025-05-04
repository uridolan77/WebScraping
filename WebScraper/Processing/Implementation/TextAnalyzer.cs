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
    /// Analyzes text and extracts features
    /// </summary>
    public class TextAnalyzer : ITextAnalyzer
    {
        private readonly ILogger<TextAnalyzer> _logger;
        
        /// <summary>
        /// Creates a new instance of the TextAnalyzer
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public TextAnalyzer(ILogger<TextAnalyzer> logger)
        {
            _logger = logger;
        }
        
        /// <summary>
        /// Analyzes text and extracts features
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Text features</returns>
        public Task<TextFeatures> AnalyzeAsync(string text)
        {
            _logger.LogInformation("Analyzing text features");
            
            try
            {
                var features = new TextFeatures
                {
                    Length = text.Length,
                    SentenceCount = CountSentences(text),
                    ParagraphCount = CountParagraphs(text)
                };
                
                // Calculate word-based metrics
                var words = ExtractWords(text);
                features.UniqueWordCount = words.Distinct(StringComparer.OrdinalIgnoreCase).Count();
                
                // Calculate average word length
                if (words.Count > 0)
                {
                    features.AverageWordLength = words.Average(w => w.Length);
                }
                
                // Calculate average sentence length
                var sentences = ExtractSentences(text);
                if (sentences.Count > 0)
                {
                    features.AverageSentenceLength = sentences.Average(s => ExtractWords(s).Count);
                }
                
                // Calculate readability score (Flesch-Kincaid)
                features.ReadabilityScore = CalculateReadabilityScore(text, features);
                
                return Task.FromResult(features);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing text features");
                throw;
            }
        }
        
        /// <summary>
        /// Counts sentences in text
        /// </summary>
        private int CountSentences(string text)
        {
            // Simple sentence counting based on punctuation
            var count = Regex.Matches(text, @"[.!?]+").Count;
            return count > 0 ? count : 1; // Ensure at least one sentence
        }
        
        /// <summary>
        /// Counts paragraphs in text
        /// </summary>
        private int CountParagraphs(string text)
        {
            // Count double line breaks as paragraph separators
            var count = Regex.Matches(text, @"\n\s*\n").Count + 1;
            return count > 0 ? count : 1; // Ensure at least one paragraph
        }
        
        /// <summary>
        /// Extracts words from text
        /// </summary>
        private List<string> ExtractWords(string text)
        {
            // Extract words using regex
            var matches = Regex.Matches(text, @"\b[a-zA-Z0-9_']+\b");
            return matches.Select(m => m.Value).ToList();
        }
        
        /// <summary>
        /// Extracts sentences from text
        /// </summary>
        private List<string> ExtractSentences(string text)
        {
            // Split text into sentences
            var sentences = Regex.Split(text, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            
            return sentences;
        }
        
        /// <summary>
        /// Calculates readability score
        /// </summary>
        private double CalculateReadabilityScore(string text, TextFeatures features)
        {
            // Implement Flesch-Kincaid readability score
            // Score = 206.835 - 1.015 * (words / sentences) - 84.6 * (syllables / words)
            
            var words = ExtractWords(text);
            var wordCount = words.Count;
            var sentenceCount = features.SentenceCount;
            
            if (wordCount == 0 || sentenceCount == 0)
            {
                return 0;
            }
            
            // Estimate syllable count (simplified)
            var syllableCount = EstimateSyllableCount(words);
            
            // Calculate score
            var score = 206.835 - 1.015 * (wordCount / (double)sentenceCount) - 84.6 * (syllableCount / (double)wordCount);
            
            // Clamp score to reasonable range (0-100)
            return Math.Max(0, Math.Min(100, score));
        }
        
        /// <summary>
        /// Estimates syllable count in words
        /// </summary>
        private int EstimateSyllableCount(List<string> words)
        {
            int syllableCount = 0;
            
            foreach (var word in words)
            {
                // Simple syllable counting heuristic
                var count = CountSyllables(word);
                syllableCount += count;
            }
            
            return syllableCount;
        }
        
        /// <summary>
        /// Counts syllables in a word
        /// </summary>
        private int CountSyllables(string word)
        {
            // Simple syllable counting heuristic
            word = word.ToLower().Trim();
            
            // Count vowel groups
            int count = Regex.Matches(word, @"[aeiouy]+").Count;
            
            // Adjust for common patterns
            if (word.EndsWith("e") && !word.EndsWith("le"))
            {
                count--;
            }
            
            // Ensure at least one syllable
            return Math.Max(1, count);
        }
    }
}
