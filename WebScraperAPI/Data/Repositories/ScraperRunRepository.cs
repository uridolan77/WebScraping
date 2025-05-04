using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class ScraperRunRepository : IScraperRunRepository
    {
        private readonly WebScraperDbContext _context;

        public ScraperRunRepository(WebScraperDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            return await _context.ScraperRun
                .Where(r => r.ScraperId == scraperId)
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId)
        {
            return await _context.ScraperRun
                .Include(r => r.LogEntries)
                .Include(r => r.ContentChangeRecords)
                .Include(r => r.ProcessedDocuments)
                .FirstOrDefaultAsync(r => r.Id == runId);
        }

        public async Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRun.Add(run);
            await _context.SaveChangesAsync();

            // Update the scraper's LastRun property
            var scraper = await _context.ScraperConfigs.FindAsync(run.ScraperId);
            if (scraper != null)
            {
                scraper.LastRun = run.StartTime;
                scraper.RunCount++;
                await _context.SaveChangesAsync();
            }

            return run;
        }

        public async Task<ScraperRunEntity> UpdateScraperRunAsync(ScraperRunEntity run)
        {
            _context.Entry(run).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return run;
        }

        public async Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100)
        {
            return await _context.LogEntry
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry)
        {
            try
            {
                Console.WriteLine($"Adding log entry to database: ScraperId={logEntry.ScraperId}, Message={logEntry.Message?.Substring(0, Math.Min(50, logEntry.Message?.Length ?? 0))}...");
                
                // Clear any tracked entities to prevent tracking issues
                _context.ChangeTracker.Clear();
                
                // Set timestamp if not already set
                if (logEntry.Timestamp == default)
                {
                    logEntry.Timestamp = DateTime.Now;
                }
                
                // Create a new entity to avoid tracking issues
                var newLogEntry = new LogEntryEntity
                {
                    ScraperId = logEntry.ScraperId,
                    Message = logEntry.Message,
                    Timestamp = logEntry.Timestamp,
                    Level = logEntry.Level ?? "Info", // Using Level instead of LogLevel
                    RunId = logEntry.RunId
                };
                
                // Add the log entry to the context
                _context.LogEntry.Add(newLogEntry);
                
                // Save changes to the database
                await _context.SaveChangesAsync();
                
                // Copy back the generated ID
                logEntry.Id = newLogEntry.Id;
                
                Console.WriteLine($"Successfully added log entry to database with ID: {logEntry.Id}");
                return logEntry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding log entry to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Return the original entry even though it wasn't saved
                return logEntry;
            }
        }

        public async Task<List<ScraperLogEntity>> GetScraperLogsAsync(string scraperId, int limit = 100)
        {
            return await _context.ScraperLog
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperLogEntity> AddScraperLogAsync(ScraperLogEntity logEntry)
        {
            try
            {
                Console.WriteLine($"Adding log entry to database: ScraperId={logEntry.ScraperId}, LogLevel={logEntry.LogLevel}");
                
                // IMPORTANT: Clear the change tracker to avoid EntityState.Unchanged errors
                _context.ChangeTracker.Clear();
                
                // Ensure the ID is 0 for new entities
                logEntry.Id = 0;
                
                // Ensure non-null values for required fields
                logEntry.ScraperId = logEntry.ScraperId ?? string.Empty;
                logEntry.LogLevel = logEntry.LogLevel ?? "Info";
                logEntry.Message = logEntry.Message ?? string.Empty;
                
                // Set timestamp if not already set
                if (logEntry.Timestamp == default)
                {
                    logEntry.Timestamp = DateTime.Now;
                }
                
                // Create a completely new entity to avoid any tracking issues
                var newLogEntry = new ScraperLogEntity
                {
                    ScraperId = logEntry.ScraperId,
                    Timestamp = logEntry.Timestamp,
                    LogLevel = logEntry.LogLevel,
                    Message = logEntry.Message
                };
                
                // Add the new entity to the context
                _context.ScraperLog.Add(newLogEntry);
                
                // Save changes to the database
                await _context.SaveChangesAsync();
                
                // Copy back the generated ID
                logEntry.Id = newLogEntry.Id;
                
                Console.WriteLine($"Successfully added log entry to database with ID: {logEntry.Id}");
                return logEntry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding log entry to database: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Just return the original entry - we've done our best to save it
                return logEntry;
            }
        }

        public async Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            return await _context.ContentChangeRecord
                .Where(c => c.ScraperId == scraperId)
                .OrderByDescending(c => c.DetectedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change)
        {
            _context.ContentChangeRecord.Add(change);
            await _context.SaveChangesAsync();

            return change;
        }
    }
}