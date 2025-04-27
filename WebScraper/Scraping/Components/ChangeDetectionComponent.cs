using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.ContentChange;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that handles content change detection
    /// </summary>
    public class ChangeDetectionComponent : ScraperComponentBase, IChangeDetector
    {
        private ContentChangeDetector _changeDetector;
        private bool _changeDetectionEnabled;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            _changeDetectionEnabled = Config.EnableChangeDetection;
            if (!_changeDetectionEnabled)
            {
                LogInfo("Change detection not enabled, component will be inactive");
                return;
            }
            
            await InitializeChangeDetectorAsync();
        }
        
        /// <summary>
        /// Initializes the change detector
        /// </summary>
        private async Task InitializeChangeDetectorAsync()
        {
            try
            {
                LogInfo("Initializing content change detector...");
                
                // Create the change detector
                _changeDetector = new ContentChangeDetector(Config.OutputDirectory, LogInfo);
                
                // Register scraper with change detector
                _changeDetector.RegisterScraper(
                    Config.Name ?? "default",
                    Config.ScraperName ?? "DefaultScraper",
                    Config.MaxVersionsToKeep,
                    Config.TrackContentVersions,
                    Config.NotifyOnChanges,
                    Config.NotificationEmail
                );
                
                // Load previous version history
                await LoadVersionHistoryAsync();
                
                LogInfo("Content change detector initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize content change detector");
            }
        }
        
        /// <summary>
        /// Loads the version history from storage
        /// </summary>
        private async Task LoadVersionHistoryAsync()
        {
            try
            {
                var versionHistoryPath = Path.Combine(Config.OutputDirectory, "version_history.json");
                
                if (File.Exists(versionHistoryPath))
                {
                    var json = await File.ReadAllTextAsync(versionHistoryPath);
                    var history = JsonSerializer.Deserialize<Dictionary<string, List<PageVersion>>>(
                        json, 
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    
                    if (history != null)
                    {
                        _changeDetector.LoadVersionHistory(history);
                        LogInfo($"Loaded version history for {history.Count} pages");
                    }
                }
                else
                {
                    LogInfo("No version history found, starting fresh");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error loading version history");
            }
        }
        
        /// <summary>
        /// Saves the version history to storage
        /// </summary>
        private async Task SaveVersionHistoryAsync()
        {
            if (_changeDetector == null)
                return;
                
            try
            {
                var filePath = Path.Combine(Config.OutputDirectory, "version_history.json");
                
                // Get version history from change detector and convert to JSON
                var versionHistory = _changeDetector.GetVersionHistory();
                var json = JsonSerializer.Serialize(
                    versionHistory, 
                    new JsonSerializerOptions { WriteIndented = true }
                );
                
                // Write to file
                await File.WriteAllTextAsync(filePath, json);
                
                LogInfo($"Saved version history to {filePath}");
            }
            catch (Exception ex)
            {
                LogError(ex, "Error saving version history");
            }
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override async Task OnScrapingCompletedAsync()
        {
            if (_changeDetectionEnabled && _changeDetector != null)
            {
                await SaveVersionHistoryAsync();
            }
            
            await base.OnScrapingCompletedAsync();
        }
        
        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        public override async Task OnScrapingStoppedAsync()
        {
            if (_changeDetectionEnabled && _changeDetector != null)
            {
                await SaveVersionHistoryAsync();
            }
            
            await base.OnScrapingStoppedAsync();
        }
        
        /// <summary>
        /// Tracks a new version of a page
        /// </summary>
        public async Task<object> TrackPageVersionAsync(string url, string content, string contentType)
        {
            if (!_changeDetectionEnabled || _changeDetector == null)
                return null;
                
            try
            {
                // Extract text content if needed
                string textContent = content;
                if (contentType.Contains("html"))
                {
                    var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                    htmlDoc.LoadHtml(content);
                    textContent = htmlDoc.DocumentNode.InnerText;
                }
                
                // Track the page version
                var pageVersion = _changeDetector.TrackPageVersion(url, content, textContent);
                
                // Handle significant changes detection
                var previousVersion = _changeDetector.GetPreviousVersion(url);
                if (previousVersion != null && pageVersion.ChangeFromPrevious != ChangeType.None)
                {
                    var changes = _changeDetector.DetectSignificantChanges(
                        previousVersion.TextContent, 
                        pageVersion.TextContent
                    );
                    
                    // Process significant changes if found
                    if (changes.HasSignificantChanges)
                    {
                        await HandleSignificantChangesAsync(url, changes);
                    }
                }
                
                return pageVersion;
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error tracking page version for {url}");
                return null;
            }
        }
        
        /// <summary>
        /// Handle significant changes
        /// </summary>
        private async Task HandleSignificantChangesAsync(string url, SignificantChangesResult changes)
        {
            // Log the changes
            LogInfo($"Significant changes detected at {url}: {changes.ChangedSentences.Count} changed sentences");
            
            // Get notification component if there are critical changes and notifications are enabled
            if (Config.NotifyOnChanges && changes.HasCriticalChanges())
            {
                // In a full implementation, we would get a notification component and send an alert
                // For now, we'll just log it
                LogInfo($"CRITICAL CHANGE ALERT for {url}: {changes.Summary}");
                
                // We could implement INotifier component in the future and use it like:
                // var notifier = GetComponent<INotifier>();
                // if (notifier != null)
                // {
                //     await notifier.SendNotificationAsync(url, changes);
                // }
            }
            
            // Wait for async operations to complete
            await Task.CompletedTask;
        }
    }
}