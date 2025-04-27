using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Notifications
{
    /// <summary>
    /// Interface for webhook notification services
    /// </summary>
    public interface IWebhookNotificationService
    {
        /// <summary>
        /// Sends a notification about a detected change
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="url">URL where the change was detected</param>
        /// <param name="changeData">Data about the change</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        Task<bool> NotifyContentChangeAsync(string scraperId, string url, object changeData);
        
        /// <summary>
        /// Sends a notification about scraper status changes
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="status">New status</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        Task<bool> NotifyScraperStatusAsync(string scraperId, ScraperStatus status);
        
        /// <summary>
        /// Sends a notification with scraper results
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="results">Scraper results</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        Task<bool> NotifyScraperResultsAsync(string scraperId, object results);
        
        /// <summary>
        /// Sends a custom notification
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="eventType">Event type</param>
        /// <param name="data">Notification data</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        Task<bool> SendCustomNotificationAsync(string scraperId, string eventType, object data);
    }
}