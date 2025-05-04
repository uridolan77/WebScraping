-- Script to change all scraperConfigId fields to VARCHAR(255)
-- This ensures consistency with the Entity Framework Core model

USE webstraction_db;

-- Disable foreign key checks to avoid issues during schema modifications
SET FOREIGN_KEY_CHECKS = 0;

-- Find all tables that have a scraperConfigId column
SELECT 
    TABLE_NAME, 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH 
INTO @tables_found
FROM 
    INFORMATION_SCHEMA.COLUMNS 
WHERE 
    TABLE_SCHEMA = 'webstraction_db' 
    AND COLUMN_NAME IN ('scraperConfigId', 'scraper_config_id');

-- Create a stored procedure to execute ALTER TABLE statements
DROP PROCEDURE IF EXISTS update_scraperConfigId_columns;
DELIMITER //
CREATE PROCEDURE update_scraperConfigId_columns()
BEGIN
    DECLARE done INT DEFAULT FALSE;
    DECLARE table_name VARCHAR(255);
    DECLARE column_name VARCHAR(255);
    DECLARE data_type VARCHAR(255);
    DECLARE char_max_length INT;
    DECLARE alter_stmt VARCHAR(1000);
    
    -- Declare cursor for tables with scraperConfigId columns
    DECLARE table_cursor CURSOR FOR
        SELECT 
            TABLE_NAME, 
            COLUMN_NAME, 
            DATA_TYPE, 
            CHARACTER_MAXIMUM_LENGTH 
        FROM 
            INFORMATION_SCHEMA.COLUMNS 
        WHERE 
            TABLE_SCHEMA = 'webstraction_db' 
            AND COLUMN_NAME IN ('scraperConfigId', 'scraper_config_id');
    
    -- Declare continue handler
    DECLARE CONTINUE HANDLER FOR NOT FOUND SET done = TRUE;
    
    -- Open cursor
    OPEN table_cursor;
    
    -- Start reading
    read_loop: LOOP
        FETCH table_cursor INTO table_name, column_name, data_type, char_max_length;
        
        IF done THEN
            LEAVE read_loop;
        END IF;
        
        -- If the column is not already VARCHAR(255)
        IF data_type <> 'varchar' OR char_max_length <> 255 THEN
            -- Create ALTER TABLE statement
            SET @sql = CONCAT('ALTER TABLE `', table_name, '` 
                              MODIFY COLUMN `', column_name, '` VARCHAR(255);');
            
            -- Execute the statement
            PREPARE stmt FROM @sql;
            EXECUTE stmt;
            DEALLOCATE PREPARE stmt;
            
            -- Log the change
            SELECT CONCAT('Updated ', table_name, '.', column_name, ' to VARCHAR(255)') AS 'Change Applied';
        ELSE
            -- Log that no change was needed
            SELECT CONCAT(table_name, '.', column_name, ' is already VARCHAR(255)') AS 'No Change Needed';
        END IF;
    END LOOP;
    
    -- Close cursor
    CLOSE table_cursor;
END //
DELIMITER ;

-- Execute the procedure
CALL update_scraperConfigId_columns();

-- Drop the procedure
DROP PROCEDURE IF EXISTS update_scraperConfigId_columns;

-- Check for any tables with 'scrapedpage' in the name
SELECT 
    TABLE_NAME 
FROM 
    INFORMATION_SCHEMA.TABLES 
WHERE 
    TABLE_SCHEMA = 'webstraction_db' 
    AND TABLE_NAME LIKE '%scrapedpage%';

-- Ensure scrapedpage table has the correct scraperConfigId column
CREATE TABLE IF NOT EXISTS `scrapedpage` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraperId` varchar(255) NOT NULL,
  `url` varchar(2048) NOT NULL,
  `htmlContent` mediumtext,
  `textContent` mediumtext,
  `scrapedAt` datetime NOT NULL,
  `scraperConfigId` varchar(255) NULL,
  PRIMARY KEY (`id`),
  KEY `idx_scrapedpage_scraperid` (`scraperId`),
  KEY `idx_scrapedpage_url` (`url`(768))
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Add scraperConfigId to scrapedpage if it doesn't exist
SET @column_exists = (
    SELECT COUNT(*) 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_SCHEMA = 'webstraction_db' 
    AND TABLE_NAME = 'scrapedpage' 
    AND COLUMN_NAME = 'scraperConfigId'
);

SET @sql = IF(@column_exists = 0, 'ALTER TABLE scrapedpage ADD COLUMN scraperConfigId VARCHAR(255) NULL;', 'SELECT "scraperConfigId already exists in scrapedpage table" AS message');
PREPARE stmt FROM @sql;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Re-enable foreign key checks
SET FOREIGN_KEY_CHECKS = 1;

-- Print success message
SELECT 'All scraperConfigId fields have been updated to VARCHAR(255)' AS 'Result';