using Microsoft.Extensions.DependencyInjection;
using WebScraperApi.Services.Factories;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Extension methods for registering factory services
    /// </summary>
    public static class ScraperFactoryExtensions
    {
        /// <summary>
        /// Adds scraper factory services to the dependency injection container
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddScraperFactories(this IServiceCollection services)
        {
            // Register individual component factories
            services.AddScoped<ContentExtractionFactory>();
            services.AddScoped<DocumentProcessingFactory>();
            services.AddScoped<RegulatoryMonitorFactory>();
            services.AddScoped<StateManagementFactory>();

            // Register the main component factory
            services.AddScoped<ScraperComponentFactory>();

            return services;
        }
    }
}
