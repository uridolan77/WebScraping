const mysql = require('mysql2/promise');

async function checkScraperStatusRecords() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Check scraper_status records
    console.log('Checking scraper_status records:');
    const [records] = await connection.execute('SELECT * FROM scraper_status');
    
    if (records.length === 0) {
      console.log('No records found in scraper_status table');
    } else {
      console.log(`Found ${records.length} records in scraper_status table:`);
      records.forEach(record => {
        console.log(`- Scraper ID: ${record.scraper_id}, Running: ${record.is_running}, Message: ${record.message}`);
      });
    }
    
    // Check scraper_config records
    console.log('\nChecking scraper_config records:');
    const [scrapers] = await connection.execute('SELECT id, name, start_url FROM scraper_config');
    
    if (scrapers.length === 0) {
      console.log('No records found in scraper_config table');
    } else {
      console.log(`Found ${scrapers.length} records in scraper_config table:`);
      scrapers.forEach(scraper => {
        console.log(`- ID: ${scraper.id}, Name: ${scraper.name}, URL: ${scraper.start_url}`);
      });
    }
    
    await connection.end();
    console.log('\nConnection closed.');
    
    return true;
  } catch (error) {
    console.error('Error:', error);
    return false;
  }
}

checkScraperStatusRecords();
