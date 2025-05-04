-- Check for debug logs related to saving to scrapedpage table
SELECT * FROM scraperlog 
WHERE message LIKE '%DEBUG:%' 
   OR message LIKE '%CRITICAL DEBUG:%'
   OR message LIKE '%SUCCESS: Saved to scrapedpage%'
   OR message LIKE '%ERROR: Failed to save to scrapedpage%'
ORDER BY timestamp DESC 
LIMIT 20;
