using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Data.Common;
using System.Threading.Tasks;
using WebScraperApi.Data;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly WebScraperDbContext _dbContext;
        private readonly IConfiguration _configuration;

        public HealthController(WebScraperDbContext dbContext, IConfiguration configuration)
        {
            _dbContext = dbContext;
            _configuration = configuration;
        }
        /// <summary>
        /// Simple health check endpoint
        /// </summary>
        /// <returns>Health check response</returns>
        [HttpGet]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> Get()
        {
            bool dbConnected = false;
            string dbError = null;

            try
            {
                // Try to connect to the database
                dbConnected = await _dbContext.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                dbError = ex.Message;
            }

            return Ok(new
            {
                Status = dbConnected ? "Healthy" : "Unhealthy",
                Database = new
                {
                    Connected = dbConnected,
                    Error = dbError
                },
                Timestamp = DateTime.Now,
                Version = "1.0.0",
                Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
            });
        }

        /// <summary>
        /// Get database connection string (masked)
        /// </summary>
        /// <returns>Masked connection string</returns>
        [HttpGet("db-info")]
        [ProducesResponseType(typeof(object), 200)]
        public async Task<IActionResult> GetDbInfo()
        {
            try
            {
                // Get connection string from configuration
                var connectionString = _configuration.GetConnectionString("WebStraction")
                    ?? Environment.GetEnvironmentVariable("ConnectionStrings__WebStraction")
                    ?? "Not set in configuration or environment";

                // Mask password if present
                var maskedConnectionString = connectionString;
                if (connectionString.Contains("Password="))
                {
                    var parts = connectionString.Split(';');
                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
                        {
                            parts[i] = "Password=*****";
                        }
                    }
                    maskedConnectionString = string.Join(";", parts);
                }

                // Test database connection
                bool canConnect = false;
                string error = null;
                string version = null;

                try
                {
                    canConnect = await _dbContext.Database.CanConnectAsync();

                    if (canConnect)
                    {
                        // Try to get database version
                        try
                        {
                            var connection = _dbContext.Database.GetDbConnection();
                            if (connection.State != System.Data.ConnectionState.Open)
                            {
                                await connection.OpenAsync();
                            }

                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "SELECT VERSION()";
                                version = (await command.ExecuteScalarAsync())?.ToString();
                            }
                        }
                        catch (Exception ex)
                        {
                            version = $"Error getting version: {ex.Message}";
                        }
                    }
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }

                return Ok(new
                {
                    ConnectionString = maskedConnectionString,
                    Provider = "MySQL",
                    CanConnect = canConnect,
                    Error = error,
                    Version = version,
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    Timestamp = DateTime.Now
                });
            }
        }
    }
}
