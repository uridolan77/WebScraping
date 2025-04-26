using System;
using System.Collections.Generic;
using System.Linq;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WebScraper.RegulatoryFramework.Configuration;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraper.RegulatoryFramework.Implementation
{
    /// <summary>
    /// Extracts structured content from HTML documents
    /// </summary>
    public class StructureAwareExtractor : IContentExtractor
    {
        private readonly HierarchicalExtractionConfig _config;
        private readonly ILogger<StructureAwareExtractor> _logger;
        
        public StructureAwareExtractor(HierarchicalExtractionConfig config, ILogger<StructureAwareExtractor> logger)
        {
            _config = config;
            _logger = logger;
        }
        
        /// <summary>
        /// Extracts plain text content from an HTML document
        /// </summary>
        public string ExtractTextContent(HtmlDocument document)
        {
            try
            {
                // Remove excluded elements
                if (!string.IsNullOrEmpty(_config.ExcludeSelector))
                {
                    var excludedNodes = document.DocumentNode.SelectNodes(_config.ExcludeSelector);
                    if (excludedNodes != null)
                    {
                        foreach (var node in excludedNodes)
                        {
                            node.Remove();
                        }
                    }
                }
                
                // Extract text from content elements
                var contentNodes = document.DocumentNode.SelectNodes(_config.ContentSelector);
                
                if (contentNodes == null || contentNodes.Count == 0)
                {
                    // Fallback to body text
                    return document.DocumentNode.InnerText;
                }
                
                var textContent = string.Join("\n\n", contentNodes.Select(n => n.InnerText.Trim()));
                
                _logger.LogInformation("Extracted {Length} characters of text content", textContent.Length);
                
                return textContent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting text content");
                return document.DocumentNode.InnerText;
            }
        }
        
        /// <summary>
        /// Extracts structured content from an HTML document
        /// </summary>
        public List<ContentNode> ExtractStructuredContent(HtmlDocument document)
        {
            try
            {
                // Remove excluded elements
                if (!string.IsNullOrEmpty(_config.ExcludeSelector))
                {
                    var excludedNodes = document.DocumentNode.SelectNodes(_config.ExcludeSelector);
                    if (excludedNodes != null)
                    {
                        foreach (var node in excludedNodes)
                        {
                            node.Remove();
                        }
                    }
                }
                
                // Get parent containers
                var parentNodes = document.DocumentNode.SelectNodes(_config.ParentSelector);
                
                if (parentNodes == null || parentNodes.Count == 0)
                {
                    // Fallback to body
                    var rootNode = new ContentNode
                    {
                        Title = document.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim() ?? "Untitled",
                        Content = document.DocumentNode.InnerText.Trim(),
                        Depth = 0,
                        NodeType = "Document"
                    };
                    
                    return new List<ContentNode> { rootNode };
                }
                
                var rootNodes = new List<ContentNode>();
                
                foreach (var parentNode in parentNodes)
                {
                    // Skip deeply nested parents
                    if (GetNodeDepth(parentNode) > _config.MaxHierarchyDepth)
                    {
                        continue;
                    }
                    
                    // Extract title
                    var titleNode = parentNode.SelectSingleNode(_config.TitleSelector);
                    string title = titleNode?.InnerText.Trim() ?? "Untitled Section";
                    
                    // Extract content
                    var contentNodes = parentNode.SelectNodes(_config.ContentSelector);
                    var content = "";
                    
                    if (contentNodes != null && contentNodes.Count > 0)
                    {
                        content = string.Join("\n\n", contentNodes.Select(n => n.InnerText.Trim()));
                    }
                    else
                    {
                        // Fallback to all text minus the title
                        content = parentNode.InnerText;
                        if (titleNode != null)
                        {
                            content = content.Replace(titleNode.InnerText, "").Trim();
                        }
                    }
                    
                    // Create the content node
                    var contentNode = new ContentNode
                    {
                        Title = title,
                        Content = content,
                        Depth = GetNodeDepth(parentNode),
                        NodeType = DetermineNodeType(parentNode)
                    };
                    
                    // Add metadata
                    ExtractNodeMetadata(contentNode, parentNode);
                    
                    // Extract child nodes recursively
                    ExtractChildNodes(contentNode, parentNode);
                    
                    rootNodes.Add(contentNode);
                }
                
                _logger.LogInformation("Extracted {Count} structured content nodes", rootNodes.Count);
                
                return rootNodes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting structured content");
                
                // Return minimal content on error
                var errorNode = new ContentNode
                {
                    Title = document.DocumentNode.SelectSingleNode("//title")?.InnerText.Trim() ?? "Error",
                    Content = "Error extracting structured content",
                    Depth = 0,
                    NodeType = "Error"
                };
                
                return new List<ContentNode> { errorNode };
            }
        }
        
        /// <summary>
        /// Gets the depth of a node in the DOM hierarchy
        /// </summary>
        private int GetNodeDepth(HtmlNode node)
        {
            int depth = 0;
            var current = node;
            
            while (current != null && current.ParentNode != null)
            {
                depth++;
                current = current.ParentNode;
            }
            
            return depth;
        }
        
        /// <summary>
        /// Determines the type of a node based on its HTML tag
        /// </summary>
        private string DetermineNodeType(HtmlNode node)
        {
            switch (node.Name.ToLower())
            {
                case "section":
                    return "Section";
                case "article":
                    return "Article";
                case "div":
                    var className = node.GetAttributeValue("class", "").ToLower();
                    if (className.Contains("card"))
                        return "Card";
                    if (className.Contains("panel"))
                        return "Panel";
                    return "Division";
                default:
                    return node.Name;
            }
        }
        
        /// <summary>
        /// Extracts metadata from a node
        /// </summary>
        private void ExtractNodeMetadata(ContentNode contentNode, HtmlNode htmlNode)
        {
            // Extract data-* attributes
            foreach (var attribute in htmlNode.Attributes)
            {
                if (attribute.Name.StartsWith("data-"))
                {
                    contentNode.Metadata[attribute.Name] = attribute.Value;
                }
            }
            
            // Extract ID
            var id = htmlNode.GetAttributeValue("id", null);
            if (!string.IsNullOrEmpty(id))
            {
                contentNode.Metadata["id"] = id;
            }
            
            // Extract class
            var className = htmlNode.GetAttributeValue("class", null);
            if (!string.IsNullOrEmpty(className))
            {
                contentNode.Metadata["class"] = className;
            }
            
            // Extract publication date if available
            var dateNode = htmlNode.SelectSingleNode(".//time");
            if (dateNode != null)
            {
                var dateTime = dateNode.GetAttributeValue("datetime", null);
                if (!string.IsNullOrEmpty(dateTime))
                {
                    contentNode.Metadata["PublishedDate"] = dateTime;
                }
                else
                {
                    contentNode.Metadata["PublishedDate"] = dateNode.InnerText.Trim();
                }
            }
        }
        
        /// <summary>
        /// Recursively extracts child nodes
        /// </summary>
        private void ExtractChildNodes(ContentNode parentNode, HtmlNode parentHtmlNode)
        {
            // Only process child nodes if we haven't reached the maximum depth
            if (parentNode.Depth >= _config.MaxHierarchyDepth)
            {
                return;
            }
            
            // Find child containers
            var childContainers = parentHtmlNode.SelectNodes(_config.ParentSelector);
            
            if (childContainers == null || childContainers.Count == 0)
            {
                return;
            }
            
            foreach (var childHtmlNode in childContainers)
            {
                // Skip if this is actually the same node (can happen with complex selectors)
                if (childHtmlNode.Equals(parentHtmlNode))
                {
                    continue;
                }
                
                // Skip if not a direct descendant (in the hierarchical sense)
                if (!IsDirectHierarchicalDescendant(parentHtmlNode, childHtmlNode))
                {
                    continue;
                }
                
                // Extract title
                var titleNode = childHtmlNode.SelectSingleNode(_config.TitleSelector);
                string title = titleNode?.InnerText.Trim() ?? "Untitled Subsection";
                
                // Extract content
                var contentNodes = childHtmlNode.SelectNodes(_config.ContentSelector);
                var content = "";
                
                if (contentNodes != null && contentNodes.Count > 0)
                {
                    content = string.Join("\n\n", contentNodes.Select(n => n.InnerText.Trim()));
                }
                else
                {
                    // Fallback to all text minus the title
                    content = childHtmlNode.InnerText;
                    if (titleNode != null)
                    {
                        content = content.Replace(titleNode.InnerText, "").Trim();
                    }
                }
                
                // Create child node
                var childNode = new ContentNode
                {
                    Title = title,
                    Content = content,
                    Depth = parentNode.Depth + 1,
                    NodeType = DetermineNodeType(childHtmlNode)
                };
                
                // Extract metadata
                ExtractNodeMetadata(childNode, childHtmlNode);
                
                // Add to parent
                parentNode.Children.Add(childNode);
                
                // Recursively extract grandchildren
                ExtractChildNodes(childNode, childHtmlNode);
            }
        }
        
        /// <summary>
        /// Determines if a node is a direct hierarchical descendant of another node
        /// </summary>
        private bool IsDirectHierarchicalDescendant(HtmlNode parent, HtmlNode potential)
        {
            // Check if the potential descendant is contained within the parent
            if (!parent.InnerHtml.Contains(potential.OuterHtml))
            {
                return false;
            }
            
            // Check if there are any other container elements between parent and potential
            var currentNode = potential.ParentNode;
            
            while (currentNode != null && !currentNode.Equals(parent))
            {
                // If we encounter another container node between them, it's not a direct descendant
                if (currentNode.MatchesSelector(_config.ParentSelector))
                {
                    return false;
                }
                
                currentNode = currentNode.ParentNode;
            }
            
            return currentNode != null; // True if we found the parent in the hierarchy
        }
    }
    
    /// <summary>
    /// Extension methods for HtmlNode
    /// </summary>
    public static class HtmlNodeExtensions
    {
        /// <summary>
        /// Determines if a node matches a CSS selector
        /// </summary>
        public static bool MatchesSelector(this HtmlNode node, string selector)
        {
            try
            {
                // This is a simplification - in a real implementation this would properly parse the selector
                if (string.IsNullOrEmpty(selector))
                {
                    return false;
                }
                
                // Split the selector by commas
                var selectors = selector.Split(',');
                
                foreach (var singleSelector in selectors)
                {
                    var trimmed = singleSelector.Trim();
                    
                    // Simple tag selector
                    if (trimmed.Equals(node.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    
                    // Simple class selector
                    if (trimmed.StartsWith("."))
                    {
                        var className = trimmed.Substring(1);
                        var classAttr = node.GetAttributeValue("class", "");
                        
                        if (!string.IsNullOrEmpty(classAttr))
                        {
                            var classes = classAttr.Split(' ');
                            if (classes.Any(c => c.Equals(className, StringComparison.OrdinalIgnoreCase)))
                            {
                                return true;
                            }
                        }
                    }
                    
                    // Simple ID selector
                    if (trimmed.StartsWith("#"))
                    {
                        var id = trimmed.Substring(1);
                        var nodeId = node.GetAttributeValue("id", "");
                        
                        if (nodeId.Equals(id, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    
                    // Tag with class (e.g., div.content)
                    if (trimmed.Contains(".") && !trimmed.StartsWith("."))
                    {
                        var parts = trimmed.Split('.');
                        var tagName = parts[0];
                        var className = parts[1];
                        
                        if (node.Name.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                        {
                            var classAttr = node.GetAttributeValue("class", "");
                            if (!string.IsNullOrEmpty(classAttr))
                            {
                                var classes = classAttr.Split(' ');
                                if (classes.Any(c => c.Equals(className, StringComparison.OrdinalIgnoreCase)))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}