using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.RegulatoryFramework.Interfaces;
// Remove conflicting import that causes ambiguity
// using WebScraper.ContentChange;
using IRF = WebScraper.RegulatoryFramework.Interfaces; // Add alias for clarity

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating regulatory monitoring components
    /// </summary>
    public class RegulatoryMonitorFactory
    {
        private readonly ILogger<RegulatoryMonitorFactory> _logger;

        public RegulatoryMonitorFactory(ILogger<RegulatoryMonitorFactory> logger)
        {
            _logger = logger;
        }

        public IChangeDetector? CreateChangeDetector(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                if (!config.EnableChangeDetection)
                {
                    logAction("Change detection disabled, skipping change detector creation");
                    return null;
                }

                logAction("Creating default change detector");
                return new DefaultChangeDetector(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating change detector");
                logAction($"Error creating change detector: {ex.Message}");
                return null;
            }
        }

        public IContentClassifier? CreateContentClassifier(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                if (!config.ClassifyRegulatoryDocuments)
                {
                    logAction("Content classification disabled, skipping classifier creation");
                    return null;
                }

                logAction("Creating default content classifier");
                return new DefaultContentClassifier();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating content classifier");
                logAction($"Error creating content classifier: {ex.Message}");
                return null;
            }
        }

        public IAlertService? CreateAlertService(ScraperConfig config, Action<string> logAction)
        {
            try
            {
                if (!config.NotifyOnChanges)
                {
                    logAction("Notifications disabled, skipping alert service creation");
                    return null;
                }

                logAction("Creating default alert service");
                return new DefaultAlertService(_logger);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating alert service");
                logAction($"Error creating alert service: {ex.Message}");
                return null;
            }
        }

        // Simple implementations for the interfaces
        private class DefaultChangeDetector : IChangeDetector
        {
            private readonly ILogger _logger;

            public DefaultChangeDetector(ILogger logger)
            {
                _logger = logger;
            }
            
            public bool DetectChanges(string oldContent, string newContent)
            {
                // Simple implementation - just check if content lengths are different
                return oldContent?.Length != newContent?.Length;
            }
            
            public IRF.ChangeAnalysisResult AnalyzeChanges(string oldContent, string newContent)
            {
                var result = new IRF.ChangeAnalysisResult();
                
                try
                {
                    if (string.IsNullOrEmpty(oldContent) || string.IsNullOrEmpty(newContent))
                    {
                        result.ChangePercentage = 100.0;
                        result.Status = IRF.AnalysisStatus.Complete;
                        return result;
                    }
                    
                    // Calculate a basic similarity metric
                    var lengthDiff = Math.Abs(oldContent.Length - newContent.Length);
                    var maxLength = Math.Max(oldContent.Length, newContent.Length);
                    
                    if (maxLength > 0)
                    {
                        result.ChangePercentage = ((double)lengthDiff / maxLength) * 100;
                    }
                    else
                    {
                        result.ChangePercentage = 0.0;
                    }
                    
                    result.Status = IRF.AnalysisStatus.Complete;
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing changes");
                    result.Status = IRF.AnalysisStatus.Error;
                    result.ErrorMessage = ex.Message ?? "Unknown error";
                    return result;
                }
            }
            
            public IRF.SignificantChangesResult DetectSignificantChanges(string oldContent, string newContent)
            {
                try
                {
                    var result = new IRF.SignificantChangesResult();
                    
                    if (string.IsNullOrEmpty(oldContent) && !string.IsNullOrEmpty(newContent))
                    {
                        // New content added
                        result.HasSignificantChanges = true;
                        result.ChangeType = IRF.ChangeType.Addition;
                        result.Summary = "New content added";
                        result.ChangedSections = new Dictionary<string, string>
                        {
                            ["new"] = newContent.Length > 100 ? newContent.Substring(0, 100) + "..." : newContent
                        };
                    }
                    else if (!string.IsNullOrEmpty(oldContent) && string.IsNullOrEmpty(newContent))
                    {
                        // Content removed
                        result.HasSignificantChanges = true;
                        result.ChangeType = IRF.ChangeType.Removal;
                        result.Summary = "Content removed";
                        result.ChangedSections = new Dictionary<string, string>
                        {
                            ["removed"] = oldContent.Length > 100 ? oldContent.Substring(0, 100) + "..." : oldContent
                        };
                    }
                    else if (oldContent != newContent)
                    {
                        // Content modified
                        result.HasSignificantChanges = true;
                        result.ChangeType = IRF.ChangeType.Modification;
                        result.Summary = "Content has been modified";
                        
                        // In a real implementation, you would do a more sophisticated diff
                        // here to identify specific changes
                        result.ChangedSections = new Dictionary<string, string>
                        {
                            ["modification"] = "Content length before: " + oldContent?.Length + 
                                              ", Content length after: " + newContent?.Length
                        };
                    }
                    else
                    {
                        // No changes
                        result.HasSignificantChanges = false;
                        result.ChangeType = IRF.ChangeType.None;
                        result.Summary = "No changes detected";
                        result.ChangedSections = new Dictionary<string, string>();
                    }
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error detecting significant changes");
                    
                    var result = new IRF.SignificantChangesResult
                    {
                        HasSignificantChanges = false,
                        ChangeType = IRF.ChangeType.Error,
                        Summary = "Error detecting changes: " + ex.Message,
                        ChangedSections = new Dictionary<string, string>()
                    };
                    
                    return result;
                }
            }
            
            public async Task<IRF.PageVersion> TrackPageVersionAsync(string url, string currentContent, string contentType)
            {
                try
                {
                    // Simple implementation - create a new page version
                    var version = new IRF.PageVersion
                    {
                        Url = url ?? string.Empty,
                        Content = currentContent ?? string.Empty,
                        ContentType = contentType ?? "text/html",
                        CaptureDate = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString()
                    };
                    
                    // Log the tracking
                    _logger.LogInformation($"Tracking version for URL: {url}, Content Type: {contentType}, Version ID: {version.Id}");
                    
                    // Add a small delay to make this truly async
                    await Task.Delay(1);
                    
                    return version;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error tracking page version for {url}");
                    return new IRF.PageVersion
                    {
                        Url = url ?? string.Empty,
                        ContentType = contentType ?? "text/html",
                        Content = string.Empty,
                        CaptureDate = DateTime.UtcNow,
                        Id = Guid.NewGuid().ToString(),
                        Metadata = new Dictionary<string, string> { ["error"] = ex.Message ?? "Unknown error" }
                    };
                }
            }
        }

        private class DefaultContentClassifier : IContentClassifier
        {
            public string ClassifyContent(string content)
            {
                // Simple implementation - just return "Unknown"
                return "Unknown";
            }
            
            public IRF.ClassificationResult ClassifyContent(string url, string title, HtmlDocument document)
            {
                var result = new IRF.ClassificationResult
                {
                    PrimaryCategory = "Unknown",
                    Tags = new List<string>(),
                    Confidence = 0.0,
                    IsRegulatoryContent = false,
                    CategoryScores = new Dictionary<string, double>()
                };
                
                // Simple implementation - try to guess content type based on URL and title
                if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(title) && document == null)
                {
                    return result;
                }
                
                // Check for typical regulatory content in the URL or title
                var urlLower = url?.ToLowerInvariant() ?? "";
                var titleLower = title?.ToLowerInvariant() ?? "";
                
                if (urlLower.Contains("regulation") || urlLower.Contains("compliance") || 
                    urlLower.Contains("policy") || urlLower.Contains("guideline") ||
                    titleLower.Contains("regulation") || titleLower.Contains("compliance") ||
                    titleLower.Contains("policy") || titleLower.Contains("guideline"))
                {
                    result.PrimaryCategory = "Regulatory";
                    result.Confidence = 0.8;
                    result.IsRegulatoryContent = true;
                    result.Tags.Add("regulation");
                    return result;
                }
                
                if (urlLower.Contains("news") || urlLower.Contains("press") || 
                    urlLower.Contains("release") || urlLower.Contains("announcement") ||
                    titleLower.Contains("news") || titleLower.Contains("press") ||
                    titleLower.Contains("release") || titleLower.Contains("announcement"))
                {
                    result.PrimaryCategory = "News";
                    result.Confidence = 0.7;
                    return result;
                }
                
                if (urlLower.Contains("about") || urlLower.Contains("contact") || 
                    urlLower.Contains("faq") || urlLower.Contains("help") ||
                    titleLower.Contains("about") || titleLower.Contains("contact") ||
                    titleLower.Contains("faq") || titleLower.Contains("help"))
                {
                    result.PrimaryCategory = "Information";
                    result.Confidence = 0.6;
                    return result;
                }
                
                result.PrimaryCategory = "General";
                result.Confidence = 0.5;
                return result;
            }
        }

        private class DefaultAlertService : IAlertService
        {
            private readonly ILogger _logger;

            public DefaultAlertService(ILogger logger)
            {
                _logger = logger;
            }
            
            public void SendAlert(string message)
            {
                // Simple implementation - just log the alert
                _logger.LogInformation($"Alert: {message}");
            }
            
            public async Task ProcessAlertAsync(string url, IRF.SignificantChangesResult changeResult)
            {
                try
                {
                    if (changeResult?.HasSignificantChanges == true)
                    {
                        _logger.LogInformation($"Significant change detected for URL: {url}, Type: {changeResult.ChangeType}, Summary: {changeResult.Summary}");
                        
                        // In a real implementation, you would send notifications here
                        // For now, just log the alert
                        
                        foreach (var section in changeResult.ChangedSections)
                        {
                            _logger.LogInformation($"  Changed section '{section.Key}': {section.Value}");
                        }
                    }
                    
                    // Add a small delay to make this truly async
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error processing alert for {url}");
                }
            }
            
            // This method needs to match the interface exactly, which expects IRF.AlertImportance
            public async Task<bool> SendNotificationAsync(string recipient, string message, IRF.AlertImportance importance = IRF.AlertImportance.Normal)
            {
                try
                {
                    _logger.LogInformation($"Sending notification to {recipient}, Importance: {importance}, Message: {message}");
                    
                    // In a real implementation, you would send an email, SMS, or webhook notification
                    // For now, just log the notification
                    
                    // Add a small delay to make this truly async
                    await Task.Delay(1);
                    
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending notification to {recipient}");
                    return false;
                }
            }
        }
    }

    // Define enums and classes if they don't exist in your project
    public enum AlertImportance
    {
        Low,
        Normal,
        High,
        Critical
    }

    public enum AnalysisStatus
    {
        NotStarted,
        InProgress,
        Complete,
        Error
    }

    public class ChangeAnalysisResult
    {
        public double ChangePercentage { get; set; }
        public AnalysisStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
    }

    public class ClassificationResult
    {
        public string PrimaryCategory { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public double Confidence { get; set; }
        public bool IsRegulatoryContent { get; set; }
        public Dictionary<string, double> CategoryScores { get; set; } = new Dictionary<string, double>();
    }
}