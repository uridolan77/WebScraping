const mysql = require('mysql2/promise');

async function fixScraperStatus() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Get all scraper IDs from scraper_config
    const [scrapers] = await connection.execute('SELECT id, name FROM scraper_config');
    console.log(`Found ${scrapers.length} scrapers in scraper_config table`);
    
    // Get all scraper IDs from scraper_status
    const [statuses] = await connection.execute('SELECT scraper_id FROM scraper_status');
    console.log(`Found ${statuses.length} records in scraper_status table`);
    
    // Find scrapers without status
    const statusIds = statuses.map(s => s.scraper_id);
    const missingStatuses = scrapers.filter(s => !statusIds.includes(s.id));
    
    if (missingStatuses.length === 0) {
      console.log('All scrapers have status records. No fix needed.');
    } else {
      console.log(`Found ${missingStatuses.length} scrapers without status records:`);
      
      // Add missing status records
      for (const scraper of missingStatuses) {
        console.log(`- Adding status record for scraper ${scraper.id} (${scraper.name})`);
        
        const now = new Date().toISOString().slice(0, 19).replace('T', ' ');
        const query = `
          INSERT INTO scraper_status 
          (scraper_id, is_running, urls_processed, urls_queued, documents_processed, has_errors, message, last_update) 
          VALUES (?, 0, 0, 0, 0, 0, 'Ready to run', ?)
        `;
        
        const [result] = await connection.execute(query, [scraper.id, now]);
        console.log(`  Status record created successfully! Affected rows: ${result.affectedRows}`);
      }
    }
    
    // Verify all scrapers now have status records
    const [updatedStatuses] = await connection.execute('SELECT scraper_id FROM scraper_status');
    console.log(`\nAfter fix: Found ${updatedStatuses.length} records in scraper_status table`);
    
    await connection.end();
    console.log('\nConnection closed.');
    
    return true;
  } catch (error) {
    console.error('Error:', error);
    return false;
  }
}

fixScraperStatus();
