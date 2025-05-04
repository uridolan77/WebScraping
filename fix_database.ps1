param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "check_and_create_scraper.sql"
)

# Check if the script file exists
if (-not (Test-Path $scriptPath)) {
    Write-Error "SQL script file not found: $scriptPath"
    exit 1
}

# Read the SQL script content
$sqlScript = Get-Content -Path $scriptPath -Raw

Write-Host "Executing SQL script to fix database issues..."
Write-Host "1. Creating scrapermetric table if it doesn't exist"
Write-Host "2. Checking for scraper with ID 70344351-c33f-4d0c-8f27-dd478ff257da"
Write-Host "3. Creating the scraper if it doesn't exist"

# Create the MySQL command
$mysqlCmd = "mysql -h $server -u $username -p`"$password`" $database -e `"$sqlScript`""

try {
    # Execute the command
    Invoke-Expression $mysqlCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database fixes applied successfully!" -ForegroundColor Green
    } else {
        Write-Error "Failed to apply database fixes. MySQL returned exit code: $LASTEXITCODE"
    }
} catch {
    Write-Error "An error occurred: $_"
}
