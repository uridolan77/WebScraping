-- Script to add missing columns to the scrapermetric table
ALTER TABLE scrapermetric ADD COLUMN IF NOT EXISTS RunId VARCHAR(50);
ALTER TABLE scrapermetric ADD COLUMN IF NOT EXISTS scraperConfigId VARCHAR(50);
