-- Create the scraper_start_url table if it doesn't exist
CREATE TABLE IF NOT EXISTS `scraper_start_url` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `url` varchar(2048) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_scraper_start_url_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_scraper_start_url_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the content_extractor_selector table if it doesn't exist
CREATE TABLE IF NOT EXISTS `content_extractor_selector` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `selector` varchar(1024) NOT NULL,
  `is_exclude` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`id`),
  KEY `IX_content_extractor_selector_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_content_extractor_selector_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the keyword_alert table if it doesn't exist
CREATE TABLE IF NOT EXISTS `keyword_alert` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `keyword` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_keyword_alert_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_keyword_alert_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the webhook_trigger table if it doesn't exist
CREATE TABLE IF NOT EXISTS `webhook_trigger` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `trigger_name` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_webhook_trigger_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_webhook_trigger_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the domain_rate_limit table if it doesn't exist
CREATE TABLE IF NOT EXISTS `domain_rate_limit` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `domain` varchar(255) NOT NULL,
  `requests_per_minute` int NOT NULL DEFAULT '60',
  PRIMARY KEY (`id`),
  KEY `IX_domain_rate_limit_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_domain_rate_limit_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the proxy_configuration table if it doesn't exist
CREATE TABLE IF NOT EXISTS `proxy_configuration` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `username` varchar(255) NOT NULL,
  `password` varchar(255) NOT NULL,
  `proxy_url` varchar(1024) DEFAULT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `failure_count` int NOT NULL DEFAULT '0',
  `last_used` datetime DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_proxy_configuration_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_proxy_configuration_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Create the scraper_schedule table if it doesn't exist
CREATE TABLE IF NOT EXISTS `scraper_schedule` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraper_id` varchar(255) NOT NULL,
  `name` varchar(255) NOT NULL,
  `cron_expression` varchar(255) NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `last_run` datetime DEFAULT NULL,
  `next_run` datetime DEFAULT NULL,
  `max_runtime_minutes` int NOT NULL DEFAULT '60',
  `notification_email` varchar(255) NOT NULL,
  PRIMARY KEY (`id`),
  KEY `IX_scraper_schedule_scraper_id` (`scraper_id`),
  CONSTRAINT `FK_scraper_schedule_scraper_config_scraper_id` FOREIGN KEY (`scraper_id`) REFERENCES `scraper_config` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
