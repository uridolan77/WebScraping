-- SQL Script to check and fix database tables for the WebScraper application

-- Disable foreign key checks to avoid issues during table creation/modification
SET FOREIGN_KEY_CHECKS = 0;

-- Check if the scrapedpage table exists, if not create it
CREATE TABLE IF NOT EXISTS `scrapedpage` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `scraper_id` VARCHAR(36) NOT NULL,
  `url` VARCHAR(2000) NOT NULL,
  `html_content` MEDIUMTEXT NOT NULL,
  `text_content` MEDIUMTEXT NOT NULL,
  `scraped_at` DATETIME NOT NULL,
  
  -- Index on scraper_id for faster lookups
  INDEX `ix_scrapedpage_scraper_id` (`scraper_id`),
  
  -- Add foreign key constraint to scraperconfig table
  CONSTRAINT `fk_scrapedpage_scraperconfig` 
  FOREIGN KEY (`scraper_id`) 
  REFERENCES `scraperconfig` (`id`) 
  ON DELETE CASCADE
);

-- Check if the scraper_status table exists, if not create it
CREATE TABLE IF NOT EXISTS `scraper_status` (
  `scraper_id` VARCHAR(36) PRIMARY KEY,
  `is_running` BOOLEAN NOT NULL DEFAULT FALSE,
  `start_time` DATETIME NULL,
  `end_time` DATETIME NULL,
  `elapsed_time` VARCHAR(50) NULL,
  `urls_processed` INT NOT NULL DEFAULT 0,
  `urls_queued` INT NOT NULL DEFAULT 0,
  `documents_processed` INT NOT NULL DEFAULT 0,
  `has_errors` BOOLEAN NOT NULL DEFAULT FALSE,
  `message` VARCHAR(500) NULL,
  `last_status_update` DATETIME NULL,
  `last_update` DATETIME NULL,
  `last_monitor_check` DATETIME NULL,
  `last_error` TEXT NULL,
  
  -- Add foreign key constraint to scraperconfig table
  CONSTRAINT `fk_scraper_status_scraperconfig` 
  FOREIGN KEY (`scraper_id`) 
  REFERENCES `scraperconfig` (`id`) 
  ON DELETE CASCADE
);

-- Check if the scrapermetric table exists, if not create it
CREATE TABLE IF NOT EXISTS `scrapermetric` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `scraper_id` VARCHAR(36) NOT NULL,
  `metric_name` VARCHAR(100) NOT NULL,
  `metric_value` DOUBLE NOT NULL,
  `timestamp` DATETIME NOT NULL,
  
  -- Index on scraper_id for faster lookups
  INDEX `ix_scrapermetric_scraper_id` (`scraper_id`),
  
  -- Add foreign key constraint to scraperconfig table
  CONSTRAINT `fk_scrapermetric_scraperconfig` 
  FOREIGN KEY (`scraper_id`) 
  REFERENCES `scraperconfig` (`id`) 
  ON DELETE CASCADE
);

-- Check if the scraperlog table exists, if not create it
CREATE TABLE IF NOT EXISTS `scraperlog` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `scraper_id` VARCHAR(36) NOT NULL,
  `timestamp` DATETIME NOT NULL,
  `log_level` VARCHAR(20) NOT NULL,
  `message` TEXT NOT NULL,
  
  -- Index on scraper_id for faster lookups
  INDEX `ix_scraperlog_scraper_id` (`scraper_id`),
  
  -- Add foreign key constraint to scraperconfig table
  CONSTRAINT `fk_scraperlog_scraperconfig` 
  FOREIGN KEY (`scraper_id`) 
  REFERENCES `scraperconfig` (`id`) 
  ON DELETE CASCADE
);

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print success message
SELECT 'Database tables have been checked and fixed' AS 'Result';
