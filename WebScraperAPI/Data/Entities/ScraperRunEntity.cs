using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ScraperRunEntity
    {
        [Key]
        public string Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public int UrlsProcessed { get; set; }
        
        public int DocumentsProcessed { get; set; }
        
        public bool? Successful { get; set; }
        
        public string ErrorMessage { get; set; }
        
        public string ElapsedTime { get; set; }

        // Navigation properties
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
        public virtual ICollection<LogEntryEntity> LogEntries { get; set; }
        public virtual ICollection<ContentChangeRecordEntity> ContentChangeRecords { get; set; }
        public virtual ICollection<ProcessedDocumentEntity> ProcessedDocuments { get; set; }
    }
}
