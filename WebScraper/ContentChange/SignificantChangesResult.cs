using System;
using System.Collections.Generic;

namespace WebScraper.ContentChange
{
    /// <summary>
    /// Represents the result of analyzing changes between content versions
    /// </summary>
    public class SignificantChangesResult
    {
        /// <summary>
        /// Gets or sets the type of change detected
        /// </summary>
        public ContentChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Gets or sets the dictionary of significant changes with section name as key and change description as value
        /// </summary>
        public Dictionary<string, string> SignificantChanges { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets the importance of the detected changes
        /// </summary>
        public RegulatoryChangeImportance Importance { get; set; }
        
        /// <summary>
        /// Gets or sets the date of the previous version
        /// </summary>
        public DateTime PreviousVersionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the date of the current version
        /// </summary>
        public DateTime CurrentVersionDate { get; set; }
        
        /// <summary>
        /// Gets or sets whether the content hash changed
        /// </summary>
        public bool HashChanged { get; set; }
        
        /// <summary>
        /// Gets or sets the percentage of content changed
        /// </summary>
        public double ChangePercentage { get; set; }
        
        /// <summary>
        /// Gets or sets any regulatory implications of the changes
        /// </summary>
        public List<string> RegulatoryImplications { get; set; } = new List<string>();
    }
    
    /// <summary>
    /// Represents the type of change detected in content
    /// </summary>
    public enum ContentChangeType
    {
        /// <summary>
        /// No changes detected
        /// </summary>
        NoChange,
        
        /// <summary>
        /// Minor changes detected
        /// </summary>
        Minor,
        
        /// <summary>
        /// Significant changes detected
        /// </summary>
        Significant,
        
        /// <summary>
        /// Major changes detected
        /// </summary>
        Major,
        
        /// <summary>
        /// The document has been removed
        /// </summary>
        Removed,
        
        /// <summary>
        /// The document is a new one
        /// </summary>
        New
    }
    
    /// <summary>
    /// Represents the importance of detected changes
    /// </summary>
    public enum RegulatoryChangeImportance
    {
        /// <summary>
        /// Low importance changes
        /// </summary>
        Low,
        
        /// <summary>
        /// Medium importance changes
        /// </summary>
        Medium,
        
        /// <summary>
        /// High importance changes
        /// </summary>
        High,
        
        /// <summary>
        /// Critical importance changes
        /// </summary>
        Critical
    }
}