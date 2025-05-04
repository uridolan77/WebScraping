using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperLogEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; } = string.Empty;
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string LogLevel { get; set; } = string.Empty;
        
        [Required]
        public string Message { get; set; } = string.Empty;
        
        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}