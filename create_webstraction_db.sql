-- Create the webstraction_db database
CREATE DATABASE IF NOT EXISTS webstraction_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Use the database
USE webstraction_db;

-- Create ScraperConfig table
CREATE TABLE IF NOT EXISTS scraper_config (
    id VARCHAR(36) PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_modified DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    last_run DATETIME NULL,
    run_count INT NOT NULL DEFAULT 0,
    start_url TEXT NOT NULL,
    base_url TEXT NOT NULL,
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
    user_agent VARCHAR(255) NOT NULL DEFAULT 'Mozilla/5.0 WebScraper Bot',
    back_off_on_errors BOOLEAN NOT NULL DEFAULT TRUE,
    use_proxies BOOLEAN NOT NULL DEFAULT FALSE,
    proxy_rotation_strategy VARCHAR(50) NOT NULL DEFAULT 'RoundRobin',
    test_proxies_before_use BOOLEAN NOT NULL DEFAULT TRUE,
    max_proxy_failures_before_removal INT NOT NULL DEFAULT 3,
    enable_continuous_monitoring BOOLEAN NOT NULL DEFAULT FALSE,
    monitoring_interval_minutes INT NOT NULL DEFAULT 1440,
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
    webhook_format VARCHAR(50) NOT NULL DEFAULT 'json',
    enable_content_compression BOOLEAN NOT NULL DEFAULT TRUE,
    compression_threshold_bytes INT NOT NULL DEFAULT 1024,
    collect_detailed_metrics BOOLEAN NOT NULL DEFAULT TRUE,
    metrics_reporting_interval_seconds INT NOT NULL DEFAULT 60,
    track_domain_metrics BOOLEAN NOT NULL DEFAULT TRUE,
    scraper_type VARCHAR(50) NOT NULL DEFAULT 'Standard'
);

-- Create table for start URLs (many-to-one relationship with scraper_config)
CREATE TABLE IF NOT EXISTS scraper_start_urls (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url TEXT NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for content extractor selectors
CREATE TABLE IF NOT EXISTS content_extractor_selectors (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    selector TEXT NOT NULL,
    is_exclude BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for keyword alert list
CREATE TABLE IF NOT EXISTS keyword_alert_list (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    keyword VARCHAR(255) NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for webhook triggers
CREATE TABLE IF NOT EXISTS webhook_triggers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    trigger_name VARCHAR(50) NOT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for domain rate limits
CREATE TABLE IF NOT EXISTS domain_rate_limits (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    domain VARCHAR(255) NOT NULL,
    max_requests_per_minute INT NOT NULL DEFAULT 60,
    delay_between_requests INT NOT NULL DEFAULT 1000,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for proxy configurations
CREATE TABLE IF NOT EXISTS proxy_configurations (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    host VARCHAR(255) NOT NULL,
    port INT NOT NULL,
    username VARCHAR(255) NULL,
    password VARCHAR(255) NULL,
    protocol VARCHAR(50) NOT NULL DEFAULT 'http',
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    failure_count INT NOT NULL DEFAULT 0,
    last_used DATETIME NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for scraper schedules
CREATE TABLE IF NOT EXISTS scraper_schedules (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    name VARCHAR(255) NOT NULL,
    cron_expression VARCHAR(100) NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_run DATETIME NULL,
    next_run DATETIME NULL,
    max_runtime_minutes INT NULL,
    notification_email VARCHAR(255) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for scraper runs
CREATE TABLE IF NOT EXISTS scraper_runs (
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
);

-- Create table for scraper status
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
    message TEXT NULL,
    last_status_update DATETIME NULL,
    last_update DATETIME NULL,
    last_monitor_check DATETIME NULL,
    last_error TEXT NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for pipeline metrics
CREATE TABLE IF NOT EXISTS pipeline_metrics (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    processing_items INT NOT NULL DEFAULT 0,
    queued_items INT NOT NULL DEFAULT 0,
    completed_items INT NOT NULL DEFAULT 0,
    failed_items INT NOT NULL DEFAULT 0,
    average_processing_time_ms DOUBLE NOT NULL DEFAULT 0,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create table for log entries
CREATE TABLE IF NOT EXISTS log_entries (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    timestamp DATETIME NOT NULL,
    message TEXT NOT NULL,
    level VARCHAR(20) NOT NULL DEFAULT 'Info',
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_runs(id) ON DELETE CASCADE
);

-- Create table for content change records
CREATE TABLE IF NOT EXISTS content_change_records (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url TEXT NOT NULL,
    change_type ENUM('Addition', 'Removal', 'Modification', 'StructuralChange', 'Other') NOT NULL,
    detected_at DATETIME NOT NULL,
    significance INT NOT NULL DEFAULT 0,
    change_details TEXT NULL,
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_runs(id) ON DELETE CASCADE
);

-- Create table for processed documents
CREATE TABLE IF NOT EXISTS processed_documents (
    id VARCHAR(36) PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    url TEXT NOT NULL,
    title VARCHAR(255) NULL,
    document_type VARCHAR(50) NOT NULL DEFAULT 'HTML',
    processed_at DATETIME NOT NULL,
    content_size_bytes BIGINT NOT NULL DEFAULT 0,
    run_id VARCHAR(36) NULL,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE,
    FOREIGN KEY (run_id) REFERENCES scraper_runs(id) ON DELETE CASCADE
);

-- Create table for document metadata
CREATE TABLE IF NOT EXISTS document_metadata (
    id INT AUTO_INCREMENT PRIMARY KEY,
    document_id VARCHAR(36) NOT NULL,
    meta_key VARCHAR(255) NOT NULL,
    meta_value TEXT NULL,
    FOREIGN KEY (document_id) REFERENCES processed_documents(id) ON DELETE CASCADE
);

-- Create table for scraper metrics
CREATE TABLE IF NOT EXISTS scraper_metrics (
    id INT AUTO_INCREMENT PRIMARY KEY,
    scraper_id VARCHAR(36) NOT NULL,
    metric_name VARCHAR(100) NOT NULL,
    metric_value DOUBLE NOT NULL,
    timestamp DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (scraper_id) REFERENCES scraper_config(id) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX idx_scraper_runs_scraper_id ON scraper_runs(scraper_id);
CREATE INDEX idx_log_entries_scraper_id ON log_entries(scraper_id);
CREATE INDEX idx_log_entries_timestamp ON log_entries(timestamp);
CREATE INDEX idx_content_change_records_scraper_id ON content_change_records(scraper_id);
CREATE INDEX idx_content_change_records_detected_at ON content_change_records(detected_at);
CREATE INDEX idx_processed_documents_scraper_id ON processed_documents(scraper_id);
CREATE INDEX idx_processed_documents_processed_at ON processed_documents(processed_at);
CREATE INDEX idx_scraper_metrics_scraper_id ON scraper_metrics(scraper_id);
CREATE INDEX idx_scraper_metrics_timestamp ON scraper_metrics(timestamp);

-- Create a view for scraper summary
CREATE OR REPLACE VIEW scraper_summary AS
SELECT 
    sc.id,
    sc.name,
    sc.created_at,
    sc.last_modified,
    sc.last_run,
    sc.run_count,
    sc.start_url,
    sc.base_url,
    ss.is_running,
    ss.urls_processed,
    ss.documents_processed,
    ss.has_errors,
    ss.last_update,
    (SELECT COUNT(*) FROM scraper_runs sr WHERE sr.scraper_id = sc.id) AS total_runs,
    (SELECT COUNT(*) FROM content_change_records ccr WHERE ccr.scraper_id = sc.id) AS total_changes,
    (SELECT COUNT(*) FROM processed_documents pd WHERE pd.scraper_id = sc.id) AS total_documents
FROM 
    scraper_config sc
LEFT JOIN 
    scraper_status ss ON sc.id = ss.scraper_id;

-- Create a user for the application
CREATE USER IF NOT EXISTS 'webstraction_user'@'localhost' IDENTIFIED BY 'webstraction_password';
GRANT ALL PRIVILEGES ON webstraction_db.* TO 'webstraction_user'@'localhost';
FLUSH PRIVILEGES;

-- Insert a sample UKGC scraper
INSERT INTO scraper_config (
    id, 
    name, 
    created_at,
    start_url, 
    base_url, 
    is_ukgc_website,
    enable_change_detection,
    notify_on_changes,
    process_pdf_documents
) VALUES (
    'ukgc-scraper-1',
    'UKGC Scraper',
    NOW(),
    'https://www.gamblingcommission.gov.uk',
    'https://www.gamblingcommission.gov.uk',
    TRUE,
    TRUE,
    TRUE,
    TRUE
);

-- Insert sample start URLs for UKGC scraper
INSERT INTO scraper_start_urls (scraper_id, url) VALUES 
('ukgc-scraper-1', 'https://www.gamblingcommission.gov.uk/licensees-and-businesses'),
('ukgc-scraper-1', 'https://www.gamblingcommission.gov.uk/public-and-players'),
('ukgc-scraper-1', 'https://www.gamblingcommission.gov.uk/news');

-- Insert sample keyword alerts for UKGC scraper
INSERT INTO keyword_alert_list (scraper_id, keyword) VALUES 
('ukgc-scraper-1', 'enforcement'),
('ukgc-scraper-1', 'fine'),
('ukgc-scraper-1', 'penalty'),
('ukgc-scraper-1', 'license revocation'),
('ukgc-scraper-1', 'regulatory action');

-- Insert sample scraper status
INSERT INTO scraper_status (
    scraper_id,
    is_running,
    urls_processed,
    documents_processed,
    has_errors,
    last_update
) VALUES (
    'ukgc-scraper-1',
    FALSE,
    120,
    15,
    FALSE,
    NOW()
);

-- Insert sample scraper run
INSERT INTO scraper_runs (
    id,
    scraper_id,
    start_time,
    end_time,
    urls_processed,
    documents_processed,
    successful,
    elapsed_time
) VALUES (
    'run-1',
    'ukgc-scraper-1',
    DATE_SUB(NOW(), INTERVAL 1 DAY),
    DATE_SUB(NOW(), INTERVAL 23 HOUR),
    120,
    15,
    TRUE,
    '01:00:00'
);

-- Insert sample log entries
INSERT INTO log_entries (scraper_id, timestamp, message, level, run_id) VALUES 
('ukgc-scraper-1', DATE_SUB(NOW(), INTERVAL 1 DAY), 'Scraper started', 'Info', 'run-1'),
('ukgc-scraper-1', DATE_SUB(NOW(), INTERVAL 23 HOUR), 'Scraper completed successfully', 'Info', 'run-1');

-- Insert sample content change record
INSERT INTO content_change_records (
    scraper_id,
    url,
    change_type,
    detected_at,
    significance,
    change_details,
    run_id
) VALUES (
    'ukgc-scraper-1',
    'https://www.gamblingcommission.gov.uk/news/article/gambling-commission-announces-new-rules-to-stamp-out-irresponsible-vip-customer-practices',
    'Modification',
    DATE_SUB(NOW(), INTERVAL 23 HOUR),
    75,
    'Content updated with new regulatory information',
    'run-1'
);

-- Insert sample processed document
INSERT INTO processed_documents (
    id,
    scraper_id,
    url,
    title,
    document_type,
    processed_at,
    content_size_bytes,
    run_id
) VALUES (
    'doc-1',
    'ukgc-scraper-1',
    'https://www.gamblingcommission.gov.uk/news/article/gambling-commission-announces-new-rules-to-stamp-out-irresponsible-vip-customer-practices',
    'Gambling Commission announces new rules to stamp out irresponsible VIP customer practices',
    'HTML',
    DATE_SUB(NOW(), INTERVAL 23 HOUR),
    15240,
    'run-1'
);

-- Insert sample document metadata
INSERT INTO document_metadata (document_id, meta_key, meta_value) VALUES 
('doc-1', 'author', 'Gambling Commission'),
('doc-1', 'published_date', '2023-04-15'),
('doc-1', 'category', 'Regulatory Update');

-- Insert sample scraper metrics
INSERT INTO scraper_metrics (scraper_id, metric_name, metric_value, timestamp) VALUES 
('ukgc-scraper-1', 'average_request_time_ms', 245.6, DATE_SUB(NOW(), INTERVAL 23 HOUR)),
('ukgc-scraper-1', 'memory_usage_mb', 128.4, DATE_SUB(NOW(), INTERVAL 23 HOUR)),
('ukgc-scraper-1', 'cpu_usage_percent', 12.5, DATE_SUB(NOW(), INTERVAL 23 HOUR));

-- Insert sample pipeline metrics
INSERT INTO pipeline_metrics (
    scraper_id,
    processing_items,
    queued_items,
    completed_items,
    failed_items,
    average_processing_time_ms,
    timestamp
) VALUES (
    'ukgc-scraper-1',
    0,
    0,
    120,
    0,
    245.6,
    DATE_SUB(NOW(), INTERVAL 23 HOUR)
);

-- Insert a sample schedule for the UKGC scraper
INSERT INTO scraper_schedules (
    scraper_id,
    name,
    cron_expression,
    is_active,
    last_run,
    next_run
) VALUES (
    'ukgc-scraper-1',
    'Daily UKGC Scan',
    '0 0 * * *',  -- Run at midnight every day
    TRUE,
    DATE_SUB(NOW(), INTERVAL 1 DAY),
    DATE_ADD(NOW(), INTERVAL 1 DAY)
);
