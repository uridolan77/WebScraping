using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.RateLimiting;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles request rate limiting for different domains
    /// </summary>
    public class RateLimitingComponent : ScraperComponentBase
    {
        private ConcurrentDictionary<string, AdaptiveRateLimiter> _domainLimiters = new ConcurrentDictionary<string, AdaptiveRateLimiter>();
        private AdaptiveRateLimiter _defaultLimiter;
        private bool _rateLimitingEnabled;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            _rateLimitingEnabled = Config.EnableRateLimiting;
            if (!_rateLimitingEnabled)
            {
                LogInfo("Rate limiting not enabled, component will be inactive");
                return;
            }
            
            try
            {
                // Create default rate limiter
                _defaultLimiter = new AdaptiveRateLimiter(
                    Config.DefaultRequestsPerMinute,
                    Config.DefaultAdaptiveRateFactor);
                
                LogInfo($"Rate limiting component initialized with default rate of {Config.DefaultRequestsPerMinute} requests per minute");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize rate limiting component");
            }
        }
        
        /// <summary>
        /// Waits for rate limit permission before making a request
        /// </summary>
        /// <param name="url">The URL to request</param>
        public async Task WaitForRatePermissionAsync(string url)
        {
            if (!_rateLimitingEnabled || string.IsNullOrEmpty(url))
                return;
                
            try
            {
                // Extract domain from URL
                string domain = ExtractDomain(url);
                
                // Get or create limiter for domain
                var limiter = GetRateLimiterForDomain(domain);
                
                // Wait for permission to proceed
                await limiter.WaitForPermissionAsync();
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error in rate limiting for URL: {url}");
            }
        }
        
        /// <summary>
        /// Reports a response from a domain to adjust rate limiting
        /// </summary>
        /// <param name="url">The URL requested</param>
        /// <param name="statusCode">The HTTP status code</param>
        /// <param name="responseTimeMs">The response time in milliseconds</param>
        public void ReportResponse(string url, int statusCode, double responseTimeMs)
        {
            if (!_rateLimitingEnabled || string.IsNullOrEmpty(url))
                return;
                
            try
            {
                // Extract domain from URL
                string domain = ExtractDomain(url);
                
                // Get limiter for domain
                var limiter = GetRateLimiterForDomain(domain);
                
                // Report success or failure based on status code
                if (statusCode >= 200 && statusCode < 300)
                {
                    // Success - adjust rate based on response time
                    limiter.ReportSuccess(responseTimeMs);
                }
                else if (statusCode == 429 || statusCode == 503)
                {
                    // Rate limited or service overloaded
                    limiter.ReportRateLimited();
                }
                else if (statusCode >= 500)
                {
                    // Server error
                    limiter.ReportServerError();
                }
                else if (statusCode >= 400)
                {
                    // Client error - not a rate limiting issue
                    limiter.ReportClientError();
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error reporting response for URL: {url}");
            }
        }
        
        /// <summary>
        /// Sets the requests per minute for a specific domain
        /// </summary>
        /// <param name="domain">The domain to set rate for</param>
        /// <param name="requestsPerMinute">The requests per minute</param>
        public void SetDomainRate(string domain, double requestsPerMinute)
        {
            if (!_rateLimitingEnabled || string.IsNullOrEmpty(domain))
                return;
                
            try
            {
                var limiter = GetRateLimiterForDomain(domain);
                limiter.SetRequestsPerMinute(requestsPerMinute);
                LogInfo($"Set rate limit for {domain} to {requestsPerMinute} requests per minute");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error setting domain rate for: {domain}");
            }
        }
        
        /// <summary>
        /// Gets the current rate limit for a domain
        /// </summary>
        /// <param name="domain">The domain</param>
        public double GetCurrentRate(string domain)
        {
            if (!_rateLimitingEnabled || string.IsNullOrEmpty(domain))
                return double.MaxValue;
                
            try
            {
                var limiter = GetRateLimiterForDomain(domain);
                return limiter.GetCurrentRequestsPerMinute();
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error getting current rate for: {domain}");
                return Config.DefaultRequestsPerMinute;
            }
        }
        
        /// <summary>
        /// Extracts domain from a URL
        /// </summary>
        private string ExtractDomain(string url)
        {
            try
            {
                var uri = new Uri(url);
                return uri.Host;
            }
            catch
            {
                return "unknown";
            }
        }
        
        /// <summary>
        /// Gets or creates a rate limiter for a domain
        /// </summary>
        private AdaptiveRateLimiter GetRateLimiterForDomain(string domain)
        {
            if (string.IsNullOrEmpty(domain))
                return _defaultLimiter;
                
            return _domainLimiters.GetOrAdd(domain, d => 
            {
                // Create new domain-specific limiter
                var limiter = new AdaptiveRateLimiter(
                    Config.DefaultRequestsPerMinute,
                    Config.DefaultAdaptiveRateFactor);
                
                LogInfo($"Created rate limiter for domain: {domain}");
                return limiter;
            });
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override Task OnScrapingCompletedAsync()
        {
            if (_rateLimitingEnabled)
            {
                // Log rate limiting statistics
                LogInfo("Rate limiting statistics:");
                foreach (var domain in _domainLimiters.Keys)
                {
                    var limiter = _domainLimiters[domain];
                    LogInfo($"  {domain}: {limiter.GetCurrentRequestsPerMinute():F2} requests/minute");
                }
            }
            
            return base.OnScrapingCompletedAsync();
        }
    }
}