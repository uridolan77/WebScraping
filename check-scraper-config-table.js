const mysql = require('mysql2/promise');

async function checkScraperConfigTable() {
  let connection;
  try {
    // Create connection with updated parameters
    connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db',
      allowPublicKeyRetrieval: true
    });
    
    console.log('Connected to MySQL database!');
    
    // Check if scraper_config table exists
    const [tables] = await connection.execute("SHOW TABLES LIKE 'scraper_config'");
    if (tables.length === 0) {
      console.log('The scraper_config table does not exist in the database.');
      return;
    }
    
    // Get record count from scraper_config table
    const [countResult] = await connection.execute('SELECT COUNT(*) as count FROM scraper_config');
    const recordCount = countResult[0].count;
    console.log(`The scraper_config table has ${recordCount} records.`);
    
    // If records exist, show the first few
    if (recordCount > 0) {
      const [records] = await connection.execute('SELECT id, name, start_url FROM scraper_config LIMIT 5');
      console.log('Sample records:');
      records.forEach(record => {
        console.log(`ID: ${record.id}, Name: ${record.name}, Start URL: ${record.start_url}`);
      });
    }
    
  } catch (error) {
    console.error('Error connecting to MySQL database:', error.message);
    if (error.code === 'ER_ACCESS_DENIED_ERROR') {
      console.error('Access denied - check your username and password');
    } else if (error.code === 'ECONNREFUSED') {
      console.error('Connection refused - make sure MySQL server is running');
    }
  } finally {
    if (connection) {
      await connection.end();
      console.log('MySQL connection closed');
    }
  }
}

checkScraperConfigTable();
