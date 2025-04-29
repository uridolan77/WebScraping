using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebScraperApi.Data;
using WebScraperApi.Data.Repositories;

namespace WebScraperApi.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWebScraperServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext
            services.AddDbContext<WebScraperDbContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
                )
            );

            // Add repositories
            services.AddScoped<IScraperRepository, ScraperRepository>();

            // Add services
            services.AddScoped<ScraperConfigService>();
            services.AddScoped<ScraperRunService>();
            services.AddScoped<ContentChangeService>();
            services.AddScoped<DocumentService>();
            services.AddScoped<MetricsService>();
            
            // Add the existing execution service
            // Note: This assumes the existing ScraperExecutionService is already registered elsewhere
            
            // Add the composite service
            services.AddScoped<IScraperService, ScraperService>();

            return services;
        }
    }
}
