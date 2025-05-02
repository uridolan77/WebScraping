using Microsoft.EntityFrameworkCore;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data
{
    public class WebScraperDbContext : DbContext
    {
        public WebScraperDbContext(DbContextOptions<WebScraperDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for each entity
        public DbSet<ScraperConfigEntity> ScraperConfigs { get; set; }
        public DbSet<ScraperStartUrlEntity> ScraperStartUrls { get; set; }
        public DbSet<ContentExtractorSelectorEntity> ContentExtractorSelectors { get; set; }
        public DbSet<KeywordAlertEntity> KeywordAlerts { get; set; }
        public DbSet<WebhookTriggerEntity> WebhookTriggers { get; set; }
        public DbSet<DomainRateLimitEntity> DomainRateLimits { get; set; }
        public DbSet<ProxyConfigurationEntity> ProxyConfigurations { get; set; }
        public DbSet<ScraperScheduleEntity> ScraperSchedules { get; set; }
        public DbSet<ScraperRunEntity> ScraperRuns { get; set; }
        public DbSet<ScraperStatusEntity> ScraperStatuses { get; set; }
        public DbSet<PipelineMetricEntity> PipelineMetrics { get; set; }
        public DbSet<LogEntryEntity> LogEntries { get; set; }
        public DbSet<ContentChangeRecordEntity> ContentChangeRecords { get; set; }
        public DbSet<ProcessedDocumentEntity> ProcessedDocuments { get; set; }
        public DbSet<DocumentMetadataEntity> DocumentMetadata { get; set; }
        public DbSet<ScraperMetricEntity> ScraperMetrics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names (to match the SQL script)
            modelBuilder.Entity<ScraperConfigEntity>().ToTable("scraper_config");
            modelBuilder.Entity<ScraperStartUrlEntity>().ToTable("scraper_start_url");
            modelBuilder.Entity<ContentExtractorSelectorEntity>().ToTable("content_extractor_selector");
            modelBuilder.Entity<KeywordAlertEntity>().ToTable("keyword_alert");
            modelBuilder.Entity<WebhookTriggerEntity>().ToTable("webhook_trigger");
            modelBuilder.Entity<DomainRateLimitEntity>().ToTable("domain_rate_limit");
            modelBuilder.Entity<ProxyConfigurationEntity>().ToTable("proxy_configuration");
            modelBuilder.Entity<ScraperScheduleEntity>().ToTable("scraper_schedule");
            modelBuilder.Entity<ScraperRunEntity>().ToTable("scraper_run");
            modelBuilder.Entity<ScraperStatusEntity>().ToTable("scraper_status");
            modelBuilder.Entity<PipelineMetricEntity>().ToTable("pipeline_metric");
            modelBuilder.Entity<LogEntryEntity>().ToTable("log_entry");
            modelBuilder.Entity<ContentChangeRecordEntity>().ToTable("content_change_record");
            modelBuilder.Entity<ProcessedDocumentEntity>().ToTable("processed_document");
            modelBuilder.Entity<DocumentMetadataEntity>().ToTable("document_metadata");
            modelBuilder.Entity<ScraperMetricEntity>().ToTable("scraper_metrics");

            // Configure case-sensitive column names for MySQL
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                // Replace camel case property names with snake_case column names
                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(SnakeCase(property.Name));
                }

                // Replace camel case navigation/foreign key names with snake_case
                foreach (var key in entity.GetForeignKeys())
                {
                    foreach (var column in key.Properties)
                    {
                        column.SetColumnName(SnakeCase(column.Name));
                    }
                }
            }

            // Configure relationships
            modelBuilder.Entity<ScraperStartUrlEntity>()
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.StartUrls)
                .HasForeignKey(s => s.ScraperId);

            modelBuilder.Entity<ContentExtractorSelectorEntity>()
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ContentExtractorSelectors)
                .HasForeignKey(s => s.ScraperId);

            modelBuilder.Entity<KeywordAlertEntity>()
                .HasOne(k => k.ScraperConfig)
                .WithMany(c => c.KeywordAlerts)
                .HasForeignKey(k => k.ScraperId);

            modelBuilder.Entity<WebhookTriggerEntity>()
                .HasOne(w => w.ScraperConfig)
                .WithMany(c => c.WebhookTriggers)
                .HasForeignKey(w => w.ScraperId);

            modelBuilder.Entity<DomainRateLimitEntity>()
                .HasOne(d => d.ScraperConfig)
                .WithMany(c => c.DomainRateLimits)
                .HasForeignKey(d => d.ScraperId);

            modelBuilder.Entity<ProxyConfigurationEntity>()
                .HasOne(p => p.ScraperConfig)
                .WithMany(c => c.ProxyConfigurations)
                .HasForeignKey(p => p.ScraperId);

            modelBuilder.Entity<ScraperScheduleEntity>()
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ScraperId);

            modelBuilder.Entity<ScraperRunEntity>()
                .HasOne(r => r.ScraperConfig)
                .WithMany(c => c.Runs)
                .HasForeignKey(r => r.ScraperId);

            modelBuilder.Entity<ScraperStatusEntity>()
                .HasOne(s => s.ScraperConfig)
                .WithOne(c => c.Status)
                .HasForeignKey<ScraperStatusEntity>(s => s.ScraperId);

            modelBuilder.Entity<PipelineMetricEntity>()
                .HasOne(p => p.ScraperConfig)
                .WithMany(c => c.PipelineMetrics)
                .HasForeignKey(p => p.ScraperId);

            modelBuilder.Entity<LogEntryEntity>()
                .HasOne(l => l.ScraperConfig)
                .WithMany(c => c.LogEntries)
                .HasForeignKey(l => l.ScraperId);

            modelBuilder.Entity<LogEntryEntity>()
                .HasOne(l => l.Run)
                .WithMany(r => r.LogEntries)
                .HasForeignKey(l => l.RunId)
                .IsRequired(false);

            modelBuilder.Entity<ContentChangeRecordEntity>()
                .HasOne(c => c.ScraperConfig)
                .WithMany(c => c.ContentChangeRecords)
                .HasForeignKey(c => c.ScraperId);

            modelBuilder.Entity<ContentChangeRecordEntity>()
                .HasOne(c => c.Run)
                .WithMany(r => r.ContentChangeRecords)
                .HasForeignKey(c => c.RunId)
                .IsRequired(false);

            modelBuilder.Entity<ProcessedDocumentEntity>()
                .HasOne(p => p.ScraperConfig)
                .WithMany(c => c.ProcessedDocuments)
                .HasForeignKey(p => p.ScraperId);

            modelBuilder.Entity<ProcessedDocumentEntity>()
                .HasOne(p => p.Run)
                .WithMany(r => r.ProcessedDocuments)
                .HasForeignKey(p => p.RunId)
                .IsRequired(false);

            modelBuilder.Entity<DocumentMetadataEntity>()
                .HasOne(d => d.ProcessedDocument)
                .WithMany(p => p.Metadata)
                .HasForeignKey(d => d.DocumentId);

            modelBuilder.Entity<ScraperMetricEntity>()
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Metrics)
                .HasForeignKey(s => s.ScraperId);
        }

        // Helper method to convert CamelCase to snake_case for MySQL compatibility
        private string SnakeCase(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input.ToString();
            
            // Handle common ID abbreviation
            result = System.Text.RegularExpressions.Regex.Replace(result, "ID$", "_id");
            
            // Convert other camel case
            result = System.Text.RegularExpressions.Regex.Replace(
                result, 
                "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])", 
                "_$1", 
                System.Text.RegularExpressions.RegexOptions.Compiled)
                .ToLower();
            
            return result;
        }
    }
}
