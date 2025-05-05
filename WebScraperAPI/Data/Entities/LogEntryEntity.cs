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
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        public string Message { get; set; }

        public string Level { get; set; } = "INFO";

        public string? RunId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
        public virtual ScraperRunEntity? Run { get; set; }
    }
}
