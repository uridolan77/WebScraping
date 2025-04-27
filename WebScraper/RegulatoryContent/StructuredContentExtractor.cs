using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using WebScraper.Processing;

namespace WebScraper.RegulatoryContent
{
    /// <summary>
    /// Extracts structured content from HTML documents
    /// Specialized in extracting regulatory content with semantic understanding
    /// </summary>
    public class StructuredContentExtractor
    {
        private readonly Action<string> _logger;
        
        public StructuredContentExtractor()
        {
            _logger = _ => { }; // Empty logger by default
        }

        public StructuredContentExtractor(Action<string> logger)
        {
            _logger = logger ?? (_ => { });
        }

        /// <summary>
        /// Extracts structured content from an HTML document
        /// </summary>
        /// <param name="htmlDoc">The HTML document</param>
        /// <returns>A list of content nodes representing the document structure</returns>
        public List<ContentNode> ExtractStructuredContent(HtmlDocument htmlDoc)
        {
            if (htmlDoc == null)
                return new List<ContentNode>();
                
            var nodes = new List<ContentNode>();
            
            // Extract document structure and content
            ExtractHeadings(htmlDoc, nodes);
            ExtractParagraphs(htmlDoc, nodes);
            ExtractLists(htmlDoc, nodes);
            ExtractTables(htmlDoc, nodes);
            
            return nodes;
        }
        
        /// <summary>
        /// Extracts structured content from an HTML document and includes URL information
        /// </summary>
        /// <param name="htmlDoc">The HTML document</param>
        /// <param name="url">The URL of the document</param>
        /// <returns>A ContentSection object containing the document structure</returns>
        public ContentSection ExtractStructuredContent(HtmlDocument htmlDoc, string url)
        {
            if (htmlDoc == null)
                return new ContentSection();
                
            try {
                _logger?.Invoke($"Extracting structured content from {url}");
                
                // Create the content section
                var contentSection = new ContentSection
                {
                    Url = url,
                    Title = ExtractTitle(htmlDoc),
                    PublishedDate = ExtractPublishedDate(htmlDoc),
                    Category = DetermineCategory(url, htmlDoc),
                    ContentNodes = new List<ContentNode>()
                };
                
                // Extract content nodes
                var nodes = ExtractStructuredContent(htmlDoc);
                contentSection.ContentNodes.AddRange(nodes);
                
                return contentSection;
            }
            catch (Exception ex) {
                _logger?.Invoke($"Error extracting structured content: {ex.Message}");
                return new ContentSection { Url = url };
            }
        }
        
        private string ExtractTitle(HtmlDocument htmlDoc)
        {
            // Try to get title from h1 first
            var h1 = htmlDoc.DocumentNode.SelectSingleNode("//h1");
            if (h1 != null && !string.IsNullOrWhiteSpace(h1.InnerText))
            {
                return h1.InnerText.Trim();
            }
            
            // Fall back to title tag
            var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
            if (titleNode != null && !string.IsNullOrWhiteSpace(titleNode.InnerText))
            {
                return titleNode.InnerText.Trim();
            }
            
            return "Untitled Document";
        }
        
        private DateTime? ExtractPublishedDate(HtmlDocument htmlDoc)
        {
            // Try common date patterns
            var metaDate = htmlDoc.DocumentNode.SelectSingleNode("//meta[@property='article:published_time']/@content")?.GetAttributeValue("content", null);
            if (!string.IsNullOrEmpty(metaDate))
            {
                if (DateTime.TryParse(metaDate, out DateTime date))
                {
                    return date;
                }
            }
            
            // Look for date patterns in text
            var dateNodes = htmlDoc.DocumentNode.SelectNodes("//*[contains(@class, 'date') or contains(@class, 'published')]");
            if (dateNodes != null)
            {
                foreach (var node in dateNodes)
                {
                    if (DateTime.TryParse(node.InnerText, out DateTime date))
                    {
                        return date;
                    }
                }
            }
            
            // No date found
            return null;
        }
        
        private string DetermineCategory(string url, HtmlDocument htmlDoc)
        {
            // Extract from URL path segments
            var segments = url.Split('/');
            foreach (var segment in segments)
            {
                if (!string.IsNullOrEmpty(segment) && 
                    segment.Length > 3 && 
                    !segment.EndsWith(".html") && 
                    !segment.EndsWith(".php") &&
                    !segment.Contains("."))
                {
                    return segment;
                }
            }
            
            // Try to extract from breadcrumbs or categories
            var categoryNode = htmlDoc.DocumentNode.SelectSingleNode("//div[contains(@class, 'category')]/a");
            if (categoryNode != null)
            {
                return categoryNode.InnerText.Trim();
            }
            
            return "General";
        }
        
        private void ExtractHeadings(HtmlDocument htmlDoc, List<ContentNode> nodes)
        {
            var headings = htmlDoc.DocumentNode.SelectNodes("//h1|//h2|//h3|//h4|//h5|//h6");
            if (headings != null)
            {
                foreach (var heading in headings)
                {
                    string level = heading.Name.Substring(1);
                    nodes.Add(new ContentNode
                    {
                        Type = "heading",
                        Content = heading.InnerText.Trim(),
                        Level = int.Parse(level)
                    });
                }
            }
        }
        
        private void ExtractParagraphs(HtmlDocument htmlDoc, List<ContentNode> nodes)
        {
            var paragraphs = htmlDoc.DocumentNode.SelectNodes("//p");
            if (paragraphs != null)
            {
                foreach (var paragraph in paragraphs)
                {
                    if (!string.IsNullOrWhiteSpace(paragraph.InnerText))
                    {
                        nodes.Add(new ContentNode
                        {
                            Type = "paragraph",
                            Content = paragraph.InnerText.Trim(),
                            Level = 0
                        });
                    }
                }
            }
        }
        
        private void ExtractLists(HtmlDocument htmlDoc, List<ContentNode> nodes)
        {
            var lists = htmlDoc.DocumentNode.SelectNodes("//ul|//ol");
            if (lists != null)
            {
                foreach (var list in lists)
                {
                    var listItems = list.SelectNodes(".//li");
                    if (listItems != null)
                    {
                        var listNode = new ContentNode
                        {
                            Type = list.Name == "ul" ? "unordered-list" : "ordered-list",
                            Content = "",
                            Level = 0,
                            Children = new List<ContentNode>()
                        };
                        
                        foreach (var item in listItems)
                        {
                            listNode.Children.Add(new ContentNode
                            {
                                Type = "list-item",
                                Content = item.InnerText.Trim(),
                                Level = 0
                            });
                        }
                        
                        nodes.Add(listNode);
                    }
                }
            }
        }
        
        private void ExtractTables(HtmlDocument htmlDoc, List<ContentNode> nodes)
        {
            var tables = htmlDoc.DocumentNode.SelectNodes("//table");
            if (tables != null)
            {
                foreach (var table in tables)
                {
                    // Create a table node
                    var tableNode = new ContentNode
                    {
                        Type = "table",
                        Content = "",
                        Level = 0,
                        Children = new List<ContentNode>()
                    };
                    
                    // Extract headers
                    var headers = table.SelectNodes(".//th");
                    if (headers != null && headers.Count > 0)
                    {
                        var headerRow = new ContentNode
                        {
                            Type = "table-header-row",
                            Content = "",
                            Level = 0,
                            Children = new List<ContentNode>()
                        };
                        
                        foreach (var header in headers)
                        {
                            headerRow.Children.Add(new ContentNode
                            {
                                Type = "table-cell",
                                Content = header.InnerText.Trim(),
                                Level = 0
                            });
                        }
                        
                        tableNode.Children.Add(headerRow);
                    }
                    
                    // Extract rows
                    var rows = table.SelectNodes(".//tr");
                    if (rows != null)
                    {
                        foreach (var row in rows)
                        {
                            // Skip if this is a header row we've already processed
                            if (row.SelectNodes(".//th") != null && row.SelectNodes(".//th").Count > 0)
                                continue;
                                
                            var cells = row.SelectNodes(".//td");
                            if (cells != null && cells.Count > 0)
                            {
                                var rowNode = new ContentNode
                                {
                                    Type = "table-row",
                                    Content = "",
                                    Level = 0,
                                    Children = new List<ContentNode>()
                                };
                                
                                foreach (var cell in cells)
                                {
                                    rowNode.Children.Add(new ContentNode
                                    {
                                        Type = "table-cell",
                                        Content = cell.InnerText.Trim(),
                                        Level = 0
                                    });
                                }
                                
                                tableNode.Children.Add(rowNode);
                            }
                        }
                    }
                    
                    nodes.Add(tableNode);
                }
            }
        }
        
        /// <summary>
        /// Represents a section of content with structured organization
        /// </summary>
        public class ContentSection
        {
            /// <summary>
            /// URL of the content
            /// </summary>
            public string Url { get; set; }
            
            /// <summary>
            /// Title of the content
            /// </summary>
            public string Title { get; set; }
            
            /// <summary>
            /// When the content was published (if available)
            /// </summary>
            public DateTime? PublishedDate { get; set; }
            
            /// <summary>
            /// Category of the content
            /// </summary>
            public string Category { get; set; }
            
            /// <summary>
            /// List of content nodes making up the structure
            /// </summary>
            public List<ContentNode> ContentNodes { get; set; } = new List<ContentNode>();
            
            /// <summary>
            /// Gets the full text content of all nodes
            /// </summary>
            public string FullText
            {
                get
                {
                    var text = new System.Text.StringBuilder();
                    foreach (var node in ContentNodes)
                    {
                        text.AppendLine(node.Content);
                        
                        if (node.Children != null)
                        {
                            foreach (var child in node.Children)
                            {
                                text.AppendLine(child.Content);
                            }
                        }
                    }
                    return text.ToString();
                }
            }
        }
    }
}