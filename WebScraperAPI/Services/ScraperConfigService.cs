using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    public class ScraperConfigService : BaseService
    {
        public ScraperConfigService(IScraperRepository repository, ILogger<ScraperConfigService> logger)
            : base(repository, logger)
        {
        }

        public async Task<List<ScraperConfigModel>> GetAllScrapersAsync()
        {
            try
            {
                var scrapers = await _repository.GetAllScrapersAsync();
                return scrapers.Select(e => MapToModel(e)).Where(m => m != null).Select(m => m!).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all scrapers");
                throw;
            }
        }

        public async Task<ScraperConfigModel?> GetScraperByIdAsync(string id)
        {
            try
            {
                var scraper = await _repository.GetScraperByIdAsync(id);
                if (scraper != null)
                {
                    return MapToModel(scraper);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting scraper with ID {ScraperId}", id);
                throw;
            }
        }

        public async Task<ScraperConfigModel> CreateScraperAsync(ScraperConfigModel model)
        {
            try
            {
                var entity = MapToEntity(model);
                if (entity != null)
                {
                    var result = await _repository.CreateScraperAsync(entity);
                    return MapToModel(result) ?? new ScraperConfigModel(); // Ensure we never return null
                }
                return new ScraperConfigModel(); // Create an empty model if mapping failed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scraper {ScraperName}", model.Name);
                throw;
            }
        }

        public async Task<ScraperConfigModel?> UpdateScraperAsync(string id, ScraperConfigModel model)
        {
            try
            {
                var existingScraper = await _repository.GetScraperByIdAsync(id);
                if (existingScraper == null)
                    return null;

                // Update properties but preserve the ID
                model.Id = id;
                var entity = MapToEntity(model);
                if (entity == null)
                {
                    _logger.LogError("Failed to map model to entity for scraper with ID {ScraperId}", id);
                    return null;
                }

                if (existingScraper != null)
                {
                    entity.CreatedAt = existingScraper.CreatedAt;
                }

                var result = await _repository.UpdateScraperAsync(entity);
                return MapToModel(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scraper with ID {ScraperId}", id);
                throw;
            }
        }

        public async Task<bool> DeleteScraperAsync(string id)
        {
            try
            {
                return await _repository.DeleteScraperAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scraper with ID {ScraperId}", id);
                throw;
            }
        }

        #region Mapping Methods

        private ScraperConfigModel? MapToModel(WebScraperApi.Data.Entities.ScraperConfigEntity entity)
        {
            if (entity == null)
                return null;

            var model = new ScraperConfigModel
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                LastModified = entity.LastModified,
                LastRun = entity.LastRun,
                RunCount = entity.RunCount,
                StartUrl = entity.StartUrl,
                BaseUrl = entity.BaseUrl,
                OutputDirectory = entity.OutputDirectory,
                DelayBetweenRequests = entity.DelayBetweenRequests,
                MaxConcurrentRequests = entity.MaxConcurrentRequests,
                MaxDepth = entity.MaxDepth,
                MaxPages = entity.MaxPages,
                FollowLinks = entity.FollowLinks,
                FollowExternalLinks = entity.FollowExternalLinks,
                RespectRobotsTxt = entity.RespectRobotsTxt,
                AutoLearnHeaderFooter = entity.AutoLearnHeaderFooter,
                LearningPagesCount = entity.LearningPagesCount,
                EnableChangeDetection = entity.EnableChangeDetection,
                TrackContentVersions = entity.TrackContentVersions,
                MaxVersionsToKeep = entity.MaxVersionsToKeep,
                EnableAdaptiveCrawling = entity.EnableAdaptiveCrawling,
                PriorityQueueSize = entity.PriorityQueueSize,
                AdjustDepthBasedOnQuality = entity.AdjustDepthBasedOnQuality,
                EnableAdaptiveRateLimiting = entity.EnableAdaptiveRateLimiting,
                MinDelayBetweenRequests = entity.MinDelayBetweenRequests,
                MaxDelayBetweenRequests = entity.MaxDelayBetweenRequests,
                MonitorResponseTimes = entity.MonitorResponseTimes,
                MaxRequestsPerMinute = entity.MaxRequestsPerMinute,
                UserAgent = entity.UserAgent,
                BackOffOnErrors = entity.BackOffOnErrors,
                UseProxies = entity.UseProxies,
                ProxyRotationStrategy = entity.ProxyRotationStrategy,
                TestProxiesBeforeUse = entity.TestProxiesBeforeUse,
                MaxProxyFailuresBeforeRemoval = entity.MaxProxyFailuresBeforeRemoval,
                EnableContinuousMonitoring = entity.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = entity.MonitoringIntervalMinutes,
                NotifyOnChanges = entity.NotifyOnChanges,
                NotificationEmail = entity.NotificationEmail,
                TrackChangesHistory = entity.TrackChangesHistory,
                EnableRegulatoryContentAnalysis = entity.EnableRegulatoryContentAnalysis,
                TrackRegulatoryChanges = entity.TrackRegulatoryChanges,
                ClassifyRegulatoryDocuments = entity.ClassifyRegulatoryDocuments,
                ExtractStructuredContent = entity.ExtractStructuredContent,
                ProcessPdfDocuments = entity.ProcessPdfDocuments,
                MonitorHighImpactChanges = entity.MonitorHighImpactChanges,
                ExtractMetadata = entity.ExtractMetadata,
                ExtractStructuredData = entity.ExtractStructuredData,
                CustomJsExtractor = entity.CustomJsExtractor,
                WaitForSelector = entity.WaitForSelector,
                IsUKGCWebsite = entity.IsUKGCWebsite,
                PrioritizeEnforcementActions = entity.PrioritizeEnforcementActions,
                PrioritizeLCCP = entity.PrioritizeLCCP,
                PrioritizeAML = entity.PrioritizeAML,
                NotificationEndpoint = entity.NotificationEndpoint,
                WebhookEnabled = entity.WebhookEnabled,
                WebhookUrl = entity.WebhookUrl,
                NotifyOnContentChanges = entity.NotifyOnContentChanges,
                NotifyOnDocumentProcessed = entity.NotifyOnDocumentProcessed,
                NotifyOnScraperStatusChange = entity.NotifyOnScraperStatusChange,
                WebhookFormat = entity.WebhookFormat,
                EnableContentCompression = entity.EnableContentCompression,
                CompressionThresholdBytes = entity.CompressionThresholdBytes,
                CollectDetailedMetrics = entity.CollectDetailedMetrics,
                MetricsReportingIntervalSeconds = entity.MetricsReportingIntervalSeconds,
                TrackDomainMetrics = entity.TrackDomainMetrics,
                ScraperType = entity.ScraperType
            };

            // Map collections
            if (entity.StartUrls != null)
            {
                model.StartUrls = entity.StartUrls.Select(u => u.Url).ToList();
            }

            if (entity.ContentExtractorSelectors != null)
            {
                model.ContentExtractorSelectors = entity.ContentExtractorSelectors
                    .Where(s => !s.IsExclude)
                    .Select(s => s.Selector)
                    .ToList();

                model.ContentExtractorExcludeSelectors = entity.ContentExtractorSelectors
                    .Where(s => s.IsExclude)
                    .Select(s => s.Selector)
                    .ToList();
            }

            if (entity.KeywordAlerts != null)
            {
                model.KeywordAlertList = entity.KeywordAlerts.Select(k => k.Keyword).ToList();
            }

            if (entity.WebhookTriggers != null)
            {
                model.WebhookTriggers = entity.WebhookTriggers.Select(t => t.TriggerName).ToList();
            }

            if (entity.Schedules != null)
            {
                model.Schedules = entity.Schedules.Select(s => new Dictionary<string, object>
                {
                    ["name"] = s.Name,
                    ["cronExpression"] = s.CronExpression,
                    ["isActive"] = s.IsActive,
                    ["lastRun"] = s.LastRun ?? DateTime.MinValue,
                    ["nextRun"] = s.NextRun ?? DateTime.MinValue,
                    ["maxRuntimeMinutes"] = s.MaxRuntimeMinutes ?? 0,
                    ["notificationEmail"] = s.NotificationEmail
                }).ToList();
            }

            return model;
        }

        private WebScraperApi.Data.Entities.ScraperConfigEntity? MapToEntity(ScraperConfigModel model)
        {
            if (model == null)
                return null;

            var entity = new WebScraperApi.Data.Entities.ScraperConfigEntity
            {
                Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                LastModified = DateTime.Now,
                LastRun = model.LastRun,
                RunCount = model.RunCount,
                StartUrl = model.StartUrl,
                BaseUrl = model.BaseUrl,
                OutputDirectory = model.OutputDirectory,
                DelayBetweenRequests = model.DelayBetweenRequests,
                MaxConcurrentRequests = model.MaxConcurrentRequests,
                MaxDepth = model.MaxDepth,
                MaxPages = model.MaxPages,
                FollowLinks = model.FollowLinks,
                FollowExternalLinks = model.FollowExternalLinks,
                RespectRobotsTxt = model.RespectRobotsTxt,
                AutoLearnHeaderFooter = model.AutoLearnHeaderFooter,
                LearningPagesCount = model.LearningPagesCount,
                EnableChangeDetection = model.EnableChangeDetection,
                TrackContentVersions = model.TrackContentVersions,
                MaxVersionsToKeep = model.MaxVersionsToKeep,
                EnableAdaptiveCrawling = model.EnableAdaptiveCrawling,
                PriorityQueueSize = model.PriorityQueueSize,
                AdjustDepthBasedOnQuality = model.AdjustDepthBasedOnQuality,
                EnableAdaptiveRateLimiting = model.EnableAdaptiveRateLimiting,
                MinDelayBetweenRequests = model.MinDelayBetweenRequests,
                MaxDelayBetweenRequests = model.MaxDelayBetweenRequests,
                MonitorResponseTimes = model.MonitorResponseTimes,
                MaxRequestsPerMinute = model.MaxRequestsPerMinute,
                UserAgent = model.UserAgent,
                BackOffOnErrors = model.BackOffOnErrors,
                UseProxies = model.UseProxies,
                ProxyRotationStrategy = model.ProxyRotationStrategy,
                TestProxiesBeforeUse = model.TestProxiesBeforeUse,
                MaxProxyFailuresBeforeRemoval = model.MaxProxyFailuresBeforeRemoval,
                EnableContinuousMonitoring = model.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = model.MonitoringIntervalMinutes,
                NotifyOnChanges = model.NotifyOnChanges,
                NotificationEmail = model.NotificationEmail ?? string.Empty,
                TrackChangesHistory = model.TrackChangesHistory,
                EnableRegulatoryContentAnalysis = model.EnableRegulatoryContentAnalysis,
                TrackRegulatoryChanges = model.TrackRegulatoryChanges,
                ClassifyRegulatoryDocuments = model.ClassifyRegulatoryDocuments,
                ExtractStructuredContent = model.ExtractStructuredContent,
                ProcessPdfDocuments = model.ProcessPdfDocuments,
                MonitorHighImpactChanges = model.MonitorHighImpactChanges,
                ExtractMetadata = model.ExtractMetadata,
                ExtractStructuredData = model.ExtractStructuredData,
                CustomJsExtractor = model.CustomJsExtractor ?? string.Empty,
                WaitForSelector = model.WaitForSelector ?? string.Empty,
                IsUKGCWebsite = model.IsUKGCWebsite,
                PrioritizeEnforcementActions = model.PrioritizeEnforcementActions,
                PrioritizeLCCP = model.PrioritizeLCCP,
                PrioritizeAML = model.PrioritizeAML,
                NotificationEndpoint = model.NotificationEndpoint ?? string.Empty,
                WebhookEnabled = model.WebhookEnabled,
                WebhookUrl = model.WebhookUrl ?? string.Empty,
                NotifyOnContentChanges = model.NotifyOnContentChanges,
                NotifyOnDocumentProcessed = model.NotifyOnDocumentProcessed,
                NotifyOnScraperStatusChange = model.NotifyOnScraperStatusChange,
                WebhookFormat = model.WebhookFormat,
                EnableContentCompression = model.EnableContentCompression,
                CompressionThresholdBytes = model.CompressionThresholdBytes,
                CollectDetailedMetrics = model.CollectDetailedMetrics,
                MetricsReportingIntervalSeconds = model.MetricsReportingIntervalSeconds,
                TrackDomainMetrics = model.TrackDomainMetrics,
                ScraperType = model.ScraperType,
                StartUrls = new List<WebScraperApi.Data.Entities.ScraperStartUrlEntity>(),
                ContentExtractorSelectors = new List<WebScraperApi.Data.Entities.ContentExtractorSelectorEntity>(),
                KeywordAlerts = new List<WebScraperApi.Data.Entities.KeywordAlertEntity>(),
                WebhookTriggers = new List<WebScraperApi.Data.Entities.WebhookTriggerEntity>(),
                Schedules = new List<WebScraperApi.Data.Entities.ScraperScheduleEntity>()
            };

            // Map collections
            if (model.StartUrls != null)
            {
                entity.StartUrls = model.StartUrls.Select(url => new WebScraperApi.Data.Entities.ScraperStartUrlEntity
                {
                    ScraperId = entity.Id,
                    Url = url
                }).ToList();
            }

            if (model.ContentExtractorSelectors != null)
            {
                foreach (var selector in model.ContentExtractorSelectors)
                {
                    entity.ContentExtractorSelectors.Add(new WebScraperApi.Data.Entities.ContentExtractorSelectorEntity
                    {
                        ScraperId = entity.Id,
                        Selector = selector,
                        IsExclude = false
                    });
                }
            }

            if (model.ContentExtractorExcludeSelectors != null)
            {
                foreach (var selector in model.ContentExtractorExcludeSelectors)
                {
                    entity.ContentExtractorSelectors.Add(new WebScraperApi.Data.Entities.ContentExtractorSelectorEntity
                    {
                        ScraperId = entity.Id,
                        Selector = selector,
                        IsExclude = true
                    });
                }
            }

            if (model.KeywordAlertList != null)
            {
                entity.KeywordAlerts = model.KeywordAlertList.Select(keyword => new WebScraperApi.Data.Entities.KeywordAlertEntity
                {
                    ScraperId = entity.Id,
                    Keyword = keyword
                }).ToList();
            }

            if (model.WebhookTriggers != null)
            {
                entity.WebhookTriggers = model.WebhookTriggers.Select(trigger => new WebScraperApi.Data.Entities.WebhookTriggerEntity
                {
                    ScraperId = entity.Id,
                    TriggerName = trigger
                }).ToList();
            }

            if (model.Schedules != null)
            {
                foreach (var schedule in model.Schedules)
                {
                    entity.Schedules.Add(new WebScraperApi.Data.Entities.ScraperScheduleEntity
                    {
                        ScraperId = entity.Id,
                        Name = schedule.ContainsKey("name") ? schedule["name"]?.ToString() ?? "Default Schedule" : "Default Schedule",
                        CronExpression = schedule.ContainsKey("cronExpression") ? schedule["cronExpression"]?.ToString() ?? "0 0 * * *" : "0 0 * * *",
                        IsActive = schedule.ContainsKey("isActive") && schedule["isActive"] is bool isActive ? isActive : true,
                        LastRun = schedule.ContainsKey("lastRun") && schedule["lastRun"] is DateTime lastRun ? lastRun : null,
                        NextRun = schedule.ContainsKey("nextRun") && schedule["nextRun"] is DateTime nextRun ? nextRun : null,
                        MaxRuntimeMinutes = schedule.ContainsKey("maxRuntimeMinutes") && schedule["maxRuntimeMinutes"] is int maxRuntime ? maxRuntime : null,
                        NotificationEmail = schedule.ContainsKey("notificationEmail") ? schedule["notificationEmail"]?.ToString() ?? string.Empty : string.Empty
                    });
                }
            }

            return entity;
        }

        #endregion
    }
}
