# PowerShell script to run the WebScraperAPI and test the scraper

# Create the ScrapedData directory if it doesn't exist
$scrapedDataDir = Join-Path (Get-Location) "ScrapedData"
if (-not (Test-Path $scrapedDataDir)) {
    Write-Host "Creating ScrapedData directory..." -ForegroundColor Cyan
    New-Item -Path $scrapedDataDir -ItemType Directory -Force | Out-Null
    Write-Host "ScrapedData directory created at: $scrapedDataDir" -ForegroundColor Green
}

# Create the ukgc subdirectory if it doesn't exist
$ukgcDir = Join-Path $scrapedDataDir "ukgc"
if (-not (Test-Path $ukgcDir)) {
    Write-Host "Creating ukgc directory..." -ForegroundColor Cyan
    New-Item -Path $ukgcDir -ItemType Directory -Force | Out-Null
    Write-Host "ukgc directory created at: $ukgcDir" -ForegroundColor Green
}

# Create the run_history subdirectory if it doesn't exist
$runHistoryDir = Join-Path $ukgcDir "run_history"
if (-not (Test-Path $runHistoryDir)) {
    Write-Host "Creating run_history directory..." -ForegroundColor Cyan
    New-Item -Path $runHistoryDir -ItemType Directory -Force | Out-Null
    Write-Host "run_history directory created at: $runHistoryDir" -ForegroundColor Green
}

# Set permissions on the directories
Write-Host "Setting permissions on directories..." -ForegroundColor Cyan
try {
    $acl = Get-Acl $scrapedDataDir
    $accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("Everyone", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
    $acl.SetAccessRule($accessRule)
    Set-Acl $scrapedDataDir $acl
    Write-Host "Permissions set successfully" -ForegroundColor Green
}
catch {
    Write-Host "Failed to set permissions: $_" -ForegroundColor Red
}

# Check if the API is already running
$apiPort = 5203
$apiRunning = $false
try {
    $connection = New-Object System.Net.Sockets.TcpClient("localhost", $apiPort)
    if ($connection.Connected) {
        $apiRunning = $true
        $connection.Close()
    }
}
catch {
    $apiRunning = $false
}

if ($apiRunning) {
    Write-Host "API is already running on port $apiPort" -ForegroundColor Yellow
}
else {
    Write-Host "Starting the WebScraperAPI..." -ForegroundColor Cyan
    Write-Host "Please open a new terminal and run the API using 'dotnet run --project WebScraperAPI'" -ForegroundColor Yellow
    Write-Host "Press Enter when the API is running..." -ForegroundColor Yellow
    Read-Host
}

# Test the API connection
Write-Host "Testing API connection..." -ForegroundColor Cyan
try {
    $response = Invoke-RestMethod -Uri "http://localhost:5203/api/scraper" -Method Get
    Write-Host "API connection successful!" -ForegroundColor Green
    Write-Host "Found scrapers:" -ForegroundColor Green
    $response | ForEach-Object {
        Write-Host "  - $($_.name) (ID: $($_.id))" -ForegroundColor Green
    }
}
catch {
    Write-Host "Failed to connect to API: $_" -ForegroundColor Red
    exit 1
}

# Get the UKGC scraper ID
$ukgcScraperId = $null
$response | ForEach-Object {
    if ($_.name -eq "ukgc") {
        $ukgcScraperId = $_.id
    }
}

if (-not $ukgcScraperId) {
    Write-Host "UKGC scraper not found in the API response" -ForegroundColor Red
    exit 1
}

# Start the UKGC scraper
Write-Host "Starting the UKGC scraper (ID: $ukgcScraperId)..." -ForegroundColor Cyan
try {
    $startResponse = Invoke-RestMethod -Uri "http://localhost:5203/api/scraper/$ukgcScraperId/start" -Method Post
    Write-Host "Scraper started successfully!" -ForegroundColor Green
}
catch {
    Write-Host "Failed to start scraper: $_" -ForegroundColor Red
    exit 1
}

# Monitor the scraper status
Write-Host "Monitoring scraper status..." -ForegroundColor Cyan
$running = $true
$maxAttempts = 30
$attempt = 0

while ($running -and $attempt -lt $maxAttempts) {
    $attempt++
    try {
        $statusResponse = Invoke-RestMethod -Uri "http://localhost:5203/api/scraper/$ukgcScraperId/status" -Method Get
        Write-Host "Status: $($statusResponse.message)" -ForegroundColor Cyan
        Write-Host "  URLs Processed: $($statusResponse.urlsProcessed)" -ForegroundColor Cyan
        Write-Host "  URLs Queued: $($statusResponse.urlsQueued)" -ForegroundColor Cyan
        Write-Host "  Is Running: $($statusResponse.isRunning)" -ForegroundColor Cyan
        
        if (-not $statusResponse.isRunning) {
            $running = $false
            Write-Host "Scraper has stopped running" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "Failed to get scraper status: $_" -ForegroundColor Red
    }
    
    Start-Sleep -Seconds 5
}

# Check if any content was saved
Write-Host "Checking for saved content..." -ForegroundColor Cyan

# Check database
try {
    $scrapedPagesResponse = Invoke-RestMethod -Uri "http://localhost:5203/api/scraper/$ukgcScraperId/pages" -Method Get
    $pageCount = $scrapedPagesResponse.Count
    Write-Host "Found $pageCount pages in the database" -ForegroundColor Green
    
    if ($pageCount -gt 0) {
        Write-Host "First page URL: $($scrapedPagesResponse[0].url)" -ForegroundColor Green
    }
}
catch {
    Write-Host "Failed to get scraped pages from database: $_" -ForegroundColor Red
}

# Check files
$htmlFiles = Get-ChildItem -Path $ukgcDir -Filter "*.html" -File
$textFiles = Get-ChildItem -Path $ukgcDir -Filter "*.txt" -File
$htmlCount = $htmlFiles.Count
$textCount = $textFiles.Count

Write-Host "Found $htmlCount HTML files and $textCount text files in $ukgcDir" -ForegroundColor Green

if ($htmlCount -gt 0) {
    Write-Host "First HTML file: $($htmlFiles[0].Name)" -ForegroundColor Green
}

if ($textCount -gt 0) {
    Write-Host "First text file: $($textFiles[0].Name)" -ForegroundColor Green
}

# Check run history
$historyFiles = Get-ChildItem -Path $runHistoryDir -File
$historyCount = $historyFiles.Count

Write-Host "Found $historyCount history files in $runHistoryDir" -ForegroundColor Green

if ($historyCount -gt 0) {
    Write-Host "First history file: $($historyFiles[0].Name)" -ForegroundColor Green
}

Write-Host "Test completed!" -ForegroundColor Green
