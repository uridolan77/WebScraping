const fs = require('fs');
const path = require('path');
const mysql = require('mysql2/promise');

// Function to convert snake_case to camelCase and PascalCase
function convertSnakeCaseToCamelCase(str) {
  return str.replace(/_([a-z])/g, (g) => g[1].toUpperCase());
}

function convertSnakeCaseToPascalCase(str) {
  const camelCase = convertSnakeCaseToCamelCase(str);
  return camelCase.charAt(0).toUpperCase() + camelCase.slice(1);
}

// Function to recursively search for a string in files
async function searchInFiles(directory, searchString, fileExtensions) {
  let results = [];
  
  try {
    const entries = fs.readdirSync(directory, { withFileTypes: true });
    
    for (const entry of entries) {
      const fullPath = path.join(directory, entry.name);
      
      if (entry.isDirectory() && !fullPath.includes('node_modules') && !fullPath.includes('bin') && !fullPath.includes('obj')) {
        // Recursively search subdirectories
        const subdirResults = await searchInFiles(fullPath, searchString, fileExtensions);
        results = results.concat(subdirResults);
      } 
      else if (entry.isFile() && fileExtensions.includes(path.extname(entry.name).toLowerCase())) {
        // Check if file content contains the search string
        const content = fs.readFileSync(fullPath, 'utf8');
        if (content.includes(searchString)) {
          results.push({
            file: fullPath,
            matches: content.split('\n')
                           .filter(line => line.includes(searchString))
                           .map(line => line.trim())
          });
        }
      }
    }
  } catch (error) {
    console.error(`Error searching ${directory}: ${error.message}`);
  }
  
  return results;
}

async function main() {
  let connection;
  try {
    // Create connection
    connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database!\n');
    
    // Get all table names
    const [tables] = await connection.execute('SHOW TABLES');
    
    console.log('Analyzing database table names and searching for inconsistencies:\n');
    
    // Search for each table with its camelCase and PascalCase versions
    for (const table of tables) {
      const tableName = Object.values(table)[0];
      
      // Skip the EF migrations history table
      if (tableName === '__efmigrationshistory') continue;
      
      const camelCase = convertSnakeCaseToCamelCase(tableName);
      const pascalCase = convertSnakeCaseToPascalCase(tableName);
      const singularPascalCase = pascalCase.endsWith('s') ? pascalCase.slice(0, -1) : pascalCase;
      const pluralPascalCase = pascalCase.endsWith('s') ? pascalCase : pascalCase + 's';
      
      console.log(`Table: ${tableName}`);
      console.log(`  - CamelCase: ${camelCase}`);
      console.log(`  - PascalCase: ${pascalCase}`);
      console.log(`  - SingularPascalCase: ${singularPascalCase}`);
      console.log(`  - PluralPascalCase: ${pluralPascalCase}`);
      
      // Search for the camelCase and PascalCase versions in .cs files
      console.log(`  Searching in code...`);
      
      const rootDir = 'c:\\dev\\WebScraping';
      const extensions = ['.cs', '.csproj', '.json'];
      
      // Search for all versions
      const camelResults = await searchInFiles(rootDir, camelCase, extensions);
      const pascalResults = await searchInFiles(rootDir, pascalCase, extensions);
      const singularResults = await searchInFiles(rootDir, singularPascalCase, extensions);
      const pluralResults = await searchInFiles(rootDir, pluralPascalCase, extensions);
      
      // Combine all results
      let allResults = [...camelResults, ...pascalResults, ...singularResults, ...pluralResults];
      
      // Remove duplicates (same file)
      const uniqueFiles = new Set();
      allResults = allResults.filter(result => {
        if (uniqueFiles.has(result.file)) {
          return false;
        }
        uniqueFiles.add(result.file);
        return true;
      });
      
      if (allResults.length > 0) {
        console.log(`  Found ${allResults.length} files with potential references:`);
        allResults.forEach(result => {
          console.log(`    - ${result.file}`);
          // Show a maximum of 3 matches per file to keep output manageable
          const maxMatches = 3;
          result.matches.slice(0, maxMatches).forEach(match => {
            console.log(`      ${match}`);
          });
          if (result.matches.length > maxMatches) {
            console.log(`      ... and ${result.matches.length - maxMatches} more matches`);
          }
        });
      } else {
        console.log(`  No potential naming inconsistencies found for this table.`);
      }
      
      console.log('');
    }
    
  } catch (error) {
    console.error('Error:', error.message);
  } finally {
    if (connection) {
      await connection.end();
      console.log('MySQL connection closed');
    }
  }
}

main();