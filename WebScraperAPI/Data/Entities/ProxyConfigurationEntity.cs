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
        public string Host { get; set; }
        
        public int Port { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Protocol { get; set; }
        
        public bool IsActive { get; set; }
        
        public int FailureCount { get; set; }
        
        public DateTime? LastUsed { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
