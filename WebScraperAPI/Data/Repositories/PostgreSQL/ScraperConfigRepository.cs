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
        public async Task<IEnumerable<ScraperConfigEntity>> GetAllAsync()
        {
            return await _context.ScraperConfigs
                .AsNoTracking()
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ScraperConfigEntity> GetByIdAsync(Guid id)
        {
            return await _context.ScraperConfigs
                .Include(c => c.Runs.OrderByDescending(r => r.StartTime).Take(5))
                .FirstOrDefaultAsync(c => c.Id == id.ToString());
        }

        /// <inheritdoc/>
        public async Task<ScraperConfigEntity> CreateAsync(ScraperConfigEntity config)
        {
            _context.ScraperConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }

        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(ScraperConfigEntity config)
        {
            _context.Entry(config).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.ScraperConfigs.AnyAsync(c => c.Id == config.Id))
                {
                    return false;
                }
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(Guid id)
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
        public async Task<IEnumerable<LogEntryEntity>> GetLogsAsync(Guid scraperId, int limit = 100)
        {
            return await _context.ScraperLogs
                .Where(l => l.ScraperId == scraperId.ToString())
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task AddLogAsync(LogEntryEntity log)
        {
            _context.ScraperLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ScraperRunEntity>> GetRunsAsync(Guid scraperId, int limit = 10)
        {
            return await _context.ScraperRuns
                .Where(r => r.ScraperId == scraperId.ToString())
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ScraperRunEntity> AddRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRuns.Add(run);
            await _context.SaveChangesAsync();
            return run;
        }

        /// <inheritdoc/>
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
