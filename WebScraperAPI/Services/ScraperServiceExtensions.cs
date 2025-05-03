using Microsoft.Extensions.DependencyInjection;
using System;
using WebScraperApi.Services.Analytics;
using WebScraperApi.Services.Configuration;
using WebScraperApi.Services.Execution;
using WebScraperApi.Services.Monitoring;
using WebScraperApi.Services.Notifications;
using WebScraperApi.Services.Scheduling;
using WebScraperApi.Services.State;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Extension methods for registering scraper services
    /// </summary>
    public static class ScraperServiceExtensions
    {
        /// <summary>
        /// Adds all scraper services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddScraperServices(this IServiceCollection services)
        {
            // Register state service (must be registered as singleton because it holds in-memory state)
            services.AddSingleton<IScraperStateService, ScraperStateService>();

            // Register configuration service
            services.AddScoped<IScraperConfigurationService, Configuration.ScraperConfigurationService>();

            // Register execution service
            services.AddScoped<IScraperExecutionService, Execution.ScraperExecutionService>();

            // Register monitoring service
            services.AddSingleton<IScraperMonitoringService, ScraperMonitoringService>();

            // Register analytics service
            services.AddScoped<IScraperAnalyticsService, ScraperAnalyticsService>();

            // Register scheduling service (must be registered as singleton because it holds in-memory schedules)
            services.AddSingleton<IScraperSchedulingService, ScraperSchedulingService>();

            // Register notification service
            services.AddScoped<IWebhookNotificationService, WebhookNotificationService>();

            // Register the main scraper manager as scoped to match its dependencies
            services.AddScoped<ScraperManager>();

            // Register the ScraperManager as a singleton for services that need it
            // This helps avoid circular dependencies
            services.AddSingleton<Func<ScraperManager>>(sp => () => sp.GetRequiredService<ScraperManager>());

            // Comment out the hosted service registration for now
            // services.AddHostedService<IHostedService>();

            // Register HTTP client for webhook notifications
            services.AddHttpClient<IWebhookNotificationService, WebhookNotificationService>();

            // Register the services needed by ScraperService
            services.AddScoped<ScraperConfigService>();
            services.AddScoped<ScraperRunService>();
            services.AddScoped<ContentChangeService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<MetricsService>();

            // Register the main scraper service
            services.AddScoped<IScraperService, ScraperService>();

            return services;
        }
    }
}