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

        public ScraperRepository(WebScraperDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
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
            return await _context.ScraperConfigs
                .Include(s => s.Status)
                .OrderByDescending(s => s.LastModified)
                .ToListAsync();
        }

        public async Task<ScraperConfigEntity> GetScraperByIdAsync(string id)
        {
            try
            {
                var scraperConfig = await _context.ScraperConfigs
                    .Include(s => s.Status)
                    .Include(s => s.StartUrls)
                    .Include(s => s.ContentExtractorSelectors)
                    .Include(s => s.KeywordAlerts)
                    .Include(s => s.WebhookTriggers)
                    .Include(s => s.DomainRateLimits)
                    .Include(s => s.ProxyConfigurations)
                    .Include(s => s.Schedules)
                    .FirstOrDefaultAsync(s => s.Id == id);

                // Initialize collections if they are null
                if (scraperConfig != null)
                {
                    // Initialize collections if they are null
                    // Initialize collections if they are null
                    if (scraperConfig.StartUrls == null)
                        scraperConfig.StartUrls = new List<WebScraperApi.Data.Entities.ScraperStartUrlEntity>();

                    if (scraperConfig.ContentExtractorSelectors == null)
                        scraperConfig.ContentExtractorSelectors = new List<WebScraperApi.Data.Entities.ContentExtractorSelectorEntity>();

                    if (scraperConfig.KeywordAlerts == null)
                        scraperConfig.KeywordAlerts = new List<WebScraperApi.Data.Entities.KeywordAlertEntity>();

                    if (scraperConfig.WebhookTriggers == null)
                        scraperConfig.WebhookTriggers = new List<WebScraperApi.Data.Entities.WebhookTriggerEntity>();

                    if (scraperConfig.DomainRateLimits == null)
                        scraperConfig.DomainRateLimits = new List<WebScraperApi.Data.Entities.DomainRateLimitEntity>();

                    if (scraperConfig.ProxyConfigurations == null)
                        scraperConfig.ProxyConfigurations = new List<WebScraperApi.Data.Entities.ProxyConfigurationEntity>();

                    if (scraperConfig.Schedules == null)
                        scraperConfig.Schedules = new List<WebScraperApi.Data.Entities.ScraperScheduleEntity>();
                }

                return scraperConfig;
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error retrieving scraper with ID {id}: {ex.Message}");

                // If the error is related to a missing table, return a default scraper config
                if (ex.Message.Contains("doesn't exist"))
                {
                    return await _context.ScraperConfigs
                        .FirstOrDefaultAsync(s => s.Id == id);
                }

                // Rethrow the exception
                throw;
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
                    var existingSelectors = await _context.ContentExtractorSelectors
                        .Where(s => s.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ContentExtractorSelectors.RemoveRange(existingSelectors);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing selectors: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingKeywords = await _context.KeywordAlerts
                        .Where(k => k.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.KeywordAlerts.RemoveRange(existingKeywords);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing keywords: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingWebhooks = await _context.WebhookTriggers
                        .Where(w => w.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.WebhookTriggers.RemoveRange(existingWebhooks);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing webhooks: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingDomainRateLimits = await _context.DomainRateLimits
                        .Where(d => d.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.DomainRateLimits.RemoveRange(existingDomainRateLimits);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing domain rate limits: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingProxyConfigurations = await _context.ProxyConfigurations
                        .Where(p => p.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ProxyConfigurations.RemoveRange(existingProxyConfigurations);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error removing existing proxy configurations: {ex.Message}");
                    // Continue with the update even if this fails
                }

                try
                {
                    var existingSchedules = await _context.ScraperSchedules
                        .Where(s => s.ScraperId == scraper.Id)
                        .ToListAsync();
                    _context.ScraperSchedules.RemoveRange(existingSchedules);
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

                            _context.ContentExtractorSelectors.Add(newSelector);
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

                    _context.KeywordAlerts.Add(newKeyword);
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

                    _context.WebhookTriggers.Add(newWebhook);
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

                    _context.DomainRateLimits.Add(newDomainRateLimit);
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

                    _context.ProxyConfigurations.Add(newProxyConfig);
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

                    _context.ScraperSchedules.Add(newSchedule);
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

        #endregion

        #region Scraper Run Operations

        public async Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            return await _context.ScraperRuns
                .Where(r => r.ScraperId == scraperId)
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId)
        {
            return await _context.ScraperRuns
                .Include(r => r.LogEntries)
                .Include(r => r.ContentChangeRecords)
                .Include(r => r.ProcessedDocuments)
                .FirstOrDefaultAsync(r => r.Id == runId);
        }

        public async Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRuns.Add(run);
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
            return await _context.LogEntries
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry)
        {
            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();

            return logEntry;
        }

        #endregion

        #region Content Change Record Operations

        public async Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            return await _context.ContentChangeRecords
                .Where(c => c.ScraperId == scraperId)
                .OrderByDescending(c => c.DetectedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change)
        {
            _context.ContentChangeRecords.Add(change);
            await _context.SaveChangesAsync();

            return change;
        }

        #endregion

        #region Processed Document Operations

        public async Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            return await _context.ProcessedDocuments
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.ProcessedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId)
        {
            return await _context.ProcessedDocuments
                .Include(p => p.Metadata)
                .FirstOrDefaultAsync(p => p.Id == documentId);
        }

        public async Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document)
        {
            _context.ProcessedDocuments.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        #endregion

        #region Metrics Operations

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to)
        {
            var query = _context.ScraperMetrics
                .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to);

            if (!string.IsNullOrEmpty(metricName))
            {
                query = query.Where(m => m.MetricName == metricName);
            }

            return await query
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric)
        {
            _context.ScraperMetrics.Add(metric);
            await _context.SaveChangesAsync();

            return metric;
        }

        #endregion

        #region Pipeline Metrics Operations

        public async Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId)
        {
            return await _context.PipelineMetrics
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric)
        {
            _context.PipelineMetrics.Add(metric);
            await _context.SaveChangesAsync();

            return metric;
        }

        #endregion
    }
}
