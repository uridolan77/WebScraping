﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Newtonsoft.Json;
using System.Text;

namespace WebScraper.PatternLearning
{
    // Using record types for data-only classes
    public record UrlPattern
    {
        public string Pattern { get; init; }
        public int Occurrences { get; set; }
        public Dictionary<string, int> AssociatedTerms { get; init; } = new();
        public double Score { get; set; } = 0.0;
    }

    public record ContentPattern
    {
        public string Selector { get; init; }
        public int Occurrences { get; set; }
        public string ExampleContent { get; init; }
    }

    public class PatternLearner
    {
        private readonly List<UrlPattern> _urlPatterns = new();
        private readonly List<ContentPattern> _contentPatterns = new();
        private readonly HashSet<string> _commonWords = new() { "the", "and", "a", "to", "in", "of", "is", "that", "for", "on", "with", "as", "at", "by", "from", "up", "about", "into", "over", "after" };
        private readonly Action<string> _logger;
        private readonly string _dataDirectory;
        private readonly Dictionary<string, double> _patternScores = new();

        public PatternLearner(Action<string> logger = null, string dataDirectory = "ScrapedData")
        {
            _logger = logger ?? (_ => { });
            _dataDirectory = dataDirectory;

            // Ensure data directory exists
            Directory.CreateDirectory(_dataDirectory);
        }

        // Method referenced in AdaptiveCrawlingComponent.cs
        public void LearnFromUrl(string url, bool contentRelevant)
        {
            if (string.IsNullOrEmpty(url))
                return;

            try
            {
                _logger($"Learning from URL: {url}, relevant: {contentRelevant}");

                // Parse the URL to find patterns
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                // Extract path segments
                var segments = path.Trim('/').Split('/');

                // Learn from each segment
                foreach (var segment in segments)
                {
                    if (!string.IsNullOrEmpty(segment) && !IsCommonWord(segment) && segment.Length > 2)
                    {
                        LearnPattern(segment, contentRelevant ? 0.5 : -0.3);
                    }
                }

                // Extract and learn from query parameters
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var query = uri.Query.TrimStart('?');
                    var parameters = query.Split('&');

                    foreach (var param in parameters)
                    {
                        var parts = param.Split('=');
                        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                        {
                            // Learn from query parameter names
                            LearnPattern(parts[0], contentRelevant ? 0.3 : -0.2);

                            // If parameter has a value, also learn from it if it's not numeric or too short
                            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) &&
                                parts[1].Length > 3 && !Regex.IsMatch(parts[1], @"^\d+$"))
                            {
                                LearnPattern(parts[1], contentRelevant ? 0.2 : -0.1);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error learning from URL: {ex.Message}");
            }
        }

        // Method referenced in AdaptiveCrawlingComponent.cs
        public void LearnPattern(string pattern, double scoreAdjustment)
        {
            if (string.IsNullOrEmpty(pattern))
                return;

            try
            {
                // Normalize pattern
                pattern = pattern.ToLowerInvariant();

                // Update pattern score using null-coalescing assignment
                _patternScores.TryGetValue(pattern, out double currentScore);
                double newScore = currentScore + scoreAdjustment;

                // Cap the score to prevent extreme values (-5 to 5 range)
                newScore = Math.Max(-5.0, Math.Min(5.0, newScore));

                _patternScores[pattern] = newScore;

                // Update URL pattern if it exists using pattern matching
                if (_urlPatterns.FirstOrDefault(p => p.Pattern.Equals(pattern, StringComparison.OrdinalIgnoreCase)) is UrlPattern urlPattern)
                {
                    urlPattern.Score = newScore;
                }
            }
            catch (Exception ex)
            {
                _logger($"Error learning pattern '{pattern}': {ex.Message}");
            }
        }

        // Method referenced in AdaptiveCrawlingComponent.cs
        public double EvaluateUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return 0;

            try
            {
                double score = 0.0;
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                // Extract path segments and evaluate each non-empty segment
                score += path.Trim('/')
                    .Split('/')
                    .Where(segment => !string.IsNullOrEmpty(segment))
                    .Select(segment => segment.ToLowerInvariant())
                    .Sum(normalizedSegment =>
                        _patternScores.TryGetValue(normalizedSegment, out double segmentScore) ? segmentScore : 0);

                // Evaluate query parameters if present
                if (!string.IsNullOrEmpty(uri.Query))
                {
                    var query = uri.Query.TrimStart('?');

                    // Process each parameter
                    foreach (var param in query.Split('&'))
                    {
                        var parts = param.Split('=');

                        // Process parameter name
                        if (parts.Length > 0 && !string.IsNullOrEmpty(parts[0]))
                        {
                            string normalizedParam = parts[0].ToLowerInvariant();
                            if (_patternScores.TryGetValue(normalizedParam, out double paramScore))
                            {
                                score += paramScore * 0.5; // Query parameters get half weight
                            }

                            // Process parameter value
                            if (parts.Length > 1 && !string.IsNullOrEmpty(parts[1]))
                            {
                                string normalizedValue = parts[1].ToLowerInvariant();
                                if (_patternScores.TryGetValue(normalizedValue, out double valueScore))
                                {
                                    score += valueScore * 0.3; // Query values get even less weight
                                }
                            }
                        }
                    }
                }

                return score;
            }
            catch (Exception ex)
            {
                _logger($"Error evaluating URL '{url}': {ex.Message}");
                return 0;
            }
        }

        public async Task LearnFromUrlAsync(string url, string htmlContent)
        {
            try
            {
                _logger($"Learning patterns from {url}");

                // Parse the URL to find patterns
                var uri = new Uri(url);
                var path = uri.AbsolutePath;

                // Find URL pattern by extracting path segments and parameters
                var segments = path.Trim('/').Split('/');
                var basePattern = string.Join("/", segments.Take(Math.Min(2, segments.Length)));

                if (!string.IsNullOrEmpty(basePattern))
                {
                    var existingPattern = _urlPatterns.FirstOrDefault(p => p.Pattern == basePattern);
                    if (existingPattern != null)
                    {
                        existingPattern.Occurrences++;

                        // Add common terms from the URL
                        foreach (var segment in segments.Skip(Math.Min(2, segments.Length)))
                        {
                            if (!IsCommonWord(segment) && segment.Length > 3)
                            {
                                if (!existingPattern.AssociatedTerms.ContainsKey(segment))
                                {
                                    existingPattern.AssociatedTerms[segment] = 0;
                                }
                                existingPattern.AssociatedTerms[segment]++;
                            }
                        }
                    }
                    else
                    {
                        var newPattern = new UrlPattern
                        {
                            Pattern = basePattern,
                            Occurrences = 1
                        };
                        _urlPatterns.Add(newPattern);
                    }
                }

                // Parse HTML content
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(htmlContent);

                await AnalyzePageForPatternsAsync(htmlDoc);

                // Process and consolidate learned patterns
                if (_urlPatterns.Count > 10 || _contentPatterns.Count > 20)
                {
                    ProcessLearnedPatterns();
                }

                _logger($"Patterns learned: {_urlPatterns.Count} URL patterns, {_contentPatterns.Count} content patterns");
            }
            catch (Exception ex)
            {
                _logger($"Error learning patterns: {ex.Message}");
            }
        }

        private async Task AnalyzePageForPatternsAsync(HtmlDocument doc)
        {
            try
            {
                // Analyze common HTML structures

                // Headers
                var headers = doc.DocumentNode.SelectNodes("//h1 | //h2 | //h3");
                if (headers != null && headers.Count > 0)
                {
                    var headerPattern = new ContentPattern
                    {
                        Selector = "Headers (h1-h3)",
                        Occurrences = headers.Count,
                        ExampleContent = headers.First().InnerText.Trim()
                    };

                    var existing = _contentPatterns.FirstOrDefault(p => p.Selector == headerPattern.Selector);
                    if (existing != null)
                    {
                        existing.Occurrences += headerPattern.Occurrences;
                    }
                    else
                    {
                        _contentPatterns.Add(headerPattern);
                    }

                    _logger($"Found {headers.Count} headers");
                }

                // Main content areas
                var contentAreas = doc.DocumentNode.SelectNodes("//article | //main | //div[@class='content'] | //div[@id='content']");
                if (contentAreas != null && contentAreas.Count > 0)
                {
                    var contentPattern = new ContentPattern
                    {
                        Selector = "Main Content Areas",
                        Occurrences = contentAreas.Count,
                        ExampleContent = contentAreas.First().InnerText.Substring(0, Math.Min(100, contentAreas.First().InnerText.Length)).Trim()
                    };

                    var existing = _contentPatterns.FirstOrDefault(p => p.Selector == contentPattern.Selector);
                    if (existing != null)
                    {
                        existing.Occurrences += contentPattern.Occurrences;
                    }
                    else
                    {
                        _contentPatterns.Add(contentPattern);
                    }

                    _logger($"Found {contentAreas.Count} main content areas");
                }

                // Lists
                var lists = doc.DocumentNode.SelectNodes("//ul | //ol");
                if (lists != null && lists.Count > 0)
                {
                    var listPattern = new ContentPattern
                    {
                        Selector = "Lists (ul/ol)",
                        Occurrences = lists.Count,
                        ExampleContent = lists.First().InnerText.Substring(0, Math.Min(100, lists.First().InnerText.Length)).Trim()
                    };

                    var existing = _contentPatterns.FirstOrDefault(p => p.Selector == listPattern.Selector);
                    if (existing != null)
                    {
                        existing.Occurrences += listPattern.Occurrences;
                    }
                    else
                    {
                        _contentPatterns.Add(listPattern);
                    }

                    _logger($"Found {lists.Count} lists");
                }
            }
            catch (Exception ex)
            {
                _logger($"Error analyzing page patterns: {ex.Message}");
            }
        }

        // Using expression-bodied member for simple method
        private bool IsCommonWord(string text) =>
            _commonWords.Contains(text.ToLower()) || text.Length <= 2 || Regex.IsMatch(text, @"^\d+$");

        private void ProcessLearnedPatterns()
        {
            // Keep only the most frequent patterns
            _urlPatterns.Sort((a, b) => b.Occurrences.CompareTo(a.Occurrences));
            if (_urlPatterns.Count > 10)
            {
                _urlPatterns.RemoveRange(10, _urlPatterns.Count - 10);
            }

            _contentPatterns.Sort((a, b) => b.Occurrences.CompareTo(a.Occurrences));
            if (_contentPatterns.Count > 20)
            {
                _contentPatterns.RemoveRange(20, _contentPatterns.Count - 20);
            }

            _logger("Processed and pruned learned patterns");
        }

        // Using expression-bodied member for simple method
        public string ExtractTextContent(string htmlContent)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(htmlContent);
            return ExtractTextContent(htmlDoc);
        }

        public string ExtractTextContent(HtmlDocument doc)
        {
            var sb = new CustomStringBuilder(_logger);

            // Extract body content
            var bodyNode = doc.DocumentNode.SelectSingleNode("//body");
            if (bodyNode != null)
            {
                ExtractTextFromNode(bodyNode, sb);
            }
            else
            {
                // Fallback if body not found
                ExtractTextFromNode(doc.DocumentNode, sb);
            }

            return sb.ToString();
        }

        private void ExtractTextFromNode(HtmlNode node, CustomStringBuilder sb)
        {
            if (node == null)
                return;

            // Skip script, style, comment, etc.
            if (node.Name == "script" || node.Name == "style" || node.Name == "noscript" ||
                node.Name == "iframe" || node.Name == "#comment")
            {
                return;
            }

            // For text nodes, get the text
            if (node.NodeType == HtmlNodeType.Text)
            {
                string text = node.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.Append(text + " ");
                }
                return;
            }

            // Handle headers and paragraph elements specially
            if (node.Name == "h1" || node.Name == "h2" || node.Name == "h3" ||
                node.Name == "h4" || node.Name == "h5" || node.Name == "h6")
            {
                sb.AppendLine("\n" + node.InnerText.Trim() + "\n");
                return;
            }

            if (node.Name == "p" || node.Name == "div" || node.Name == "li")
            {
                string text = node.InnerText.Trim();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                }
                return;
            }

            // Handle line breaks
            if (node.Name == "br")
            {
                sb.AppendLine("");
                return;
            }

            // Recursively process child nodes
            foreach (var childNode in node.ChildNodes)
            {
                ExtractTextFromNode(childNode, sb);
            }
        }

        // Using expression-bodied members for simple getters
        public List<UrlPattern> GetUrlPatterns() => _urlPatterns;

        public List<ContentPattern> GetContentPatterns() => _contentPatterns;

        public async Task LoadLearnedPatternsAsync()
        {
            var patternsPath = Path.Combine(_dataDirectory, "learned_patterns.json");

            if (!File.Exists(patternsPath))
            {
                _logger("No previously learned patterns found");
                return;
            }

            try
            {
                var json = await File.ReadAllTextAsync(patternsPath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger("Pattern file exists but is empty");
                    return;
                }

                var data = JsonConvert.DeserializeObject<PatternData>(json);

                if (data is null)
                {
                    _logger("Failed to deserialize pattern data");
                    return;
                }

                // Use null-conditional operator with AddRange
                _urlPatterns.AddRange(data.UrlPatterns ?? Enumerable.Empty<UrlPattern>());
                _contentPatterns.AddRange(data.ContentPatterns ?? Enumerable.Empty<ContentPattern>());

                _logger($"Loaded {_urlPatterns.Count} URL patterns and {_contentPatterns.Count} content patterns");
            }
            catch (JsonException jsonEx)
            {
                _logger($"Error parsing pattern data: {jsonEx.Message}");
                // Consider backing up the corrupted file
                File.Copy(patternsPath, $"{patternsPath}.corrupted", true);
            }
            catch (IOException ioEx)
            {
                _logger($"Error reading pattern file: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                _logger($"Unexpected error loading patterns: {ex.Message}");
            }
        }

        public async Task SaveLearnedPatternsAsync()
        {
            if (_urlPatterns.Count == 0 && _contentPatterns.Count == 0)
            {
                _logger("No patterns to save");
                return;
            }

            var patternsPath = Path.Combine(_dataDirectory, "learned_patterns.json");
            var tempPath = $"{patternsPath}.tmp";

            try
            {
                // Create data object with current patterns
                var data = new PatternData
                {
                    UrlPatterns = _urlPatterns,
                    ContentPatterns = _contentPatterns
                };

                // Serialize to JSON with indentation for readability
                var json = JsonConvert.SerializeObject(data, Formatting.Indented);

                // Write to temporary file first to avoid corruption if process is interrupted
                await File.WriteAllTextAsync(tempPath, json);

                // If a previous file exists, create a backup
                if (File.Exists(patternsPath))
                {
                    File.Copy(patternsPath, $"{patternsPath}.bak", true);
                }

                // Move the temporary file to the final location
                File.Move(tempPath, patternsPath, true);

                _logger($"Successfully saved {_urlPatterns.Count} URL patterns and {_contentPatterns.Count} content patterns");
            }
            catch (IOException ioEx)
            {
                _logger($"I/O error saving patterns: {ioEx.Message}");

                // Try to clean up temporary file if it exists
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* Ignore cleanup errors */ }
                }
            }
            catch (JsonException jsonEx)
            {
                _logger($"Error serializing pattern data: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger($"Unexpected error saving patterns: {ex.Message}");
            }
        }

        // Helper class for serialization using record type
        private record PatternData
        {
            public List<UrlPattern> UrlPatterns { get; init; } = new();
            public List<ContentPattern> ContentPatterns { get; init; } = new();
        }
    }

    // Helper class to append and log text
    public class CustomStringBuilder
    {
        private readonly StringBuilder _builder = new();
        private readonly Action<string> _logger;

        public CustomStringBuilder(Action<string> logger = null)
        {
            _logger = logger ?? (_ => { });
        }

        // Using expression-bodied members for simple methods
        public void Append(string text) => _builder.Append(text);

        public void AppendLine(string text) => _builder.AppendLine(text);

        public override string ToString() => _builder.ToString();
    }
}
