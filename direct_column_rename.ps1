# PowerShell script to directly execute the column renaming script
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "direct_column_rename.sql"
)

Write-Host "============================================"
Write-Host "  Direct Column Renaming to camelCase"
Write-Host "============================================"
Write-Host ""

try {
    # Use the MySQL command directly without && operator
    Write-Host "Checking for MySQL command-line client..."
    
    # Test MySQL connection
    $mysqlTest = Start-Process -FilePath "mysql" -ArgumentList "--host=$server --user=$username --password=$password --execute=`"SELECT 'MySQL connection successful!'`"" -NoNewWindow -Wait -PassThru
    
    if ($mysqlTest.ExitCode -ne 0) {
        Write-Host "ERROR: Could not connect to MySQL. Please check your credentials and connection."
        exit 1
    }
    
    Write-Host "MySQL connection successful!"
    
    # Check if the script file exists
    if (-not (Test-Path $scriptPath)) {
        Write-Host "ERROR: SQL script file not found at $scriptPath"
        exit 1
    }
    
    # Show current column format before changes
    Write-Host ""
    Write-Host "Current column format (before changes):"
    Start-Process -FilePath "mysql" -ArgumentList "--host=$server --user=$username --password=$password --database=$database --execute=`"DESCRIBE scraperconfig`"" -NoNewWindow -Wait
    
    # Execute the script
    Write-Host ""
    Write-Host "Executing direct column rename script..."
    Write-Host "This uses explicit column definitions to ensure proper type preservation."
    $result = Start-Process -FilePath "mysql" -ArgumentList "--host=$server --user=$username --password=$password --database=$database --execute=`"source $scriptPath`"" -NoNewWindow -Wait -PassThru
    
    if ($result.ExitCode -eq 0) {
        Write-Host ""
        Write-Host "Script executed successfully!"
        
        # Verify columns after the changes
        Write-Host ""
        Write-Host "Current column format (after changes):"
        Start-Process -FilePath "mysql" -ArgumentList "--host=$server --user=$username --password=$password --database=$database --execute=`"DESCRIBE scraperconfig`"" -NoNewWindow -Wait
        
        Write-Host ""
        Write-Host "The 'Unknown column 's.AdjustDepthBasedOnQuality' in 'field list'' error should now be resolved."
        Write-Host "Please restart your application to apply the changes."
    }
    else {
        Write-Host "ERROR: Failed to execute the SQL script. Exit code: $($result.ExitCode)"
        exit 1
    }
}
catch {
    Write-Host "ERROR: $_"
    exit 1
}