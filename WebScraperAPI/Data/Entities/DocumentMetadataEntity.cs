using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class DocumentMetadataEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string DocumentId { get; set; }
        
        [Required]
        public string MetaKey { get; set; }
        
        public string MetaValue { get; set; }

        // Navigation property
        public virtual ProcessedDocumentEntity ProcessedDocument { get; set; }
    }
}
