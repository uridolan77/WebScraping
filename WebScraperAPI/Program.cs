using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using WebScraperAPI.Data;
using WebScraperAPI.Data.Repositories;
using WebScraperAPI.Data.Repositories.MongoDB;
using WebScraperAPI.Data.Repositories.PostgreSQL;
using WebScraperAPI.Data.Repositories.Redis;
using WebScraperApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins(
                "http://localhost:5000",  // WebScraperWeb default port
                "http://localhost:5192",  // Original React app URL
                "https://localhost:5001"  // WebScraperWeb default HTTPS port
            )
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add database connections
// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgreSQL")));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp => 
    new MongoClient(builder.Configuration.GetConnectionString("MongoDB")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));

// Register repositories
builder.Services.AddScoped<IScraperConfigRepository, ScraperConfigRepository>();
builder.Services.AddScoped<IScrapedContentRepository, ScrapedContentRepository>();
builder.Services.AddSingleton<ICacheRepository, CacheRepository>();

// Add the ScraperManager as a singleton hosted service
builder.Services.AddSingleton<ScraperManager>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ScraperManager>());

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web Scraper API", Version = "v1" });
});

var app = builder.Build();

// Apply database migrations if enabled
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Scraper API v1"));
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowReactApp");
app.UseAuthorization();
app.MapControllers();

app.Run();
