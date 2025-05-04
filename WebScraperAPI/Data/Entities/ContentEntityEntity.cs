using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    /// <summary>
    /// Entity for storing content entities
    /// </summary>
    public class ContentEntityEntity
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        [Key]
        public string Id { get; set; }
        
        /// <summary>
        /// Gets or sets the classification ID
        /// </summary>
        [Required]
        public string ClassificationId { get; set; }
        
        /// <summary>
        /// Gets or sets the entity type
        /// </summary>
        [Required]
        public string Type { get; set; }
        
        /// <summary>
        /// Gets or sets the entity value
        /// </summary>
        [Required]
        public string Value { get; set; }
        
        /// <summary>
        /// Gets or sets the entity position
        /// </summary>
        public int Position { get; set; }
        
        /// <summary>
        /// Gets or sets the entity confidence
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Gets or sets the classification
        /// </summary>
        public virtual ContentClassificationEntity Classification { get; set; }
    }
}
