using System.ComponentModel.DataAnnotations;

namespace WebScraperApi.Data.Entities
{
    public class ContentExtractorSelectorEntity
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string ScraperId { get; set; }
        
        [Required]
        public string Selector { get; set; }
        
        public bool IsExclude { get; set; }

        // Navigation property
        public virtual ScraperConfigEntity ScraperConfig { get; set; }
    }
}
