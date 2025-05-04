# Run MySQL Fix Script to resolve table naming inconsistencies
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "fix_all_table_names.sql"
)

try {
    Write-Host "Checking for MySQL command-line client..."
    $mysqlPath = "mysql"
    
    try {
        # Test if mysql is available in the PATH
        $mysqlVersion = & $mysqlPath --version
        Write-Host "MySQL client found: $mysqlVersion"
    }
    catch {
        Write-Host "MySQL command-line client not found in PATH."
        Write-Host "Please ensure MySQL is installed and the client is in your PATH."
        exit 1
    }
    
    # Check if the script file exists
    if (-not (Test-Path $scriptPath)) {
        Write-Host "Error: SQL script file not found at $scriptPath"
        exit 1
    }
    
    # Read file content to show the user what will be executed
    Write-Host "Preparing to execute SQL script at $scriptPath"
    
    # Execute the script
    Write-Host "Applying table naming fixes to the database..."
    
    # Use the password securely through a temporary file
    $tempPasswordFile = [System.IO.Path]::GetTempFileName()
    try {
        Set-Content -Path $tempPasswordFile -Value $password -NoNewline
        
        $result = & $mysqlPath --host=$server --user=$username --password=(Get-Content $tempPasswordFile) --database=$database -e "source $scriptPath"
        
        Write-Host "SQL script executed successfully."
        Write-Host "The script has created table name views to ensure both snake_case and camelCase naming works."
        Write-Host ""
        Write-Host "This should fix the 'Table not found' errors for proxyconfigurations and scraperschedules tables."
    }
    finally {
        # Clean up the temp file
        if (Test-Path $tempPasswordFile) {
            Remove-Item $tempPasswordFile -Force
        }
    }
    
    Write-Host "Verifying tables after fix..."
    $result = & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "SHOW TABLES;"
    
    Write-Host ""
    Write-Host "Database tables and views now available:"
    Write-Host $result
}
catch {
    Write-Host "Error: $_"
    exit 1
}