using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebScraper.ContentChange
{
    public enum ChangeType
    {
        None,
        Minor,
        Moderate,
        Major
    }

    public class PageVersion
    {
        public string Url { get; set; }
        public string Content { get; set; }
        public string TextContent { get; set; }
        public string Hash { get; set; }
        public DateTime VersionDate { get; set; }
        public ChangeType ChangeFromPrevious { get; set; }
        public Dictionary<string, string> ChangedSections { get; set; } = new Dictionary<string, string>();
        public string ScraperId { get; set; } // Track which scraper created this version
    }

    public class ChangeNotification
    {
        public string Url { get; set; }
        public string ScraperId { get; set; }
        public string ScraperName { get; set; }
        public ChangeType ChangeType { get; set; }
        public DateTime DetectedAt { get; set; }
        public Dictionary<string, string> ChangedSections { get; set; }
        public string Summary { get; set; }
    }

    public class ContentChangeDetector
    {
        private readonly Dictionary<string, Dictionary<string, List<PageVersion>>> _scraperPageVersions = new Dictionary<string, Dictionary<string, List<PageVersion>>>();
        private readonly Dictionary<string, ScraperContentSettings> _scraperSettings = new Dictionary<string, ScraperContentSettings>();
        private readonly Action<string> _logger;
        private readonly string _dataStoragePath;
        private List<ChangeNotification> _pendingNotifications = new List<ChangeNotification>();

        public ContentChangeDetector(string dataStoragePath = null, Action<string> logger = null)
        {
            _dataStoragePath = dataStoragePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentVersions");
            _logger = logger ?? (_ => {}); // Default no-op logger if none provided
            
            // Ensure storage directory exists
            if (!Directory.Exists(_dataStoragePath))
            {
                Directory.CreateDirectory(_dataStoragePath);
            }
        }

        public class ScraperContentSettings
        {
            public string ScraperId { get; set; }
            public string ScraperName { get; set; }
            public int MaxVersionsToKeep { get; set; } = 5;
            public bool TrackChangesHistory { get; set; } = true;
            public bool NotifyOnChanges { get; set; } = false;
            public string NotificationEmail { get; set; }
        }

        public void RegisterScraper(string scraperId, string scraperName, int maxVersionsToKeep = 5, bool trackChangesHistory = true, bool notifyOnChanges = false, string notificationEmail = null)
        {
            _scraperSettings[scraperId] = new ScraperContentSettings
            {
                ScraperId = scraperId,
                ScraperName = scraperName,
                MaxVersionsToKeep = maxVersionsToKeep,
                TrackChangesHistory = trackChangesHistory,
                NotifyOnChanges = notifyOnChanges,
                NotificationEmail = notificationEmail
            };
            
            if (!_scraperPageVersions.ContainsKey(scraperId))
            {
                _scraperPageVersions[scraperId] = new Dictionary<string, List<PageVersion>>();
                
                // Try to load existing data for this scraper
                LoadVersionHistory(scraperId);
            }
            
            _logger($"Registered scraper {scraperId} ({scraperName}) with max {maxVersionsToKeep} versions");
        }

        public void UpdateScraperSettings(string scraperId, int? maxVersionsToKeep = null, bool? trackChangesHistory = null, bool? notifyOnChanges = null, string notificationEmail = null)
        {
            if (!_scraperSettings.ContainsKey(scraperId))
            {
                _logger($"Cannot update settings for unknown scraper: {scraperId}");
                return;
            }
            
            var settings = _scraperSettings[scraperId];
            
            if (maxVersionsToKeep.HasValue)
                settings.MaxVersionsToKeep = maxVersionsToKeep.Value;
                
            if (trackChangesHistory.HasValue)
                settings.TrackChangesHistory = trackChangesHistory.Value;
                
            if (notifyOnChanges.HasValue)
                settings.NotifyOnChanges = notifyOnChanges.Value;
                
            if (notificationEmail != null)
                settings.NotificationEmail = notificationEmail;
                
            _logger($"Updated settings for scraper {scraperId}");
        }

        public void LoadVersionHistory(string scraperId)
        {
            try
            {
                var filePath = Path.Combine(_dataStoragePath, $"{scraperId}_versions.json");
                
                if (File.Exists(filePath))
                {
                    var json = File.ReadAllText(filePath);
                    var history = JsonConvert.DeserializeObject<Dictionary<string, List<PageVersion>>>(json);
                    
                    if (history != null)
                    {
                        _scraperPageVersions[scraperId] = history;
                        _logger($"Loaded version history for scraper {scraperId} with {history.Count} URLs");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error loading version history for scraper {scraperId}: {ex.Message}");
            }
        }

        public void SaveVersionHistory(string scraperId)
        {
            if (!_scraperSettings.TryGetValue(scraperId, out var settings) || !settings.TrackChangesHistory)
            {
                return; // Don't save if tracking is disabled
            }
            
            try
            {
                if (_scraperPageVersions.TryGetValue(scraperId, out var versions))
                {
                    var filePath = Path.Combine(_dataStoragePath, $"{scraperId}_versions.json");
                    var json = JsonConvert.SerializeObject(versions);
                    File.WriteAllText(filePath, json);
                    _logger($"Saved version history for scraper {scraperId} with {versions.Count} URLs");
                }
            }
            catch (Exception ex)
            {
                _logger($"Error saving version history for scraper {scraperId}: {ex.Message}");
            }
        }

        public void LoadVersionHistory(Dictionary<string, List<PageVersion>> history, string scraperId = null)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                scraperId = "default";
            }
            
            if (!_scraperPageVersions.ContainsKey(scraperId))
            {
                _scraperPageVersions[scraperId] = new Dictionary<string, List<PageVersion>>();
            }
            
            foreach (var entry in history)
            {
                _scraperPageVersions[scraperId][entry.Key] = entry.Value;
            }
            
            _logger($"Loaded version history for scraper {scraperId} with {history.Count} URLs");
        }

        public PageVersion TrackPageVersion(string url, string content, string textContent, string scraperId = null)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                scraperId = "default";
            }
            
            // Ensure we have settings for this scraper
            if (!_scraperSettings.ContainsKey(scraperId))
            {
                RegisterScraper(scraperId, "Unknown Scraper");
            }
            
            // Ensure we have a dictionary for this scraper
            if (!_scraperPageVersions.ContainsKey(scraperId))
            {
                _scraperPageVersions[scraperId] = new Dictionary<string, List<PageVersion>>();
            }
            
            var scraperVersions = _scraperPageVersions[scraperId];
            var settings = _scraperSettings[scraperId];
            var maxVersions = settings.MaxVersionsToKeep;
            
            var hash = ComputeHash(content);

            // Create new version
            var newVersion = new PageVersion
            {
                Url = url,
                Content = content,
                TextContent = textContent,
                Hash = hash,
                VersionDate = DateTime.Now,
                ChangeFromPrevious = ChangeType.None,
                ScraperId = scraperId
            };

            // Check if we already have versions for this URL
            if (scraperVersions.TryGetValue(url, out var versions))
            {
                var latestVersion = versions.OrderByDescending(v => v.VersionDate).FirstOrDefault();

                if (latestVersion != null && latestVersion.Hash != hash)
                {
                    // Analyze what changed
                    var changeType = AnalyzeChanges(latestVersion.TextContent, textContent);
                    newVersion.ChangeFromPrevious = changeType;
                    
                    _logger($"Detected {changeType} change for {url} [Scraper: {scraperId}]");

                    // If significant change, extract the changed sections
                    if (changeType > ChangeType.Minor)
                    {
                        newVersion.ChangedSections = ExtractChangedSections(latestVersion.TextContent, textContent);
                        _logger($"Extracted {newVersion.ChangedSections.Count} changed sections");
                        
                        // Create notification if needed
                        if (settings.NotifyOnChanges && !string.IsNullOrEmpty(settings.NotificationEmail))
                        {
                            CreateChangeNotification(newVersion, settings);
                        }
                    }

                    // Add new version
                    versions.Add(newVersion);

                    // Keep only the latest N versions
                    if (versions.Count > maxVersions)
                    {
                        versions = versions.OrderByDescending(v => v.VersionDate)
                            .Take(maxVersions)
                            .ToList();
                        scraperVersions[url] = versions;
                        _logger($"Pruned version history for {url} to {maxVersions} versions");
                    }
                    
                    // Save changes if tracking is enabled
                    if (settings.TrackChangesHistory)
                    {
                        SaveVersionHistory(scraperId);
                    }

                    return newVersion;
                }

                // No change, just return the latest version
                _logger($"No content change detected for {url} [Scraper: {scraperId}]");
                return latestVersion;
            }
            else
            {
                // First time seeing this URL
                scraperVersions[url] = new List<PageVersion> { newVersion };
                _logger($"Created first version for {url} [Scraper: {scraperId}]");
                
                // Save the new version
                if (settings.TrackChangesHistory)
                {
                    SaveVersionHistory(scraperId);
                }
                
                return newVersion;
            }
        }
        
        /// <summary>
        /// Gets the previous version of a URL's content
        /// </summary>
        public PageVersion GetPreviousVersion(string url, string scraperId = null)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                scraperId = "default";
            }
            
            if (_scraperPageVersions.TryGetValue(scraperId, out var versions))
            {
                if (versions.TryGetValue(url, out var urlVersions) && urlVersions.Count > 1)
                {
                    return urlVersions.OrderByDescending(v => v.VersionDate).Skip(1).FirstOrDefault();
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Detects significant changes between two text contents
        /// </summary>
        public SignificantChangesResult DetectSignificantChanges(string oldContent, string newContent, string url = null)
        {
            if (string.IsNullOrEmpty(oldContent) || string.IsNullOrEmpty(newContent))
            {
                _logger("Cannot detect changes with empty content");
                return new SignificantChangesResult
                {
                    HasSignificantChanges = false,
                    Summary = "Cannot compare empty content"
                };
            }
            
            try
            {
                var result = new SignificantChangesResult();
                result.DetectedAt = DateTime.Now;
                result.CurrentVersionDate = DateTime.Now;
                result.PreviousVersionDate = DateTime.Now.AddDays(-1); // Default assumption
                
                // Determine if hash changed
                var oldHash = ComputeHash(oldContent);
                var newHash = ComputeHash(newContent);
                result.HashChanged = oldHash != newHash;
                
                if (!result.HashChanged)
                {
                    return result; // No changes detected
                }
                
                // Analyze the type of change
                result.ChangeType = AnalyzeChanges(oldContent, newContent);
                result.HasSignificantChanges = result.ChangeType >= ChangeType.Moderate;
                
                // Extract changed sections
                result.ChangedSections = ExtractChangedSections(oldContent, newContent);
                
                // Calculate change percentage
                var totalWords = CountWords(newContent);
                var changedWords = 0;
                
                if (result.ChangedSections.TryGetValue("Added", out var added))
                {
                    changedWords += CountWords(added);
                }
                
                if (result.ChangedSections.TryGetValue("Removed", out var removed))
                {
                    changedWords += CountWords(removed);
                }
                
                result.ChangePercentage = totalWords > 0 ? (changedWords * 100.0 / totalWords) : 0;
                
                // Extract changed sentences
                result.ChangedSentences = ExtractChangedSentences(oldContent, newContent);
                
                // Determine importance based on change type
                switch (result.ChangeType)
                {
                    case ChangeType.Major:
                        result.Importance = RegulatoryChangeImportance.High;
                        break;
                    case ChangeType.Moderate:
                        result.Importance = RegulatoryChangeImportance.Medium;
                        break;
                    default:
                        result.Importance = RegulatoryChangeImportance.Low;
                        break;
                }
                
                // Generate a summary
                result.Summary = GenerateChangeSummary(url, result);
                
                return result;
            }
            catch (Exception ex)
            {
                _logger($"Error detecting significant changes: {ex.Message}");
                return new SignificantChangesResult
                {
                    HasSignificantChanges = false,
                    Summary = $"Error analyzing changes: {ex.Message}"
                };
            }
        }
        
        private int CountWords(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;
                
            return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        
        private string GenerateChangeSummary(string url, SignificantChangesResult result)
        {
            var summary = new StringBuilder();
            
            if (string.IsNullOrEmpty(url))
            {
                summary.AppendLine($"Content change detected (Change Level: {result.ChangeType})");
            }
            else
            {
                summary.AppendLine($"Content change detected for {url} (Change Level: {result.ChangeType})");
            }
            
            summary.AppendLine($"Change Percentage: {result.ChangePercentage:F1}%");
            summary.AppendLine($"Importance: {result.Importance}");
            summary.AppendLine();
            
            if (result.ChangedSentences.Count > 0)
            {
                summary.AppendLine($"Found {result.ChangedSentences.Count} changed sentences:");
                int count = Math.Min(result.ChangedSentences.Count, 3);
                for (int i = 0; i < count; i++)
                {
                    var cs = result.ChangedSentences[i];
                    summary.AppendLine($"- {cs.ChangeType}: {TruncateContent(cs.After ?? cs.Before, 100)}");
                }
            }
            
            return summary.ToString();
        }
        
        private string TruncateContent(string content, int maxLength)
        {
            if (content.Length <= maxLength)
                return content;
                
            return content.Substring(0, maxLength) + "...";
        }

        public ChangeType AnalyzeChanges(string oldContent, string newContent)
        {
            if (string.IsNullOrEmpty(oldContent) || string.IsNullOrEmpty(newContent))
                return ChangeType.Major;

            // Simple analysis based on text differences
            var oldParagraphs = SplitIntoParagraphs(oldContent);
            var newParagraphs = SplitIntoParagraphs(newContent);

            // Calculate similarity ratio
            int commonParagraphs = 0;
            foreach (var oldPara in oldParagraphs)
            {
                if (newParagraphs.Any(p => p.Equals(oldPara, StringComparison.OrdinalIgnoreCase)))
                {
                    commonParagraphs++;
                }
            }

            double similarityRatio = 0;
            if (oldParagraphs.Count > 0 && newParagraphs.Count > 0)
            {
                similarityRatio = (double)commonParagraphs / Math.Max(oldParagraphs.Count, newParagraphs.Count);
            }

            // Determine change type based on similarity
            if (similarityRatio > 0.9) return ChangeType.Minor;
            if (similarityRatio > 0.7) return ChangeType.Moderate;
            return ChangeType.Major;
        }

        public Dictionary<string, string> ExtractChangedSections(string oldContent, string newContent)
        {
            var result = new Dictionary<string, string>();

            var oldParagraphs = SplitIntoParagraphs(oldContent);
            var newParagraphs = SplitIntoParagraphs(newContent);

            // Find new paragraphs
            var addedParagraphs = newParagraphs.Where(p =>
                !oldParagraphs.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();

            // Find removed paragraphs
            var removedParagraphs = oldParagraphs.Where(p =>
                !newParagraphs.Contains(p, StringComparer.OrdinalIgnoreCase)).ToList();

            if (addedParagraphs.Any())
            {
                result["Added"] = string.Join("\n\n", addedParagraphs);
            }

            if (removedParagraphs.Any())
            {
                result["Removed"] = string.Join("\n\n", removedParagraphs);
            }

            return result;
        }

        private List<string> SplitIntoParagraphs(string content)
        {
            return content.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToList();
        }

        public Dictionary<string, List<PageVersion>> GetVersionHistory(string scraperId = null)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                scraperId = "default";
            }
            
            if (_scraperPageVersions.TryGetValue(scraperId, out var versions))
            {
                return versions;
            }
            
            return new Dictionary<string, List<PageVersion>>();
        }
        
        public List<ChangeNotification> GetPendingNotifications()
        {
            var notifications = _pendingNotifications.ToList();
            _pendingNotifications.Clear();
            return notifications;
        }

        private string ComputeHash(string content)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var contentBytes = Encoding.UTF8.GetBytes(content);
                var hashBytes = sha.ComputeHash(contentBytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private void CreateChangeNotification(PageVersion version, ScraperContentSettings settings)
        {
            var notification = new ChangeNotification
            {
                Url = version.Url,
                ScraperId = settings.ScraperId,
                ScraperName = settings.ScraperName,
                ChangeType = version.ChangeFromPrevious,
                DetectedAt = DateTime.Now,
                ChangedSections = version.ChangedSections,
                Summary = GenerateChangeSummary(version)
            };
            
            _pendingNotifications.Add(notification);
            _logger($"Created change notification for {version.Url} ({version.ChangeFromPrevious})");
        }
        
        private string GenerateChangeSummary(PageVersion version)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"Content change detected for: {version.Url}");
            summary.AppendLine($"Change level: {version.ChangeFromPrevious}");
            summary.AppendLine($"Detected at: {version.VersionDate}");
            summary.AppendLine();
            
            if (version.ChangedSections.TryGetValue("Added", out var addedContent))
            {
                summary.AppendLine("ADDED CONTENT:");
                summary.AppendLine(TruncateContent(addedContent, 500));
                summary.AppendLine();
            }
            
            if (version.ChangedSections.TryGetValue("Removed", out var removedContent))
            {
                summary.AppendLine("REMOVED CONTENT:");
                summary.AppendLine(TruncateContent(removedContent, 500));
            }
            
            return summary.ToString();
        }
        
        private string TruncateContent(string content, int maxLength)
        {
            if (content.Length <= maxLength)
                return content;
                
            return content.Substring(0, maxLength) + "...";
        }
    }
}
