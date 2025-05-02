using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Models;
using WebScraperApi.Services.Notifications;
using WebScraperApi.Services.State;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IWebhookNotificationService _notificationService;
        private readonly IScraperStateService _stateService;
        
        public NotificationsController(
            IWebhookNotificationService notificationService, 
            IScraperStateService stateService)
        {
            _notificationService = notificationService;
            _stateService = stateService;
        }
        
        [HttpPost("webhook-test")]
        public async Task<IActionResult> TestWebhook([FromBody] WebhookTestRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (string.IsNullOrEmpty(request.ScraperId))
            {
                return BadRequest("ScraperId is required");
            }
            
            var instance = _stateService.GetScraperInstance(request.ScraperId);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {request.ScraperId} not found");
            }
            
            // Use the configured URL or a one-time URL for testing
            var tempConfig = instance.Config;
            var originalUrl = tempConfig.WebhookUrl;
            var originalEnabled = tempConfig.WebhookEnabled;
            
            try
            {
                // Override webhook settings for this test if needed
                if (!string.IsNullOrEmpty(request.WebhookUrl))
                {
                    tempConfig.WebhookUrl = request.WebhookUrl;
                    tempConfig.WebhookEnabled = true;
                }
                
                // Make sure there's a webhook URL
                if (string.IsNullOrEmpty(tempConfig.WebhookUrl))
                {
                    return BadRequest("No webhook URL provided or configured");
                }
                
                // Send the test notification
                var testData = new
                {
                    message = request.Message ?? "This is a test webhook notification",
                    timestamp = DateTime.Now,
                    testId = Guid.NewGuid().ToString()
                };
                
                var success = await _notificationService.SendCustomNotificationAsync(
                    request.ScraperId, "webhook_test", testData);
                
                if (!success)
                {
                    return BadRequest("Failed to send webhook notification. Please check the webhook URL and configuration.");
                }
                
                return Ok(new { 
                    Message = "Test webhook notification sent successfully",
                    SentTo = tempConfig.WebhookUrl,
                    Payload = testData
                });
            }
            finally
            {
                // Restore original webhook settings
                if (!string.IsNullOrEmpty(request.WebhookUrl))
                {
                    tempConfig.WebhookUrl = originalUrl;
                    tempConfig.WebhookEnabled = originalEnabled;
                }
            }
        }
        
        [HttpPut("scraper/{id}/webhook-config")]
        public async Task<IActionResult> UpdateWebhookConfig(string id, [FromBody] WebhookConfig config)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            // Update the webhook configuration
            instance.Config.WebhookUrl = config.WebhookUrl;
            instance.Config.WebhookEnabled = config.Enabled;
            instance.Config.WebhookFormat = config.Format ?? "json";
            instance.Config.WebhookTriggers = config.Triggers != null ? config.Triggers.ToList() : new List<string> { "all" };
            _stateService.AddOrUpdateScraper(id, instance);
            
            // Validate the webhook by sending a test notification if requested
            if (config.SendTestNotification && config.Enabled && !string.IsNullOrEmpty(config.WebhookUrl))
            {
                var testData = new
                {
                    message = "Testing webhook configuration",
                    timestamp = DateTime.Now
                };
                
                var success = await _notificationService.SendCustomNotificationAsync(id, "webhook_config_test", testData);
                
                if (!success)
                {
                    return BadRequest("Webhook configuration saved but the test notification failed. Please check your webhook URL.");
                }
            }
            
            return Ok(new { 
                Message = "Webhook configuration updated successfully", 
                WebhookEnabled = config.Enabled 
            });
        }
        
        [HttpPost("scraper/{id}/notify")]
        public async Task<IActionResult> SendCustomNotification(string id, [FromBody] CustomNotificationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            var instance = _stateService.GetScraperInstance(id);
            if (instance == null)
            {
                return NotFound($"Scraper with ID {id} not found");
            }
            
            if (string.IsNullOrEmpty(request.EventType))
            {
                return BadRequest("EventType is required");
            }
            
            var success = await _notificationService.SendCustomNotificationAsync(
                id, request.EventType, request.Data ?? new object());
                
            if (!success)
            {
                return BadRequest("Failed to send notification. Check webhook configuration and try again.");
            }
            
            return Ok(new { Message = "Notification sent successfully" });
        }
    }
    
    public class WebhookTestRequest
    {
        public string? ScraperId { get; set; }
        public string? WebhookUrl { get; set; }
        public string? Message { get; set; }
    }
    
    public class WebhookConfig
    {
        public string? WebhookUrl { get; set; }
        public bool Enabled { get; set; } = true;
        public string? Format { get; set; } = "json";
        public string[]? Triggers { get; set; }
        public bool SendTestNotification { get; set; } = false;
    }
    
    public class CustomNotificationRequest
    {
        public string? EventType { get; set; }
        public object? Data { get; set; }
    }
}