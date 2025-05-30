using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebScraperApi.Data.Entities;
using System;

// Comment out these entities until they are properly implemented
// using WebScraperApi.Data.Entities.ContentClassification;
// using WebScraperApi.Data.Entities.ContentEntity;

namespace WebScraperApi.Data
{
    public class WebScraperDbContext : DbContext
    {
        public WebScraperDbContext(DbContextOptions<WebScraperDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for each entity - using singular names to match database table names
        public DbSet<ScraperConfigEntity> ScraperConfigs { get; set; }
        public DbSet<ScraperStartUrlEntity> ScraperStartUrls { get; set; }
        public DbSet<ContentExtractorSelectorEntity> ContentExtractorSelector { get; set; }
        public DbSet<KeywordAlertEntity> KeywordAlert { get; set; }
        public DbSet<WebhookTriggerEntity> WebhookTrigger { get; set; }
        public DbSet<DomainRateLimitEntity> DomainRateLimit { get; set; }
        public DbSet<ProxyConfigurationEntity> ProxyConfiguration { get; set; }
        public DbSet<ScraperScheduleEntity> ScraperSchedule { get; set; }
        public DbSet<ScraperRunEntity> ScraperRun { get; set; }
        public DbSet<ScraperStatusEntity> ScraperStatuses { get; set; }
        public DbSet<PipelineMetricEntity> PipelineMetric { get; set; }
        public DbSet<LogEntryEntity> LogEntry { get; set; }
        public DbSet<ContentChangeRecordEntity> ContentChangeRecord { get; set; }
        public DbSet<ProcessedDocumentEntity> ProcessedDocument { get; set; }
        public DbSet<DocumentMetadataEntity> DocumentMetadata { get; set; }
        public DbSet<ScraperMetricEntity> ScraperMetric { get; set; }
        public DbSet<CustomMetricEntity> CustomMetric { get; set; }
        public DbSet<ScraperLogEntity> ScraperLog { get; set; }
        public DbSet<ScrapedPageEntity> ScrapedPage { get; set; }
        // Commented out until properly implemented
        // public DbSet<ContentClassificationEntity> ContentClassifications { get; set; }
        // public DbSet<ContentEntityEntity> ContentEntities { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure table names to use singular camelCase format (without underscores)

            // Configure ScraperConfigEntity
            modelBuilder.Entity<ScraperConfigEntity>(entity =>
            {
                entity.ToTable("scraperconfig");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.CreatedAt).HasColumnName("createdat");
                entity.Property(e => e.LastModified).HasColumnName("lastmodified");
                entity.Property(e => e.LastRun).HasColumnName("lastrun");
                entity.Property(e => e.RunCount).HasColumnName("runcount");
                entity.Property(e => e.StartUrl).HasColumnName("starturl");
                entity.Property(e => e.BaseUrl).HasColumnName("baseurl");
                entity.Property(e => e.OutputDirectory).HasColumnName("outputdirectory");
                entity.Property(e => e.DelayBetweenRequests).HasColumnName("delaybetweenrequests");
                entity.Property(e => e.MaxConcurrentRequests).HasColumnName("maxconcurrentrequests");
                entity.Property(e => e.MaxDepth).HasColumnName("maxdepth");
                entity.Property(e => e.MaxPages).HasColumnName("maxpages");
                entity.Property(e => e.FollowLinks).HasColumnName("followlinks");
                entity.Property(e => e.FollowExternalLinks).HasColumnName("followexternallinks");
                entity.Property(e => e.RespectRobotsTxt).HasColumnName("respectrobotstxt");

                // Ignore navigation properties in queries
                entity.Ignore(e => e.Status);
                entity.Ignore(e => e.StartUrls);
                entity.Ignore(e => e.ContentExtractorSelectors);
                entity.Ignore(e => e.KeywordAlerts);
                entity.Ignore(e => e.WebhookTriggers);
                entity.Ignore(e => e.DomainRateLimits);
                entity.Ignore(e => e.ProxyConfigurations);
                entity.Ignore(e => e.Schedules);
                entity.Ignore(e => e.Runs);
                entity.Ignore(e => e.PipelineMetrics);
                entity.Ignore(e => e.LogEntries);
                entity.Ignore(e => e.ContentChangeRecords);
                entity.Ignore(e => e.ProcessedDocuments);
                entity.Ignore(e => e.Metrics);
                entity.Ignore(e => e.Logs);
                entity.Ignore(e => e.ScrapedPages);
            });

            // Configure ScraperStartUrlEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperStartUrlEntity>(entity =>
            {
                entity.ToTable("scraperstarturlentity");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Url).HasColumnName("url");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.StartUrls)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ContentExtractorSelectorEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentExtractorSelectorEntity>(entity =>
            {
                entity.ToTable("contentextractorselector");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Selector).HasColumnName("selector");
                entity.Property(e => e.IsExclude).HasColumnName("isexclude");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.ContentExtractorSelectors)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure KeywordAlertEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.KeywordAlertEntity>(entity =>
            {
                entity.ToTable("keywordalert");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Keyword).HasColumnName("keyword");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.KeywordAlerts)
                    .HasForeignKey(k => k.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure WebhookTriggerEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.WebhookTriggerEntity>(entity =>
            {
                entity.ToTable("webhooktrigger");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.TriggerName).HasColumnName("triggername");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.WebhookTriggers)
                    .HasForeignKey(w => w.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure DomainRateLimitEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DomainRateLimitEntity>(entity =>
            {
                entity.ToTable("domainratelimit");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Domain).HasColumnName("domain");
                entity.Property(e => e.RequestsPerMinute).HasColumnName("requestsperminute");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.DomainRateLimits)
                    .HasForeignKey(d => d.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ProxyConfigurationEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProxyConfigurationEntity>(entity =>
            {
                entity.ToTable("proxyconfiguration");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.ProxyUrl).HasColumnName("proxyurl");
                entity.Property(e => e.Username).HasColumnName("username");
                entity.Property(e => e.Password).HasColumnName("password");
                entity.Property(e => e.IsActive).HasColumnName("isactive");
                entity.Property(e => e.FailureCount).HasColumnName("failurecount");
                entity.Property(e => e.LastUsed).HasColumnName("lastused");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.ProxyConfigurations)
                    .HasForeignKey(p => p.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ScraperScheduleEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperScheduleEntity>(entity =>
            {
                entity.ToTable("scraperschedule");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Name).HasColumnName("name");
                entity.Property(e => e.CronExpression).HasColumnName("cronexpression");
                entity.Property(e => e.IsActive).HasColumnName("isactive");
                entity.Property(e => e.LastRun).HasColumnName("lastrun");
                entity.Property(e => e.NextRun).HasColumnName("nextrun");
                entity.Property(e => e.MaxRuntimeMinutes).HasColumnName("maxruntimeminutes");
                entity.Property(e => e.NotificationEmail).HasColumnName("notificationemail");

                // Explicitly tell EF not to look for ScraperConfigId column
                entity.Ignore(e => e.ScraperConfig);

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany(c => c.Schedules)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ScraperRunEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ScraperRunEntity>(entity =>
            {
                entity.ToTable("scraperrun");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.StartTime).HasColumnName("starttime");
                entity.Property(e => e.EndTime).HasColumnName("endtime");
                entity.Property(e => e.UrlsProcessed).HasColumnName("urlsprocessed");
                entity.Property(e => e.DocumentsProcessed).HasColumnName("documentsprocessed");
                entity.Property(e => e.Successful).HasColumnName("successful");
                entity.Property(e => e.ErrorMessage).HasColumnName("errormessage");
                entity.Property(e => e.ElapsedTime).HasColumnName("elapsedtime");

                // Ignore ScraperConfig property in queries
                entity.Ignore(e => e.ScraperConfig);
            });

            // Configure ScraperStatusEntity
            modelBuilder.Entity<ScraperStatusEntity>(entity =>
            {
                entity.ToTable("scraperstatus");
                entity.HasKey(e => e.ScraperId);

                // Configure columns explicitly to match the actual database column names (camelCase)
                entity.Property(e => e.ScraperId).HasColumnName("scraperId");
                entity.Property(e => e.IsRunning).HasColumnName("isRunning");
                entity.Property(e => e.StartTime).HasColumnName("startTime");
                entity.Property(e => e.EndTime).HasColumnName("endTime");
                entity.Property(e => e.ElapsedTime).HasColumnName("elapsedTime");
                entity.Property(e => e.UrlsProcessed).HasColumnName("urlsProcessed");
                entity.Property(e => e.UrlsQueued).HasColumnName("urlsQueued");
                entity.Property(e => e.DocumentsProcessed).HasColumnName("documentsProcessed");
                entity.Property(e => e.HasErrors).HasColumnName("hasErrors");
                entity.Property(e => e.Message).HasColumnName("message");
                entity.Property(e => e.LastStatusUpdate).HasColumnName("lastStatusUpdate");
                entity.Property(e => e.LastUpdate).HasColumnName("lastUpdate");
                entity.Property(e => e.LastMonitorCheck).HasColumnName("lastMonitorCheck");
                entity.Property(e => e.LastError).HasColumnName("lastError");

                // Ignore ScraperConfig property in queries
                entity.Ignore(e => e.ScraperConfig);
            });

            // Configure PipelineMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.PipelineMetricEntity>(entity =>
            {
                entity.ToTable("pipelinemetric");

                // Configure columns explicitly
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.ProcessingItems).HasColumnName("processingitems");
                entity.Property(e => e.QueuedItems).HasColumnName("queueditems");
                entity.Property(e => e.CompletedItems).HasColumnName("completeditems");
                entity.Property(e => e.FailedItems).HasColumnName("faileditems");
                entity.Property(e => e.AverageProcessingTimeMs).HasColumnName("averageprocessingtimems");
                entity.Property(e => e.RunId).HasColumnName("runid");

                // Ignore navigation properties in queries
                entity.Ignore(e => e.ScraperConfig);
                entity.Ignore(e => e.Run);
            });

            // Configure LogEntryEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.LogEntryEntity>()
                .ToTable("logentry")
                .HasOne(s => s.ScraperConfig)
                .WithMany(c => c.LogEntries)
                .HasForeignKey(s => s.ScraperId)
                .HasPrincipalKey(c => c.Id);

            // Configure ContentChangeRecordEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ContentChangeRecordEntity>(entity =>
            {
                entity.ToTable("contentchangerecord");

                // Configure columns explicitly to match the actual database schema
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.DetectedAt).HasColumnName("detectedat");
                entity.Property(e => e.ChangeType).HasColumnName("changetype");
                entity.Property(e => e.SignificanceScore).HasColumnName("significanceScore");
                entity.Property(e => e.ChangeSummary).HasColumnName("changeSummary");
                entity.Property(e => e.PreviousVersionHash).HasColumnName("previousVersionHash");
                entity.Property(e => e.CurrentVersionHash).HasColumnName("currentVersionHash");
                entity.Property(e => e.RunId).HasColumnName("runid");
                entity.Property(e => e.ScraperConfigId).HasColumnName("scraperConfigId");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne(s => s.ScraperConfig)
                    .WithMany(c => c.ContentChangeRecords)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .IsRequired(false);

                // Configure the relationship with ScraperRunEntity
                entity.HasOne(s => s.Run)
                    .WithMany(r => r.ContentChangeRecords)
                    .HasForeignKey(s => s.RunId)
                    .HasPrincipalKey(r => r.Id)
                    .IsRequired(false);
            });

            // Configure ProcessedDocumentEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.ProcessedDocumentEntity>(entity =>
            {
                entity.ToTable("processeddocument");

                // Configure columns explicitly to match the actual database schema
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperid");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.Title).HasColumnName("title");
                entity.Property(e => e.DocumentType).HasColumnName("documentType");
                entity.Property(e => e.ProcessedAt).HasColumnName("processedAt");
                entity.Property(e => e.ContentSizeBytes).HasColumnName("contentSizeBytes");
                entity.Property(e => e.RunId).HasColumnName("runid");
                entity.Property(e => e.ScraperConfigId).HasColumnName("scraperConfigId");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne(s => s.ScraperConfig)
                    .WithMany(c => c.ProcessedDocuments)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id)
                    .IsRequired(false);

                // Configure the relationship with ScraperRunEntity
                entity.HasOne(s => s.Run)
                    .WithMany(r => r.ProcessedDocuments)
                    .HasForeignKey(s => s.RunId)
                    .HasPrincipalKey(r => r.Id)
                    .IsRequired(false);
            });

            // Configure DocumentMetadataEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.DocumentMetadataEntity>(entity =>
            {
                entity.ToTable("documentmetadata");

                // Configure columns explicitly to match the actual database schema
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DocumentId).HasColumnName("documentId");
                entity.Property(e => e.MetadataKey).HasColumnName("metaKey");
                entity.Property(e => e.MetadataValue).HasColumnName("metaValue");

                // Configure the relationship with ProcessedDocumentEntity
                entity.HasOne(m => m.ProcessedDocument)
                    .WithMany(d => d.Metadata)
                    .HasForeignKey(m => m.DocumentId)
                    .HasPrincipalKey(d => d.Id)
                    .IsRequired(false);
            });

            // Configure ScraperMetricEntity
            modelBuilder.Entity<ScraperMetricEntity>(entity =>
            {
                entity.ToTable("scrapermetric");

                // Configure columns explicitly to match the actual database column names
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperId");
                entity.Property(e => e.ScraperConfigId).HasColumnName("scraperConfigId");
                entity.Property(e => e.ScraperName).HasColumnName("scraperName");
                entity.Property(e => e.MetricName).HasColumnName("metricName");
                entity.Property(e => e.MetricValue).HasColumnName("metricValue");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.RunId).HasColumnName("runId");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne<ScraperConfigEntity>()
                    .WithMany()
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id);
            });

            // Configure CustomMetricEntity
            modelBuilder.Entity<WebScraperApi.Data.Entities.CustomMetricEntity>()
                .ToTable("custommetric");

            // Configure ScraperLogEntity
            modelBuilder.Entity<ScraperLogEntity>(entity =>
            {
                entity.ToTable("scraperlog");

                // Configure columns explicitly to match the actual database column names (camelCase)
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperId");
                entity.Property(e => e.Timestamp).HasColumnName("timestamp");
                entity.Property(e => e.LogLevel).HasColumnName("logLevel");
                entity.Property(e => e.Message).HasColumnName("message");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne(s => s.ScraperConfig)
                    .WithMany(c => c.Logs)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id);
            });

            // Configure ScrapedPageEntity
            modelBuilder.Entity<ScrapedPageEntity>(entity =>
            {
                entity.ToTable("scrapedpage");

                // Configure columns explicitly to match the actual database column names (camelCase)
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperId");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.HtmlContent).HasColumnName("htmlContent");
                entity.Property(e => e.TextContent).HasColumnName("textContent");
                entity.Property(e => e.ScrapedAt).HasColumnName("scrapedAt");
                entity.Property(e => e.ScraperConfigId).HasColumnName("scraperConfigId");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne(s => s.ScraperConfig)
                    .WithMany(c => c.ScrapedPages)
                    .HasForeignKey(s => s.ScraperId)
                    .HasPrincipalKey(c => c.Id);
            });

            // Commented out until properly implemented
            /*
            // Configure ContentClassificationEntity
            modelBuilder.Entity<ContentClassificationEntity>(entity =>
            {
                entity.ToTable("contentclassification");

                // Configure columns explicitly to match the actual database column names (camelCase)
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ScraperId).HasColumnName("scraperId");
                entity.Property(e => e.Url).HasColumnName("url");
                entity.Property(e => e.ContentLength).HasColumnName("contentLength");
                entity.Property(e => e.SentenceCount).HasColumnName("sentenceCount");
                entity.Property(e => e.ParagraphCount).HasColumnName("paragraphCount");
                entity.Property(e => e.ReadabilityScore).HasColumnName("readabilityScore");
                entity.Property(e => e.PositiveScore).HasColumnName("positiveScore");
                entity.Property(e => e.NegativeScore).HasColumnName("negativeScore");
                entity.Property(e => e.OverallSentiment).HasColumnName("overallSentiment");
                entity.Property(e => e.DocumentType).HasColumnName("documentType");
                entity.Property(e => e.Confidence).HasColumnName("confidence");
                entity.Property(e => e.ClassifiedAt).HasColumnName("classifiedAt");

                // Configure the relationship with ScraperConfigEntity
                entity.HasOne(e => e.ScraperConfig)
                    .WithMany()
                    .HasForeignKey(e => e.ScraperId)
                    .HasPrincipalKey(e => e.Id);

                // Configure the relationship with ContentEntityEntity
                entity.HasMany(e => e.Entities)
                    .WithOne(e => e.Classification)
                    .HasForeignKey(e => e.ClassificationId)
                    .HasPrincipalKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure ContentEntityEntity
            modelBuilder.Entity<ContentEntityEntity>(entity =>
            {
                entity.ToTable("contententity");

                // Configure columns explicitly to match the actual database column names (camelCase)
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ClassificationId).HasColumnName("classificationId");
                entity.Property(e => e.Type).HasColumnName("type");
                entity.Property(e => e.Value).HasColumnName("value");
                entity.Property(e => e.Position).HasColumnName("position");
                entity.Property(e => e.Confidence).HasColumnName("confidence");

                // Configure the relationship with ContentClassificationEntity
                entity.HasOne(e => e.Classification)
                    .WithMany(e => e.Entities)
                    .HasForeignKey(e => e.ClassificationId)
                    .HasPrincipalKey(e => e.Id)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            */

            // Configure one-to-one relationship between ScraperConfigEntity and ScraperStatusEntity
            modelBuilder.Entity<ScraperConfigEntity>()
                .Ignore(s => s.Status);
        }
    }
}
