-- filepath: c:\dev\WebScraping\fix_scraperlog_table.sql
-- SQL Script to fix the scraperlog table structure

-- Disable foreign key checks to avoid issues during table modification
SET FOREIGN_KEY_CHECKS = 0;

-- Check if the scraperlog table exists
SELECT COUNT(*) INTO @table_exists FROM information_schema.tables
WHERE table_schema = 'webstraction_db' AND table_name = 'scraperlog';

-- Create the table if it doesn't exist
SET @create_table = CONCAT("
CREATE TABLE IF NOT EXISTS `scraperlog` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraperId` varchar(255) NOT NULL,
  `timestamp` datetime NOT NULL,
  `logLevel` varchar(50) NOT NULL,
  `message` text NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_scraperlog_scraperid` (`scraperId`),
  KEY `idx_scraperlog_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
");

-- Check if any columns need to be fixed
SET @check_columns = CONCAT("
SELECT 
  COUNT(*) INTO @needs_fix 
FROM information_schema.columns 
WHERE 
  table_schema = 'webstraction_db' 
  AND table_name = 'scraperlog' 
  AND (
    (column_name = 'id' AND data_type != 'int') OR
    (column_name = 'scraperId' AND data_type != 'varchar') OR
    (column_name = 'timestamp' AND data_type != 'datetime') OR
    (column_name = 'logLevel' AND data_type != 'varchar') OR
    (column_name = 'message' AND data_type != 'text')
  );
");

-- Fix column definitions if needed
SET @fix_columns = CONCAT("
ALTER TABLE scraperlog
  MODIFY COLUMN `id` int NOT NULL AUTO_INCREMENT,
  MODIFY COLUMN `scraperId` varchar(255) NOT NULL,
  MODIFY COLUMN `timestamp` datetime NOT NULL,
  MODIFY COLUMN `logLevel` varchar(50) NOT NULL,
  MODIFY COLUMN `message` text NOT NULL;
");

-- Execute statements based on whether the table exists
SET @sql = IF(@table_exists = 0, @create_table, CONCAT(@check_columns, "
  SELECT IF(@needs_fix > 0, '", @fix_columns, "', 'SELECT \"scraperlog table has correct structure\" AS Result');
"));

PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check if indexes exist
SELECT COUNT(*) INTO @index_scraperid_exists FROM information_schema.statistics
WHERE table_schema = 'webstraction_db' AND table_name = 'scraperlog' AND index_name = 'idx_scraperlog_scraperid';

SELECT COUNT(*) INTO @index_timestamp_exists FROM information_schema.statistics
WHERE table_schema = 'webstraction_db' AND table_name = 'scraperlog' AND index_name = 'idx_scraperlog_timestamp';

-- Create indexes if they don't exist
SET @create_indexes = CONCAT("
  ", IF(@index_scraperid_exists = 0, "CREATE INDEX idx_scraperlog_scraperid ON scraperlog(scraperId);", "SELECT 'ScraperId index exists' AS Result;"), "
  ", IF(@index_timestamp_exists = 0, "CREATE INDEX idx_scraperlog_timestamp ON scraperlog(timestamp);", "SELECT 'Timestamp index exists' AS Result;"), "
");

PREPARE stmt FROM @create_indexes;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Check the current structure of the scraperlog table
SELECT 
  COLUMN_NAME, 
  DATA_TYPE, 
  IS_NULLABLE, 
  COLUMN_KEY
FROM 
  information_schema.columns 
WHERE 
  table_schema = 'webstraction_db' 
  AND table_name = 'scraperlog' 
ORDER BY 
  ORDINAL_POSITION;

-- Print success message
SELECT 'scraperlog table has been checked and fixed if needed' AS 'Result';