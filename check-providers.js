// JavaScript to check providers in the database
// Check if providers table exists and has data
const mysql = require('mysql2/promise');

async function checkProviders() {
    const connection = await mysql.createConnection({
        host: 'localhost',
        user: 'root',
        password: 'Dt%g_9W3z0*!I',
        database: 'webstraction_db'
    });

    try {
        console.log('Checking database for provider-related tables...');
        
        // Check if provider tables exist
        const [tables] = await connection.execute(`
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = 'webstraction_db' 
            AND TABLE_NAME LIKE '%provider%'
        `);
        
        console.log('Provider-related tables:', tables.map(t => t.TABLE_NAME));
        
        // Try to find alternative tables that might contain provider data
        const [potentialTables] = await connection.execute(`
            SELECT TABLE_NAME 
            FROM INFORMATION_SCHEMA.TABLES 
            WHERE TABLE_SCHEMA = 'webstraction_db'
        `);
        
        console.log('\nAll tables in database:');
        potentialTables.forEach(t => console.log(t.TABLE_NAME));
        
        // Check proxyconfiguration table which might contain provider info
        console.log('\nChecking proxyconfiguration table...');
        try {
            const [proxyColumns] = await connection.execute(`DESCRIBE proxyconfiguration`);
            console.log('Columns in proxyconfiguration:', proxyColumns.map(c => c.Field));
            
            const [proxyCount] = await connection.execute(`SELECT COUNT(*) as count FROM proxyconfiguration`);
            console.log('Number of records:', proxyCount[0].count);
            
            if (proxyCount[0].count > 0) {
                const [proxyData] = await connection.execute(`SELECT * FROM proxyconfiguration LIMIT 5`);
                console.log('Sample proxy data:', JSON.stringify(proxyData, null, 2));
            }
        } catch (err) {
            console.log('Error querying proxyconfiguration:', err.message);
        }
        
        // Check scraperconfig table for provider-related columns
        console.log('\nChecking scraperconfig table for provider-related columns...');
        try {
            const [scraperConfigColumns] = await connection.execute(`DESCRIBE scraperconfig`);
            const providerColumns = scraperConfigColumns
                .filter(c => c.Field.toLowerCase().includes('provider') || c.Field.toLowerCase().includes('proxy'))
                .map(c => c.Field);
            
            console.log('Provider-related columns in scraperconfig:', providerColumns);
            
            if (providerColumns.length > 0) {
                const columnsStr = providerColumns.join(', ');
                const [providerData] = await connection.execute(`SELECT id, name, ${columnsStr} FROM scraperconfig LIMIT 5`);
                console.log('Sample provider data from scraperconfig:', JSON.stringify(providerData, null, 2));
            }
        } catch (err) {
            console.log('Error querying scraperconfig:', err.message);
        }
        
    } catch (err) {
        console.error('Error checking providers:', err);
    } finally {
        await connection.end();
        console.log('Database connection closed');
    }
}

checkProviders().catch(console.error);