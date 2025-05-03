-- SQL Script to fix the database issue

-- Check if the database exists, if not create it
CREATE DATABASE IF NOT EXISTS webstraction_db;

USE webstraction_db;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_config (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_run DATETIME NULL,
    run_count INT NOT NULL DEFAULT 0,
    start_url VARCHAR(500) NOT NULL,
    base_url VARCHAR(500) NOT NULL,
    output_directory VARCHAR(255) NOT NULL DEFAULT 'ScrapedData',
    delay_between_requests INT NOT NULL DEFAULT 1000,
    max_concurrent_requests INT NOT NULL DEFAULT 5,
    max_depth INT NOT NULL DEFAULT 5,
    max_pages INT NOT NULL DEFAULT 1000,
    follow_links BOOLEAN NOT NULL DEFAULT TRUE,
    follow_external_links BOOLEAN NOT NULL DEFAULT FALSE,
    respect_robots_txt BOOLEAN NOT NULL DEFAULT TRUE,
    auto_learn_header_footer BOOLEAN NOT NULL DEFAULT TRUE,
    learning_pages_count INT NOT NULL DEFAULT 5,
    enable_change_detection BOOLEAN NOT NULL DEFAULT TRUE,
    track_content_versions BOOLEAN NOT NULL DEFAULT TRUE,
    max_versions_to_keep INT NOT NULL DEFAULT 5,
    enable_adaptive_crawling BOOLEAN NOT NULL DEFAULT TRUE,
    priority_queue_size INT NOT NULL DEFAULT 100,
    adjust_depth_based_on_quality BOOLEAN NOT NULL DEFAULT TRUE,
    enable_adaptive_rate_limiting BOOLEAN NOT NULL DEFAULT TRUE,
    min_delay_between_requests INT NOT NULL DEFAULT 500,
    max_delay_between_requests INT NOT NULL DEFAULT 5000,
    monitor_response_times BOOLEAN NOT NULL DEFAULT TRUE,
    max_requests_per_minute INT NOT NULL DEFAULT 60,
    user_agent VARCHAR(255) NOT NULL DEFAULT 'WebStraction Crawler/1.0',
    back_off_on_errors BOOLEAN NOT NULL DEFAULT TRUE,
    use_proxies BOOLEAN NOT NULL DEFAULT FALSE,
    proxy_rotation_strategy VARCHAR(50) NULL,
    test_proxies_before_use BOOLEAN NOT NULL DEFAULT TRUE,
    max_proxy_failures_before_removal INT NOT NULL DEFAULT 3,
    enable_continuous_monitoring BOOLEAN NOT NULL DEFAULT FALSE,
    monitoring_interval_minutes INT NOT NULL DEFAULT 60,
    notify_on_changes BOOLEAN NOT NULL DEFAULT FALSE,
    notification_email VARCHAR(255) NULL,
    track_changes_history BOOLEAN NOT NULL DEFAULT TRUE,
    enable_regulatory_content_analysis BOOLEAN NOT NULL DEFAULT FALSE,
    track_regulatory_changes BOOLEAN NOT NULL DEFAULT FALSE,
    classify_regulatory_documents BOOLEAN NOT NULL DEFAULT FALSE,
    extract_structured_content BOOLEAN NOT NULL DEFAULT FALSE,
    process_pdf_documents BOOLEAN NOT NULL DEFAULT FALSE,
    monitor_high_impact_changes BOOLEAN NOT NULL DEFAULT FALSE,
    extract_metadata BOOLEAN NOT NULL DEFAULT TRUE,
    extract_structured_data BOOLEAN NOT NULL DEFAULT FALSE,
    custom_js_extractor TEXT NULL,
    wait_for_selector VARCHAR(255) NULL,
    is_ukgc_website BOOLEAN NOT NULL DEFAULT FALSE,
    prioritize_enforcement_actions BOOLEAN NOT NULL DEFAULT TRUE,
    prioritize_lccp BOOLEAN NOT NULL DEFAULT TRUE,
    prioritize_aml BOOLEAN NOT NULL DEFAULT TRUE,
    notification_endpoint VARCHAR(255) NULL,
    webhook_enabled BOOLEAN NOT NULL DEFAULT FALSE,
    webhook_url VARCHAR(255) NULL,
    notify_on_content_changes BOOLEAN NOT NULL DEFAULT FALSE,
    notify_on_document_processed BOOLEAN NOT NULL DEFAULT FALSE,
    notify_on_scraper_status_change BOOLEAN NOT NULL DEFAULT FALSE,
    webhook_format VARCHAR(50) NULL DEFAULT 'JSON',
    enable_content_compression BOOLEAN NOT NULL DEFAULT FALSE,
    compression_threshold_bytes INT NOT NULL DEFAULT 102400,
    collect_detailed_metrics BOOLEAN NOT NULL DEFAULT TRUE,
    metrics_reporting_interval_seconds INT NOT NULL DEFAULT 60,
    track_domain_metrics BOOLEAN NOT NULL DEFAULT FALSE,
    scraper_type VARCHAR(50) NULL DEFAULT 'Standard'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

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

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_run (
    id VARCHAR(36) PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    start_time DATETIME NOT NULL,
    end_time DATETIME NULL,
    urls_processed INT NOT NULL DEFAULT 0,
    documents_processed INT NOT NULL DEFAULT 0,
    successful BOOLEAN NULL,
    error_message TEXT NULL,
    elapsed_time VARCHAR(50) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS pipeline_metric (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    processing_items INT NOT NULL DEFAULT 0,
    queued_items INT NOT NULL DEFAULT 0,
    completed_items INT NOT NULL DEFAULT 0,
    failed_items INT NOT NULL DEFAULT 0,
    average_processing_time_ms DOUBLE NOT NULL DEFAULT 0,
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_run(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS log_entry (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    message TEXT NOT NULL,
    level VARCHAR(20) NOT NULL DEFAULT 'INFO',
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_run(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS content_change_record (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url VARCHAR(500) NOT NULL,
    detected_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    change_type VARCHAR(50) NOT NULL,
    significance_score FLOAT NOT NULL DEFAULT 0,
    change_summary TEXT NULL,
    previous_version_hash VARCHAR(64) NULL,
    current_version_hash VARCHAR(64) NULL,
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_run(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS processed_document (
    id VARCHAR(36) PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url VARCHAR(500) NOT NULL,
    title VARCHAR(255) NULL,
    document_type VARCHAR(50) NOT NULL,
    processed_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    content_size_bytes BIGINT NOT NULL DEFAULT 0,
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_run(id) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS document_metadata (
    id INT AUTO_INCREMENT PRIMARY KEY,
    document_id VARCHAR(36) NOT NULL,
    metadata_key VARCHAR(100) NOT NULL,
    metadata_value TEXT NULL,
    FOREIGN KEY (document_id) REFERENCES processed_document(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS scraper_metrics (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    scraper_name VARCHAR(100) NOT NULL,
    first_run_time DATETIME NOT NULL DEFAULT '1970-01-01 00:00:00',
    last_run_time DATETIME NOT NULL DEFAULT '1970-01-01 00:00:00',
    last_metrics_update DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    total_runs INT NOT NULL DEFAULT 0,
    failed_runs INT NOT NULL DEFAULT 0,
    execution_count INT NOT NULL DEFAULT 0,
    processed_urls INT NOT NULL DEFAULT 0,
    failed_urls INT NOT NULL DEFAULT 0,
    bytes_downloaded BIGINT NOT NULL DEFAULT 0,
    documents_processed INT NOT NULL DEFAULT 0,
    documents_crawled INT NOT NULL DEFAULT 0,
    processing_time_ms BIGINT NOT NULL DEFAULT 0,
    total_crawl_time_ms BIGINT NOT NULL DEFAULT 0,
    session_start_time DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_session_duration_ms BIGINT NOT NULL DEFAULT 0,
    total_scraping_time_ms BIGINT NOT NULL DEFAULT 0,
    successful_requests INT NOT NULL DEFAULT 0,
    failed_requests INT NOT NULL DEFAULT 0,
    pages_processed INT NOT NULL DEFAULT 0,
    total_links_extracted INT NOT NULL DEFAULT 0,
    content_items_extracted INT NOT NULL DEFAULT 0,
    content_changes_detected INT NOT NULL DEFAULT 0,
    total_page_processing_time_ms BIGINT NOT NULL DEFAULT 0,
    total_document_processing_time_ms BIGINT NOT NULL DEFAULT 0,
    total_document_size_bytes BIGINT NOT NULL DEFAULT 0,
    client_errors INT NOT NULL DEFAULT 0,
    server_errors INT NOT NULL DEFAULT 0,
    rate_limit_errors INT NOT NULL DEFAULT 0,
    rate_limits_encountered INT NOT NULL DEFAULT 0,
    network_errors INT NOT NULL DEFAULT 0,
    timeout_errors INT NOT NULL DEFAULT 0,
    pending_requests INT NOT NULL DEFAULT 0,
    total_bytes_downloaded BIGINT NOT NULL DEFAULT 0,
    current_memory_usage_mb DOUBLE NOT NULL DEFAULT 0,
    peak_memory_usage_mb DOUBLE NOT NULL DEFAULT 0,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Check if the table exists, if not create it
CREATE TABLE IF NOT EXISTS custom_metric (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_metrics_id INT NOT NULL,
    metric_name VARCHAR(100) NOT NULL,
    metric_value DOUBLE NOT NULL DEFAULT 0,
    FOREIGN KEY (scraper_metrics_id) REFERENCES scraper_metrics(id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create indexes for performance if they don't exist
CREATE INDEX IF NOT EXISTS idx_scraper_run_scraper_id ON scraper_run(scraper_id);
CREATE INDEX IF NOT EXISTS idx_log_entry_scraper_id ON log_entry(scraper_id);
CREATE INDEX IF NOT EXISTS idx_content_change_record_scraper_id ON content_change_record(scraper_id);
CREATE INDEX IF NOT EXISTS idx_processed_document_scraper_id ON processed_document(scraper_id);
CREATE INDEX IF NOT EXISTS idx_scraper_metrics_scraper_id ON scraper_metrics(scraper_id);

-- Insert sample data for UKGC scraper if it doesn't exist
INSERT IGNORE INTO scraper_config (id, name, created_at, last_modified, start_url, base_url, output_directory, is_ukgc_website)
VALUES ('d6d6eb97-7136-4eaf-b7ca-16ed6202c7ad', 'ukgc', NOW(), NOW(), 'https://www.gamblingcommission.gov.uk/licensees-and-businesses', 'https://www.gamblingcommission.gov.uk', 'ScrapedData', TRUE);

-- Insert sample data for MGA scraper if it doesn't exist
INSERT IGNORE INTO scraper_config (id, name, created_at, last_modified, start_url, base_url, output_directory)
VALUES ('1af8de30-c878-4507-9ff6-5e595960e14c', 'mga', NOW(), NOW(), 'https://www.mga.org.mt/licensee-hub/', 'https://www.mga.org.mt', 'ScrapedData');
