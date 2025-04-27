using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraper.StateManagement;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that manages state for the scraper
    /// </summary>
    public class StateManagerComponent : ScraperComponentBase, IStateManager
    {
        private PersistentStateManager _stateManager;
        private string _scraperInstanceId;
        private string _dbConnectionString;
        
        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            
            if (!Config.EnablePersistentState)
            {
                LogInfo("Persistent state management not enabled, component will operate in limited mode");
                return;
            }
            
            await InitializeStateManagerAsync();
        }
        
        /// <summary>
        /// Initializes the state manager
        /// </summary>
        private async Task InitializeStateManagerAsync()
        {
            try
            {
                LogInfo("Initializing persistent state manager...");
                
                // Ensure output directory exists
                if (!Directory.Exists(Config.OutputDirectory))
                {
                    Directory.CreateDirectory(Config.OutputDirectory);
                }
                
                // Create connection string
                _dbConnectionString = $"Data Source={Path.Combine(Config.OutputDirectory, "scraper_state.db")}";
                _scraperInstanceId = Config.Name ?? "default";
                
                // Create state manager
                _stateManager = new PersistentStateManager(_dbConnectionString, LogInfo);
                await _stateManager.InitializeAsync();
                
                // Save initial scraper state
                var initialState = new StateManagement.ScraperState
                {
                    ScraperId = _scraperInstanceId,
                    Status = "Initializing",
                    LastRunStartTime = DateTime.Now,
                    ProgressData = "{}",
                    ConfigSnapshot = JsonSerializer.Serialize(Config)
                };
                
                await _stateManager.SaveScraperStateAsync(initialState);
                
                LogInfo("Persistent state manager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize persistent state manager");
            }
        }
        
        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public override async Task OnScrapingStartedAsync()
        {
            await SaveStateAsync("Running");
        }
        
        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override async Task OnScrapingCompletedAsync()
        {
            await SaveStateAsync("Completed");
        }
        
        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        public override async Task OnScrapingStoppedAsync()
        {
            await SaveStateAsync("Stopped");
        }
        
        /// <summary>
        /// Saves the state of the scraper
        /// </summary>
        public async Task SaveStateAsync(string status)
        {
            if (_stateManager == null)
                return;
                
            try
            {
                var state = new StateManagement.ScraperState
                {
                    ScraperId = _scraperInstanceId,
                    Status = status,
                    UpdatedAt = DateTime.Now,
                    LastRunEndTime = status == "Running" ? (DateTime?)null : DateTime.Now,
                    LastSuccessfulRunTime = status == "Completed" ? DateTime.Now : (DateTime?)null,
                    ProgressData = "{}"
                };
                
                await _stateManager.SaveScraperStateAsync(state);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to save state: {status}");
            }
        }
        
        /// <summary>
        /// Marks a URL as visited
        /// </summary>
        public async Task MarkUrlVisitedAsync(string url, int statusCode)
        {
            if (_stateManager == null)
                return;
                
            try
            {
                await _stateManager.MarkUrlVisitedAsync(_scraperInstanceId, url, statusCode, 0);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to mark URL as visited: {url}");
            }
        }
        
        /// <summary>
        /// Checks if a URL has been visited
        /// </summary>
        public async Task<bool> HasUrlBeenVisitedAsync(string url)
        {
            if (_stateManager == null)
                return false;
                
            try
            {
                return await _stateManager.HasUrlBeenVisitedAsync(_scraperInstanceId, url);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to check if URL has been visited: {url}");
                return false;
            }
        }
        
        /// <summary>
        /// Saves content for a URL
        /// </summary>
        public async Task SaveContentAsync(string url, string content, string contentType)
        {
            if (_stateManager == null || !Config.StoreContentInDatabase)
                return;
                
            try
            {
                var item = new ContentItemImpl
                {
                    Url = url,
                    ContentType = contentType,
                    RawContent = content,
                    ScraperId = _scraperInstanceId,
                    LastStatusCode = 200,
                    IsReachable = true,
                    ContentHash = ComputeHash(content),
                    Title = ExtractTitle(content)
                };
                
                await _stateManager.SaveContentItemAsync(item);
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to save content for URL: {url}");
            }
        }
        
        /// <summary>
        /// Computes a hash for content
        /// </summary>
        private string ComputeHash(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
                
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(content);
                var hash = sha.ComputeHash(bytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }
        
        /// <summary>
        /// Extracts the title from HTML content
        /// </summary>
        private string ExtractTitle(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;
                
            try
            {
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content);
                
                var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                if (titleNode != null)
                {
                    return titleNode.InnerText.Trim();
                }
                
                return string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}