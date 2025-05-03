-- SQL Script to create missing tables in the webstraction_db database

USE webstraction_db;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_start_url (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url VARCHAR(500) NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS content_extractor_selector (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    selector VARCHAR(255) NOT NULL,
    is_exclude BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS keyword_alert (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    keyword VARCHAR(255) NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS webhook_trigger (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    trigger_name VARCHAR(50) NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS domain_rate_limit (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    domain VARCHAR(255) NOT NULL,
    requests_per_minute INT NOT NULL DEFAULT 60,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS proxy_configuration (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    proxy_url VARCHAR(255) NOT NULL,
    username VARCHAR(100) NULL,
    password VARCHAR(100) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    failure_count INT NOT NULL DEFAULT 0,
    last_used DATETIME NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_schedule (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    name VARCHAR(100) NOT NULL DEFAULT 'Default Schedule',
    cron_expression VARCHAR(100) NOT NULL DEFAULT '0 0 * * *',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_run DATETIME NULL,
    next_run DATETIME NULL,
    max_runtime_minutes INT NULL,
    notification_email VARCHAR(255) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_status (
    scraper_id VARCHAR(36) PRIMARY KEY,
    is_running BOOLEAN NOT NULL DEFAULT FALSE,
    start_time DATETIME NULL,
    end_time DATETIME NULL,
    elapsed_time VARCHAR(50) NULL,
    urls_processed INT NOT NULL DEFAULT 0,
    urls_queued INT NOT NULL DEFAULT 0,
    documents_processed INT NOT NULL DEFAULT 0,
    has_errors BOOLEAN NOT NULL DEFAULT FALSE,
    message VARCHAR(500) NULL,
    last_status_update DATETIME NULL,
    last_update DATETIME NULL,
    last_monitor_check DATETIME NULL,
    last_error TEXT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
