-- Script to create the missing scrapermetric table
-- This follows the snake_case naming convention used in the database

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

-- Add some comments to explain the table
ALTER TABLE `scrapermetric`
COMMENT = 'Stores metrics data for scrapers such as performance, counts, etc.';
