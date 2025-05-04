using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ProxyConfigurationEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("proxyurl")]
        public string ProxyUrl { get; set; } = string.Empty;

        [Column("username")]
        public string Username { get; set; } = string.Empty;

        [Column("password")]
        public string Password { get; set; } = string.Empty;

        [Column("isactive")]
        public bool IsActive { get; set; } = true;

        [Column("failurecount")]
        public int FailureCount { get; set; } = 0;

        [Column("lastused")]
        public DateTime? LastUsed { get; set; }

        // Navigation property
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
    }
}
