-- Fix table names in webstraction_db to ensure consistent naming
-- This script addresses the naming inconsistency by creating proper snake_case table names

-- Create missing tables with proper snake_case names if they don't exist
-- First, create proxy_configuration table if it doesn't exist
CREATE TABLE IF NOT EXISTS `proxy_configuration` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `scraper_id` VARCHAR(36) NOT NULL,
    `proxy_url` VARCHAR(255) NOT NULL,
    `proxy_type` VARCHAR(50) NULL,
    `username` VARCHAR(100) NULL,
    `password` VARCHAR(100) NULL,
    `is_active` BOOLEAN DEFAULT TRUE,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `last_used` DATETIME NULL,
    FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create scraper_schedule table if it doesn't exist
CREATE TABLE IF NOT EXISTS `scraper_schedule` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `scraper_id` VARCHAR(36) NOT NULL,
    `scraper_config_id` VARCHAR(36) NULL,
    `name` VARCHAR(100) NOT NULL,
    `cron_expression` VARCHAR(100) NOT NULL,
    `is_active` BOOLEAN DEFAULT TRUE,
    `last_run` DATETIME NULL,
    `next_run` DATETIME NULL,
    `max_runtime_minutes` INT DEFAULT 30,
    `notification_email` VARCHAR(255) NULL,
    FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config`(`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create views that map between camelCase and snake_case table names
-- This helps ensure backward compatibility with code expecting camelCase names

-- Create view for proxyconfigurations
CREATE OR REPLACE VIEW `proxyconfigurations` AS
SELECT * FROM `proxy_configuration`;

-- Create view for scraperschedules
CREATE OR REPLACE VIEW `scraperschedules` AS
SELECT * FROM `scraper_schedule`;

-- Similarly, create views for other potential naming inconsistencies
CREATE OR REPLACE VIEW `scraperconfigs` AS
SELECT * FROM `scraper_config`;

CREATE OR REPLACE VIEW `scraperstarturls` AS
SELECT * FROM `scraper_start_url`;

CREATE OR REPLACE VIEW `contentextractorselectors` AS
SELECT * FROM `content_extractor_selector`;

CREATE OR REPLACE VIEW `keywordalerts` AS
SELECT * FROM `keyword_alert`;

CREATE OR REPLACE VIEW `webhooktriggers` AS
SELECT * FROM `webhook_trigger`;

CREATE OR REPLACE VIEW `domainratelimits` AS
SELECT * FROM `domain_rate_limit`;

CREATE OR REPLACE VIEW `scraperruns` AS
SELECT * FROM `scraper_run`;

CREATE OR REPLACE VIEW `scraperstatuses` AS
SELECT * FROM `scraper_status`;

CREATE OR REPLACE VIEW `pipelinemetrics` AS
SELECT * FROM `pipeline_metric`;

CREATE OR REPLACE VIEW `logentries` AS
SELECT * FROM `log_entry`;

CREATE OR REPLACE VIEW `contentchangerecords` AS
SELECT * FROM `content_change_record`;

CREATE OR REPLACE VIEW `processeddocuments` AS
SELECT * FROM `processed_document`;

CREATE OR REPLACE VIEW `documentmetadata` AS
SELECT * FROM `document_metadata`;

CREATE OR REPLACE VIEW `scrapermetrics` AS
SELECT * FROM `scraper_metric`;

CREATE OR REPLACE VIEW `scraperlogs` AS
SELECT * FROM `scraper_log`;

CREATE OR REPLACE VIEW `scrapedpages` AS
SELECT * FROM `scraped_page`;

-- Output success message
SELECT 'Table naming consistency fixes applied successfully' AS Result;