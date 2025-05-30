using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    public class ScraperRunService : BaseService
    {
        public ScraperRunService(IScraperRepository repository, ILogger<ScraperRunService> logger)
            : base(repository, logger)
        {
        }

        public async Task<List<ScraperRun>> GetScraperRunsAsync(string scraperId, int limit = 10)
        {
            try
            {
                var runs = await _repository.GetScraperRunsAsync(scraperId, limit);
                return runs.Select(e => MapToModel(e)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting runs for scraper with ID {ScraperId}", scraperId);
                throw;
            }
        }

        public async Task<ScraperRun> GetScraperRunByIdAsync(string runId)
        {
            try
            {
                var run = await _repository.GetScraperRunByIdAsync(runId);
                if (run != null)
                {
                    return MapToModel(run);
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting run with ID {RunId}", runId);
                throw;
            }
        }

        #region Mapping Methods

        private ScraperRun MapToModel(WebScraperApi.Data.ScraperRunEntity entity)
        {
            if (entity == null)
                return null;

            var model = new ScraperRun
            {
                Id = entity.Id,
                ScraperId = entity.ScraperId,
                StartTime = entity.StartTime,
                EndTime = entity.EndTime,
                UrlsProcessed = entity.UrlsProcessed,
                DocumentsProcessed = entity.DocumentsProcessed,
                Successful = entity.Successful ?? false,
                ErrorMessage = entity.ErrorMessage,
                ElapsedTime = entity.ElapsedTime
            };

            return model;
        }

        #endregion
    }
}
