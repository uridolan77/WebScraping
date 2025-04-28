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

  // More comprehensive email regex pattern
  const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
  return emailRegex.test(email);
};

/**
 * Validate that a value is not empty (string, array, object)
 * @param {*} value - The value to validate
 * @returns {boolean} Whether the value is not empty
 */
export const isNotEmpty = (value) => {
  if (value === null || value === undefined) return false;

  if (typeof value === 'string') {
    return value.trim() !== '';
  }

  if (Array.isArray(value)) {
    return value.length > 0;
  }

  if (typeof value === 'object') {
    return Object.keys(value).length > 0;
  }

  return true;
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
  return !isNaN(numValue) && numValue >= min && numValue <= max;
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
 * Validates if a string is a valid ID (alphanumeric, hyphens, underscores)
 * @param {string} id - The ID to validate
 * @returns {boolean} Whether the ID is valid
 */
export const isValidId = (id) => {
  if (!id) return false;

  // Only allow letters, numbers, hyphens, and underscores
  const idRegex = /^[a-zA-Z0-9-_]+$/;
  return idRegex.test(id);
};

/**
 * Validates if a string is a valid file path
 * @param {string} path - The file path to validate
 * @returns {boolean} Whether the file path is valid
 */
export const isValidFilePath = (path) => {
  if (!path) return false;

  // Check for invalid characters in file paths
  const invalidChars = /[<>:"|?*]/;
  return !invalidChars.test(path);
};

/**
 * Validates if a value is a positive integer
 * @param {number|string} value - The value to validate
 * @returns {boolean} Whether the value is a positive integer
 */
export const isPositiveInteger = (value) => {
  if (value === null || value === undefined) return false;

  const numValue = Number(value);
  return !isNaN(numValue) && Number.isInteger(numValue) && numValue > 0;
};

/**
 * Validates if a value is a non-negative integer (zero or positive)
 * @param {number|string} value - The value to validate
 * @returns {boolean} Whether the value is a non-negative integer
 */
export const isNonNegativeInteger = (value) => {
  if (value === null || value === undefined) return false;

  const numValue = Number(value);
  return !isNaN(numValue) && Number.isInteger(numValue) && numValue >= 0;
};

/**
 * Validates if a string has a minimum length
 * @param {string} value - The string to validate
 * @param {number} minLength - The minimum length required
 * @returns {boolean} Whether the string meets the minimum length
 */
export const hasMinLength = (value, minLength) => {
  if (!value) return false;
  return String(value).length >= minLength;
};

/**
 * Validates if a string does not exceed a maximum length
 * @param {string} value - The string to validate
 * @param {number} maxLength - The maximum length allowed
 * @returns {boolean} Whether the string is within the maximum length
 */
export const hasMaxLength = (value, maxLength) => {
  if (!value) return true; // Empty strings are valid for max length checks
  return String(value).length <= maxLength;
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
  } else if (!hasMinLength(config.secret, 8)) {
    errors.secret = 'Secret must be at least 8 characters long';
  }

  if (config.headers && !isNotEmpty(config.headers)) {
    errors.headers = 'Headers must not be empty if provided';
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
  } else if (!hasMaxLength(config.name, 100)) {
    errors.name = 'Task name must be at most 100 characters';
  }

  if (!config.scraperId) {
    errors.scraperId = 'Scraper is required';
  } else if (!isValidId(config.scraperId)) {
    errors.scraperId = 'Invalid scraper ID format';
  }

  if (!config.schedule) {
    errors.schedule = 'Schedule is required';
  } else if (typeof config.schedule === 'string' && !isValidCronExpression(config.schedule)) {
    errors.schedule = 'Invalid cron expression format';
  }

  if (config.email && !isValidEmail(config.email)) {
    errors.email = 'Invalid email format';
  }

  if (config.maxRuntime && !isPositiveInteger(config.maxRuntime)) {
    errors.maxRuntime = 'Max runtime must be a positive integer';
  }

  return {
    isValid: Object.keys(errors).length === 0,
    errors
  };
};

/**
 * Validate a cron expression
 * @param {string} cron - The cron expression to validate
 * @returns {boolean} Whether the cron expression is valid
 */
export const isValidCronExpression = (cron) => {
  if (!cron) return false;

  // Basic cron expression validation (5 or 6 space-separated fields)
  const cronRegex = /^(\S+\s+){4,5}\S+$/;
  if (!cronRegex.test(cron)) return false;

  // Split into fields
  const fields = cron.split(/\s+/);

  // Check if we have 5 or 6 fields
  if (fields.length !== 5 && fields.length !== 6) return false;

  // Simple validation for common cron expressions
  // This is a simplified approach that accepts most valid cron expressions
  // but may not catch all edge cases
  const simpleValidators = [
    // Seconds/Minutes: 0-59
    (field) => /^(\*|[0-9]|[1-5][0-9]|(\*\/[0-9]+)|([0-9]+(-[0-9]+)?(\/[0-9]+)?)(,[0-9]+(-[0-9]+)?(\/[0-9]+)?)*|\?)$/.test(field),

    // Hours: 0-23
    (field) => /^(\*|[0-9]|1[0-9]|2[0-3]|(\*\/[0-9]+)|([0-9]+(-[0-9]+)?(\/[0-9]+)?)(,[0-9]+(-[0-9]+)?(\/[0-9]+)?)*|\?)$/.test(field),

    // Day of month: 1-31
    (field) => /^(\*|[1-9]|[12][0-9]|3[01]|(\*\/[0-9]+)|([0-9]+(-[0-9]+)?(\/[0-9]+)?)(,[0-9]+(-[0-9]+)?(\/[0-9]+)?)*|\?)$/.test(field),

    // Month: 1-12 or JAN-DEC
    (field) => /^(\*|[1-9]|1[0-2]|JAN|FEB|MAR|APR|MAY|JUN|JUL|AUG|SEP|OCT|NOV|DEC|(\*\/[0-9]+)|([0-9]+(-[0-9]+)?(\/[0-9]+)?)(,[0-9]+(-[0-9]+)?(\/[0-9]+)?)*|\?)$/.test(field),

    // Day of week: 0-6 or SUN-SAT
    (field) => /^(\*|[0-6]|SUN|MON|TUE|WED|THU|FRI|SAT|(\*\/[0-9]+)|([0-9]+(-[0-9]+)?(\/[0-9]+)?)(,[0-9]+(-[0-9]+)?(\/[0-9]+)?)*|\?)$/.test(field)
  ];

  // If we have 6 fields, add a validator for seconds (0-59)
  if (fields.length === 6) {
    // Use the same validator as for minutes
    simpleValidators.unshift(simpleValidators[0]);
  }

  // Validate each field
  for (let i = 0; i < fields.length; i++) {
    if (!simpleValidators[i](fields[i])) {
      return false;
    }
  }

  return true;
};
