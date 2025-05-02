const mysql = require('mysql2/promise');
const { v4: uuidv4 } = require('uuid');

async function createTestScraper() {
  try {
    console.log('Connecting to MySQL database...');
    
    const connection = await mysql.createConnection({
      host: 'localhost',
      user: 'root',
      password: 'Dt%g_9W3z0*!I',
      database: 'webstraction_db'
    });
    
    console.log('Connected to MySQL database successfully!');
    
    // Create a test scraper
    const scraperId = uuidv4();
    const now = new Date().toISOString().slice(0, 19).replace('T', ' ');
    
    const scraperConfig = {
      id: scraperId,
      name: 'UKGC',
      created_at: now,
      last_modified: now,
      last_run: null,
      run_count: 0,
      start_url: 'https://www.gamblingcommission.gov.uk/licensees-and-businesses',
      base_url: 'https://www.gamblingcommission.gov.uk',
      output_directory: 'UKGCData',
      delay_between_requests: 1000,
      max_concurrent_requests: 5,
      max_depth: 5,
      max_pages: 1000,
      follow_links: 1,
      follow_external_links: 0,
      respect_robots_txt: 1,
      auto_learn_header_footer: 1,
      learning_pages_count: 5,
      enable_change_detection: 1,
      track_content_versions: 1,
      max_versions_to_keep: 5,
      enable_adaptive_crawling: 1,
      priority_queue_size: 100,
      adjust_depth_based_on_quality: 1,
      enable_adaptive_rate_limiting: 1,
      min_delay_between_requests: 500,
      max_delay_between_requests: 5000,
      monitor_response_times: 1,
      max_requests_per_minute: 60,
      user_agent: 'Mozilla/5.0 WebScraper Bot',
      back_off_on_errors: 1,
      use_proxies: 0,
      proxy_rotation_strategy: 'RoundRobin',
      test_proxies_before_use: 1,
      max_proxy_failures_before_removal: 3,
      enable_continuous_monitoring: 0,
      monitoring_interval_minutes: 60,
      notify_on_changes: 1,
      notification_email: 'test@example.com',
      track_changes_history: 1,
      enable_regulatory_content_analysis: 1,
      track_regulatory_changes: 1,
      classify_regulatory_documents: 1,
      extract_structured_content: 1,
      process_pdf_documents: 1,
      monitor_high_impact_changes: 1,
      extract_metadata: 1,
      extract_structured_data: 1,
      custom_js_extractor: '',
      wait_for_selector: '',
      is_ukgc_website: 1,
      prioritize_enforcement_actions: 1,
      prioritize_lccp: 1,
      prioritize_aml: 1,
      notification_endpoint: '',
      webhook_enabled: 0,
      webhook_url: '',
      notify_on_content_changes: 1,
      notify_on_document_processed: 0,
      notify_on_scraper_status_change: 0,
      webhook_format: 'JSON',
      enable_content_compression: 0,
      compression_threshold_bytes: 102400,
      collect_detailed_metrics: 1,
      metrics_reporting_interval_seconds: 60,
      track_domain_metrics: 0,
      scraper_type: 'Standard'
    };
    
    // Build the SQL query
    const fields = Object.keys(scraperConfig).join(', ');
    const placeholders = Object.keys(scraperConfig).map(() => '?').join(', ');
    const values = Object.values(scraperConfig);
    
    const query = `INSERT INTO scraper_config (${fields}) VALUES (${placeholders})`;
    
    // Execute the query
    const [result] = await connection.execute(query, values);
    console.log('Scraper created successfully!', result);
    
    // Create a scraper status entry with the correct fields
    const statusQuery = `
      INSERT INTO scraper_status (
        scraper_id, is_running, start_time, end_time, elapsed_time, 
        urls_processed, urls_queued, documents_processed, has_errors, 
        message, last_status_update, last_update, last_monitor_check, last_error
      )
      VALUES (
        ?, 0, NULL, NULL, '00:00:00', 
        0, 0, 0, 0, 
        'Ready to run', ?, ?, NULL, ''
      )
    `;
    
    const [statusResult] = await connection.execute(statusQuery, [scraperId, now, now]);
    console.log('Scraper status created successfully!', statusResult);
    
    // Verify the scraper was created
    const [scrapers] = await connection.execute('SELECT id, name, start_url FROM scraper_config');
    console.log('\nScrapers in database:');
    scrapers.forEach(scraper => {
      console.log(`- ${scraper.id}: ${scraper.name} (${scraper.start_url})`);
    });
    
    await connection.end();
    console.log('\nConnection closed.');
    
    return scraperId;
  } catch (error) {
    console.error('Error:', error);
    return null;
  }
}

createTestScraper()
  .then(scraperId => {
    if (scraperId) {
      console.log(`Test scraper created with ID: ${scraperId}`);
      process.exit(0);
    } else {
      console.error('Failed to create test scraper');
      process.exit(1);
    }
  })
  .catch(err => {
    console.error('Unexpected error:', err);
    process.exit(1);
  });
