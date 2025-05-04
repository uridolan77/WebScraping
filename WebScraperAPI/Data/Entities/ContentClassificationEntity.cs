using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    /// <summary>
    /// Entity for storing content classification results
    /// </summary>
    public class ContentClassificationEntity
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the scraper ID
        /// </summary>
        [Required]
        public string ScraperId { get; set; }
        
        /// <summary>
        /// Gets or sets the URL
        /// </summary>
        [Required]
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the content length
        /// </summary>
        public int ContentLength { get; set; }
        
        /// <summary>
        /// Gets or sets the sentence count
        /// </summary>
        public int SentenceCount { get; set; }
        
        /// <summary>
        /// Gets or sets the paragraph count
        /// </summary>
        public int ParagraphCount { get; set; }
        
        /// <summary>
        /// Gets or sets the readability score
        /// </summary>
        public double ReadabilityScore { get; set; }
        
        /// <summary>
        /// Gets or sets the positive score
        /// </summary>
        public int PositiveScore { get; set; }
        
        /// <summary>
        /// Gets or sets the negative score
        /// </summary>
        public int NegativeScore { get; set; }
        
        /// <summary>
        /// Gets or sets the overall sentiment
        /// </summary>
        public string OverallSentiment { get; set; }
        
        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public string DocumentType { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets when the content was classified
        /// </summary>
        [Required]
        public DateTime ClassifiedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Gets or sets the entities
        /// </summary>
        public virtual ICollection<ContentEntityEntity> Entities { get; set; }
        
        /// <summary>
        /// Gets or sets the scraper config
        /// </summary>
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
