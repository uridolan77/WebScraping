using System;
using System.Collections.Generic;

namespace WebScraper.RegulatoryFramework.Interfaces
{
    /// <summary>
    /// Represents a node in a content structure hierarchy
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Title of the content node
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Textual content of the node
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Type of content node (e.g., Section, Article, Division)
        /// </summary>
        public string NodeType { get; set; }
        
        /// <summary>
        /// Depth of the node in the hierarchy (0 for root)
        /// </summary>
        public int Depth { get; set; }
        
        /// <summary>
        /// Child content nodes
        /// </summary>
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Metadata about the content node
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Relevance score for the content (0.0-1.0)
        /// </summary>
        public double RelevanceScore { get; set; }
        
        /// <summary>
        /// HTML attributes of the source node
        /// </summary>
        public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Converts this node to a legacy WebScraper.ContentNode
        /// </summary>
        public WebScraper.ContentNode ToLegacyContentNode()
        {
            var result = new WebScraper.ContentNode
            {
                NodeType = this.NodeType,
                Content = this.Content,
                Depth = this.Depth,
                Title = this.Title,
                RelevanceScore = this.RelevanceScore,
                Attributes = new Dictionary<string, string>(this.Attributes),
                Children = new List<WebScraper.ContentNode>()
            };
            
            // Convert children recursively
            foreach (var child in this.Children)
            {
                result.Children.Add(child.ToLegacyContentNode());
            }
            
            // Convert metadata to the legacy format
            result.Metadata = new Dictionary<string, object>(this.Metadata);
            
            return result;
        }

        /// <summary>
        /// Creates a ContentNode from a legacy WebScraper.ContentNode
        /// </summary>
        public static ContentNode FromLegacyContentNode(WebScraper.ContentNode legacyNode)
        {
            if (legacyNode == null) return null;
            
            var result = new ContentNode
            {
                NodeType = legacyNode.NodeType,
                Content = legacyNode.Content,
                Depth = legacyNode.Depth,
                Title = legacyNode.Title,
                RelevanceScore = legacyNode.RelevanceScore,
                Attributes = new Dictionary<string, string>(legacyNode.Attributes ?? new Dictionary<string, string>())
            };
            
            // Convert children recursively
            foreach (var child in legacyNode.Children ?? new List<WebScraper.ContentNode>())
            {
                result.Children.Add(FromLegacyContentNode(child));
            }
            
            // Convert metadata
            if (legacyNode.Metadata != null)
            {
                foreach (var kvp in legacyNode.Metadata)
                {
                    result.Metadata[kvp.Key] = kvp.Value;
                }
            }
            
            return result;
        }
    }
}