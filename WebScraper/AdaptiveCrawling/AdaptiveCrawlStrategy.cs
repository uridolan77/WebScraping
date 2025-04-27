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
        
        // Properties referenced in AdaptiveCrawlingComponent.cs
        public double LastModifiedWeight { get; set; } = 0.5;
        public double ContentRelevanceWeight { get; set; } = 0.8;
        public double DepthWeight { get; set; } = -0.2;
        public double ChangeFrequencyWeight { get; set; } = 0.3;
        public double ImportanceLinkWeight { get; set; } = 0.6;

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
        
        public bool ShouldCrawl(string url)
        {
            // Basic implementation to avoid revisiting URLs too frequently
            if (_pageMetadata.ContainsKey(url))
            {
                var lastVisit = _pageMetadata[url].LastVisited;
                var timeSinceLastVisit = DateTime.Now - lastVisit;
                
                // Don't crawl if visited in the last 24 hours
                if (timeSinceLastVisit.TotalHours < 24)
                {
                    _logger($"Skipping {url} - visited {timeSinceLastVisit.TotalHours:F1} hours ago");
                    return false;
                }
            }
            
            return true;
        }
        
        // Method referenced in AdaptiveCrawlingComponent.cs
        public double CalculateUrlPriority(string url)
        {
            if (string.IsNullOrEmpty(url))
                return 0;
                
            try
            {
                double priority = 0.0;
                Uri uri = new Uri(url);
                
                // Check if we have metadata for this URL
                if (_pageMetadata.ContainsKey(url))
                {
                    var metadata = _pageMetadata[url];
                    
                    // Prioritize by last modified time (newer = higher priority)
                    var hoursSinceLastVisit = (DateTime.Now - metadata.LastVisited).TotalHours;
                    priority += LastModifiedWeight * (24.0 / (hoursSinceLastVisit + 1));
                    
                    // Prioritize by content relevance (approximated by importance score)
                    priority += ContentRelevanceWeight * metadata.ImportanceScore;
                    
                    // Apply depth penalty for deep URLs
                    int depth = url.Count(c => c == '/') - 2; // Rough depth estimation
                    if (depth > 0)
                    {
                        priority += DepthWeight * depth;
                    }
                }
                else
                {
                    // For new URLs, give a moderate priority
                    priority += 0.5;
                }
                
                return priority;
            }
            catch (Exception)
            {
                return 0.0;
            }
        }
        
        // Method referenced in AdaptiveCrawlingComponent.cs  
        public void TrackUrlProcessed(string url, bool successful, bool contentRelevant)
        {
            try
            {
                if (successful && !_pageMetadata.ContainsKey(url))
                {
                    // Initialize metadata if we don't have it yet
                    _pageMetadata[url] = new PageMetadata
                    {
                        Url = url,
                        LastVisited = DateTime.Now
                    };
                }
                
                if (_pageMetadata.ContainsKey(url))
                {
                    if (contentRelevant)
                    {
                        // Boost importance score for relevant content
                        _pageMetadata[url].ImportanceScore += 0.5;
                    }
                    else
                    {
                        // Reduce importance score for irrelevant content
                        _pageMetadata[url].ImportanceScore -= 0.2;
                        _pageMetadata[url].ImportanceScore = Math.Max(0, _pageMetadata[url].ImportanceScore);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error tracking URL processing: {ex.Message}");
            }
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