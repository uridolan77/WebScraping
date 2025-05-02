const mysql = require('mysql2/promise');

async function checkScraperStatusTable() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Check scraper_status table structure
    console.log('Checking scraper_status table structure:');
    const [columns] = await connection.execute('DESCRIBE scraper_status');
    columns.forEach(column => {
      console.log(`- ${column.Field}: ${column.Type} ${column.Null === 'YES' ? '(nullable)' : '(not null)'}`);
    });
    
    await connection.end();
    console.log('\nConnection closed.');
    
    return true;
  } catch (error) {
    console.error('Error:', error);
    return false;
  }
}

checkScraperStatusTable()
  .then(success => {
    console.log('Check completed. Success:', success);
    process.exit(success ? 0 : 1);
  })
  .catch(err => {
    console.error('Unexpected error:', err);
    process.exit(1);
  });
