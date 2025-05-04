using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebScraperApi.Data.Entities
{
    public class ScraperStatusEntity
    {
        [Key]
        [Column("scraperid")]
        public string ScraperId { get; set; } = string.Empty;

        [Column("isrunning")]
        public bool IsRunning { get; set; }

        [Column("starttime")]
        public DateTime? StartTime { get; set; }

        [Column("endtime")]
        public DateTime? EndTime { get; set; }

        [Column("elapsedtime")]
        public string? ElapsedTime { get; set; }

        [Column("urlsprocessed")]
        public int UrlsProcessed { get; set; }

        [Column("urlsqueued")]
        public int UrlsQueued { get; set; }

        [Column("documentsprocessed")]
        public int DocumentsProcessed { get; set; }

        [Column("haserrors")]
        public bool HasErrors { get; set; }

        [Column("message")]
        public string? Message { get; set; }

        [Column("laststatusupdate")]
        public DateTime? LastStatusUpdate { get; set; }

        [Column("lastupdate")]
        public DateTime? LastUpdate { get; set; }

        [Column("lastmonitorcheck")]
        public DateTime? LastMonitorCheck { get; set; }

        [Column("lasterror")]
        public string? LastError { get; set; }

        // Navigation property
        [NotMapped]
        public virtual ScraperConfigEntity? ScraperConfig { get; set; }
    }
}
