using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebScraperApi.Data.Entities;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Models;

namespace WebScraperApi.Services
{
    public class DocumentService : BaseService
    {
        public DocumentService(IScraperRepository repository, ILogger<DocumentService> logger)
            : base(repository, logger)
        {
        }

        public async Task<List<ProcessedDocument>> GetProcessedDocumentsAsync(string scraperId, int limit = 50)
        {
            try
            {
                var documents = await _repository.GetProcessedDocumentsAsync(scraperId, limit);
                return documents.Select(MapToModel).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting processed documents for scraper with ID {ScraperId}", scraperId);
                throw;
            }
        }

        public async Task<ProcessedDocument> GetDocumentByIdAsync(string documentId)
        {
            try
            {
                var document = await _repository.GetDocumentByIdAsync(documentId);
                return document != null ? MapToModel(document) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting document with ID {DocumentId}", documentId);
                throw;
            }
        }

        #region Mapping Methods

        private ProcessedDocument MapToModel(ProcessedDocumentEntity entity)
        {
            if (entity == null)
                return null;

            var model = new ProcessedDocument
            {
                Id = entity.Id,
                Url = entity.Url,
                Title = entity.Title,
                DocumentType = entity.DocumentType,
                ProcessedAt = entity.ProcessedAt,
                ContentSizeBytes = entity.ContentSizeBytes,
                Metadata = entity.Metadata?.ToDictionary(m => m.MetaKey, m => m.MetaValue) ?? new Dictionary<string, string>()
            };

            return model;
        }

        #endregion
    }
}
