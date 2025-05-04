# PowerShell script to create the ScrapedData directory and check permissions

# Get the base directory of the application
$baseDir = Get-Location
$scrapedDataDir = Join-Path $baseDir "ScrapedData"

Write-Host "Creating ScrapedData directory at: $scrapedDataDir"

# Create the directory if it doesn't exist
if (-not (Test-Path $scrapedDataDir)) {
    try {
        New-Item -Path $scrapedDataDir -ItemType Directory -Force
        Write-Host "Successfully created ScrapedData directory" -ForegroundColor Green
    }
    catch {
        Write-Host "Error creating ScrapedData directory: $_" -ForegroundColor Red
        exit 1
    }
}
else {
    Write-Host "ScrapedData directory already exists" -ForegroundColor Yellow
}

# Create a test file to verify write permissions
$testFilePath = Join-Path $scrapedDataDir "test_write.txt"
try {
    "Test file created at $(Get-Date)" | Out-File -FilePath $testFilePath
    Write-Host "Successfully created test file at: $testFilePath" -ForegroundColor Green
    
    # Delete the test file
    Remove-Item -Path $testFilePath -Force
    Write-Host "Test file deleted successfully" -ForegroundColor Green
}
catch {
    Write-Host "Error writing to ScrapedData directory: $_" -ForegroundColor Red
    Write-Host "Please check permissions on the directory" -ForegroundColor Red
    exit 1
}

# Create a subdirectory for the UKGC scraper
$ukgcDir = Join-Path $scrapedDataDir "ukgc"
if (-not (Test-Path $ukgcDir)) {
    try {
        New-Item -Path $ukgcDir -ItemType Directory -Force
        Write-Host "Successfully created UKGC scraper directory" -ForegroundColor Green
    }
    catch {
        Write-Host "Error creating UKGC scraper directory: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "UKGC scraper directory already exists" -ForegroundColor Yellow
}

# Create a run_history subdirectory
$runHistoryDir = Join-Path $ukgcDir "run_history"
if (-not (Test-Path $runHistoryDir)) {
    try {
        New-Item -Path $runHistoryDir -ItemType Directory -Force
        Write-Host "Successfully created run_history directory" -ForegroundColor Green
    }
    catch {
        Write-Host "Error creating run_history directory: $_" -ForegroundColor Red
    }
}
else {
    Write-Host "run_history directory already exists" -ForegroundColor Yellow
}

Write-Host "Directory setup completed successfully" -ForegroundColor Green
