const mysql = require('mysql2/promise');

async function checkRelatedTables() {
  let connection;
  try {
    // Create connection
    connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database!');
    
    // Check key related tables
    const tables = [
      'scraper_start_url',
      'content_extractor_selector',
      'keyword_alert',
      'webhook_trigger',
      'domain_rate_limit',
      'proxy_configuration',
      'scraper_schedule'
    ];
    
    for (const table of tables) {
      try {
        const [countResult] = await connection.execute(`SELECT COUNT(*) as count FROM ${table}`);
        const recordCount = countResult[0].count;
        console.log(`The ${table} table has ${recordCount} records.`);
      } catch (error) {
        console.log(`Error checking ${table} table: ${error.message}`);
      }
    }
    
  } catch (error) {
    console.error('Error connecting to MySQL database:', error.message);
  } finally {
    if (connection) {
      await connection.end();
      console.log('MySQL connection closed');
    }
  }
}

checkRelatedTables();