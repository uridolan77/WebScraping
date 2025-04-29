using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class LogEntryEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public DateTime Timestamp { get; set; }
        
        [Required]
        public string Message { get; set; }
        
        [Required]
        public string Level { get; set; }
        
        public string RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ScraperRunEntity Run { get; set; }
    }
}
