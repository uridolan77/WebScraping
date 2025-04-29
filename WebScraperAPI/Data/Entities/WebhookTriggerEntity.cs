using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class WebhookTriggerEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string TriggerName { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
