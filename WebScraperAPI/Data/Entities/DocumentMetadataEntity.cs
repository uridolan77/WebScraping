using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class DocumentMetadataEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string? DocumentId { get; set; }

        [Required]
        public string? MetadataKey { get; set; }

        public string? MetadataValue { get; set; }

        // Navigation property
        public virtual ProcessedDocumentEntity? ProcessedDocument { get; set; }
    }
}
