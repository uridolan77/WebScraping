using HtmlAgilityPack;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using WebScraper.AdaptiveCrawling;
using WebScraper.ContentChange;
using WebScraper.PatternLearning;
using WebScraper.RateLimiting;

namespace WebScraper
{
    public class Scraper
    {
        // Configuration and state
        private readonly ScraperConfig _config;
        private readonly HashSet<string> _visitedUrls = new HashSet<string>();
        private readonly Dictionary<string, ScrapedPage> _scrapedData = new Dictionary<string, ScrapedPage>();
        private readonly Action<string> _logger;
        private readonly string _outputDirectory;

        // Component modules
        private readonly ContentChangeDetector _changeDetector;
        private readonly AdaptiveCrawlStrategy _crawlStrategy;
        private readonly PatternLearner _patternLearner;
        private readonly AdaptiveRateLimiter _rateLimiter;

        // HTTP client
        private readonly HttpClient _httpClient;

        // Crawling state
        private bool _isRunning = false;
        private readonly object _lock = new object();
        private CancellationTokenSource _continuousScrapingCts;

        public Scraper(ScraperConfig config, Action<string> logger = null)
            : this(config, logger, config?.OutputDirectory ?? "ScrapedData")
        {
            // Calls the more specific constructor
        }

        public Scraper(ScraperConfig config, Action<string> logger, string outputDirectory)
        {
            _config = config ?? new ScraperConfig();
            _logger = logger ?? (message => Console.WriteLine(message));
            _outputDirectory = outputDirectory;

            // Set the output directory in the config if it's not set
            if (string.IsNullOrEmpty(_config.OutputDirectory))
            {
                _config.OutputDirectory = outputDirectory;
            }

            // Initialize modules with the logger
            _changeDetector = new ContentChangeDetector(_config.OutputDirectory, _logger);
            
            // Register this scraper with the change detector
            if (!string.IsNullOrEmpty(_config.ScraperId))
            {
                _changeDetector.RegisterScraper(
                    _config.ScraperId,
                    _config.ScraperName,
                    _config.MaxVersionsToKeep,
                    _config.TrackContentVersions,
                    _config.NotifyOnChanges,
                    _config.NotificationEmail
                );
            }
            
            _crawlStrategy = new AdaptiveCrawlStrategy(_logger);
            _patternLearner = new PatternLearner(_logger, _config.OutputDirectory);
            _rateLimiter = new AdaptiveRateLimiter(_logger);

            // Initialize HTTP client with default settings
            _httpClient = new HttpClient(new HttpClientHandler
            {
                AllowAutoRedirect = true,
                MaxAutomaticRedirections = 5,
                AutomaticDecompression = System.Net.DecompressionMethods.All
            });
            _httpClient.Timeout = TimeSpan.FromSeconds(_config.RequestTimeoutSeconds);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", _config.UserAgent);

            // Create output directory if it doesn't exist
            if (!Directory.Exists(_config.OutputDirectory))
            {
                Directory.CreateDirectory(_config.OutputDirectory);
            }

            _logger("Scraper initialized");
        }

        public async Task InitializeAsync()
        {
            _logger("Initializing scraper...");

            // Load previously scraped data if exists
            await LoadPreviouslyScrapedDataAsync();

            // Load previously learned patterns if they exist
            await _patternLearner.LoadLearnedPatternsAsync();

            // Load version history for content change detection
            if (_config.EnableChangeDetection)
            {
                await LoadVersionHistoryAsync();
            }

            // Load page metadata for adaptive crawling
            if (_config.EnableAdaptiveCrawling)
            {
                await LoadPageMetadataAsync();
            }

            // Load site profiles for adaptive rate limiting
            if (_config.EnableAdaptiveRateLimiting)
            {
                await LoadSiteProfilesAsync();
            }

            _logger("Scraper initialized successfully");
        }

        public async Task SetupContinuousScrapingAsync(TimeSpan interval, CancellationToken? cancellationToken = null)
        {
            _continuousScrapingCts = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken ?? CancellationToken.None);
            
            _logger($"Setting up continuous scraping with interval of {interval.TotalMinutes} minutes");
            
            try
            {
                while (!_continuousScrapingCts.Token.IsCancellationRequested)
                {
                    _logger("Starting scheduled scraping run");
                    await StartScrapingAsync();
                    _logger($"Scheduled scraping completed, waiting {interval.TotalMinutes} minutes until next run");
                    
                    // Save results after each run
                    await SaveVersionHistoryAsync();
                    await SavePageMetadataAsync(); 
                    await SaveSiteProfilesAsync();
                    await _patternLearner.SaveLearnedPatternsAsync();
                    
                    try
                    {
                        await Task.Delay(interval, _continuousScrapingCts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        _logger("Continuous scraping canceled");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error in continuous scraping: {ex.Message}");
                throw;
            }
        }

        public void StopContinuousScraping()
        {
            _logger("Stopping continuous scraping");
            _continuousScrapingCts?.Cancel();
        }

        public async Task StartScrapingAsync()
        {
            _isRunning = true;

            // Add start URL to pending queue
            var startUrls = new ConcurrentBag<string>();
            startUrls.Add(_config.StartUrl);

            // Initialize priority queue if using adaptive crawling
            if (_config.EnableAdaptiveCrawling)
            {
                _crawlStrategy.InitializePriorityQueue(startUrls);
            }

            // Start the scraping process
            await ScrapeUrlsAsync(startUrls);

            _isRunning = false;
        }

        private async Task ScrapeUrlsAsync(ConcurrentBag<string> urls)
        {
            var tasks = new List<Task>();

            while (urls.TryTake(out var url))
            {
                if (_visitedUrls.Contains(url))
                    continue;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        // Process the URL
                        await ProcessUrlAsync(url);

                        // Extract links and add to queue
                        var links = ExtractLinks(_httpClient, url);
                        foreach (var link in links)
                        {
                            urls.Add(link);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger($"Error processing {url}: {ex.Message}");
                    }
                }));

                // Respect the delay between requests
                await Task.Delay(_config.DelayBetweenRequests);
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessUrlAsync(string url)
        {
            if (!IsAllowedByRobotsTxt(url))
                return;

            // Mark as visited
            _visitedUrls.Add(url);

            // Scrape the page content
            var content = await _httpClient.GetStringAsync(url);

            // Extract text content using pattern learner
            var textContent = _patternLearner.ExtractTextContent(content);

            // Content change detection if enabled
            if (_config.EnableChangeDetection)
            {
                _changeDetector.TrackPageVersion(url, content, textContent);
            }

            // Store the scraped data
            var scrapedPage = new ScrapedPage
            {
                Url = url,
                ScrapedDateTime = DateTime.Now,
                TextContent = textContent
            };

            lock (_scrapedData)
            {
                _scrapedData[url] = scrapedPage;
            }

            // Save the content to file
            await SavePageToDiskAsync(url, content, textContent);
        }

        private List<string> ExtractLinks(HttpClient httpClient, string currentUrl)
        {
            var result = new List<string>();
            var baseUri = new Uri(currentUrl);

            // Fetch and parse the HTML document
            var htmlDoc = new HtmlDocument();
            var htmlContent = httpClient.GetStringAsync(currentUrl).Result;
            htmlDoc.LoadHtml(htmlContent);

            var linkNodes = htmlDoc.DocumentNode.SelectNodes("//a[@href]");
            if (linkNodes == null)
                return result;

            foreach (var linkNode in linkNodes)
            {
                var href = linkNode.GetAttributeValue("href", string.Empty);

                if (string.IsNullOrWhiteSpace(href) || href.StartsWith("#") || href.StartsWith("javascript:"))
                    continue;

                if (Uri.TryCreate(baseUri, href, out Uri absoluteUri))
                {
                    var absoluteUrl = absoluteUri.ToString();

                    // Check if we should follow this link
                    if (!_config.FollowExternalLinks && !absoluteUrl.StartsWith(_config.BaseUrl))
                        continue;

                    // Normalize URL (remove fragments, default page names, etc.)
                    absoluteUrl = NormalizeUrl(absoluteUrl);

                    result.Add(absoluteUrl);
                }
            }

            return result;
        }

        private string NormalizeUrl(string url)
        {
            // Remove fragments
            int fragmentIndex = url.IndexOf('#');
            if (fragmentIndex >= 0)
            {
                url = url.Substring(0, fragmentIndex);
            }

            // Remove trailing slashes
            url = url.TrimEnd('/');

            // Add additional normalization as needed for your specific case

            return url;
        }

        private async Task SavePageToDiskAsync(string url, string rawHtml, string textContent)
        {
            try
            {
                // Create a safe filename from the URL
                var uri = new Uri(url);
                var path = uri.Host + uri.AbsolutePath.Replace("/", "_");
                if (string.IsNullOrEmpty(Path.GetExtension(path)))
                {
                    path += ".html";
                }
                path = Path.Combine(_config.OutputDirectory, path);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Save raw HTML
                await File.WriteAllTextAsync(path, rawHtml);

                // Save extracted text
                await File.WriteAllTextAsync(path + ".txt", textContent);
            }
            catch (Exception ex)
            {
                _logger($"Error saving page {url} to disk: {ex.Message}");
            }
        }

        private bool IsAllowedByRobotsTxt(string url)
        {
            // Implement robots.txt checking logic here
            return true;
        }

        private async Task LoadPreviouslyScrapedDataAsync()
        {
            try
            {
                var metadataPath = Path.Combine(_config.OutputDirectory, "metadata.json");
                var visitedUrlsPath = Path.Combine(_config.OutputDirectory, "visited_urls.txt");

                if (File.Exists(metadataPath))
                {
                    var json = await File.ReadAllTextAsync(metadataPath);
                    var data = JsonConvert.DeserializeObject<Dictionary<string, ScrapedPage>>(json);

                    if (data != null)
                    {
                        foreach (var kvp in data)
                        {
                            _scrapedData[kvp.Key] = kvp.Value;
                            _visitedUrls.Add(kvp.Key);
                        }
                    }
                }
                else if (File.Exists(visitedUrlsPath))
                {
                    var urls = await File.ReadAllLinesAsync(visitedUrlsPath);
                    foreach (var url in urls)
                    {
                        if (!string.IsNullOrWhiteSpace(url))
                        {
                            _visitedUrls.Add(url);
                        }
                    }
                }

                _logger($"Loaded previously scraped data: {_visitedUrls.Count} URLs.");
            }
            catch (Exception ex)
            {
                _logger($"Error loading previously scraped data: {ex.Message}");
            }
        }

        private async Task LoadVersionHistoryAsync()
        {
            try
            {
                var versionHistoryPath = Path.Combine(_config.OutputDirectory, "version_history.json");

                if (File.Exists(versionHistoryPath))
                {
                    var json = await File.ReadAllTextAsync(versionHistoryPath);
                    var history = JsonConvert.DeserializeObject<Dictionary<string, List<PageVersion>>>(json);

                    if (history != null)
                    {
                        _changeDetector.LoadVersionHistory(history);
                        _logger($"Loaded version history for {history.Count} pages.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error loading version history: {ex.Message}");
            }
        }

        private async Task SaveVersionHistoryAsync()
        {
            try
            {
                var history = _changeDetector.GetVersionHistory();
                var versionHistoryPath = Path.Combine(_config.OutputDirectory, "version_history.json");

                await File.WriteAllTextAsync(
                    versionHistoryPath,
                    JsonConvert.SerializeObject(history, Newtonsoft.Json.Formatting.Indented)
                );

                _logger($"Saved version history for {history.Count} pages.");
            }
            catch (Exception ex)
            {
                _logger($"Error saving version history: {ex.Message}");
            }
        }

        private async Task LoadPageMetadataAsync()
        {
            try
            {
                var metadataPath = Path.Combine(_config.OutputDirectory, "page_metadata.json");

                if (File.Exists(metadataPath))
                {
                    var json = await File.ReadAllTextAsync(metadataPath);
                    var metadata = JsonConvert.DeserializeObject<Dictionary<string, PageMetadata>>(json);

                    if (metadata != null)
                    {
                        _crawlStrategy.LoadMetadata(metadata);
                        _logger($"Loaded metadata for {metadata.Count} pages.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error loading page metadata: {ex.Message}");
            }
        }

        private async Task SavePageMetadataAsync()
        {
            try
            {
                var metadata = _crawlStrategy.GetPageMetadata();
                var metadataPath = Path.Combine(_config.OutputDirectory, "page_metadata.json");

                await File.WriteAllTextAsync(
                    metadataPath,
                    JsonConvert.SerializeObject(metadata, Newtonsoft.Json.Formatting.Indented)
                );

                _logger($"Saved metadata for {metadata.Count} pages.");
            }
            catch (Exception ex)
            {
                _logger($"Error saving page metadata: {ex.Message}");
            }
        }

        private async Task LoadSiteProfilesAsync()
        {
            try
            {
                var profilesPath = Path.Combine(_config.OutputDirectory, "site_profiles.json");

                if (File.Exists(profilesPath))
                {
                    var json = await File.ReadAllTextAsync(profilesPath);
                    var profiles = JsonConvert.DeserializeObject<Dictionary<string, SiteProfile>>(json);

                    if (profiles != null)
                    {
                        _rateLimiter.LoadSiteProfiles(profiles);
                        _logger($"Loaded profiles for {profiles.Count} sites.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger($"Error loading site profiles: {ex.Message}");
            }
        }

        private async Task SaveSiteProfilesAsync()
        {
            try
            {
                var profiles = _rateLimiter.GetSiteProfiles();
                var profilesPath = Path.Combine(_config.OutputDirectory, "site_profiles.json");

                await File.WriteAllTextAsync(
                    profilesPath,
                    JsonConvert.SerializeObject(profiles, Newtonsoft.Json.Formatting.Indented)
                );

                _logger($"Saved profiles for {profiles.Count} sites.");
            }
            catch (Exception ex)
            {
                _logger($"Error saving site profiles: {ex.Message}");
            }
        }
    }
}
