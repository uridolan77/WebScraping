using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ProcessedDocumentEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string ScraperId { get; set; }

        [Required]
        public string Url { get; set; }

        public string Title { get; set; }

        [Required]
        public string DocumentType { get; set; }

        [Required]
        public DateTime ProcessedAt { get; set; } = DateTime.Now;

        public long ContentSizeBytes { get; set; }

        public string? RunId { get; set; }

        public string? ScraperConfigId { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
        public virtual ScraperRunEntity? Run { get; set; }
        public virtual ICollection<DocumentMetadataEntity>? Metadata { get; set; } = new List<DocumentMetadataEntity>();
    }
}
