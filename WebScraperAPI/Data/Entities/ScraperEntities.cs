namespace WebScraperApi.Data.Entities
{
    public class ScraperStatusEntityDuplicate
    {
        public int Id { get; set; }
        public string ScraperId { get; set; } = string.Empty;
        public bool IsRunning { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string ElapsedTime { get; set; } = string.Empty;
        public int UrlsProcessed { get; set; }
        public int UrlsQueued { get; set; }
        public int DocumentsProcessed { get; set; }
        public bool HasErrors { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime LastStatusUpdate { get; set; }
        public DateTime LastUpdate { get; set; }
        public DateTime LastMonitorCheck { get; set; }
        public string LastError { get; set; } = string.Empty;
    }

    public class ScraperMetricEntityDuplicate
    {
        public int Id { get; set; }
        public string ScraperId { get; set; } = string.Empty;
        public string MetricName { get; set; } = string.Empty;
        public double MetricValue { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class ScraperLogEntityDuplicate
    {
        public int Id { get; set; }
        public string ScraperId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string LogLevel { get; set; } = string.Empty; // Info, Warning, Error
        public string Message { get; set; } = string.Empty;
    }

    public class ScrapedPageEntityDuplicate
    {
        public int Id { get; set; }
        public string ScraperId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string HtmlContent { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public DateTime ScrapedAt { get; set; }
    }
}