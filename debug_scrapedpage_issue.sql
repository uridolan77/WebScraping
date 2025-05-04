-- Check if there are any errors in the log related to saving to the database
SELECT * FROM scraperlog 
WHERE message LIKE '%CRITICAL DEBUG: About to save content to database%' 
   OR message LIKE '%SUCCESSFULLY saved content to database%'
   OR message LIKE '%Failed to%database%'
ORDER BY timestamp DESC 
LIMIT 20;

-- Check if there are any constraints that might be preventing inserts
SHOW CREATE TABLE scrapedpage;

-- Create a test record in the scrapedpage table to verify we can insert
INSERT INTO scrapedpage (scraperId, url, htmlContent, textContent, scrapedAt)
VALUES ('70344351-c33f-4d0c-8f27-dd478ff257da', 'https://test.com', 'Test HTML', 'Test Text', NOW());

-- Verify the test record was inserted
SELECT * FROM scrapedpage WHERE url = 'https://test.com';

-- Reset the scraper status to not running so we can restart it
UPDATE scraperstatus 
SET isRunning = 0, 
    endTime = NOW(), 
    message = 'Scraper stopped for debugging'
WHERE scraperId = '70344351-c33f-4d0c-8f27-dd478ff257da';
