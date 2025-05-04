using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WebScraper.Scraping;
using WebScraper.Scraping.Components;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;
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
        private string _outputDirectory;
        private bool _initialized = false;

        /// <summary>
        /// Initializes a new instance of the ContentSaverAdapter class
        /// </summary>
        public ContentSaverAdapter(IScraperRepository repository, ILogger<ContentSaverAdapter> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

            // Create a timestamped subfolder for this run
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _outputDirectory = Path.Combine(_outputDirectory, timestamp);
            LogInfo($"Using timestamped output directory for this run: {_outputDirectory}");

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
            }

            _initialized = true;
            LogInfo("ContentSaverAdapter initialized successfully");
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

                // Save to database
                try
                {
                    // Get the scraper ID from the configuration
                    string scraperId = Core.Config?.Name ?? "unknown";
                    LogInfo($"Attempting to save content to database for URL: {url} with ScraperId: {scraperId}");

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

                    // Save to database
                    var savedEntity = await _repository.AddScrapedPageAsync(scrapedPage);
                    LogInfo($"Successfully saved content to database for URL: {url}, Entity ID: {savedEntity.Id}");
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
                        string htmlFilePath = Path.Combine(_outputDirectory, $"{filename}.html");
                        LogInfo($"Attempting to save HTML content to: {htmlFilePath}");
                        await TrySaveFileWithRetryAsync(htmlFilePath, htmlContent);

                        // Save text file
                        string textFilePath = Path.Combine(_outputDirectory, $"{filename}.txt");
                        LogInfo($"Attempting to save text content to: {textFilePath}");
                        await TrySaveFileWithRetryAsync(textFilePath, textContent);

                        // Log the file saving
                        LogInfo($"Successfully saved content to files: {htmlFilePath} and {textFilePath}");

                        // Add a log entry to the database
                        try
                        {
                            // Get the scraper ID from the configuration
                            string scraperId = Core.Config?.Name ?? "unknown";
                            LogInfo($"Adding log entry to database for file saving, ScraperId: {scraperId}");

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
            }
            catch (Exception ex)
            {
                LogError(ex, $"Error in SaveContentAsync for URL: {url}");
            }
        }

        /// <summary>
        /// Create a safe filename from a URL
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
        /// Tries to save a file with retry logic and alternative file creation if needed
        /// </summary>
        private async Task TrySaveFileWithRetryAsync(string filePath, string content, int maxRetries = 3)
        {
            Exception lastException = null;

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
                string directory = Path.GetDirectoryName(filePath);
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
                throw new AggregateException($"Failed to save file after retries and alternative file creation", lastException, ex);
            }
        }

        /// <summary>
        /// Checks if the exception is due to a locked file
        /// </summary>
        private bool IsFileLocked(IOException exception)
        {
            int errorCode = Marshal.GetHRForException(exception) & 0xFFFF;
            return errorCode == 32 || errorCode == 33 || errorCode == 0x20; // 32=ERROR_SHARING_VIOLATION, 33=ERROR_LOCK_VIOLATION
        }
    }
}
