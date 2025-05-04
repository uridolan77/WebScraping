# Script to check and fix run history directories
Write-Host "Checking run history directories..."

# Get the base directory where the application is running
$baseDir = Split-Path -Parent $PSScriptRoot
$apiDir = Join-Path $baseDir "WebScraperAPI"

# Define expected run history directories
$runHistoryPaths = @(
    (Join-Path $apiDir "ScrapedData\run_history"),
    (Join-Path $apiDir "run_history"),
    (Join-Path $baseDir "run_history")
)

# Create all potential run history directories
foreach ($path in $runHistoryPaths) {
    if (-not (Test-Path $path)) {
        Write-Host "Creating missing run history directory: $path"
        New-Item -Path $path -ItemType Directory -Force
    } else {
        Write-Host "Run history directory exists: $path"
    }

    # Check write permissions
    Try {
        $testFile = Join-Path $path "write_test.tmp"
        Set-Content -Path $testFile -Value "Write test" -ErrorAction Stop
        Remove-Item $testFile -ErrorAction Stop
        Write-Host "  - Directory is writable: $path" -ForegroundColor Green
    } Catch {
        Write-Host "  - Directory is NOT writable: $path" -ForegroundColor Red
        Write-Host "    Error: $($_.Exception.Message)"
    }
}

# Fix the state directory structure
$stateDir = Join-Path $apiDir "ScrapedData\state"
if (Test-Path $stateDir) {
    $stateDirDuplicate = Join-Path $stateDir "state"
    if (Test-Path $stateDirDuplicate) {
        Write-Host "Found duplicate state directory structure. Fixing..."
        $files = Get-ChildItem -Path $stateDirDuplicate -File
        foreach ($file in $files) {
            Move-Item -Path $file.FullName -Destination $stateDir -Force
            Write-Host "  - Moved $($file.Name) to correct directory"
        }
    }
}

Write-Host "Checking for existing run history files..."
$existingHistoryFiles = Get-ChildItem -Path $runHistoryPaths -Filter "run_history_*.json" -Recurse -ErrorAction SilentlyContinue
if ($existingHistoryFiles -and $existingHistoryFiles.Count -gt 0) {
    Write-Host "Found $($existingHistoryFiles.Count) run history files:" -ForegroundColor Green
    foreach ($file in $existingHistoryFiles) {
        Write-Host "  - $($file.FullName)"
    }
} else {
    Write-Host "No run history files found." -ForegroundColor Yellow
}

# Create a sample run history file in each directory to ensure functionality
foreach ($path in $runHistoryPaths) {
    $timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
    $sampleFile = Join-Path $path "run_history_sample_$timestamp.json"
    
    $sampleContent = @{
        ScraperId = "sample-scraper"
        ScraperName = "Sample Scraper"
        RunId = "sample-$timestamp"
        StartTime = (Get-Date).ToString("o")
        Message = "This is a sample run history file created to test directory permissions"
    } | ConvertTo-Json -Depth 10
    
    Try {
        Set-Content -Path $sampleFile -Value $sampleContent -ErrorAction Stop
        Write-Host "Created sample run history file: $sampleFile" -ForegroundColor Green
    } Catch {
        Write-Host "Failed to create sample file in $path" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)"
    }
}

Write-Host "Run history directory check complete."