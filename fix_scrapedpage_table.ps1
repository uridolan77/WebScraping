# PowerShell script to apply the SQL fixes for the ScrapedPage table
# This resolves the "Collection was modified" and "Unexpected entry.EntityState: Unchanged" errors

Write-Host "Starting database table fix for ScrapedPage..." -ForegroundColor Green

# MySQL connection parameters - you should adjust these as needed
$MySQLUser = "root"
$MySQLPassword = "password"  # Use your actual MySQL password
$MySQLHost = "localhost"
$MySQLPort = "3306"
$MySQLDatabase = "webstraction_db"

# Path to the SQL script we want to execute
$SqlScriptPath = "c:\dev\WebScraping\fix_scrapedpage_saving.sql"

# Check if the SQL script exists
if (-not (Test-Path $SqlScriptPath)) {
    Write-Host "Error: SQL script not found at $SqlScriptPath" -ForegroundColor Red
    exit 1
}

# Confirm that mysql.exe is available on the path
try {
    $mysqlPath = Get-Command mysql -ErrorAction Stop
    Write-Host "Found MySQL at: $($mysqlPath.Source)" -ForegroundColor Green
} catch {
    Write-Host "Error: MySQL command not found in PATH. Please ensure MySQL is installed and in your PATH." -ForegroundColor Red
    exit 1
}

# Execute the SQL script
try {
    Write-Host "Executing SQL script to fix ScrapedPage table..." -ForegroundColor Yellow
    
    # Build the mysql command
    $command = "mysql -u$MySQLUser -p$MySQLPassword -h$MySQLHost -P$MySQLPort $MySQLDatabase < `"$SqlScriptPath`""
    
    # Execute the command using Invoke-Expression
    Invoke-Expression "cmd /c $command"
    
    # Check if the command executed successfully
    if ($LASTEXITCODE -eq 0) {
        Write-Host "SQL script executed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Error executing SQL script. Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error executing SQL script: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nDatabase fix completed. The ScrapedPage table has been optimized." -ForegroundColor Green
Write-Host "This should resolve the entity tracking and collection modification errors." -ForegroundColor Green
Write-Host "`nYou can now restart your WebScraper application and test again." -ForegroundColor Cyan