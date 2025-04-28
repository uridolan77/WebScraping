using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using WebScraperAPI.Data;
using WebScraperAPI.Data.Repositories;
using WebScraperAPI.Data.Repositories.MongoDB;
using WebScraperAPI.Data.Repositories.PostgreSQL;
using WebScraperAPI.Data.Repositories.Redis;
using WebScraperApi.Services;
using WebScraperApi.Services.Factories;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System.Reflection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Configure Azure Key Vault
var keyVaultEndpoint = builder.Configuration["KeyVault:Endpoint"];
if (!string.IsNullOrEmpty(keyVaultEndpoint))
{
    try
    {
        // Use DefaultAzureCredential for authentication to Azure Key Vault
        // This will try multiple authentication methods including:
        // - Environment variables (client ID, client secret, tenant ID)
        // - Managed Identity
        // - Visual Studio credentials
        // - Azure CLI credentials
        // - Interactive browser login (in development)
        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ExcludeInteractiveBrowserCredential = !builder.Environment.IsDevelopment()
            });

        // Create a SecretClient to access Key Vault
        var secretClient = new SecretClient(
            new Uri(keyVaultEndpoint),
            credential);

        // Add Key Vault to the configuration system
        builder.Configuration.AddAzureKeyVault(
            secretClient,
            new KeyVaultSecretManager());

        // Register SecretClient for services that need direct Key Vault access
        builder.Services.AddSingleton(secretClient);

        Console.WriteLine("Azure Key Vault configuration added");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to configure Azure Key Vault: {ex.Message}");
        // Continue execution without Key Vault
    }
}

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

// Helper method to get connection strings from Key Vault or local configuration
static string GetConnectionString(IConfiguration configuration, string name)
{
    // Try to get from Key Vault first (keys follow the naming convention)
    var keyVaultKey = $"ConnectionString-{name}";
    var connectionString = configuration[keyVaultKey];

    // Fall back to local configuration if not in Key Vault
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = configuration.GetConnectionString(name);
    }

    // Return empty string if no connection string found
    return connectionString ?? string.Empty;
}

// Add database connections with secure connection strings
// PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(GetConnectionString(builder.Configuration, "PostgreSQL")));

// MongoDB
builder.Services.AddSingleton<IMongoClient>(sp =>
    new MongoClient(GetConnectionString(builder.Configuration, "MongoDB")));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(GetConnectionString(builder.Configuration, "Redis")));

// Register repositories
builder.Services.AddScoped<IScraperConfigRepository, ScraperConfigRepository>();
builder.Services.AddScoped<IScrapedContentRepository, ScrapedContentRepository>();
builder.Services.AddSingleton<ICacheRepository, CacheRepository>();

// Register factory services
builder.Services.AddScraperFactories();

// Add all scraper services
builder.Services.AddScraperServices();

// Add Swagger with enhanced configuration
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo {
        Title = "Web Scraper API",
        Version = "v1",
        Description = "API for managing web scrapers and their configurations",
        Contact = new OpenApiContact
        {
            Name = "Web Scraper Team",
            Email = "support@webscraper.example.com"
        }
    });

    // Set the comments path for the Swagger JSON and UI
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Use fully qualified object names to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);

    // No need for EnableAnnotations() as it's not available in the current version
});

var app = builder.Build();

// Apply database migrations if enabled
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
        Console.WriteLine("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to apply database migrations: {ex.Message}");
        Console.WriteLine("The application will continue without database support");
        // Continue execution without database support
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
