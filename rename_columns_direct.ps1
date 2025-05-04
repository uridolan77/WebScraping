# PowerShell script to execute the direct column renaming script
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "rename_columns_direct.sql"
)

Write-Host "============================================"
Write-Host "  Direct Column Renaming to camelCase"
Write-Host "  (Working around MySQL limitations)"
Write-Host "============================================"
Write-Host ""

try {
    # Test MySQL connection with simple command
    Write-Host "Testing MySQL connection..."
    $mysqlCmd = "mysql"
    $mysqlTestArgs = "--host=$server", "--user=$username", "--password=$password", "--execute=SELECT 'Connection successful'"
    
    & $mysqlCmd $mysqlTestArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERROR: Could not connect to MySQL. Please check your credentials."
        exit 1
    }
    
    # Check if script exists
    if (-not (Test-Path $scriptPath)) {
        Write-Host "ERROR: SQL script not found at: $scriptPath"
        exit 1
    }
    
    # Execute the script
    Write-Host ""
    Write-Host "Executing direct column renaming script..."
    Write-Host "This will rename all snake_case columns to camelCase across all tables."
    Write-Host ""
    
    & $mysqlCmd "--host=$server" "--user=$username" "--password=$password" "--database=$database" "--execute=source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Column renaming completed successfully!"
        Write-Host ""
        
        # Check if any snake_case columns remain
        Write-Host "Performing final verification of database columns..."
        & $mysqlCmd "--host=$server" "--user=$username" "--password=$password" "--database=$database" "--execute=SELECT TABLE_NAME, COUNT(*) AS snake_case_columns FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'webstraction_db' AND COLUMN_NAME LIKE '%\_%' GROUP BY TABLE_NAME ORDER BY TABLE_NAME"
        
        Write-Host ""
        Write-Host "============================================"
        Write-Host "             NEXT STEPS"
        Write-Host "============================================"
        Write-Host "1. Restart your application to apply the changes."
        Write-Host ""
        Write-Host "2. All 'Unknown column' errors should now be resolved"
        Write-Host "   as all columns match Entity Framework Core's camelCase convention."
        Write-Host "============================================"
    }
    else {
        Write-Host "ERROR: Failed to execute the SQL script."
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_"
    exit 1
}