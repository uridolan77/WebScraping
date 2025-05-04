-- First, create the scrapermetric table if it doesn't exist
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

-- Now check if the scraper with ID 70344351-c33f-4d0c-8f27-dd478ff257da exists
SET @scraper_id = '70344351-c33f-4d0c-8f27-dd478ff257da';
SET @scraper_exists = (SELECT COUNT(*) FROM `scraperconfig` WHERE `id` = @scraper_id);

-- If the scraper doesn't exist, create it
SET @scraper_name = 'UKGC Scraper';
SET @start_url = 'https://www.gamblingcommission.gov.uk/';

-- Only insert if it doesn't exist
SET @insert_sql = CONCAT('
    INSERT INTO `scraperconfig` (`id`, `name`, `start_url`, `base_url`, `max_depth`, `max_pages`, `follow_links`, `follow_external_links`, `created_at`, `last_modified`)
    SELECT "', @scraper_id, '", "', @scraper_name, '", "', @start_url, '", "', @start_url, '", 5, 1000, 1, 0, NOW(), NOW()
    WHERE NOT EXISTS (SELECT 1 FROM `scraperconfig` WHERE `id` = "', @scraper_id, '")
');

PREPARE stmt FROM @insert_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if we need to add a start URL for this scraper
SET @start_url_exists = (SELECT COUNT(*) FROM `scraperstarturlentity` WHERE `scraper_id` = @scraper_id);

-- If no start URL exists, add one
SET @insert_start_url_sql = CONCAT('
    INSERT INTO `scraperstarturlentity` (`scraper_id`, `url`)
    SELECT "', @scraper_id, '", "', @start_url, '"
    WHERE NOT EXISTS (SELECT 1 FROM `scraperstarturlentity` WHERE `scraper_id` = "', @scraper_id, '")
');

PREPARE stmt FROM @insert_start_url_sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Verify the scraper exists now
SELECT * FROM `scraperconfig` WHERE `id` = @scraper_id;
