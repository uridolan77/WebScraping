using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WebScraper.DependencyInjection;
using WebScraper.Processing;
using WebScraper.Resilience;
using WebScraper.Security;
using WebScraperApi.Data;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Services.Execution;
using WebScraperApi.Services.Factories;

namespace WebScraperApi.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebScraperServices(this IServiceCollection services, IConfiguration configuration = null)
        {
            // Add DbContext if configuration is provided
            if (configuration != null)
            {
                services.AddDbContext<WebScraperDbContext>(options =>
                    options.UseMySql(
                        configuration.GetConnectionString("DefaultConnection"),
                        MySqlServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
                    )
                );
            }

            // Add specialized repositories
            services.AddScoped<IScraperConfigRepository, ScraperConfigRepository>();
            services.AddScoped<IScraperStatusRepository, ScraperStatusRepository>();
            services.AddScoped<IScraperRunRepository, ScraperRunRepository>();
            services.AddScoped<IScrapedPageRepository, ScrapedPageRepository>();
            services.AddScoped<IMetricsRepository, MetricsRepository>();
            
            // Add facade that implements the original interface for backward compatibility
            services.AddScoped<IScraperRepository, ScraperRepositoryFacade>();

            // Add services
            services.AddScoped<ScraperConfigService>();
            services.AddScoped<ScraperRunService>();
            services.AddScoped<ContentChangeService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<MetricsService>();

            // Add the execution service
            services.AddScoped<IScraperExecutionService, WebScraperApi.Services.Execution.ScraperExecutionService>();

            // Add the composite service
            services.AddScoped<IScraperService, ScraperService>();

            // Add enhanced components from WebScraper project
            services.AddWebScraperEnhanced();

            return services;
        }

        /// <summary>
        /// Adds factory services to the service collection
        /// </summary>
        public static IServiceCollection AddScraperFactories(this IServiceCollection services)
        {
            // Add factory services
            services.AddScoped<ScraperComponentFactory>();
            services.AddScoped<ContentExtractionFactory>();
            services.AddScoped<DocumentProcessingFactory>();
            services.AddScoped<RegulatoryMonitorFactory>();
            services.AddScoped<StateManagementFactory>();

            return services;
        }

        /// <summary>
        /// Adds enhanced WebScraper components to the service collection
        /// </summary>
        public static IServiceCollection AddEnhancedComponents(this IServiceCollection services)
        {
            // Register circuit breaker
            services.AddSingleton<CircuitBreakerRateLimiter>(sp =>
                new CircuitBreakerRateLimiter(
                    sp.GetRequiredService<ILogger<CircuitBreakerRateLimiter>>(),
                    5,
                    30));

            // Register streaming content extractor
            services.AddSingleton<StreamingContentExtractor>(sp =>
                new StreamingContentExtractor(
                    sp.GetRequiredService<ILogger<StreamingContentExtractor>>()));

            // Register security validator
            services.AddSingleton<SecurityValidator>(sp =>
                new SecurityValidator(
                    sp.GetRequiredService<ILogger<SecurityValidator>>()));

            // Register text analyzer
            services.AddSingleton<WebScraper.Processing.Interfaces.ITextAnalyzer, WebScraper.Processing.Implementation.TextAnalyzer>();

            // Register sentiment analyzer
            services.AddSingleton<WebScraper.Processing.Interfaces.ISentimentAnalyzer, WebScraper.Processing.Implementation.SentimentAnalyzer>();

            // Register entity recognizer
            services.AddSingleton<WebScraper.Processing.Interfaces.IEntityRecognizer, WebScraper.Processing.Implementation.EntityRecognizer>();

            // Register machine learning content classifier with advanced analyzers
            services.AddSingleton<MachineLearningContentClassifier>(sp =>
                new MachineLearningContentClassifier(
                    sp.GetRequiredService<ILogger<MachineLearningContentClassifier>>(),
                    sp.GetRequiredService<WebScraper.Processing.Interfaces.ITextAnalyzer>(),
                    sp.GetRequiredService<WebScraper.Processing.Interfaces.ISentimentAnalyzer>(),
                    sp.GetRequiredService<WebScraper.Processing.Interfaces.IEntityRecognizer>()));

            return services;
        }
    }
}
