using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public class ScraperRepository : IScraperRepository
    {
        private readonly WebScraperDbContext _context;

        public ScraperRepository(WebScraperDbContext context)
        {
            _context = context;
        }

        #region Scraper Config Operations

        public async Task<List<ScraperConfigEntity>> GetAllScrapersAsync()
        {
            return await _context.ScraperConfigs
                .Include(s => s.Status)
                .OrderByDescending(s => s.LastModified)
                .ToListAsync();
        }

        public async Task<ScraperConfigEntity> GetScraperByIdAsync(string id)
        {
            return await _context.ScraperConfigs
                .Include(s => s.Status)
                .Include(s => s.StartUrls)
                .Include(s => s.ContentExtractorSelectors)
                .Include(s => s.KeywordAlerts)
                .Include(s => s.WebhookTriggers)
                .Include(s => s.DomainRateLimits)
                .Include(s => s.ProxyConfigurations)
                .Include(s => s.Schedules)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<ScraperConfigEntity> CreateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.CreatedAt = DateTime.Now;
            scraper.LastModified = DateTime.Now;
            
            _context.ScraperConfigs.Add(scraper);
            await _context.SaveChangesAsync();
            
            return scraper;
        }

        public async Task<ScraperConfigEntity> UpdateScraperAsync(ScraperConfigEntity scraper)
        {
            scraper.LastModified = DateTime.Now;
            
            _context.Entry(scraper).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return scraper;
        }

        public async Task<bool> DeleteScraperAsync(string id)
        {
            var scraper = await _context.ScraperConfigs.FindAsync(id);
            if (scraper == null)
                return false;
                
            _context.ScraperConfigs.Remove(scraper);
            await _context.SaveChangesAsync();
            
            return true;
        }

        #endregion

        #region Scraper Status Operations

        public async Task<ScraperStatusEntity> GetScraperStatusAsync(string scraperId)
        {
            return await _context.ScraperStatuses
                .FirstOrDefaultAsync(s => s.ScraperId == scraperId);
        }

        public async Task<ScraperStatusEntity> UpdateScraperStatusAsync(ScraperStatusEntity status)
        {
            status.LastUpdate = DateTime.Now;
            
            var existingStatus = await _context.ScraperStatuses
                .FirstOrDefaultAsync(s => s.ScraperId == status.ScraperId);
                
            if (existingStatus == null)
            {
                _context.ScraperStatuses.Add(status);
            }
            else
            {
                _context.Entry(existingStatus).CurrentValues.SetValues(status);
            }
            
            await _context.SaveChangesAsync();
            
            return status;
        }

        #endregion

        #region Scraper Run Operations

        public async Task<List<ScraperRunEntity>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            return await _context.ScraperRuns
                .Where(r => r.ScraperId == scraperId)
                .OrderByDescending(r => r.StartTime)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ScraperRunEntity> GetScraperRunByIdAsync(string runId)
        {
            return await _context.ScraperRuns
                .Include(r => r.LogEntries.OrderByDescending(l => l.Timestamp).Take(100))
                .Include(r => r.ContentChangeRecords.OrderByDescending(c => c.DetectedAt).Take(50))
                .Include(r => r.ProcessedDocuments.OrderByDescending(p => p.ProcessedAt).Take(50))
                .FirstOrDefaultAsync(r => r.Id == runId);
        }

        public async Task<ScraperRunEntity> CreateScraperRunAsync(ScraperRunEntity run)
        {
            _context.ScraperRuns.Add(run);
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

        #endregion

        #region Log Entry Operations

        public async Task<List<LogEntryEntity>> GetLogEntriesAsync(string scraperId, int limit = 100)
        {
            return await _context.LogEntries
                .Where(l => l.ScraperId == scraperId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<LogEntryEntity> AddLogEntryAsync(LogEntryEntity logEntry)
        {
            _context.LogEntries.Add(logEntry);
            await _context.SaveChangesAsync();
            
            return logEntry;
        }

        #endregion

        #region Content Change Record Operations

        public async Task<List<ContentChangeRecordEntity>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            return await _context.ContentChangeRecords
                .Where(c => c.ScraperId == scraperId)
                .OrderByDescending(c => c.DetectedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ContentChangeRecordEntity> AddContentChangeAsync(ContentChangeRecordEntity change)
        {
            _context.ContentChangeRecords.Add(change);
            await _context.SaveChangesAsync();
            
            return change;
        }

        #endregion

        #region Processed Document Operations

        public async Task<List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            return await _context.ProcessedDocuments
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.ProcessedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId)
        {
            return await _context.ProcessedDocuments
                .Include(p => p.Metadata)
                .FirstOrDefaultAsync(p => p.Id == documentId);
        }

        public async Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document)
        {
            _context.ProcessedDocuments.Add(document);
            await _context.SaveChangesAsync();
            
            return document;
        }

        #endregion

        #region Metrics Operations

        public async Task<List<ScraperMetricEntity>> GetScraperMetricsAsync(string scraperId, string metricName, DateTime from, DateTime to)
        {
            var query = _context.ScraperMetrics
                .Where(m => m.ScraperId == scraperId && m.Timestamp >= from && m.Timestamp <= to);
                
            if (!string.IsNullOrEmpty(metricName))
            {
                query = query.Where(m => m.MetricName == metricName);
            }
            
            return await query
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }

        public async Task<ScraperMetricEntity> AddScraperMetricAsync(ScraperMetricEntity metric)
        {
            _context.ScraperMetrics.Add(metric);
            await _context.SaveChangesAsync();
            
            return metric;
        }

        #endregion

        #region Pipeline Metrics Operations

        public async Task<PipelineMetricEntity> GetLatestPipelineMetricAsync(string scraperId)
        {
            return await _context.PipelineMetrics
                .Where(p => p.ScraperId == scraperId)
                .OrderByDescending(p => p.Timestamp)
                .FirstOrDefaultAsync();
        }

        public async Task<PipelineMetricEntity> AddPipelineMetricAsync(PipelineMetricEntity metric)
        {
            _context.PipelineMetrics.Add(metric);
            await _context.SaveChangesAsync();
            
            return metric;
        }

        #endregion
    }
}
