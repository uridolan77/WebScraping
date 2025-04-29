using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using WebScraperApi.Data;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Services.Execution;

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
                    MySqlServerVersion.AutoDetect(configuration.GetConnectionString("DefaultConnection"))
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

            // Add the execution service
            services.AddScoped<IScraperExecutionService, WebScraperApi.Services.Execution.ScraperExecutionService>();

            // Add the composite service
            services.AddScoped<IScraperService, ScraperService>();

            return services;
        }
    }
}
