using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WebScraper.ContentChange
{
    /// <summary>
    /// Represents the type of change detected in content
    /// </summary>
    public enum ChangeType
    {
        /// <summary>No change detected</summary>
        None,

        /// <summary>Minor changes (formatting, typos, etc.)</summary>
        Minor,

        /// <summary>Moderate changes (paragraph additions/removals)</summary>
        Moderate,

        /// <summary>Major changes (significant content restructuring)</summary>
        Major
    }

    /// <summary>
    /// Represents a version of a web page's content
    /// </summary>
    public record PageVersion
    {
        /// <summary>URL of the page</summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>Raw content of the page</summary>
        public string Content { get; init; } = string.Empty;

        /// <summary>Extracted text content</summary>
        public string TextContent { get; init; } = string.Empty;

        /// <summary>Hash of the content for quick comparison</summary>
        public string Hash { get; init; } = string.Empty;

        /// <summary>When this version was captured</summary>
        public DateTime VersionDate { get; init; } = DateTime.Now;

        /// <summary>Type of change from the previous version</summary>
        public ChangeType ChangeFromPrevious { get; set; } = ChangeType.None;

        /// <summary>Sections that changed from the previous version</summary>
        public Dictionary<string, string> ChangedSections { get; set; } = new();

        /// <summary>ID of the scraper that created this version</summary>
        public string ScraperId { get; init; } = string.Empty;
    }

    /// <summary>
    /// Represents a notification about content changes
    /// </summary>
    public record ChangeNotification
    {
        /// <summary>URL of the changed page</summary>
        public string Url { get; init; } = string.Empty;

        /// <summary>ID of the scraper that detected the change</summary>
        public string ScraperId { get; init; } = string.Empty;

        /// <summary>Name of the scraper that detected the change</summary>
        public string ScraperName { get; init; } = string.Empty;

        /// <summary>Type of change detected</summary>
        public ChangeType ChangeType { get; init; } = ChangeType.None;

        /// <summary>When the change was detected</summary>
        public DateTime DetectedAt { get; init; } = DateTime.Now;

        /// <summary>Sections that changed</summary>
        public Dictionary<string, string> ChangedSections { get; init; } = new();

        /// <summary>Human-readable summary of the changes</summary>
        public string Summary { get; init; } = string.Empty;
    }

    /// <summary>
    /// Detects and tracks changes in web page content over time
    /// </summary>
    public class ContentChangeDetector
    {
        // Dictionary structure: ScraperId -> URL -> List of versions
        private readonly Dictionary<string, Dictionary<string, List<PageVersion>>> _scraperPageVersions = new(StringComparer.OrdinalIgnoreCase);

        // Settings for each registered scraper
        private readonly Dictionary<string, ScraperContentSettings> _scraperSettings = new(StringComparer.OrdinalIgnoreCase);

        // Logger function
        private readonly Action<string> _logger;

        // Path to store version history files
        private readonly string _dataStoragePath;

        // Notifications waiting to be processed
        private readonly List<ChangeNotification> _pendingNotifications = new();

        /// <summary>
        /// Creates a new ContentChangeDetector
        /// </summary>
        /// <param name="dataStoragePath">Path to store version history files (optional)</param>
        /// <param name="logger">Logger function (optional)</param>
        public ContentChangeDetector(string dataStoragePath = null, Action<string> logger = null)
        {
            // Use null-coalescing operator for default path
            _dataStoragePath = dataStoragePath ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ContentVersions");

            // Use null-coalescing operator for default logger
            _logger = logger ?? (_ => { }); // Default no-op logger if none provided

            // Ensure storage directory exists
            try
            {
                if (!Directory.Exists(_dataStoragePath))
                {
                    Directory.CreateDirectory(_dataStoragePath);
                    _logger($"Created storage directory: {_dataStoragePath}");
                }
            }
            catch (Exception ex)
            {
                _logger($"Error creating storage directory: {ex.Message}");
                // Fall back to using the current directory
                _dataStoragePath = ".";
            }
        }

        /// <summary>
        /// Settings for a scraper's content change detection
        /// </summary>
        public record ScraperContentSettings
        {
            /// <summary>Unique identifier for the scraper</summary>
            public string ScraperId { get; init; } = string.Empty;

            /// <summary>Human-readable name for the scraper</summary>
            public string ScraperName { get; init; } = string.Empty;

            /// <summary>Maximum number of versions to keep per URL</summary>
            public int MaxVersionsToKeep { get; set; } = 5;

            /// <summary>Whether to track and persist version history</summary>
            public bool TrackChangesHistory { get; set; } = true;

            /// <summary>Whether to generate notifications for changes</summary>
            public bool NotifyOnChanges { get; set; } = false;

            /// <summary>Email address to send notifications to</summary>
            public string NotificationEmail { get; set; } = string.Empty;
        }

        /// <summary>
        /// Registers a scraper with the change detector
        /// </summary>
        /// <param name="scraperId">Unique identifier for the scraper</param>
        /// <param name="scraperName">Human-readable name for the scraper</param>
        /// <param name="maxVersionsToKeep">Maximum number of versions to keep per URL</param>
        /// <param name="trackChangesHistory">Whether to track and persist version history</param>
        /// <param name="notifyOnChanges">Whether to generate notifications for changes</param>
        /// <param name="notificationEmail">Email address to send notifications to</param>
        public void RegisterScraper(string scraperId, string scraperName, int maxVersionsToKeep = 5, bool trackChangesHistory = true, bool notifyOnChanges = false, string notificationEmail = null)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                throw new ArgumentException("Scraper ID cannot be null or empty", nameof(scraperId));
            }

            // Create settings for this scraper
            _scraperSettings[scraperId] = new ScraperContentSettings
            {
                ScraperId = scraperId,
                ScraperName = scraperName ?? "Unnamed Scraper",
                MaxVersionsToKeep = Math.Max(1, maxVersionsToKeep), // Ensure at least 1 version is kept
                TrackChangesHistory = trackChangesHistory,
                NotifyOnChanges = notifyOnChanges,
                NotificationEmail = notificationEmail ?? string.Empty
            };

            // Initialize version history dictionary if needed
            if (!_scraperPageVersions.TryGetValue(scraperId, out _))
            {
                _scraperPageVersions[scraperId] = new Dictionary<string, List<PageVersion>>(StringComparer.OrdinalIgnoreCase);

                // Try to load existing data for this scraper
                LoadVersionHistory(scraperId);
            }

            _logger($"Registered scraper {scraperId} ({scraperName}) with max {maxVersionsToKeep} versions");
        }

        /// <summary>
        /// Updates settings for a registered scraper
        /// </summary>
        /// <param name="scraperId">ID of the scraper to update</param>
        /// <param name="maxVersionsToKeep">New maximum versions to keep (optional)</param>
        /// <param name="trackChangesHistory">New tracking setting (optional)</param>
        /// <param name="notifyOnChanges">New notification setting (optional)</param>
        /// <param name="notificationEmail">New notification email (optional)</param>
        public void UpdateScraperSettings(string scraperId, int? maxVersionsToKeep = null, bool? trackChangesHistory = null, bool? notifyOnChanges = null, string notificationEmail = null)
        {
            // Use TryGetValue pattern instead of ContainsKey + indexer
            if (!_scraperSettings.TryGetValue(scraperId, out var settings))
            {
                _logger($"Cannot update settings for unknown scraper: {scraperId}");
                return;
            }

            // Update settings using null-coalescing assignment operator
            if (maxVersionsToKeep.HasValue)
                settings.MaxVersionsToKeep = Math.Max(1, maxVersionsToKeep.Value);

            if (trackChangesHistory.HasValue)
                settings.TrackChangesHistory = trackChangesHistory.Value;

            if (notifyOnChanges.HasValue)
                settings.NotifyOnChanges = notifyOnChanges.Value;

            // Only update email if provided
            settings.NotificationEmail = notificationEmail ?? settings.NotificationEmail;

            _logger($"Updated settings for scraper {scraperId}");
        }

        /// <summary>
        /// Loads version history for a scraper from disk
        /// </summary>
        /// <param name="scraperId">ID of the scraper to load history for</param>
        public void LoadVersionHistory(string scraperId)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                _logger("Cannot load version history for null or empty scraper ID");
                return;
            }

            var filePath = Path.Combine(_dataStoragePath, $"{scraperId}_versions.json");
            var backupPath = $"{filePath}.bak";

            if (!File.Exists(filePath))
            {
                // Check if a backup file exists
                if (File.Exists(backupPath))
                {
                    _logger($"Main history file not found, attempting to load from backup for {scraperId}");
                    filePath = backupPath;
                }
                else
                {
                    _logger($"No version history found for scraper {scraperId}");
                    return;
                }
            }

            try
            {
                // Read the file
                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                {
                    _logger($"Empty version history file for scraper {scraperId}");
                    return;
                }

                // Deserialize the JSON
                var history = JsonConvert.DeserializeObject<Dictionary<string, List<PageVersion>>>(json);

                if (history == null)
                {
                    _logger($"Failed to deserialize version history for scraper {scraperId}");
                    return;
                }

                // Store the history
                _scraperPageVersions[scraperId] = history;
                _logger($"Loaded version history for scraper {scraperId} with {history.Count} URLs");
            }
            catch (JsonException jsonEx)
            {
                _logger($"Error parsing version history for scraper {scraperId}: {jsonEx.Message}");

                // Try to load from backup if main file is corrupted
                if (filePath != backupPath && File.Exists(backupPath))
                {
                    _logger($"Attempting to load from backup file for {scraperId}");
                    try
                    {
                        var backupJson = File.ReadAllText(backupPath);
                        var backupHistory = JsonConvert.DeserializeObject<Dictionary<string, List<PageVersion>>>(backupJson);

                        if (backupHistory != null)
                        {
                            _scraperPageVersions[scraperId] = backupHistory;
                            _logger($"Successfully loaded backup version history for scraper {scraperId} with {backupHistory.Count} URLs");
                        }
                    }
                    catch (Exception backupEx)
                    {
                        _logger($"Error loading backup version history: {backupEx.Message}");
                    }
                }
            }
            catch (IOException ioEx)
            {
                _logger($"I/O error loading version history for scraper {scraperId}: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                _logger($"Unexpected error loading version history for scraper {scraperId}: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves version history for a scraper to disk
        /// </summary>
        /// <param name="scraperId">ID of the scraper to save history for</param>
        public void SaveVersionHistory(string scraperId)
        {
            // Check if tracking is enabled for this scraper
            if (!_scraperSettings.TryGetValue(scraperId, out var settings) || !settings.TrackChangesHistory)
            {
                _logger($"Version history tracking is disabled for scraper {scraperId}");
                return;
            }

            // Check if we have any versions to save
            if (!_scraperPageVersions.TryGetValue(scraperId, out var versions) || versions.Count == 0)
            {
                _logger($"No version history to save for scraper {scraperId}");
                return;
            }

            var filePath = Path.Combine(_dataStoragePath, $"{scraperId}_versions.json");
            var tempPath = $"{filePath}.tmp";

            try
            {
                // Serialize to JSON with indentation for readability
                var json = JsonConvert.SerializeObject(versions, Formatting.Indented);

                // Write to temporary file first to avoid corruption if process is interrupted
                File.WriteAllText(tempPath, json);

                // If a previous file exists, create a backup
                if (File.Exists(filePath))
                {
                    File.Copy(filePath, $"{filePath}.bak", true);
                }

                // Move the temporary file to the final location
                File.Move(tempPath, filePath, true);

                _logger($"Saved version history for scraper {scraperId} with {versions.Count} URLs");
            }
            catch (IOException ioEx)
            {
                _logger($"I/O error saving version history for scraper {scraperId}: {ioEx.Message}");

                // Try to clean up temporary file if it exists
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { /* Ignore cleanup errors */ }
                }
            }
            catch (JsonException jsonEx)
            {
                _logger($"Error serializing version history for scraper {scraperId}: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                _logger($"Unexpected error saving version history for scraper {scraperId}: {ex.Message}");
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

        private List<ChangedSentence> ExtractChangedSentences(string oldContent, string newContent)
        {
            var result = new List<ChangedSentence>();

            // Split content into sentences
            var oldSentences = SplitIntoSentences(oldContent);
            var newSentences = SplitIntoSentences(newContent);

            // Find added sentences
            foreach (var sentence in newSentences)
            {
                if (!oldSentences.Contains(sentence, StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(new ChangedSentence
                    {
                        After = sentence,
                        Before = null,
                        ChangeType = SentenceChangeType.Added,
                        Context = GetSentenceContext(newSentences, sentence),
                        Importance = 0.7
                    });
                }
            }

            // Find removed sentences
            foreach (var sentence in oldSentences)
            {
                if (!newSentences.Contains(sentence, StringComparer.OrdinalIgnoreCase))
                {
                    result.Add(new ChangedSentence
                    {
                        After = null,
                        Before = sentence,
                        ChangeType = SentenceChangeType.Removed,
                        Context = GetSentenceContext(oldSentences, sentence),
                        Importance = 0.8
                    });
                }
            }

            // Find modified sentences using similarity detection
            var potentialModifications = FindSimilarSentences(oldSentences, newSentences);
            foreach (var pair in potentialModifications)
            {
                result.Add(new ChangedSentence
                {
                    Before = pair.Item1,
                    After = pair.Item2,
                    ChangeType = SentenceChangeType.Modified,
                    Context = GetSentenceContext(newSentences, pair.Item2),
                    Importance = 0.9
                });
            }

            return result;
        }

        private List<string> SplitIntoSentences(string text)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            // Simple sentence splitting on common sentence terminators
            var sentences = text.Split(new[] { ". ", "! ", "? ", ".\r", "!\r", "?\r", ".\n", "!\n", "?\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s) && s.Length > 5)
                .ToList();

            // Add final period to last sentence if needed
            for (int i = 0; i < sentences.Count; i++)
            {
                var s = sentences[i];
                if (!s.EndsWith(".") && !s.EndsWith("!") && !s.EndsWith("?"))
                {
                    sentences[i] = s + ".";
                }
            }

            return sentences;
        }

        private string GetSentenceContext(List<string> sentences, string sentence)
        {
            int index = sentences.IndexOf(sentence);
            if (index < 0) return string.Empty;

            // Try to get one sentence before and after for context
            int start = Math.Max(0, index - 1);
            int end = Math.Min(sentences.Count - 1, index + 1);

            if (start == index && end == index)
                return "Section: Unknown";

            if (start == index)
                return "Beginning of section";

            if (end == index)
                return "End of section";

            return "Middle of section";
        }

        private List<Tuple<string, string>> FindSimilarSentences(List<string> oldSentences, List<string> newSentences)
        {
            var result = new List<Tuple<string, string>>();

            // Simple similarity detection based on word overlap
            foreach (var oldSentence in oldSentences)
            {
                if (newSentences.Contains(oldSentence, StringComparer.OrdinalIgnoreCase))
                    continue; // Exact match, not modified

                // Get words from old sentence
                var oldWords = oldSentence.Split(new[] { ' ', '\t', ',', ';', ':', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 3)
                    .ToList();

                if (oldWords.Count < 3)
                    continue; // Too short to reliably detect similarity

                foreach (var newSentence in newSentences)
                {
                    if (oldSentences.Contains(newSentence, StringComparer.OrdinalIgnoreCase))
                        continue; // Exact match with another old sentence

                    // Get words from new sentence
                    var newWords = newSentence.Split(new[] { ' ', '\t', ',', ';', ':', '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                        .Where(w => w.Length > 3)
                        .ToList();

                    if (newWords.Count < 3)
                        continue; // Too short

                    // Count common words
                    int commonWords = oldWords.Count(w => newWords.Contains(w, StringComparer.OrdinalIgnoreCase));
                    double similarity = (double)commonWords / Math.Max(oldWords.Count, newWords.Count);

                    // If similarity is high but not identical, consider it a modification
                    if (similarity > 0.5 && similarity < 0.95)
                    {
                        result.Add(new Tuple<string, string>(oldSentence, newSentence));
                        break; // Only match each old sentence to one new sentence at most
                    }
                }
            }

            return result;
        }
    }
}
