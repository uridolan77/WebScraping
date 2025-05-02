using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WebScraperApi.Models;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service for managing scraper configurations
    /// </summary>
    public class ScraperConfigurationService
    {
        private readonly ILogger _logger;
        private readonly IScraperConfigRepository _configRepository;
        private readonly string _configFilePath = "scraperConfigs.json";

        public ScraperConfigurationService(ILogger logger, IScraperConfigRepository configRepository)
        {
            _logger = logger;
            _configRepository = configRepository;
        }

        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        /// <returns>A collection of scraper configurations</returns>
        public async Task<IEnumerable<ScraperConfigModel>> GetAllScraperConfigsAsync()
        {
            try
            {
                var configs = await _configRepository.GetAllAsync();
                return configs.Select(ToModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all scraper configurations from database");
                throw;
            }
        }

        /// <summary>
        /// Gets a specific scraper configuration by ID
        /// </summary>
        public async Task<ScraperConfigModel> GetScraperConfigAsync(string id)
        {
            try
            {
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return null;
                }

                var entity = await _configRepository.GetByIdAsync(guidId);
                if (entity == null)
                {
                    return null;
                }

                return ToModel(entity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting scraper configuration with ID {id} from database");
                throw;
            }
        }

        /// <summary>
        /// Creates a new scraper configuration
        /// </summary>
        public async Task<ScraperConfigModel> CreateScraperConfigAsync(ScraperConfigModel config)
        {
            try
            {
                // Generate a new ID if none provided
                if (string.IsNullOrEmpty(config.Id))
                {
                    config.Id = Guid.NewGuid().ToString();
                }

                // Ensure creation date is set
                if (config.CreatedAt == default)
                {
                    config.CreatedAt = DateTime.Now;
                }

                // Convert to entity for the repository
                var entity = ToEntity(config);

                // Save to repository
                await _configRepository.CreateAsync(entity);

                // Get the saved entity with generated ID
                config.Id = entity.Id.ToString();

                _logger.LogInformation($"Created scraper configuration: {config.Name} ({config.Id})");
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating scraper configuration '{config.Name}'");
                throw;
            }
        }

        /// <summary>
        /// Updates an existing scraper configuration
        /// </summary>
        public async Task<bool> UpdateScraperConfigAsync(string id, ScraperConfigModel config)
        {
            try
            {
                // Ensure the ID stays the same
                config.Id = id;

                // Convert to entity for the repository
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return false;
                }

                // Get the existing entity
                var existingEntity = await _configRepository.GetByIdAsync(guidId);
                if (existingEntity == null)
                {
                    _logger.LogWarning($"Scraper with ID {id} not found in database");
                    return false;
                }

                // Update entity properties
                var entity = ToEntity(config);

                // Update in repository
                await _configRepository.UpdateAsync(entity);

                _logger.LogInformation($"Updated scraper configuration: {config.Name} ({config.Id})");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating scraper configuration with ID {id}");
                return false;
            }
        }

        /// <summary>
        /// Deletes a scraper configuration
        /// </summary>
        public async Task<bool> DeleteScraperConfigAsync(string id)
        {
            try
            {
                // Parse the ID to GUID
                if (!Guid.TryParse(id, out var guidId))
                {
                    _logger.LogWarning($"Invalid GUID format for scraper ID: {id}");
                    return false;
                }

                // Delete from repository
                var entity = await _configRepository.GetByIdAsync(guidId);
                if (entity == null)
                {
                    _logger.LogWarning($"Scraper with ID {id} not found in database");
                    return false;
                }

                await _configRepository.DeleteAsync(guidId);
                _logger.LogInformation($"Deleted scraper configuration with ID {id}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting scraper configuration with ID {id}");
                return false;
            }
        }

        /// <summary>
        /// Loads scraper configurations from the database with fallback to file
        /// </summary>
        public async Task<List<ScraperConfigModel>> LoadScraperConfigurationsAsync()
        {
            try
            {
                var configs = await _configRepository.GetAllAsync();
                var configModels = configs.Select(ToModel).ToList();
                _logger.LogInformation($"Loaded {configModels.Count} scraper configurations from database");
                return configModels;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading scraper configurations from database");

                // Fallback to file-based configuration if database fails
                if (File.Exists(_configFilePath))
                {
                    try
                    {
                        var json = File.ReadAllText(_configFilePath);
                        var configs = JsonConvert.DeserializeObject<List<ScraperConfigModel>>(json);

                        if (configs != null)
                        {
                            _logger.LogInformation($"Loaded {configs.Count} scraper configurations from file");
                            return configs;
                        }
                    }
                    catch (Exception fileEx)
                    {
                        _logger.LogError(fileEx, "Error loading scraper configurations from file");
                    }
                }

                return new List<ScraperConfigModel>();
            }
        }

        /// <summary>
        /// Saves scraper configurations to a file (used as backup)
        /// </summary>
        public void SaveScraperConfigurationsToFile(IEnumerable<ScraperConfigModel> configs)
        {
            try
            {
                var configsList = configs.ToList();
                var json = JsonConvert.SerializeObject(configsList, Formatting.Indented);
                File.WriteAllText(_configFilePath, json);
                _logger.LogInformation($"Saved {configsList.Count} scraper configurations to file");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving scraper configurations to file");
            }
        }

        /// <summary>
        /// Converts a ScraperConfigEntity entity to a ScraperConfigModel
        /// </summary>
        private ScraperConfigModel ToModel(ScraperConfigEntity entity)
        {
            return new ScraperConfigModel
            {
                Id = entity.Id.ToString(),
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                LastRun = entity.LastRun,
                StartUrl = entity.StartUrl,
                BaseUrl = entity.BaseUrl,
                OutputDirectory = entity.OutputDirectory,
                DelayBetweenRequests = entity.DelayBetweenRequests,
                MaxConcurrentRequests = entity.MaxConcurrentRequests,
                MaxDepth = entity.MaxDepth,
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
                EnableContinuousMonitoring = entity.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = entity.MonitoringIntervalMinutes,
                NotifyOnChanges = entity.NotifyOnChanges,
                NotificationEmail = entity.NotificationEmail,
                TrackChangesHistory = entity.TrackChangesHistory
            };
        }

        /// <summary>
        /// Converts a ScraperConfigModel to a ScraperConfigEntity entity
        /// </summary>
        private ScraperConfigEntity ToEntity(ScraperConfigModel model)
        {
            return new ScraperConfigEntity
            {
                Id = model.Id,
                Name = model.Name,
                CreatedAt = model.CreatedAt,
                LastRun = model.LastRun,
                StartUrl = model.StartUrl,
                BaseUrl = model.BaseUrl,
                OutputDirectory = model.OutputDirectory,
                DelayBetweenRequests = model.DelayBetweenRequests,
                MaxConcurrentRequests = model.MaxConcurrentRequests,
                MaxDepth = model.MaxDepth,
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
                EnableContinuousMonitoring = model.EnableContinuousMonitoring,
                MonitoringIntervalMinutes = model.MonitoringIntervalMinutes,
                NotifyOnChanges = model.NotifyOnChanges,
                NotificationEmail = model.NotificationEmail,
                TrackChangesHistory = model.TrackChangesHistory
            };
        }
    }
}