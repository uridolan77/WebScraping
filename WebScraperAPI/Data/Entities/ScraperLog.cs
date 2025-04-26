using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.Extensions.Logging;

namespace WebScraperAPI.Data.Entities
{
    public class ScraperLog
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ScraperConfigId { get; set; }
        
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        public LogLevel Level { get; set; } = LogLevel.Information;
        
        // Navigation property
        [ForeignKey("ScraperConfigId")]
        public ScraperConfig ScraperConfig { get; set; }
        
        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss}] {Message}";
        }
    }
}
