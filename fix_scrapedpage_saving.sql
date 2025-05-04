-- Check if there's any data in the scrapedpage table
SELECT COUNT(*) AS record_count FROM scrapedpage;

-- Check the structure of the scrapedpage table
DESCRIBE scrapedpage;

-- Insert a test record to verify we can insert data
INSERT INTO scrapedpage (scraperId, url, htmlContent, textContent, scrapedAt)
VALUES ('70344351-c33f-4d0c-8f27-dd478ff257da', 'https://test-fix.com', 'Test HTML Content', 'Test Text Content', NOW());

-- Verify the test record was inserted
SELECT * FROM scrapedpage WHERE url = 'https://test-fix.com';

-- Reset the scraper status to not running
UPDATE scraperstatus 
SET isRunning = 0, 
    endTime = NOW(), 
    message = 'Scraper stopped for debugging'
WHERE scraperId = '70344351-c33f-4d0c-8f27-dd478ff257da';
