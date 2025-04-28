using Microsoft.Extensions.Logging;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Scheduling
{
    /// <summary>
    /// Service for scheduling scraper operations
    /// </summary>
    public class ScraperSchedulingService : IScraperSchedulingService
    {
        private readonly ILogger<ScraperSchedulingService> _logger;
        private readonly Dictionary<string, List<ScheduleItem>> _scheduleStore;
        private readonly object _lock = new object();

        public ScraperSchedulingService(ILogger<ScraperSchedulingService> logger)
        {
            _logger = logger;
            _scheduleStore = new Dictionary<string, List<ScheduleItem>>();
        }

        /// <summary>
        /// Gets all scheduled items
        /// </summary>
        /// <returns>Collection of scheduled items</returns>
        public IEnumerable<ScheduleItem> GetAllScheduledItems()
        {
            lock (_lock)
            {
                return _scheduleStore
                    .SelectMany(kv => kv.Value)
                    .ToList();
            }
        }

        /// <summary>
        /// Gets scheduled items for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <returns>Collection of scheduled items</returns>
        public IEnumerable<ScheduleItem> GetScheduledItems(string scraperId)
        {
            lock (_lock)
            {
                if (_scheduleStore.TryGetValue(scraperId, out var items))
                {
                    return items.ToList();
                }
            }

            return new List<ScheduleItem>();
        }

        /// <summary>
        /// Schedules a scraper to run at a specific time
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="schedule">Schedule configuration</param>
        /// <returns>Created schedule item</returns>
        public Task<ScheduleItem> ScheduleScraper(string scraperId, ScheduleConfig schedule)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                throw new ArgumentException("Scraper ID cannot be null or empty", nameof(scraperId));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            var scheduleItem = new ScheduleItem
            {
                Id = Guid.NewGuid().ToString(),
                ScraperId = scraperId,
                Name = schedule.Name,
                CronExpression = schedule.CronExpression,
                Enabled = schedule.Enabled,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            try
            {
                // Parse the cron expression to validate and calculate next occurrence
                var schedule_expression = CrontabSchedule.Parse(schedule.CronExpression);
                scheduleItem.NextRunTime = schedule_expression.GetNextOccurrence(DateTime.Now);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Invalid cron expression: {schedule.CronExpression}");
                throw new ArgumentException($"Invalid cron expression: {ex.Message}", nameof(schedule.CronExpression), ex);
            }

            lock (_lock)
            {
                if (!_scheduleStore.ContainsKey(scraperId))
                {
                    _scheduleStore[scraperId] = new List<ScheduleItem>();
                }

                _scheduleStore[scraperId].Add(scheduleItem);
            }

            _logger.LogInformation($"Scheduled scraper {scraperId} with schedule '{schedule.Name}', next run at {scheduleItem.NextRunTime}");

            return Task.FromResult(scheduleItem);
        }

        /// <summary>
        /// Updates an existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="schedule">Updated schedule configuration</param>
        /// <returns>True if successful, false otherwise</returns>
        public Task<bool> UpdateSchedule(string scheduleId, ScheduleConfig schedule)
        {
            if (string.IsNullOrEmpty(scheduleId))
            {
                throw new ArgumentException("Schedule ID cannot be null or empty", nameof(scheduleId));
            }

            if (schedule == null)
            {
                throw new ArgumentNullException(nameof(schedule));
            }

            lock (_lock)
            {
                foreach (var scheduleList in _scheduleStore.Values)
                {
                    var existingItem = scheduleList.FirstOrDefault(s => s.Id == scheduleId);
                    if (existingItem != null)
                    {
                        existingItem.Name = schedule.Name;
                        existingItem.CronExpression = schedule.CronExpression;
                        existingItem.Enabled = schedule.Enabled;
                        existingItem.UpdatedAt = DateTime.Now;

                        try
                        {
                            // Parse the cron expression to validate and calculate next occurrence
                            var schedule_expression = CrontabSchedule.Parse(schedule.CronExpression);
                            existingItem.NextRunTime = schedule_expression.GetNextOccurrence(DateTime.Now);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Invalid cron expression: {schedule.CronExpression}");
                            throw new ArgumentException($"Invalid cron expression: {ex.Message}", nameof(schedule.CronExpression), ex);
                        }

                        _logger.LogInformation($"Updated schedule {scheduleId}, next run at {existingItem.NextRunTime}");
                        return Task.FromResult(true);
                    }
                }
            }

            _logger.LogWarning($"Failed to update schedule {scheduleId}: not found");
            return Task.FromResult(false);
        }

        /// <summary>
        /// Removes a schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public Task<bool> RemoveSchedule(string scheduleId)
        {
            if (string.IsNullOrEmpty(scheduleId))
            {
                throw new ArgumentException("Schedule ID cannot be null or empty", nameof(scheduleId));
            }

            lock (_lock)
            {
                foreach (var key in _scheduleStore.Keys)
                {
                    var scheduleList = _scheduleStore[key];
                    var existingItem = scheduleList.FirstOrDefault(s => s.Id == scheduleId);
                    if (existingItem != null)
                    {
                        scheduleList.Remove(existingItem);
                        _logger.LogInformation($"Removed schedule {scheduleId}");
                        return Task.FromResult(true);
                    }
                }
            }

            _logger.LogWarning($"Failed to remove schedule {scheduleId}: not found");
            return Task.FromResult(false);
        }

        /// <summary>
        /// Gets scrapers that need to run according to their schedules
        /// </summary>
        /// <returns>Collection of scraper IDs that need to run</returns>
        public Task<List<string>> GetScrapersToRun()
        {
            var now = DateTime.Now;
            var scrapersToRun = new List<string>();

            lock (_lock)
            {
                foreach (var kvp in _scheduleStore)
                {
                    var scraperId = kvp.Key;
                    var schedules = kvp.Value.Where(s => s.Enabled && s.NextRunTime.HasValue);

                    foreach (var schedule in schedules)
                    {
                        if (schedule.NextRunTime <= now)
                        {
                            // Add the scraper to the list of scrapers to run
                            if (!scrapersToRun.Contains(scraperId))
                            {
                                scrapersToRun.Add(scraperId);
                                _logger.LogInformation($"Scraper {scraperId} scheduled to run (schedule: {schedule.Name})");
                            }

                            // Update the next run time
                            try
                            {
                                var cronSchedule = CrontabSchedule.Parse(schedule.CronExpression);
                                schedule.LastRunTime = now;
                                schedule.NextRunTime = cronSchedule.GetNextOccurrence(now);
                                _logger.LogInformation($"Updated next run time for schedule {schedule.Id} to {schedule.NextRunTime}");
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, $"Error updating next run time for schedule {schedule.Id}");
                            }
                        }
                    }
                }
            }

            return Task.FromResult(scrapersToRun);
        }

        /// <summary>
        /// Schedules a scraper to run with the specified options
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="options">Schedule options</param>
        /// <returns>Result object with schedule information</returns>
        public async Task<object> ScheduleScraperAsync(string scraperId, ScheduleOptions options)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                throw new ArgumentException("Scraper ID cannot be null or empty", nameof(scraperId));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            var scheduleConfig = new ScheduleConfig
            {
                Name = options.ScheduleName ?? "Scheduled Run",
                CronExpression = options.CronExpression ?? "0 0 * * *", // Default to daily at midnight
                Enabled = options.IsRecurring // Use IsRecurring as Enabled
            };

            try
            {
                var scheduleItem = await ScheduleScraper(scraperId, scheduleConfig);

                return new
                {
                    Success = true,
                    Message = $"Scraper scheduled successfully",
                    ScheduleId = scheduleItem.Id,
                    NextRunTime = scheduleItem.NextRunTime
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error scheduling scraper {scraperId}");
                return new
                {
                    Success = false,
                    Message = $"Error scheduling scraper: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Gets all schedules for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <returns>Collection of schedule objects</returns>
        public Task<IEnumerable<object>> GetSchedulesAsync(string scraperId)
        {
            var schedules = GetScheduledItems(scraperId);

            var result = schedules.Select(s => new
            {
                Id = s.Id,
                Name = s.Name,
                CronExpression = s.CronExpression,
                Enabled = s.Enabled,
                NextRunTime = s.NextRunTime,
                LastRunTime = s.LastRunTime,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            }).Cast<object>();

            return Task.FromResult(result);
        }

        /// <summary>
        /// Removes a schedule for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RemoveScheduleAsync(string scraperId, string scheduleId)
        {
            if (string.IsNullOrEmpty(scraperId))
            {
                throw new ArgumentException("Scraper ID cannot be null or empty", nameof(scraperId));
            }

            if (string.IsNullOrEmpty(scheduleId))
            {
                throw new ArgumentException("Schedule ID cannot be null or empty", nameof(scheduleId));
            }

            // First check if the scraper exists and has this schedule
            lock (_lock)
            {
                if (!_scheduleStore.TryGetValue(scraperId, out var schedules))
                {
                    _logger.LogWarning($"Cannot remove schedule: scraper {scraperId} not found");
                    return false;
                }

                if (!schedules.Any(s => s.Id == scheduleId))
                {
                    _logger.LogWarning($"Cannot remove schedule: schedule {scheduleId} not found for scraper {scraperId}");
                    return false;
                }
            }

            // Use the existing RemoveSchedule method
            return await RemoveSchedule(scheduleId);
        }
    }

    /// <summary>
    /// Represents a scheduled item
    /// </summary>
    public class ScheduleItem
    {
        /// <summary>
        /// Schedule ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Scraper ID
        /// </summary>
        public string ScraperId { get; set; }

        /// <summary>
        /// Schedule name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Cron expression for the schedule
        /// </summary>
        public string CronExpression { get; set; }

        /// <summary>
        /// Enable or disable the schedule
        /// </summary>
        public bool Enabled { get; set; } = true;

        /// <summary>
        /// Next run time
        /// </summary>
        public DateTime? NextRunTime { get; set; }

        /// <summary>
        /// Last run time
        /// </summary>
        public DateTime? LastRunTime { get; set; }

        /// <summary>
        /// When the schedule was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When the schedule was last updated
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}