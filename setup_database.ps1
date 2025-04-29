# PowerShell script to set up the MySQL database for WebScraper

# MySQL connection parameters
$mysqlHost = "localhost"
$mysqlUser = "root"
$mysqlPassword = ""  # Replace with your MySQL root password if needed
$sqlFilePath = "C:\dev\WebScraping\create_webstraction_db.sql"

# Check if MySQL is installed
try {
    $mysqlPath = (Get-Command mysql -ErrorAction Stop).Source
    Write-Host "MySQL found at: $mysqlPath"
} catch {
    Write-Host "MySQL not found. Please make sure MySQL is installed and added to your PATH." -ForegroundColor Red
    exit 1
}

# Execute the SQL script
try {
    Write-Host "Creating database and tables..." -ForegroundColor Yellow

    # Get the content of the SQL file
    $sqlContent = Get-Content -Path $sqlFilePath -Raw

    if ($mysqlPassword -eq "") {
        # No password
        # Write the SQL to a temporary file
        $tempFile = [System.IO.Path]::GetTempFileName()
        $sqlContent | Out-File -FilePath $tempFile -Encoding ASCII

        # Use Get-Content to pipe the content to mysql
        Get-Content -Path $tempFile -Raw | & mysql -h $mysqlHost -u $mysqlUser

        # Clean up
        Remove-Item -Path $tempFile -Force
    } else {
        # With password
        # Write the SQL to a temporary file
        $tempFile = [System.IO.Path]::GetTempFileName()
        $sqlContent | Out-File -FilePath $tempFile -Encoding ASCII

        # Use Get-Content to pipe the content to mysql
        Get-Content -Path $tempFile -Raw | & mysql -h $mysqlHost -u $mysqlUser -p$mysqlPassword

        # Clean up
        Remove-Item -Path $tempFile -Force
    }

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Database setup completed successfully!" -ForegroundColor Green
    } else {
        Write-Host "Error executing SQL script. Exit code: $LASTEXITCODE" -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

# Verify database creation
try {
    Write-Host "Verifying database creation..." -ForegroundColor Yellow

    if ($mysqlPassword -eq "") {
        # No password
        $query = "SHOW DATABASES LIKE 'webstraction_db';"
        $result = $query | & mysql -h $mysqlHost -u $mysqlUser
    } else {
        # With password
        $query = "SHOW DATABASES LIKE 'webstraction_db';"
        $result = $query | & mysql -h $mysqlHost -u $mysqlUser -p$mysqlPassword
    }

    if ($result -match "webstraction_db") {
        Write-Host "Database 'webstraction_db' verified!" -ForegroundColor Green

        # Show tables
        Write-Host "Tables in webstraction_db:" -ForegroundColor Cyan
        if ($mysqlPassword -eq "") {
            # No password
            $query = "USE webstraction_db; SHOW TABLES;"
            $query | & mysql -h $mysqlHost -u $mysqlUser
        } else {
            # With password
            $query = "USE webstraction_db; SHOW TABLES;"
            $query | & mysql -h $mysqlHost -u $mysqlUser -p$mysqlPassword
        }
    } else {
        Write-Host "Database 'webstraction_db' not found after creation attempt." -ForegroundColor Red
        exit 1
    }
} catch {
    Write-Host "Error verifying database: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nDatabase setup complete. Connection string for your application:" -ForegroundColor Green
Write-Host "Server=localhost;Database=webstraction_db;User=webstraction_user;Password=webstraction_password;" -ForegroundColor Yellow

# Instructions for Entity Framework
Write-Host "`nTo use with Entity Framework Core, add this connection string to your appsettings.json:" -ForegroundColor Cyan
Write-Host '{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=webstraction_db;User=webstraction_user;Password=webstraction_password;"
  }
}' -ForegroundColor Yellow

Write-Host "`nThen in your Startup.cs or Program.cs, configure the DbContext with:" -ForegroundColor Cyan
Write-Host 'services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(Configuration.GetConnectionString("DefaultConnection"))));' -ForegroundColor Yellow

Write-Host "`nDon't forget to install the required NuGet packages:" -ForegroundColor Cyan
Write-Host "Pomelo.EntityFrameworkCore.MySql" -ForegroundColor Yellow
