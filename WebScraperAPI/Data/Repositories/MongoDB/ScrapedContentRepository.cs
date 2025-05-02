using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data.Repositories.MongoDB
{
    /// <summary>
    /// MongoDB implementation of the IScrapedContentRepository interface
    /// </summary>
    public class ScrapedContentRepository : IScrapedContentRepository
    {
        private readonly IMongoCollection<ScrapedContentEntity> _contents;
        private readonly IMongoCollection<ContentVersionEntity> _versions;

        /// <summary>
        /// Initializes a new instance of the ScrapedContentRepository class
        /// </summary>
        /// <param name="mongoClient">The MongoDB client</param>
        /// <param name="databaseName">The database name</param>
        public ScrapedContentRepository(IMongoClient mongoClient, string databaseName = "WebScraper")
        {
            var database = mongoClient.GetDatabase(databaseName);
            _contents = database.GetCollection<ScrapedContentEntity>("scrapedContents");
            _versions = database.GetCollection<ContentVersionEntity>("contentVersions");

            // Create indexes
            CreateIndexes();
        }

        /// <inheritdoc/>
        public async Task<ScrapedContentEntity> GetByUrlAsync(string url)
        {
            return await _contents.Find(c => c.Url == url).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<ScrapedContentEntity> GetByIdAsync(string id)
        {
            return await _contents.Find(c => c.Id == id).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ScrapedContentEntity> Items, int TotalCount)> GetByScraperIdAsync(
            Guid scraperId, int page = 1, int pageSize = 20)
        {
            var filter = Builders<ScrapedContentEntity>.Filter.Eq(c => c.ScraperId, scraperId.ToString());
            var totalCount = await _contents.CountDocumentsAsync(filter);

            var items = await _contents.Find(filter)
                .Sort(Builders<ScrapedContentEntity>.Sort.Descending(c => c.ScrapedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        /// <inheritdoc/>
        public async Task<ScrapedContentEntity> SaveContentAsync(ScrapedContentEntity content)
        {
            var filter = Builders<ScrapedContentEntity>.Filter.Eq(c => c.Url, content.Url);
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
        public async Task<ContentVersionEntity> AddVersionAsync(ContentVersionEntity version)
        {
            if (string.IsNullOrEmpty(version.Id))
            {
                version.Id = ObjectId.GenerateNewId().ToString();
            }

            await _versions.InsertOneAsync(version);
            return version;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ContentVersionEntity>> GetVersionsAsync(string contentId, int limit = 5)
        {
            return await _versions
                .Find(v => v.ContentId == contentId)
                .Sort(Builders<ContentVersionEntity>.Sort.Descending(v => v.VersionDate))
                .Limit(limit)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<(IEnumerable<ScrapedContentEntity> Items, int TotalCount)> SearchAsync(
            string query, Guid? scraperId = null, int page = 1, int pageSize = 20)
        {
            var builder = Builders<ScrapedContentEntity>.Filter;
            var filter = builder.Text(query);

            if (scraperId.HasValue)
            {
                filter = builder.And(filter, builder.Eq(c => c.ScraperId, scraperId.Value.ToString()));
            }

            var totalCount = await _contents.CountDocumentsAsync(filter);

            var items = await _contents.Find(filter)
                .Sort(Builders<ScrapedContentEntity>.Sort.Descending(c => c.ScrapedAt))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (items, (int)totalCount);
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ScrapedContentEntity>> GetChangedContentSinceAsync(
            DateTime since, Guid? scraperId = null)
        {
            var builder = Builders<ScrapedContentEntity>.Filter;
            var filter = builder.Gte(c => c.ScrapedAt, since);

            if (scraperId.HasValue)
            {
                filter = builder.And(filter, builder.Eq(c => c.ScraperId, scraperId.Value.ToString()));
            }

            return await _contents.Find(filter)
                .Sort(Builders<ScrapedContentEntity>.Sort.Descending(c => c.ScrapedAt))
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
            var contentFilter = Builders<ScrapedContentEntity>.Filter.Eq(c => c.ScraperId, scraperId.ToString());
            var contentResult = await _contents.DeleteManyAsync(contentFilter);

            var versionFilter = Builders<ContentVersionEntity>.Filter.Eq(v => v.ScraperId, scraperId.ToString());
            await _versions.DeleteManyAsync(versionFilter);

            return (int)contentResult.DeletedCount;
        }

        private void CreateIndexes()
        {
            // Create indexes for ScrapedContent collection
            var contentIndexes = _contents.Indexes;

            // URL index (unique)
            var urlIndexModel = new CreateIndexModel<ScrapedContentEntity>(
                Builders<ScrapedContentEntity>.IndexKeys.Ascending(c => c.Url),
                new CreateIndexOptions { Unique = true });

            // ScraperId index
            var scraperIdIndexModel = new CreateIndexModel<ScrapedContentEntity>(
                Builders<ScrapedContentEntity>.IndexKeys.Ascending(c => c.ScraperId));

            // Text index for search
            var textIndexModel = new CreateIndexModel<ScrapedContentEntity>(
                Builders<ScrapedContentEntity>.IndexKeys.Text(c => c.TextContent)
                    .Text(c => c.Title)
                    .Text(c => c.Url));

            // Date index
            var dateIndexModel = new CreateIndexModel<ScrapedContentEntity>(
                Builders<ScrapedContentEntity>.IndexKeys.Descending(c => c.ScrapedAt));

            contentIndexes.CreateMany(new[] { urlIndexModel, scraperIdIndexModel, textIndexModel, dateIndexModel });

            // Create indexes for ContentVersion collection
            var versionIndexes = _versions.Indexes;

            // ContentId index
            var contentIdIndexModel = new CreateIndexModel<ContentVersionEntity>(
                Builders<ContentVersionEntity>.IndexKeys.Ascending(v => v.ContentId));

            // ScraperId index
            var versionScraperIdIndexModel = new CreateIndexModel<ContentVersionEntity>(
                Builders<ContentVersionEntity>.IndexKeys.Ascending(v => v.ScraperId));

            // Date index
            var versionDateIndexModel = new CreateIndexModel<ContentVersionEntity>(
                Builders<ContentVersionEntity>.IndexKeys.Descending(v => v.VersionDate));

            versionIndexes.CreateMany(new[] { contentIdIndexModel, versionScraperIdIndexModel, versionDateIndexModel });
        }
    }
}
