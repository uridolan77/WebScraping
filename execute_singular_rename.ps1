# PowerShell script to execute the table renaming SQL script with singular naming
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "rename_tables_singular.sql"
)

Write-Host "============================================"
Write-Host "  Database Table Name Converter"
Write-Host "  Converting to singular camelCase names"
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
    
    # Execute SHOW TABLES before renaming
    Write-Host ""
    Write-Host "Current tables in database before renaming:"
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SHOW TABLES;"
    
    # Execute the rename script
    Write-Host ""
    Write-Host "Executing table rename script to use singular camelCase names..."
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Table renaming completed successfully!"
        
        # Show tables after renaming
        Write-Host ""
        Write-Host "Tables in database after renaming:"
        & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SHOW TABLES;"
        
        Write-Host ""
        Write-Host "============================================"
        Write-Host "             NEXT STEPS"
        Write-Host "============================================"
        Write-Host "1. Now we need to update WebScraperDbContext.cs"
        Write-Host "   to use the new singular table names."
        Write-Host ""
        Write-Host "2. Restart your application to apply changes."
        Write-Host "============================================"
    }
    else {
        Write-Host "ERROR: Failed to execute the rename script."
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_"
    exit 1
}