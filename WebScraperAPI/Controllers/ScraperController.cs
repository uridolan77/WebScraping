using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using WebScraperApi.Models;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScraperController : ControllerBase
    {
        private readonly IScraperRepository _scraperRepository;
        private readonly ILogger<ScraperController> _logger;
        private readonly IConfiguration _configuration;

        public ScraperController(
            IScraperRepository scraperRepository,
            ILogger<ScraperController> logger,
            IConfiguration configuration)
        {
            _scraperRepository = scraperRepository ?? throw new ArgumentNullException(nameof(scraperRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <summary>
        /// Test endpoint to check database connection
        /// </summary>
        /// <returns>Database connection status</returns>
        [HttpGet("test-connection")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult TestConnection()
        {
            try
            {
                var isConnected = _scraperRepository.TestDatabaseConnection();
                return Ok(new {
                    IsConnected = isConnected,
                    Message = isConnected ? "Database connection successful" : "Database connection failed",
                    Timestamp = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing database connection: {ErrorMessage}", ex.Message);
                return StatusCode(500, new {
                    Message = "Error testing database connection",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Simple ping endpoint to check if the API is running
        /// </summary>
        /// <returns>Ping response</returns>
        [HttpGet("ping")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult Ping()
        {
            return Ok(new {
                Message = "API is running",
                Timestamp = DateTime.Now,
                Version = "1.0.0"
            });
        }

        /// <summary>
        /// Get API and database configuration information
        /// </summary>
        /// <returns>Configuration information</returns>
        [HttpGet("config-info")]
        [ProducesResponseType(typeof(object), 200)]
        public IActionResult GetConfigInfo()
        {
            try
            {
                // Get connection string from configuration
                var connectionString = _configuration.GetConnectionString("WebStraction");

                // Mask password if present
                var maskedConnectionString = connectionString;
                if (!string.IsNullOrEmpty(connectionString) && connectionString.Contains("Password="))
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

                // Get environment information
                var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

                // Get assembly information
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var assemblyName = assembly.GetName();
                var version = assemblyName.Version?.ToString() ?? "Unknown";

                return Ok(new {
                    ApiVersion = version,
                    Environment = environment,
                    ConnectionString = maskedConnectionString ?? "Not configured",
                    DatabaseProvider = "MySQL",
                    ServerTime = DateTime.Now,
                    ServerTimeUtc = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting configuration information");
                return StatusCode(500, new {
                    Message = "Error getting configuration information",
                    Error = ex.Message
                });
            }
        }

        /// <summary>
        /// Create a test scraper with minimal data
        /// </summary>
        /// <returns>Created scraper</returns>
        [HttpGet("create-test")]
        [ProducesResponseType(typeof(ScraperConfigModel), 201)]
        [ProducesResponseType(typeof(object), 500)]
        public async Task<IActionResult> CreateTestScraper()
        {
            try
            {
                // Create a minimal scraper model
                var model = new ScraperConfigModel
                {
                    Name = "Test Scraper " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    StartUrl = "https://www.gamblingcommission.gov.uk/licensees-and-businesses",
                    BaseUrl = "https://www.gamblingcommission.gov.uk",
                    OutputDirectory = "TestScraperData",
                    MaxDepth = 5,
                    MaxPages = 1000,
                    MaxConcurrentRequests = 5,
                    DelayBetweenRequests = 1000,
                    FollowLinks = true,
                    FollowExternalLinks = false
                };

                // Map to entity
                var entity = MapModelToEntity(model);
                if (entity == null)
                {
                    return StatusCode(500, new { Message = "Failed to map model to entity" });
                }

                // Set default values
                entity.Id = Guid.NewGuid().ToString();
                entity.CreatedAt = DateTime.UtcNow;
                entity.LastModified = DateTime.UtcNow;

                // Save to database
                var result = await _scraperRepository.CreateScraperAsync(entity);

                // Map back to model
                var createdModel = MapEntityToModel(result);

                return CreatedAtAction(nameof(GetScraper), new { id = createdModel.Id }, createdModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating test scraper: {ErrorMessage}", ex.Message);

                return StatusCode(500, new {
                    Message = "An error occurred while creating the test scraper",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Get all scrapers
        /// </summary>
        /// <returns>List of all scrapers</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ScraperConfigModel>), 200)]
        public async Task<IActionResult> GetAllScrapers()
        {
            try
            {
                var scraperConfigs = await _scraperRepository.GetAllScrapersAsync();
                var models = new List<ScraperConfigModel>();

                foreach (var config in scraperConfigs)
                {
                    models.Add(MapEntityToModel(config));
                }

                return Ok(models);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scrapers");
                return StatusCode(500, "An error occurred while retrieving scrapers");
            }
        }

        /// <summary>
        /// Get a specific scraper by ID
        /// </summary>
        /// <param name="id">The ID of the scraper to retrieve</param>
        /// <returns>The requested scraper</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ScraperConfigModel), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScraper(string id)
        {
            try
            {
                _logger.LogInformation("Attempting to retrieve scraper with ID: {ScraperId}", id);

                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig == null)
                {
                    _logger.LogWarning("Scraper with ID {ScraperId} not found", id);
                    return NotFound($"Scraper with ID {id} not found");
                }

                _logger.LogInformation("Successfully retrieved scraper with ID: {ScraperId}, mapping to model", id);

                try
                {
                    var model = MapEntityToModel(scraperConfig);
                    return Ok(model);
                }
                catch (Exception mapEx)
                {
                    _logger.LogError(mapEx, "Error mapping entity to model for scraper with ID {ScraperId}: {ErrorMessage}",
                        id, mapEx.Message);
                    return StatusCode(500, new {
                        Message = $"An error occurred while mapping scraper {id}",
                        Error = mapEx.Message,
                        InnerError = mapEx.InnerException?.Message,
                        StackTrace = mapEx.StackTrace
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving scraper with ID {ScraperId}: {ErrorMessage}",
                    id, ex.Message);
                return StatusCode(500, new {
                    Message = $"An error occurred while retrieving scraper {id}",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    StackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Create a new scraper
        /// </summary>
        /// <param name="model">The scraper configuration to create</param>
        /// <returns>The newly created scraper configuration</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ScraperConfigModel), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateScraper([FromBody] ScraperConfigModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Ensure we have a new ID if not provided
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = Guid.NewGuid().ToString();
                }

                model.CreatedAt = DateTime.Now;
                model.LastModified = DateTime.Now;

                var entity = MapModelToEntity(model);
                var result = await _scraperRepository.CreateScraperAsync(entity);

                var createdModel = MapEntityToModel(result);
                return CreatedAtAction(nameof(GetScraper), new { id = createdModel.Id }, createdModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scraper: {ErrorMessage}", ex.Message);

                // Log the model for debugging
                _logger.LogInformation("Scraper model: {@Model}", model);

                // Return more detailed error information for debugging
                var errorDetails = new
                {
                    Message = "An error occurred while creating the scraper",
                    Error = ex.Message,
                    InnerError = ex.InnerException?.Message,
                    InnerStackTrace = ex.InnerException?.StackTrace,
                    StackTrace = ex.StackTrace,
                    ModelState = ModelState.IsValid ? "Valid" : "Invalid",
                    ModelErrors = ModelState.IsValid ? null : ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList()
                };

                return StatusCode(500, errorDetails);
            }
        }

        /// <summary>
        /// Update an existing scraper
        /// </summary>
        /// <param name="id">The ID of the scraper to update</param>
        /// <param name="model">The updated scraper configuration</param>
        /// <returns>The updated scraper configuration</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ScraperConfigModel), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateScraper(string id, [FromBody] ScraperConfigModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != model.Id)
            {
                return BadRequest("ID in URL does not match ID in request body");
            }

            try
            {
                var existingScraper = await _scraperRepository.GetScraperByIdAsync(id);
                if (existingScraper == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }

                // Preserve creation date but update the modification date
                model.CreatedAt = existingScraper.CreatedAt;
                model.LastModified = DateTime.Now;

                var entity = MapModelToEntity(model);
                var result = await _scraperRepository.UpdateScraperAsync(entity);

                var updatedModel = MapEntityToModel(result);
                return Ok(updatedModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating scraper with ID {ScraperId}", id);
                return StatusCode(500, $"An error occurred while updating scraper {id}");
            }
        }

        /// <summary>
        /// Delete a scraper
        /// </summary>
        /// <param name="id">The ID of the scraper to delete</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteScraper(string id)
        {
            try
            {
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }

                var result = await _scraperRepository.DeleteScraperAsync(id);
                if (result)
                {
                    return NoContent();
                }
                else
                {
                    return StatusCode(500, $"Failed to delete scraper {id}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting scraper with ID {ScraperId}", id);
                return StatusCode(500, $"An error occurred while deleting scraper {id}");
            }
        }

        /// <summary>
        /// Start a scraper
        /// </summary>
        /// <param name="id">The ID of the scraper to start</param>
        /// <returns>Status information for the started scraper</returns>
        [HttpPost("{id}/start")]
        [ProducesResponseType(typeof(ScraperStatusEntity), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StartScraper(string id)
        {
            try
            {
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }

                // This will be implemented with the ScraperExecutionService
                // For now, just return a simple status
                var status = new ScraperStatusEntity
                {
                    ScraperId = id,
                    IsRunning = true,
                    StartTime = DateTime.Now,
                    Message = "Scraper started",
                    LastUpdate = DateTime.Now
                };

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting scraper with ID {ScraperId}", id);
                return StatusCode(500, $"An error occurred while starting scraper {id}");
            }
        }

        /// <summary>
        /// Get scraper status
        /// </summary>
        /// <param name="id">The ID of the scraper to get status for</param>
        /// <returns>Status information for the scraper</returns>
        [HttpGet("{id}/status")]
        [ProducesResponseType(typeof(ScraperStatusEntity), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetScraperStatus(string id)
        {
            try
            {
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }

                // Try to get actual status from repository if available
                var status = await _scraperRepository.GetScraperStatusAsync(id);

                // If no status exists yet, create a default one
                if (status == null)
                {
                    status = new ScraperStatusEntity
                    {
                        ScraperId = id,
                        IsRunning = false,
                        UrlsProcessed = 0,
                        HasErrors = false,
                        Message = "Idle",
                        LastUpdate = DateTime.Now
                    };

                    // Save the default status
                    await _scraperRepository.UpdateScraperStatusAsync(status);
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for scraper with ID {ScraperId}", id);
                return StatusCode(500, $"An error occurred while getting status for scraper {id}");
            }
        }

        /// <summary>
        /// Stop a scraper
        /// </summary>
        /// <param name="id">The ID of the scraper to stop</param>
        /// <returns>Status information for the stopped scraper</returns>
        [HttpPost("{id}/stop")]
        [ProducesResponseType(typeof(ScraperStatusEntity), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> StopScraper(string id)
        {
            try
            {
                var scraperConfig = await _scraperRepository.GetScraperByIdAsync(id);
                if (scraperConfig == null)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }

                // Get current status or create new if it doesn't exist
                var status = await _scraperRepository.GetScraperStatusAsync(id);
                if (status == null)
                {
                    status = new ScraperStatusEntity
                    {
                        ScraperId = id,
                        IsRunning = false,
                        UrlsProcessed = 0,
                        HasErrors = false,
                        Message = "Idle",
                        LastUpdate = DateTime.Now
                    };
                }
                else
                {
                    // Update status to stopped
                    status.IsRunning = false;
                    status.Message = "Scraper stopped";
                    status.LastUpdate = DateTime.Now;
                }

                // Save the status
                await _scraperRepository.UpdateScraperStatusAsync(status);

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping scraper with ID {ScraperId}", id);
                return StatusCode(500, $"An error occurred while stopping scraper {id}");
            }
        }

        #region Mapping Methods

        private ScraperConfigModel? MapEntityToModel(ScraperConfigEntity entity)
        {
            if (entity == null)
                return null;

            var model = new ScraperConfigModel
            {
                Id = entity.Id,
                Name = entity.Name,
                CreatedAt = entity.CreatedAt,
                LastModified = entity.LastModified,
                LastRun = entity.LastRun,
                RunCount = entity.RunCount,
                StartUrl = entity.StartUrl,
                BaseUrl = entity.BaseUrl,
                OutputDirectory = entity.OutputDirectory,
                DelayBetweenRequests = entity.DelayBetweenRequests,
                MaxConcurrentRequests = entity.MaxConcurrentRequests,
                MaxDepth = entity.MaxDepth,
                MaxPages = entity.MaxPages,
                FollowLinks = entity.FollowLinks,
                FollowExternalLinks = entity.FollowExternalLinks
            };

            // Map collections if they exist
            if (entity.StartUrls != null)
            {
                model.StartUrls = new List<string>();
                foreach (var url in entity.StartUrls)
                {
                    model.StartUrls.Add(url.Url);
                }
            }

            if (entity.ContentExtractorSelectors != null)
            {
                model.ContentExtractorSelectors = new List<string>();
                model.ContentExtractorExcludeSelectors = new List<string>();

                foreach (var selector in entity.ContentExtractorSelectors)
                {
                    if (selector.IsExclude)
                    {
                        model.ContentExtractorExcludeSelectors.Add(selector.Selector);
                    }
                    else
                    {
                        model.ContentExtractorSelectors.Add(selector.Selector);
                    }
                }
            }

            return model;
        }

        private ScraperConfigEntity? MapModelToEntity(ScraperConfigModel model)
        {
            if (model == null)
                return null;

            // Set default values for required fields if not provided
            var now = DateTime.UtcNow;

            var entity = new ScraperConfigEntity
            {
                Id = string.IsNullOrEmpty(model.Id) ? Guid.NewGuid().ToString() : model.Id,
                Name = model.Name ?? "Unnamed Scraper",
                CreatedAt = now,
                LastModified = now,
                LastRun = model.LastRun,
                RunCount = model.RunCount,
                StartUrl = model.StartUrl ?? "",
                BaseUrl = model.BaseUrl ?? "",
                OutputDirectory = model.OutputDirectory ?? "ScrapedData",
                DelayBetweenRequests = model.DelayBetweenRequests > 0 ? model.DelayBetweenRequests : 1000,
                MaxConcurrentRequests = model.MaxConcurrentRequests > 0 ? model.MaxConcurrentRequests : 5,
                MaxDepth = model.MaxDepth > 0 ? model.MaxDepth : 5,
                MaxPages = model.MaxPages > 0 ? model.MaxPages : 1000,
                FollowLinks = model.FollowLinks,
                FollowExternalLinks = model.FollowExternalLinks,
                StartUrls = new List<ScraperStartUrlEntity>(),
                ContentExtractorSelectors = new List<ContentExtractorSelectorEntity>()
            };

            // Map collections
            if (model.StartUrls != null)
            {
                foreach (var url in model.StartUrls)
                {
                    entity.StartUrls.Add(new ScraperStartUrlEntity
                    {
                        ScraperId = entity.Id,
                        Url = url
                    });
                }
            }

            if (model.ContentExtractorSelectors != null)
            {
                foreach (var selector in model.ContentExtractorSelectors)
                {
                    entity.ContentExtractorSelectors.Add(new ContentExtractorSelectorEntity
                    {
                        ScraperId = entity.Id,
                        Selector = selector,
                        IsExclude = false
                    });
                }
            }

            if (model.ContentExtractorExcludeSelectors != null)
            {
                foreach (var selector in model.ContentExtractorExcludeSelectors)
                {
                    entity.ContentExtractorSelectors.Add(new ContentExtractorSelectorEntity
                    {
                        ScraperId = entity.Id,
                        Selector = selector,
                        IsExclude = true
                    });
                }
            }

            return entity;
        }

        #endregion
    }
}
