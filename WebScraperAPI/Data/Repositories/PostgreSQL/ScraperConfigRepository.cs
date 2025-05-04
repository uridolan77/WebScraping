using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories.PostgreSQL
{
    /// <summary>
    /// PostgreSQL implementation of the IScraperConfigRepository interface
    /// </summary>
    public class ScraperConfigRepository : IScraperConfigRepository
    {
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ScraperConfigRepository class
        /// </summary>
        /// <param name="context">The database context</param>
        public ScraperConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public bool TestDatabaseConnection()
        {
            try
            {
                // Try to connect to the database
                return _context.Database.CanConnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database connection error: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<List<ScraperConfigEntity>> GetAllScrapersAsync()
        {
            return await _context.ScraperConfigs
                .AsNoTracking()
                .OrderByDescending(s => s.LastModified)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ScraperConfigEntity> GetScraperByIdAsync(string id)
        {
            return await _context.ScraperConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <inheritdoc/>
        public async Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.CreatedAt = DateTime.Now;
            scraper.LastModified = DateTime.Now;

            _context.ScraperConfigs.Add(scraper);
            await _context.SaveChangesAsync();
            return scraper;
        }

        /// <inheritdoc/>
        public async Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.LastModified = DateTime.Now;
            _context.Entry(scraper).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return scraper;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ScraperConfigs.AnyAsync(c => c.Id == scraper.Id))
                {
                    throw new KeyNotFoundException($"Scraper with ID {scraper.Id} not found");
                }
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteScraperAsync(string id)
        {
            var config = await _context.ScraperConfigs.FindAsync(id);
            if (config == null)
            {
                return false;
            }

            _context.ScraperConfigs.Remove(config);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc/>
        public async Task<List<ScraperStartUrlEntity>> GetScraperStartUrlsAsync(string scraperId)
        {
            return await _context.ScraperStartUrls
                .AsNoTracking()
                .Where(s => s.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ContentExtractorSelectorEntity>> GetContentExtractorSelectorsAsync(string scraperId)
        {
            return await _context.ContentExtractorSelectors
                .AsNoTracking()
                .Where(s => s.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<KeywordAlertEntity>> GetKeywordAlertsAsync(string scraperId)
        {
            return await _context.KeywordAlerts
                .AsNoTracking()
                .Where(k => k.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<DomainRateLimitEntity>> GetDomainRateLimitsAsync(string scraperId)
        {
            return await _context.DomainRateLimits
                .AsNoTracking()
                .Where(d => d.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ProxyConfigurationEntity>> GetProxyConfigurationsAsync(string scraperId)
        {
            return await _context.ProxyConfigurations
                .AsNoTracking()
                .Where(p => p.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<WebhookTriggerEntity>> GetWebhookTriggersAsync(string scraperId)
        {
            return await _context.WebhookTriggers
                .AsNoTracking()
                .Where(w => w.ScraperId == scraperId)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<ScraperScheduleEntity>> GetScraperSchedulesAsync(string scraperId)
        {
            return await _context.ScraperSchedules
                .AsNoTracking()
                .Where(s => s.ScraperId == scraperId)
                .ToListAsync();
        }

        // Legacy methods kept for compatibility
        public async Task<IEnumerable<ScraperConfigEntity>> GetAllAsync()
        {
            return await GetAllScrapersAsync();
        }

        public async Task<ScraperConfigEntity> GetByIdAsync(Guid id)
        {
            return await GetScraperByIdAsync(id.ToString());
        }

        public async Task<ScraperConfigEntity> CreateAsync(ScraperConfigEntity config)
        {
            return await CreateScraperAsync(config);
        }

        public async Task<bool> UpdateAsync(ScraperConfigEntity config)
        {
            await UpdateScraperAsync(config);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            return await DeleteScraperAsync(id.ToString());
        }

        public async Task<IEnumerable<LogEntryEntity>> GetLogsAsync(Guid scraperId, int limit = 100)
        {
            return await _context.ScraperLogs
                .Where(l => l.ScraperId == scraperId.ToString())
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task AddLogAsync(LogEntryEntity log)
        {
            _context.ScraperLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ScraperRunEntity>> GetRunsAsync(Guid scraperId, int limit = 10)
        {
            return await _context.ScraperRuns
                .Where(r => r.ScraperId == scraperId.ToString())
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperRunEntity> AddRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRuns.Add(run);
            await _context.SaveChangesAsync();
            return run;
        }

        public async Task<bool> UpdateRunAsync(ScraperRunEntity run)
        {
            _context.Entry(run).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ScraperRuns.AnyAsync(r => r.Id == run.Id))
                {
                    return false;
                }
                throw;
            }
        }
    }
}
