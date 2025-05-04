using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ScraperScheduleEntity
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Required]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column("cronexpression")]
        public string CronExpression { get; set; } = string.Empty;

        [Column("isactive")]
        public bool IsActive { get; set; }

        [Column("lastrun")]
        public DateTime? LastRun { get; set; }

        [Column("nextrun")]
        public DateTime? NextRun { get; set; }

        [Column("maxruntimeminutes")]
        public int? MaxRuntimeMinutes { get; set; }

        [Column("notificationemail")]
        public string NotificationEmail { get; set; } = string.Empty;

        // Navigation property
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
    }
}
