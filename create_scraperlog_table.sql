-- Create the scraperlog table if it doesn't exist
CREATE TABLE IF NOT EXISTS `scraperlog` (
  `id` int NOT NULL AUTO_INCREMENT,
  `scraperId` varchar(255) NOT NULL,
  `timestamp` datetime NOT NULL,
  `logLevel` varchar(50) NOT NULL,
  `message` text NOT NULL,
  PRIMARY KEY (`id`),
  KEY `idx_scraperlog_scraperid` (`scraperId`),
  KEY `idx_scraperlog_timestamp` (`timestamp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
