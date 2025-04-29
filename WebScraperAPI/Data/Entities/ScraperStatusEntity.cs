using System;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperStatusEntity
    {
        [Key]
        public string ScraperId { get; set; }
        
        public bool IsRunning { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public string ElapsedTime { get; set; }
        
        public int UrlsProcessed { get; set; }
        
        public int UrlsQueued { get; set; }
        
        public int DocumentsProcessed { get; set; }
        
        public bool HasErrors { get; set; }
        
        public string Message { get; set; }
        
        public DateTime? LastStatusUpdate { get; set; }
        
        public DateTime? LastUpdate { get; set; }
        
        public DateTime? LastMonitorCheck { get; set; }
        
        public string LastError { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
