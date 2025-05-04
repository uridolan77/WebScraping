using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Resilience;

namespace WebScraper.Security
{
    /// <summary>
    /// Validates security aspects of URLs and websites
    /// </summary>
    public class SecurityValidator
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of the SecurityValidator
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public SecurityValidator(ILogger logger)
        {
            _logger = logger;
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        /// <summary>
        /// Validates the security of a URL
        /// </summary>
        /// <param name="url">URL to validate</param>
        /// <returns>Security validation result</returns>
        public async Task<SecurityValidationResult> ValidateUrlSecurityAsync(string url)
        {
            var result = new SecurityValidationResult
            {
                Url = url
            };

            try
            {
                // Check for HTTPS
                result.IsHttps = url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

                // Make a HEAD request to check headers
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                using var response = await _httpClient.SendAsync(request);

                // Record status code
                result.StatusCode = (int)response.StatusCode;

                // Check security headers
                if (response.Headers.TryGetValues("Content-Security-Policy", out var cspValues))
                {
                    result.HasCsp = true;
                    result.CspDirectives = ParseCspDirectives(cspValues.FirstOrDefault());
                }

                if (response.Headers.TryGetValues("Strict-Transport-Security", out var hstsValues))
                {
                    result.HasHsts = true;
                    result.HstsDirectives = ParseHstsDirectives(hstsValues.FirstOrDefault());
                }

                result.HasXFrameOptions = response.Headers.Contains("X-Frame-Options");
                result.HasXContentTypeOptions = response.Headers.Contains("X-Content-Type-Options");
                result.HasReferrerPolicy = response.Headers.Contains("Referrer-Policy");
                result.HasPermissionsPolicy = response.Headers.Contains("Permissions-Policy");

                // Check for cookies
                if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
                {
                    result.HasCookies = true;
                    result.CookiesSecure = cookies.All(c => c.Contains("Secure"));
                    result.CookiesHttpOnly = cookies.All(c => c.Contains("HttpOnly"));
                    result.CookiesSameSite = cookies.All(c => c.Contains("SameSite"));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating security for URL: {url}");
                result.Error = ex.Message;
                return result;
            }
        }

        /// <summary>
        /// Parses Content-Security-Policy header directives
        /// </summary>
        /// <param name="csp">CSP header value</param>
        /// <returns>Dictionary of directives and their values</returns>
        private Dictionary<string, List<string>> ParseCspDirectives(string csp)
        {
            var directives = new Dictionary<string, List<string>>();
            if (string.IsNullOrEmpty(csp))
                return directives;

            foreach (var directive in csp.Split(';'))
            {
                var trimmedDirective = directive.Trim();
                if (string.IsNullOrEmpty(trimmedDirective))
                    continue;

                var parts = trimmedDirective.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 0)
                {
                    var name = parts[0];
                    var values = parts.Skip(1).ToList();
                    directives[name] = values;
                }
            }

            return directives;
        }

        /// <summary>
        /// Parses Strict-Transport-Security header directives
        /// </summary>
        /// <param name="hsts">HSTS header value</param>
        /// <returns>Dictionary of directives and their values</returns>
        private Dictionary<string, string> ParseHstsDirectives(string hsts)
        {
            var directives = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(hsts))
                return directives;

            foreach (var directive in hsts.Split(';'))
            {
                var trimmedDirective = directive.Trim();
                if (string.IsNullOrEmpty(trimmedDirective))
                    continue;

                var parts = trimmedDirective.Split('=', 2);
                if (parts.Length == 1)
                {
                    directives[parts[0]] = string.Empty;
                }
                else if (parts.Length == 2)
                {
                    directives[parts[0]] = parts[1];
                }
            }

            return directives;
        }

        /// <summary>
        /// Evaluates the overall security score of a URL
        /// </summary>
        /// <param name="result">Security validation result</param>
        /// <returns>Security score (0-100)</returns>
        public int EvaluateSecurityScore(SecurityValidationResult result)
        {
            int score = 0;

            // HTTPS is a must (30 points)
            if (result.IsHttps)
                score += 30;

            // Content-Security-Policy (20 points)
            if (result.HasCsp)
                score += 20;

            // HSTS (15 points)
            if (result.HasHsts)
                score += 15;

            // X-Frame-Options (10 points)
            if (result.HasXFrameOptions)
                score += 10;

            // X-Content-Type-Options (5 points)
            if (result.HasXContentTypeOptions)
                score += 5;

            // Referrer-Policy (5 points)
            if (result.HasReferrerPolicy)
                score += 5;

            // Permissions-Policy (5 points)
            if (result.HasPermissionsPolicy)
                score += 5;

            // Secure cookies (10 points)
            if (result.HasCookies)
            {
                if (result.CookiesSecure)
                    score += 4;
                if (result.CookiesHttpOnly)
                    score += 3;
                if (result.CookiesSameSite)
                    score += 3;
            }
            else
            {
                // No cookies is also good
                score += 10;
            }

            return score;
        }
    }

    /// <summary>
    /// Result of a security validation
    /// </summary>
    public class SecurityValidationResult
    {
        /// <summary>
        /// URL that was validated
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Whether the URL uses HTTPS
        /// </summary>
        public bool IsHttps { get; set; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Whether the site has a Content-Security-Policy header
        /// </summary>
        public bool HasCsp { get; set; }

        /// <summary>
        /// Content-Security-Policy directives
        /// </summary>
        public Dictionary<string, List<string>> CspDirectives { get; set; } = new Dictionary<string, List<string>>();

        /// <summary>
        /// Whether the site has a Strict-Transport-Security header
        /// </summary>
        public bool HasHsts { get; set; }

        /// <summary>
        /// Strict-Transport-Security directives
        /// </summary>
        public Dictionary<string, string> HstsDirectives { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Whether the site has an X-Frame-Options header
        /// </summary>
        public bool HasXFrameOptions { get; set; }

        /// <summary>
        /// Whether the site has an X-Content-Type-Options header
        /// </summary>
        public bool HasXContentTypeOptions { get; set; }

        /// <summary>
        /// Whether the site has a Referrer-Policy header
        /// </summary>
        public bool HasReferrerPolicy { get; set; }

        /// <summary>
        /// Whether the site has a Permissions-Policy header
        /// </summary>
        public bool HasPermissionsPolicy { get; set; }

        /// <summary>
        /// Whether the site sets cookies
        /// </summary>
        public bool HasCookies { get; set; }

        /// <summary>
        /// Whether all cookies have the Secure flag
        /// </summary>
        public bool CookiesSecure { get; set; }

        /// <summary>
        /// Whether all cookies have the HttpOnly flag
        /// </summary>
        public bool CookiesHttpOnly { get; set; }

        /// <summary>
        /// Whether all cookies have the SameSite attribute
        /// </summary>
        public bool CookiesSameSite { get; set; }

        /// <summary>
        /// Error message if validation failed
        /// </summary>
        public string Error { get; set; }
    }
}
