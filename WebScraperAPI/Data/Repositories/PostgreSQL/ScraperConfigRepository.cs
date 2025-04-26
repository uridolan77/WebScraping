using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperAPI.Data.Entities;

namespace WebScraperAPI.Data.Repositories.PostgreSQL
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
        public async Task<IEnumerable<ScraperConfig>> GetAllAsync()
        {
            return await _context.ScraperConfigs
                .AsNoTracking()
                .ToListAsync();
        }
        
        /// <inheritdoc/>
        public async Task<ScraperConfig> GetByIdAsync(Guid id)
        {
            return await _context.ScraperConfigs
                .Include(c => c.Runs.OrderByDescending(r => r.StartTime).Take(5))
                .FirstOrDefaultAsync(c => c.Id == id);
        }
        
        /// <inheritdoc/>
        public async Task<ScraperConfig> CreateAsync(ScraperConfig config)
        {
            _context.ScraperConfigs.Add(config);
            await _context.SaveChangesAsync();
            return config;
        }
        
        /// <inheritdoc/>
        public async Task<bool> UpdateAsync(ScraperConfig config)
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
        public async Task<IEnumerable<ScraperLog>> GetLogsAsync(Guid scraperId, int limit = 100)
        {
            return await _context.ScraperLogs
                .Where(l => l.ScraperConfigId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }
        
        /// <inheritdoc/>
        public async Task AddLogAsync(ScraperLog log)
        {
            _context.ScraperLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<ScraperRun>> GetRunsAsync(Guid scraperId, int limit = 10)
        {
            return await _context.ScraperRuns
                .Where(r => r.ScraperConfigId == scraperId)
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }
        
        /// <inheritdoc/>
        public async Task<ScraperRun> AddRunAsync(ScraperRun run)
        {
            _context.ScraperRuns.Add(run);
            await _context.SaveChangesAsync();
            return run;
        }
        
        /// <inheritdoc/>
        public async Task<bool> UpdateRunAsync(ScraperRun run)
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
