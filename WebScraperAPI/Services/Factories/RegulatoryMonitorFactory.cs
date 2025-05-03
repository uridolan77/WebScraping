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
            
            // Use a more generic approach that doesn't rely on specific property names
            public IRF.ChangeAnalysisResult AnalyzeChanges(string oldContent, string newContent)
            {
                // Create a new instance without assuming specific property names
                var result = Activator.CreateInstance<IRF.ChangeAnalysisResult>();
                
                try
                {
                    // Use reflection to set properties if they exist
                    Type resultType = result.GetType();
                    
                    // Calculate the change percentage
                    double changePercentage = 0;
                    if (string.IsNullOrEmpty(oldContent) || string.IsNullOrEmpty(newContent))
                    {
                        changePercentage = 100.0;
                    }
                    else
                    {
                        // Calculate a basic similarity metric
                        var lengthDiff = Math.Abs(oldContent.Length - newContent.Length);
                        var maxLength = Math.Max(oldContent.Length, newContent.Length);
                        
                        if (maxLength > 0)
                        {
                            changePercentage = ((double)lengthDiff / maxLength) * 100;
                        }
                    }
                    
                    // Try different possible property names for change percentage
                    TrySetProperty(resultType, result, "ChangePct", changePercentage);
                    TrySetProperty(resultType, result, "ChangePercentage", changePercentage);
                    TrySetProperty(resultType, result, "PercentChanged", changePercentage);
                    TrySetProperty(resultType, result, "ChangePercent", changePercentage);
                    
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error analyzing changes");
                    
                    // Try different possible property names for error message
                    TrySetProperty(result.GetType(), result, "ErrorMessage", ex.Message ?? "Unknown error");
                    TrySetProperty(result.GetType(), result, "Error", ex.Message ?? "Unknown error");
                    TrySetProperty(result.GetType(), result, "ErrorDetails", ex.Message ?? "Unknown error");
                    
                    return result;
                }
            }
            
            // Helper method to set a property if it exists
            private void TrySetProperty(Type type, object obj, string propertyName, object value)
            {
                try
                {
                    var property = type.GetProperty(propertyName);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(obj, value);
                    }
                }
                catch
                {
                    // Ignore any errors - the property doesn't exist or isn't writable
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
                        // Set the ChangeType using reflection instead of directly accessing enum values
                        TrySetEnumProperty(result, "ChangeType", "Addition", 1);
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
                        // Set the ChangeType using reflection instead of directly accessing enum values
                        TrySetEnumProperty(result, "ChangeType", "Removal", 2);
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
                        // Set the ChangeType using reflection instead of directly accessing enum values
                        TrySetEnumProperty(result, "ChangeType", "Modification", 3);
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
                        // Set the ChangeType using reflection instead of directly accessing enum values
                        TrySetEnumProperty(result, "ChangeType", "None", 0);
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
                        Summary = "Error detecting changes: " + ex.Message,
                        ChangedSections = new Dictionary<string, string>()
                    };
                    
                    // Set the ChangeType using reflection instead of directly accessing enum values
                    TrySetEnumProperty(result, "ChangeType", "Error", 4);
                    
                    return result;
                }
            }
            
            // Helper method to set an enum property by name or value
            private void TrySetEnumProperty(object obj, string propertyName, string enumValueName, int fallbackValue)
            {
                try
                {
                    var property = obj.GetType().GetProperty(propertyName);
                    if (property != null && property.CanWrite)
                    {
                        if (property.PropertyType.IsEnum)
                        {
                            // Try to use the enum value by name
                            try
                            {
                                var enumValue = Enum.Parse(property.PropertyType, enumValueName);
                                property.SetValue(obj, enumValue);
                                return;
                            }
                            catch
                            {
                                // If the enum name doesn't exist, try to use the integer value
                                try
                                {
                                    property.SetValue(obj, Enum.ToObject(property.PropertyType, fallbackValue));
                                }
                                catch
                                {
                                    // If all else fails, use the default value of the enum
                                    property.SetValue(obj, Activator.CreateInstance(property.PropertyType));
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Ignore any errors
                }
            }
            
            public async Task<IRF.PageVersion> TrackPageVersionAsync(string url, string currentContent, string contentType)
            {
                try
                {
                    // Create a new instance without making assumptions about specific properties
                    var version = Activator.CreateInstance<IRF.PageVersion>();
                    
                    // Set the URL property which definitely exists
                    version.Url = url ?? string.Empty;
                    
                    // Set text content which likely exists
                    version.TextContent = currentContent ?? string.Empty;
                    
                    // Use reflection to set other properties if they exist
                    TrySetProperty(version, "VersionNumber", 1);
                    TrySetProperty(version, "VersionId", Guid.NewGuid().ToString());
                    TrySetProperty(version, "VersionDate", DateTime.UtcNow);
                    TrySetProperty(version, "Timestamp", DateTime.UtcNow);
                    TrySetProperty(version, "ContentType", contentType);
                    
                    // Log the tracking without referencing potentially non-existent properties
                    _logger.LogInformation($"Tracking version for URL: {url}");
                    
                    // Add a small delay to make this truly async
                    await Task.Delay(1);
                    
                    return version;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error tracking page version for {url}");
                    
                    // Create a new instance for the error case
                    var errorVersion = Activator.CreateInstance<IRF.PageVersion>();
                    errorVersion.Url = url ?? string.Empty;
                    errorVersion.TextContent = $"Error: {ex.Message ?? "Unknown error"}";
                    
                    // Use reflection to set other properties if they exist
                    TrySetProperty(errorVersion, "VersionNumber", 0);
                    // Check for null before passing ex.Message
                    if (ex.Message != null)
                    {
                        TrySetProperty(errorVersion, "Error", ex.Message);
                        TrySetProperty(errorVersion, "ErrorMessage", ex.Message);
                    }
                    else
                    {
                        TrySetProperty(errorVersion, "Error", "Unknown error");
                        TrySetProperty(errorVersion, "ErrorMessage", "Unknown error");
                    }
                    
                    return errorVersion;
                }
            }
            
            // Helper method to set a property if it exists
            private void TrySetProperty(object obj, string propertyName, object value)
            {
                try
                {
                    var property = obj.GetType().GetProperty(propertyName);
                    if (property != null && property.CanWrite)
                    {
                        property.SetValue(obj, value);
                    }
                }
                catch
                {
                    // Ignore any errors - the property doesn't exist or isn't writable
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
                // Create new result object for each classification scenario
                // This avoids the issue with init-only properties
                if (string.IsNullOrEmpty(url) && string.IsNullOrEmpty(title) && document == null)
                {
                    return new IRF.ClassificationResult 
                    { 
                        Category = "Unknown",
                        Confidence = 0.0
                    };
                }
                
                // Check for typical regulatory content in the URL or title
                var urlLower = url?.ToLowerInvariant() ?? "";
                var titleLower = title?.ToLowerInvariant() ?? "";
                
                if (urlLower.Contains("regulation") || urlLower.Contains("compliance") || 
                    urlLower.Contains("policy") || urlLower.Contains("guideline") ||
                    titleLower.Contains("regulation") || titleLower.Contains("compliance") ||
                    titleLower.Contains("policy") || titleLower.Contains("guideline"))
                {
                    return new IRF.ClassificationResult 
                    { 
                        Category = "Regulatory",
                        Confidence = 0.8
                    };
                }
                
                if (urlLower.Contains("news") || urlLower.Contains("press") || 
                    urlLower.Contains("release") || urlLower.Contains("announcement") ||
                    titleLower.Contains("news") || titleLower.Contains("press") ||
                    titleLower.Contains("release") || titleLower.Contains("announcement"))
                {
                    return new IRF.ClassificationResult 
                    { 
                        Category = "News",
                        Confidence = 0.7
                    };
                }
                
                if (urlLower.Contains("about") || urlLower.Contains("contact") || 
                    urlLower.Contains("faq") || urlLower.Contains("help") ||
                    titleLower.Contains("about") || titleLower.Contains("contact") ||
                    titleLower.Contains("faq") || titleLower.Contains("help"))
                {
                    return new IRF.ClassificationResult 
                    { 
                        Category = "Information",
                        Confidence = 0.6
                    };
                }
                
                return new IRF.ClassificationResult 
                { 
                    Category = "General",
                    Confidence = 0.5
                };
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
            
            // Fix the return type to match the interface (Task instead of Task<bool>)
            // Also use the correct AlertImportance enum from WebScraper.RegulatoryFramework.Interfaces
            public async Task SendNotificationAsync(string recipient, string message, WebScraper.RegulatoryFramework.Interfaces.AlertImportance importance = WebScraper.RegulatoryFramework.Interfaces.AlertImportance.Medium)
            {
                try
                {
                    _logger.LogInformation($"Sending notification to {recipient}, Importance: {importance}, Message: {message}");
                    
                    // In a real implementation, you would send an email, SMS, or webhook notification
                    // For now, just log the notification
                    
                    // Add a small delay to make it truly async
                    await Task.Delay(1);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending notification to {recipient}");
                }
            }
        }
    }

    // Define the classes with properly initialized non-nullable properties
    public class ChangeAnalysisResult
    {
        public ChangeAnalysisResult()
        {
            // Initialize non-nullable properties
            ErrorMessage = string.Empty;
            Metrics = new Dictionary<string, double>();
        }
        
        public double ChangePercentage { get; set; }
        public AnalysisStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, double> Metrics { get; set; }
    }

    public class ClassificationResult
    {
        public ClassificationResult()
        {
            // Initialize non-nullable properties
            PrimaryCategory = string.Empty;
            Tags = new List<string>();
            CategoryScores = new Dictionary<string, double>();
        }
        
        public string PrimaryCategory { get; set; }
        public List<string> Tags { get; set; }
        public double Confidence { get; set; }
        public bool IsRegulatoryContent { get; set; }
        public Dictionary<string, double> CategoryScores { get; set; }
    }

    // Define the enum that was missing
    public enum AnalysisStatus
    {
        NotStarted = 0,
        InProgress = 1,
        Complete = 2,
        Error = 3
    }
}