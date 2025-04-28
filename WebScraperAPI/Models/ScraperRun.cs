using System;

namespace WebScraperApi.Models
{
    /// <summary>
    /// Represents a single run of a scraper
    /// </summary>
    public class ScraperRun
    {
        /// <summary>
        /// Unique identifier for this run
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// ID of the scraper that performed this run
        /// </summary>
        public string ScraperId { get; set; } = string.Empty;

        /// <summary>
        /// When the run started
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>
        /// When the run ended (null if still running)
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Number of URLs processed during this run
        /// </summary>
        public int UrlsProcessed { get; set; }

        /// <summary>
        /// Number of documents processed during this run
        /// </summary>
        public int DocumentsProcessed { get; set; }

        /// <summary>
        /// Whether the run was successful
        /// </summary>
        public bool Successful { get; set; }

        /// <summary>
        /// Error message if the run failed
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// Elapsed time as a formatted string
        /// </summary>
        public string ElapsedTime { get; set; } = string.Empty;

        /// <summary>
        /// Gets the duration of the run as a TimeSpan
        /// </summary>
        public TimeSpan? Duration => EndTime.HasValue ? EndTime.Value - StartTime : null;
    }
}
