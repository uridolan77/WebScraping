using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperScheduleEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Name { get; set; }
        
        [Required]
        public string CronExpression { get; set; }
        
        public bool IsActive { get; set; }
        
        public DateTime? LastRun { get; set; }
        
        public DateTime? NextRun { get; set; }
        
        public int? MaxRuntimeMinutes { get; set; }
        
        public string NotificationEmail { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
