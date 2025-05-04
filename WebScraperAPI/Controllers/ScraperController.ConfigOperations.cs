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
        [HttpPost]
        public async Task<IActionResult> CreateScraper([FromBody] ScraperConfigModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Scraper configuration is required");
                }
                
                // Validate basic requirements
                if (string.IsNullOrEmpty(model.Name) || string.IsNullOrEmpty(model.StartUrl))
                {
                    return BadRequest("Scraper name and start URL are required");
                }
                
                // Generate new ID if not provided
                if (string.IsNullOrEmpty(model.Id))
                {
                    model.Id = Guid.NewGuid().ToString();
                }
                
                // Set default values
                model.CreatedAt = DateTime.Now;
                model.LastModified = DateTime.Now;
                
                _logger.LogInformation($"Creating new scraper: {model.Name} with ID {model.Id}");
                
                // Try to save to database first
                try
                {
                    var scraperEntity = new ScraperConfigEntity
                    {
                        Id = model.Id,
                        Name = model.Name,
                        StartUrl = model.StartUrl,
                        BaseUrl = model.BaseUrl,
                        OutputDirectory = model.OutputDirectory,
                        MaxDepth = model.MaxDepth,
                        MaxPages = model.MaxPages,
                        DelayBetweenRequests = model.DelayBetweenRequests,
                        MaxConcurrentRequests = model.MaxConcurrentRequests,
                        FollowLinks = model.FollowLinks,
                        FollowExternalLinks = model.FollowExternalLinks,
                        CreatedAt = model.CreatedAt,
                        LastModified = model.LastModified,
                        LastRun = model.LastRun,
                        RunCount = model.RunCount
                    };
                    
                    await _scraperRepository.CreateScraperAsync(scraperEntity);
                    
                    _logger.LogInformation($"Successfully saved scraper {model.Id} to database");
                    return CreatedAtAction(nameof(GetScraper), new { id = model.Id }, model);
                }
                catch (Exception dbEx)
                {
                    _logger.LogWarning(dbEx, $"Could not save scraper to database, falling back to JSON file: {dbEx.Message}");
                    
                    // Fallback to JSON file
                    var configs = await LoadScraperConfigsAsync();
                    
                    // Check if ID already exists
                    if (configs.Any(c => c.Id == model.Id))
                    {
                        return Conflict($"Scraper with ID {model.Id} already exists");
                    }
                    
                    configs.Add(model);
                    await SaveScraperConfigsAsync(configs);
                    
                    _logger.LogInformation($"Successfully saved scraper {model.Id} to JSON file");
                    return CreatedAtAction(nameof(GetScraper), new { id = model.Id }, model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating scraper");
                return StatusCode(500, new
                {
                    Message = "An error occurred while creating the scraper",
                    Error = ex.Message
                });
            }
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScraper(string id, [FromBody] ScraperConfigModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Scraper configuration is required");
                }
                
                if (id != model.Id)
                {
                    return BadRequest("ID in the URL must match the ID in the request body");
                }
                
                _logger.LogInformation($"Updating scraper with ID {id}");
                
                // Check if the scraper is currently running
                if (_activeScrapers.ContainsKey(id))
                {
                    return BadRequest("Cannot update a scraper while it is running");
                }
                
                // Try to update in database first
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                
                if (dbScraper != null)
                {
                    // Update database entity
                    dbScraper.Name = model.Name;
                    dbScraper.StartUrl = model.StartUrl;
                    dbScraper.BaseUrl = model.BaseUrl;
                    dbScraper.OutputDirectory = model.OutputDirectory;
                    dbScraper.MaxDepth = model.MaxDepth;
                    dbScraper.MaxPages = model.MaxPages;
                    dbScraper.DelayBetweenRequests = model.DelayBetweenRequests;
                    dbScraper.MaxConcurrentRequests = model.MaxConcurrentRequests;
                    dbScraper.FollowLinks = model.FollowLinks;
                    dbScraper.FollowExternalLinks = model.FollowExternalLinks;
                    dbScraper.LastModified = DateTime.Now;
                    
                    await _scraperRepository.UpdateScraperAsync(dbScraper);
                    
                    _logger.LogInformation($"Successfully updated scraper {id} in database");
                    return Ok(model);
                }
                
                // Fallback to JSON file if not in database
                var configs = await LoadScraperConfigsAsync();
                var existingIndex = configs.FindIndex(c => c.Id == id);
                
                if (existingIndex < 0)
                {
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                // Update the model
                model.LastModified = DateTime.Now;
                configs[existingIndex] = model;
                
                await SaveScraperConfigsAsync(configs);
                
                _logger.LogInformation($"Successfully updated scraper {id} in JSON file");
                return Ok(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while updating scraper {id}",
                    Error = ex.Message
                });
            }
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScraper(string id)
        {
            try
            {
                _logger.LogInformation($"Deleting scraper with ID {id}");
                
                // Check if the scraper is currently running
                if (_activeScrapers.ContainsKey(id))
                {
                    return BadRequest("Cannot delete a scraper while it is running");
                }
                
                // Try to delete from database first
                var dbScraper = await _scraperRepository.GetScraperByIdAsync(id);
                
                if (dbScraper != null)
                {
                    await _scraperRepository.DeleteScraperAsync(id);
                    _logger.LogInformation($"Successfully deleted scraper {id} from database");
                }
                else
                {
                    _logger.LogWarning($"Scraper with ID {id} not found in database, checking JSON file");
                }
                
                // Also check and update JSON file
                var configs = await LoadScraperConfigsAsync();
                var existingIndex = configs.FindIndex(c => c.Id == id);
                
                if (existingIndex >= 0)
                {
                    configs.RemoveAt(existingIndex);
                    await SaveScraperConfigsAsync(configs);
                    _logger.LogInformation($"Successfully deleted scraper {id} from JSON file");
                }
                else if (dbScraper == null)
                {
                    // Not found in either database or JSON
                    return NotFound($"Scraper with ID {id} not found");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting scraper {id}");
                return StatusCode(500, new
                {
                    Message = $"An error occurred while deleting scraper {id}",
                    Error = ex.Message
                });
            }
        }
    }
}