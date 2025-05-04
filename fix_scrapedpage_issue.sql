-- Check if there's any data in the scrapedpage table
SELECT COUNT(*) AS record_count FROM scrapedpage;

-- Check the structure of the scrapedpage table
DESCRIBE scrapedpage;

-- Check if there are any records in the scraperlog table for comparison
SELECT COUNT(*) AS log_record_count FROM scraperlog;

-- Check the scraperlog table structure
DESCRIBE scraperlog;

-- Check the most recent log entries
SELECT * FROM scraperlog ORDER BY timestamp DESC LIMIT 5;

-- Check the scraperstatus table
SHOW TABLES LIKE 'scraperstatus';

-- Check the structure of the scraperstatus table
DESCRIBE scraperstatus;

-- Check if the scraper is marked as running
SELECT * FROM scraperstatus WHERE isRunning = 1;
