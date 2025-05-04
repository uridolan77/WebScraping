# PowerShell script to run the database fix SQL script

param (
    [string]$mysqlUser = "root",
    [string]$mysqlPassword = "Dt%g_9W3z0*!I",
    [string]$mysqlHost = "localhost",
    [string]$database = "webstraction_db",
    [string]$sqlScriptPath = "fix_database_tables.sql"
)

# Check if the SQL file exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Error "SQL script file not found: $sqlScriptPath"
    exit 1
}

# Function to execute MySQL commands
function Invoke-MySQLCommand {
    param (
        [string]$command,
        [string]$description
    )

    Write-Host "Executing: $description" -ForegroundColor Cyan

    try {
        $result = Invoke-Expression "mysql -u`"$mysqlUser`" -p`"$mysqlPassword`" -h`"$mysqlHost`" -D`"$database`" -e `"$command`""
        Write-Host "Command executed successfully" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Error "Error executing MySQL command: $_"
        return $false
    }
}

# Check if MySQL is available
$testConnection = Invoke-MySQLCommand -command "SELECT 1;" -description "Testing MySQL connection"
if (-not $testConnection) {
    Write-Error "Could not connect to MySQL. Make sure MySQL is installed and running."
    exit 1
}

# Execute the SQL script to fix the database
Write-Host "Running database fix script..." -ForegroundColor Cyan
try {
    $sqlContent = Get-Content -Path $sqlScriptPath -Raw
    $command = "mysql -u`"$mysqlUser`" -p`"$mysqlPassword`" -h`"$mysqlHost`" -D`"$database`" -e `"$sqlContent`""
    $output = Invoke-Expression $command
    Write-Host "Database fix script executed successfully!" -ForegroundColor Green
}
catch {
    Write-Error "Error running database fix script: $_"
    exit 1
}

# Check the tables after the fix
Write-Host "Checking database tables after fix..." -ForegroundColor Cyan
$tables = @("scrapedpage", "scraper_status", "scrapermetric", "scraperlog")
foreach ($table in $tables) {
    $checkTable = Invoke-MySQLCommand -command "SHOW TABLES LIKE '$table';" -description "Checking if table $table exists"
    if ($checkTable) {
        $describeTable = Invoke-MySQLCommand -command "DESCRIBE $table;" -description "Describing table $table"
        if ($describeTable) {
            Write-Host "Table $table exists and is properly configured" -ForegroundColor Green
        }
    }
    else {
        Write-Host "Table $table does not exist after fix script" -ForegroundColor Red
    }
}

Write-Host "Database fix completed" -ForegroundColor Green
