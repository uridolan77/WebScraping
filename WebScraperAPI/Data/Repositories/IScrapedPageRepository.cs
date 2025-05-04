using System.Threading.Tasks;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories
{
    public interface IScrapedPageRepository
    {
        // Scraped Page operations
        Task<ScrapedPageEntity> AddScrapedPageAsync(ScrapedPageEntity page);
        
        // Processed Document operations
        Task<System.Collections.Generic.List<ProcessedDocumentEntity>> GetProcessedDocumentsAsync(string scraperId, int limit = 50);
        Task<ProcessedDocumentEntity> GetDocumentByIdAsync(string documentId);
        Task<ProcessedDocumentEntity> AddProcessedDocumentAsync(ProcessedDocumentEntity document);
        
        // Content Classification operations
        Task<object> GetContentClassificationAsync(string scraperId, string url);
        Task<System.Collections.Generic.List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50);
        Task<object> SaveContentClassificationAsync(object classification);
    }
}