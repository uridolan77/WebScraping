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
        public DbSet<ScraperLogEntity> ScraperLogs { get; set; }
        public DbSet<ScrapedPageEntity> ScrapedPages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to use singular camelCase format (without underscores)
            
            // Configure ScraperConfigEntity
            modelBuilder.Entity<ScraperConfigEntity>().ToTable("scraperconfig");

            // Configure ScraperStartUrlEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperStartUrlEntity>()
                .ToTable("scraperstarturlentity")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.StartUrls)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ContentExtractorSelectorEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentExtractorSelectorEntity>()
                .ToTable("contentextractorselector")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ContentExtractorSelectors)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure KeywordAlertEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.KeywordAlertEntity>()
                .ToTable("keywordalert")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.KeywordAlerts)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure WebhookTriggerEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.WebhookTriggerEntity>()
                .ToTable("webhooktrigger")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.WebhookTriggers)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure DomainRateLimitEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DomainRateLimitEntity>()
                .ToTable("domainratelimit")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.DomainRateLimits)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ProxyConfigurationEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProxyConfigurationEntity>()
                .ToTable("proxyconfiguration", schema: null)
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ProxyConfigurations)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperScheduleEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperScheduleEntity>()
                .ToTable("scraperschedule")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Schedules)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperRunEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperRunEntity>()
                .ToTable("scraperrun")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Runs)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperStatusEntity
            modelBuilder.Entity<ScraperStatusEntity>()
                .ToTable("scraperstatus")
                .HasOne(s => s.ScraperConfig)
                .WithOne(c => c.Status)
                .HasForeignKey<ScraperStatusEntity>(s => s.ScraperId)
                .HasPrincipalKey<ScraperConfigEntity>(c => c.Id);

            // Configure PipelineMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.PipelineMetricEntity>()
                .ToTable("pipelinemetric")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.PipelineMetrics)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure LogEntryEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.LogEntryEntity>()
                .ToTable("logentry")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.LogEntries)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ContentChangeRecordEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentChangeRecordEntity>()
                .ToTable("contentchangerecord")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ContentChangeRecords)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ProcessedDocumentEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProcessedDocumentEntity>()
                .ToTable("processeddocument")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ProcessedDocuments)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure DocumentMetadataEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DocumentMetadataEntity>()
                .ToTable("documentmetadata");

            // Configure ScraperMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperMetricEntity>()
                .ToTable("scrapermetric")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Metrics)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ScraperLogEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperLogEntity>()
                .ToTable("scraperlog")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.Logs)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);
    
            // Configure ScrapedPageEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScrapedPageEntity>()
                .ToTable("scrapedpage") 
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.ScrapedPages)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure one-to-one relationship between ScraperConfigEntity and ScraperStatusEntity
            modelBuilder.Entity<ScraperConfigEntity>()
                .HasOne(s => s.Status)
                .WithOne(s => s.ScraperConfig)
                .HasForeignKey<ScraperStatusEntity>(s => s.ScraperId);
        }
    }
}
