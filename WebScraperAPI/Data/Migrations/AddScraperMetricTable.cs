using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;
using System;

namespace WebScraperApi.Data.Migrations
{
    public class AddScraperMetricTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create the scrapermetric table
            migrationBuilder.CreateTable(
                name: "scrapermetric",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ScraperId = table.Column<string>(nullable: false),
                    ScraperName = table.Column<string>(nullable: false),
                    MetricName = table.Column<string>(nullable: false),
                    MetricValue = table.Column<double>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scrapermetric", x => x.Id);
                    table.ForeignKey(
                        name: "fk_scrapermetric_scraperconfig",
                        column: x => x.ScraperId,
                        principalTable: "scraperconfig",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Add index for better performance
            migrationBuilder.CreateIndex(
                name: "idx_scrapermetric_scraper_id",
                table: "scrapermetric",
                column: "scraper_id");

            // Insert the UKGC scraper if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `scraperconfig` 
                (`id`, `name`, `start_url`, `base_url`, `max_depth`, `max_pages`, `follow_links`, `follow_external_links`, `created_at`, `last_modified`)
                VALUES 
                ('70344351-c33f-4d0c-8f27-dd478ff257da', 'UKGC Scraper', 'https://www.gamblingcommission.gov.uk/', 'https://www.gamblingcommission.gov.uk/', 5, 1000, 1, 0, NOW(), NOW());
            ");

            // Add a start URL for this scraper if it doesn't exist
            migrationBuilder.Sql(@"
                INSERT IGNORE INTO `scraperstarturlentity` 
                (`scraper_id`, `url`)
                VALUES 
                ('70344351-c33f-4d0c-8f27-dd478ff257da', 'https://www.gamblingcommission.gov.uk/');
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the scrapermetric table
            migrationBuilder.DropTable(
                name: "scrapermetric");
            
            // Note: We don't delete the scraper in the Down method
            // as it might be used by other parts of the application
        }
    }
}
