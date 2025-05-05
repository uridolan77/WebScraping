using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ContentChangeRecordEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? ScraperId { get; set; }

        [Required]
        public string? Url { get; set; }

        [Required]
        public DateTime DetectedAt { get; set; } = DateTime.Now;

        [Required]
        public string? ChangeType { get; set; }

        public float SignificanceScore { get; set; }

        public string? ChangeSummary { get; set; }

        public string? PreviousVersionHash { get; set; }

        public string? CurrentVersionHash { get; set; }

        public string? RunId { get; set; }

        public string? ScraperConfigId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
        public virtual ScraperRunEntity? Run { get; set; }
    }
}
