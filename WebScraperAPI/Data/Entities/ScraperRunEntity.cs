using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ScraperRunEntity
    {
        [Key]
        [Column("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("starttime")]
        public DateTime StartTime { get; set; }

        [Column("endtime")]
        public DateTime? EndTime { get; set; }

        [Column("urlsprocessed")]
        public int UrlsProcessed { get; set; }

        [Column("documentsprocessed")]
        public int DocumentsProcessed { get; set; }

        [Column("successful")]
        public bool? Successful { get; set; }

        [Column("errormessage")]
        public string? ErrorMessage { get; set; }

        [Column("elapsedtime")]
        public string? ElapsedTime { get; set; }

        // Navigation properties
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }

        public virtual ICollection<LogEntryEntity> LogEntries { get; set; } = new List<LogEntryEntity>();
        public virtual ICollection<ContentChangeRecordEntity> ContentChangeRecords { get; set; } = new List<ContentChangeRecordEntity>();
        public virtual ICollection<ProcessedDocumentEntity> ProcessedDocuments { get; set; } = new List<ProcessedDocumentEntity>();
    }
}
