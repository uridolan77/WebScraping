using System;
using System.Threading.Tasks;
using WebScraper;

namespace WebScraper
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Web Scraper Starting...");

            // Initial configuration
            var config = new ScraperConfig
            {
                StartUrl = "https://www.gamblingcommission.gov.uk/licensees-and-businesses", // Change this to any starting URL
                BaseUrl = "https://www.gamblingcommission.gov.uk",  // Change this to the base domain you want to scrape
                OutputDirectory = "ScrapedData",  // Change the output directory name if desired
                DelayBetweenRequests = 1000, // 1 second delay to be respectful
                MaxConcurrentRequests = 5,
                MaxDepth = 5, // Maximum depth of links to follow (can be adjusted)
                FollowExternalLinks = true, // Only follow links within the specified domain
                RespectRobotsTxt = true,

                // Header/footer pattern learning
                AutoLearnHeaderFooter = true, // Enable auto-learning of header/footer patterns
                LearningPagesCount = 5, // Number of pages to analyze before learning patterns

                // Content Change Detection
                EnableChangeDetection = true,
                TrackContentVersions = true,
                MaxVersionsToKeep = 5,

                // Adaptive Crawling
                EnableAdaptiveCrawling = true,
                PriorityQueueSize = 100,
                AdjustDepthBasedOnQuality = true,

                // Smart Rate Limiting
                EnableAdaptiveRateLimiting = true,
                MinDelayBetweenRequests = 500,
                MaxDelayBetweenRequests = 5000,
                MonitorResponseTimes = true
            };

            // Create scraper and start the process
            var scraper = new Scraper(config);
            await scraper.InitializeAsync();
            await scraper.StartScrapingAsync();

            // Setup continuous scraping
            await scraper.SetupContinuousScrapingAsync(TimeSpan.FromDays(7)); // Check for updates weekly

            Console.WriteLine("Initial scraping completed. Press any key to exit...");
            Console.ReadKey();
        }
    }
}