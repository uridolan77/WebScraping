using System;
using System.Collections.Generic;

namespace WebScraper.RegulatoryFramework.Interfaces
{
    /// <summary>
    /// Represents results from detecting significant changes in content
    /// </summary>
    public class SignificantChangesResult
    {
        /// <summary>
        /// Whether any significant changes were detected
        /// </summary>
        public bool HasSignificantChanges { get; set; }
        
        /// <summary>
        /// Type of change detected
        /// </summary>
        public ChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Importance level of the detected changes
        /// </summary>
        public int ImportanceLevel { get; set; }
        
        /// <summary>
        /// Summary of the detected changes
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Detailed description of changes
        /// </summary>
        public string DetailedDescription { get; set; }
        
        /// <summary>
        /// Keywords or terms added in the new version
        /// </summary>
        public List<string> AddedTerms { get; set; } = new List<string>();
        
        /// <summary>
        /// Keywords or terms removed in the new version
        /// </summary>
        public List<string> RemovedTerms { get; set; } = new List<string>();
        
        /// <summary>
        /// Categories of content that were changed
        /// </summary>
        public List<string> ChangedCategories { get; set; } = new List<string>();
        
        /// <summary>
        /// Sections that were changed
        /// </summary>
        public Dictionary<string, string> ChangedSections { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Date when the change was detected
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Create a new result with default values
        /// </summary>
        public SignificantChangesResult()
        {
        }
        
        /// <summary>
        /// Create a new result with specified change type
        /// </summary>
        public SignificantChangesResult(ChangeType changeType)
        {
            ChangeType = changeType;
            HasSignificantChanges = changeType != ChangeType.None;
            
            // Set importance level based on change type
            ImportanceLevel = changeType switch
            {
                ChangeType.Major => 3,
                ChangeType.Minor => 2,
                ChangeType.Format => 1,
                _ => 0
            };
        }
    }
    
    /// <summary>
    /// Represents the type of change detected in content
    /// </summary>
    public enum ChangeType
    {
        /// <summary>
        /// No changes detected
        /// </summary>
        None,
        
        /// <summary>
        /// Only formatting or style changes
        /// </summary>
        Format,
        
        /// <summary>
        /// Minor content changes that don't affect meaning
        /// </summary>
        Minor,
        
        /// <summary>
        /// Significant content changes that affect meaning
        /// </summary>
        Major,
        
        /// <summary>
        /// Complete overhaul of the content
        /// </summary>
        Complete
    }
}