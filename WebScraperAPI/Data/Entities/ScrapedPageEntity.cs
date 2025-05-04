using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScrapedPageEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; } = string.Empty;
        
        [Required]
        public string Url { get; set; } = string.Empty;
        
        [Required]
        public string HtmlContent { get; set; } = string.Empty;
        
        [Required]
        public string TextContent { get; set; } = string.Empty;
        
        [Required]
        public DateTime ScrapedAt { get; set; }
        
        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}