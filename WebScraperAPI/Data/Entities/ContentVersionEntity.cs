using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ContentVersionEntity
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string ContentId { get; set; }

        [Required]
        public string ScraperId { get; set; }

        [Required]
        public DateTime VersionDate { get; set; }

        [Required]
        public string HtmlContent { get; set; }

        public string TextContent { get; set; }

        public string ContentHash { get; set; }

        public string ChangeDescription { get; set; }

        public int ChangeSignificance { get; set; }

        // Navigation property
        public virtual ScrapedContentEntity ScrapedContent { get; set; }
    }
}
