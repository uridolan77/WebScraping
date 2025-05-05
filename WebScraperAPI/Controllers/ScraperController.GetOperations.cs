using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using WebScraperApi.Data.Entities;

namespace WebScraperAPI.Controllers
{
    public partial class ScraperController
    {
        // Default endpoint that responds to GET /api/Scraper to retrieve all scrapers
        [HttpGet]
        public async Task<IActionResult> GetScrapers()
        {
            try
            {
                // First check if database connection is working
                var isConnected = _scraperRepository.TestDatabaseConnection();
                _logger.LogInformation("Database connection test result: {IsConnected}", isConnected);
                
                // First try to get scrapers from the database
                _logger.LogInformation("Attempting to fetch scrapers from database...");
                var dbScrapers = await _scraperRepository.GetAllScrapersAsync();
                
                _logger.LogInformation("Database returned {Count} scrapers", dbScrapers?.Count ?? 0);
                
                if (dbScrapers != null && dbScrapers.Count > 0)
                {
                    // Convert database entities to API model
                    var scraperModels = dbScrapers.Select(s => new ScraperConfigModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartUrl = s.StartUrl,
                        BaseUrl = s.BaseUrl,
                        OutputDirectory = s.OutputDirectory,
                        MaxDepth = s.MaxDepth,
                        MaxPages = s.MaxPages,
                        DelayBetweenRequests = s.DelayBetweenRequests,
                        MaxConcurrentRequests = s.MaxConcurrentRequests,
                        FollowLinks = s.FollowLinks,
                        FollowExternalLinks = s.FollowExternalLinks,
                        CreatedAt = s.CreatedAt,
                        LastModified = s.LastModified,
                        LastRun = s.LastRun,
                        RunCount = s.RunCount
                    }).ToList();
                    
                    _logger.LogInformation("Returning {Count} scrapers from database", scraperModels.Count);
                    return Ok(scraperModels);
                }
                
                // Fallback to JSON file if no scrapers in database
                var configs = await LoadScraperConfigsAsync();
                
                _logger.LogWarning("No scrapers found in database, falling back to JSON file. Found {Count} scrapers.", configs.Count);
                
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scrapers");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving scrapers",
                    Error = ex.Message
                });
            }
        }

        // Add a new endpoint that responds to GET /api/Scraper to retrieve all scrapers with related collections
        [HttpGet("with-collections")]
        public async Task<IActionResult> GetScrapersWithCollections()
        {
            try
            {
                // Check if database connection is working
                var isConnected = _scraperRepository.TestDatabaseConnection();
                _logger.LogInformation("Database connection test result: {IsConnected}", isConnected);
                
                // Get scrapers from the database
                _logger.LogInformation("Fetching scrapers from database...");
                var dbScrapers = await _scraperRepository.GetAllScrapersAsync();
                
                _logger.LogInformation("Database returned {Count} scrapers", dbScrapers?.Count ?? 0);
                
                if (dbScrapers != null && dbScrapers.Count > 0)
                {
                    // Convert database entities to API model
                    var scraperModels = dbScrapers.Select(s => new ScraperConfigModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartUrl = s.StartUrl,
                        BaseUrl = s.BaseUrl,
                        OutputDirectory = s.OutputDirectory,
                        MaxDepth = s.MaxDepth,
                        MaxPages = s.MaxPages,
                        DelayBetweenRequests = s.DelayBetweenRequests,
                        MaxConcurrentRequests = s.MaxConcurrentRequests,
                        FollowLinks = s.FollowLinks,
                        FollowExternalLinks = s.FollowExternalLinks,
                        CreatedAt = s.CreatedAt,
                        LastModified = s.LastModified,
                        LastRun = s.LastRun,
                        RunCount = s.RunCount,
                        // Include related entity collections
                        StartUrls = s.StartUrls?.Select(u => u.Url)?.ToList(),
                        ContentExtractorSelectors = s.ContentExtractorSelectors?.Select(c => c.Selector)?.ToList(),
                        ContentExtractorExcludeSelectors = s.ContentExtractorSelectors?.Where(c => c.IsExclude)?.Select(c => c.Selector)?.ToList()
                    }).ToList();
                    
                    _logger.LogInformation("Returning {Count} scrapers from database", scraperModels.Count);
                    return Ok(scraperModels);
                }
                
                // Fallback to JSON file if no scrapers in database
                var configs = await LoadScraperConfigsAsync();
                
                _logger.LogWarning("No scrapers found in database, falling back to JSON file. Found {Count} scrapers.", configs.Count);
                
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scrapers");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving scrapers",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetAllScrapers()
        {
            try
            {
                // First check if database connection is working
                var isConnected = _scraperRepository.TestDatabaseConnection();
                _logger.LogInformation("Database connection test result: {IsConnected}", isConnected);
                
                // First try to get scrapers from the database
                _logger.LogInformation("Attempting to fetch scrapers from database...");
                var dbScrapers = await _scraperRepository.GetAllScrapersAsync();
                
                _logger.LogInformation("Database returned {Count} scrapers", dbScrapers?.Count ?? 0);
                
                if (dbScrapers != null && dbScrapers.Count > 0)
                {
                    // Convert database entities to API model
                    var scraperModels = dbScrapers.Select(s => new ScraperConfigModel
                    {
                        Id = s.Id,
                        Name = s.Name,
                        StartUrl = s.StartUrl,
                        BaseUrl = s.BaseUrl,
                        OutputDirectory = s.OutputDirectory,
                        MaxDepth = s.MaxDepth,
                        MaxPages = s.MaxPages,
                        DelayBetweenRequests = s.DelayBetweenRequests,
                        MaxConcurrentRequests = s.MaxConcurrentRequests,
                        FollowLinks = s.FollowLinks,
                        FollowExternalLinks = s.FollowExternalLinks,
                        CreatedAt = s.CreatedAt,
                        LastModified = s.LastModified,
                        LastRun = s.LastRun,
                        RunCount = s.RunCount
                    }).ToList();
                    
                    _logger.LogInformation("Returning {Count} scrapers from database", scraperModels.Count);
                    return Ok(scraperModels);
                }
                
                // Fallback to JSON file if no scrapers in database
                var configs = await LoadScraperConfigsAsync();
                
                _logger.LogWarning("No scrapers found in database, falling back to JSON file. Found {Count} scrapers.", configs.Count);
                
                return Ok(configs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scrapers");
                return StatusCode(500, new
                {
                    Message = "An error occurred while retrieving scrapers",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetScraperDetails(string id)
        {
            try
            {
                // First try to get the scraper from the database
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                
                if (dbScraper != null)
                {
                    // Convert the database entity to API model
                    var scraperModel = new ScraperConfigModel
                    {
                        Id = dbScraper.Id,
                        Name = dbScraper.Name,
                        StartUrl = dbScraper.StartUrl,
                        BaseUrl = dbScraper.BaseUrl,
                        OutputDirectory = dbScraper.OutputDirectory,
                        MaxDepth = dbScraper.MaxDepth,
                        MaxPages = dbScraper.MaxPages,
                        DelayBetweenRequests = dbScraper.DelayBetweenRequests,
                        MaxConcurrentRequests = dbScraper.MaxConcurrentRequests,
                        FollowLinks = dbScraper.FollowLinks,
                        FollowExternalLinks = dbScraper.FollowExternalLinks,
                        CreatedAt = dbScraper.CreatedAt,
                        LastModified = dbScraper.LastModified,
                        LastRun = dbScraper.LastRun,
                        RunCount = dbScraper.RunCount
                    };
                    
                    return Ok(scraperModel);
                }
                
                // Fallback to JSON file if not found in database
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                _logger.LogWarning("Scraper with ID {Id} not found in database, falling back to JSON file.", id);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraper");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while retrieving scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("by-id/{id}")]     // Renamed to be more specific and avoid conflicts
        public async Task<IActionResult> GetScraperById(string id)
        {
            try
            {
                // First try to get the scraper from the database
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                
                if (dbScraper != null)
                {
                    // Convert the database entity to API model
                    var scraperModel = new ScraperConfigModel
                    {
                        Id = dbScraper.Id,
                        Name = dbScraper.Name,
                        StartUrl = dbScraper.StartUrl,
                        BaseUrl = dbScraper.BaseUrl,
                        OutputDirectory = dbScraper.OutputDirectory,
                        MaxDepth = dbScraper.MaxDepth,
                        MaxPages = dbScraper.MaxPages,
                        DelayBetweenRequests = dbScraper.DelayBetweenRequests,
                        MaxConcurrentRequests = dbScraper.MaxConcurrentRequests,
                        FollowLinks = dbScraper.FollowLinks,
                        FollowExternalLinks = dbScraper.FollowExternalLinks,
                        CreatedAt = dbScraper.CreatedAt,
                        LastModified = dbScraper.LastModified,
                        LastRun = dbScraper.LastRun,
                        RunCount = dbScraper.RunCount,
                        // Include related entity collections
                        StartUrls = dbScraper.StartUrls?.Select(u => u.Url)?.ToList(),
                        ContentExtractorSelectors = dbScraper.ContentExtractorSelectors?.Where(c => !c.IsExclude)?.Select(c => c.Selector)?.ToList(),
                        ContentExtractorExcludeSelectors = dbScraper.ContentExtractorSelectors?.Where(c => c.IsExclude)?.Select(c => c.Selector)?.ToList()
                    };
                    
                    _logger.LogInformation("Retrieved scraper {Id} from database", id);
                    return Ok(scraperModel);
                }
                
                // Fallback to JSON file if not found in database
                var configs = await LoadScraperConfigsAsync();
                var config = configs.FirstOrDefault(c => c.Id == id);
                
                if (config == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                _logger.LogWarning("Scraper with ID {Id} not found in database, falling back to JSON file.", id);
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraper {Id}: {Message}", id, ex.Message);
                return StatusCode(500, new
                {
                    Message = $"An error occurred while retrieving scraper {id}",
                    Error = ex.Message
                });
            }
        }

        [HttpGet("debug/database-test")]
        public IActionResult TestDatabaseConnection()
        {
            try
            {
                // First check if database connection is working
                var isConnected = _scraperRepository.TestDatabaseConnection();
                
                // Try to get the context type from the repository
                var contextType = "Unknown";
                try
                {
                    var repository = _scraperRepository as WebScraperApi.Data.Repositories.ScraperRepository;
                    if (repository != null)
                    {
                        var context = repository.GetType().GetField("_context", 
                            System.Reflection.BindingFlags.NonPublic | 
                            System.Reflection.BindingFlags.Instance)?.GetValue(repository);
                        
                        if (context != null)
                        {
                            contextType = context.GetType().FullName;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting repository context type");
                }
                
                return Ok(new
                {
                    IsConnected = isConnected,
                    RepositoryType = _scraperRepository.GetType().FullName,
                    ContextType = contextType,
                    ConnectionString = "[REDACTED]",
                    Message = isConnected ? "Database connection successful" : "Database connection failed"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing database connection");
                return StatusCode(500, new
                {
                    Message = "An error occurred while testing database connection",
                    Error = ex.Message
                });
            }
        }
    }
}