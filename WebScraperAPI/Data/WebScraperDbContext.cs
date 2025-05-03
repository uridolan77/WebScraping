using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebScraperApi.Data.Entities;
using System.Collections.Generic;
using System.Linq;
using System;

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

            // Configure ScraperStartUrlEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperStartUrlEntity>()
                .ToTable("scraper_start_url")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.StartUrls)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ContentExtractorSelectorEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentExtractorSelectorEntity>()
                .ToTable("content_extractor_selector")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ContentExtractorSelectors)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure KeywordAlertEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.KeywordAlertEntity>()
                .ToTable("keyword_alert")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.KeywordAlerts)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure WebhookTriggerEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.WebhookTriggerEntity>()
                .ToTable("webhook_trigger")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.WebhookTriggers)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure DomainRateLimitEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DomainRateLimitEntity>()
                .ToTable("domain_rate_limit")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.DomainRateLimits)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ProxyConfigurationEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProxyConfigurationEntity>()
                .ToTable("proxy_configuration")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ProxyConfigurations)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperScheduleEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperScheduleEntity>()
                .ToTable("scraper_schedule")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperRunEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperRunEntity>()
                .ToTable("scraper_run")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Runs)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperStatusEntity
            modelBuilder.Entity<ScraperStatusEntity>()
                .ToTable("scraper_status")
                .HasOne(s => s.ScraperConfig)
                .WithOne(c => c.Status)
                .HasForeignKey<ScraperStatusEntity>(s => s.ScraperId)
                .HasPrincipalKey<ScraperConfigEntity>(c => c.Id);

            // Configure PipelineMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.PipelineMetricEntity>()
                .ToTable("pipeline_metric")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.PipelineMetrics)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure LogEntryEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.LogEntryEntity>()
                .ToTable("log_entry")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.LogEntries)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ContentChangeRecordEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentChangeRecordEntity>()
                .ToTable("content_change_record")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ContentChangeRecords)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ProcessedDocumentEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProcessedDocumentEntity>()
                .ToTable("processed_document")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ProcessedDocuments)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure DocumentMetadataEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DocumentMetadataEntity>()
                .ToTable("document_metadata");

            // Configure ScraperMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperMetricEntity>()
                .ToTable("scraper_metrics")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Metrics)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Remove the duplicate configuration for ScraperStartUrlEntity since it's already configured above

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

            // Configure one-to-one relationship between ScraperConfigEntity and ScraperStatusEntity
            modelBuilder.Entity<ScraperConfigEntity>()
                .HasOne(s => s.Status)
                .WithOne(s => s.ScraperConfig)
                .HasForeignKey<ScraperStatusEntity>(s => s.ScraperId);

            // Other relationships will be configured by convention
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
