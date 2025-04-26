using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperAPI.Data.Entities
{
    public class ScraperRun
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ScraperConfigId { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public int UrlsProcessed { get; set; }
        
        public bool Successful { get; set; }
        
        public string ErrorMessage { get; set; }
        
        [MaxLength(50)]
        public string ElapsedTime { get; set; }
        
        // Navigation property
        [ForeignKey("ScraperConfigId")]
        public ScraperConfig ScraperConfig { get; set; }
    }
}
