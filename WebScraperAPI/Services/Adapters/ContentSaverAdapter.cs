using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WebScraper.Scraping;
using WebScraper.Scraping.Components;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Models;
using WebScraperApi.Services;
using HtmlAgilityPack;

namespace WebScraperApi.Services.Adapters
{
    /// <summary>
    /// Adapter component that saves content to both files and database
    /// </summary>
    public class ContentSaverAdapter : ScraperComponentBase
    {
        private readonly IScraperRepository _repository;
        private readonly ILogger<ContentSaverAdapter> _logger;
        private readonly ScraperRunHistoryLogger _historyLogger;
        private string _outputDirectory;
        private bool _initialized = false;
        private int _pagesProcessed = 0;
        private DateTime _startTime;
        private readonly Process _currentProcess;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        /// <summary>
        /// Initializes a new instance of the ContentSaverAdapter class
        /// </summary>
        public ContentSaverAdapter(IScraperRepository repository, ILogger<ContentSaverAdapter> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _startTime = DateTime.Now;
            _historyLogger = new ScraperRunHistoryLogger(logger);
            _currentProcess = Process.GetCurrentProcess();
            _stopwatch.Start();
        }

        /// <summary>
        /// Initialize the component
        /// </summary>
        public override async Task InitializeAsync(ScraperCore core)
        {
            await base.InitializeAsync(core);

            // Get the output directory from the configuration
            _outputDirectory = Config.OutputDirectory;

            // Log the output directory
            LogInfo($"ContentSaverAdapter initializing with output directory: {_outputDirectory}");

            // If output directory is not specified, use a default directory
            if (string.IsNullOrEmpty(_outputDirectory))
            {
                _outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData", Core.Config.Name);
                LogInfo($"Output directory not specified, using default: {_outputDirectory}");
            }

            // Also create the base ScrapedData directory if it doesn't exist
            string baseScrapedDataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData");
            if (!Directory.Exists(baseScrapedDataDir))
            {
                try
                {
                    Directory.CreateDirectory(baseScrapedDataDir);
                    LogInfo($"Created base ScrapedData directory: {baseScrapedDataDir}");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to create base ScrapedData directory: {baseScrapedDataDir}");
                }
            }

            // Create a dedicated subdirectory for run history files to avoid path conflicts
            string runHistoryDir = Path.Combine(_outputDirectory, "run_history");
            if (!Directory.Exists(runHistoryDir))
            {
                try
                {
                    Directory.CreateDirectory(runHistoryDir);
                    LogInfo($"Created run history directory: {runHistoryDir}");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to create run history directory: {runHistoryDir}");
                    // Fallback to main output directory
                    runHistoryDir = _outputDirectory;
                }
            }

            // Use the root output directory without creating a timestamped subfolder
            LogInfo($"Using root output directory for content: {_outputDirectory}");

            // Create the output directory if it doesn't exist
            if (!Directory.Exists(_outputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(_outputDirectory);
                    LogInfo($"Created output directory: {_outputDirectory}");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to create output directory: {_outputDirectory}");
                }
            }

            // Create a test file to verify write permissions
            try
            {
                string testFilePath = Path.Combine(_outputDirectory, "test_write.txt");
                await File.WriteAllTextAsync(testFilePath, $"ContentSaverAdapter test file created at {DateTime.Now}");
                LogInfo($"Successfully created test file at: {testFilePath}");

                // Delete the test file
                File.Delete(testFilePath);
                LogInfo("Test file deleted successfully");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to create test file in output directory. File saving may not work.");

                // Try to create the directory again with more explicit error handling
                try
                {
                    LogInfo($"Attempting to recreate output directory: {_outputDirectory}");
                    Directory.CreateDirectory(_outputDirectory);

                    // Try to create the test file again
                    string retryTestFilePath = Path.Combine(_outputDirectory, "test_write_retry.txt");
                    await File.WriteAllTextAsync(retryTestFilePath, $"ContentSaverAdapter retry test file created at {DateTime.Now}");
                    LogInfo($"Successfully created retry test file at: {retryTestFilePath}");

                    // Delete the retry test file
                    File.Delete(retryTestFilePath);
                    LogInfo("Retry test file deleted successfully");
                }
                catch (Exception retryEx)
                {
                    LogError(retryEx, "Failed to create retry test file in output directory. File saving will not work.");
                }
            }

            // Initialize the run history logger
            try
            {
                string scraperId = Core.Config?.Name ?? "unknown";
                string scraperName = Core.Config?.Name ?? "Unknown Scraper";

                // Use the dedicated run history directory instead of the main output directory
                _logger.LogInformation($"Initializing run history logger with scraper ID: {scraperId}, name: {scraperName}, output dir: {runHistoryDir}");

                // Create a configuration object for the run history
                var runConfig = new ScraperRunConfiguration
                {
                    StartUrl = Core.Config?.StartUrl ?? string.Empty,
                    BaseUrl = Core.Config?.BaseUrl ?? string.Empty,
                    MaxDepth = Core.Config?.MaxDepth ?? 0,
                    MaxPages = 100, // Default value since MaxPages doesn't exist in ScraperConfig
                    MaxConcurrentRequests = Core.Config?.MaxConcurrentRequests ?? 0,
                    DelayBetweenRequests = Core.Config?.DelayBetweenRequests ?? 0,
                    FollowLinks = true, // Default value since FollowLinks doesn't exist in ScraperConfig
                    FollowExternalLinks = Core.Config?.FollowExternalLinks ?? false,
                    RespectRobotsTxt = Core.Config?.RespectRobotsTxt ?? true
                };

                // Add content selectors if available
                if (Core.Config?.ContentExtractorSelectors != null)
                {
                    runConfig.ContentExtractorSelectors.AddRange(Core.Config.ContentExtractorSelectors);
                }

                // Add exclude selectors if available
                if (Core.Config?.ContentExtractorExcludeSelectors != null)
                {
                    runConfig.ContentExtractorExcludeSelectors.AddRange(Core.Config.ContentExtractorExcludeSelectors);
                }

                // Initialize the run history logger with the dedicated directory
                await _historyLogger.InitializeAsync(scraperId, scraperName, runHistoryDir, runConfig);

                LogInfo($"Successfully initialized run history logger with output file: {_historyLogger.GetHistoryFilePath()}");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize run history logger");
            }

            // Initialize the scraper status in the database
            try
            {
                string scraperId = Core.Config?.Name ?? "unknown";
                LogInfo($"Initializing scraper status in database for ScraperId: {scraperId}");

                var status = new ScraperStatusEntity
                {
                    ScraperId = scraperId,
                    IsRunning = true,
                    StartTime = DateTime.Now,
                    UrlsProcessed = 0,
                    UrlsQueued = 0,
                    DocumentsProcessed = 0,
                    HasErrors = false,
                    Message = "Scraper started",
                    ElapsedTime = "00:00:00",
                    LastStatusUpdate = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    LastError = ""
                };

                await _repository.UpdateScraperStatusAsync(status);
                LogInfo($"Successfully initialized scraper status in database for ScraperId: {scraperId}");

                // Also add a log entry to the database
                var logEntry = new ScraperLogEntity
                {
                    ScraperId = scraperId,
                    Timestamp = DateTime.Now,
                    LogLevel = "Info",
                    Message = "Scraper started"
                };

                await _repository.AddScraperLogAsync(logEntry);
                LogInfo($"Successfully added start log entry to database");

                // Add the initialization log entry to the run history
                await _historyLogger.AddLogEntryAsync("Info", "Scraper started");
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to initialize scraper status in database");
                LogError(ex, $"Database connection error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                }

                // Add the error to the run history
                await _historyLogger.AddLogEntryAsync("Error", $"Failed to initialize scraper status in database: {ex.Message}");
            }

            _initialized = true;
            LogInfo("ContentSaverAdapter initialized successfully");
            await _historyLogger.AddLogEntryAsync("Info", "ContentSaverAdapter initialized successfully");
        }

        /// <summary>
        /// Save content to both files and database
        /// </summary>
        public async Task SaveContentAsync(string url, string htmlContent, string textContent)
        {
            if (!_initialized && Core != null)
            {
                await InitializeAsync(Core);
            }

            try
            {
                // Create a stopwatch to measure processing time for this URL
                var urlProcessingStopwatch = new Stopwatch();
                urlProcessingStopwatch.Start();

                // Increment the pages processed counter
                _pagesProcessed++;

                // Extract title from HTML
                string title = "Unknown";
                try
                {
                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlContent);
                    var titleNode = htmlDoc.DocumentNode.SelectSingleNode("//title");
                    if (titleNode != null)
                    {
                        title = titleNode.InnerText.Trim();
                    }
                }
                catch (Exception ex)
                {
                    LogWarning($"Could not extract title: {ex.Message}");
                }

                // Get the scraper ID from the configuration
                string scraperId = Core.Config?.Name ?? "unknown";

                // Update scraper status in the database - WITH EXPLICIT ERROR HANDLING
                try
                {
                    // Calculate elapsed time
                    TimeSpan elapsed = DateTime.Now - _startTime;
                    string elapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                    LogInfo($"CRITICAL DEBUG: About to update scraper status in database with ScraperId: {scraperId}, Processed URLs: {_pagesProcessed}");

                    // Try to get the existing status first with error handling
                    ScraperStatusEntity? status = null;
                    try
                    {
                        status = await _repository.GetScraperStatusAsync(scraperId);
                        LogInfo($"Successfully retrieved status for ScraperId: {scraperId}, IsRunning: {status?.IsRunning ?? false}");
                    }
                    catch (Exception getEx)
                    {
                        LogError(getEx, $"Failed to get existing status from database for ScraperId: {scraperId}");
                        LogInfo($"Creating new status entity for ScraperId: {scraperId}");
                        // Continue with null status, we'll create a new one
                    }

                    if (status == null)
                    {
                        LogInfo($"Creating new status entity for ScraperId: {scraperId}");
                        status = new ScraperStatusEntity
                        {
                            ScraperId = scraperId,
                            IsRunning = true,
                            StartTime = _startTime,
                            UrlsProcessed = _pagesProcessed,
                            UrlsQueued = 0,
                            DocumentsProcessed = _pagesProcessed,
                            HasErrors = false,
                            Message = $"Processing URL: {url}",
                            ElapsedTime = elapsedTime,
                            LastStatusUpdate = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            LastError = ""
                        };
                    }
                    else
                    {
                        LogInfo($"Updating existing status entity for ScraperId: {scraperId}");
                        status.IsRunning = true;
                        status.UrlsProcessed = _pagesProcessed;
                        status.DocumentsProcessed = _pagesProcessed;
                        status.Message = $"Processing URL: {url}";
                        status.ElapsedTime = elapsedTime;
                        status.LastStatusUpdate = DateTime.Now;
                        status.LastUpdate = DateTime.Now;
                    }

                    try
                    {
                        var updatedStatus = await _repository.UpdateScraperStatusAsync(status);
                        LogInfo($"SUCCESSFULLY updated scraper status in database: ScraperId={scraperId}, ProcessedURLs={_pagesProcessed}, ElapsedTime={elapsedTime}");
                        if (updatedStatus != null)
                        {
                            LogInfo($"Updated status ID: {updatedStatus.ScraperId}, IsRunning: {updatedStatus.IsRunning}, URLs: {updatedStatus.UrlsProcessed}");
                        }
                    }
                    catch (Exception updateEx)
                    {
                        LogError(updateEx, $"CRITICAL: Failed to update scraper status in database for ScraperId: {scraperId}");
                        if (updateEx.InnerException != null)
                        {
                            LogError(updateEx.InnerException, $"Inner exception: {updateEx.InnerException.Message}");
                        }
                        LogError(updateEx, $"Stack trace: {updateEx.StackTrace}");
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to update scraper status in database");
                    LogError(ex, $"Stack trace: {ex.StackTrace}");
                }

                // Save to database with explicit error handling
                try
                {
                    LogInfo($"CRITICAL DEBUG: About to save content to database for URL: {url} with ScraperId: {scraperId}");

                    // Add a log entry to confirm this code is being executed
                    try
                    {
                        var debugLogEntry = new ScraperLogEntity
                        {
                            ScraperId = scraperId,
                            Timestamp = DateTime.Now,
                            LogLevel = "Info",
                            Message = $"DEBUG: Attempting to save to scrapedpage table for URL: {url}"
                        };
                        await _repository.AddScraperLogAsync(debugLogEntry);
                    }
                    catch (Exception logEx)
                    {
                        LogError(logEx, $"Failed to add debug log entry: {logEx.Message}");
                    }

                    var scrapedPage = new ScrapedPageEntity
                    {
                        ScraperId = scraperId,
                        Url = url,
                        HtmlContent = htmlContent,
                        TextContent = textContent,
                        ScrapedAt = DateTime.Now
                    };

                    // Log the entity details before saving
                    LogInfo($"ScrapedPageEntity created with ScraperId: {scrapedPage.ScraperId}, URL: {scrapedPage.Url}, Content Length: {scrapedPage.HtmlContent.Length}, ScrapedAt: {scrapedPage.ScrapedAt}");

                    // Add another log entry right before the database call
                    try
                    {
                        var preDbLogEntry = new ScraperLogEntity
                        {
                            ScraperId = scraperId,
                            Timestamp = DateTime.Now,
                            LogLevel = "Info",
                            Message = $"DEBUG: Calling _repository.AddScrapedPageAsync for URL: {url}"
                        };
                        await _repository.AddScraperLogAsync(preDbLogEntry);
                    }
                    catch (Exception logEx)
                    {
                        LogError(logEx, $"Failed to add pre-db log entry: {logEx.Message}");
                    }

                    // Save to database with explicit try/catch
                    ScrapedPageEntity? savedEntity = null;
                    try
                    {
                        savedEntity = await _repository.AddScrapedPageAsync(scrapedPage);
                        LogInfo($"SUCCESSFULLY saved content to database for URL: {url}" +
                            (savedEntity != null ? $", Entity ID: {savedEntity.Id}" : ", but returned entity was null"));

                        // Add a success log entry
                        try
                        {
                            var successLogEntry = new ScraperLogEntity
                            {
                                ScraperId = scraperId,
                                Timestamp = DateTime.Now,
                                LogLevel = "Info",
                                Message = $"SUCCESS: Saved to scrapedpage table for URL: {url}" +
                                    (savedEntity != null ? $", ID: {savedEntity.Id}" : ", but returned entity was null")
                            };
                            await _repository.AddScraperLogAsync(successLogEntry);
                        }
                        catch (Exception logEx)
                        {
                            LogError(logEx, $"Failed to add success log entry: {logEx.Message}");
                        }
                    }
                    catch (Exception saveEx)
                    {
                        LogError(saveEx, $"CRITICAL: Failed to save content to database for URL: {url}");
                        if (saveEx.InnerException != null)
                        {
                            LogError(saveEx.InnerException, $"Inner exception: {saveEx.InnerException.Message}");
                        }
                        LogError(saveEx, $"Stack trace: {saveEx.StackTrace}");

                        // Add an error log entry
                        try
                        {
                            var errorLogEntry = new ScraperLogEntity
                            {
                                ScraperId = scraperId,
                                Timestamp = DateTime.Now,
                                LogLevel = "Error",
                                Message = $"ERROR: Failed to save to scrapedpage table for URL: {url}. Error: {saveEx.Message}"
                            };
                            await _repository.AddScraperLogAsync(errorLogEntry);
                        }
                        catch (Exception logEx)
                        {
                            LogError(logEx, $"Failed to add error log entry: {logEx.Message}");
                        }
                    }

                    // Add metric for pages processed - with explicit error handling
                    try
                    {
                        LogInfo($"CRITICAL DEBUG: About to add metric to database: PagesProcessed={_pagesProcessed}");

                        // Create metric entity using the correct namespace/type
                        var metricEntity = new WebScraperApi.Data.ScraperMetricEntity
                        {
                            ScraperId = scraperId,
                            MetricName = "PagesProcessed",
                            MetricValue = _pagesProcessed,
                            Timestamp = DateTime.Now
                        };

                        try
                        {
                            var savedMetric = await _repository.AddScraperMetricAsync(metricEntity);
                            LogInfo($"SUCCESSFULLY added PagesProcessed metric to database: {_pagesProcessed}" +
                                (savedMetric != null ? $", Metric ID: {savedMetric.Id}" : ", but returned metric was null"));
                        }
                        catch (Exception metricEx)
                        {
                            LogError(metricEx, $"CRITICAL: Failed to add metric to database");
                            if (metricEx.InnerException != null)
                            {
                                LogError(metricEx.InnerException, $"Inner exception: {metricEx.InnerException.Message}");
                            }
                            LogError(metricEx, $"Stack trace: {metricEx.StackTrace}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Failed to add metric to database: {ex.Message}");
                        LogError(ex, $"Stack trace: {ex.StackTrace}");
                    }

                    // Add log entry for successful processing - with explicit error handling
                    try
                    {
                        var logEntry = new ScraperLogEntity
                        {
                            ScraperId = scraperId,
                            Timestamp = DateTime.Now,
                            LogLevel = "Info",
                            Message = $"Processed URL: {url} (#{_pagesProcessed})"
                        };

                        try
                        {
                            var savedLog = await _repository.AddScraperLogAsync(logEntry);
                            LogInfo($"SUCCESSFULLY added log entry to database for URL processing");
                        }
                        catch (Exception logEx)
                        {
                            LogError(logEx, $"Failed to add log entry to database");
                            if (logEx.InnerException != null)
                            {
                                LogError(logEx.InnerException, $"Inner exception: {logEx.InnerException.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Failed to create log entry: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to save content to database for URL: {url}");
                    // Log more detailed error information
                    if (ex.InnerException != null)
                    {
                        LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                    }
                    LogError(ex, $"Stack trace: {ex.StackTrace}");
                }

                // Variable to track whether file saving was successful
                bool fileSavingSuccess = false;
                string htmlFilePath = string.Empty;
                string textFilePath = string.Empty;

                // Save to files if output directory is configured
                if (!string.IsNullOrEmpty(_outputDirectory))
                {
                    try
                    {
                        LogInfo($"Saving files to output directory: {_outputDirectory}");

                        // Check if directory exists
                        if (!Directory.Exists(_outputDirectory))
                        {
                            LogInfo($"Output directory does not exist, creating: {_outputDirectory}");
                            Directory.CreateDirectory(_outputDirectory);
                        }

                        // Create a safe filename from the URL
                        string filename = GetSafeFilename(url);
                        LogInfo($"Generated safe filename: {filename} for URL: {url}");

                        // Save HTML file
                        htmlFilePath = Path.Combine(_outputDirectory, $"{filename}.html");
                        LogInfo($"Attempting to save HTML content to: {htmlFilePath}");
                        await TrySaveFileWithRetryAsync(htmlFilePath, htmlContent);

                        // Save text file
                        textFilePath = Path.Combine(_outputDirectory, $"{filename}.txt");
                        LogInfo($"Attempting to save text content to: {textFilePath}");
                        await TrySaveFileWithRetryAsync(textFilePath, textContent);

                        // Log the file saving
                        LogInfo($"Successfully saved content to files: {htmlFilePath} and {textFilePath}");
                        fileSavingSuccess = true;

                        // Add a log entry to the database
                        try
                        {
                            var logEntry = new ScraperLogEntity
                            {
                                ScraperId = scraperId,
                                Timestamp = DateTime.Now,
                                LogLevel = "Info",
                                Message = $"Saved content for {url} to {htmlFilePath} and {textFilePath}"
                            };

                            await _repository.AddScraperLogAsync(logEntry);
                            LogInfo($"Successfully added log entry to database for file saving");
                        }
                        catch (Exception ex)
                        {
                            LogWarning($"Could not add log entry to database: {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                LogWarning($"Inner exception: {ex.InnerException.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, $"Failed to save content to files for URL: {url}");
                        if (ex.InnerException != null)
                        {
                            LogError(ex.InnerException, $"Inner exception: {ex.InnerException.Message}");
                        }
                        LogError(ex, $"Stack trace: {ex.StackTrace}");
                    }
                }
                else
                {
                    LogWarning($"Output directory is not configured, skipping file saving for URL: {url}");
                }

                // Add the processed URL to the run history
                urlProcessingStopwatch.Stop();

                try
                {
                    var urlInfo = new ProcessedUrlInfo
                    {
                        Url = url,
                        ProcessedAt = DateTime.Now,
                        ProcessingTime = urlProcessingStopwatch.Elapsed,
                        Success = true, // Assume success since we got this far
                        FilePath = fileSavingSuccess ? htmlFilePath : string.Empty,
                        ContentSizeBytes = htmlContent.Length,
                        Title = title
                    };

                    await _historyLogger.RecordProcessedUrlAsync(urlInfo);

                    // Update metrics in the run history
                    await _historyLogger.UpdateMetricsAsync(
                        urlsQueued: 0, // We don't have this information here
                        documentsProcessed: _pagesProcessed,
                        peakMemoryUsageMb: _currentProcess.WorkingSet64 / 1024.0 / 1024.0
                    );

                    // Add a log entry to the run history
                    await _historyLogger.AddLogEntryAsync("Info", $"Processed URL: {url} (#{_pagesProcessed})");

                    LogInfo($"Added URL processing information to run history: {url}");
                }
                catch (Exception ex)
                {
                    LogError(ex, $"Failed to update run history with processed URL: {url}");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error in SaveContentAsync for URL: {url}");
                LogError(ex, $"Stack trace: {ex.StackTrace}");

                // Record the error in the run history
                try
                {
                    // Add an error entry to the run history
                    await _historyLogger.AddLogEntryAsync("Error", $"Failed to process URL: {url} - {ex.Message}");

                    // Record this URL as failed
                    var urlInfo = new ProcessedUrlInfo
                    {
                        Url = url,
                        ProcessedAt = DateTime.Now,
                        ProcessingTime = TimeSpan.Zero,
                        Success = false,
                        ContentSizeBytes = 0,
                        ErrorMessage = ex.Message
                    };

                    await _historyLogger.RecordProcessedUrlAsync(urlInfo);
                }
                catch (Exception logEx)
                {
                    LogError(logEx, $"Failed to log error to run history: {logEx.Message}");
                }
            }
        }

        /// <summary>
        /// Called when scraping completes
        /// </summary>
        public override async Task OnScrapingCompletedAsync()
        {
            await base.OnScrapingCompletedAsync();

            try
            {
                string scraperId = Core.Config?.Name ?? "unknown";
                LogInfo($"Scraping completed, updating status in database for ScraperId: {scraperId}");

                // Calculate elapsed time
                TimeSpan elapsed = DateTime.Now - _startTime;
                string elapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";

                // Update the status
                var status = await _repository.GetScraperStatusAsync(scraperId);

                if (status == null)
                {
                    status = new ScraperStatusEntity
                    {
                        ScraperId = scraperId,
                        IsRunning = false,
                        StartTime = _startTime,
                        EndTime = DateTime.Now,
                        UrlsProcessed = _pagesProcessed,
                        UrlsQueued = 0,
                        DocumentsProcessed = _pagesProcessed,
                        HasErrors = false,
                        Message = "Scraping completed",
                        ElapsedTime = elapsedTime,
                        LastStatusUpdate = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        LastError = ""
                    };
                }
                else
                {
                    status.IsRunning = false;
                    status.EndTime = DateTime.Now;
                    status.UrlsProcessed = _pagesProcessed;
                    status.DocumentsProcessed = _pagesProcessed;
                    status.Message = "Scraping completed";
                    status.ElapsedTime = elapsedTime;
                    status.LastStatusUpdate = DateTime.Now;
                    status.LastUpdate = DateTime.Now;
                }

                await _repository.UpdateScraperStatusAsync(status);
                LogInfo($"Updated final scraper status in database: ProcessedURLs={_pagesProcessed}, ElapsedTime={elapsedTime}");

                // Also add a log entry to the database
                var logEntry = new ScraperLogEntity
                {
                    ScraperId = scraperId,
                    Timestamp = DateTime.Now,
                    LogLevel = "Info",
                    Message = $"Scraping completed. Processed {_pagesProcessed} pages in {elapsedTime}."
                };

                await _repository.AddScraperLogAsync(logEntry);
                LogInfo($"Successfully added completion log entry to database");

                // Complete the run history
                try
                {
                    // Stop the overall performance stopwatch
                    _stopwatch.Stop();

                    // Get the final memory usage
                    double peakMemoryUsageMb = _currentProcess.PeakWorkingSet64 / 1024.0 / 1024.0;

                    // Update the metrics in the run history
                    await _historyLogger.UpdateMetricsAsync(
                        urlsQueued: 0, // We don't have this information here
                        documentsProcessed: _pagesProcessed,
                        peakMemoryUsageMb: peakMemoryUsageMb
                    );

                    // Complete the run history with success
                    string completionMessage = $"Scraping completed successfully. Processed {_pagesProcessed} pages in {elapsedTime}.";
                    await _historyLogger.CompleteRunAsync(true, completionMessage);

                    LogInfo($"Successfully completed run history log at {_historyLogger.GetHistoryFilePath()}");

                    // Dispose the history logger to ensure all resources are released
                    _historyLogger.Dispose();
                }
                catch (Exception ex)
                {
                    LogError(ex, "Failed to complete run history log");
                }
            }
            catch (Exception ex)
            {
                LogError(ex, "Failed to update completion status in database");

                // Try to complete the run history with an error
                try
                {
                    string errorMessage = $"Scraping completed with errors: {ex.Message}";
                    await _historyLogger.CompleteRunAsync(false, errorMessage);
                    _historyLogger.Dispose();
                }
                catch (Exception historyEx)
                {
                    LogError(historyEx, "Failed to complete run history with error status");
                }
            }
        }

        /// <summary>
        /// Create a safe filename from a URL
        /// </summary>
        private static string GetSafeFilename(string url)
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
                filename = filename[..100];
            }

            return filename;
        }

        /// <summary>
        /// Tries to save a file with retry logic and alternative file creation if needed
        /// </summary>
        private async Task TrySaveFileWithRetryAsync(string filePath, string content, int maxRetries = 3)
        {
            Exception? lastException = null;

            // Try a few times with the original file
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await File.WriteAllTextAsync(filePath, content);
                    return; // Success
                }
                catch (IOException ex) when (IsFileLocked(ex))
                {
                    lastException = ex;
                    LogWarning($"File {filePath} is locked, retrying in 100ms... (Attempt {i+1}/{maxRetries})");
                    await Task.Delay(100); // Wait a bit before retrying
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    LogWarning($"Error writing to file {filePath}: {ex.Message}");
                    break; // Don't retry for other types of exceptions
                }
            }

            // If we couldn't save to the original file, create an alternative file
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(directory))
                {
                    directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ScrapedData");
                    LogInfo($"Using default directory for alternative file: {directory}");

                    // Create the directory if it doesn't exist
                    if (!Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath);
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                string alternativeFilePath = Path.Combine(directory, $"{fileName}_{timestamp}{extension}");

                LogInfo($"Creating alternative file: {alternativeFilePath}");
                await File.WriteAllTextAsync(alternativeFilePath, content);

                // Log success with alternative file
                LogInfo($"Successfully saved to alternative file: {alternativeFilePath}");
            }
            catch (Exception ex)
            {
                LogError(ex, $"Failed to create alternative file: {ex.Message}");
                if (lastException != null)
                {
                    throw new AggregateException($"Failed to save file after retries and alternative file creation", lastException, ex);
                }
                else
                {
                    throw new AggregateException($"Failed to save file after retries and alternative file creation", ex);
                }
            }
        }

        /// <summary>
        /// Checks if the exception is due to a locked file
        /// </summary>
        private static bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & 0xFFFF;
            return errorCode == 32 || errorCode == 33 || errorCode == 0x20; // 32=ERROR_SHARING_VIOLATION, 33=ERROR_LOCK_VIOLATION
        }
    }
}
