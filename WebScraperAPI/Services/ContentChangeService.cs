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
    public class ContentChangeService : BaseService
    {
        public ContentChangeService(IScraperRepository repository, ILogger<ContentChangeService> logger)
            : base(repository, logger)
        {
        }

        public async Task<List<ContentChangeRecord>> GetContentChangesAsync(string scraperId, int limit = 50)
        {
            try
            {
                var changes = await _repository.GetContentChangesAsync(scraperId, limit);
                return changes.Select(e => MapToModel(e)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content changes for scraper with ID {ScraperId}", scraperId);
                throw;
            }
        }

        #region Mapping Methods

        private ContentChangeRecord MapToModel(WebScraperApi.Data.ContentChangeRecordEntity entity)
        {
            if (entity == null)
                return null;

            var model = new ContentChangeRecord
            {
                Url = entity.Url,
                ChangeType = ParseChangeType(entity.ChangeType),
                DetectedAt = entity.DetectedAt,
                Significance = entity.Significance,
                ChangeDetails = entity.ChangeDetails
            };

            return model;
        }

        private ContentChangeType ParseChangeType(string changeType)
        {
            if (Enum.TryParse<ContentChangeType>(changeType, out var result))
            {
                return result;
            }
            return ContentChangeType.Other;
        }

        #endregion
    }
}
