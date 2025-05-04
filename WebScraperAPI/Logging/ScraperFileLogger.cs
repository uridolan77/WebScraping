using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScraperAPI.Logging
{
    /// <summary>
    /// A file logger for scrapers that creates a log file with a timestamp for each scraper run
    /// </summary>
    public class ScraperFileLogger
    {
        private readonly string _scraperId;
        private readonly string _scraperName;
        private readonly string _logFilePath;
        private readonly StringBuilder _logBuffer = new StringBuilder();
        private readonly object _lockObject = new object();
        private readonly ILogger _logger;
        private bool _initialized = false;

        /// <summary>
        /// Creates a new instance of the ScraperFileLogger
        /// </summary>
        /// <param name="scraperId">The ID of the scraper</param>
        /// <param name="scraperName">The name of the scraper</param>
        /// <param name="outputDirectory">The directory where log files should be stored</param>
        /// <param name="logger">Optional logger for internal logging</param>
        public ScraperFileLogger(string scraperId, string scraperName, string outputDirectory, ILogger logger = null)
        {
            _scraperId = scraperId;
            _scraperName = scraperName;
            _logger = logger;

            // Create a log file with a timestamp in the scraper's output directory
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string logFileName = $"scraper_{scraperId}_{timestamp}.log";
            
            // If output directory is not specified, use a default logs directory
            if (string.IsNullOrEmpty(outputDirectory))
            {
                outputDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            }
            
            // Create the directory if it doesn't exist
            if (!Directory.Exists(outputDirectory))
            {
                try
                {
                    Directory.CreateDirectory(outputDirectory);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, $"Failed to create log directory: {outputDirectory}");
                }
            }
            
            _logFilePath = Path.Combine(outputDirectory, logFileName);
            
            // Write initial log header
            LogInfo($"=== Scraper Log: {_scraperName} ({_scraperId}) ===");
            LogInfo($"Started at: {DateTime.Now}");
            LogInfo($"Log file: {_logFilePath}");
            LogInfo("===============================================");
            
            // Initialize the log file
            Initialize();
        }
        
        /// <summary>
        /// Initializes the log file
        /// </summary>
        private void Initialize()
        {
            try
            {
                // Write the initial buffer to the file
                File.WriteAllText(_logFilePath, _logBuffer.ToString());
                _initialized = true;
                
                _logger?.LogInformation($"Initialized scraper log file: {_logFilePath}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to initialize log file: {_logFilePath}");
            }
        }
        
        /// <summary>
        /// Logs an information message
        /// </summary>
        public void LogInfo(string message)
        {
            LogMessage("INFO", message);
        }
        
        /// <summary>
        /// Logs a warning message
        /// </summary>
        public void LogWarning(string message)
        {
            LogMessage("WARNING", message);
        }
        
        /// <summary>
        /// Logs an error message
        /// </summary>
        public void LogError(string message, Exception ex = null)
        {
            LogMessage("ERROR", message);
            
            if (ex != null)
            {
                LogMessage("ERROR", $"Exception: {ex.Message}");
                
                if (ex.InnerException != null)
                {
                    LogMessage("ERROR", $"Inner Exception: {ex.InnerException.Message}");
                }
                
                LogMessage("ERROR", $"Stack Trace: {ex.StackTrace}");
            }
        }
        
        /// <summary>
        /// Logs a message with the specified level
        /// </summary>
        private void LogMessage(string level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string logLine = $"[{timestamp}] [{level}] {message}";
            
            lock (_lockObject)
            {
                // Add to buffer
                _logBuffer.AppendLine(logLine);
                
                // Write to file if initialized
                if (_initialized)
                {
                    try
                    {
                        File.AppendAllText(_logFilePath, logLine + Environment.NewLine);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, $"Failed to write to log file: {_logFilePath}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Flushes the log buffer to the file
        /// </summary>
        public async Task FlushAsync()
        {
            if (!_initialized)
            {
                Initialize();
                return;
            }
            
            string buffer;
            lock (_lockObject)
            {
                buffer = _logBuffer.ToString();
                _logBuffer.Clear();
            }
            
            try
            {
                await File.AppendAllTextAsync(_logFilePath, buffer);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, $"Failed to flush log buffer to file: {_logFilePath}");
            }
        }
        
        /// <summary>
        /// Logs the completion of the scraper run
        /// </summary>
        public async Task LogCompletionAsync(bool success, string message = null)
        {
            LogInfo("===============================================");
            LogInfo($"Scraper {(success ? "completed successfully" : "failed")} at: {DateTime.Now}");
            
            if (!string.IsNullOrEmpty(message))
            {
                LogInfo(message);
            }
            
            LogInfo("===============================================");
            
            await FlushAsync();
        }
    }
}
