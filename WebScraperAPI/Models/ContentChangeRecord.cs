using System;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents a record of content change detected by the scraper
    /// </summary>
    public class ContentChangeRecord
    {
        /// <summary>
        /// URL where the change was detected
        /// </summary>
        public string Url { get; set; } = string.Empty;
        
        /// <summary>
        /// Type of change detected
        /// </summary>
        public ContentChangeType ChangeType { get; set; }
        
        /// <summary>
        /// When the change was detected
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Significance score of the change (0-100)
        /// </summary>
        public int Significance { get; set; }
        
        /// <summary>
        /// Details about the change
        /// </summary>
        public string ChangeDetails { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// Types of content changes that can be detected
    /// </summary>
    public enum ContentChangeType
    {
        /// <summary>
        /// New content added
        /// </summary>
        Addition,
        
        /// <summary>
        /// Existing content removed
        /// </summary>
        Removal,
        
        /// <summary>
        /// Content modified
        /// </summary>
        Modification,
        
        /// <summary>
        /// Content structure changed
        /// </summary>
        StructuralChange,
        
        /// <summary>
        /// Other type of change
        /// </summary>
        Other
    }
}
