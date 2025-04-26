using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using WebScraperApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder
            .WithOrigins("http://localhost:5192") // React app URL
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// Add the ScraperManager as a singleton hosted service
builder.Services.AddSingleton<ScraperManager>();
builder.Services.AddHostedService(provider => provider.GetRequiredService<ScraperManager>());

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Web Scraper API", Version = "v1" });
});

var app = builder.Build();

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