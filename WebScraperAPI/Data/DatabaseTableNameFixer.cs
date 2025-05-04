using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace WebScraperApi.Data
{
    /// <summary>
    /// Utility class to ensure that all database tables exist and match the expected naming convention
    /// </summary>
    public static class DatabaseTableNameFixer
    {
        /// <summary>
        /// Makes sure all database tables follow the snake_case convention expected by the application
        /// </summary>
        public static void EnsureCorrectTableNames(IServiceProvider serviceProvider, string connectionString)
        {
            using var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<WebScraperDbContext>>();
            
            logger.LogInformation("Checking database table names to ensure they match expected snake_case convention...");
            
            try
            {
                // Get all entity types and their configured table names from EF Core
                var dbContext = scope.ServiceProvider.GetRequiredService<WebScraperDbContext>();
                var entityMappings = dbContext.Model.GetEntityTypes()
                    .Where(e => !e.GetTableName().StartsWith("__")) // Skip EF migration history table
                    .Select(e => new 
                    {
                        EntityType = e.ClrType.Name,
                        TableName = e.GetTableName(),
                        SchemaName = e.GetSchema()
                    })
                    .ToList();
                
                // Log the entity to table mappings
                foreach (var mapping in entityMappings)
                {
                    logger.LogInformation("Entity {EntityType} is mapped to table {TableName}", 
                        mapping.EntityType, mapping.TableName);
                }
                
                // Check if the expected tables exist in the database
                var connection = dbContext.Database.GetDbConnection();
                if (connection.State != System.Data.ConnectionState.Open)
                {
                    connection.Open();
                }
                
                // Get all tables from the database using EF Core's relational services
                var tables = new System.Collections.Generic.List<string>();
                
                // Execute SQL to get all table names - works with MySQL
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SHOW TABLES";
                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        tables.Add(reader.GetString(0));
                    }
                }
                
                // Check each entity mapping against the database tables
                foreach (var mapping in entityMappings)
                {
                    var expectedTableName = mapping.TableName?.ToLower() ?? "";
                    
                    if (!tables.Contains(expectedTableName, StringComparer.OrdinalIgnoreCase))
                    {
                        logger.LogWarning("Expected table {ExpectedTableName} for entity {EntityType} not found in database!", 
                            expectedTableName, mapping.EntityType);
                        
                        // Try to find similar table names to suggest alternatives
                        var similarTables = tables.Where(t => 
                            t.Replace("_", "").Equals(expectedTableName.Replace("_", ""), StringComparison.OrdinalIgnoreCase) ||
                            t.Contains(expectedTableName.Replace("_", ""), StringComparison.OrdinalIgnoreCase) ||
                            expectedTableName.Replace("_", "").Contains(t.Replace("_", ""), StringComparison.OrdinalIgnoreCase))
                            .ToList();
                        
                        if (similarTables.Any())
                        {
                            logger.LogWarning("Found similar table(s): {SimilarTables}", 
                                string.Join(", ", similarTables));
                        }
                    }
                    else
                    {
                        logger.LogInformation("Table {TableName} for entity {EntityType} exists as expected", 
                            expectedTableName, mapping.EntityType);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error checking database table names");
            }
        }
    }
}