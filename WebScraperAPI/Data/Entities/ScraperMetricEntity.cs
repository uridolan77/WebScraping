using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ScraperMetricEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        // Add ScraperConfigId to match database schema if needed
        [Column("scraperconfigid")]
        public string? ScraperConfigId { get; set; }

        [Required]
        [Column("scrapername")]
        public string ScraperName { get; set; } = string.Empty;

        [Required]
        [Column("metricname")]
        public string MetricName { get; set; } = string.Empty;

        [Required]
        [Column("metricvalue")]
        public double MetricValue { get; set; }

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Column("runid")]
        public string? RunId { get; set; }

        // Navigation properties - mark as NotMapped to avoid tracking issues
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }

        [ForeignKey("RunId")]
        [NotMapped]
        public virtual ScraperRunEntity? Run { get; set; }
    }
}
