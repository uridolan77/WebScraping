using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebScraperAPI.Data.Entities
{
    public class ScraperConfig
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        
        public DateTime CreatedAt { get; set; }
        
        public DateTime? LastRun { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string StartUrl { get; set; }
        
        [Required]
        [MaxLength(500)]
        public string BaseUrl { get; set; }
        
        [MaxLength(255)]
        public string OutputDirectory { get; set; } = "ScrapedData";
        
        public int DelayBetweenRequests { get; set; } = 1000;
        
        public int MaxConcurrentRequests { get; set; } = 5;
        
        public int MaxDepth { get; set; } = 5;
        
        public bool FollowExternalLinks { get; set; } = false;
        
        public bool RespectRobotsTxt { get; set; } = true;
        
        // Header/footer pattern learning
        public bool AutoLearnHeaderFooter { get; set; } = true;
        
        public int LearningPagesCount { get; set; } = 5;
        
        // Content Change Detection options
        public bool EnableChangeDetection { get; set; } = true;
        
        public bool TrackContentVersions { get; set; } = true;
        
        public int MaxVersionsToKeep { get; set; } = 5;
        
        // Adaptive Crawling options
        public bool EnableAdaptiveCrawling { get; set; } = true;
        
        public int PriorityQueueSize { get; set; } = 100;
        
        public bool AdjustDepthBasedOnQuality { get; set; } = true;
        
        // Smart Rate Limiting options
        public bool EnableAdaptiveRateLimiting { get; set; } = true;
        
        public int MinDelayBetweenRequests { get; set; } = 500;
        
        public int MaxDelayBetweenRequests { get; set; } = 5000;
        
        public bool MonitorResponseTimes { get; set; } = true;
        
        // Continuous monitoring settings
        public bool EnableContinuousMonitoring { get; set; } = false;
        
        public int MonitoringIntervalMinutes { get; set; } = 1440; // Default: 24 hours
        
        public bool NotifyOnChanges { get; set; } = false;
        
        [MaxLength(255)]
        public string NotificationEmail { get; set; }
        
        public bool TrackChangesHistory { get; set; } = true;
        
        // Navigation properties for Entity Framework relationships
        public List<ScraperRun> Runs { get; set; } = new List<ScraperRun>();
        
        public List<ScraperLog> Logs { get; set; } = new List<ScraperLog>();
        
        // User ownership - will be implemented in the authentication phase
        public string OwnerId { get; set; }
    }
}
