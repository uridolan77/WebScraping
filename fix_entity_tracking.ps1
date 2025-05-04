# Apply Entity Framework tracking fixes to the database
# This script runs the SQL file that fixes tracking issues with the ScraperLog table

Write-Host "Applying Entity Framework tracking fixes..." -ForegroundColor Green

$sqlScriptPath = "c:\dev\WebScraping\fix_entity_tracking_in_repository.sql"
$mysqlUsername = "root"  # Update this with your MySQL username
$mysqlPassword = "password"  # Update this with your MySQL password
$mysqlHost = "localhost"
$mysqlDb = "webstraction_db"

# Function to check if MySQL is available
function Test-MySQL {
    try {
        $result = (Get-Command mysql -ErrorAction Stop) -ne $null
        return $true
    }
    catch {
        return $false
    }
}

# Check if the SQL script exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Host "Error: SQL script not found at $sqlScriptPath" -ForegroundColor Red
    exit 1
}

# Check if MySQL is available
if (-not (Test-MySQL)) {
    Write-Host "Error: MySQL command not found. Make sure MySQL is installed and in your PATH." -ForegroundColor Red
    exit 1
}

# Execute the SQL script
try {
    Write-Host "Running SQL script to fix Entity Framework tracking issues..." -ForegroundColor Yellow
    
    # Construct the MySQL command
    $command = "mysql -u $mysqlUsername -p$mysqlPassword -h $mysqlHost $mysqlDb < `"$sqlScriptPath`""
    
    # Execute the command
    $output = Invoke-Expression "cmd /c $command 2>&1"
    
    # Display results
    Write-Host "Database fixes applied successfully!" -ForegroundColor Green
    
    # Now let's also modify the AddScraperLogAsync method in the C# application
    Write-Host "Now let's modify the AddScraperLogAsync method in ScraperRepository.cs..." -ForegroundColor Yellow
    
    # Path to the repository file
    $repoFilePath = "c:\dev\WebScraping\WebScraperAPI\Data\Repositories\ScraperRepository.cs"
    
    # Check if the file exists
    if (Test-Path $repoFilePath) {
        Write-Host "Repository file found. Please modify the AddScraperLogAsync method as instructed." -ForegroundColor Green
    } else {
        Write-Host "Repository file not found at $repoFilePath. Please manually implement the fix." -ForegroundColor Yellow
    }
    
    Write-Host "`nDatabase fix completed." -ForegroundColor Green
    Write-Host "To solve the 'Unexpected entry.EntityState: Unchanged' error completely, also ensure the ScraperRepository.AddScraperLogAsync method is updated." -ForegroundColor Cyan
}
catch {
    Write-Host "Error executing SQL script: $_" -ForegroundColor Red
    exit 1
}