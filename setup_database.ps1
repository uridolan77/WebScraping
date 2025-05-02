# PowerShell script to set up the webstraction_db database
# Make sure MySQL is installed and running

param (
    [string]$mysqlUser = "root",
    [string]$mysqlPassword = "",
    [string]$mysqlHost = "localhost",
    [string]$sqlScriptPath = "create_webstraction_db.sql",
    [string]$mysqlPath = "", # Custom path to MySQL executable
    [switch]$generateSqlOnly = $false # Just generate SQL output without executing
)

# Check if the SQL file exists
if (-not (Test-Path $sqlScriptPath)) {
    Write-Error "SQL script file not found: $sqlScriptPath"
    exit 1
}

# Function to find MySQL executable
function Find-MySQLExecutable {
    # Check custom path provided
    if ($mysqlPath -ne "" -and (Test-Path $mysqlPath)) {
        return $mysqlPath
    }
    
    # Try to find in PATH
    try {
        $mysqlInPath = (Get-Command "mysql" -ErrorAction SilentlyContinue).Source
        if ($mysqlInPath) {
            return $mysqlInPath
        }
    } catch {
        # MySQL not in PATH
    }
    
    # Check common installation paths for MySQL
    $commonPaths = @(
        "C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe",
        "C:\Program Files\MySQL\MySQL Server 5.7\bin\mysql.exe",
        "C:\Program Files (x86)\MySQL\MySQL Server 8.0\bin\mysql.exe",
        "C:\Program Files (x86)\MySQL\MySQL Server 5.7\bin\mysql.exe",
        "C:\xampp\mysql\bin\mysql.exe",
        "C:\wamp64\bin\mysql\mysql8.0.21\bin\mysql.exe",
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

# Execute the SQL script to create the database
Write-Host "Creating webstraction_db database..." -ForegroundColor Cyan
$setupDatabase = Invoke-Expression "mysql -u`"$mysqlUser`" -p`"$mysqlPassword`" -h`"$mysqlHost`" < `"$sqlScriptPath`"" 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to set up database: $setupDatabase"
    exit 1
}

Write-Host "Database 'webstraction_db' created successfully!" -ForegroundColor Green
Write-Host "Connection string for your app: 'Server=$mysqlHost;Database=webstraction_db;User=$mysqlUser;Password=$mysqlPassword;'" -ForegroundColor Cyan
