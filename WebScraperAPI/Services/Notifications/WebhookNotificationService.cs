using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebScraperApi.Models;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services.Notifications
{
    /// <summary>
    /// Service for sending webhook notifications
    /// </summary>
    public class WebhookNotificationService : IWebhookNotificationService
    {
        private readonly ILogger<WebhookNotificationService> _logger;
        private readonly IScraperStateService _stateService;
        private readonly HttpClient _httpClient;
        
        public WebhookNotificationService(
            ILogger<WebhookNotificationService> logger,
            IScraperStateService stateService,
            HttpClient httpClient = null)
        {
            _logger = logger;
            _stateService = stateService;
            _httpClient = httpClient ?? new HttpClient();
            
            // Set default timeout for webhooks (to avoid waiting too long)
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }
        
        /// <summary>
        /// Sends a notification about a detected change
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="url">URL where the change was detected</param>
        /// <param name="changeData">Data about the change</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        public async Task<bool> NotifyContentChangeAsync(string scraperId, string url, object changeData)
        {
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null || !instance.Config.WebhookEnabled || string.IsNullOrEmpty(instance.Config.WebhookUrl))
            {
                return false;
            }
            
            // Check if content change notifications are enabled
            var triggers = instance.Config.WebhookTriggers ?? new List<string>();
            if (!triggers.Contains("content_change") && !triggers.Contains("all"))
            {
                return false;
            }
            
            var payload = new
            {
                eventType = "content_change",
                scraperId = scraperId,
                scraperName = instance.Config.Name,
                timestamp = DateTime.Now,
                url = url,
                changeDetails = changeData
            };
            
            return await SendWebhookAsync(instance.Config, payload);
        }
        
        /// <summary>
        /// Sends a notification about scraper status changes
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="status">New status</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        public async Task<bool> NotifyScraperStatusAsync(string scraperId, ScraperStatus status)
        {
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null || !instance.Config.WebhookEnabled || string.IsNullOrEmpty(instance.Config.WebhookUrl))
            {
                return false;
            }
            
            // Check if status notifications are enabled
            var triggers = instance.Config.WebhookTriggers ?? new List<string>();
            if (!triggers.Contains("status_change") && !triggers.Contains("all"))
            {
                return false;
            }
            
            var payload = new
            {
                eventType = "status_change",
                scraperId = scraperId,
                scraperName = instance.Config.Name,
                timestamp = DateTime.Now,
                status = new
                {
                    isRunning = status.IsRunning,
                    message = status.Message,
                    startTime = status.StartTime,
                    endTime = status.EndTime,
                    hasErrors = status.HasErrors,
                    lastError = status.LastError,
                    urlsProcessed = status.UrlsProcessed
                }
            };
            
            return await SendWebhookAsync(instance.Config, payload);
        }
        
        /// <summary>
        /// Sends a notification with scraper results
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="results">Scraper results</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        public async Task<bool> NotifyScraperResultsAsync(string scraperId, object results)
        {
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null || !instance.Config.WebhookEnabled || string.IsNullOrEmpty(instance.Config.WebhookUrl))
            {
                return false;
            }
            
            // Check if results notifications are enabled
            var triggers = instance.Config.WebhookTriggers ?? new List<string>();
            if (!triggers.Contains("results") && !triggers.Contains("all"))
            {
                return false;
            }
            
            var payload = new
            {
                eventType = "results",
                scraperId = scraperId,
                scraperName = instance.Config.Name,
                timestamp = DateTime.Now,
                results = results
            };
            
            return await SendWebhookAsync(instance.Config, payload);
        }
        
        /// <summary>
        /// Sends a custom notification
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="eventType">Event type</param>
        /// <param name="data">Notification data</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        public async Task<bool> SendCustomNotificationAsync(string scraperId, string eventType, object data)
        {
            var instance = _stateService.GetScraperInstance(scraperId);
            if (instance == null || !instance.Config.WebhookEnabled || string.IsNullOrEmpty(instance.Config.WebhookUrl))
            {
                return false;
            }
            
            // Check if custom notifications are enabled
            var triggers = instance.Config.WebhookTriggers ?? new List<string>();
            if (!triggers.Contains("custom") && !triggers.Contains("all"))
            {
                return false;
            }
            
            var payload = new
            {
                eventType = eventType,
                scraperId = scraperId,
                scraperName = instance.Config.Name,
                timestamp = DateTime.Now,
                data = data
            };
            
            return await SendWebhookAsync(instance.Config, payload);
        }
        
        /// <summary>
        /// Sends a webhook notification
        /// </summary>
        /// <param name="config">Scraper configuration</param>
        /// <param name="payload">Notification payload</param>
        /// <returns>True if notification was sent successfully, false otherwise</returns>
        private async Task<bool> SendWebhookAsync(ScraperConfigModel config, object payload)
        {
            try
            {
                var webhookUrl = config.WebhookUrl;
                if (string.IsNullOrEmpty(webhookUrl))
                {
                    return false;
                }
                
                // Format the payload according to the specified format
                string content;
                string contentType;
                
                switch (config.WebhookFormat?.ToLower())
                {
                    case "json":
                    default:
                        content = JsonConvert.SerializeObject(payload);
                        contentType = "application/json";
                        break;
                        
                    case "form":
                        // Convert to form data
                        var formData = new Dictionary<string, string>();
                        var properties = payload.GetType().GetProperties();
                        foreach (var prop in properties)
                        {
                            var value = prop.GetValue(payload);
                            if (value != null)
                            {
                                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string))
                                {
                                    formData[prop.Name] = JsonConvert.SerializeObject(value);
                                }
                                else
                                {
                                    formData[prop.Name] = value.ToString();
                                }
                            }
                        }
                        
                        var formContent = new FormUrlEncodedContent(formData);
                        var response = await _httpClient.PostAsync(webhookUrl, formContent);
                        return response.IsSuccessStatusCode;
                }
                
                // Send the webhook notification
                var stringContent = new StringContent(content, Encoding.UTF8, contentType);
                var httpResponse = await _httpClient.PostAsync(webhookUrl, stringContent);
                
                if (httpResponse.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Webhook sent successfully to {webhookUrl}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Failed to send webhook to {webhookUrl}: {httpResponse.StatusCode} - {await httpResponse.Content.ReadAsStringAsync()}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending webhook notification to {config.WebhookUrl}");
                return false;
            }
        }
    }
}