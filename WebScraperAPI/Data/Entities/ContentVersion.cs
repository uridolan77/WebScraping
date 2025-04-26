using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebScraperAPI.Data.Entities
{
    public class ContentVersion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("contentId")]
        public string ContentId { get; set; }
        
        [BsonElement("versionDate")]
        public DateTime VersionDate { get; set; }
        
        [BsonElement("hash")]
        public string Hash { get; set; }
        
        [BsonElement("content")]
        public string Content { get; set; }
        
        [BsonElement("changeType")]
        public string ChangeType { get; set; }
        
        [BsonElement("changedSections")]
        public Dictionary<string, string> ChangedSections { get; set; } = new Dictionary<string, string>();
        
        [BsonElement("scraperId")]
        public Guid ScraperConfigId { get; set; }
    }
}
