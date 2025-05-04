-- Simple and direct MySQL script to rename columns in scraperconfig table
-- This script explicitly renames each column using ALTER TABLE statements

SET FOREIGN_KEY_CHECKS = 0;

-- Check if the table exists first
SELECT COUNT(*) INTO @table_exists FROM information_schema.tables 
WHERE table_schema = 'webstraction_db' AND table_name = 'scraperconfig';

-- Only try to rename if the table exists
SET @rename_sql = IF(@table_exists > 0, 'SELECT "Renaming columns in scraperconfig table..." AS message', 'SELECT "Table scraperconfig does not exist!" AS message');
PREPARE stmt FROM @rename_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Rename columns in scraperconfig table if the table exists
SET @rename_config = IF(@table_exists > 0, 
'ALTER TABLE `scraperconfig` 
  CHANGE COLUMN `adjust_depth_based_on_quality` `adjustDepthBasedOnQuality` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `auto_learn_header_footer` `autoLearnHeaderFooter` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `back_off_on_errors` `backOffOnErrors` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `base_url` `baseUrl` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `classify_regulatory_documents` `classifyRegulatoryDocuments` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `collect_detailed_metrics` `collectDetailedMetrics` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `compression_threshold_bytes` `compressionThresholdBytes` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `created_at` `createdAt` DATETIME NOT NULL,
  CHANGE COLUMN `custom_js_extractor` `customJsExtractor` TEXT NULL,
  CHANGE COLUMN `delay_between_requests` `delayBetweenRequests` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `enable_adaptive_crawling` `enableAdaptiveCrawling` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `enable_adaptive_rate_limiting` `enableAdaptiveRateLimiting` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `enable_change_detection` `enableChangeDetection` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `enable_content_compression` `enableContentCompression` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `enable_continuous_monitoring` `enableContinuousMonitoring` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `enable_regulatory_content_analysis` `enableRegulatoryContentAnalysis` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `extract_metadata` `extractMetadata` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `extract_structured_content` `extractStructuredContent` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `extract_structured_data` `extractStructuredData` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `follow_external_links` `followExternalLinks` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `follow_links` `followLinks` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `is_ukgc_website` `isUKGCWebsite` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `last_modified` `lastModified` DATETIME NOT NULL,
  CHANGE COLUMN `last_run` `lastRun` DATETIME NULL,
  CHANGE COLUMN `learning_pages_count` `learningPagesCount` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_concurrent_requests` `maxConcurrentRequests` INT NOT NULL DEFAULT 1,
  CHANGE COLUMN `max_delay_between_requests` `maxDelayBetweenRequests` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_depth` `maxDepth` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_pages` `maxPages` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_proxy_failures_before_removal` `maxProxyFailuresBeforeRemoval` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_requests_per_minute` `maxRequestsPerMinute` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `max_versions_to_keep` `maxVersionsToKeep` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `metrics_reporting_interval_seconds` `metricsReportingIntervalSeconds` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `min_delay_between_requests` `minDelayBetweenRequests` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `monitor_high_impact_changes` `monitorHighImpactChanges` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `monitor_response_times` `monitorResponseTimes` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `monitoring_interval_minutes` `monitoringIntervalMinutes` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `notification_email` `notificationEmail` VARCHAR(255) NULL,
  CHANGE COLUMN `notification_endpoint` `notificationEndpoint` VARCHAR(255) NULL,
  CHANGE COLUMN `notify_on_changes` `notifyOnChanges` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `notify_on_content_changes` `notifyOnContentChanges` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `notify_on_document_processed` `notifyOnDocumentProcessed` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `notify_on_scraper_status_change` `notifyOnScraperStatusChange` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `output_directory` `outputDirectory` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `prioritize_aml` `prioritizeAML` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `prioritize_enforcement_actions` `prioritizeEnforcementActions` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `prioritize_lccp` `prioritizeLCCP` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `priority_queue_size` `priorityQueueSize` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `process_pdf_documents` `processPdfDocuments` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `proxy_rotation_strategy` `proxyRotationStrategy` VARCHAR(50) NULL,
  CHANGE COLUMN `respect_robots_txt` `respectRobotsTxt` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `run_count` `runCount` INT NOT NULL DEFAULT 0,
  CHANGE COLUMN `scraper_type` `scraperType` VARCHAR(50) NULL,
  CHANGE COLUMN `start_url` `startUrl` VARCHAR(255) NOT NULL,
  CHANGE COLUMN `test_proxies_before_use` `testProxiesBeforeUse` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `track_changes_history` `trackChangesHistory` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `track_content_versions` `trackContentVersions` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `track_domain_metrics` `trackDomainMetrics` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `track_regulatory_changes` `trackRegulatoryChanges` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `use_proxies` `useProxies` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `user_agent` `userAgent` VARCHAR(255) NULL,
  CHANGE COLUMN `wait_for_selector` `waitForSelector` VARCHAR(255) NULL,
  CHANGE COLUMN `webhook_enabled` `webhookEnabled` BOOLEAN NOT NULL DEFAULT FALSE,
  CHANGE COLUMN `webhook_format` `webhookFormat` VARCHAR(50) NULL,
  CHANGE COLUMN `webhook_url` `webhookUrl` VARCHAR(255) NULL
', 'SELECT "Skipping rename operation" AS message');

PREPARE stmt FROM @rename_config;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if the rename was successful
SELECT COUNT(*) INTO @camel_case_columns FROM information_schema.columns 
WHERE table_schema = 'webstraction_db' AND table_name = 'scraperconfig' AND column_name = 'adjustDepthBasedOnQuality';

-- Report success or failure
SET @result = IF(@camel_case_columns > 0, 
                 'Column renaming successful! Found camelCase columns.', 
                 'Column renaming failed! No camelCase columns found.');
                 
SELECT @result AS Result;

SET FOREIGN_KEY_CHECKS = 1;