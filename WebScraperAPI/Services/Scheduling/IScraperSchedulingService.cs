using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Scheduling
{
    /// <summary>
    /// Interface for scraper scheduling services
    /// </summary>
    public interface IScraperSchedulingService
    {
        /// <summary>
        /// Gets all scheduled items
        /// </summary>
        /// <returns>Collection of scheduled items</returns>
        IEnumerable<ScheduleItem> GetAllScheduledItems();

        /// <summary>
        /// Gets scheduled items for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <returns>Collection of scheduled items</returns>
        IEnumerable<ScheduleItem> GetScheduledItems(string scraperId);

        /// <summary>
        /// Schedules a scraper to run at a specific time
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="schedule">Schedule configuration</param>
        /// <returns>Created schedule item</returns>
        Task<ScheduleItem> ScheduleScraper(string scraperId, ScheduleConfig schedule);

        /// <summary>
        /// Updates an existing schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <param name="schedule">Updated schedule configuration</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateSchedule(string scheduleId, ScheduleConfig schedule);

        /// <summary>
        /// Removes a schedule
        /// </summary>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveSchedule(string scheduleId);

        /// <summary>
        /// Schedules a scraper to run with the specified options
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="options">Schedule options</param>
        /// <returns>Result object with schedule information</returns>
        Task<object> ScheduleScraperAsync(string scraperId, ScheduleOptions options);

        /// <summary>
        /// Gets all schedules for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <returns>Collection of schedule objects</returns>
        Task<IEnumerable<object>> GetSchedulesAsync(string scraperId);

        /// <summary>
        /// Removes a schedule for a specific scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="scheduleId">Schedule ID</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RemoveScheduleAsync(string scraperId, string scheduleId);

        /// <summary>
        /// Gets scrapers that need to run
        /// </summary>
        /// <returns>Collection of scraper IDs that need to run</returns>
        Task<List<string>> GetScrapersToRun();
    }

    /// <summary>
    /// Schedule configuration for a scraper
    /// </summary>
    public class ScheduleConfig
    {
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
    }
}