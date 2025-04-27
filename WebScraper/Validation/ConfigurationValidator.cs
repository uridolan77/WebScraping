// filepath: c:\dev\WebScraping\WebScraper\Validation\ConfigurationValidator.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebScraper.Validation
{
    /// <summary>
    /// Validates scraper configuration to ensure it's suitable for operation
    /// </summary>
    public class ConfigurationValidator
    {
        private readonly HttpClient _httpClient;
        private readonly Action<string> _logger;

        public ConfigurationValidator(HttpClient httpClient = null, Action<string> logger = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _logger = logger ?? Console.WriteLine;
        }

        /// <summary>
        /// Validates a scraper configuration
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <returns>Validation results</returns>
        public async Task<ValidationResult> ValidateConfigurationAsync(ScraperConfig config)
        {
            var result = new ValidationResult();

            try
            {
                // Validate basic properties
                ValidateBasicProperties(config, result);

                // Validate URL and network settings
                await ValidateUrlAndNetworkSettingsAsync(config, result);

                // Validate paths and directories
                ValidatePathsAndDirectories(config, result);

                // Validate performance settings
                ValidatePerformanceSettings(config, result);

                // Validate monitoring and notification settings
                ValidateMonitoringSettings(config, result);

                // Validate regulatory compliance settings
                ValidateRegulatorySettings(config, result);

                // Validate advanced settings
                ValidateAdvancedSettings(config, result);

                result.IsValid = result.Errors.Count == 0;
                result.CanRunWithWarnings = result.Warnings.Count > 0 && result.Errors.Count == 0;
                
                _logger($"Configuration validation completed with {result.Errors.Count} errors and {result.Warnings.Count} warnings.");
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.CanRunWithWarnings = false;
                result.Errors.Add($"Unexpected error during validation: {ex.Message}");
                _logger($"Validation error: {ex.Message}");
            }

            return result;
        }

        private void ValidateBasicProperties(ScraperConfig config, ValidationResult result)
        {
            // Check for null or empty name
            if (string.IsNullOrWhiteSpace(config.Name))
            {
                result.Errors.Add("Configuration name cannot be empty.");
            }
            else if (config.Name.Length > 100)
            {
                result.Warnings.Add("Configuration name is quite long (>100 chars) which may cause display issues.");
            }

            // Validate that required URLs are provided
            if (string.IsNullOrWhiteSpace(config.StartUrl))
            {
                result.Errors.Add("Start URL cannot be empty.");
            }
            else if (!Uri.TryCreate(config.StartUrl, UriKind.Absolute, out _))
            {
                result.Errors.Add($"Invalid start URL format: {config.StartUrl}");
            }

            // Base URL validation (if provided)
            if (!string.IsNullOrWhiteSpace(config.BaseUrl) && !Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out _))
            {
                result.Errors.Add($"Invalid base URL format: {config.BaseUrl}");
            }

            // Warn about very old configurations
            if (config.CreatedAt != default && (DateTime.Now - config.CreatedAt).TotalDays > 365)
            {
                result.Warnings.Add($"This configuration is over 1 year old (created on {config.CreatedAt.ToShortDateString()}). Consider reviewing for updates.");
            }
        }

        private async Task ValidateUrlAndNetworkSettingsAsync(ScraperConfig config, ValidationResult result)
        {
            if (!string.IsNullOrWhiteSpace(config.StartUrl) && Uri.TryCreate(config.StartUrl, UriKind.Absolute, out var uri))
            {
                // Test if the domain resolves
                try
                {
                    IPHostEntry hostEntry = await Dns.GetHostEntryAsync(uri.Host);
                    if (hostEntry.AddressList.Length == 0)
                    {
                        result.Errors.Add($"Could not resolve domain: {uri.Host}");
                    }

                    // Check if the URL is reachable (only on valid hostnames)
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Head, config.StartUrl);
                        var response = await _httpClient.SendAsync(request);
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            int statusCode = (int)response.StatusCode;
                            result.Warnings.Add($"Start URL returned HTTP status code {statusCode}.");
                            
                            if (statusCode == 403)
                            {
                                result.Errors.Add("Access to the start URL is forbidden (HTTP 403). The site may block automated access.");
                            }
                            else if (statusCode == 404)
                            {
                                result.Errors.Add("Start URL returns 'Not Found' (HTTP 404). The page may have been moved or deleted.");
                            }
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        result.Warnings.Add($"Could not connect to start URL: {ex.Message}");
                    }
                    catch (TaskCanceledException)
                    {
                        result.Warnings.Add("Connection to start URL timed out. The site may be slow or blocking automated requests.");
                    }
                }
                catch (Exception ex)
                {
                    result.Warnings.Add($"Could not resolve domain: {ex.Message}");
                }
                
                // Check for HTTPS if dealing with sensitive data
                if ((config.ClassifyRegulatoryDocuments || config.IsUKGCWebsite || config.EnableRegulatoryContentAnalysis) &&
                    !uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                {
                    result.Warnings.Add("Using non-HTTPS URL with regulatory content processing. Consider using HTTPS for security.");
                }

                // Verify robots.txt conformance if enabled
                if (config.RespectRobotsTxt)
                {
                    try
                    {
                        string robotsUrl = $"{uri.Scheme}://{uri.Host}/robots.txt";
                        var robotsResponse = await _httpClient.GetAsync(robotsUrl);
                        
                        if (robotsResponse.IsSuccessStatusCode)
                        {
                            string robotsContent = await robotsResponse.Content.ReadAsStringAsync();
                            if (RobotsTxtDisallowsAccess(robotsContent, config.StartUrl))
                            {
                                result.Errors.Add("The site's robots.txt disallows access to the start URL.");
                            }
                        }
                        else
                        {
                            result.Warnings.Add("Could not retrieve robots.txt, but 'RespectRobotsTxt' is enabled.");
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Warnings.Add($"Error checking robots.txt: {ex.Message}");
                    }
                }
            }

            // Validate rate limiting values
            if (config.DelayBetweenRequests < 0)
            {
                result.Errors.Add("Delay between requests cannot be negative.");
            }
            else if (config.DelayBetweenRequests < 500)
            {
                result.Warnings.Add("Delay between requests is very low (<500ms). This may cause high server load or get your IP blocked.");
            }

            // Check for excessive parallelism
            if (config.MaxConcurrentRequests <= 0)
            {
                result.Errors.Add("Maximum concurrent requests must be greater than zero.");
            }
            else if (config.MaxConcurrentRequests > 10)
            {
                result.Warnings.Add("Very high number of concurrent requests (>10) may lead to IP blocking or server overload.");
            }

            // Validate adaptive rate limiting settings if enabled
            if (config.EnableAdaptiveRateLimiting)
            {
                if (config.MinDelayBetweenRequests < 0 || config.MaxDelayBetweenRequests < 0)
                {
                    result.Errors.Add("Minimum and maximum delay between requests cannot be negative.");
                }
                
                if (config.MinDelayBetweenRequests >= config.MaxDelayBetweenRequests)
                {
                    result.Errors.Add("Minimum delay must be less than maximum delay for adaptive rate limiting.");
                }
            }
        }

        private void ValidatePathsAndDirectories(ScraperConfig config, ValidationResult result)
        {
            if (string.IsNullOrWhiteSpace(config.OutputDirectory))
            {
                result.Errors.Add("Output directory cannot be empty.");
            }
            else
            {
                try
                {
                    // Check if the path is valid
                    if (!Path.IsPathRooted(config.OutputDirectory))
                    {
                        // This is a relative path, which is fine
                        result.Warnings.Add($"Output directory is a relative path: '{config.OutputDirectory}'. Consider using an absolute path.");
                    }
                    else
                    {
                        // For absolute paths, check directory exists or can be created
                        if (!Directory.Exists(config.OutputDirectory))
                        {
                            try
                            {
                                // Try creating the directory as a test
                                Directory.CreateDirectory(config.OutputDirectory);
                                Directory.Delete(config.OutputDirectory);
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Cannot create output directory: {ex.Message}");
                            }
                        }
                        else
                        {
                            // Check write permissions by trying to create a temporary file
                            try
                            {
                                string testFile = Path.Combine(config.OutputDirectory, $"test_{Guid.NewGuid()}.tmp");
                                File.WriteAllText(testFile, "test");
                                File.Delete(testFile);
                            }
                            catch (Exception ex)
                            {
                                result.Errors.Add($"Cannot write to output directory: {ex.Message}");
                            }
                        }

                        // Check for low disk space
                        try
                        {
                            var driveInfo = new DriveInfo(Path.GetPathRoot(config.OutputDirectory));
                            var freeSpaceGB = driveInfo.AvailableFreeSpace / (1024.0 * 1024 * 1024);
                            
                            if (freeSpaceGB < 1)
                            {
                                result.Warnings.Add($"Low disk space on drive containing output directory: {freeSpaceGB:F2} GB free.");
                            }
                        }
                        catch (Exception ex)
                        {
                            result.Warnings.Add($"Could not check disk space: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Invalid output directory path: {ex.Message}");
                }
            }
        }

        private void ValidatePerformanceSettings(ScraperConfig config, ValidationResult result)
        {
            // Validate max depth
            if (config.MaxDepth <= 0)
            {
                result.Errors.Add("Maximum depth must be greater than zero.");
            }
            else if (config.MaxDepth > 10)
            {
                result.Warnings.Add("Very high maximum depth (>10) may lead to excessive crawling and resource usage.");
            }

            // Validate learning settings
            if (config.AutoLearnHeaderFooter && config.LearningPagesCount <= 0)
            {
                result.Errors.Add("Learning pages count must be greater than zero when auto-learn is enabled.");
            }
            else if (config.AutoLearnHeaderFooter && config.LearningPagesCount > 20)
            {
                result.Warnings.Add("Very high learning pages count (>20) may lead to excessive resource usage during initialization.");
            }

            // Validate change detection settings
            if (config.EnableChangeDetection && !config.TrackContentVersions)
            {
                result.Warnings.Add("Change detection is enabled, but content versions are not being tracked. Consider enabling version tracking.");
            }

            if (config.TrackContentVersions && config.MaxVersionsToKeep <= 0)
            {
                result.Errors.Add("Maximum versions to keep must be greater than zero when tracking content versions.");
            }
        }

        private void ValidateMonitoringSettings(ScraperConfig config, ValidationResult result)
        {
            if (config.EnableContinuousMonitoring)
            {
                if (config.MonitoringIntervalMinutes <= 0)
                {
                    result.Errors.Add("Monitoring interval must be greater than zero when continuous monitoring is enabled.");
                }
                else if (config.MonitoringIntervalMinutes < 5)
                {
                    result.Warnings.Add("Very short monitoring interval (<5 minutes) may lead to excessive resource usage and potential IP blocking.");
                }
                else if (config.MonitoringIntervalMinutes > 1440) // 24 hours
                {
                    result.Warnings.Add("Monitoring interval is longer than 24 hours, which may miss frequent updates to monitored content.");
                }

                // Validate notification settings
                if (config.NotifyOnChanges)
                {
                    if (string.IsNullOrWhiteSpace(config.NotificationEmail))
                    {
                        result.Errors.Add("Notification email address is required when notifications are enabled.");
                    }
                    else if (!IsValidEmail(config.NotificationEmail))
                    {
                        result.Errors.Add($"Invalid email format: {config.NotificationEmail}");
                    }
                }
                else
                {
                    result.Warnings.Add("Continuous monitoring is enabled but notifications are disabled. Changes will be detected but not reported.");
                }
            }
        }

        private void ValidateRegulatorySettings(ScraperConfig config, ValidationResult result)
        {
            // Validate UKGC settings
            if (config.IsUKGCWebsite)
            {
                if (!config.ProcessPdfDocuments)
                {
                    result.Warnings.Add("UKGC websites often publish regulatory content as PDFs. Consider enabling PDF processing.");
                }

                if (!config.ClassifyRegulatoryDocuments)
                {
                    result.Warnings.Add("Consider enabling document classification for better regulatory content monitoring.");
                }
            }

            // Validate PDF processing
            if (config.ProcessPdfDocuments)
            {
                // Check if the appropriate dependencies are likely available
                bool iTextSharpAvailable = false;
                try
                {
                    var iTextAssemblyName = typeof(iText.Kernel.Pdf.PdfDocument).Assembly.GetName().Name;
                    iTextSharpAvailable = !string.IsNullOrEmpty(iTextAssemblyName);
                }
                catch (Exception)
                {
                    iTextSharpAvailable = false;
                }

                if (!iTextSharpAvailable)
                {
                    result.Warnings.Add("PDF processing is enabled but iTextSharp library may not be properly referenced.");
                }
            }

            // If regulatory content analysis is enabled, some parameters need to be properly set
            if (config.EnableRegulatoryContentAnalysis || config.TrackRegulatoryChanges)
            {
                if (config.MaxVersionsToKeep < 5)
                {
                    result.Warnings.Add("For regulatory compliance, consider keeping at least 5 versions of content to track historical changes.");
                }

                if (!config.MonitorHighImpactChanges)
                {
                    result.Warnings.Add("Consider enabling high-impact change monitoring for better regulatory compliance tracking.");
                }
            }
        }

        private void ValidateAdvancedSettings(ScraperConfig config, ValidationResult result)
        {
            // Validate adaptive crawling settings
            if (config.EnableAdaptiveCrawling)
            {
                if (config.PriorityQueueSize <= 0)
                {
                    result.Errors.Add("Priority queue size must be greater than zero when adaptive crawling is enabled.");
                }

                if (!config.AdjustDepthBasedOnQuality)
                {
                    result.Warnings.Add("Consider enabling depth adjustment based on content quality for better adaptive crawling results.");
                }
            }

            // Check for rare but problematic combinations
            if (config.EnableAdaptiveCrawling && config.MaxDepth < 3)
            {
                result.Warnings.Add("Adaptive crawling with very low max depth (<3) may not be effective. Consider increasing the depth.");
            }

            if (config.ClassifyRegulatoryDocuments && !config.ProcessPdfDocuments)
            {
                result.Warnings.Add("Document classification is enabled but PDF processing is disabled. Regulatory documents are often PDFs.");
            }

            // Check for potential memory issues
            int estimatedMemoryMB = EstimateMemoryUsage(config);
            if (estimatedMemoryMB > 1000) // 1GB
            {
                result.Warnings.Add($"Configuration may require substantial memory (~{estimatedMemoryMB}MB). Consider reducing concurrent requests or max depth.");
            }
        }

        private bool RobotsTxtDisallowsAccess(string robotsTxt, string url)
        {
            // Basic robots.txt parsing - more sophisticated solutions would use a library
            if (string.IsNullOrWhiteSpace(robotsTxt) || string.IsNullOrWhiteSpace(url))
            {
                return false;
            }

            Uri uri = new Uri(url);
            string path = uri.AbsolutePath;
            
            bool userAgentMatched = false;
            bool allUserAgentsSection = false;
            
            foreach (var line in robotsTxt.Split('\n'))
            {
                string trimmedLine = line.Trim().ToLowerInvariant();
                
                if (trimmedLine.StartsWith("user-agent:"))
                {
                    string agent = trimmedLine.Substring("user-agent:".Length).Trim();
                    
                    if (agent == "*")
                    {
                        allUserAgentsSection = true;
                        userAgentMatched = true;
                    }
                    else
                    {
                        allUserAgentsSection = false;
                        userAgentMatched = false; // Reset for specific user agent
                    }
                }
                else if (userAgentMatched || allUserAgentsSection)
                {
                    if (trimmedLine.StartsWith("disallow:"))
                    {
                        string disallowedPath = trimmedLine.Substring("disallow:".Length).Trim();
                        
                        if (!string.IsNullOrEmpty(disallowedPath))
                        {
                            if (disallowedPath == "/")
                            {
                                return true; // Everything is disallowed
                            }
                            
                            if (disallowedPath.EndsWith("*"))
                            {
                                string prefix = disallowedPath.TrimEnd('*');
                                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                            else if (path.Equals(disallowedPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return true;
                            }
                        }
                    }
                    else if (trimmedLine.StartsWith("allow:"))
                    {
                        // Allow takes precedence over disallow for the same path
                        string allowedPath = trimmedLine.Substring("allow:".Length).Trim();
                        
                        if (!string.IsNullOrEmpty(allowedPath))
                        {
                            if (allowedPath.EndsWith("*"))
                            {
                                string prefix = allowedPath.TrimEnd('*');
                                if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                                {
                                    return false;
                                }
                            }
                            else if (path.Equals(allowedPath, StringComparison.OrdinalIgnoreCase))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        private int EstimateMemoryUsage(ScraperConfig config)
        {
            // This is a rough estimate based on typical usage patterns
            int baseMemory = 100; // Base memory in MB
            
            // Adjust for concurrent requests
            baseMemory += config.MaxConcurrentRequests * 20;
            
            // Adjust for depth (exponential memory growth with depth)
            baseMemory += (int)(Math.Pow(2, Math.Min(config.MaxDepth, 10)) * 5);
            
            // Adjust for version tracking
            if (config.TrackContentVersions)
            {
                baseMemory += config.MaxVersionsToKeep * 30;
            }
            
            // Adjust for PDF processing
            if (config.ProcessPdfDocuments)
            {
                baseMemory += 200;
            }
            
            // Adjust for headless browser if likely to be used
            if (config.IsUKGCWebsite || config.EnableRegulatoryContentAnalysis)
            {
                baseMemory += 300; // Headless browsers use a lot of memory
            }
            
            return baseMemory;
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }
            
            // Basic email validation
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Result of configuration validation
    /// </summary>
    public class ValidationResult
    {
        /// <summary>
        /// Whether the configuration is valid and can be run without issues
        /// </summary>
        public bool IsValid { get; set; }
        
        /// <summary>
        /// Whether the configuration can run with warnings (no errors)
        /// </summary>
        public bool CanRunWithWarnings { get; set; }
        
        /// <summary>
        /// List of errors that prevent the configuration from running
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();
        
        /// <summary>
        /// List of warnings that don't prevent the configuration from running but may cause issues
        /// </summary>
        public List<string> Warnings { get; set; } = new List<string>();
    }
}