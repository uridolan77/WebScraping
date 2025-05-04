using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class WebhookTriggerEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("triggername")]
        public string TriggerName { get; set; } = string.Empty;

        // Navigation property
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
    }
}
