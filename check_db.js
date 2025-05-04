const mysql = require('mysql2/promise');

async function checkDatabase() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Check if scrapedpage table exists
    const [tables] = await connection.execute("SHOW TABLES LIKE 'scrapedpage'");
    if (tables.length === 0) {
      console.log('scrapedpage table does not exist');
    } else {
      console.log('scrapedpage table exists');
      
      // Check structure
      const [columns] = await connection.execute('DESCRIBE scrapedpage');
      console.log('scrapedpage table structure:');
      columns.forEach(col => {
        console.log(`- ${col.Field}: ${col.Type} ${col.Null === 'YES' ? 'NULL' : 'NOT NULL'}`);
      });
      
      // Count records
      const [count] = await connection.execute('SELECT COUNT(*) as count FROM scrapedpage');
      console.log(`Number of records in scrapedpage: ${count[0].count}`);
      
      // Get sample records
      if (count[0].count > 0) {
        const [records] = await connection.execute('SELECT id, scraper_id, url, scraped_at FROM scrapedpage LIMIT 5');
        console.log('Sample records:');
        records.forEach(record => {
          console.log(`- ID: ${record.id}, ScraperID: ${record.scraper_id}, URL: ${record.url}, ScrapedAt: ${record.scraped_at}`);
        });
      }
    }
    
    // Check scraper_status table
    const [statusTables] = await connection.execute("SHOW TABLES LIKE 'scraper_status'");
    if (statusTables.length === 0) {
      console.log('scraper_status table does not exist');
    } else {
      console.log('scraper_status table exists');
      
      // Check structure
      const [statusColumns] = await connection.execute('DESCRIBE scraper_status');
      console.log('scraper_status table structure:');
      statusColumns.forEach(col => {
        console.log(`- ${col.Field}: ${col.Type} ${col.Null === 'YES' ? 'NULL' : 'NOT NULL'}`);
      });
      
      // Get records
      const [statusRecords] = await connection.execute('SELECT * FROM scraper_status');
      console.log(`Number of records in scraper_status: ${statusRecords.length}`);
      statusRecords.forEach(record => {
        console.log(`- ScraperID: ${record.scraper_id}, IsRunning: ${record.is_running}, URLs Processed: ${record.urls_processed}`);
      });
    }
    
    // Check scrapermetric table
    const [metricTables] = await connection.execute("SHOW TABLES LIKE 'scrapermetric'");
    if (metricTables.length === 0) {
      console.log('scrapermetric table does not exist');
    } else {
      console.log('scrapermetric table exists');
      
      // Check structure
      const [metricColumns] = await connection.execute('DESCRIBE scrapermetric');
      console.log('scrapermetric table structure:');
      metricColumns.forEach(col => {
        console.log(`- ${col.Field}: ${col.Type} ${col.Null === 'YES' ? 'NULL' : 'NOT NULL'}`);
      });
      
      // Count records
      const [metricCount] = await connection.execute('SELECT COUNT(*) as count FROM scrapermetric');
      console.log(`Number of records in scrapermetric: ${metricCount[0].count}`);
    }
    
    await connection.end();
    console.log('Connection closed.');
    
  } catch (error) {
    console.error('Error:', error);
  }
}

checkDatabase();
