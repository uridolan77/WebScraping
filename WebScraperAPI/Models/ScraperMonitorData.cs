using System;
using System.Collections.Generic;

namespace WebScraperApi.Models
{
    public class ScraperMonitorData
    {
        public string CurrentUrl { get; set; }
        public int PercentComplete { get; set; }
        public string EstimatedTimeRemaining { get; set; }
        public int CurrentDepth { get; set; }
        public double RequestsPerSecond { get; set; }
        public string MemoryUsage { get; set; }
        public string CpuUsage { get; set; }
        public int ActiveThreads { get; set; }
        public List<object> RecentActivity { get; set; } = new List<object>();
    }
}
