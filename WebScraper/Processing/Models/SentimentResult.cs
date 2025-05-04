namespace WebScraper.Processing.Models
{
    /// <summary>
    /// Result of sentiment analysis
    /// </summary>
    public class SentimentResult
    {
        /// <summary>
        /// Score for positive sentiment (0-100)
        /// </summary>
        public int PositiveScore { get; set; }
        
        /// <summary>
        /// Score for negative sentiment (0-100)
        /// </summary>
        public int NegativeScore { get; set; }
        
        /// <summary>
        /// Overall sentiment (Positive, Negative, Neutral)
        /// </summary>
        public string OverallSentiment { get; set; }
        
        /// <summary>
        /// Confidence in the sentiment analysis (0-1)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Emotional tone detected (if any)
        /// </summary>
        public string EmotionalTone { get; set; }
    }
}
