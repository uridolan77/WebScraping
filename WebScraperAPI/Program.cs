using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;
using StackExchange.Redis;
using WebScraperApi.Data;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Data.Repositories.MongoDB;
using WebScraperApi.Data.Repositories.PostgreSQL;

using WebScraperApi.Services;
using WebScraperApi.Services.Factories;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using System.Reflection;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        policy =>
        {
            // For development, explicitly allow multiple origins
            policy.WithOrigins(
                    "http://localhost:5173",  // Original React app origin
                    "http://localhost:5203",  // Frontend origin that's trying to access the API
                    "https://localhost:7143"  // API's own origin for self-calls
                  )
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serialization to handle circular references
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
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

// Add MySQL connection for WebStraction
builder.Services.AddDbContext<WebScraperDbContext>(options =>
{
    var connectionString = GetConnectionString(builder.Configuration, "WebStraction");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null));
            Console.WriteLine("Using MySQL database for WebStraction");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to configure MySQL: {ex.Message}");
            Console.WriteLine("Using empty database context");
        }
    }
    else
    {
        Console.WriteLine("Using empty database context");
    }
});

// Add database connections with secure connection strings - kept for compatibility
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
builder.Services.AddScoped<IScraperStatusRepository, ScraperStatusRepository>();
builder.Services.AddScoped<IScraperRunRepository, ScraperRunRepository>();
builder.Services.AddScoped<IScrapedPageRepository, ScrapedPageRepository>();
builder.Services.AddScoped<IMetricsRepository, MetricsRepository>();

// Replace the original repository with our facade for backward compatibility
builder.Services.AddScoped<IScraperRepository, ScraperRepositoryFacade>();

// Register factory services
WebScraperApi.Services.Factories.ScraperFactoryExtensions.AddScraperFactories(builder.Services); // Using the fully qualified method name to avoid ambiguity

// Add all scraper services
builder.Services.AddScraperServices();

// Add enhanced components
builder.Services.AddEnhancedComponents(); // Using the extension method from WebScraperApi.Services.ServiceCollectionExtensions

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
        Console.WriteLine($"Including XML comments from: {xmlPath}");
    }
    else
    {
        Console.WriteLine($"Warning: XML documentation file not found at: {xmlPath}");
    }

    // Use fully qualified object names to avoid conflicts
    c.CustomSchemaIds(type => type.FullName);

    // Add a conflict resolver for actions with the same path
    c.ResolveConflictingActions(apiDescriptions =>
    {
        // Use the first action in case of conflicts
        // This helps when you have duplicate routes
        return apiDescriptions.First();
    });
});

var app = builder.Build();

// Initialize the ServiceLocator with the service provider
WebScraperApi.Services.ServiceLocator.Initialize(app.Services);

// Apply database migrations if enabled
if (builder.Configuration.GetValue<bool>("Database:AutoMigrate"))
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            try
            {
                // Apply PostgreSQL migrations if configured
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (dbContext.Database.ProviderName != null && !dbContext.Database.ProviderName.Contains("InMemory"))
                {
                    dbContext.Database.Migrate();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to apply PostgreSQL migrations: {ex.Message}");
            }

            try
            {
                // Apply MySQL migrations if configured
                var webScraperDbContext = scope.ServiceProvider.GetRequiredService<WebScraperDbContext>();
                if (webScraperDbContext.Database.ProviderName != null && !webScraperDbContext.Database.ProviderName.Contains("InMemory"))
                {
                    // Ensure database exists
                    webScraperDbContext.Database.EnsureCreated();

                    try
                    {
                        // Try to apply migrations
                        webScraperDbContext.Database.Migrate();
                    }
                    catch (Exception migrateEx)
                    {
                        Console.WriteLine($"Warning: Failed to apply MySQL migrations: {migrateEx.Message}");
                        Console.WriteLine("Attempting to ensure tables exist...");

                        // If migrations fail, try to ensure the database is created
                        try
                        {
                            // Check if the scraper_start_url table exists
                            var connection = webScraperDbContext.Database.GetDbConnection();
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = @"
                                    CREATE TABLE IF NOT EXISTS scraper_start_url (
                                        id INT AUTO_INCREMENT PRIMARY KEY,
                                        scraper_id VARCHAR(36) NOT NULL,
                                        url VARCHAR(500) NOT NULL,
                                        FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
                                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;";
                                command.ExecuteNonQuery();
                            }
                            connection.Close();

                            Console.WriteLine("Successfully ensured scraper_start_url table exists");
                        }
                        catch (Exception tableEx)
                        {
                            Console.WriteLine($"Warning: Failed to create scraper_start_url table: {tableEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Failed to apply MySQL migrations: {ex.Message}");
            }
        }
        Console.WriteLine("Database setup completed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to set up database: {ex.Message}");
        Console.WriteLine("The application will continue with limited functionality");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Web Scraper API v1"));

    // In development, check database table naming conventions
    try
    {
        var connectionString = GetConnectionString(builder.Configuration, "WebStraction");
        if (!string.IsNullOrEmpty(connectionString))
        {
            // Check if all tables follow expected naming conventions
            WebScraperApi.Data.DatabaseTableNameFixer.EnsureCorrectTableNames(
                app.Services,
                connectionString);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Failed to check database table names: {ex.Message}");
    }
}

// Move UseCors before routing for correct CORS handling
app.UseCors(); // Use the default CORS policy

// Only use HTTPS redirection in production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// Ensure the scraper configs directory exists
var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scraperConfigs.json");
var configDir = Path.GetDirectoryName(configPath);
if (!Directory.Exists(configDir) && configDir != null)
{
    Directory.CreateDirectory(configDir);
}

// Check for phantom running scrapers on startup
try
{
    using (var scope = app.Services.CreateScope())
    {
        var scraperRepository = scope.ServiceProvider.GetRequiredService<IScraperRepository>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Get all scrapers that are marked as running in the database
        var runningScrapers = scraperRepository.GetAllRunningScrapersAsync().GetAwaiter().GetResult();

        if (runningScrapers != null && runningScrapers.Count > 0)
        {
            logger.LogWarning("Found {Count} scrapers marked as running in the database on startup. These are likely phantom scrapers that were not properly stopped.", runningScrapers.Count);

            // Update each scraper's status to not running
            foreach (var scraper in runningScrapers)
            {
                logger.LogInformation("Updating phantom running scraper {ScraperId} to stopped status", scraper.ScraperId);

                var status = new WebScraperApi.Data.Entities.ScraperStatusEntity
                {
                    ScraperId = scraper.ScraperId,
                    IsRunning = false,
                    EndTime = DateTime.Now,
                    LastUpdate = DateTime.Now,
                    Message = "Scraper marked as stopped due to server restart",
                    StartTime = scraper.StartTime,
                    UrlsProcessed = scraper.UrlsProcessed,
                    UrlsQueued = scraper.UrlsQueued,
                    DocumentsProcessed = scraper.DocumentsProcessed,
                    HasErrors = scraper.HasErrors
                };

                scraperRepository.UpdateScraperStatusAsync(status).GetAwaiter().GetResult();
            }

            logger.LogInformation("Successfully updated {Count} phantom running scrapers to stopped status", runningScrapers.Count);
        }
        else
        {
            logger.LogInformation("No phantom running scrapers found on startup");
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Failed to check for phantom running scrapers: {ex.Message}");
}

app.Run();
