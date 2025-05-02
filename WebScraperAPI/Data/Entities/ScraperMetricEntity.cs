using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperMetricEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string ScraperName { get; set; }
        
        [Required]
        public string MetricName { get; set; }
        
        [Required]
        public double MetricValue { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
