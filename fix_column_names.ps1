# PowerShell script to execute the column renaming SQL script
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "fix_column_names.sql"
)

Write-Host "============================================"
Write-Host "  Converting Column Names to camelCase"
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
    
    # Show column format before the changes
    Write-Host ""
    Write-Host "Current column format in the scraperconfig table (before changes):"
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "DESCRIBE scraperconfig;"
    
    # Execute the script
    Write-Host ""
    Write-Host "Renaming all columns from snake_case to camelCase format..."
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Column renaming completed successfully!"
        
        Write-Host ""
        Write-Host "============================================"
        Write-Host "             NEXT STEPS"
        Write-Host "============================================"
        Write-Host "1. Restart your application to apply the changes."
        Write-Host ""
        Write-Host "2. If you encounter any other issues, additional"
        Write-Host "   columns might need to be renamed using the same approach."
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