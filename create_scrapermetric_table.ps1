param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "create_scrapermetric_table.sql"
)

# Check if the script file exists
if (-not (Test-Path $scriptPath)) {
    Write-Error "SQL script file not found: $scriptPath"
    exit 1
}

# Read the SQL script content
$sqlScript = Get-Content -Path $scriptPath -Raw

# Create the MySQL command
$mysqlCmd = "mysql -h $server -u $username -p`"$password`" $database -e `"$sqlScript`""

Write-Host "Executing SQL script to create the scrapermetric table..."

try {
    # Execute the command
    Invoke-Expression $mysqlCmd
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Table 'scrapermetric' created successfully!" -ForegroundColor Green
    } else {
        Write-Error "Failed to create table. MySQL returned exit code: $LASTEXITCODE"
    }
} catch {
    Write-Error "An error occurred: $_"
}
