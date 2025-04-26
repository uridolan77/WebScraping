using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScraperAPI.Data.Migrations
{
    /// <summary>
    /// Initial migration to create the database schema
    /// </summary>
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc/>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScraperConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastRun = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    OutputDirectory = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    DelayBetweenRequests = table.Column<int>(type: "integer", nullable: false),
                    MaxConcurrentRequests = table.Column<int>(type: "integer", nullable: false),
                    MaxDepth = table.Column<int>(type: "integer", nullable: false),
                    FollowExternalLinks = table.Column<bool>(type: "boolean", nullable: false),
                    RespectRobotsTxt = table.Column<bool>(type: "boolean", nullable: false),
                    AutoLearnHeaderFooter = table.Column<bool>(type: "boolean", nullable: false),
                    LearningPagesCount = table.Column<int>(type: "integer", nullable: false),
                    EnableChangeDetection = table.Column<bool>(type: "boolean", nullable: false),
                    TrackContentVersions = table.Column<bool>(type: "boolean", nullable: false),
                    MaxVersionsToKeep = table.Column<int>(type: "integer", nullable: false),
                    EnableAdaptiveCrawling = table.Column<bool>(type: "boolean", nullable: false),
                    PriorityQueueSize = table.Column<int>(type: "integer", nullable: false),
                    AdjustDepthBasedOnQuality = table.Column<bool>(type: "boolean", nullable: false),
                    EnableAdaptiveRateLimiting = table.Column<bool>(type: "boolean", nullable: false),
                    MinDelayBetweenRequests = table.Column<int>(type: "integer", nullable: false),
                    MaxDelayBetweenRequests = table.Column<int>(type: "integer", nullable: false),
                    MonitorResponseTimes = table.Column<bool>(type: "boolean", nullable: false),
                    EnableContinuousMonitoring = table.Column<bool>(type: "boolean", nullable: false),
                    MonitoringIntervalMinutes = table.Column<int>(type: "integer", nullable: false),
                    NotifyOnChanges = table.Column<bool>(type: "boolean", nullable: false),
                    NotificationEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    TrackChangesHistory = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScraperConfigs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScraperLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScraperConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScraperLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScraperLogs_ScraperConfigs_ScraperConfigId",
                        column: x => x.ScraperConfigId,
                        principalTable: "ScraperConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScraperRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ScraperConfigId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UrlsProcessed = table.Column<int>(type: "integer", nullable: false),
                    Successful = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    ElapsedTime = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScraperRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScraperRuns_ScraperConfigs_ScraperConfigId",
                        column: x => x.ScraperConfigId,
                        principalTable: "ScraperConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScraperLogs_ScraperConfigId",
                table: "ScraperLogs",
                column: "ScraperConfigId");

            migrationBuilder.CreateIndex(
                name: "IX_ScraperRuns_ScraperConfigId",
                table: "ScraperRuns",
                column: "ScraperConfigId");
        }

        /// <inheritdoc/>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScraperLogs");

            migrationBuilder.DropTable(
                name: "ScraperRuns");

            migrationBuilder.DropTable(
                name: "ScraperConfigs");
        }
    }
}
