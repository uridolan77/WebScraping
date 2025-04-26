using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
    }

    public class ContentChangeDetector
    {
        private readonly Dictionary<string, List<PageVersion>> _pageVersions = new Dictionary<string, List<PageVersion>>();
        private readonly int _maxVersionsToKeep;
        private readonly Action<string> _logger;

        public ContentChangeDetector(int maxVersionsToKeep = 5, Action<string> logger = null)
        {
            _maxVersionsToKeep = maxVersionsToKeep;
            _logger = logger ?? (_ => {}); // Default no-op logger if none provided
        }

        public void LoadVersionHistory(Dictionary<string, List<PageVersion>> history)
        {
            foreach (var entry in history)
            {
                _pageVersions[entry.Key] = entry.Value;
            }
            _logger($"Loaded version history for {history.Count} URLs");
        }

        public PageVersion TrackPageVersion(string url, string content, string textContent)
        {
            var hash = ComputeHash(content);

            // Create new version
            var newVersion = new PageVersion
            {
                Url = url,
                Content = content,
                TextContent = textContent,
                Hash = hash,
                VersionDate = DateTime.Now,
                ChangeFromPrevious = ChangeType.None
            };

            // Check if we already have versions for this URL
            if (_pageVersions.TryGetValue(url, out var versions))
            {
                var latestVersion = versions.OrderByDescending(v => v.VersionDate).FirstOrDefault();

                if (latestVersion != null && latestVersion.Hash != hash)
                {
                    // Analyze what changed
                    var changeType = AnalyzeChanges(latestVersion.TextContent, textContent);
                    newVersion.ChangeFromPrevious = changeType;
                    
                    _logger($"Detected {changeType} change for {url}");

                    // If significant change, extract the changed sections
                    if (changeType > ChangeType.Minor)
                    {
                        newVersion.ChangedSections = ExtractChangedSections(latestVersion.TextContent, textContent);
                        _logger($"Extracted {newVersion.ChangedSections.Count} changed sections");
                    }

                    // Add new version
                    versions.Add(newVersion);

                    // Keep only the latest N versions
                    if (versions.Count > _maxVersionsToKeep)
                    {
                        versions = versions.OrderByDescending(v => v.VersionDate)
                            .Take(_maxVersionsToKeep)
                            .ToList();
                        _pageVersions[url] = versions;
                        _logger($"Pruned version history for {url} to {_maxVersionsToKeep} versions");
                    }

                    return newVersion;
                }

                // No change, just return the latest version
                _logger($"No content change detected for {url}");
                return latestVersion;
            }
            else
            {
                // First time seeing this URL
                _pageVersions[url] = new List<PageVersion> { newVersion };
                _logger($"Created first version for {url}");
                return newVersion;
            }
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

        public Dictionary<string, List<PageVersion>> GetVersionHistory()
        {
            return _pageVersions;
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
    }
}
