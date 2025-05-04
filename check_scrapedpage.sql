-- Check if the scrapedpage table exists
SHOW TABLES LIKE 'scrapedpage';

-- Check the structure of the scrapedpage table
DESCRIBE scrapedpage;

-- Count the number of records in the scrapedpage table
SELECT COUNT(*) FROM scrapedpage;

-- Check the scraper_status table
SHOW TABLES LIKE 'scraper_status';

-- Check the structure of the scraper_status table
DESCRIBE scraper_status;

-- Check records in the scraper_status table
SELECT * FROM scraper_status;

-- Check the scrapermetric table
SHOW TABLES LIKE 'scrapermetric';

-- Check the structure of the scrapermetric table
DESCRIBE scrapermetric;

-- Count the number of records in the scrapermetric table
SELECT COUNT(*) FROM scrapermetric;
