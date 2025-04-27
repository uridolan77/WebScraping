using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.ContentChange;

namespace WebScraper.Notifications
{
    /// <summary>
    /// Interface for notification services
    /// </summary>
    public interface IAlertService
    {
        Task SendAlertAsync(string url, string subject, string message);
        Task ProcessAlertAsync(string url, SignificantChangesResult changes);
    }

    /// <summary>
    /// Sends notifications via webhooks when content changes are detected
    /// </summary>
    public class WebhookNotifier : IAlertService
    {
        private readonly ILogger<WebhookNotifier> _logger;
        private readonly HttpClient _httpClient;
        private readonly string _webhookUrl;
        private readonly string _scraperId;
        private readonly string _scraperName;

        public WebhookNotifier(
            ILogger<WebhookNotifier> logger,
            string scraperId,
            string scraperName,
            string webhookUrl)
        {
            _logger = logger;
            _scraperId = scraperId;
            _scraperName = scraperName;
            _webhookUrl = webhookUrl;
            _httpClient = new HttpClient();
        }

        /// <summary>
        /// Sends a general alert notification
        /// </summary>
        public async Task SendAlertAsync(string url, string subject, string message)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookUrl))
                    return;

                var payload = new
                {
                    EventType = "Alert",
                    ScraperId = _scraperId,
                    ScraperName = _scraperName,
                    Url = url,
                    Subject = subject,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await SendWebhookNotificationAsync(payload);
                _logger.LogInformation($"Alert notification sent for {url}: {subject}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending alert notification for {url}");
            }
        }

        /// <summary>
        /// Processes and sends a notification about content changes
        /// </summary>
        public async Task ProcessAlertAsync(string url, SignificantChangesResult changes)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookUrl) || changes == null)
                    return;

                var payload = new
                {
                    EventType = "ContentChanged",
                    ScraperId = _scraperId,
                    ScraperName = _scraperName,
                    Url = url,
                    ChangeType = changes.ChangeType.ToString(),
                    Changes = changes.SignificantChanges,
                    PreviousVersion = changes.PreviousVersionDate,
                    CurrentVersion = changes.CurrentVersionDate,
                    Importance = changes.Importance.ToString(),
                    ContentHashChanged = changes.HashChanged,
                    Timestamp = DateTime.UtcNow
                };

                await SendWebhookNotificationAsync(payload);
                _logger.LogInformation($"Change notification sent for {url}: {changes.ChangeType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending change notification for {url}");
            }
        }
        
        /// <summary>
        /// Sends notification about a new document being processed
        /// </summary>
        public async Task NotifyDocumentProcessedAsync(string url, string documentType, string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookUrl))
                    return;

                var payload = new
                {
                    EventType = "DocumentProcessed",
                    ScraperId = _scraperId,
                    ScraperName = _scraperName,
                    Url = url,
                    DocumentType = documentType,
                    FilePath = filePath,
                    Timestamp = DateTime.UtcNow
                };

                await SendWebhookNotificationAsync(payload);
                _logger.LogInformation($"Document processed notification sent for {url}: {documentType}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending document processed notification for {url}");
            }
        }
        
        /// <summary>
        /// Notifies about scraper status changes
        /// </summary>
        public async Task NotifyScraperStatusChangeAsync(string status, string message = null)
        {
            try
            {
                if (string.IsNullOrEmpty(_webhookUrl))
                    return;

                var payload = new
                {
                    EventType = "ScraperStatusChange",
                    ScraperId = _scraperId,
                    ScraperName = _scraperName,
                    Status = status,
                    Message = message,
                    Timestamp = DateTime.UtcNow
                };

                await SendWebhookNotificationAsync(payload);
                _logger.LogInformation($"Scraper status change notification sent: {status}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending scraper status notification");
            }
        }

        /// <summary>
        /// Sends the webhook notification with retry logic
        /// </summary>
        private async Task SendWebhookNotificationAsync(object payload, int retryCount = 3)
        {
            if (string.IsNullOrEmpty(_webhookUrl))
                return;

            Exception lastException = null;
            
            for (int i = 0; i < retryCount; i++)
            {
                try
                {
                    var response = await _httpClient.PostAsJsonAsync(_webhookUrl, payload);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        return; // Success
                    }
                    
                    // Log error response
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning($"Webhook notification failed with status {response.StatusCode}: {errorContent}");
                    
                    // Wait before retry (exponential backoff)
                    if (i < retryCount - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    
                    // Wait before retry (exponential backoff)
                    if (i < retryCount - 1)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
                    }
                }
            }
            
            if (lastException != null)
            {
                _logger.LogError(lastException, "Failed to send webhook notification after {RetryCount} attempts", retryCount);
            }
        }
    }
}