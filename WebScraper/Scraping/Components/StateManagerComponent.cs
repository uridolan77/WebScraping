using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace WebScraper.Scraping.Components
{
    /// <summary>
    /// Component that manages state for the scraper
    /// </summary>
    public class StateManagerComponent : ScraperComponentBase, IStateManager
    {
        private StateManagement.PersistentStateManager _stateManager;
        private string _scraperInstanceId;
        private string _dbConnectionString;
        private int _pagesProcessed = 0;
        private DateTime _startTime;
        private dynamic _repository; // This will hold the IScraperRepository if we can find it

        /// <summary>
        /// Initializes the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);
            _startTime = DateTime.Now;

            // Try to find an IScraperRepository in the Core
            TryFindRepository();

            if (!Config.EnablePersistentState)
            {
                LogInfo("Persistent state management not enabled, component will operate in limited mode");
                return;
            }

            await InitializeStateManagerAsync();
        }

        /// <summary>
        /// Tries to find a repository in the scraper components
        /// </summary>
        private void TryFindRepository()
        {
            try
            {
                LogInfo("Looking for IScraperRepository in scraper components...");

                // If we can access the components collection in the Core
                PropertyInfo componentsProperty = Core.GetType().GetProperty("Components", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
                if (componentsProperty != null)
                {
                    var components = componentsProperty.GetValue(Core) as System.Collections.IEnumerable;
                    if (components != null)
                    {
                        foreach (var component in components)
                        {
                            // Check if this component has a _repository field of type IScraperRepository
                            var fields = component.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
                            foreach (var field in fields)
                            {
                                if (field.Name == "_repository")
                                {
                                    var repo = field.GetValue(component);
                                    if (repo != null)
                                    {
                                        LogInfo($"Found repository in component {component.GetType().Name}");
                                        _repository = repo;
                                        break;
                                    }
                                }
                            }

                            if (_repository != null)
                                break;
                        }
                    }
                }

                if (_repository == null)
                {
                    LogWarning("Could not find IScraperRepository in scraper components");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Error trying to find repository");
            }
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
                    LogInfo($"Creating output directory: {Config.OutputDirectory}");
                    Directory.CreateDirectory(Config.OutputDirectory);
                }

                // Get scraper instance ID
                _scraperInstanceId = Config.Name ?? "default";
                LogInfo($"Using scraper instance ID: {_scraperInstanceId}");

                // Create a valid state directory
                var stateDirectory = Path.Combine(Config.OutputDirectory, "state");
                if (!Directory.Exists(stateDirectory))
                {
                    LogInfo($"Creating state directory: {stateDirectory}");
                    Directory.CreateDirectory(stateDirectory);
                }

                // Create state manager with the directory (not a connection string)
                LogInfo($"Creating state manager with directory: {stateDirectory}");
                _stateManager = new StateManagement.PersistentStateManager(stateDirectory, LogInfo);

                LogInfo("Initializing state manager...");
                await _stateManager.InitializeAsync();

                // Save initial scraper state
                LogInfo("Saving initial scraper state...");
                var initialState = new StateManagement.ScraperState
                {
                    ScraperId = _scraperInstanceId,
                    Status = "Initializing",
                    LastRunStartTime = DateTime.Now,
                    ProgressData = "{}",
                    ConfigSnapshot = JsonSerializer.Serialize(Config)
                };

                await _stateManager.SaveScraperStateAsync(initialState);

                // Initialize the scraper status in the database if repository is available
                if (_repository != null)
                {
                    try
                    {
                        LogInfo($"Initializing scraper status in database for ScraperId: {_scraperInstanceId}");

                        // Use reflection to call UpdateScraperStatusAsync on the repository
                        var updateMethod = _repository.GetType().GetMethod("UpdateScraperStatusAsync");
                        if (updateMethod != null)
                        {
                            // Create a new status object
                            // First try to get the ScraperStatusEntity type
                            var statusType = Type.GetType("WebScraperApi.Data.Entities.ScraperStatusEntity, WebScraperApi");
                            if (statusType != null)
                            {
                                var status = Activator.CreateInstance(statusType);

                                // Set properties using reflection
                                statusType.GetProperty("ScraperId").SetValue(status, _scraperInstanceId);
                                statusType.GetProperty("IsRunning").SetValue(status, true);
                                statusType.GetProperty("StartTime").SetValue(status, DateTime.Now);
                                statusType.GetProperty("UrlsProcessed").SetValue(status, 0);
                                statusType.GetProperty("UrlsQueued").SetValue(status, 0);
                                statusType.GetProperty("DocumentsProcessed").SetValue(status, 0);
                                statusType.GetProperty("HasErrors").SetValue(status, false);
                                statusType.GetProperty("Message").SetValue(status, "Scraper started");
                                statusType.GetProperty("ElapsedTime").SetValue(status, "00:00:00");
                                statusType.GetProperty("LastStatusUpdate").SetValue(status, DateTime.Now);
                                statusType.GetProperty("LastUpdate").SetValue(status, DateTime.Now);
                                statusType.GetProperty("LastError").SetValue(status, "");

                                // Call UpdateScraperStatusAsync
                                updateMethod.Invoke(_repository, new[] { status });
                                LogInfo($"Successfully initialized scraper status in database for ScraperId: {_scraperInstanceId}");

                                // Also add a log entry to the database
                                var logMethod = _repository.GetType().GetMethod("AddScraperLogAsync");
                                if (logMethod != null)
                                {
                                    var logType = Type.GetType("WebScraperApi.Data.Entities.ScraperLogEntity, WebScraperApi");
                                    if (logType != null)
                                    {
                                        var logEntry = Activator.CreateInstance(logType);

                                        // Set properties using reflection
                                        logType.GetProperty("ScraperId").SetValue(logEntry, _scraperInstanceId);
                                        logType.GetProperty("Timestamp").SetValue(logEntry, DateTime.Now);
                                        logType.GetProperty("LogLevel").SetValue(logEntry, "Info");
                                        logType.GetProperty("Message").SetValue(logEntry, "Scraper started by StateManagerComponent");

                                        // Call AddScraperLogAsync
                                        logMethod.Invoke(_repository, new[] { logEntry });
                                        LogInfo($"Successfully added start log entry to database");
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Failed to initialize scraper status in database");
                    }
                }

                LogInfo("Persistent state manager initialized successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize persistent state manager");
                LogError(ex, $"Error details: {ex.Message}");

                if (ex.InnerException != null)
                {
                    LogError(ex, $"Inner exception: {ex.InnerException.Message}");
                }

                // Instead of silently continuing, re-throw to properly handle the error
                throw;
            }
        }

        /// <summary>
        /// Called when scraping starts
        /// </summary>
        public override async Task OnScrapingStartedAsync()
        {
            await SaveStateAsync("Running");

            // Update database status if repository is available
            if (_repository != null)
            {
                try
                {
                    LogInfo($"Updating scraper status to 'Running' in database");

                    var updateMethod = _repository.GetType().GetMethod("UpdateScraperStatusAsync");
                    if (updateMethod != null)
                    {
                        var statusType = Type.GetType("WebScraperApi.Data.Entities.ScraperStatusEntity, WebScraperApi");
                        if (statusType != null)
                        {
                            var status = Activator.CreateInstance(statusType);

                            // Set properties using reflection
                            statusType.GetProperty("ScraperId").SetValue(status, _scraperInstanceId);
                            statusType.GetProperty("IsRunning").SetValue(status, true);
                            statusType.GetProperty("StartTime").SetValue(status, _startTime);
                            statusType.GetProperty("UrlsProcessed").SetValue(status, _pagesProcessed);
                            statusType.GetProperty("Message").SetValue(status, "Scraper started running");
                            statusType.GetProperty("ElapsedTime").SetValue(status, "00:00:00");
                            statusType.GetProperty("LastStatusUpdate").SetValue(status, DateTime.Now);
                            statusType.GetProperty("LastUpdate").SetValue(status, DateTime.Now);

                            // Call UpdateScraperStatusAsync
                            updateMethod.Invoke(_repository, new[] { status });
                            LogInfo($"Successfully updated scraper status to 'Running' in database");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to update scraper status to 'Running' in database");
                }
            }
        }

        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override async Task OnScrapingCompletedAsync()
        {
            await SaveStateAsync("Completed");

            // Update database status if repository is available
            if (_repository != null)
            {
                try
                {
                    LogInfo($"Updating scraper status to 'Completed' in database");

                    // Calculate elapsed time
                    TimeSpan elapsed = DateTime.Now - _startTime;
                    string elapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                    var updateMethod = _repository.GetType().GetMethod("UpdateScraperStatusAsync");
                    if (updateMethod != null)
                    {
                        var statusType = Type.GetType("WebScraperApi.Data.Entities.ScraperStatusEntity, WebScraperApi");
                        if (statusType != null)
                        {
                            var status = Activator.CreateInstance(statusType);

                            // Set properties using reflection
                            statusType.GetProperty("ScraperId").SetValue(status, _scraperInstanceId);
                            statusType.GetProperty("IsRunning").SetValue(status, false);
                            statusType.GetProperty("StartTime").SetValue(status, _startTime);
                            statusType.GetProperty("EndTime").SetValue(status, DateTime.Now);
                            statusType.GetProperty("UrlsProcessed").SetValue(status, _pagesProcessed);
                            statusType.GetProperty("Message").SetValue(status, $"Scraping completed. Processed {_pagesProcessed} pages.");
                            statusType.GetProperty("ElapsedTime").SetValue(status, elapsedTime);
                            statusType.GetProperty("LastStatusUpdate").SetValue(status, DateTime.Now);
                            statusType.GetProperty("LastUpdate").SetValue(status, DateTime.Now);

                            // Call UpdateScraperStatusAsync
                            updateMethod.Invoke(_repository, new[] { status });
                            LogInfo($"Successfully updated scraper status to 'Completed' in database");

                            // Also add a log entry
                            var logMethod = _repository.GetType().GetMethod("AddScraperLogAsync");
                            if (logMethod != null)
                            {
                                var logType = Type.GetType("WebScraperApi.Data.Entities.ScraperLogEntity, WebScraperApi");
                                if (logType != null)
                                {
                                    var logEntry = Activator.CreateInstance(logType);

                                    // Set properties using reflection
                                    logType.GetProperty("ScraperId").SetValue(logEntry, _scraperInstanceId);
                                    logType.GetProperty("Timestamp").SetValue(logEntry, DateTime.Now);
                                    logType.GetProperty("LogLevel").SetValue(logEntry, "Info");
                                    logType.GetProperty("Message").SetValue(logEntry, $"Scraping completed. Processed {_pagesProcessed} pages in {elapsedTime}.");

                                    // Call AddScraperLogAsync
                                    logMethod.Invoke(_repository, new[] { logEntry });
                                    LogInfo($"Successfully added completion log entry to database");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to update scraper status to 'Completed' in database");
                }
            }
        }

        /// <summary>
        /// Called when scraping is stopped
        /// </summary>
        public override async Task OnScrapingStoppedAsync()
        {
            await SaveStateAsync("Stopped");

            // Update database status if repository is available
            if (_repository != null)
            {
                try
                {
                    LogInfo($"Updating scraper status to 'Stopped' in database");

                    // Calculate elapsed time
                    TimeSpan elapsed = DateTime.Now - _startTime;
                    string elapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                    var updateMethod = _repository.GetType().GetMethod("UpdateScraperStatusAsync");
                    if (updateMethod != null)
                    {
                        var statusType = Type.GetType("WebScraperApi.Data.Entities.ScraperStatusEntity, WebScraperApi");
                        if (statusType != null)
                        {
                            var status = Activator.CreateInstance(statusType);

                            // Set properties using reflection
                            statusType.GetProperty("ScraperId").SetValue(status, _scraperInstanceId);
                            statusType.GetProperty("IsRunning").SetValue(status, false);
                            statusType.GetProperty("StartTime").SetValue(status, _startTime);
                            statusType.GetProperty("EndTime").SetValue(status, DateTime.Now);
                            statusType.GetProperty("UrlsProcessed").SetValue(status, _pagesProcessed);
                            statusType.GetProperty("Message").SetValue(status, $"Scraping stopped. Processed {_pagesProcessed} pages.");
                            statusType.GetProperty("ElapsedTime").SetValue(status, elapsedTime);
                            statusType.GetProperty("LastStatusUpdate").SetValue(status, DateTime.Now);
                            statusType.GetProperty("LastUpdate").SetValue(status, DateTime.Now);

                            // Call UpdateScraperStatusAsync
                            updateMethod.Invoke(_repository, new[] { status });
                            LogInfo($"Successfully updated scraper status to 'Stopped' in database");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to update scraper status to 'Stopped' in database");
                }
            }
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
            LogInfo($"SaveContentAsync called for URL: {url}, ContentType: {contentType}, Content Length: {content?.Length ?? 0}");

            try
            {
                // Increment pages processed counter
                _pagesProcessed++;

                // Always try to save to file, regardless of database settings
                await SaveContentToFileAsync(url, content, contentType);

                // Additionally save to database if configured
                if (_stateManager != null && Config.StoreContentInDatabase)
                {
                    try
                    {
                        LogInfo($"Saving content to database using state manager for URL: {url}");
                        var item = new WebScraper.ContentItem
                        {
                            Url = url,
                            ContentType = contentType,
                            RawContent = content,
                            TextContent = ExtractTextContent(content),
                            ScraperId = _scraperInstanceId,
                            LastStatusCode = 200,
                            IsReachable = true,
                            ContentHash = ComputeHash(content),
                            Title = ExtractTitle(content),
                            CapturedAt = DateTime.Now
                        };

                        var result = await _stateManager.SaveContentItemAsync(item);
                        LogInfo($"Database save result for URL {url}: {(result ? "Success" : "Failed")}");
                    }
                    catch (Exception dbEx)
                    {
                        LogError(dbEx, $"Failed to save content to database for URL: {url}");
                        if (dbEx.InnerException != null)
                        {
                            LogError(dbEx.InnerException, $"Inner exception saving to database: {dbEx.InnerException.Message}");
                        }
                    }
                }
                else
                {
                    LogWarning($"Skipping database save using StateManager - StateManager null: {_stateManager == null}, StoreContentInDatabase: {Config?.StoreContentInDatabase}");
                }

                // Update database using repository if available
                if (_repository != null)
                {
                    try
                    {
                        LogInfo($"Saving content to database using repository for URL: {url}");

                        // 1. First update the ScraperStatus with current progress
                        try {
                            // Calculate elapsed time
                            TimeSpan elapsed = DateTime.Now - _startTime;
                            string elapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                            var getStatusMethod = _repository.GetType().GetMethod("GetScraperStatusAsync");
                            var updateStatusMethod = _repository.GetType().GetMethod("UpdateScraperStatusAsync");

                            if (getStatusMethod != null && updateStatusMethod != null)
                            {
                                // Make sure we have the right parameters for GetScraperStatusAsync
                                var getStatusParams = getStatusMethod.GetParameters();
                                if (getStatusParams.Length != 1 || getStatusParams[0].ParameterType != typeof(string))
                                {
                                    LogWarning($"GetScraperStatusAsync method has unexpected parameters. Expected 1 string parameter, found {getStatusParams.Length} parameters.");
                                }
                                else
                                {
                                    // Get current status - safely await the task
                                    var task = (Task)getStatusMethod.Invoke(_repository, new object[] { _scraperInstanceId });
                                    await task;
                                    
                                    // Extract the result from the completed task using reflection
                                    var resultProperty = task.GetType().GetProperty("Result");
                                    var status = resultProperty?.GetValue(task);

                                    // If status is null, create a new one
                                    var statusType = Type.GetType("WebScraperApi.Data.Entities.ScraperStatusEntity, WebScraperApi");
                                    if (status == null && statusType != null)
                                    {
                                        status = Activator.CreateInstance(statusType);

                                        // Set properties using reflection
                                        statusType.GetProperty("ScraperId").SetValue(status, _scraperInstanceId);
                                        statusType.GetProperty("IsRunning").SetValue(status, true);
                                        statusType.GetProperty("StartTime").SetValue(status, _startTime);
                                        statusType.GetProperty("UrlsProcessed").SetValue(status, 0);
                                        statusType.GetProperty("DocumentsProcessed").SetValue(status, 0);
                                        statusType.GetProperty("Message").SetValue(status, "Processing started");
                                        statusType.GetProperty("ElapsedTime").SetValue(status, "00:00:00");
                                        statusType.GetProperty("LastError").SetValue(status, "");
                                    }

                                    if (status != null)
                                    {
                                        // Update the status
                                        try {
                                            statusType.GetProperty("IsRunning").SetValue(status, true);
                                            statusType.GetProperty("UrlsProcessed").SetValue(status, _pagesProcessed);
                                            statusType.GetProperty("DocumentsProcessed").SetValue(status, _pagesProcessed);
                                            
                                            // Truncate URL if it's too long to avoid any potential issues
                                            string truncatedUrl = url;
                                            if (url != null && url.Length > 100)
                                                truncatedUrl = url.Substring(0, 100) + "...";
                                                
                                            statusType.GetProperty("Message").SetValue(status, $"Processing URL: {truncatedUrl}");
                                            statusType.GetProperty("ElapsedTime").SetValue(status, elapsedTime);
                                            statusType.GetProperty("LastStatusUpdate").SetValue(status, DateTime.Now);
                                            statusType.GetProperty("LastUpdate").SetValue(status, DateTime.Now);

                                            // Check parameters for UpdateScraperStatusAsync
                                            var updateStatusParams = updateStatusMethod.GetParameters();
                                            if (updateStatusParams.Length != 1 || !updateStatusParams[0].ParameterType.IsAssignableFrom(statusType))
                                            {
                                                LogWarning($"UpdateScraperStatusAsync method has unexpected parameters. Expected 1 parameter of type {statusType.Name}.");
                                            }
                                            else
                                            {
                                                // Update the status - safely await task
                                                var updateTask = (Task)updateStatusMethod.Invoke(_repository, new object[] { status });
                                                await updateTask;
                                                LogInfo($"Updated scraper status in database: Pages={_pagesProcessed}, ElapsedTime={elapsedTime}");
                                            }
                                        }
                                        catch (Exception statusUpdateEx) {
                                            LogError(statusUpdateEx, "Error updating status properties");
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception statusEx) {
                            LogError(statusEx, "Error updating scraper status");
                        }

                        // 2. Now save the content to the ScrapedPage table
                        try {
                            var addPageMethod = _repository.GetType().GetMethod("AddScrapedPageAsync");
                            if (addPageMethod != null)
                            {
                                var pageType = Type.GetType("WebScraperApi.Data.Entities.ScrapedPageEntity, WebScraperApi");
                                if (pageType != null)
                                {
                                    // Create the page entity
                                    var page = Activator.CreateInstance(pageType);
                                    
                                    // Set essential properties, with null handling
                                    pageType.GetProperty("ScraperId").SetValue(page, _scraperInstanceId);
                                    pageType.GetProperty("Url").SetValue(page, url ?? string.Empty);
                                    pageType.GetProperty("ScrapedAt").SetValue(page, DateTime.Now);
                                    
                                    // Safely set content properties with null checks
                                    pageType.GetProperty("HtmlContent").SetValue(page, content ?? string.Empty);
                                    string textContent = null;
                                    try {
                                        textContent = ExtractTextContent(content);
                                    } catch (Exception textEx) {
                                        LogError(textEx, "Error extracting text content");
                                        textContent = string.Empty;
                                    }
                                    pageType.GetProperty("TextContent").SetValue(page, textContent ?? string.Empty);

                                    // Check parameter info
                                    var parameters = addPageMethod.GetParameters();
                                    if (parameters.Length != 1 || !parameters[0].ParameterType.IsAssignableFrom(pageType))
                                    {
                                        LogError(null, $"AddScrapedPageAsync method parameter mismatch: Expected {pageType.Name}, Found {(parameters.Length > 0 ? parameters[0].ParameterType.Name : "none")}");
                                    }
                                    else
                                    {
                                        // Save the page - safely await the task
                                        var addTask = (Task)addPageMethod.Invoke(_repository, new object[] { page });
                                        await addTask;
                                        LogInfo($"Successfully saved content to database using repository for URL: {url}");
                                    }
                                }
                            }
                        }
                        catch (Exception pageEx) {
                            LogError(pageEx, "Error saving scraped page");
                            if (pageEx.InnerException != null) {
                                LogError(pageEx.InnerException, $"Inner exception: {pageEx.InnerException.Message}");
                            }
                        }

                        // 3. Add a log entry - in a separate try/catch so it doesn't prevent other operations
                        try {
                            // Add the log entry separately to avoid collection modification during enumeration
                            Task.Run(async () => {
                                try {
                                    var addLogMethod = _repository.GetType().GetMethod("AddScraperLogAsync");
                                    if (addLogMethod != null)
                                    {
                                        var logType = Type.GetType("WebScraperApi.Data.Entities.ScraperLogEntity, WebScraperApi");
                                        if (logType != null)
                                        {
                                            var log = Activator.CreateInstance(logType);

                                            // Set properties using reflection, with truncation for long URLs
                                            logType.GetProperty("ScraperId").SetValue(log, _scraperInstanceId);
                                            logType.GetProperty("Timestamp").SetValue(log, DateTime.Now);
                                            logType.GetProperty("LogLevel").SetValue(log, "Info");
                                            
                                            // Truncate URL if needed to avoid issues
                                            string message = $"Processed URL: {url}";
                                            if (message.Length > 1000) {
                                                message = message.Substring(0, 1000) + "...";
                                            }
                                            logType.GetProperty("Message").SetValue(log, message);

                                            // Make sure the log method takes the expected parameter
                                            var logParams = addLogMethod.GetParameters();
                                            if (logParams.Length != 1 || !logParams[0].ParameterType.IsAssignableFrom(logType))
                                            {
                                                LogWarning($"AddScraperLogAsync method has unexpected parameters. Expected 1 parameter of type {logType.Name}.");
                                            }
                                            else
                                            {
                                                // Save the log entry - safely await the task
                                                var logTask = (Task)addLogMethod.Invoke(_repository, new object[] { log });
                                                await logTask;
                                                LogInfo($"Successfully added log entry to database");
                                            }
                                        }
                                    }
                                }
                                catch (Exception logEx) {
                                    LogError(logEx, "Error adding log entry");
                                }
                            }).ConfigureAwait(false);
                        }
                        catch (Exception logEx) {
                            LogError(logEx, "Error initiating log entry task");
                        }
                    }
                    catch (Exception repEx)
                    {
                        LogError(repEx, $"Failed to save content to database using repository for URL: {url}");
                        if (repEx.InnerException != null)
                        {
                            LogError(repEx.InnerException, $"Inner exception: {repEx.InnerException.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error in SaveContentAsync for URL: {url}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                }
                LogError(ex, $"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Saves content to file
        /// </summary>
        private async Task SaveContentToFileAsync(string url, string content, string contentType)
        {
            try
            {
                // Ensure we have an output directory
                string outputDir = Config.OutputDirectory;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData", _scraperInstanceId ?? "default");
                    LogInfo($"Output directory not specified, using default: {outputDir}");
                }

                // Use the main output directory instead of creating a timestamped subfolder
                LogInfo($"Saving content to main output directory: {outputDir}");

                // Ensure directory exists
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                    LogInfo($"Created directory: {outputDir}");
                }

                // Create safe filename
                string safeFilename = GetSafeFilename(url);
                LogInfo($"Using safe filename: {safeFilename} for URL: {url}");

                // Save HTML content
                string extension = contentType.Contains("html") ? ".html" : ".txt";
                string filePath = Path.Combine(outputDir, safeFilename + extension);

                LogInfo($"Saving content to file: {filePath}");
                await File.WriteAllTextAsync(filePath, content);
                LogInfo($"Successfully saved content to file: {filePath}");

                // Save extracted text content separately
                string textContent = ExtractTextContent(content);
                string textFilePath = Path.Combine(outputDir, safeFilename + ".txt");

                if (extension != ".txt")
                {
                    LogInfo($"Saving extracted text to file: {textFilePath}");
                    await File.WriteAllTextAsync(textFilePath, textContent);
                    LogInfo($"Successfully saved text content to file: {textFilePath}");
                }

                // Create a status file with metadata
                var metadata = new
                {
                    Url = url,
                    ScrapedAt = DateTime.Now,
                    ContentType = contentType,
                    ContentLength = content?.Length ?? 0,
                    TextLength = textContent?.Length ?? 0,
                    Title = ExtractTitle(content),
                    ScraperId = _scraperInstanceId
                };

                string metadataPath = Path.Combine(outputDir, safeFilename + ".meta.json");
                LogInfo($"Saving metadata to file: {metadataPath}");
                await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }));
                LogInfo($"Successfully saved metadata to file: {metadataPath}");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to save content to file for URL: {url}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException, $"Inner exception when saving to file: {ex.InnerException.Message}");
                }
                throw; // Re-throw to let the caller handle it
            }
        }

        /// <summary>
        /// Creates a safe filename from a URL
        /// </summary>
        private string GetSafeFilename(string url)
        {
            // Remove protocol and domain
            string filename = url.Replace("http://", "").Replace("https://", "");

            // Replace invalid filename characters
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                filename = filename.Replace(c, '_');
            }

            // Replace other problematic characters
            filename = filename.Replace('/', '_').Replace('\\', '_').Replace(':', '_').Replace('?', '_').Replace('&', '_');

            // Limit the length
            if (filename.Length > 100)
            {
                filename = filename.Substring(0, 100);
            }

            return filename;
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

        /// <summary>
        /// Extracts text content from HTML
        /// </summary>
        private string ExtractTextContent(string content)
        {
            if (string.IsNullOrEmpty(content))
                return string.Empty;

            try
            {
                var htmlDoc = new HtmlAgilityPack.HtmlDocument();
                htmlDoc.LoadHtml(content);

                // Remove script and style elements
                var scriptNodes = htmlDoc.DocumentNode.SelectNodes("//script|//style");
                if (scriptNodes != null)
                {
                    foreach (var node in scriptNodes)
                    {
                        node.Remove();
                    }
                }

                // Get and clean text content
                string text = htmlDoc.DocumentNode.InnerText;

                // Replace multiple whitespace with a single space
                text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");

                // Trim whitespace
                text = text.Trim();

                return text;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}