using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraper.AdaptiveCrawling
{
    public class PageMetadata
    {
        public string Url { get; set; }
        public int ContentLength { get; set; }
        public int LinksCount { get; set; }
        public DateTime LastVisited { get; set; }
        public double ImportanceScore { get; set; }
        public List<string> Keywords { get; set; } = new List<string>();
    }

    public class AdaptiveCrawlStrategy
    {
        private readonly Dictionary<string, PageMetadata> _pageMetadata = new Dictionary<string, PageMetadata>();
        private readonly Dictionary<string, int> _domainVisitCount = new Dictionary<string, int>();
        private readonly List<string> _commonWords = new List<string> { "the", "and", "a", "to", "in", "of", "is", "you", "that", "it" };
        private readonly Action<string> _logger;
        private List<string> _priorityQueue = new List<string>();
        
        public AdaptiveCrawlStrategy(Action<string> logger = null)
        {
            _logger = logger ?? (_ => {}); // Default no-op logger if none provided
        }

        public void LoadPageMetadata(Dictionary<string, PageMetadata> metadata)
        {
            foreach (var entry in metadata)
            {
                _pageMetadata[entry.Key] = entry.Value;
            }
            _logger($"Loaded metadata for {metadata.Count} pages");
        }

        public void LoadMetadata(Dictionary<string, PageMetadata> metadata)
        {
            // Call the existing method for compatibility
            LoadPageMetadata(metadata);
        }

        public void InitializePriorityQueue(IEnumerable<string> startUrls)
        {
            _priorityQueue = new List<string>(startUrls);
            _logger($"Initialized priority queue with {_priorityQueue.Count} URLs");
        }

        public IEnumerable<string> PrioritizeUrls(List<string> urls, int maxUrls = 10)
        {
            if (urls == null || !urls.Any())
                return Enumerable.Empty<string>();

            _logger($"Prioritizing {urls.Count} URLs...");
            
            var scoredUrls = urls.Select(url =>
            {
                double score = CalculateUrlScore(url);
                return new { Url = url, Score = score };
            })
            .OrderByDescending(x => x.Score)
            .Take(maxUrls)
            .ToList();

            _logger($"Selected top {scoredUrls.Count} URLs based on priority scores");
            
            return scoredUrls.Select(x => x.Url);
        }

        public void UpdatePageMetadata(string url, HtmlDocument document, string textContent)
        {
            try
            {
                // Extract metadata
                var links = document.DocumentNode.SelectNodes("//a[@href]");
                var linksCount = links?.Count ?? 0;

                // Extract potential keywords
                var words = Regex.Split(textContent.ToLower(), @"\W+")
                    .Where(word => !string.IsNullOrEmpty(word) && word.Length > 3 && !_commonWords.Contains(word))
                    .GroupBy(word => word)
                    .OrderByDescending(g => g.Count())
                    .Take(10)
                    .Select(g => g.Key)
                    .ToList();

                // Create or update metadata
                var metadata = new PageMetadata
                {
                    Url = url,
                    ContentLength = textContent.Length,
                    LinksCount = linksCount,
                    LastVisited = DateTime.Now,
                    Keywords = words,
                    ImportanceScore = CalculateImportanceScore(textContent.Length, linksCount, words)
                };

                _pageMetadata[url] = metadata;

                // Update domain visit count
                var domain = new Uri(url).Host;
                if (!_domainVisitCount.ContainsKey(domain))
                {
                    _domainVisitCount[domain] = 0;
                }
                _domainVisitCount[domain]++;

                _logger($"Updated metadata for {url}: Content length={textContent.Length}, Links={linksCount}");
            }
            catch (Exception ex)
            {
                _logger($"Error updating page metadata: {ex.Message}");
            }
        }

        public Dictionary<string, PageMetadata> GetPageMetadata()
        {
            return _pageMetadata;
        }

        private double CalculateUrlScore(string url)
        {
            try
            {
                double score = 1.0; // Base score
                Uri uri = new Uri(url);
                string domain = uri.Host;

                // Prefer URLs we haven't visited yet
                if (!_pageMetadata.ContainsKey(url))
                {
                    score += 2.0;
                }

                // Domain diversity (slightly prefer domains we've visited less)
                if (_domainVisitCount.ContainsKey(domain))
                {
                    score -= Math.Min(0.5, _domainVisitCount[domain] * 0.1); // Cap the penalty
                }

                // Prefer shorter paths (often more important pages)
                var pathSegments = uri.Segments.Length;
                score -= (pathSegments - 1) * 0.2; // Segment count penalty

                // Prefer URLs with certain keywords
                string lowerUrl = url.ToLower();
                string[] preferredTerms = new[] { "about", "faq", "help", "guide", "news", "contact" };
                foreach (var term in preferredTerms)
                {
                    if (lowerUrl.Contains(term))
                    {
                        score += 0.5;
                    }
                }

                // Avoid certain file types
                string[] avoidExtensions = new[] { ".pdf", ".jpg", ".png", ".gif", ".mp3", ".mp4", ".zip" };
                foreach (var ext in avoidExtensions)
                {
                    if (lowerUrl.EndsWith(ext))
                    {
                        score -= 5.0; // Strong penalty
                    }
                }

                return score;
            }
            catch
            {
                return 0.0;
            }
        }

        private double CalculateImportanceScore(int contentLength, int linksCount, List<string> keywords)
        {
            double score = 0.0;

            // Content length contributes to score (larger content may be more important, to a point)
            score += Math.Min(1.0, contentLength / 5000.0);

            // Pages with more links might be hub pages (good to certain point)
            score += Math.Min(1.0, linksCount / 30.0);

            // Keyword richness may indicate content value
            score += Math.Min(1.0, keywords.Count / 5.0);

            return score;
        }
    }
}