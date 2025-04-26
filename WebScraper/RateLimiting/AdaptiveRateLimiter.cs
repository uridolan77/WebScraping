using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace WebScraper.RateLimiting
{
    public class SiteProfile
    {
        public string Domain { get; set; }
        public int RequestsMade { get; set; }
        public DateTime LastRequestTime { get; set; }
        public TimeSpan AverageResponseTime { get; set; }
        public TimeSpan CurrentDelay { get; set; }
        public int ErrorCount { get; set; }
        public int SuccessCount { get; set; }
        public bool IsSensitive { get; set; }
    }

    public class AdaptiveRateLimiter
    {
        private readonly ConcurrentDictionary<string, SiteProfile> _siteProfiles = new ConcurrentDictionary<string, SiteProfile>();
        private readonly Action<string> _logger;
        
        // Default settings
        private readonly TimeSpan _minDelay = TimeSpan.FromMilliseconds(500);
        private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(10);
        private readonly TimeSpan _defaultDelay = TimeSpan.FromSeconds(1);

        public AdaptiveRateLimiter(Action<string> logger = null)
        {
            _logger = logger ?? (_ => {}); // Default no-op logger if none provided
        }

        public void LoadSiteProfiles(ConcurrentDictionary<string, SiteProfile> profiles)
        {
            foreach (var item in profiles)
            {
                _siteProfiles[item.Key] = item.Value;
            }
            _logger($"Loaded rate limiting profiles for {profiles.Count} domains");
        }

        public void LoadSiteProfiles(Dictionary<string, SiteProfile> profiles)
        {
            foreach (var item in profiles)
            {
                _siteProfiles[item.Key] = item.Value;
            }
            _logger($"Loaded rate limiting profiles for {profiles.Count} domains");
        }

        public async Task DelayIfNeededAsync(string url)
        {
            try
            {
                var domain = new Uri(url).Host;
                _logger($"Checking rate limits for {domain}");

                // Get or create site profile
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);

                // Calculate time since last request
                var timeSinceLastRequest = DateTime.Now - profile.LastRequestTime;

                // If we haven't waited long enough, delay
                if (timeSinceLastRequest < profile.CurrentDelay)
                {
                    var delayTime = profile.CurrentDelay - timeSinceLastRequest;
                    _logger($"Rate limiting: Delaying request to {domain} for {delayTime.TotalMilliseconds}ms");
                    await Task.Delay(delayTime);
                }

                // Update last request time
                profile.LastRequestTime = DateTime.Now;
                profile.RequestsMade++;
            }
            catch (Exception ex)
            {
                _logger($"Error in rate limiter: {ex.Message}");
            }
        }

        public void UpdateSiteProfile(string url, TimeSpan responseTime, bool wasSuccessful)
        {
            try
            {
                var domain = new Uri(url).Host;
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);

                // Update counters
                if (wasSuccessful)
                {
                    profile.SuccessCount++;
                }
                else
                {
                    profile.ErrorCount++;
                    _logger($"Request error recorded for {domain}");
                }

                // Update average response time (simple moving average)
                if (profile.AverageResponseTime == TimeSpan.Zero)
                {
                    profile.AverageResponseTime = responseTime;
                }
                else
                {
                    // Calculate weighted average (more weight to new data)
                    profile.AverageResponseTime = TimeSpan.FromMilliseconds(
                        (profile.AverageResponseTime.TotalMilliseconds * 0.7) +
                        (responseTime.TotalMilliseconds * 0.3)
                    );
                }

                // Adjust delay based on response time and error rate
                AdjustDelay(profile);
                
                _logger($"Updated site profile for {domain}: Delay={profile.CurrentDelay.TotalMilliseconds}ms, Errors={profile.ErrorCount}, Success={profile.SuccessCount}");
            }
            catch (Exception ex)
            {
                _logger($"Error updating site profile: {ex.Message}");
            }
        }

        private void AdjustDelay(SiteProfile profile)
        {
            // Start with the default delay
            TimeSpan newDelay = _defaultDelay;

            // Adjust based on error rate
            if (profile.RequestsMade > 0)
            {
                double errorRate = (double)profile.ErrorCount / profile.RequestsMade;
                
                if (errorRate > 0.2) // More than 20% errors
                {
                    // Significant increase
                    newDelay = TimeSpan.FromMilliseconds(profile.CurrentDelay.TotalMilliseconds * 1.5);
                    _logger($"Increased delay for {profile.Domain} due to high error rate ({errorRate:P2})");
                }
                else if (errorRate > 0.05) // 5-20% errors
                {
                    // Moderate increase
                    newDelay = TimeSpan.FromMilliseconds(profile.CurrentDelay.TotalMilliseconds * 1.2);
                }
                else if (profile.SuccessCount > 10 && errorRate < 0.01) // Less than 1% errors after enough requests
                {
                    // Slight decrease if we've been successful
                    newDelay = TimeSpan.FromMilliseconds(profile.CurrentDelay.TotalMilliseconds * 0.95);
                }
            }

            // Adjust based on average response time
            if (profile.AverageResponseTime > TimeSpan.FromSeconds(2))
            {
                // Site is slow, we should be more conservative
                newDelay = TimeSpan.FromMilliseconds(Math.Max(
                    newDelay.TotalMilliseconds,
                    profile.AverageResponseTime.TotalMilliseconds * 0.5 // Wait at least half the avg response time
                ));
                
                _logger($"Adjusted delay for {profile.Domain} due to slow response times ({profile.AverageResponseTime.TotalMilliseconds}ms)");
            }

            // Ensure we stay within min and max delay constraints
            profile.CurrentDelay = TimeSpan.FromMilliseconds(
                Math.Min(
                    Math.Max(newDelay.TotalMilliseconds, _minDelay.TotalMilliseconds),
                    _maxDelay.TotalMilliseconds
                )
            );
        }

        public void MarkSiteAsSensitive(string domain, bool isSensitive = true)
        {
            var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
            profile.IsSensitive = isSensitive;
            
            if (isSensitive)
            {
                // For sensitive sites, we use a more conservative delay
                profile.CurrentDelay = TimeSpan.FromMilliseconds(
                    Math.Max(profile.CurrentDelay.TotalMilliseconds, 2000)
                );
                _logger($"Marked {domain} as sensitive, increased minimum delay to {profile.CurrentDelay.TotalMilliseconds}ms");
            }
        }

        private SiteProfile CreateNewProfile(string domain)
        {
            _logger($"Creating new rate limiting profile for {domain}");
            return new SiteProfile
            {
                Domain = domain,
                RequestsMade = 0,
                LastRequestTime = DateTime.Now.AddSeconds(-10), // Allow immediate first request
                AverageResponseTime = TimeSpan.Zero,
                CurrentDelay = _defaultDelay,
                ErrorCount = 0,
                SuccessCount = 0,
                IsSensitive = false
            };
        }

        public ConcurrentDictionary<string, SiteProfile> GetSiteProfiles()
        {
            return _siteProfiles;
        }
    }
}