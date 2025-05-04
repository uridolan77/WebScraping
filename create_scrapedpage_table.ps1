# PowerShell script to create the missing scrapedpage table
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "create_scrapedpage_table.sql"
)

Write-Host "============================================"
Write-Host "  Creating Missing scrapedpage Table"
Write-Host "============================================"
Write-Host ""

try {
    # Verify MySQL client is available
    Write-Host "Checking for MySQL command-line client..."
    $mysqlPath = "mysql"
    
    try {
        $mysqlVersion = & $mysqlPath --version
        Write-Host "MySQL client found: $mysqlVersion"
    }
    catch {
        Write-Host "ERROR: MySQL command-line client not found in PATH."
        Write-Host "Please ensure MySQL is installed and the client is in your PATH."
        exit 1
    }
    
    # Check if the script file exists
    if (-not (Test-Path $scriptPath)) {
        Write-Host "ERROR: SQL script file not found at $scriptPath"
        exit 1
    }
    
    # Execute SHOW TABLES before creating the table
    Write-Host ""
    Write-Host "Checking if scrapedpage table already exists:"
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SHOW TABLES LIKE 'scrapedpage';"
    
    # Execute the table creation script
    Write-Host ""
    Write-Host "Creating scrapedpage table..."
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Table creation completed successfully!"
        
        # Show tables after creation
        Write-Host ""
        Write-Host "Verifying scrapedpage table exists:"
        & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SHOW TABLES LIKE 'scrapedpage';"
        
        # Show table structure
        Write-Host ""
        Write-Host "Structure of the created scrapedpage table:"
        & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "DESCRIBE scrapedpage;"
    }
    else {
        Write-Host "ERROR: Failed to execute the table creation script."
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_"
    exit 1
}