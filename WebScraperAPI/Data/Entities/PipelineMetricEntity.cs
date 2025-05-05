using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class PipelineMetricEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Column("processingitems")]
        public int ProcessingItems { get; set; }

        [Column("queueditems")]
        public int QueuedItems { get; set; }

        [Column("completeditems")]
        public int CompletedItems { get; set; }

        [Column("faileditems")]
        public int FailedItems { get; set; }

        [Column("averageprocessingtimems")]
        public double AverageProcessingTimeMs { get; set; }

        [Column("runid")]
        public string? RunId { get; set; }

        // Navigation properties
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }

        [NotMapped]
        public virtual ScraperRunEntity? Run { get; set; }
    }
}
