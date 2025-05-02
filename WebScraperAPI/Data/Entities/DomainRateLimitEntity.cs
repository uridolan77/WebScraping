using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class DomainRateLimitEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Domain { get; set; }
        
        public int RequestsPerMinute { get; set; } = 60;

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
