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
        /// Adds all factory services to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection AddScraperFactories(this IServiceCollection services)
        {
            // Register all factories
            services.AddSingleton<ContentExtractionFactory>();
            services.AddSingleton<DocumentProcessingFactory>();
            services.AddSingleton<RegulatoryMonitorFactory>();
            services.AddSingleton<StateManagementFactory>();
            
            // Register the main component factory
            services.AddSingleton<ScraperComponentFactory>();
            
            return services;
        }
    }
}
