-- Script to fix the custommetric table to use snake_case column names

-- First, check if the table exists
SELECT COUNT(*) FROM information_schema.tables 
WHERE table_schema = 'webstraction_db' AND table_name = 'custommetric';

-- Rename columns to use snake_case if they exist in camelCase
ALTER TABLE `custommetric` 
CHANGE COLUMN IF EXISTS `metricName` `metric_name` VARCHAR(100) NOT NULL,
CHANGE COLUMN IF EXISTS `metricValue` `metric_value` DOUBLE NOT NULL DEFAULT 0;

-- Add a description to the table
ALTER TABLE `custommetric` 
COMMENT = 'Stores custom metrics for scrapers with name-value pairs';
