-- SQL Script to update the scrapedpage table to match our entity model

-- Disable foreign key checks to avoid issues during table modification
SET FOREIGN_KEY_CHECKS = 0;

-- Check if the scrapedpage table exists
SELECT COUNT(*) INTO @table_exists FROM information_schema.tables
WHERE table_schema = 'webstraction_db' AND table_name = 'scrapedpage';

-- Check if columns need to be updated
SELECT COUNT(*) INTO @column_exists FROM information_schema.columns
WHERE table_schema = 'webstraction_db' AND table_name = 'scrapedpage'
AND column_name = 'htmlContent';

-- Check if snake_case columns exist
SELECT COUNT(*) INTO @snake_column_exists FROM information_schema.columns
WHERE table_schema = 'webstraction_db' AND table_name = 'scrapedpage'
AND column_name = 'html_content';

-- If the table doesn't exist, create it
SET @create_table = CONCAT("
CREATE TABLE IF NOT EXISTS `scrapedpage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraperId` varchar(255) NOT NULL,
  `url` varchar(2048) NOT NULL,
  `htmlContent` mediumtext,
  `textContent` mediumtext,
  `scrapedAt` datetime NOT NULL,
  `scraperConfigId` int NULL,
  PRIMARY KEY (`id`),
  KEY `idx_scrapedpage_scraperid` (`scraperId`),
  KEY `idx_scrapedpage_url` (`url`(768))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
");

-- If the table exists with snake_case columns, alter it
SET @alter_table = CONCAT("
ALTER TABLE scrapedpage
CHANGE COLUMN `scraper_id` `scraperId` VARCHAR(255) NOT NULL,
CHANGE COLUMN `html_content` `htmlContent` MEDIUMTEXT,
CHANGE COLUMN `text_content` `textContent` MEDIUMTEXT,
CHANGE COLUMN `scraped_at` `scrapedAt` DATETIME NOT NULL;
");

-- Execute the appropriate SQL based on conditions
SET @sql = IF(@table_exists = 0, @create_table,
              IF(@column_exists > 0, 'SELECT "scrapedpage table already has camelCase column names, no changes needed" AS Result',
                 IF(@snake_column_exists > 0, @alter_table, 'SELECT "scrapedpage table has unexpected column names, manual inspection needed" AS Result')));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print success message
SELECT 'scrapedpage table has been checked and updated if needed' AS 'Result';
