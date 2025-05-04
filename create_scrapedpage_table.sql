-- SQL Script to create the missing scrapedpage table
-- This script is based on the structure defined in ScrapedPageEntity.cs

-- Disable foreign key checks to avoid issues during table creation
SET FOREIGN_KEY_CHECKS = 0;

-- Create scrapedpage table if it doesn't exist
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
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print success message
SELECT 'scrapedpage table has been created successfully' AS 'Result';