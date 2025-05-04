using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ScrapedPageEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Column("scraperId")]
        public string ScraperId { get; set; } = string.Empty;

        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Column("htmlContent")]
        public string HtmlContent { get; set; } = string.Empty;

        [Column("textContent")]
        public string TextContent { get; set; } = string.Empty;

        [Column("scrapedAt")]
        public DateTime ScrapedAt { get; set; }

        [Column("scraperConfigId")]
        public int? ScraperConfigId { get; set; }

        // Navigation property
        [ForeignKey("ScraperId")]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
    }
}