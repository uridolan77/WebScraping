namespace WebScraper.Processing.Models
{
    /// <summary>
    /// Features extracted from text analysis
    /// </summary>
    public class TextFeatures
    {
        /// <summary>
        /// Length of the text in characters
        /// </summary>
        public int Length { get; set; }
        
        /// <summary>
        /// Number of sentences in the text
        /// </summary>
        public int SentenceCount { get; set; }
        
        /// <summary>
        /// Number of paragraphs in the text
        /// </summary>
        public int ParagraphCount { get; set; }
        
        /// <summary>
        /// Readability score (higher means more complex)
        /// </summary>
        public double ReadabilityScore { get; set; }
        
        /// <summary>
        /// Average sentence length in words
        /// </summary>
        public double AverageSentenceLength { get; set; }
        
        /// <summary>
        /// Average word length in characters
        /// </summary>
        public double AverageWordLength { get; set; }
        
        /// <summary>
        /// Number of unique words in the text
        /// </summary>
        public int UniqueWordCount { get; set; }
    }
}
