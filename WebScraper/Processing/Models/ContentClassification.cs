using System.Collections.Generic;

namespace WebScraper.Processing.Models
{
    /// <summary>
    /// Result of content classification
    /// </summary>
    public class ContentClassification
    {
        /// <summary>
        /// Length of the content in characters
        /// </summary>
        public int ContentLength { get; set; }
        
        /// <summary>
        /// Number of sentences in the content
        /// </summary>
        public int SentenceCount { get; set; }
        
        /// <summary>
        /// Number of paragraphs in the content
        /// </summary>
        public int ParagraphCount { get; set; }
        
        /// <summary>
        /// Readability score (higher means more complex)
        /// </summary>
        public double ReadabilityScore { get; set; }
        
        /// <summary>
        /// Score for positive sentiment
        /// </summary>
        public int PositiveScore { get; set; }
        
        /// <summary>
        /// Score for negative sentiment
        /// </summary>
        public int NegativeScore { get; set; }
        
        /// <summary>
        /// Overall sentiment (Positive, Negative, Neutral)
        /// </summary>
        public string OverallSentiment { get; set; }
        
        /// <summary>
        /// Type of document
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Confidence in the classification (0-1)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Entities extracted from the content
        /// </summary>
        public List<Entity> Entities { get; set; } = new List<Entity>();
        
        /// <summary>
        /// Error message if classification failed
        /// </summary>
        public string Error { get; set; }
    }
    
    /// <summary>
    /// Entity extracted from content
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Type of entity (Person, Organization, Location, Date, etc.)
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Value of the entity
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Position in the text
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Confidence score for the entity (0-1)
        /// </summary>
        public double Confidence { get; set; }
    }
}
