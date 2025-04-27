using System.Collections.Generic;

namespace WebScraper.RegulatoryFramework.Interfaces
{
    /// <summary>
    /// Represents a node in a structured content hierarchy
    /// </summary>
    public class ContentNode
    {
        /// <summary>
        /// Title or heading of the node
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Text content of the node
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Depth of the node in the hierarchy (0 = root)
        /// </summary>
        public int Depth { get; set; }
        
        /// <summary>
        /// Type of the node (e.g., Section, Article, Division)
        /// </summary>
        public string NodeType { get; set; }
        
        /// <summary>
        /// Child nodes
        /// </summary>
        public List<ContentNode> Children { get; set; } = new List<ContentNode>();
        
        /// <summary>
        /// Additional metadata for this node
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
}