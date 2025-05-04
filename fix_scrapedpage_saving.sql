-- Script to fix and optimize the ScrapedPage table
-- This will address the "Unexpected entry.EntityState: Unchanged" errors

USE webstraction_db;

-- Disable foreign key checks for the update
SET FOREIGN_KEY_CHECKS = 0;

-- Check if scrapedpage table exists
SELECT COUNT(*) INTO @table_exists 
FROM information_schema.tables 
WHERE table_schema = 'webstraction_db' AND table_name = 'scrapedpage';

-- If the scrapedpage table doesn't exist, create it
SET @create_table = IF(@table_exists = 0, '
CREATE TABLE `scrapedpage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraperId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `scraperConfigId` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `url` varchar(2048) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `htmlContent` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `textContent` mediumtext CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NULL,
  `scrapedAt` datetime NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_scrapedpage_scraperid` (`scraperId`),
  KEY `idx_scrapedpage_url` (`url`(768))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;', 'SELECT "ScrapedPage table already exists" AS Result');

PREPARE stmt FROM @create_table;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- If the table exists, check if we need to modify it
IF @table_exists > 0 THEN
    -- Check if scraperConfigId column exists
    SELECT COUNT(*) INTO @column_exists 
    FROM information_schema.columns 
    WHERE table_schema = 'webstraction_db' 
    AND table_name = 'scrapedpage' 
    AND column_name = 'scraperConfigId';
    
    -- Add the scraperConfigId column if it doesn't exist
    SET @add_column = IF(@column_exists = 0, 
        'ALTER TABLE scrapedpage ADD COLUMN scraperConfigId varchar(255) NULL AFTER scraperId;',
        'SELECT "scraperConfigId column already exists" AS Result');
        
    PREPARE stmt FROM @add_column;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    
    -- Check column types and update if needed
    SELECT 
        IF(DATA_TYPE != 'varchar' OR CHARACTER_MAXIMUM_LENGTH != 255, 1, 0) INTO @scraperid_needs_fix
    FROM 
        information_schema.columns 
    WHERE 
        table_schema = 'webstraction_db' 
        AND table_name = 'scrapedpage' 
        AND column_name = 'scraperId';
    
    -- Fix scraperId column if needed
    IF @scraperid_needs_fix = 1 THEN
        ALTER TABLE scrapedpage MODIFY COLUMN scraperId varchar(255) NOT NULL;
        SELECT 'Fixed scraperId column to varchar(255)' AS Result;
    END IF;
    
    -- Fix url column if needed
    SELECT 
        IF(DATA_TYPE != 'varchar' OR CHARACTER_MAXIMUM_LENGTH != 2048, 1, 0) INTO @url_needs_fix
    FROM 
        information_schema.columns 
    WHERE 
        table_schema = 'webstraction_db' 
        AND table_name = 'scrapedpage' 
        AND column_name = 'url';
    
    IF @url_needs_fix = 1 THEN
        ALTER TABLE scrapedpage MODIFY COLUMN url varchar(2048) NOT NULL;
        SELECT 'Fixed url column to varchar(2048)' AS Result;
    END IF;
    
    -- Fix htmlContent column if needed
    SELECT 
        IF(DATA_TYPE != 'mediumtext', 1, 0) INTO @htmlcontent_needs_fix
    FROM 
        information_schema.columns 
    WHERE 
        table_schema = 'webstraction_db' 
        AND table_name = 'scrapedpage' 
        AND column_name = 'htmlContent';
    
    IF @htmlcontent_needs_fix = 1 THEN
        ALTER TABLE scrapedpage MODIFY COLUMN htmlContent mediumtext NULL;
        SELECT 'Fixed htmlContent column to mediumtext' AS Result;
    END IF;
    
    -- Fix textContent column if needed
    SELECT 
        IF(DATA_TYPE != 'mediumtext', 1, 0) INTO @textcontent_needs_fix
    FROM 
        information_schema.columns 
    WHERE 
        table_schema = 'webstraction_db' 
        AND table_name = 'scrapedpage' 
        AND column_name = 'textContent';
    
    IF @textcontent_needs_fix = 1 THEN
        ALTER TABLE scrapedpage MODIFY COLUMN textContent mediumtext NULL;
        SELECT 'Fixed textContent column to mediumtext' AS Result;
    END IF;
    
    -- Check if index on scraperId exists
    SELECT COUNT(*) INTO @index_exists 
    FROM information_schema.statistics 
    WHERE table_schema = 'webstraction_db' 
    AND table_name = 'scrapedpage' 
    AND index_name = 'idx_scrapedpage_scraperid';
    
    -- Add index if it doesn't exist
    SET @create_index = IF(@index_exists = 0, 
        'CREATE INDEX idx_scrapedpage_scraperid ON scrapedpage(scraperId);',
        'SELECT "Index on scraperId already exists" AS Result');
        
    PREPARE stmt FROM @create_index;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
    
    -- Check if index on url exists
    SELECT COUNT(*) INTO @url_index_exists 
    FROM information_schema.statistics 
    WHERE table_schema = 'webstraction_db' 
    AND table_name = 'scrapedpage' 
    AND index_name = 'idx_scrapedpage_url';
    
    -- Add url index if it doesn't exist
    SET @create_url_index = IF(@url_index_exists = 0, 
        'CREATE INDEX idx_scrapedpage_url ON scrapedpage(url(768));',
        'SELECT "Index on url already exists" AS Result');
        
    PREPARE stmt FROM @create_url_index;
    EXECUTE stmt;
    DEALLOCATE PREPARE stmt;
END IF;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

SELECT 'ScrapedPage table check and fix completed' AS Result;
