using System;
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
    public class UrlPattern
    {
        public string Pattern { get; set; }
        public int Occurrences { get; set; }
        public Dictionary<string, int> AssociatedTerms { get; set; } = new Dictionary<string, int>();
    }

    public class ContentPattern
    {
        public string Selector { get; set; }
        public int Occurrences { get; set; }
        public string ExampleContent { get; set; }
    }

    public class PatternLearner
    {
        private readonly List<UrlPattern> _urlPatterns = new List<UrlPattern>();
        private readonly List<ContentPattern> _contentPatterns = new List<ContentPattern>();
        private readonly HashSet<string> _commonWords = new HashSet<string>() { "the", "and", "a", "to", "in", "of", "is", "that", "for", "on", "with", "as", "at", "by", "from", "up", "about", "into", "over", "after" };
        private readonly Action<string> _logger;
        private readonly string _dataDirectory;

        public PatternLearner(Action<string> logger = null, string dataDirectory = "ScrapedData")
        {
            _logger = logger ?? (_ => {});
            _dataDirectory = dataDirectory;
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
                
                // Let other tasks run
                await Task.Delay(1);
            }
            catch (Exception ex)
            {
                _logger($"Error analyzing page patterns: {ex.Message}");
            }
        }

        private bool IsCommonWord(string text)
        {
            return _commonWords.Contains(text.ToLower()) || text.Length <= 2 || Regex.IsMatch(text, @"^\d+$");
        }

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

        public List<UrlPattern> GetUrlPatterns()
        {
            return _urlPatterns;
        }
        
        public List<ContentPattern> GetContentPatterns()
        {
            return _contentPatterns;
        }

        public async Task LoadLearnedPatternsAsync()
        {
            try
            {
                var patternsPath = Path.Combine(_dataDirectory, "learned_patterns.json");
                
                if (File.Exists(patternsPath))
                {
                    var json = await File.ReadAllTextAsync(patternsPath);
                    var data = JsonConvert.DeserializeObject<PatternData>(json);

                    if (data != null)
                    {
                        if (data.UrlPatterns != null) _urlPatterns.AddRange(data.UrlPatterns);
                        if (data.ContentPatterns != null) _contentPatterns.AddRange(data.ContentPatterns);
                    }
                    
                    _logger($"Loaded {_urlPatterns.Count} URL patterns and {_contentPatterns.Count} content patterns");
                }
                else
                {
                    _logger("No previously learned patterns found");
                }
            }
            catch (Exception ex)
            {
                _logger($"Error loading learned patterns: {ex.Message}");
            }
        }

        public async Task SaveLearnedPatternsAsync()
        {
            try
            {
                var patternsPath = Path.Combine(_dataDirectory, "learned_patterns.json");
                
                var data = new PatternData
                {
                    UrlPatterns = _urlPatterns,
                    ContentPatterns = _contentPatterns
                };
                
                await File.WriteAllTextAsync(
                    patternsPath,
                    JsonConvert.SerializeObject(data, Formatting.Indented)
                );
                
                _logger($"Saved {_urlPatterns.Count} URL patterns and {_contentPatterns.Count} content patterns");
            }
            catch (Exception ex)
            {
                _logger($"Error saving learned patterns: {ex.Message}");
            }
        }

        // Helper class for serialization
        private class PatternData
        {
            public List<UrlPattern> UrlPatterns { get; set; }
            public List<ContentPattern> ContentPatterns { get; set; }
        }
    }
    
    // Helper class to append and log text
    public class CustomStringBuilder
    {
        private readonly StringBuilder _builder = new StringBuilder();
        private readonly Action<string> _logger;

        public CustomStringBuilder(Action<string> logger = null)
        {
            _logger = logger ?? (_ => {});
        }

        public void Append(string text)
        {
            _builder.Append(text);
        }

        public void AppendLine(string text)
        {
            _builder.AppendLine(text);
        }

        public override string ToString()
        {
            return _builder.ToString();
        }
    }
}
