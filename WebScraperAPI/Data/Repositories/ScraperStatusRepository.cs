using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class ScraperStatusRepository : IScraperStatusRepository
    {
        private readonly WebScraperDbContext _context;

        public ScraperStatusRepository(WebScraperDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId)
        {
            try
            {
                var status = await _context.ScraperStatuses
                    .FirstOrDefaultAsync(s => s.ScraperId == scraperId);

                // Ensure non-nullable string properties have default values
                if (status != null)
                {
                    status.Message = status.Message ?? string.Empty;
                    status.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    status.LastError = status.LastError ?? string.Empty;
                }

                return status;
            }
            catch (InvalidCastException ex)
            {
                // If we get an InvalidCastException, it's likely due to NULL values in the database
                // Let's create a new status object with default values
                return new ScraperStatusEntity
                {
                    ScraperId = scraperId,
                    IsRunning = false,
                    UrlsProcessed = 0,
                    HasErrors = false,
                    Message = "Idle",
                    ElapsedTime = string.Empty,
                    LastError = string.Empty,
                    LastUpdate = DateTime.Now
                };
            }
        }

        public async Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status)
        {
            status.LastUpdate = DateTime.Now;

            // Ensure non-nullable string properties have default values
            status.Message = status.Message ?? string.Empty;
            status.ElapsedTime = status.ElapsedTime ?? string.Empty;
            status.LastError = status.LastError ?? string.Empty;

            var existingStatus = await _context.ScraperStatuses
                .FirstOrDefaultAsync(s => s.ScraperId == status.ScraperId);

            if (existingStatus == null)
            {
                _context.ScraperStatuses.Add(status);
            }
            else
            {
                // Handle NULL values in the database
                try
                {
                    _context.Entry(existingStatus).CurrentValues.SetValues(status);
                }
                catch (InvalidCastException ex)
                {
                    // If we get an InvalidCastException, it's likely due to NULL values in the database
                    // Let's handle each property individually
                    existingStatus.IsRunning = status.IsRunning;
                    existingStatus.StartTime = status.StartTime;
                    existingStatus.EndTime = status.EndTime;
                    existingStatus.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    existingStatus.UrlsProcessed = status.UrlsProcessed;
                    existingStatus.UrlsQueued = status.UrlsQueued;
                    existingStatus.DocumentsProcessed = status.DocumentsProcessed;
                    existingStatus.HasErrors = status.HasErrors;
                    existingStatus.Message = status.Message ?? string.Empty;
                    existingStatus.LastStatusUpdate = status.LastStatusUpdate;
                    existingStatus.LastUpdate = status.LastUpdate;
                    existingStatus.LastMonitorCheck = status.LastMonitorCheck;
                    existingStatus.LastError = status.LastError ?? string.Empty;
                }
            }

            await _context.SaveChangesAsync();

            return status;
        }

        public async Task<List<ScraperStatusEntity>> GetAllRunningScrapersAsync()
        {
            try
            {
                // Get all scraper statuses where IsRunning is true
                var runningScrapers = await _context.ScraperStatuses
                    .Where(s => s.IsRunning)
                    .ToListAsync();

                // Ensure non-nullable string properties have default values
                foreach (var status in runningScrapers)
                {
                    status.Message = status.Message ?? string.Empty;
                    status.ElapsedTime = status.ElapsedTime ?? string.Empty;
                    status.LastError = status.LastError ?? string.Empty;
                }

                return runningScrapers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting running scrapers: {ex.Message}");
                return new List<ScraperStatusEntity>();
            }
        }
    }
}