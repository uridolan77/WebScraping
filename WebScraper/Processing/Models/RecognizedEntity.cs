namespace WebScraper.Processing.Models
{
    /// <summary>
    /// Entity recognized in text
    /// </summary>
    public class RecognizedEntity
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
        /// Length of the entity in the text
        /// </summary>
        public int Length { get; set; }
        
        /// <summary>
        /// Confidence score for the entity recognition (0-1)
        /// </summary>
        public double Confidence { get; set; }
    }
}
