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
        /// Gets or sets whether significant changes were detected
        /// </summary>
        public bool HasSignificantChanges { get; set; }
        
        /// <summary>
        /// Gets or sets the type of change detected
        /// </summary>
        public ChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Gets or sets a summary of the changes
        /// </summary>
        public string Summary { get; set; }
        
        /// <summary>
        /// Gets or sets the dictionary of changed sections
        /// </summary>
        public Dictionary<string, string> ChangedSections { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets the dictionary of significant changes with section name as key and change description as value
        /// </summary>
        public Dictionary<string, string> SignificantChanges { get; set; } = new Dictionary<string, string>();
        
        /// <summary>
        /// Gets or sets the list of changed sentences
        /// </summary>
        public List<ChangedSentence> ChangedSentences { get; set; } = new List<ChangedSentence>();
        
        /// <summary>
        /// Gets or sets the importance of the detected changes
        /// </summary>
        public RegulatoryChangeImportance Importance { get; set; }
        
        /// <summary>
        /// Gets or sets the date when the changes were detected
        /// </summary>
        public DateTime DetectedAt { get; set; } = DateTime.Now;
        
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
        
        /// <summary>
        /// Gets or sets key terms added in the new version
        /// </summary>
        public List<string> AddedTerms { get; set; } = new List<string>();
        
        /// <summary>
        /// Determines if the changes are critical based on importance level and regulatory implications
        /// </summary>
        /// <returns>True if changes are critical, false otherwise</returns>
        public bool HasCriticalChanges()
        {
            // Critical importance level indicates critical changes
            if (Importance == RegulatoryChangeImportance.Critical)
                return true;
                
            // High importance with regulatory implications is also critical
            if (Importance == RegulatoryChangeImportance.High && RegulatoryImplications.Count > 0)
                return true;
                
            // Consider changes critical if percentage is above 25%
            if (ChangePercentage > 25)
                return true;
                
            return false;
        }
    }
    
    /// <summary>
    /// Represents a changed sentence with before and after text
    /// </summary>
    public class ChangedSentence
    {
        /// <summary>
        /// Original text before the change
        /// </summary>
        public string Before { get; set; }
        
        /// <summary>
        /// New text after the change
        /// </summary>
        public string After { get; set; }
        
        /// <summary>
        /// The section or context where this sentence appears
        /// </summary>
        public string Context { get; set; }
        
        /// <summary>
        /// The type of change for this sentence
        /// </summary>
        public SentenceChangeType ChangeType { get; set; }
        
        /// <summary>
        /// Importance of this particular sentence change
        /// </summary>
        public double Importance { get; set; }
    }
    
    /// <summary>
    /// Types of sentence changes
    /// </summary>
    public enum SentenceChangeType
    {
        /// <summary>
        /// Sentence was added
        /// </summary>
        Added,
        
        /// <summary>
        /// Sentence was removed
        /// </summary>
        Removed,
        
        /// <summary>
        /// Sentence was modified
        /// </summary>
        Modified
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