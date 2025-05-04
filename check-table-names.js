const mysql = require('mysql2/promise');

async function checkTableNames() {
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
    
    // Get all table names
    const [tables] = await connection.execute('SHOW TABLES');
    
    console.log('Tables in the database:');
    tables.forEach(table => {
      // The column name for the table name depends on how MySQL returns it
      const tableName = Object.values(table)[0];
      console.log(tableName);
    });
    
    // Specifically check for proxy configuration tables
    console.log('\nChecking for proxy configuration tables:');
    const [proxyTables] = await connection.execute("SHOW TABLES LIKE '%proxy%'");
    
    if (proxyTables.length === 0) {
      console.log('No tables with "proxy" in the name found');
    } else {
      proxyTables.forEach(table => {
        const tableName = Object.values(table)[0];
        console.log(`Found proxy-related table: ${tableName}`);
      });
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

checkTableNames();