# PowerShell script to execute the SQL script for converting all field names to camelCase
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "convert_all_fields_to_camelcase.sql"
)

Write-Host "============================================"
Write-Host "  Converting ALL Database Fields to camelCase"
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
    
    # Show a sample of current column format before the changes
    Write-Host ""
    Write-Host "Sample of current fields (before changes):"
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'webstraction_db' AND COLUMN_NAME LIKE '%\_%' LIMIT 10;"
    
    # Execute the script
    Write-Host ""
    Write-Host "Converting all field names from snake_case to camelCase format..."
    Write-Host "This may take a while depending on the size of your database."
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Field renaming completed successfully!"
        
        # Show a sample of the renamed columns
        Write-Host ""
        Write-Host "Sample of fields after conversion (should be in camelCase):"
        & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'webstraction_db' LIMIT 10;"
        
        Write-Host ""
        Write-Host "============================================"
        Write-Host "             NEXT STEPS"
        Write-Host "============================================"
        Write-Host "1. Restart your application to apply the changes."
        Write-Host ""
        Write-Host "2. If your application generates SQL queries,"
        Write-Host "   make sure to update them to use the new column names."
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