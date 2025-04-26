using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Extracts structured content from regulatory websites, preserving the hierarchical relationship between content sections
    /// </summary>
    public class StructuredContentExtractor
    {
        private readonly Action<string> _logger;
        private readonly Dictionary<string, string> _sectionSelectors;

        public StructuredContentExtractor(Action<string> logger = null)
        {
            _logger = logger ?? (_ => { });
            
            // Default selectors for common regulatory content structures
            _sectionSelectors = new Dictionary<string, string>
            {
                { "guidance", "//div[contains(@class, 'guidance') or contains(@class, 'guide')]" },
                { "regulatory-update", "//div[contains(@class, 'update') or contains(@class, 'regulatory')]" },
                { "news", "//div[contains(@class, 'news')]" },
                { "consultation", "//div[contains(@class, 'consultation')]" },
                { "enforcement", "//div[contains(@class, 'enforcement')]" }
            };
        }

        /// <summary>
        /// Represents a structured content section with hierarchical relationships
        /// </summary>
        public class ContentSection
        {
            public string SectionType { get; set; } // e.g., "guidance", "regulatory-update", "news"
            public string Title { get; set; }
            public string Content { get; set; }
            public DateTime? PublishedDate { get; set; }
            public string Category { get; set; } // e.g., "AML", "Compliance", "Licensing"
            public List<ContentSection> SubSections { get; set; } = new List<ContentSection>();
            public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
        }

        /// <summary>
        /// Extracts structured content from a regulatory website page
        /// </summary>
        /// <param name="document">The HtmlDocument to extract content from</param>
        /// <param name="baseUrl">The base URL for resolving relative links</param>
        /// <returns>A hierarchical ContentSection structure</returns>
        public ContentSection ExtractStructuredContent(HtmlDocument document, string baseUrl)
        {
            try
            {
                // Root section representing the whole document
                var rootSection = new ContentSection
                {
                    SectionType = "page",
                    Title = ExtractTitle(document),
                    PublishedDate = ExtractPublishedDate(document),
                    Category = ExtractCategory(document)
                };

                // Extract main content and clean it
                var mainContent = ExtractMainContent(document);
                rootSection.Content = mainContent;

                // Extract metadata
                rootSection.Metadata = ExtractMetadata(document);

                // Extract hierarchical sections
                foreach (var sectionType in _sectionSelectors.Keys)
                {
                    var sections = ExtractSections(document, sectionType);
                    rootSection.SubSections.AddRange(sections);
                }

                // Try to detect subsections by headers if no sections were found by class
                if (rootSection.SubSections.Count == 0)
                {
                    rootSection.SubSections.AddRange(ExtractSectionsByHeaders(document));
                }

                _logger?.Invoke($"Extracted structured content with {rootSection.SubSections.Count} main sections");

                return rootSection;
            }
            catch (Exception ex)
            {
                _logger?.Invoke($"Error extracting structured content: {ex.Message}");
                return new ContentSection
                {
                    SectionType = "error",
                    Title = "Content extraction failed",
                    Content = ex.Message
                };
            }
        }

        /// <summary>
        /// Add custom section selectors for specific websites
        /// </summary>
        public void AddSectionSelector(string sectionType, string cssSelector)
        {
            _sectionSelectors[sectionType] = cssSelector;
            _logger?.Invoke($"Added custom selector for {sectionType}: {cssSelector}");
        }

        private string ExtractTitle(HtmlDocument document)
        {
            var titleNode = document.DocumentNode.SelectSingleNode("//h1");
            if (titleNode != null)
            {
                return titleNode.InnerText.Trim();
            }

            titleNode = document.DocumentNode.SelectSingleNode("//title");
            return titleNode?.InnerText.Trim() ?? "Untitled Document";
        }

        private DateTime? ExtractPublishedDate(HtmlDocument document)
        {
            // Try several common date formats and locations
            var datePatterns = new[]
            {
                "//div[contains(@class, 'date')]",
                "//span[contains(@class, 'date')]",
                "//p[contains(@class, 'date')]",
                "//div[contains(@class, 'meta')]//span[contains(@class, 'date')]",
                "//time"
            };

            foreach (var pattern in datePatterns)
            {
                var dateNode = document.DocumentNode.SelectSingleNode(pattern);
                if (dateNode != null)
                {
                    var dateText = dateNode.InnerText.Trim();
                    
                    // Try to parse the date
                    if (DateTime.TryParse(dateText, out var date))
                    {
                        return date;
                    }
                    
                    // Try to extract date using a regex pattern
                    var dateRegex = new Regex(@"\b\d{1,2}[\/\.\-]\d{1,2}[\/\.\-]\d{2,4}\b|\b\d{1,2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\s+\d{2,4}\b");
                    var match = dateRegex.Match(dateText);
                    if (match.Success && DateTime.TryParse(match.Value, out date))
                    {
                        return date;
                    }
                }
            }

            return null;
        }

        private string ExtractCategory(HtmlDocument document)
        {
            // Try to find category information
            var categoryPatterns = new[]
            {
                "//div[contains(@class, 'category')]",
                "//span[contains(@class, 'category')]",
                "//ul[contains(@class, 'breadcrumb')]//li[2]", // Often the second breadcrumb is the category
                "//div[contains(@class, 'meta')]//span[contains(@class, 'category')]"
            };

            foreach (var pattern in categoryPatterns)
            {
                var categoryNode = document.DocumentNode.SelectSingleNode(pattern);
                if (categoryNode != null)
                {
                    return categoryNode.InnerText.Trim();
                }
            }

            return "Uncategorized";
        }

        private string ExtractMainContent(HtmlDocument document)
        {
            // Try to find main content area using common patterns
            var contentPatterns = new[]
            {
                "//div[@id='content']",
                "//div[contains(@class, 'content')]",
                "//main",
                "//article",
                "//div[@role='main']"
            };

            foreach (var pattern in contentPatterns)
            {
                var contentNode = document.DocumentNode.SelectSingleNode(pattern);
                if (contentNode != null)
                {
                    // Remove scripts, comments, etc.
                    foreach (var node in contentNode.SelectNodes("//script|//style|//comment()")?.ToList() ?? new List<HtmlNode>())
                    {
                        node.Remove();
                    }

                    return contentNode.InnerText.Trim();
                }
            }

            // Fallback to body if no specific content area found
            return document.DocumentNode.SelectSingleNode("//body")?.InnerText.Trim() ?? "";
        }

        private Dictionary<string, string> ExtractMetadata(HtmlDocument document)
        {
            var metadata = new Dictionary<string, string>();

            // Extract metadata from meta tags
            var metaTags = document.DocumentNode.SelectNodes("//meta[@name and @content]") ?? new HtmlNodeCollection(document.DocumentNode);
            foreach (var metaTag in metaTags)
            {
                var name = metaTag.GetAttributeValue("name", "");
                var content = metaTag.GetAttributeValue("content", "");

                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(content))
                {
                    metadata[name] = content;
                }
            }

            // Try to find last updated info
            var lastUpdatedNode = document.DocumentNode.SelectSingleNode("//div[contains(text(), 'Last updated')]");
            if (lastUpdatedNode != null)
            {
                var lastUpdatedText = lastUpdatedNode.InnerText.Trim();
                metadata["LastUpdated"] = lastUpdatedText;
            }

            return metadata;
        }

        private List<ContentSection> ExtractSections(HtmlDocument document, string sectionType)
        {
            var sections = new List<ContentSection>();
            var selector = _sectionSelectors[sectionType];
            
            var sectionNodes = document.DocumentNode.SelectNodes(selector);
            if (sectionNodes != null)
            {
                foreach (var node in sectionNodes)
                {
                    var title = node.SelectSingleNode(".//h1|.//h2|.//h3|.//h4") ?.InnerText.Trim() ?? "Untitled Section";
                    
                    var section = new ContentSection
                    {
                        SectionType = sectionType,
                        Title = title,
                        Content = node.InnerText.Trim()
                    };
                    
                    // Try to extract date within this section
                    var dateNode = node.SelectSingleNode(".//span[contains(@class, 'date')]|.//div[contains(@class, 'date')]|.//time");
                    if (dateNode != null && DateTime.TryParse(dateNode.InnerText.Trim(), out var date))
                    {
                        section.PublishedDate = date;
                    }
                    
                    // Extract subsections by looking at subheadings
                    var subheadings = node.SelectNodes(".//h3|.//h4|.//h5");
                    if (subheadings != null)
                    {
                        foreach (var subheading in subheadings)
                        {
                            // Find all content until the next subheading
                            var currentNode = subheading.NextSibling;
                            var subContent = new StringBuilder();
                            
                            while (currentNode != null && 
                                  (currentNode.Name != "h3" && currentNode.Name != "h4" && currentNode.Name != "h5"))
                            {
                                subContent.Append(currentNode.InnerText + " ");
                                currentNode = currentNode.NextSibling;
                            }
                            
                            section.SubSections.Add(new ContentSection
                            {
                                SectionType = "subsection",
                                Title = subheading.InnerText.Trim(),
                                Content = subContent.ToString().Trim()
                            });
                        }
                    }
                    
                    sections.Add(section);
                }
            }
            
            return sections;
        }

        private List<ContentSection> ExtractSectionsByHeaders(HtmlDocument document)
        {
            var sections = new List<ContentSection>();
            var mainContent = document.DocumentNode.SelectSingleNode("//main") ?? 
                            document.DocumentNode.SelectSingleNode("//div[@id='content']") ?? 
                            document.DocumentNode.SelectSingleNode("//body");

            if (mainContent == null) return sections;

            var headers = mainContent.SelectNodes(".//h2");
            if (headers == null) return sections;

            foreach (var header in headers)
            {
                var sectionTitle = header.InnerText.Trim();
                
                // Find all content until the next h2
                var currentNode = header.NextSibling;
                var sectionContent = new StringBuilder();
                var subSections = new List<ContentSection>();
                
                while (currentNode != null && currentNode.Name != "h2")
                {
                    if (currentNode.Name == "h3")
                    {
                        var subTitle = currentNode.InnerText.Trim();
                        var subContent = new StringBuilder();
                        var subNode = currentNode.NextSibling;
                        
                        while (subNode != null && subNode.Name != "h2" && subNode.Name != "h3")
                        {
                            subContent.Append(subNode.InnerText + " ");
                            subNode = subNode.NextSibling;
                        }
                        
                        subSections.Add(new ContentSection
                        {
                            SectionType = "subsection",
                            Title = subTitle,
                            Content = subContent.ToString().Trim()
                        });
                        
                        currentNode = subNode;
                        continue;
                    }
                    
                    sectionContent.Append(currentNode.InnerText + " ");
                    currentNode = currentNode.NextSibling;
                }
                
                // Try to determine section type based on title keywords
                string sectionType = DetermineSectionType(sectionTitle);
                
                sections.Add(new ContentSection
                {
                    SectionType = sectionType,
                    Title = sectionTitle,
                    Content = sectionContent.ToString().Trim(),
                    SubSections = subSections
                });
            }
            
            return sections;
        }
        
        private string DetermineSectionType(string title)
        {
            title = title.ToLower();
            
            if (title.Contains("guidance") || title.Contains("guide"))
                return "guidance";
            if (title.Contains("regulation") || title.Contains("regulatory") || title.Contains("rule"))
                return "regulation";
            if (title.Contains("news") || title.Contains("announcement"))
                return "news";
            if (title.Contains("consultation"))
                return "consultation";
            if (title.Contains("enforcement") || title.Contains("action") || title.Contains("penalty"))
                return "enforcement";
            if (title.Contains("license") || title.Contains("licensing") || title.Contains("permit"))
                return "licensing";
            
            return "general";
        }
    }
}