using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ProxyConfigurationEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string ProxyUrl { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public int FailureCount { get; set; } = 0;
        
        public DateTime? LastUsed { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
