using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WebScraperAPI.Data.Entities;

namespace WebScraperAPI.Data.Repositories.MongoDB
{
    /// <summary>
    /// MongoDB implementation of the IScrapedContentRepository interface
    /// </summary>
    public class ScrapedContentRepository : IScrapedContentRepository
    {
        private readonly IMongoCollection<ScrapedContent> _contents;
        private readonly IMongoCollection<ContentVersion> _versions;
        
        /// <summary>
        /// Initializes a new instance of the ScrapedContentRepository class
        /// </summary>
        /// <param name="mongoClient">The MongoDB client</param>
        /// <param name="databaseName">The database name</param>
        public ScrapedContentRepository(IMongoClient mongoClient, string databaseName = "WebScraper")
        {
            var database = mongoClient.GetDatabase(databaseName);
            _contents = database.GetCollection<ScrapedContent>("scrapedContents");
            _versions = database.GetCollection<ContentVersion>("contentVersions");
            
            // Create indexes
            CreateIndexes();
        }
        
        /// <inheritdoc/>
        public async Task<ScrapedContent> GetByUrlAsync(string url)
        {
            return await _contents.Find(c => c.Url == url).FirstOrDefaultAsync();
        }
        
        /// <inheritdoc/>
        public async Task<ScrapedContent> GetByIdAsync(string id)
        {
            return await _contents.Find(c => c.Id == id).FirstOrDefaultAsync();
        }
        
        /// <inheritdoc/>
        public async Task<(IEnumerable<ScrapedContent> Items, int TotalCount)> GetByScraperIdAsync(
            Guid scraperId, int page = 1, int pageSize = 20)
        {
            var filter = Builders<ScrapedContent>.Filter.Eq(c => c.ScraperConfigId, scraperId);
            var totalCount = await _contents.CountDocumentsAsync(filter);
            
            var items = await _contents.Find(filter)
                .Sort(Builders<ScrapedContent>.Sort.Descending(c => c.ScrapedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
                
            return (items, (int)totalCount);
        }
        
        /// <inheritdoc/>
        public async Task<ScrapedContent> SaveContentAsync(ScrapedContent content)
        {
            var filter = Builders<ScrapedContent>.Filter.Eq(c => c.Url, content.Url);
            var existing = await _contents.Find(filter).FirstOrDefaultAsync();
            
            if (existing != null)
            {
                content.Id = existing.Id;
                await _contents.ReplaceOneAsync(filter, content);
            }
            else
            {
                if (string.IsNullOrEmpty(content.Id))
                {
                    content.Id = ObjectId.GenerateNewId().ToString();
                }
                await _contents.InsertOneAsync(content);
            }
            
            return content;
        }
        
        /// <inheritdoc/>
        public async Task<ContentVersion> AddVersionAsync(ContentVersion version)
        {
            if (string.IsNullOrEmpty(version.Id))
            {
                version.Id = ObjectId.GenerateNewId().ToString();
            }
            
            await _versions.InsertOneAsync(version);
            return version;
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<ContentVersion>> GetVersionsAsync(string contentId, int limit = 5)
        {
            return await _versions
                .Find(v => v.ContentId == contentId)
                .Sort(Builders<ContentVersion>.Sort.Descending(v => v.VersionDate))
                .Limit(limit)
                .ToListAsync();
        }
        
        /// <inheritdoc/>
        public async Task<(IEnumerable<ScrapedContent> Items, int TotalCount)> SearchAsync(
            string query, Guid? scraperId = null, int page = 1, int pageSize = 20)
        {
            var builder = Builders<ScrapedContent>.Filter;
            var filter = builder.Text(query);
            
            if (scraperId.HasValue)
            {
                filter = builder.And(filter, builder.Eq(c => c.ScraperConfigId, scraperId.Value));
            }
            
            var totalCount = await _contents.CountDocumentsAsync(filter);
            
            var items = await _contents.Find(filter)
                .Sort(Builders<ScrapedContent>.Sort.Descending(c => c.ScrapedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();
                
            return (items, (int)totalCount);
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<ScrapedContent>> GetChangedContentSinceAsync(
            DateTime since, Guid? scraperId = null)
        {
            var builder = Builders<ScrapedContent>.Filter;
            var filter = builder.Gte(c => c.ScrapedAt, since);
            
            if (scraperId.HasValue)
            {
                filter = builder.And(filter, builder.Eq(c => c.ScraperConfigId, scraperId.Value));
            }
            
            return await _contents.Find(filter)
                .Sort(Builders<ScrapedContent>.Sort.Descending(c => c.ScrapedAt))
                .ToListAsync();
        }
        
        /// <inheritdoc/>
        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _contents.DeleteOneAsync(c => c.Id == id);
            
            // Also delete all versions
            await _versions.DeleteManyAsync(v => v.ContentId == id);
            
            return result.DeletedCount > 0;
        }
        
        /// <inheritdoc/>
        public async Task<int> DeleteByScraperIdAsync(Guid scraperId)
        {
            var contentFilter = Builders<ScrapedContent>.Filter.Eq(c => c.ScraperConfigId, scraperId);
            var contentResult = await _contents.DeleteManyAsync(contentFilter);
            
            var versionFilter = Builders<ContentVersion>.Filter.Eq(v => v.ScraperConfigId, scraperId);
            await _versions.DeleteManyAsync(versionFilter);
            
            return (int)contentResult.DeletedCount;
        }
        
        private void CreateIndexes()
        {
            // Create indexes for ScrapedContent collection
            var contentIndexes = _contents.Indexes;
            
            // URL index (unique)
            var urlIndexModel = new CreateIndexModel<ScrapedContent>(
                Builders<ScrapedContent>.IndexKeys.Ascending(c => c.Url),
                new CreateIndexOptions { Unique = true });
                
            // ScraperConfigId index
            var scraperIdIndexModel = new CreateIndexModel<ScrapedContent>(
                Builders<ScrapedContent>.IndexKeys.Ascending(c => c.ScraperConfigId));
                
            // Text index for search
            var textIndexModel = new CreateIndexModel<ScrapedContent>(
                Builders<ScrapedContent>.IndexKeys.Text(c => c.ProcessedContent)
                    .Text(c => c.Title)
                    .Text(c => c.Url));
                    
            // Date index
            var dateIndexModel = new CreateIndexModel<ScrapedContent>(
                Builders<ScrapedContent>.IndexKeys.Descending(c => c.ScrapedAt));
                
            contentIndexes.CreateMany(new[] { urlIndexModel, scraperIdIndexModel, textIndexModel, dateIndexModel });
            
            // Create indexes for ContentVersion collection
            var versionIndexes = _versions.Indexes;
            
            // ContentId index
            var contentIdIndexModel = new CreateIndexModel<ContentVersion>(
                Builders<ContentVersion>.IndexKeys.Ascending(v => v.ContentId));
                
            // ScraperConfigId index
            var versionScraperIdIndexModel = new CreateIndexModel<ContentVersion>(
                Builders<ContentVersion>.IndexKeys.Ascending(v => v.ScraperConfigId));
                
            // Date index
            var versionDateIndexModel = new CreateIndexModel<ContentVersion>(
                Builders<ContentVersion>.IndexKeys.Descending(v => v.VersionDate));
                
            versionIndexes.CreateMany(new[] { contentIdIndexModel, versionScraperIdIndexModel, versionDateIndexModel });
        }
    }
}
