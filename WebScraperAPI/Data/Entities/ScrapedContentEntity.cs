using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScrapedContentEntity
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Url { get; set; }
        
        public string Title { get; set; }
        
        [Required]
        public string HtmlContent { get; set; }
        
        public string TextContent { get; set; }
        
        [Required]
        public DateTime ScrapedAt { get; set; }
        
        public DateTime? LastModified { get; set; }
        
        public bool HasChanged { get; set; }
        
        public string ContentHash { get; set; }
        
        public string MetadataJson { get; set; }
        
        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ICollection<ContentVersionEntity> Versions { get; set; }
    }
}
