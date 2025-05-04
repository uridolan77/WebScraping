# PowerShell script to fix the AdjustDepthBasedOnQuality column issue
param(
    [string]$server = "localhost",
    [string]$username = "root",
    [string]$password = "Dt%g_9W3z0*!I",
    [string]$database = "webstraction_db",
    [string]$scriptPath = "fix_adjust_depth_column.sql"
)

Write-Host "============================================"
Write-Host "  Fixing AdjustDepthBasedOnQuality Column"
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
    
    # Execute the script
    Write-Host ""
    Write-Host "Fixing AdjustDepthBasedOnQuality column to match Entity Framework expectations..."
    & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "source $scriptPath"
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "Column fix completed successfully!"
        
        # Verify the table structure after the fix
        Write-Host ""
        Write-Host "Current structure of the scraperconfig table (look for adjustdepthbasedonquality):"
        & $mysqlPath --host=$server --user=$username --password=$password --database=$database -e "DESCRIBE scraperconfig;"
        
        Write-Host ""
        Write-Host "The 'Unknown column 's.AdjustDepthBasedOnQuality' in 'field list'' error should now be resolved."
        Write-Host ""
        Write-Host "Please restart your application to apply the changes."
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