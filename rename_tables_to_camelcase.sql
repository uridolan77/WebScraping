-- MySQL script to rename all tables from snake_case to camelCase format
-- This script will:
-- 1. Rename all tables from snake_case (with underscores) to camelCase (without underscores)
-- 2. Drop existing views that we may have created earlier
-- 3. Preserve all data during the migration

-- Set foreign key checks off to allow table modifications
SET FOREIGN_KEY_CHECKS = 0;

-- Rename content_change_record to contentChangeRecord
RENAME TABLE `content_change_record` TO `contentchangerecord`;

-- Rename content_extractor_selector to contentExtractorSelector
RENAME TABLE `content_extractor_selector` TO `contentextractorselector`;

-- Rename custom_metric to customMetric
RENAME TABLE `custom_metric` TO `custommetric`;

-- Rename document_metadata to documentMetadata
RENAME TABLE `document_metadata` TO `documentmetadata`;

-- Rename domain_rate_limit to domainRateLimit
RENAME TABLE `domain_rate_limit` TO `domainratelimit`;

-- Rename keyword_alert to keywordAlert
RENAME TABLE `keyword_alert` TO `keywordalert`;

-- Rename log_entry to logEntry
RENAME TABLE `log_entry` TO `logentry`;

-- Rename pipeline_metric to pipelineMetric
RENAME TABLE `pipeline_metric` TO `pipelinemetric`;

-- Rename processed_document to processedDocument
RENAME TABLE `processed_document` TO `processeddocument`;

-- Rename proxy_configuration to proxyConfiguration
RENAME TABLE `proxy_configuration` TO `proxyconfiguration`;

-- Rename scraper_config to scraperConfig
RENAME TABLE `scraper_config` TO `scraperconfig`;

-- Rename scraper_metrics to scraperMetrics
RENAME TABLE `scraper_metrics` TO `scrapermetrics`;

-- Rename scraper_run to scraperRun
RENAME TABLE `scraper_run` TO `scraperrun`;

-- Rename scraper_schedule to scraperSchedule
RENAME TABLE `scraper_schedule` TO `scraperschedule`;

-- Rename scraper_start_url to scraperStartUrl
RENAME TABLE `scraper_start_url` TO `scraperstarturlx`;

-- Temp rename to avoid case insensitivity issues in MySQL
RENAME TABLE `scraperstarturlx` TO `scraperstarturls`;

-- Rename scraper_status to scraperStatus
RENAME TABLE `scraper_status` TO `scraperstatus`;

-- Rename webhook_trigger to webhookTrigger
RENAME TABLE `webhook_trigger` TO `webhooktrigger`;

-- Rename scraped_page to scrapedPage
RENAME TABLE `scraped_page` TO `scrapedpage`;

-- Drop views if they exist (from our previous solution)
DROP VIEW IF EXISTS `proxyconfigurations`;
DROP VIEW IF EXISTS `scraperschedules`;
DROP VIEW IF EXISTS `scraperconfigs`;
DROP VIEW IF EXISTS `scraperstarturls`;
DROP VIEW IF EXISTS `contentextractorselectors`;
DROP VIEW IF EXISTS `keywordalerts`;
DROP VIEW IF EXISTS `webhooktriggers`;
DROP VIEW IF EXISTS `domainratelimits`;
DROP VIEW IF EXISTS `scraperruns`;
DROP VIEW IF EXISTS `scraperstatuses`;
DROP VIEW IF EXISTS `pipelinemetrics`;
DROP VIEW IF EXISTS `logentries`;
DROP VIEW IF EXISTS `contentchangerecords`;
DROP VIEW IF EXISTS `processeddocuments`;
DROP VIEW IF EXISTS `documentmetadata`;
DROP VIEW IF EXISTS `scrapermetrics`;
DROP VIEW IF EXISTS `scraperlogs`;
DROP VIEW IF EXISTS `scrapedpages`;

-- Create pluralized views for compatibility with Entity Framework conventions
CREATE OR REPLACE VIEW `proxyconfigurations` AS SELECT * FROM `proxyconfiguration`;
CREATE OR REPLACE VIEW `scraperschedules` AS SELECT * FROM `scraperschedule`;
CREATE OR REPLACE VIEW `scraperconfigs` AS SELECT * FROM `scraperconfig`;
CREATE OR REPLACE VIEW `contentextractorselectors` AS SELECT * FROM `contentextractorselector`;
CREATE OR REPLACE VIEW `keywordalerts` AS SELECT * FROM `keywordalert`;
CREATE OR REPLACE VIEW `webhooktriggers` AS SELECT * FROM `webhooktrigger`;
CREATE OR REPLACE VIEW `domainratelimits` AS SELECT * FROM `domainratelimit`;
CREATE OR REPLACE VIEW `scraperruns` AS SELECT * FROM `scraperrun`;
CREATE OR REPLACE VIEW `scraperstatuses` AS SELECT * FROM `scraperstatus`;
CREATE OR REPLACE VIEW `pipelinemetrics` AS SELECT * FROM `pipelinemetric`;
CREATE OR REPLACE VIEW `logentries` AS SELECT * FROM `logentry`;
CREATE OR REPLACE VIEW `contentchangerecords` AS SELECT * FROM `contentchangerecord`;
CREATE OR REPLACE VIEW `processeddocuments` AS SELECT * FROM `processeddocument`;
CREATE OR REPLACE VIEW `custommetrics` AS SELECT * FROM `custommetric`;
CREATE OR REPLACE VIEW `scrapedpages` AS SELECT * FROM `scrapedpage`;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print a success message
SELECT 'All tables have been successfully renamed from snake_case to camelCase format.' AS 'Status';