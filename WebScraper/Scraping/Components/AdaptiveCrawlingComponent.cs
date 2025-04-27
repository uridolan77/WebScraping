using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WebScraper.AdaptiveCrawling;
using WebScraper.PatternLearning;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles adaptive crawling and URL prioritization
    /// </summary>
    public class AdaptiveCrawlingComponent : ScraperComponentBase
    {
        private AdaptiveCrawlStrategy _adaptiveCrawlStrategy;
        private PatternLearner _patternLearner;
        private Dictionary<string, double> _urlScores = new Dictionary<string, double>();
        private Dictionary<string, int> _urlHits = new Dictionary<string, int>();
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            try
            {
                // Initialize adaptive crawl strategy with default weights
                _adaptiveCrawlStrategy = new AdaptiveCrawlStrategy
                {
                    LastModifiedWeight = 0.5,
                    ContentRelevanceWeight = 0.8,
                    DepthWeight = -0.2,
                    ChangeFrequencyWeight = 0.3,
                    ImportanceLinkWeight = 0.6
                };
                
                // Initialize pattern learner
                _patternLearner = new PatternLearner();
                
                LogInfo("Adaptive crawling component initialized");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize adaptive crawling component");
            }
        }
        
        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public override Task OnScrapingStartedAsync()
        {
            // Reset URL scores at the start of each scraping session
            _urlScores.Clear();
            return base.OnScrapingStartedAsync();
        }
        
        /// <summary>
        /// Prioritizes URLs based on learned patterns and adaptive strategies
        /// </summary>
        /// <param name="urls">URLs to prioritize</param>
        /// <param name="maxUrls">Maximum number of URLs to return</param>
        /// <returns>Prioritized list of URLs</returns>
        public List<string> PrioritizeUrls(List<string> urls, int maxUrls = 10)
        {
            if (urls == null || !urls.Any())
                return new List<string>();
            
            try
            {
                // Calculate score for each URL
                var scoredUrls = new List<(string Url, double Score)>();
                
                foreach (var url in urls)
                {
                    double score = CalculateUrlScore(url);
                    scoredUrls.Add((url, score));
                    
                    // Update or set score in dictionary
                    _urlScores[url] = score;
                }
                
                // Sort by score descending and take top URLs
                var prioritizedUrls = scoredUrls
                    .OrderByDescending(u => u.Score)
                    .Take(maxUrls)
                    .Select(u => u.Url)
                    .ToList();
                
                return prioritizedUrls;
            }
            catch (Exception ex)
            {
                LogError(ex, "Error prioritizing URLs");
                return urls.Take(maxUrls).ToList();
            }
        }
        
        /// <summary>
        /// Processes a URL to update pattern learning
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="successful">Whether the processing was successful</param>
        /// <param name="contentRelevant">Whether the content was relevant</param>
        public void ProcessUrlResult(string url, bool successful, bool contentRelevant)
        {
            try
            {
                // Track URL hits
                if (!_urlHits.ContainsKey(url))
                {
                    _urlHits[url] = 0;
                }
                _urlHits[url]++;
                
                // Learn from this URL
                if (successful)
                {
                    _patternLearner.LearnFromUrl(url, contentRelevant);
                    
                    // Extract patterns from URL
                    ExtractPatternsFromUrl(url, contentRelevant);
                }
                
                // Update adaptive strategy based on results
                _adaptiveCrawlStrategy.TrackUrlProcessed(url, successful, contentRelevant);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error processing URL result: {url}");
            }
        }
        
        /// <summary>
        /// Calculates a priority score for a URL
        /// </summary>
        private double CalculateUrlScore(string url)
        {
            if (string.IsNullOrEmpty(url))
                return 0;
            
            try
            {
                // Base score
                double score = 1.0;
                
                // Adjust score based on URL patterns learned
                score += _patternLearner.EvaluateUrl(url);
                
                // Add score from adaptive strategy
                score += _adaptiveCrawlStrategy.CalculateUrlPriority(url);
                
                // Adjust for previously seen URLs
                if (_urlHits.TryGetValue(url, out int hits))
                {
                    // Penalize repeatedly visited URLs
                    score -= Math.Log(hits + 1) * 0.1;
                }
                
                // Adjust based on patterns in the URL
                score += EvaluateUrlPatterns(url);
                
                return Math.Max(0, score); // Ensure non-negative score
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error calculating URL score: {url}");
                return 0.5; // Default middle score on error
            }
        }
        
        /// <summary>
        /// Evaluates patterns in a URL to adjust its score
        /// </summary>
        private double EvaluateUrlPatterns(string url)
        {
            double patternScore = 0;
            
            // Prefer URLs with date patterns (often indicate fresh content)
            if (Regex.IsMatch(url, @"\d{4}/\d{1,2}/\d{1,2}"))
            {
                patternScore += 0.5;
            }
            
            // Prefer URLs with specific keywords
            string[] positiveKeywords = { "news", "update", "release", "announcement", "regulation" };
            foreach (var keyword in positiveKeywords)
            {
                if (url.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    patternScore += 0.3;
                }
            }
            
            // Penalize URLs with negative patterns
            string[] negativePatterns = { "login", "signup", "register", "comment", "print" };
            foreach (var pattern in negativePatterns)
            {
                if (url.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    patternScore -= 0.4;
                }
            }
            
            // Penalize very deep URLs
            int slashCount = url.Split('/').Length - 1;
            if (slashCount > 4)
            {
                patternScore -= (slashCount - 4) * 0.1;
            }
            
            return patternScore;
        }
        
        /// <summary>
        /// Extracts patterns from a URL and its relevance
        /// </summary>
        private void ExtractPatternsFromUrl(string url, bool contentRelevant)
        {
            try
            {
                // Extract path segments
                var uri = new Uri(url);
                var pathSegments = uri.AbsolutePath.Split('/')
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToList();
                
                // Learn from path segments
                foreach (var segment in pathSegments)
                {
                    _patternLearner.LearnPattern(segment, contentRelevant ? 1.0 : -0.5);
                }
                
                // Extract and learn from query parameters
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var queryParams = uri.Query.TrimStart('?').Split('&');
                    foreach (var param in queryParams)
                    {
                        var parts = param.Split('=');
                        if (parts.Length > 0)
                        {
                            _patternLearner.LearnPattern(parts[0], contentRelevant ? 0.5 : -0.5);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error extracting patterns from URL: {url}");
            }
        }
    }
}