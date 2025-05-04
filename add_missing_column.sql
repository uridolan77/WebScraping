-- SQL Script to add the missing AdjustDepthBasedOnQuality column to the scraperconfig table

-- Check if the column already exists
SET @columnExists = 0;
SELECT COUNT(*) INTO @columnExists
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'webstraction_db'
  AND TABLE_NAME = 'scraperconfig' 
  AND COLUMN_NAME = 'adjust_depth_based_on_quality';

-- Only add the column if it doesn't exist
SET @query = IF(@columnExists = 0, 
  'ALTER TABLE `scraperconfig` ADD COLUMN `adjust_depth_based_on_quality` BOOLEAN NOT NULL DEFAULT FALSE',
  'SELECT "Column already exists, no changes made." AS Result');

PREPARE stmt FROM @query;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Set default value for existing rows
UPDATE `scraperconfig` SET `adjust_depth_based_on_quality` = FALSE WHERE `adjust_depth_based_on_quality` IS NULL;

-- Verify the column was added
SELECT 
  COLUMN_NAME, 
  DATA_TYPE,
  IS_NULLABLE,
  COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'webstraction_db'
  AND TABLE_NAME = 'scraperconfig' 
  AND COLUMN_NAME = 'adjust_depth_based_on_quality';