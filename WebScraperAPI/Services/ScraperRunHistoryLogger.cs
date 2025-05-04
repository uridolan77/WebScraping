using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service for logging scraper run history to a consolidated JSON file
    /// </summary>
    public class ScraperRunHistoryLogger : IDisposable
    {
        private readonly ILogger _logger;
        private ScraperRunHistory _runHistory;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private readonly JsonSerializerOptions _jsonOptions;
        private bool _disposed = false;
        private Timer _autoSaveTimer;
        private readonly TimeSpan _autoSaveInterval = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Creates a new instance of the ScraperRunHistoryLogger
        /// </summary>
        public ScraperRunHistoryLogger(ILogger logger)
        {
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        /// <summary>
        /// Initialize a new scraper run history log
        /// </summary>
        public async Task InitializeAsync(string scraperId, string scraperName, string outputDirectory, ScraperRunConfiguration configuration)
        {
            if (string.IsNullOrEmpty(outputDirectory))
            {
                throw new ArgumentException("Output directory cannot be empty", nameof(outputDirectory));
            }

            // Create the run ID from the current timestamp
            string runId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            // Ensure the output directory exists
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
                _logger.LogInformation($"Created output directory: {outputDirectory}");
            }

            // Create the run history object
            _runHistory = new ScraperRunHistory
            {
                ScraperId = scraperId,
                ScraperName = scraperName,
                RunId = runId,
                StartTime = DateTime.Now,
                Configuration = configuration,
                OutputDirectory = outputDirectory,
                // Create the run history JSON file directly in the output directory with a timestamp
                JsonFilePath = Path.Combine(outputDirectory, $"run_history_{runId}.json")
            };

            // Add initial log entry
            AddLogEntry("Info", $"Started scraper run for {scraperName} (ID: {scraperId})");

            // Save the initial history file
            await SaveHistoryAsync();

            // Start auto-save timer
            _autoSaveTimer = new Timer(AutoSaveCallback, null, _autoSaveInterval, _autoSaveInterval);
            
            _logger.LogInformation($"Initialized scraper run history log at {_runHistory.JsonFilePath}");
        }

        /// <summary>
        /// Auto-save callback for the timer
        /// </summary>
        private void AutoSaveCallback(object state)
        {
            try
            {
                SaveHistoryAsync().Wait();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-saving run history");
            }
        }

        /// <summary>
        /// Add a log entry to the history
        /// </summary>
        public async Task AddLogEntryAsync(string level, string message)
        {
            if (_runHistory == null)
            {
                throw new InvalidOperationException("Run history has not been initialized");
            }

            await _semaphore.WaitAsync();
            try
            {
                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = level,
                    Message = message
                };

                _runHistory.LogEntries.Add(logEntry);
                
                // If this is an error, add it to the errors list too
                if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                {
                    _runHistory.HasErrors = true;
                    _runHistory.Errors.Add(new ErrorInfo
                    {
                        Timestamp = logEntry.Timestamp,
                        Message = message
                    });
                }
                
                // Save after significant log entries if it's been at least 5 seconds since last save
                if (level.Equals("Error", StringComparison.OrdinalIgnoreCase) || 
                    level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                {
                    await SaveHistoryAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Add a log entry to the history (non-async version for convenience)
        /// </summary>
        public void AddLogEntry(string level, string message)
        {
            AddLogEntryAsync(level, message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Record information about a processed URL
        /// </summary>
        public async Task RecordProcessedUrlAsync(ProcessedUrlInfo urlInfo)
        {
            if (_runHistory == null)
            {
                throw new InvalidOperationException("Run history has not been initialized");
            }

            await _semaphore.WaitAsync();
            try
            {
                _runHistory.ProcessedUrls.Add(urlInfo);
                _runHistory.TotalUrlsProcessed++;
                
                if (urlInfo.Success)
                {
                    _runHistory.SuccessfulUrls++;
                    _runHistory.TotalBytesDownloaded += urlInfo.ContentSizeBytes;
                }
                else
                {
                    _runHistory.FailedUrls++;
                    _runHistory.HasErrors = true;
                    
                    // Add to the errors list if there's an error message
                    if (!string.IsNullOrEmpty(urlInfo.ErrorMessage))
                    {
                        _runHistory.Errors.Add(new ErrorInfo
                        {
                            Timestamp = urlInfo.ProcessedAt,
                            Message = urlInfo.ErrorMessage,
                            RelatedUrl = urlInfo.Url
                        });
                    }
                }
                
                // Update metrics
                if (_runHistory.ProcessedUrls.Count > 0)
                {
                    // Calculate average processing time
                    long totalMs = 0;
                    foreach (var url in _runHistory.ProcessedUrls)
                    {
                        totalMs += (long)url.ProcessingTime.TotalMilliseconds;
                    }
                    _runHistory.AverageProcessingTimeMs = totalMs / _runHistory.ProcessedUrls.Count;
                    
                    // Calculate requests per second
                    if (_runHistory.EndTime.HasValue)
                    {
                        var totalSecs = (_runHistory.EndTime.Value - _runHistory.StartTime).TotalSeconds;
                        _runHistory.RequestsPerSecond = totalSecs > 0 ? _runHistory.TotalUrlsProcessed / totalSecs : 0;
                    }
                    else
                    {
                        var totalSecs = (DateTime.Now - _runHistory.StartTime).TotalSeconds;
                        _runHistory.RequestsPerSecond = totalSecs > 0 ? _runHistory.TotalUrlsProcessed / totalSecs : 0;
                    }
                }
                
                // Every 10 processed URLs, save the history
                if (_runHistory.TotalUrlsProcessed % 10 == 0)
                {
                    await SaveHistoryAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Update the metrics for the current run
        /// </summary>
        public async Task UpdateMetricsAsync(int urlsQueued, int documentsProcessed, double peakMemoryUsageMb)
        {
            if (_runHistory == null)
            {
                throw new InvalidOperationException("Run history has not been initialized");
            }

            await _semaphore.WaitAsync();
            try
            {
                _runHistory.TotalUrlsQueued = urlsQueued;
                _runHistory.TotalDocumentsProcessed = documentsProcessed;
                _runHistory.PeakMemoryUsageMb = peakMemoryUsageMb;
                
                // Calculate elapsed time
                TimeSpan elapsed = (_runHistory.EndTime ?? DateTime.Now) - _runHistory.StartTime;
                _runHistory.TotalElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                
                // Calculate requests per second again
                var totalSecs = elapsed.TotalSeconds;
                _runHistory.RequestsPerSecond = totalSecs > 0 ? _runHistory.TotalUrlsProcessed / totalSecs : 0;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Record the completion of the scraper run
        /// </summary>
        public async Task CompleteRunAsync(bool success, string message)
        {
            if (_runHistory == null)
            {
                throw new InvalidOperationException("Run history has not been initialized");
            }

            await _semaphore.WaitAsync();
            try
            {
                _runHistory.EndTime = DateTime.Now;
                _runHistory.FinalStatus = success ? "Completed" : "Failed";
                
                // Calculate the final elapsed time
                TimeSpan elapsed = _runHistory.EndTime.Value - _runHistory.StartTime;
                _runHistory.TotalElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                
                // Add the completion log entry
                _runHistory.LogEntries.Add(new LogEntry
                {
                    Timestamp = DateTime.Now,
                    Level = success ? "Info" : "Error",
                    Message = message
                });
                
                // Calculate final metrics
                if (_runHistory.ProcessedUrls.Count > 0)
                {
                    // Calculate requests per second
                    var totalSecs = elapsed.TotalSeconds;
                    _runHistory.RequestsPerSecond = totalSecs > 0 ? _runHistory.TotalUrlsProcessed / totalSecs : 0;
                }
                
                // Save the final history
                await SaveHistoryAsync();
                
                // Dispose the auto-save timer
                _autoSaveTimer?.Dispose();
                _autoSaveTimer = null;
                
                _logger.LogInformation($"Completed scraper run history log at {_runHistory.JsonFilePath}");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        /// <summary>
        /// Save the current history to a JSON file
        /// </summary>
        public async Task SaveHistoryAsync()
        {
            if (_runHistory == null)
            {
                throw new InvalidOperationException("Run history has not been initialized");
            }

            await _semaphore.WaitAsync();
            try
            {
                // Update the timestamp before saving
                if (!_runHistory.EndTime.HasValue)
                {
                    TimeSpan elapsed = DateTime.Now - _runHistory.StartTime;
                    _runHistory.TotalElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                }
                
                // Serialize to JSON
                string json = JsonSerializer.Serialize(_runHistory, _jsonOptions);
                
                // Ensure directory exists (double check)
                string directory = Path.GetDirectoryName(_runHistory.JsonFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    _logger.LogInformation($"Created directory for JSON history: {directory}");
                }
                
                // Write to file with explicit try/catch to log detailed errors
                try
                {
                    // First try writing to a temporary file
                    string tempPath = _runHistory.JsonFilePath + ".tmp";
                    await File.WriteAllTextAsync(tempPath, json);
                    
                    // If that succeeds, move to the final location (safer file writing)
                    if (File.Exists(_runHistory.JsonFilePath))
                    {
                        File.Delete(_runHistory.JsonFilePath);
                    }
                    File.Move(tempPath, _runHistory.JsonFilePath);
                    
                    _logger.LogInformation($"Successfully saved scraper run history to {_runHistory.JsonFilePath}");
                    _logger.LogDebug($"Run history file size: {new FileInfo(_runHistory.JsonFilePath).Length} bytes");
                }
                catch (Exception fileEx)
                {
                    _logger.LogError(fileEx, $"File system error saving JSON history to {_runHistory.JsonFilePath}");
                    _logger.LogError($"Path accessibility check: Directory exists={Directory.Exists(directory)}, Can write={CheckWriteAccess(directory)}");
                    
                    // Try writing to an alternative location as a fallback
                    try
                    {
                        string fallbackPath = Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            "WebScraper", 
                            "RunHistory",
                            $"run_history_fallback_{DateTime.Now:yyyyMMdd_HHmmss}.json"
                        );
                        
                        Directory.CreateDirectory(Path.GetDirectoryName(fallbackPath));
                        await File.WriteAllTextAsync(fallbackPath, json);
                        _logger.LogWarning($"Saved run history to fallback location: {fallbackPath}");
                    }
                    catch (Exception fallbackEx)
                    {
                        _logger.LogError(fallbackEx, "Failed to write to fallback location");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving scraper run history to {_runHistory?.JsonFilePath ?? "unknown"}");
                _logger.LogError($"Exception details: {ex.GetType().Name}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        
        /// <summary>
        /// Check if a directory is writable
        /// </summary>
        private bool CheckWriteAccess(string directoryPath)
        {
            try
            {
                // Try to create a temporary file to check write access
                string tempFile = Path.Combine(directoryPath, $"write_test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(tempFile, "Write test");
                File.Delete(tempFile);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the path to the current run history JSON file
        /// </summary>
        public string GetHistoryFilePath()
        {
            if (_runHistory == null)
            {
                return string.Empty;
            }
            
            return _runHistory.JsonFilePath;
        }

        /// <summary>
        /// Dispose resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose pattern implementation
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _autoSaveTimer?.Dispose();
                    _semaphore?.Dispose();
                }
                _disposed = true;
            }
        }
    }
}