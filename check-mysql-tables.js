const mysql = require('mysql2/promise');

async function checkTables() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Get list of tables
    const [tables] = await connection.execute('SHOW TABLES');
    console.log('Tables in database:');
    tables.forEach(table => {
      const tableName = Object.values(table)[0];
      console.log(`- ${tableName}`);
    });
    
    // Check if scraper_config table exists
    if (tables.some(table => Object.values(table)[0] === 'scraper_config')) {
      console.log('\nChecking scraper_config table structure:');
      const [columns] = await connection.execute('DESCRIBE scraper_config');
      columns.forEach(column => {
        console.log(`- ${column.Field}: ${column.Type} ${column.Null === 'YES' ? '(nullable)' : '(not null)'}`);
      });
      
      // Check if there are any records in the scraper_config table
      const [count] = await connection.execute('SELECT COUNT(*) as count FROM scraper_config');
      console.log(`\nNumber of records in scraper_config table: ${count[0].count}`);
      
      if (count[0].count > 0) {
        const [scrapers] = await connection.execute('SELECT id, name, start_url FROM scraper_config LIMIT 5');
        console.log('\nSample scrapers:');
        scrapers.forEach(scraper => {
          console.log(`- ${scraper.id}: ${scraper.name} (${scraper.start_url})`);
        });
      }
    }
    
    await connection.end();
    console.log('\nConnection closed.');
    
    return true;
  } catch (error) {
    console.error('Error:', error);
    return false;
  }
}

checkTables()
  .then(success => {
    console.log('Check completed. Success:', success);
    process.exit(success ? 0 : 1);
  })
  .catch(err => {
    console.error('Unexpected error:', err);
    process.exit(1);
  });
