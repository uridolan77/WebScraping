# PowerShell script to execute the comprehensive column renaming script for all tables
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "fix_all_columns_camelcase.sql"
)

Write-Host "============================================"
Write-Host "  Converting All Database Columns to camelCase"
Write-Host "============================================"
Write-Host ""

try {
    # Verify MySQL client is available
    Write-Host "Checking for MySQL command-line client..."
    
    try {
        $mysqlVersion = mysql --version
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
    
    # Display information about current database tables
    Write-Host ""
    Write-Host "Getting information about database tables before conversion..."
    mysql --host=$server --user=$username --password=$password --database=$database -e "
    SELECT table_name, COUNT(*) as columns_count FROM information_schema.columns 
    WHERE table_schema = 'webstraction_db' AND column_name LIKE '%\_%'
    GROUP BY table_name
    ORDER BY table_name;"
    
    # Execute the script
    Write-Host ""
    Write-Host "Executing comprehensive column renaming script for ALL tables..."
    Write-Host "This will convert ALL snake_case columns to camelCase across the database."
    Write-Host "Please be patient as this may take a while for large databases."
    Write-Host ""
    mysql --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    $exitCode = $LASTEXITCODE
    if ($exitCode -eq 0) {
        Write-Host ""
        Write-Host "Column renaming completed successfully!"
        
        # Verify no snake_case columns remain
        Write-Host ""
        Write-Host "Checking for any remaining snake_case columns:"
        mysql --host=$server --user=$username --password=$password --database=$database -e "
        SELECT table_name, COUNT(*) as snake_case_columns 
        FROM information_schema.columns 
        WHERE table_schema = 'webstraction_db' AND column_name LIKE '%\_%'
        GROUP BY table_name;"
        
        Write-Host ""
        Write-Host "============================================"
        Write-Host "             NEXT STEPS"
        Write-Host "============================================"
        Write-Host "1. Restart your application to apply the changes."
        Write-Host ""
        Write-Host "2. The 'Unknown column' errors should now be resolved"
        Write-Host "   as all columns match Entity Framework Core's camelCase naming convention."
        Write-Host "============================================"
    }
    else {
        Write-Host "ERROR: Failed to execute the SQL script. Exit code: $exitCode"
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_"
    exit 1
}