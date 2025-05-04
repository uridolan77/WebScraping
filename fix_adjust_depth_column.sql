-- SQL Script to fix the AdjustDepthBasedOnQuality column issue
-- This script will create or rename the column to exactly match what EF Core expects

-- First, let's check if we have an existing adjust_depth_based_on_quality column
SET @columnExists = 0;
SELECT COUNT(*) INTO @columnExists
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'webstraction_db'
  AND TABLE_NAME = 'scraperconfig' 
  AND COLUMN_NAME = 'adjust_depth_based_on_quality';

-- If the column exists, drop it (we'll recreate it with the correct name)
SET @dropColumn = IF(@columnExists > 0, 
  'ALTER TABLE `scraperconfig` DROP COLUMN `adjust_depth_based_on_quality`',
  'SELECT "No adjust_depth_based_on_quality column to drop." AS Result');

PREPARE stmt FROM @dropColumn;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Now check if we have the camelCase version of the column
SET @camelCaseColumnExists = 0;
SELECT COUNT(*) INTO @camelCaseColumnExists
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'webstraction_db'
  AND TABLE_NAME = 'scraperconfig' 
  AND COLUMN_NAME = 'adjustdepthbasedonquality';

-- Create the column with the exact name Entity Framework is expecting
-- Use camelCase for the column name
SET @createColumn = IF(@camelCaseColumnExists = 0, 
  'ALTER TABLE `scraperconfig` ADD COLUMN `adjustdepthbasedonquality` BOOLEAN NOT NULL DEFAULT FALSE',
  'SELECT "Column already exists, no changes made." AS Result');

PREPARE stmt FROM @createColumn;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;

-- Set default value for existing rows
UPDATE `scraperconfig` SET `adjustdepthbasedonquality` = FALSE WHERE `adjustdepthbasedonquality` IS NULL;

-- Verify the column exists with the right name
SELECT 
  COLUMN_NAME, 
  DATA_TYPE,
  IS_NULLABLE,
  COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = 'webstraction_db'
  AND TABLE_NAME = 'scraperconfig' 
  AND COLUMN_NAME = 'adjustdepthbasedonquality';