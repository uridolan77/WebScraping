// src/utils/validators.js

/**
 * Validate a URL
 * @param {string} url - The URL to validate
 * @returns {boolean} Whether the URL is valid
 */
export const isValidUrl = (url) => {
  if (!url) return false;
  
  try {
    new URL(url);
    return true;
  } catch (error) {
    return false;
  }
};

/**
 * Validate an email address
 * @param {string} email - The email to validate
 * @returns {boolean} Whether the email is valid
 */
export const isValidEmail = (email) => {
  if (!email) return false;
  
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

/**
 * Validate that a string is not empty
 * @param {string} value - The string to validate
 * @returns {boolean} Whether the string is not empty
 */
export const isNotEmpty = (value) => {
  return value !== null && value !== undefined && value.trim() !== '';
};

/**
 * Validate that a number is within a range
 * @param {number} value - The number to validate
 * @param {number} min - The minimum value
 * @param {number} max - The maximum value
 * @returns {boolean} Whether the number is within the range
 */
export const isInRange = (value, min, max) => {
  if (value === null || value === undefined) return false;
  
  const numValue = Number(value);
  
  if (isNaN(numValue)) return false;
  
  return numValue >= min && numValue <= max;
};

/**
 * Validate a scraper configuration object
 * @param {Object} config - The scraper configuration to validate
 * @returns {Object} An object with validation results
 */
export const validateScraperConfig = (config) => {
  const errors = {};
  
  // Required fields
  if (!isNotEmpty(config.name)) {
    errors.name = 'Name is required';
  }
  
  if (!isNotEmpty(config.startUrl)) {
    errors.startUrl = 'Start URL is required';
  } else if (!isValidUrl(config.startUrl)) {
    errors.startUrl = 'Start URL must be a valid URL';
  }
  
  if (!isNotEmpty(config.baseUrl)) {
    errors.baseUrl = 'Base URL is required';
  } else if (!isValidUrl(config.baseUrl)) {
    errors.baseUrl = 'Base URL must be a valid URL';
  }
  
  // Numeric fields
  if (!isInRange(config.delayBetweenRequests, 0, 60000)) {
    errors.delayBetweenRequests = 'Delay must be between 0 and 60000 ms';
  }
  
  if (!isInRange(config.maxConcurrentRequests, 1, 20)) {
    errors.maxConcurrentRequests = 'Max concurrent requests must be between 1 and 20';
  }
  
  if (!isInRange(config.maxDepth, 1, 100)) {
    errors.maxDepth = 'Max depth must be between 1 and 100';
  }
  
  if (config.notificationEndpoint && !isValidUrl(config.notificationEndpoint)) {
    errors.notificationEndpoint = 'Notification endpoint must be a valid URL';
  }
  
  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};

/**
 * Validate a webhook configuration
 * @param {Object} config - The webhook configuration to validate
 * @returns {Object} An object with validation results
 */
export const validateWebhookConfig = (config) => {
  const errors = {};
  
  if (!isNotEmpty(config.url)) {
    errors.url = 'Webhook URL is required';
  } else if (!isValidUrl(config.url)) {
    errors.url = 'Webhook URL must be a valid URL';
  }
  
  if (!isNotEmpty(config.secret)) {
    errors.secret = 'Secret is required';
  }
  
  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};

/**
 * Validate a scheduled task configuration
 * @param {Object} config - The scheduled task configuration to validate
 * @returns {Object} An object with validation results
 */
export const validateScheduledTask = (config) => {
  const errors = {};
  
  if (!isNotEmpty(config.name)) {
    errors.name = 'Task name is required';
  }
  
  if (!config.scraperId) {
    errors.scraperId = 'Scraper is required';
  }
  
  if (!config.schedule) {
    errors.schedule = 'Schedule is required';
  }
  
  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};
