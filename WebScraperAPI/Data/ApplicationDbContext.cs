using Microsoft.EntityFrameworkCore;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Data
{
    /// <summary>
    /// Entity Framework Core database context for the WebScraper API
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the ApplicationDbContext class
        /// </summary>
        /// <param name="options">The options for this context</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets or sets the scraper configurations
        /// </summary>
        public DbSet<ScraperConfigEntity> ScraperConfigs { get; set; }

        /// <summary>
        /// Gets or sets the scraper runs
        /// </summary>
        public DbSet<ScraperRunEntity> ScraperRuns { get; set; }

        /// <summary>
        /// Gets or sets the scraper logs
        /// </summary>
        public DbSet<LogEntryEntity> ScraperLogs { get; set; }

        /// <summary>
        /// Gets or sets the scraper start URLs
        /// </summary>
        public DbSet<ScraperStartUrlEntity> ScraperStartUrls { get; set; }

        /// <summary>
        /// Gets or sets the content extractor selectors
        /// </summary>
        public DbSet<ContentExtractorSelectorEntity> ContentExtractorSelectors { get; set; }

        /// <summary>
        /// Gets or sets the keyword alerts
        /// </summary>
        public DbSet<KeywordAlertEntity> KeywordAlerts { get; set; }

        /// <summary>
        /// Gets or sets the domain rate limits
        /// </summary>
        public DbSet<DomainRateLimitEntity> DomainRateLimits { get; set; }

        /// <summary>
        /// Gets or sets the proxy configurations
        /// </summary>
        public DbSet<ProxyConfigurationEntity> ProxyConfigurations { get; set; }

        /// <summary>
        /// Gets or sets the webhook triggers
        /// </summary>
        public DbSet<WebhookTriggerEntity> WebhookTriggers { get; set; }

        /// <summary>
        /// Gets or sets the scraper schedules
        /// </summary>
        public DbSet<ScraperScheduleEntity> ScraperSchedules { get; set; }

        /// <summary>
        /// Configures the model that was discovered by convention from the entity types
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for this context</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure entity relationships and constraints
            modelBuilder.Entity<ScraperConfigEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.StartUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.BaseUrl).IsRequired().HasMaxLength(500);
                entity.Property(e => e.OutputDirectory).HasMaxLength(255);
                entity.Property(e => e.NotificationEmail).HasMaxLength(255);

                // Configure relationships
                entity.HasMany(e => e.Runs)
                    .WithOne(e => e.ScraperConfig)
                    .HasForeignKey(e => e.ScraperId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.LogEntries)
                    .WithOne(e => e.ScraperConfig)
                    .HasForeignKey(e => e.ScraperId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ScraperRunEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ElapsedTime).HasMaxLength(50);
            });

            modelBuilder.Entity<LogEntryEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired();
            });
            
            // Configure additional entities
            modelBuilder.Entity<ScraperStartUrlEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Url).IsRequired().HasMaxLength(500);
            });
            
            modelBuilder.Entity<ContentExtractorSelectorEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Selector).IsRequired().HasMaxLength(500);
            });
            
            modelBuilder.Entity<KeywordAlertEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Keyword).IsRequired().HasMaxLength(100);
            });
            
            modelBuilder.Entity<DomainRateLimitEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Domain).IsRequired().HasMaxLength(255);
            });
            
            modelBuilder.Entity<ProxyConfigurationEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).HasMaxLength(100);
                entity.Property(e => e.Password).HasMaxLength(100);
            });
            
            modelBuilder.Entity<WebhookTriggerEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TriggerName).IsRequired().HasMaxLength(100);
            });
            
            modelBuilder.Entity<ScraperScheduleEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CronExpression).IsRequired().HasMaxLength(100);
                entity.Property(e => e.NotificationEmail).HasMaxLength(255);
            });
        }
    }
}
