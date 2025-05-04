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
    
    // Get list of all tables
    const [tables] = await connection.execute('SHOW TABLES');
    console.log('Tables in database:');
    tables.forEach(table => {
      const tableName = Object.values(table)[0];
      console.log(`- ${tableName}`);
    });
    
    // Check for specific tables that might be named differently
    const tableChecks = [
      'scraperstatus',
      'scraperstatuses',
      'scraper_status',
      'scraper_statuses'
    ];
    
    for (const tableName of tableChecks) {
      const [result] = await connection.execute(`SHOW TABLES LIKE '${tableName}'`);
      if (result.length > 0) {
        console.log(`\nFound table: ${tableName}`);
        const [columns] = await connection.execute(`DESCRIBE ${tableName}`);
        console.log(`Structure of ${tableName}:`);
        columns.forEach(col => {
          console.log(`- ${col.Field}: ${col.Type} ${col.Null === 'YES' ? 'NULL' : 'NOT NULL'}`);
        });
        
        const [count] = await connection.execute(`SELECT COUNT(*) as count FROM ${tableName}`);
        console.log(`Number of records in ${tableName}: ${count[0].count}`);
        
        if (count[0].count > 0) {
          const [records] = await connection.execute(`SELECT * FROM ${tableName} LIMIT 5`);
          console.log(`Sample records from ${tableName}:`);
          records.forEach(record => {
            console.log(record);
          });
        }
      }
    }
    
    await connection.end();
    console.log('\nConnection closed.');
    
  } catch (error) {
    console.error('Error:', error);
  }
}

checkTables();
