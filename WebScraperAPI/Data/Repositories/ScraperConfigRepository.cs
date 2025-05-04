using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class ScraperConfigRepository : IScraperConfigRepository
    {
        private readonly WebScraperDbContext _context;

        public ScraperConfigRepository(WebScraperDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

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
                Console.WriteLine($"GetScraperByIdAsync: Attempting to fetch scraper with ID {id}");

                // Get the main scraper config without any includes
                var scraperConfig = await _context.ScraperConfigs
                    .AsNoTracking()  // Use AsNoTracking for better performance
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (scraperConfig == null)
                {
                    Console.WriteLine($"GetScraperByIdAsync: Scraper with ID {id} not found");
                    return null;
                }

                Console.WriteLine($"GetScraperByIdAsync: Successfully retrieved scraper with ID {id}");
                return scraperConfig;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"GetScraperByIdAsync: Error retrieving scraper with ID {id}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GetScraperByIdAsync: Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"GetScraperByIdAsync: Stack trace: {ex.StackTrace}");

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
    }
}