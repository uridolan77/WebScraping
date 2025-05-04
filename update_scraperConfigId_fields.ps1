# Script to update all scraperConfigId fields to VARCHAR(255)
# This ensures consistency with Entity Framework Core data model

# Database connection parameters
$MySQLHost = "localhost"
$MySQLUser = "root"
$MySQLPassword = ""  # You'll be prompted for this
$Database = "webstraction_db"

# Path to SQL script
$SQLScriptPath = "C:\dev\WebScraping\update_scraperConfigId_fields.sql"

Write-Host "Updating all scraperConfigId fields to VARCHAR(255)..." -ForegroundColor Cyan

# Prompt for password if not provided
if ([string]::IsNullOrEmpty($MySQLPassword)) {
    $SecurePassword = Read-Host "Enter MySQL password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    $MySQLPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

# Check if the script exists
if (!(Test-Path $SQLScriptPath)) {
    Write-Host "Error: SQL script not found at $SQLScriptPath" -ForegroundColor Red
    exit 1
}

# Run the MySQL command
try {
    $command = "mysql -h $MySQLHost -u $MySQLUser -p`"$MySQLPassword`" $Database < `"$SQLScriptPath`""
    
    # Display a command preview (without password)
    $safeCommand = "mysql -h $MySQLHost -u $MySQLUser -p****** $Database < `"$SQLScriptPath`""
    Write-Host "Executing: $safeCommand" -ForegroundColor Yellow
    
    # Execute the command
    $result = Invoke-Expression "cmd /c $command 2>&1"
    
    # Check for errors
    if ($result -and $result -like "*ERROR*") {
        Write-Host "Error executing SQL script:" -ForegroundColor Red
        Write-Host $result -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Successfully updated all scraperConfigId fields to VARCHAR(255)" -ForegroundColor Green
}
catch {
    Write-Host "Error executing SQL command: $_" -ForegroundColor Red
    exit 1
}

# Verify changes
Write-Host "Verifying changes..." -ForegroundColor Cyan
$verifyCommand = "mysql -h $MySQLHost -u $MySQLUser -p`"$MySQLPassword`" -e " + 
                 "`"SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH FROM INFORMATION_SCHEMA.COLUMNS " +
                 "WHERE TABLE_SCHEMA = '$Database' AND COLUMN_NAME IN ('scraperConfigId', 'scraper_config_id')`" " +
                 "$Database"

# Display a command preview (without password)
$safeVerifyCommand = "mysql -h $MySQLHost -u $MySQLUser -p****** -e [query] $Database"
Write-Host "Executing: $safeVerifyCommand" -ForegroundColor Yellow

# Execute the verification command
$verifyResult = Invoke-Expression "cmd /c $verifyCommand 2>&1"
Write-Host $verifyResult

Write-Host "Verification complete!" -ForegroundColor Green