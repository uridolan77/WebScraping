using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ContentChangeRecordEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        [Required]
        public string ChangeType { get; set; }
        
        [Required]
        public DateTime DetectedAt { get; set; }
        
        public int Significance { get; set; }
        
        public string ChangeDetails { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
    }
}
