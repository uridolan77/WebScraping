// filepath: c:\dev\WebScraping\fix_entity_tracking_in_repository.sql
-- SQL script to fix tracking issues in the ScraperLog table
-- This will help prevent "Unexpected entry.EntityState: Unchanged" errors

USE webstraction_db;

-- Verify the ScraperLog table schema 
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

-- We need to ensure that the id column is set as auto_increment and primary key
-- This query checks if we need to modify the id column
SELECT COUNT(*) INTO @id_needs_fix
FROM information_schema.columns 
WHERE 
    table_schema = 'webstraction_db' 
    AND table_name = 'scraperlog' 
    AND column_name = 'id'
    AND (EXTRA != 'auto_increment' OR COLUMN_KEY != 'PRI');

-- Fix the id column if needed
SET @fix_id = IF(@id_needs_fix > 0, 
    'ALTER TABLE scraperlog MODIFY COLUMN id INT NOT NULL AUTO_INCREMENT PRIMARY KEY;',
    'SELECT "ScraperLog id column is correctly configured" AS Result;');

PREPARE stmt FROM @fix_id;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check for any possible corrupted data in the ScraperLog table
-- This could cause EF tracking issues
SELECT COUNT(*) INTO @has_corrupted_data
FROM scraperlog
WHERE scraperId IS NULL OR logLevel IS NULL OR message IS NULL OR scraperId = '';

-- Log results about corrupted data if found
SELECT IF(@has_corrupted_data > 0, 
    CONCAT('Found ', @has_corrupted_data, ' potentially corrupted rows in ScraperLog'), 
    'No corrupted data found in ScraperLog') AS CorruptedDataCheck;

-- If there's corrupted data, let's fix it
SET @fix_corrupted = IF(@has_corrupted_data > 0,
    'DELETE FROM scraperlog WHERE scraperId IS NULL OR logLevel IS NULL OR message IS NULL OR scraperId = "";',
    'SELECT "No corrupted data to fix" AS Result;');

PREPARE stmt FROM @fix_corrupted;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Check for any orphaned scraper logs (referencing non-existent scrapers)
SELECT COUNT(*) INTO @orphaned_logs
FROM scraperlog s
LEFT JOIN scraperconfig c ON s.scraperId = c.id
WHERE c.id IS NULL;

-- Log results about orphaned logs if found
SELECT IF(@orphaned_logs > 0, 
    CONCAT('Found ', @orphaned_logs, ' orphaned logs (referencing non-existent scrapers)'), 
    'No orphaned logs found') AS OrphanedLogsCheck;

-- Now let's optimize the table to ensure proper indexing
OPTIMIZE TABLE scraperlog;

-- Verify all is well with a final check
SELECT 'ScraperLog table verification and optimization completed' AS Result;