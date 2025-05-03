-- SQL Script to fix the table name issue by creating views

USE webstraction_db;

-- Create a view for scraperstarturlentity that points to scraper_start_url
CREATE OR REPLACE VIEW scraperstarturlentity AS
SELECT * FROM scraper_start_url;

-- Create a view for contentextractorselectorentity that points to content_extractor_selector
CREATE OR REPLACE VIEW contentextractorselectorentity AS
SELECT * FROM content_extractor_selector;

-- Create a view for keywordalertentity that points to keyword_alert
CREATE OR REPLACE VIEW keywordalertentity AS
SELECT * FROM keyword_alert;

-- Create a view for webhooktriggerentity that points to webhook_trigger
CREATE OR REPLACE VIEW webhooktriggerentity AS
SELECT * FROM webhook_trigger;

-- Create a view for domainratelimitentity that points to domain_rate_limit
CREATE OR REPLACE VIEW domainratelimitentity AS
SELECT * FROM domain_rate_limit;

-- Create a view for proxyconfigurationentity that points to proxy_configuration
CREATE OR REPLACE VIEW proxyconfigurationentity AS
SELECT * FROM proxy_configuration;

-- Create a view for scraperscheduleentity that points to scraper_schedule
CREATE OR REPLACE VIEW scraperscheduleentity AS
SELECT * FROM scraper_schedule;
