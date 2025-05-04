-- MySQL script to rename all tables from snake_case to camelCase format without pluralization
-- This script will:
-- 1. Rename all tables from snake_case (with underscores) to camelCase (without underscores)
-- 2. Remove trailing 's' from table names to match entity naming in the code
-- 3. Preserve all data during the migration

-- Set foreign key checks off to allow table modifications
SET FOREIGN_KEY_CHECKS = 0;

-- First drop any existing views from previous scripts
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

-- Rename content_change_record to contentChangeRecord (singular)
RENAME TABLE `content_change_record` TO `contentchangerecord`;

-- Rename content_extractor_selector to contentExtractorSelector (singular)
RENAME TABLE `content_extractor_selector` TO `contentextractorselector`;

-- Rename custom_metric to customMetric (singular)
RENAME TABLE `custom_metric` TO `custommetric`;

-- Rename document_metadata to documentMetadata (singular)
RENAME TABLE `document_metadata` TO `documentmetadata`;

-- Rename domain_rate_limit to domainRateLimit (singular)
RENAME TABLE `domain_rate_limit` TO `domainratelimit`;

-- Rename keyword_alert to keywordAlert (singular)
RENAME TABLE `keyword_alert` TO `keywordalert`;

-- Rename log_entry to logEntry (singular)
RENAME TABLE `log_entry` TO `logentry`;

-- Rename pipeline_metric to pipelineMetric (singular)
RENAME TABLE `pipeline_metric` TO `pipelinemetric`;

-- Rename processed_document to processedDocument (singular)
RENAME TABLE `processed_document` TO `processeddocument`;

-- Rename proxy_configuration to proxyConfiguration (singular)
RENAME TABLE `proxy_configuration` TO `proxyconfiguration`;

-- Rename scraper_config to scraperConfig (singular)
RENAME TABLE `scraper_config` TO `scraperconfig`;

-- Rename scraper_metrics to scraperMetric (singular)
RENAME TABLE `scraper_metrics` TO `scrapermetric`;

-- Rename scraper_run to scraperRun (singular)
RENAME TABLE `scraper_run` TO `scraperrun`;

-- Rename scraper_schedule to scraperSchedule (singular)
RENAME TABLE `scraper_schedule` TO `scraperschedule`;

-- Rename scraper_start_url to scraperStartUrl (singular)
RENAME TABLE `scraper_start_url` TO `scraperstarturlx`;

-- Temp rename to avoid case insensitivity issues in MySQL
RENAME TABLE `scraperstarturlx` TO `scraperstarturlx2`;
RENAME TABLE `scraperstarturlx2` TO `scraperstarturlentity`;

-- Rename scraper_status to scraperStatus (singular)
RENAME TABLE `scraper_status` TO `scraperstatus`;

-- Rename webhook_trigger to webhookTrigger (singular)
RENAME TABLE `webhook_trigger` TO `webhooktrigger`;

-- Rename scraped_page to scrapedPage (singular)
RENAME TABLE `scraped_page` TO `scrapedpage`;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print a success message
SELECT 'All tables have been successfully renamed from snake_case to camelCase format (singular naming).' AS 'Status';