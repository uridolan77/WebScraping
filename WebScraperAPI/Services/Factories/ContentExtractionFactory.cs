#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating content extraction components
    /// </summary>
    public class ContentExtractionFactory
    {
        private readonly ILogger<ContentExtractionFactory> _logger;

        public ContentExtractionFactory(ILogger<ContentExtractionFactory> logger)
        {
            _logger = logger;
        }

        public ICrawlStrategy CreateCrawlStrategy(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Creating default crawl strategy");
                // Return a simple implementation for now
                return new DefaultCrawlStrategy(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating crawl strategy");
                logAction($"Error creating crawl strategy: {ex.Message}");
                // Return a fallback implementation
                return new DefaultCrawlStrategy(config);
            }
        }

        public IContentExtractor? CreateContentExtractor(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                logAction("Creating default content extractor");
                // Return a simple implementation for now
                return new DefaultContentExtractor(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content extractor");
                logAction($"Error creating content extractor: {ex.Message}");
                // Use non-null return when possible
                return new DefaultContentExtractor(new ScraperConfig());
            }
        }

        public IDynamicContentRenderer? CreateDynamicContentRenderer(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                // Dynamic content rendering is optional and null is a valid return value
                // because the return type is explicitly marked as nullable with the ? symbol
                logAction("Dynamic content rendering is not implemented in this version");
                IDynamicContentRenderer? renderer = null;
                return renderer; // This should not trigger a warning
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing dynamic content renderer");
                IDynamicContentRenderer? renderer = null;
                return renderer; // This should not trigger a warning
            }
        }

        // Internal implementation classes for when we don't need complex functionality
        private class DefaultCrawlStrategy : ICrawlStrategy
        {
            private readonly ScraperConfig _config;
            private readonly List<Uri> _urls = new List<Uri>();
            private int _currentIndex = 0;
            private readonly Dictionary<string, object> _pageMetadata = new Dictionary<string, object>();

            public DefaultCrawlStrategy(ScraperConfig config)
            {
                _config = config;
                // Add the start URL to our list
                if (!string.IsNullOrEmpty(config.StartUrl))
                {
                    _urls.Add(new Uri(config.StartUrl));
                }
            }

            public Uri GetNextUrl()
            {
                if (_currentIndex < _urls.Count)
                {
                    return _urls[_currentIndex++];
                }
                return null;
            }

            public void AddUrl(Uri url)
            {
                if (!_urls.Contains(url))
                {
                    _urls.Add(url);
                }
            }

            public bool HasMoreUrls()
            {
                return _currentIndex < _urls.Count;
            }

            // Implement additional ICrawlStrategy methods
            public void LoadMetadata(Dictionary<string, object> metadata)
            {
                if (metadata != null)
                {
                    foreach (var kvp in metadata)
                    {
                        _pageMetadata[kvp.Key] = kvp.Value;
                    }
                }
            }

            // Fix the return type to match the interface
            public IEnumerable<string> PrioritizeUrls(List<string> urls, int maxCount)
            {
                // Simple implementation - just return the first maxCount URLs
                if (urls.Count <= maxCount)
                {
                    return urls;
                }
                return urls.Take(maxCount);
            }

            public void UpdatePageMetadata(string url, HtmlDocument document, string content)
            {
                // Simple implementation - store basic info about the page
                _pageMetadata[url] = new
                {
                    Processed = DateTime.Now,
                    HasContent = !string.IsNullOrEmpty(content),
                    NodeCount = document?.DocumentNode?.ChildNodes?.Count ?? 0
                };
            }

            public Dictionary<string, object> GetPageMetadata()
            {
                // Return a copy of the metadata
                return new Dictionary<string, object>(_pageMetadata);
            }

            public bool ShouldCrawl(string url)
            {
                // Simple implementation - check if URL is in the same domain as the start URL
                if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(_config.StartUrl))
                {
                    return false;
                }

                try
                {
                    var uri = new Uri(url);
                    var startUri = new Uri(_config.StartUrl);
                    return uri.Host.Equals(startUri.Host, StringComparison.OrdinalIgnoreCase);
                }
                catch
                {
                    return false;
                }
            }
        }

        private class DefaultContentExtractor : IContentExtractor
        {
            private readonly ScraperConfig _config;

            public DefaultContentExtractor(ScraperConfig config)
            {
                _config = config;
            }

            public string ExtractContent(string html, Uri url)
            {
                // Simple implementation just returns the full HTML
                return html; 
            }

            // Implement additional IContentExtractor methods
            public string ExtractTextContent(HtmlDocument document)
            {
                if (document == null || document.DocumentNode == null)
                {
                    return string.Empty;
                }

                // Simple implementation - extract text from document
                return document.DocumentNode.InnerText;
            }

            // Implement with correct return type from the actual interface
            public List<WebScraper.ContentNode> ExtractStructuredContent(HtmlDocument document)
            {
                var result = new List<WebScraper.ContentNode>();
                
                if (document == null || document.DocumentNode == null)
                {
                    return result;
                }

                // Simple implementation - extract title and meta description
                var titleNode = document.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null)
                {
                    result.Add(new WebScraper.ContentNode 
                    { 
                        NodeType = "title", 
                        Content = titleNode.InnerText,
                        Children = new List<WebScraper.ContentNode>()
                    });
                }

                var metaDescription = document.DocumentNode.SelectSingleNode("//meta[@name='description']");
                if (metaDescription != null)
                {
                    var content = metaDescription.GetAttributeValue("content", "");
                    if (!string.IsNullOrEmpty(content))
                    {
                        result.Add(new WebScraper.ContentNode 
                        { 
                            NodeType = "meta", 
                            Content = content,
                            Children = new List<WebScraper.ContentNode>()
                        });
                    }
                }

                return result;
            }
        }
    }

    // Define the ContentNode class with required properties
    public class ContentNode
    {
        public ContentNode()
        {
            // Initialize non-nullable properties with default values
            NodeType = "unknown";
            Content = string.Empty;
            Children = new List<ContentNode>();
        }
        
        public string NodeType { get; set; }
        public string Content { get; set; }
        public List<ContentNode> Children { get; set; }
    }
}