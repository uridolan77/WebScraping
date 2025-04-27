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
        
        // Configurable rate limits
        private double _requestsPerMinute = 60; // Default is 1 request per second
        private readonly double _adaptiveRateFactor = 1.0;
        private readonly SemaphoreSlim _rateLimitSemaphore = new SemaphoreSlim(1, 1);
        
        public AdaptiveRateLimiter(Action<string> logger = null)
        {
            _logger = logger ?? (_ => {}); // Default no-op logger if none provided
        }
        
        // Constructor with requests per minute and adaptive rate factor parameters
        public AdaptiveRateLimiter(double requestsPerMinute, double adaptiveRateFactor, Action<string> logger = null)
        {
            _requestsPerMinute = requestsPerMinute;
            _logger = logger ?? (_ => {});
            
            // Validate and set the adaptive rate factor
            if (adaptiveRateFactor < 0.1)
                adaptiveRateFactor = 0.1;
            else if (adaptiveRateFactor > 10)
                adaptiveRateFactor = 10;
                
            _logger($"Initialized AdaptiveRateLimiter with {requestsPerMinute} requests per minute and adaptive factor {adaptiveRateFactor}");
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
        
        /// <summary>
        /// Wait until permission is granted to make a request
        /// </summary>
        public async Task<bool> WaitForPermissionAsync(string url, CancellationToken cancellationToken = default)
        {
            try
            {
                var domain = new Uri(url).Host;
                
                // Global rate limiting - wait for our turn
                await _rateLimitSemaphore.WaitAsync(cancellationToken);
                
                try
                {
                    // Get or create site profile
                    var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
                    
                    // Calculate time since last request
                    var timeSinceLastRequest = DateTime.Now - profile.LastRequestTime;
                    
                    // If we haven't waited long enough, delay
                    if (timeSinceLastRequest < profile.CurrentDelay)
                    {
                        var delayTime = profile.CurrentDelay - timeSinceLastRequest;
                        _logger($"Rate limiting: Delaying request to {domain} for {delayTime.TotalMilliseconds}ms");
                        
                        try
                        {
                            await Task.Delay(delayTime, cancellationToken);
                        }
                        catch (TaskCanceledException)
                        {
                            _logger($"Rate limiting wait was cancelled for {url}");
                            return false;
                        }
                    }
                    
                    // Update last request time
                    profile.LastRequestTime = DateTime.Now;
                    profile.RequestsMade++;
                    
                    return true;
                }
                finally
                {
                    _rateLimitSemaphore.Release();
                }
            }
            catch (OperationCanceledException)
            {
                _logger($"Rate limiting operation was cancelled for {url}");
                return false;
            }
            catch (Exception ex)
            {
                _logger($"Error in rate limiter: {ex.Message}");
                return false;
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
        
        /// <summary>
        /// Report a successful request
        /// </summary>
        public void ReportSuccess(string url, TimeSpan? responseTime = null)
        {
            try
            {
                var domain = new Uri(url).Host;
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
                
                profile.SuccessCount++;
                
                if (responseTime.HasValue)
                {
                    if (profile.AverageResponseTime == TimeSpan.Zero)
                    {
                        profile.AverageResponseTime = responseTime.Value;
                    }
                    else
                    {
                        // Calculate weighted average (more weight to new data)
                        profile.AverageResponseTime = TimeSpan.FromMilliseconds(
                            (profile.AverageResponseTime.TotalMilliseconds * 0.7) +
                            (responseTime.Value.TotalMilliseconds * 0.3)
                        );
                    }
                    
                    // Potentially decrease delay slightly after successful responses
                    if (profile.SuccessCount > 5 && profile.ErrorCount == 0)
                    {
                        var currentDelayMs = profile.CurrentDelay.TotalMilliseconds;
                        profile.CurrentDelay = TimeSpan.FromMilliseconds(Math.Max(
                            currentDelayMs * 0.95, // Decrease by 5%
                            _minDelay.TotalMilliseconds
                        ));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error reporting success: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Report a rate limited request (429)
        /// </summary>
        public void ReportRateLimited(string url)
        {
            try
            {
                var domain = new Uri(url).Host;
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
                
                profile.ErrorCount++;
                
                // Significantly increase delay when rate limited
                profile.CurrentDelay = TimeSpan.FromMilliseconds(Math.Min(
                    profile.CurrentDelay.TotalMilliseconds * 2, // Double the delay
                    _maxDelay.TotalMilliseconds
                ));
                
                _logger($"Rate limited on {domain} - increased delay to {profile.CurrentDelay.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger($"Error reporting rate limit: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Report a server error (5xx)
        /// </summary>
        public void ReportServerError(string url)
        {
            try
            {
                var domain = new Uri(url).Host;
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
                
                profile.ErrorCount++;
                
                // Moderately increase delay for server errors
                profile.CurrentDelay = TimeSpan.FromMilliseconds(Math.Min(
                    profile.CurrentDelay.TotalMilliseconds * 1.5, // 50% increase
                    _maxDelay.TotalMilliseconds
                ));
                
                _logger($"Server error on {domain} - increased delay to {profile.CurrentDelay.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger($"Error reporting server error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Report a client error (4xx)
        /// </summary>
        public void ReportClientError(string url)
        {
            try
            {
                var domain = new Uri(url).Host;
                var profile = _siteProfiles.GetOrAdd(domain, CreateNewProfile);
                
                profile.ErrorCount++;
                
                // Slight increase for client errors (they may indicate problems)
                profile.CurrentDelay = TimeSpan.FromMilliseconds(Math.Min(
                    profile.CurrentDelay.TotalMilliseconds * 1.1, // 10% increase
                    _maxDelay.TotalMilliseconds
                ));
                
                _logger($"Client error on {domain} - increased delay to {profile.CurrentDelay.TotalMilliseconds}ms");
            }
            catch (Exception ex)
            {
                _logger($"Error reporting client error: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Set the requests per minute rate
        /// </summary>
        public void SetRequestsPerMinute(double requestsPerMinute)
        {
            if (requestsPerMinute <= 0)
            {
                _logger("Invalid requests per minute rate - must be > 0");
                return;
            }
            
            _requestsPerMinute = requestsPerMinute;
            _logger($"Set rate limit to {requestsPerMinute} requests per minute");
            
            // Update all site profiles to reflect the new global rate
            foreach (var profile in _siteProfiles)
            {
                var baseDelay = TimeSpan.FromMilliseconds(60000 / _requestsPerMinute);
                var adjustedDelay = baseDelay;
                
                // If the site is sensitive, maintain a higher delay
                if (profile.Value.IsSensitive)
                {
                    adjustedDelay = TimeSpan.FromMilliseconds(Math.Max(
                        adjustedDelay.TotalMilliseconds,
                        2000 // Minimum 2 seconds for sensitive sites
                    ));
                }
                
                // Ensure we're within min/max bounds
                profile.Value.CurrentDelay = TimeSpan.FromMilliseconds(Math.Min(
                    Math.Max(adjustedDelay.TotalMilliseconds, _minDelay.TotalMilliseconds),
                    _maxDelay.TotalMilliseconds
                ));
            }
        }
        
        /// <summary>
        /// Get the current requests per minute rate
        /// </summary>
        public double GetCurrentRequestsPerMinute()
        {
            return _requestsPerMinute;
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