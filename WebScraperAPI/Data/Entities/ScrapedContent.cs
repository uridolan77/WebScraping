using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScraperApi.Data.Entities
{
    public class ScrapedContent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("scraperId")]
        public Guid ScraperConfigId { get; set; }
        
        [BsonElement("url")]
        public string Url { get; set; }
        
        [BsonElement("scrapedAt")]
        public DateTime ScrapedAt { get; set; }
        
        [BsonElement("rawContent")]
        public string RawContent { get; set; }
        
        [BsonElement("processedContent")]
        public string ProcessedContent { get; set; }
        
        [BsonElement("metadata")]
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
        
        [BsonElement("depth")]
        public int Depth { get; set; }
        
        [BsonElement("contentHash")]
        public string ContentHash { get; set; }
        
        [BsonElement("contentType")]
        public string ContentType { get; set; } = "text/html";
        
        [BsonElement("title")]
        public string Title { get; set; }
        
        [BsonElement("keywords")]
        public List<string> Keywords { get; set; } = new List<string>();
        
        [BsonElement("lastModified")]
        public DateTime? LastModified { get; set; }
    }
}
