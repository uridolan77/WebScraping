using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class PipelineMetricEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        public int ProcessingItems { get; set; }
        
        public int QueuedItems { get; set; }
        
        public int CompletedItems { get; set; }
        
        public int FailedItems { get; set; }
        
        public double AverageProcessingTimeMs { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
    }
}
