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
            try
            {
                Console.WriteLine($"GetScraperRunsAsync: Fetching runs for scraper {scraperId}");

                // Use standard LINQ query instead of FromSqlRaw
                var runs = await _context.ScraperRun
                    .Where(r => r.ScraperId == scraperId)
                    .OrderByDescending(r => r.StartTime)
                    .Take(limit)
                    .AsNoTracking()
                    .ToListAsync();

                Console.WriteLine($"GetScraperRunsAsync: Successfully retrieved {runs.Count} runs for scraper {scraperId}");
                return runs;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GetScraperRunsAsync ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return an empty list in case of error
                return new List<ScraperRunEntity>();
            }
        }

        public async Task<ScraperRunEntity?> GetScraperRunByIdAsync(string runId)
        {
            try
            {
                // Use standard LINQ query instead of FromSqlRaw
                var run = await _context.ScraperRun
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.Id == runId);

                if (run != null)
                {
                    try
                    {
                        // Load related entities separately to avoid NULL reference issues
                        run.LogEntries = await _context.LogEntry
                            .Where(l => l.RunId == runId)
                            .ToListAsync();
                    }
                    catch
                    {
                        run.LogEntries = new List<LogEntryEntity>();
                    }

                    // Skip loading ContentChangeRecords for now due to schema mismatch
                    run.ContentChangeRecords = new List<ContentChangeRecordEntity>();

                    try
                    {
                        run.ProcessedDocuments = await _context.ProcessedDocument
                            .Where(p => p.RunId == runId)
                            .ToListAsync();
                    }
                    catch
                    {
                        run.ProcessedDocuments = new List<ProcessedDocumentEntity>();
                    }
                }

                return run;
            }
            catch (Exception ex)
            {
                // Log only in case of error
                Console.WriteLine($"GetScraperRunByIdAsync ERROR: {ex.Message}");

                // Return null in case of error
                return null;
            }
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
            try
            {
                Console.WriteLine($"UpdateScraperRunAsync: Updating run with ID {run.Id}");

                // Clear any tracked entities to prevent tracking conflicts
                _context.ChangeTracker.Clear();

                // Get the existing entity from the database
                var existingRun = await _context.ScraperRun
                    .FirstOrDefaultAsync(r => r.Id == run.Id);

                if (existingRun == null)
                {
                    Console.WriteLine($"UpdateScraperRunAsync: No run found with ID {run.Id}, creating new run");
                    _context.ScraperRun.Add(run);
                }
                else
                {
                    // Update the existing entity's properties
                    existingRun.UrlsProcessed = run.UrlsProcessed;
                    existingRun.DocumentsProcessed = run.DocumentsProcessed;
                    existingRun.EndTime = run.EndTime;
                    existingRun.Successful = run.Successful;
                    existingRun.ErrorMessage = run.ErrorMessage ?? string.Empty;
                    existingRun.ElapsedTime = run.ElapsedTime ?? string.Empty;

                    // Mark as modified
                    _context.Entry(existingRun).State = EntityState.Modified;
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"UpdateScraperRunAsync: Successfully updated run with ID {run.Id}");

                return run;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UpdateScraperRunAsync ERROR: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                // Return the original run even though it wasn't updated
                return run;
            }
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