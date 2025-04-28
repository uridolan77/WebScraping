/**
 * Script to help convert JavaScript files to TypeScript
 * Usage: node scripts/convertToTypeScript.js [directory]
 */

const fs = require('fs');
const path = require('path');

// Default directory is src
const targetDir = process.argv[2] || 'src';
const rootDir = path.resolve(process.cwd(), targetDir);

// Extensions to convert
const jsExtensions = ['.js', '.jsx'];
const tsExtensions = ['.ts', '.tsx'];

// Count of files processed
let converted = 0;
let skipped = 0;

/**
 * Convert a file from JavaScript to TypeScript
 * @param {string} filePath - Path to the file
 */
function convertFile(filePath) {
  const ext = path.extname(filePath);
  
  // Skip if not a JavaScript file
  if (!jsExtensions.includes(ext)) {
    skipped++;
    return;
  }
  
  // Determine the new extension
  const newExt = ext === '.js' ? '.ts' : '.tsx';
  const newPath = filePath.replace(ext, newExt);
  
  // Skip if TypeScript file already exists
  if (fs.existsSync(newPath)) {
    console.log(`Skipping ${filePath} - ${newPath} already exists`);
    skipped++;
    return;
  }
  
  try {
    // Read the file content
    const content = fs.readFileSync(filePath, 'utf8');
    
    // Write the content to the new file
    fs.writeFileSync(newPath, content);
    
    console.log(`Converted ${filePath} to ${newPath}`);
    converted++;
  } catch (error) {
    console.error(`Error converting ${filePath}:`, error);
  }
}

/**
 * Process a directory recursively
 * @param {string} dirPath - Path to the directory
 */
function processDirectory(dirPath) {
  // Read directory contents
  const items = fs.readdirSync(dirPath);
  
  // Process each item
  for (const item of items) {
    const itemPath = path.join(dirPath, item);
    const stats = fs.statSync(itemPath);
    
    if (stats.isDirectory()) {
      // Skip node_modules and hidden directories
      if (item === 'node_modules' || item.startsWith('.')) {
        continue;
      }
      
      // Process subdirectory
      processDirectory(itemPath);
    } else if (stats.isFile()) {
      // Convert file
      convertFile(itemPath);
    }
  }
}

// Start processing
console.log(`Converting JavaScript files to TypeScript in ${rootDir}...`);
processDirectory(rootDir);
console.log(`Done! Converted: ${converted}, Skipped: ${skipped}`);
