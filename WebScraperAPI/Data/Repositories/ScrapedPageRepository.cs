using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class ScrapedPageRepository : IScrapedPageRepository
    {
        private readonly WebScraperDbContext _context;
        private readonly IScraperStatusRepository _statusRepository;

        public ScrapedPageRepository(WebScraperDbContext context, IScraperStatusRepository statusRepository)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _statusRepository = statusRepository ?? throw new ArgumentNullException(nameof(statusRepository));
        }

        public async Task<ScrapedPageEntity> AddScrapedPageAsync(ScrapedPageEntity page)
        {
            try
            {
                Console.WriteLine($"Adding scraped page to database: URL={page.Url}, ScraperId={page.ScraperId}");

                // Check if the ScrapedPage DbSet exists
                if (_context.ScrapedPage == null)
                {
                    Console.WriteLine("Error: ScrapedPage DbSet is null");
                    throw new InvalidOperationException("ScrapedPage DbSet is null");
                }

                // IMPORTANT: Clear the change tracker to avoid EntityState.Unchanged errors
                _context.ChangeTracker.Clear();

                // Get the scraper configuration to check MaxPages limit
                var scraperConfig = await _context.ScraperConfigs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.Id == page.ScraperId);
                
                if (scraperConfig != null)
                {
                    // Get current page count for this scraper
                    var currentPageCount = await _context.ScrapedPage
                        .CountAsync(p => p.ScraperId == page.ScraperId);
                    
                    // Check if we've reached the MaxPages limit
                    if (scraperConfig.MaxPages > 0 && currentPageCount >= scraperConfig.MaxPages)
                    {
                        Console.WriteLine($"MaxPages limit of {scraperConfig.MaxPages} reached for scraper {page.ScraperId}. Not adding page: {page.Url}");
                        
                        // Update scraper status to indicate completion
                        await UpdateScraperStatusForMaxPagesReached(page.ScraperId, currentPageCount, scraperConfig.MaxPages);
                        
                        // Return the page without saving it
                        return page;
                    }
                    
                    Console.WriteLine($"Current page count: {currentPageCount}, MaxPages limit: {scraperConfig.MaxPages}");
                }

                // Create a new, detached entity to avoid tracking issues
                var newPage = new ScrapedPageEntity
                {
                    ScraperId = page.ScraperId,
                    Url = page.Url,
                    HtmlContent = page.HtmlContent,
                    TextContent = page.TextContent,
                    ScrapedAt = page.ScrapedAt
                };

                // Add the entity to the context
                _context.ScrapedPage.Add(newPage);
                Console.WriteLine($"Added scraped page to context, saving changes...");

                // Save changes to the database
                await _context.SaveChangesAsync();
                
                // Copy the generated ID back to the original page
                page.Id = newPage.Id;
                Console.WriteLine($"Successfully saved scraped page to database with ID: {page.Id}");

                // Update the scraper status with a separate operation to avoid tracking issues
                try
                {
                    // Clear tracking before updating status
                    _context.ChangeTracker.Clear();
                    
                    Console.WriteLine("Updating scraper status with latest page information");
                    var status = await _context.ScraperStatuses
                        .AsNoTracking() // Use AsNoTracking to avoid EntityState issues
                        .FirstOrDefaultAsync(s => s.ScraperId == page.ScraperId);

                    if (status != null)
                    {
                        // Create a new status entity
                        var updatedStatus = new ScraperStatusEntity
                        {
                            ScraperId = status.ScraperId,
                            IsRunning = status.IsRunning,
                            StartTime = status.StartTime,
                            EndTime = status.EndTime,
                            UrlsProcessed = status.UrlsProcessed + 1,
                            DocumentsProcessed = status.DocumentsProcessed + 1,
                            UrlsQueued = status.UrlsQueued,
                            HasErrors = status.HasErrors,
                            Message = $"Processing content. {status.UrlsProcessed + 1} pages processed.",
                            LastStatusUpdate = DateTime.Now,
                            LastUpdate = DateTime.Now,
                            LastMonitorCheck = status.LastMonitorCheck,
                            LastError = status.LastError ?? string.Empty
                        };

                        // Calculate elapsed time if we have a start time
                        if (status.StartTime.HasValue)
                        {
                            TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                            updatedStatus.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                        }
                        else
                        {
                            updatedStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                        }

                        // Check if we've reached the MaxPages limit
                        if (scraperConfig != null && scraperConfig.MaxPages > 0)
                        {
                            var newPageCount = await _context.ScrapedPage
                                .CountAsync(p => p.ScraperId == page.ScraperId);
                                
                            if (newPageCount >= scraperConfig.MaxPages)
                            {
                                updatedStatus.Message = $"Completed. Reached maximum of {scraperConfig.MaxPages} pages.";
                                updatedStatus.IsRunning = false;
                                updatedStatus.EndTime = DateTime.Now;
                            }
                        }

                        // Use the status repository to update status
                        await _statusRepository.UpdateScraperStatusAsync(updatedStatus);
                        Console.WriteLine($"Updated scraper status with UrlsProcessed = {updatedStatus.UrlsProcessed}, ElapsedTime = {updatedStatus.ElapsedTime}");
                    }
                    else
                    {
                        Console.WriteLine($"No scraper status found for ScraperId: {page.ScraperId}");
                    }
                }
                catch (Exception statusEx)
                {
                    Console.WriteLine($"Error updating scraper status with page info: {statusEx.Message}");
                    if (statusEx.InnerException != null)
                    {
                        Console.WriteLine($"Inner exception: {statusEx.InnerException.Message}");
                    }
                    // Don't rethrow - we don't want status update failure to affect page saving
                }

                return page;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding scraped page to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Rethrow the exception to be handled by the caller
                throw;
            }
        }
        
        private async Task UpdateScraperStatusForMaxPagesReached(string scraperId, int pageCount, int maxPages)
        {
            try
            {
                // Clear tracking before updating status
                _context.ChangeTracker.Clear();
                
                var status = await _context.ScraperStatuses
                    .AsNoTracking()
                    .FirstOrDefaultAsync(s => s.ScraperId == scraperId);
                
                if (status != null)
                {
                    var updatedStatus = new ScraperStatusEntity
                    {
                        ScraperId = status.ScraperId,
                        IsRunning = false, // Set to false since we're stopping due to max pages
                        StartTime = status.StartTime,
                        EndTime = DateTime.Now, // Set end time
                        UrlsProcessed = pageCount,
                        DocumentsProcessed = pageCount,
                        UrlsQueued = status.UrlsQueued,
                        HasErrors = status.HasErrors,
                        Message = $"Completed. Reached maximum of {maxPages} pages.",
                        LastStatusUpdate = DateTime.Now,
                        LastUpdate = DateTime.Now,
                        LastMonitorCheck = status.LastMonitorCheck,
                        LastError = status.LastError ?? string.Empty
                    };
                    
                    // Calculate elapsed time if we have a start time
                    if (status.StartTime.HasValue)
                    {
                        TimeSpan elapsed = DateTime.Now - status.StartTime.Value;
                        updatedStatus.ElapsedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
                    }
                    else
                    {
                        updatedStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    }
                    
                    await _statusRepository.UpdateScraperStatusAsync(updatedStatus);
                    Console.WriteLine($"Updated scraper status to indicate MaxPages limit reached: {maxPages} pages");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating scraper status for MaxPages reached: {ex.Message}");
                // Don't rethrow - this is a helper method
            }
        }

        public async Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            return await _context.ProcessedDocument
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.ProcessedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId)
        {
            return await _context.ProcessedDocument
                .Include(p => p.Metadata)
                .FirstOrDefaultAsync(p => p.Id == documentId);
        }

        public async Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document)
        {
            _context.ProcessedDocument.Add(document);
            await _context.SaveChangesAsync();

            return document;
        }

        public async Task<object> GetContentClassificationAsync(string scraperId, string url)
        {
            return await Task.FromResult<object>(null);
        }

        public async Task<List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50)
        {
            return await Task.FromResult(new List<object>());
        }

        public async Task<object> SaveContentClassificationAsync(object classification)
        {
            return await Task.FromResult(classification);
        }
    }
}