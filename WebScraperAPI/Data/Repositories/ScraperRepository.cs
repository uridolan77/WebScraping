using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    /// <summary>
    /// Repository implementation for scraper operations using Entity Framework Core
    /// </summary>
    public class ScraperRepository : IScraperRepository
    {
        private readonly WebScraperDbContext _context;
        private readonly IServiceProvider _serviceProvider;

        public ScraperRepository(WebScraperDbContext context, IServiceProvider serviceProvider)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Tests the database connection
        /// </summary>
        /// <returns>True if the connection is successful, false otherwise</returns>
        public bool TestDatabaseConnection()
        {
            try
            {
                // Try to connect to the database
                return _context.Database.CanConnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }

        #region Scraper Config Operations

        public async Task<List<ScraperConfigEntity>> GetAllScrapersAsync()
        {
            try
            {
                Console.WriteLine("GetAllScrapersAsync: Attempting to fetch all scrapers from database");

                var scrapers = await _context.ScraperConfigs
                    .OrderByDescending(s => s.LastModified)
                    .ToListAsync();

                Console.WriteLine($"GetAllScrapersAsync: Successfully retrieved {scrapers.Count} scrapers");
                return scrapers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetAllScrapersAsync: Error retrieving scrapers: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetAllScrapersAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetAllScrapersAsync: Stack trace: {ex.StackTrace}");

                // Rethrow the exception to be handled by the caller
                throw;
            }
        }

        public async Task<ScraperConfigEntity> GetScraperByIdAsync(string id)
        {
            try
            {
                // Get the main scraper config without any includes
                var scraperConfig = await _context.ScraperConfigs
                    .AsNoTracking()  // Use AsNoTracking for better performance
                    .FirstOrDefaultAsync(s => s.Id == id);

                return scraperConfig;
            }
            catch (Exception ex)
            {
                // Log the error only in case of exception
                Console.WriteLine($"GetScraperByIdAsync ERROR: {ex.Message}");

                // Rethrow the exception
                throw;
            }
        }

        public async Task<List<ScraperStartUrlEntity>> GetScraperStartUrlsAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetScraperStartUrlsAsync: Attempting to fetch start URLs for scraper with ID {scraperId}");

                var startUrls = await _context.ScraperStartUrls
                    .AsNoTracking()
                    .Where(s => s.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetScraperStartUrlsAsync: Successfully retrieved {startUrls.Count} start URLs for scraper with ID {scraperId}");
                return startUrls;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetScraperStartUrlsAsync: Error retrieving start URLs for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetScraperStartUrlsAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetScraperStartUrlsAsync: Stack trace: {ex.StackTrace}");

                // Rethrow the exception
                throw;
            }
        }

        public async Task<List<ContentExtractorSelectorEntity>> GetContentExtractorSelectorsAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetContentExtractorSelectorsAsync: Attempting to fetch content extractor selectors for scraper with ID {scraperId}");

                var selectors = await _context.ContentExtractorSelector
                    .AsNoTracking()
                    .Where(s => s.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetContentExtractorSelectorsAsync: Successfully retrieved {selectors.Count} content extractor selectors for scraper with ID {scraperId}");
                return selectors;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetContentExtractorSelectorsAsync: Error retrieving content extractor selectors for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetContentExtractorSelectorsAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetContentExtractorSelectorsAsync: Stack trace: {ex.StackTrace}");

                // Rethrow the exception
                throw;
            }
        }

        public async Task<List<KeywordAlertEntity>> GetKeywordAlertsAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetKeywordAlertsAsync: Attempting to fetch keyword alerts for scraper with ID {scraperId}");

                // Use Entity Framework with AsNoTracking for better performance
                var keywordAlerts = await _context.KeywordAlert
                    .AsNoTracking()
                    .Where(k => k.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetKeywordAlertsAsync: Successfully retrieved {keywordAlerts.Count} keyword alerts for scraper with ID {scraperId}");
                return keywordAlerts;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetKeywordAlertsAsync: Error retrieving keyword alerts for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetKeywordAlertsAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetKeywordAlertsAsync: Stack trace: {ex.StackTrace}");

                // Return an empty list instead of throwing an exception
                return new List<KeywordAlertEntity>();
            }
        }

        public async Task<List<DomainRateLimitEntity>> GetDomainRateLimitsAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetDomainRateLimitsAsync: Attempting to fetch domain rate limits for scraper with ID {scraperId}");

                // Use Entity Framework with AsNoTracking for better performance
                var domainRateLimits = await _context.DomainRateLimit
                    .AsNoTracking()
                    .Where(d => d.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetDomainRateLimitsAsync: Successfully retrieved {domainRateLimits.Count} domain rate limits for scraper with ID {scraperId}");
                return domainRateLimits;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetDomainRateLimitsAsync: Error retrieving domain rate limits for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetDomainRateLimitsAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetDomainRateLimitsAsync: Stack trace: {ex.StackTrace}");

                // Return an empty list instead of throwing an exception
                return new List<DomainRateLimitEntity>();
            }
        }

        public async Task<List<ProxyConfigurationEntity>> GetProxyConfigurationsAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetProxyConfigurationsAsync: Attempting to fetch proxy configurations for scraper with ID {scraperId}");

                // Use Entity Framework with AsNoTracking for better performance
                var proxyConfigurations = await _context.ProxyConfiguration
                    .AsNoTracking()
                    .Where(p => p.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetProxyConfigurationsAsync: Successfully retrieved {proxyConfigurations.Count} proxy configurations for scraper with ID {scraperId}");
                return proxyConfigurations;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetProxyConfigurationsAsync: Error retrieving proxy configurations for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetProxyConfigurationsAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetProxyConfigurationsAsync: Stack trace: {ex.StackTrace}");

                // Return an empty list instead of throwing an exception
                return new List<ProxyConfigurationEntity>();
            }
        }

        public async Task<List<WebhookTriggerEntity>> GetWebhookTriggersAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetWebhookTriggersAsync: Attempting to fetch webhook triggers for scraper with ID {scraperId}");

                // Use Entity Framework with AsNoTracking for better performance
                var webhookTriggers = await _context.WebhookTrigger
                    .AsNoTracking()
                    .Where(w => w.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetWebhookTriggersAsync: Successfully retrieved {webhookTriggers.Count} webhook triggers for scraper with ID {scraperId}");
                return webhookTriggers;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetWebhookTriggersAsync: Error retrieving webhook triggers for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetWebhookTriggersAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetWebhookTriggersAsync: Stack trace: {ex.StackTrace}");

                // Return an empty list instead of throwing an exception
                return new List<WebhookTriggerEntity>();
            }
        }

        public async Task<List<ScraperScheduleEntity>> GetScraperSchedulesAsync(string scraperId)
        {
            try
            {
                Console.WriteLine($"GetScraperSchedulesAsync: Attempting to fetch schedules for scraper with ID {scraperId}");

                // Use Entity Framework with AsNoTracking for better performance
                var schedules = await _context.ScraperSchedule
                    .AsNoTracking()
                    .Where(s => s.ScraperId == scraperId)
                    .ToListAsync();

                Console.WriteLine($"GetScraperSchedulesAsync: Successfully retrieved {schedules.Count} schedules for scraper with ID {scraperId}");
                return schedules;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetScraperSchedulesAsync: Error retrieving schedules for scraper with ID {scraperId}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetScraperSchedulesAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetScraperSchedulesAsync: Stack trace: {ex.StackTrace}");

                // Return an empty list instead of throwing an exception
                return new List<ScraperScheduleEntity>();
            }
        }

        public async Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.CreatedAt = DateTime.Now;
            scraper.LastModified = DateTime.Now;

            _context.ScraperConfigs.Add(scraper);
            await _context.SaveChangesAsync();

            return scraper;
        }

        public async Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.LastModified = DateTime.Now;

            try
            {
                // First, remove existing related entities to avoid duplicates
                try
                {
                    var existingStartUrls = await _context.ScraperStartUrls
                        .Where(s => s.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ScraperStartUrls.RemoveRange(existingStartUrls);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing start URLs: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingSelectors = await _context.ContentExtractorSelector
                        .Where(s => s.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ContentExtractorSelector.RemoveRange(existingSelectors);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing selectors: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingKeywords = await _context.KeywordAlert
                        .Where(k => k.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.KeywordAlert.RemoveRange(existingKeywords);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing keywords: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingWebhooks = await _context.WebhookTrigger
                        .Where(w => w.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.WebhookTrigger.RemoveRange(existingWebhooks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing webhooks: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingDomainRateLimits = await _context.DomainRateLimit
                        .Where(d => d.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.DomainRateLimit.RemoveRange(existingDomainRateLimits);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing domain rate limits: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingProxyConfigurations = await _context.ProxyConfiguration
                        .Where(p => p.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ProxyConfiguration.RemoveRange(existingProxyConfigurations);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing proxy configurations: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingSchedules = await _context.ScraperSchedule
                        .Where(s => s.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ScraperSchedule.RemoveRange(existingSchedules);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing schedules: {ex.Message}");
                    // Continue with the update even if this fails
                }

                // Update the main entity
                _context.Entry(scraper).State = EntityState.Modified;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateScraperAsync: {ex.Message}");
                throw;
            }

            // Add the new related entities
            try
            {
                if (scraper.StartUrls != null && scraper.StartUrls.Any())
                {
                    foreach (var startUrl in scraper.StartUrls)
                    {
                        try
                        {
                            // Ensure the ScraperId is set correctly
                            startUrl.ScraperId = scraper.Id;

                            // Create a new entity to avoid type conflicts
                            var newStartUrl = new ScraperStartUrlEntity
                            {
                                ScraperId = startUrl.ScraperId,
                                Url = startUrl.Url
                            };

                            _context.ScraperStartUrls.Add(newStartUrl);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error adding start URL: {ex.Message}");
                            // Continue with the next URL
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding start URLs: {ex.Message}");
                // Continue with the update even if this fails
            }

            try
            {
                if (scraper.ContentExtractorSelectors != null && scraper.ContentExtractorSelectors.Any())
                {
                    foreach (var selector in scraper.ContentExtractorSelectors)
                    {
                        try
                        {
                            // Ensure the ScraperId is set correctly
                            selector.ScraperId = scraper.Id;

                            // Create a new entity to avoid type conflicts
                            var newSelector = new ContentExtractorSelectorEntity
                            {
                                ScraperId = selector.ScraperId,
                                Selector = selector.Selector,
                                IsExclude = selector.IsExclude
                            };

                            _context.ContentExtractorSelector.Add(newSelector);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error adding selector: {ex.Message}");
                            // Continue with the next selector
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding selectors: {ex.Message}");
                // Continue with the update even if this fails
            }

            if (scraper.KeywordAlerts != null && scraper.KeywordAlerts.Any())
            {
                foreach (var keyword in scraper.KeywordAlerts)
                {
                    // Ensure the ScraperId is set correctly
                    keyword.ScraperId = scraper.Id;

                    // Create a new entity to avoid type conflicts
                    var newKeyword = new KeywordAlertEntity
                    {
                        ScraperId = keyword.ScraperId,
                        Keyword = keyword.Keyword
                    };

                    _context.KeywordAlert.Add(newKeyword);
                }
            }

            if (scraper.WebhookTriggers != null && scraper.WebhookTriggers.Any())
            {
                foreach (var webhook in scraper.WebhookTriggers)
                {
                    // Ensure the ScraperId is set correctly
                    webhook.ScraperId = scraper.Id;

                    // Create a new entity to avoid type conflicts
                    var newWebhook = new WebhookTriggerEntity
                    {
                        ScraperId = webhook.ScraperId,
                        TriggerName = webhook.TriggerName
                    };

                    _context.WebhookTrigger.Add(newWebhook);
                }
            }

            if (scraper.DomainRateLimits != null && scraper.DomainRateLimits.Any())
            {
                foreach (var domainRateLimit in scraper.DomainRateLimits)
                {
                    // Ensure the ScraperId is set correctly
                    domainRateLimit.ScraperId = scraper.Id;

                    // Create a new entity to avoid type conflicts
                    // Create a new domain rate limit entity with default values
                    var newDomainRateLimit = new DomainRateLimitEntity
                    {
                        ScraperId = domainRateLimit.ScraperId,
                        Domain = domainRateLimit.Domain
                    };

                    // Set the RequestsPerMinute property using reflection if it exists
                    var requestsPerMinuteProperty = typeof(DomainRateLimitEntity).GetProperty("RequestsPerMinute");
                    if (requestsPerMinuteProperty != null)
                    {
                        requestsPerMinuteProperty.SetValue(newDomainRateLimit, 60); // Default value for rate limiting
                    }

                    _context.DomainRateLimit.Add(newDomainRateLimit);
                }
            }

            if (scraper.ProxyConfigurations != null && scraper.ProxyConfigurations.Any())
            {
                foreach (var proxyConfig in scraper.ProxyConfigurations)
                {
                    // Ensure the ScraperId is set correctly
                    proxyConfig.ScraperId = scraper.Id;

                    // Create a new entity to avoid type conflicts
                    // Extract host from the Host property if it exists
                    string hostValue = "";
                    if (proxyConfig != null)
                    {
                        // Get the Host property using reflection
                        var hostProperty = proxyConfig.GetType().GetProperty("Host");
                        if (hostProperty != null)
                        {
                            var hostValue2 = hostProperty.GetValue(proxyConfig) as string;
                            if (!string.IsNullOrEmpty(hostValue2))
                            {
                                hostValue = hostValue2;
                                if (hostValue.Contains(":"))
                                {
                                    hostValue = hostValue.Split(':')[0];
                                }
                            }
                        }
                    }

                    // Get username and password using reflection
                    string username = "";
                    string password = "";
                    bool isActive = true;
                    int failureCount = 0;
                    DateTime? lastUsed = null;

                    var usernameProperty = proxyConfig.GetType().GetProperty("Username");
                    if (usernameProperty != null)
                    {
                        username = usernameProperty.GetValue(proxyConfig) as string ?? "";
                    }

                    var passwordProperty = proxyConfig.GetType().GetProperty("Password");
                    if (passwordProperty != null)
                    {
                        password = passwordProperty.GetValue(proxyConfig) as string ?? "";
                    }

                    var isActiveProperty = proxyConfig.GetType().GetProperty("IsActive");
                    if (isActiveProperty != null && isActiveProperty.GetValue(proxyConfig) is bool isActiveValue)
                    {
                        isActive = isActiveValue;
                    }

                    var failureCountProperty = proxyConfig.GetType().GetProperty("FailureCount");
                    if (failureCountProperty != null && failureCountProperty.GetValue(proxyConfig) is int failureCountValue)
                    {
                        failureCount = failureCountValue;
                    }

                    var lastUsedProperty = proxyConfig.GetType().GetProperty("LastUsed");
                    if (lastUsedProperty != null && lastUsedProperty.GetValue(proxyConfig) is DateTime lastUsedValue)
                    {
                        lastUsed = lastUsedValue;
                    }

                    // Create the proxy configuration entity
                    var newProxyConfig = new ProxyConfigurationEntity
                    {
                        ScraperId = proxyConfig.ScraperId,
                        Username = username,
                        Password = password,
                        IsActive = isActive,
                        FailureCount = failureCount,
                        LastUsed = lastUsed
                    };

                    // Set the ProxyUrl property using reflection if it exists
                    var proxyUrlProperty = typeof(ProxyConfigurationEntity).GetProperty("ProxyUrl");
                    if (proxyUrlProperty != null)
                    {
                        proxyUrlProperty.SetValue(newProxyConfig, $"{username}:{password}@{hostValue}");
                    }

                    _context.ProxyConfiguration.Add(newProxyConfig);
                }
            }

            if (scraper.Schedules != null && scraper.Schedules.Any())
            {
                foreach (var schedule in scraper.Schedules)
                {
                    // Ensure the ScraperId is set correctly
                    schedule.ScraperId = scraper.Id;

                    // Create a new entity to avoid type conflicts
                    var newSchedule = new ScraperScheduleEntity
                    {
                        ScraperId = schedule.ScraperId,
                        Name = schedule.Name,
                        CronExpression = schedule.CronExpression,
                        IsActive = schedule.IsActive,
                        LastRun = schedule.LastRun,
                        NextRun = schedule.NextRun,
                        MaxRuntimeMinutes = schedule.MaxRuntimeMinutes,
                        NotificationEmail = schedule.NotificationEmail
                    };

                    _context.ScraperSchedule.Add(newSchedule);
                }
            }

            // Save changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}");

                // Try to save just the main entity
                try
                {
                    // Reset the entity state
                    _context.ChangeTracker.Clear();

                    // Add the entity again
                    _context.Entry(scraper).State = EntityState.Modified;

                    // Save only the main entity
                    await _context.SaveChangesAsync();
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"Error saving main entity: {innerEx.Message}");
                    throw;
                }
            }

            return scraper;
        }

        public async Task<bool> DeleteScraperAsync(string id)
        {
            var scraper = await _context.ScraperConfigs.FindAsync(id);
            if (scraper == null)
                return false;

            _context.ScraperConfigs.Remove(scraper);
            await _context.SaveChangesAsync();

            return true;
        }

        #endregion

        #region Scraper Status Operations

        public async Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId)
        {
            try
            {
                var status = await _context.ScraperStatuses
                    .FirstOrDefaultAsync(s => s.ScraperId == scraperId);

                // Ensure non-nullable string properties have default values
                if (status != null)
                {
                    status.Message = status.Message ?? string.Empty;
                    status.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    status.LastError = status.LastError ?? string.Empty;
                }

                return status;
            }
            catch (InvalidCastException ex)
            {
                // If we get an InvalidCastException, it's likely due to NULL values in the database
                // Let's create a new status object with default values
                return new ScraperStatusEntity
                {
                    ScraperId = scraperId,
                    IsRunning = false,
                    UrlsProcessed = 0,
                    HasErrors = false,
                    Message = "Idle",
                    ElapsedTime = string.Empty,
                    LastError = string.Empty,
                    LastUpdate = DateTime.Now
                };
            }
        }

        public async Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status)
        {
            status.LastUpdate = DateTime.Now;

            // Ensure non-nullable string properties have default values
            status.Message = status.Message ?? string.Empty;
            status.ElapsedTime = status.ElapsedTime ?? string.Empty;
            status.LastError = status.LastError ?? string.Empty;

            var existingStatus = await _context.ScraperStatuses
                .FirstOrDefaultAsync(s => s.ScraperId == status.ScraperId);

            if (existingStatus == null)
            {
                _context.ScraperStatuses.Add(status);
            }
            else
            {
                // Handle NULL values in the database
                try
                {
                    _context.Entry(existingStatus).CurrentValues.SetValues(status);
                }
                catch (InvalidCastException ex)
                {
                    // If we get an InvalidCastException, it's likely due to NULL values in the database
                    // Let's handle each property individually
                    existingStatus.IsRunning = status.IsRunning;
                    existingStatus.StartTime = status.StartTime;
                    existingStatus.EndTime = status.EndTime;
                    existingStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    existingStatus.UrlsProcessed = status.UrlsProcessed;
                    existingStatus.UrlsQueued = status.UrlsQueued;
                    existingStatus.DocumentsProcessed = status.DocumentsProcessed;
                    existingStatus.HasErrors = status.HasErrors;
                    existingStatus.Message = status.Message ?? string.Empty;
                    existingStatus.LastStatusUpdate = status.LastStatusUpdate;
                    existingStatus.LastUpdate = status.LastUpdate;
                    existingStatus.LastMonitorCheck = status.LastMonitorCheck;
                    existingStatus.LastError = status.LastError ?? string.Empty;
                }
            }

            await _context.SaveChangesAsync();

            return status;
        }

        public async Task<List<ScraperStatusEntity>> GetAllRunningScrapersAsync()
        {
            try
            {
                // Get all scraper statuses where IsRunning is true
                var runningScrapers = await _context.ScraperStatuses
                    .Where(s => s.IsRunning)
                    .ToListAsync();

                // Ensure non-nullable string properties have default values
                foreach (var status in runningScrapers)
                {
                    status.Message = status.Message ?? string.Empty;
                    status.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    status.LastError = status.LastError ?? string.Empty;
                }

                return runningScrapers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting running scrapers: {ex.Message}");
                return new List<ScraperStatusEntity>();
            }
        }

        #endregion

        #region Scraper Run Operations

        public async Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            return await _context.ScraperRun
                .Where(r => r.ScraperId == scraperId)
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId)
        {
            return await _context.ScraperRun
                .Include(r => r.LogEntries)
                .Include(r => r.ContentChangeRecords)
                .Include(r => r.ProcessedDocuments)
                .FirstOrDefaultAsync(r => r.Id == runId);
        }

        public async Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRun.Add(run);
            await _context.SaveChangesAsync();

            // Update the scraper's LastRun property
            var scraper = await _context.ScraperConfigs.FindAsync(run.ScraperId);
            if (scraper != null)
            {
                scraper.LastRun = run.StartTime;
                scraper.RunCount++;
                await _context.SaveChangesAsync();
            }

            return run;
        }

        public async Task<ScraperRunEntity> UpdateScraperRunAsync(ScraperRunEntity run)
        {
            _context.Entry(run).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return run;
        }

        #endregion

        #region Log Entry Operations

        public async Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100)
        {
            return await _context.LogEntry
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry)
        {
            try
            {
                Console.WriteLine($"Adding log entry to database: ScraperId={logEntry.ScraperId}, Message={logEntry.Message?.Substring(0, Math.Min(50, logEntry.Message?.Length ?? 0))}...");

                // Clear any tracked entities to prevent tracking issues
                _context.ChangeTracker.Clear();

                // Set timestamp if not already set
                if (logEntry.Timestamp == default)
                {
                    logEntry.Timestamp = DateTime.Now;
                }

                // Create a new entity to avoid tracking issues
                var newLogEntry = new LogEntryEntity
                {
                    ScraperId = logEntry.ScraperId,
                    Message = logEntry.Message,
                    Timestamp = logEntry.Timestamp,
                    Level = logEntry.Level ?? "Info", // Using Level instead of LogLevel
                    RunId = logEntry.RunId
                };

                // Add the log entry to the context
                _context.LogEntry.Add(newLogEntry);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Copy back the generated ID
                logEntry.Id = newLogEntry.Id;

                Console.WriteLine($"Successfully added log entry to database with ID: {logEntry.Id}");
                return logEntry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding log entry to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return the original entry even though it wasn't saved
                return logEntry;
            }
        }

        public async Task<ScraperLogEntity> AddScraperLogAsync(ScraperLogEntity logEntry)
        {
            try
            {
                // IMPORTANT: Clear the change tracker to avoid EntityState.Unchanged errors
                _context.ChangeTracker.Clear();

                // Ensure the ID is 0 for new entities
                logEntry.Id = 0;

                // Ensure non-null values for required fields
                logEntry.ScraperId = logEntry.ScraperId ?? string.Empty;
                logEntry.LogLevel = logEntry.LogLevel ?? "Info";
                logEntry.Message = logEntry.Message ?? string.Empty;

                // Set timestamp if not already set
                if (logEntry.Timestamp == default)
                {
                    logEntry.Timestamp = DateTime.Now;
                }

                // Create a completely new entity to avoid any tracking issues
                var newLogEntry = new ScraperLogEntity
                {
                    ScraperId = logEntry.ScraperId,
                    Timestamp = logEntry.Timestamp,
                    LogLevel = logEntry.LogLevel,
                    Message = logEntry.Message
                };

                // Add the new entity to the context
                _context.ScraperLog.Add(newLogEntry);

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Copy back the generated ID
                logEntry.Id = newLogEntry.Id;

                return logEntry;
            }
            catch (Exception ex)
            {
                // Just return the original entry - we've done our best to save it
                return logEntry;
            }
        }

        #endregion

        #region Content Change Record Operations

        public async Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            return await _context.ContentChangeRecord
                .Where(c => c.ScraperId == scraperId)
                .OrderByDescending(c => c.DetectedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change)
        {
            _context.ContentChangeRecord.Add(change);
            await _context.SaveChangesAsync();

            return change;
        }

        #endregion

        #region Processed Document Operations

        public async Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            return await _context.ProcessedDocument
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.ProcessedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId)
        {
            return await _context.ProcessedDocument
                .Include(p => p.Metadata)
                .FirstOrDefaultAsync(p => p.Id == documentId);
        }

        public async Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document)
        {
            _context.ProcessedDocument.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        #endregion

        #region Metrics Operations

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId)
        {
            try
            {
                // Default to retrieving metrics from the last 24 hours
                var from = DateTime.Now.AddDays(-1);
                var to = DateTime.Now;

                // Try to get metrics from the custommetric table instead
                // Create empty ScraperMetricEntity objects with data from custommetric
                var customMetrics = await _context.CustomMetric
                    .Select(cm => new ScraperMetricEntity
                    {
                        Id = cm.Id,
                        ScraperId = scraperId,
                        MetricName = cm.MetricName,
                        MetricValue = cm.MetricValue,
                        Timestamp = DateTime.Now
                    })
                    .ToListAsync();

                if (customMetrics.Count > 0)
                {
                    return customMetrics;
                }

                // Fallback to the original query if no custom metrics found
                return await _context.ScraperMetric
                    .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to)
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting metrics: {ex.Message}");
                // Return an empty list if there's an error
                return [];
            }
        }

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to)
        {
            try
            {
                // Try to get metrics from the custommetric table first
                var customMetrics = await _context.CustomMetric
                    .Select(cm => new ScraperMetricEntity
                    {
                        Id = cm.Id,
                        ScraperId = scraperId,
                        MetricName = cm.MetricName,
                        MetricValue = cm.MetricValue,
                        Timestamp = DateTime.Now
                    })
                    .ToListAsync();

                if (customMetrics.Count > 0)
                {
                    // Filter the custom metrics based on the metricName parameter
                    if (!string.IsNullOrEmpty(metricName))
                    {
                        customMetrics = customMetrics.Where(m => m.MetricName == metricName).ToList();
                    }

                    return customMetrics;
                }

                // Fallback to the original query if no custom metrics found
                var query = _context.ScraperMetric
                    .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to);

                if (!string.IsNullOrEmpty(metricName))
                {
                    query = query.Where(m => m.MetricName == metricName);
                }

                return await query
                    .OrderBy(m => m.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting metrics: {ex.Message}");
                // Return an empty list if there's an error
                return [];
            }
        }

        public async Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric)
        {
            // Use a separate, new context instance to avoid concurrency issues
            using (var scope = _serviceProvider.CreateScope())
            {
                try
                {
                    var newContext = scope.ServiceProvider.GetRequiredService<WebScraperDbContext>();

                    // Only log metrics at Information level
                    Console.WriteLine($"METRIC: {metric.MetricName}={metric.MetricValue} for scraper {metric.ScraperId}");

                    // Get the scraper name if not already set
                    if (string.IsNullOrEmpty(metric.ScraperName))
                    {
                        try
                        {
                            var scraperConfig = await newContext.ScraperConfigs
                                .AsNoTracking()
                                .FirstOrDefaultAsync(s => s.Id == metric.ScraperId);

                            if (scraperConfig != null)
                            {
                                metric.ScraperName = scraperConfig.Name;
                                metric.ScraperConfigId = scraperConfig.Id;
                            }
                            else
                            {
                                metric.ScraperName = "Unknown";
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to get scraper name: {ex.Message}");
                            metric.ScraperName = "Unknown";
                        }
                    }

                    // Ensure timestamp is set
                    if (metric.Timestamp == default)
                    {
                        metric.Timestamp = DateTime.Now;
                    }

                    // Create a new entity to avoid tracking issues
                    var newMetric = new ScraperMetricEntity
                    {
                        ScraperId = metric.ScraperId,
                        ScraperConfigId = metric.ScraperConfigId,
                        ScraperName = metric.ScraperName,
                        MetricName = metric.MetricName,
                        MetricValue = metric.MetricValue,
                        Timestamp = metric.Timestamp,
                        RunId = metric.RunId
                    };

                    // Add the metric to the DbSet using the new context
                    newContext.ScraperMetric.Add(newMetric);

                    // Save changes using the new context
                    await newContext.SaveChangesAsync();

                    // Copy back the generated ID
                    metric.Id = newMetric.Id;

                    Console.WriteLine($"METRIC SAVED: {metric.MetricName}={metric.MetricValue} with ID: {metric.Id}");

                    // Update the scraper status to display URLs processed in the monitor page
                    // Use a separate context for this operation too
                    using (var statusScope = _serviceProvider.CreateScope())
                    {
                        var statusContext = statusScope.ServiceProvider.GetRequiredService<WebScraperDbContext>();

                        try
                        {
                            if (metric.MetricName == "PagesProcessed")
                            {
                                var status = await statusContext.ScraperStatuses
                                    .FirstOrDefaultAsync(s => s.ScraperId == metric.ScraperId);

                                if (status != null)
                                {
                                    // Update URLs processed count
                                    status.UrlsProcessed = (int)metric.MetricValue;
                                    status.DocumentsProcessed = (int)metric.MetricValue;
                                    status.LastStatusUpdate = DateTime.Now;
                                    status.LastUpdate = DateTime.Now;

                                    // Keep original message if it's not a status message
                                    if (status.Message == null || status.Message.Contains("pages processed") || status.Message.Contains("Idle") || string.IsNullOrEmpty(status.Message))
                                    {
                                        status.Message = $"Processing content. {status.UrlsProcessed} pages processed.";
                                    }

                                    // Calculate elapsed time if we have a start time
                                    if (status.StartTime.HasValue)
                                    {
                                        TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                                        status.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                                    }

                                    await statusContext.SaveChangesAsync();
                                    Console.WriteLine($"Updated scraper status with UrlsProcessed = {status.UrlsProcessed}");
                                }
                            }
                        }
                        catch (Exception statusEx)
                        {
                            Console.WriteLine($"Error updating scraper status: {statusEx.Message}");
                        }
                    }

                    return metric;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR SAVING METRIC: {metric.MetricName}={metric.MetricValue}, Error: {ex.Message}");
                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                    }

                    throw; // Rethrow the exception to be handled by the caller
                }
            }
        }

        #endregion

        #region Pipeline Metrics Operations

        public async Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId)
        {
            return await _context.PipelineMetric
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric)
        {
            _context.PipelineMetric.Add(metric);
            await _context.SaveChangesAsync();

            return metric;
        }

        #endregion

        #region Scraper Log Operations

        public async Task<List<ScraperLogEntity>> GetScraperLogsAsync(string scraperId, int limit = 100)
        {
            return await _context.ScraperLog
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        #endregion

        #region Content Classification Operations

        // Commented out until ContentClassificationEntity is properly implemented
        /*
        public async Task<ContentClassificationEntity> GetContentClassificationAsync(string scraperId, string url)
        {
            try
            {
                return await _context.ContentClassifications
                    .Include(c => c.Entities)
                    .Where(c => c.ScraperId == scraperId && c.Url == url)
                    .OrderByDescending(c => c.ClassifiedAt)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting content classification: {ex.Message}");
                return null;
            }
        }

        public async Task<List<ContentClassificationEntity>> GetContentClassificationsAsync(string scraperId, int limit = 50)
        {
            try
            {
                return await _context.ContentClassifications
                    .Include(c => c.Entities)
                    .Where(c => c.ScraperId == scraperId)
                    .OrderByDescending(c => c.ClassifiedAt)
                    .Take(limit)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting content classifications: {ex.Message}");
                return new List<ContentClassificationEntity>();
            }
        }

        public async Task<ContentClassificationEntity> SaveContentClassificationAsync(ContentClassificationEntity classification)
        {
            try
            {
                // Check if a classification already exists for this URL
                var existing = await _context.ContentClassifications
                    .Where(c => c.ScraperId == classification.ScraperId && c.Url == classification.Url)
                    .FirstOrDefaultAsync();

                if (existing != null)
                {
                    // Update existing classification
                    _context.Entry(existing).State = EntityState.Detached;
                    classification.Id = existing.Id;
                    _context.ContentClassifications.Update(classification);
                }
                else
                {
                    // Add new classification
                    _context.ContentClassifications.Add(classification);
                }

                await _context.SaveChangesAsync();
                return classification;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving content classification: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        */

        // Temporary implementations that return empty results
        public async Task<object> GetContentClassificationAsync(string scraperId, string url)
        {
            return await Task.FromResult<object>(null);
        }

        public async Task<List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50)
        {
            return await Task.FromResult(new List<object>());
        }

        public async Task<object> SaveContentClassificationAsync(object classification)
        {
            return await Task.FromResult(classification);
        }

        #endregion

        #region Scraped Page Operations

        public async Task<ScrapedPageEntity> AddScrapedPageAsync(ScrapedPageEntity page)
        {
            try
            {
                Console.WriteLine($"Adding scraped page to database: URL={page.Url}, ScraperId={page.ScraperId}");

                // Check if the ScrapedPage DbSet exists
                if (_context.ScrapedPage == null)
                {
                    Console.WriteLine("Error: ScrapedPage DbSet is null");
                    throw new InvalidOperationException("ScrapedPage DbSet is null");
                }

                // IMPORTANT: Clear the change tracker to avoid EntityState.Unchanged errors
                _context.ChangeTracker.Clear();

                // Get the scraper configuration to check MaxPages limit
                var scraperConfig = await _context.ScraperConfigs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == page.ScraperId);

                if (scraperConfig != null)
                {
                    // Get current page count for this scraper
                    var currentPageCount = await _context.ScrapedPage
                        .CountAsync(p => p.ScraperId == page.ScraperId);

                    // Check if we've reached the MaxPages limit
                    if (scraperConfig.MaxPages > 0 && currentPageCount >= scraperConfig.MaxPages)
                    {
                        Console.WriteLine($"MaxPages limit of {scraperConfig.MaxPages} reached for scraper {page.ScraperId}. Not adding page: {page.Url}");

                        // Update scraper status to indicate completion
                        await UpdateScraperStatusForMaxPagesReached(page.ScraperId, currentPageCount, scraperConfig.MaxPages);

                        // Return the page without saving it
                        return page;
                    }

                    Console.WriteLine($"Current page count: {currentPageCount}, MaxPages limit: {scraperConfig.MaxPages}");
                }

                // Create a new, detached entity to avoid tracking issues
                var newPage = new ScrapedPageEntity
                {
                    ScraperId = page.ScraperId,
                    Url = page.Url,
                    HtmlContent = page.HtmlContent,
                    TextContent = page.TextContent,
                    ScrapedAt = page.ScrapedAt
                };

                // Add the entity to the context
                _context.ScrapedPage.Add(newPage);
                Console.WriteLine($"Added scraped page to context, saving changes...");

                // Save changes to the database
                await _context.SaveChangesAsync();

                // Copy the generated ID back to the original page
                page.Id = newPage.Id;
                Console.WriteLine($"Successfully saved scraped page to database with ID: {page.Id}");

                // Update the scraper status with a separate operation to avoid tracking issues
                try
                {
                    // Clear tracking before updating status
                    _context.ChangeTracker.Clear();

                    Console.WriteLine("Updating scraper status with latest page information");
                    var status = await _context.ScraperStatuses
                        .AsNoTracking() // Use AsNoTracking to avoid EntityState issues
                        .FirstOrDefaultAsync(s => s.ScraperId == page.ScraperId);

                    if (status != null)
                    {
                        // Create a new status entity
                        var updatedStatus = new ScraperStatusEntity
                        {
                            ScraperId = status.ScraperId,
                            IsRunning = status.IsRunning,
                            StartTime = status.StartTime,
                            EndTime = status.EndTime,
                            UrlsProcessed = status.UrlsProcessed + 1,
                            DocumentsProcessed = status.DocumentsProcessed + 1,
                            UrlsQueued = status.UrlsQueued,
                            HasErrors = status.HasErrors,
                            Message = $"Processing content. {status.UrlsProcessed + 1} pages processed.",
                            LastStatusUpdate = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            LastMonitorCheck = status.LastMonitorCheck,
                            LastError = status.LastError ?? string.Empty
                        };

                        // Calculate elapsed time if we have a start time
                        if (status.StartTime.HasValue)
                        {
                            TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                            updatedStatus.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                        }
                        else
                        {
                            updatedStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                        }

                        // Check if we've reached the MaxPages limit
                        if (scraperConfig != null && scraperConfig.MaxPages > 0)
                        {
                            var newPageCount = await _context.ScrapedPage
                                .CountAsync(p => p.ScraperId == page.ScraperId);

                            if (newPageCount >= scraperConfig.MaxPages)
                            {
                                updatedStatus.Message = $"Completed. Reached maximum of {scraperConfig.MaxPages} pages.";
                                updatedStatus.IsRunning = false;
                                updatedStatus.EndTime = DateTime.Now;
                            }
                        }

                        // Use existing method to update status
                        await UpdateScraperStatusAsync(updatedStatus);
                        Console.WriteLine($"Updated scraper status with UrlsProcessed = {updatedStatus.UrlsProcessed}, ElapsedTime = {updatedStatus.ElapsedTime}");
                    }
                    else
                    {
                        Console.WriteLine($"No scraper status found for ScraperId: {page.ScraperId}");
                    }
                }
                catch (Exception statusEx)
                {
                    Console.WriteLine($"Error updating scraper status with page info: {statusEx.Message}");
                    if (statusEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {statusEx.InnerException.Message}");
                    }
                    // Don't rethrow - we don't want status update failure to affect page saving
                }

                return page;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding scraped page to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Rethrow the exception to be handled by the caller
                throw;
            }
        }

        private async Task UpdateScraperStatusForMaxPagesReached(string scraperId, int pageCount, int maxPages)
        {
            try
            {
                // Clear tracking before updating status
                _context.ChangeTracker.Clear();

                var status = await _context.ScraperStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ScraperId == scraperId);

                if (status != null)
                {
                    var updatedStatus = new ScraperStatusEntity
                    {
                        ScraperId = status.ScraperId,
                        IsRunning = false, // Set to false since we're stopping due to max pages
                        StartTime = status.StartTime,
                        EndTime = DateTime.Now, // Set end time
                        UrlsProcessed = pageCount,
                        DocumentsProcessed = pageCount,
                        UrlsQueued = status.UrlsQueued,
                        HasErrors = status.HasErrors,
                        Message = $"Completed. Reached maximum of {maxPages} pages.",
                        LastStatusUpdate = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        LastMonitorCheck = status.LastMonitorCheck,
                        LastError = status.LastError ?? string.Empty
                    };

                    // Calculate elapsed time if we have a start time
                    if (status.StartTime.HasValue)
                    {
                        TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                        updatedStatus.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                    else
                    {
                        updatedStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    }

                    await UpdateScraperStatusAsync(updatedStatus);
                    Console.WriteLine($"Updated scraper status to indicate MaxPages limit reached: {maxPages} pages");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating scraper status for MaxPages reached: {ex.Message}");
                // Don't rethrow - this is a helper method
            }
        }

        #endregion
    }
}
