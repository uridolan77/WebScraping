-- SQL Script to rename all columns from snake_case to proper camelCase format
-- This will make the database match what Entity Framework Core expects

-- Disable foreign key checks for the operations
SET FOREIGN_KEY_CHECKS = 0;

-- Convert all columns in the scraperconfig table from snake_case to proper camelCase
ALTER TABLE `scraperconfig` 
  CHANGE COLUMN `created_at` `createdAt` DATETIME NOT NULL,
  CHANGE COLUMN `last_modified` `lastModified` DATETIME NOT NULL,
  CHANGE COLUMN `last_run` `lastRun` DATETIME NULL,
  CHANGE COLUMN `run_count` `runCount` INT NOT NULL,
  CHANGE COLUMN `start_url` `startUrl` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `base_url` `baseUrl` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `output_directory` `outputDirectory` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `delay_between_requests` `delayBetweenRequests` INT NOT NULL,
  CHANGE COLUMN `max_concurrent_requests` `maxConcurrentRequests` INT NOT NULL,
  CHANGE COLUMN `max_depth` `maxDepth` INT NOT NULL,
  CHANGE COLUMN `max_pages` `maxPages` INT NOT NULL,
  CHANGE COLUMN `follow_links` `followLinks` BOOLEAN NOT NULL,
  CHANGE COLUMN `follow_external_links` `followExternalLinks` BOOLEAN NOT NULL,
  CHANGE COLUMN `respect_robots_txt` `respectRobotsTxt` BOOLEAN NOT NULL,
  CHANGE COLUMN `auto_learn_header_footer` `autoLearnHeaderFooter` BOOLEAN NOT NULL,
  CHANGE COLUMN `learning_pages_count` `learningPagesCount` INT NOT NULL,
  CHANGE COLUMN `enable_change_detection` `enableChangeDetection` BOOLEAN NOT NULL,
  CHANGE COLUMN `track_content_versions` `trackContentVersions` BOOLEAN NOT NULL,
  CHANGE COLUMN `max_versions_to_keep` `maxVersionsToKeep` INT NOT NULL,
  CHANGE COLUMN `enable_adaptive_crawling` `enableAdaptiveCrawling` BOOLEAN NOT NULL,
  CHANGE COLUMN `priority_queue_size` `priorityQueueSize` INT NOT NULL,
  CHANGE COLUMN `adjust_depth_based_on_quality` `adjustDepthBasedOnQuality` BOOLEAN NOT NULL,
  CHANGE COLUMN `enable_adaptive_rate_limiting` `enableAdaptiveRateLimiting` BOOLEAN NOT NULL,
  CHANGE COLUMN `min_delay_between_requests` `minDelayBetweenRequests` INT NOT NULL,
  CHANGE COLUMN `max_delay_between_requests` `maxDelayBetweenRequests` INT NOT NULL,
  CHANGE COLUMN `monitor_response_times` `monitorResponseTimes` BOOLEAN NOT NULL,
  CHANGE COLUMN `max_requests_per_minute` `maxRequestsPerMinute` INT NOT NULL,
  CHANGE COLUMN `user_agent` `userAgent` VARCHAR(255) NULL,
  CHANGE COLUMN `back_off_on_errors` `backOffOnErrors` BOOLEAN NOT NULL,
  CHANGE COLUMN `use_proxies` `useProxies` BOOLEAN NOT NULL,
  CHANGE COLUMN `proxy_rotation_strategy` `proxyRotationStrategy` VARCHAR(50) NULL,
  CHANGE COLUMN `test_proxies_before_use` `testProxiesBeforeUse` BOOLEAN NOT NULL,
  CHANGE COLUMN `max_proxy_failures_before_removal` `maxProxyFailuresBeforeRemoval` INT NOT NULL,
  CHANGE COLUMN `enable_continuous_monitoring` `enableContinuousMonitoring` BOOLEAN NOT NULL,
  CHANGE COLUMN `monitoring_interval_minutes` `monitoringIntervalMinutes` INT NOT NULL,
  CHANGE COLUMN `notify_on_changes` `notifyOnChanges` BOOLEAN NOT NULL,
  CHANGE COLUMN `notification_email` `notificationEmail` VARCHAR(255) NULL,
  CHANGE COLUMN `track_changes_history` `trackChangesHistory` BOOLEAN NOT NULL,
  CHANGE COLUMN `enable_regulatory_content_analysis` `enableRegulatoryContentAnalysis` BOOLEAN NOT NULL,
  CHANGE COLUMN `track_regulatory_changes` `trackRegulatoryChanges` BOOLEAN NOT NULL,
  CHANGE COLUMN `classify_regulatory_documents` `classifyRegulatoryDocuments` BOOLEAN NOT NULL,
  CHANGE COLUMN `extract_structured_content` `extractStructuredContent` BOOLEAN NOT NULL,
  CHANGE COLUMN `process_pdf_documents` `processPdfDocuments` BOOLEAN NOT NULL,
  CHANGE COLUMN `monitor_high_impact_changes` `monitorHighImpactChanges` BOOLEAN NOT NULL,
  CHANGE COLUMN `extract_metadata` `extractMetadata` BOOLEAN NOT NULL,
  CHANGE COLUMN `extract_structured_data` `extractStructuredData` BOOLEAN NOT NULL,
  CHANGE COLUMN `custom_js_extractor` `customJsExtractor` TEXT NULL,
  CHANGE COLUMN `wait_for_selector` `waitForSelector` VARCHAR(255) NULL,
  CHANGE COLUMN `is_ukgc_website` `isUKGCWebsite` BOOLEAN NOT NULL,
  CHANGE COLUMN `prioritize_enforcement_actions` `prioritizeEnforcementActions` BOOLEAN NOT NULL,
  CHANGE COLUMN `prioritize_lccp` `prioritizeLCCP` BOOLEAN NOT NULL,
  CHANGE COLUMN `prioritize_aml` `prioritizeAML` BOOLEAN NOT NULL,
  CHANGE COLUMN `notification_endpoint` `notificationEndpoint` VARCHAR(255) NULL,
  CHANGE COLUMN `webhook_enabled` `webhookEnabled` BOOLEAN NOT NULL,
  CHANGE COLUMN `webhook_url` `webhookUrl` VARCHAR(255) NULL,
  CHANGE COLUMN `notify_on_content_changes` `notifyOnContentChanges` BOOLEAN NOT NULL,
  CHANGE COLUMN `notify_on_document_processed` `notifyOnDocumentProcessed` BOOLEAN NOT NULL,
  CHANGE COLUMN `notify_on_scraper_status_change` `notifyOnScraperStatusChange` BOOLEAN NOT NULL,
  CHANGE COLUMN `webhook_format` `webhookFormat` VARCHAR(50) NULL,
  CHANGE COLUMN `enable_content_compression` `enableContentCompression` BOOLEAN NOT NULL,
  CHANGE COLUMN `compression_threshold_bytes` `compressionThresholdBytes` INT NOT NULL,
  CHANGE COLUMN `collect_detailed_metrics` `collectDetailedMetrics` BOOLEAN NOT NULL,
  CHANGE COLUMN `metrics_reporting_interval_seconds` `metricsReportingIntervalSeconds` INT NOT NULL,
  CHANGE COLUMN `track_domain_metrics` `trackDomainMetrics` BOOLEAN NOT NULL,
  CHANGE COLUMN `scraper_type` `scraperType` VARCHAR(50) NULL;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Verify the column names after renaming
DESCRIBE `scraperconfig`;

-- Print success message
SELECT 'All columns in the scraperconfig table have been renamed to proper camelCase format.' AS Result;