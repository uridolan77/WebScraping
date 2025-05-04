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

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("url")]
        public string Url { get; set; } = string.Empty;

        [Required]
        [Column("htmlcontent")]
        public string HtmlContent { get; set; } = string.Empty;

        [Required]
        [Column("textcontent")]
        public string TextContent { get; set; } = string.Empty;

        [Required]
        [Column("scrapedat")]
        public DateTime ScrapedAt { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}