using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using WebScraper.RegulatoryFramework.Implementation;
using WebScraper.RegulatoryContent;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles content extraction from web pages
    /// </summary>
    public class ContentExtractionComponent : ScraperComponentBase, IContentExtractor
    {
        private StructuredContentExtractor _structuredExtractor;

        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);

            try
            {
                // Initialize the structured content extractor
                _structuredExtractor = new StructuredContentExtractor();

                LogInfo("Content extraction component initialized");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize content extraction component");
            }
        }

        /// <summary>
        /// Extracts text content from HTML
        /// </summary>
        public async Task<string> ExtractTextContentAsync(string html)
        {
            if (string.IsNullOrEmpty(html))
                return string.Empty;

            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                // Remove script and style elements
                var scriptNodes = htmlDoc.DocumentNode.SelectNodes("//script|//style");
                if (scriptNodes != null)
                {
                    foreach (var node in scriptNodes)
                    {
                        node.Remove();
                    }
                }

                // Get and clean text content
                string text = htmlDoc.DocumentNode.InnerText;
                text = CleanTextContent(text);

                return await Task.FromResult(text);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error extracting text content");
                return string.Empty;
            }
        }

        /// <summary>
        /// Extracts structured content from HTML
        /// </summary>
        public async Task<object> ExtractStructuredContentAsync(string html)
        {
            if (string.IsNullOrEmpty(html))
                return new List<WebScraper.ContentNode>();

            try
            {
                var htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(html);

                if (_structuredExtractor != null)
                {
                    var structuredContent = _structuredExtractor.ExtractStructuredContent(htmlDoc);
                    return await Task.FromResult(structuredContent);
                }

                // Fallback if structured extractor not available
                var nodes = new List<WebScraper.ContentNode>();

                // Extract headings
                var headings = htmlDoc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
                if (headings != null)
                {
                    foreach (var heading in headings)
                    {
                        string level = heading.Name.Substring(1);
                        nodes.Add(new WebScraper.ContentNode
                        {
                            NodeType = "heading",
                            Content = heading.InnerText.Trim(),
                            Depth = int.Parse(level)
                        });
                    }
                }

                // Extract paragraphs
                var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
                if (paragraphs != null)
                {
                    foreach (var paragraph in paragraphs)
                    {
                        if (!string.IsNullOrWhiteSpace(paragraph.InnerText))
                        {
                            nodes.Add(new WebScraper.ContentNode
                            {
                                NodeType = "paragraph",
                                Content = paragraph.InnerText.Trim(),
                                Depth = 0
                            });
                        }
                    }
                }

                // Extract lists
                var lists = htmlDoc.DocumentNode.SelectNodes("//ul|//ol");
                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        var listItems = list.SelectNodes(".//li");
                        if (listItems != null)
                        {
                            var listNode = new WebScraper.ContentNode
                            {
                                NodeType = list.Name == "ul" ? "unordered-list" : "ordered-list",
                                Content = "",
                                Depth = 0,
                                Children = new List<WebScraper.ContentNode>()
                            };

                            foreach (var item in listItems)
                            {
                                listNode.Children.Add(new WebScraper.ContentNode
                                {
                                    NodeType = "list-item",
                                    Content = item.InnerText.Trim(),
                                    Depth = 0
                                });
                            }

                            nodes.Add(listNode);
                        }
                    }
                }

                return await Task.FromResult<object>(nodes);
            }
            catch (Exception ex)
            {
                LogError(ex, "Error extracting structured content");
                return new List<WebScraper.ContentNode>();
            }
        }

        /// <summary>
        /// Cleans up extracted text content
        /// </summary>
        private string CleanTextContent(string text)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            // Replace multiple whitespace with a single space
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

            // Trim whitespace
            text = text.Trim();

            return text;
        }
    }
}