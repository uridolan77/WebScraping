-- Script to create the missing scrapermetric table and ensure the scraper exists
-- Run this in MySQL Workbench or any MySQL client

USE webstraction_db;

-- Create the scrapermetric table if it doesn't exist
CREATE TABLE IF NOT EXISTS `scrapermetric` (
    `id` INT AUTO_INCREMENT PRIMARY KEY,
    `scraper_id` VARCHAR(255) NOT NULL,
    `scraper_name` VARCHAR(255) NOT NULL,
    `metric_name` VARCHAR(255) NOT NULL,
    `metric_value` DOUBLE NOT NULL,
    `timestamp` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    
    -- Add index on scraper_id for better performance on foreign key lookups
    INDEX `idx_scrapermetric_scraper_id` (`scraper_id`),
    
    -- Add foreign key constraint
    CONSTRAINT `fk_scrapermetric_scraperconfig` 
    FOREIGN KEY (`scraper_id`) 
    REFERENCES `scraperconfig` (`id`) 
    ON DELETE CASCADE
);

-- Check if the scraper with ID 70344351-c33f-4d0c-8f27-dd478ff257da exists
-- If not, create it
INSERT IGNORE INTO `scraperconfig` 
(`id`, `name`, `start_url`, `base_url`, `max_depth`, `max_pages`, `follow_links`, `follow_external_links`, `created_at`, `last_modified`)
VALUES 
('70344351-c33f-4d0c-8f27-dd478ff257da', 'UKGC Scraper', 'https://www.gamblingcommission.gov.uk/', 'https://www.gamblingcommission.gov.uk/', 5, 1000, 1, 0, NOW(), NOW());

-- Add a start URL for this scraper if it doesn't exist
INSERT IGNORE INTO `scraperstarturlentity` 
(`scraper_id`, `url`)
VALUES 
('70344351-c33f-4d0c-8f27-dd478ff257da', 'https://www.gamblingcommission.gov.uk/');

-- Verify the scraper exists
SELECT * FROM `scraperconfig` WHERE `id` = '70344351-c33f-4d0c-8f27-dd478ff257da';
