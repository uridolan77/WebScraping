using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebScraper.Processing;
using WebScraper.Resilience;
using WebScraper.Security;

namespace WebScraper.DependencyInjection
{
    /// <summary>
    /// Extension methods for registering WebScraper services
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds WebScraper core services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddWebScraperCore(this IServiceCollection services)
        {
            // Register core services
            services.AddSingleton<Scraper>();
            
            // Register enhanced components
            services.AddSingleton<CircuitBreakerRateLimiter>(sp => 
                new CircuitBreakerRateLimiter(
                    sp.GetRequiredService<ILogger<CircuitBreakerRateLimiter>>(), 
                    5, 
                    30));
            
            services.AddSingleton<StreamingContentExtractor>(sp => 
                new StreamingContentExtractor(
                    sp.GetRequiredService<ILogger<StreamingContentExtractor>>()));
            
            services.AddSingleton<SecurityValidator>(sp => 
                new SecurityValidator(
                    sp.GetRequiredService<ILogger<SecurityValidator>>()));
            
            services.AddSingleton<MachineLearningContentClassifier>(sp => 
                new MachineLearningContentClassifier(
                    sp.GetRequiredService<ILogger<MachineLearningContentClassifier>>()));
            
            return services;
        }

        /// <summary>
        /// Adds WebScraper enhanced services to the service collection
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <returns>Service collection for chaining</returns>
        public static IServiceCollection AddWebScraperEnhanced(this IServiceCollection services)
        {
            // Add core services first
            services.AddWebScraperCore();
            
            // Register additional enhanced services
            
            return services;
        }
    }
}
