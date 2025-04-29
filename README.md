# WebScraping Project

This project consists of a WebScraperAPI and a WebScraperWeb application for managing and running web scrapers.

## Database Setup

The project uses MySQL for data storage. Follow these steps to set up the database:

1. Make sure MySQL is installed and running on your system.
2. Run the setup script to create the database and tables:

```powershell
# From the project root directory
.\setup_database.ps1
```

This script will:
- Create a database named `webstraction_db`
- Create all necessary tables
- Create a user named `webstraction_user` with password `webstraction_password`
- Insert sample data including a UKGC scraper

If you need to customize the database connection, edit the following files:
- `setup_database.ps1` - For the initial setup
- `WebScraperAPI\appsettings.json` - For the application connection string

## Project Structure

### WebScraperAPI

The API project provides endpoints for managing scrapers, viewing results, and controlling execution.

Key components:
- `Models/` - Data models for the API
- `Data/` - Database context and entity definitions
- `Services/` - Business logic for scraper operations
- `Controllers/` - API endpoints

### WebScraperWeb

The web application provides a user interface for managing scrapers.

## Running the Application

### API

1. Open the solution in Visual Studio
2. Set WebScraperAPI as the startup project
3. Press F5 to run

The API will be available at https://localhost:7143 with Swagger UI at /swagger/index.html

### Web Application

1. Navigate to the WebScraperWeb directory
2. Install dependencies:
```
npm install
```
3. Start the development server:
```
npm start
```

The web application will be available at http://localhost:3000

## Database Schema

The database includes the following main tables:

- `scraper_config` - Scraper configuration
- `scraper_status` - Current status of scrapers
- `scraper_runs` - History of scraper executions
- `content_change_records` - Detected content changes
- `processed_documents` - Documents processed by scrapers
- `log_entries` - Scraper log messages
- `scraper_metrics` - Performance metrics

## Entity Framework Core

The application uses Entity Framework Core with Pomelo.EntityFrameworkCore.MySql provider.

To update the database schema after model changes:

```
dotnet ef migrations add MigrationName
dotnet ef database update
```

## Sample Scrapers

The database is pre-populated with a sample UKGC scraper that can be used for testing.
