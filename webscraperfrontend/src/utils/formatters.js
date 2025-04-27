/**
 * Format a date string or timestamp into a human-readable date
 * @param {string|number} dateValue - Date string or timestamp
 * @param {object} options - Intl.DateTimeFormat options
 * @returns {string} - Formatted date string
 */
export const formatDate = (dateValue, options = {}) => {
  if (!dateValue) return '-';
  
  try {
    const date = new Date(dateValue);
    if (isNaN(date.getTime())) return '-';
    
    const defaultOptions = { 
      year: 'numeric', 
      month: 'short', 
      day: 'numeric',
      ...options
    };
    
    return new Intl.DateTimeFormat('en-US', defaultOptions).format(date);
  } catch (error) {
    console.error('Error formatting date:', error);
    return '-';
  }
};

/**
 * Format a date string or timestamp into a human-readable date and time
 * @param {string|number} dateValue - Date string or timestamp
 * @returns {string} - Formatted date and time string
 */
export const formatDateTime = (dateValue) => {
  if (!dateValue) return '-';
  
  return formatDate(dateValue, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  });
};

/**
 * Format a time duration in milliseconds to a human-readable string
 * @param {number} milliseconds - Duration in milliseconds
 * @returns {string} - Formatted duration string
 */
export const formatDuration = (milliseconds) => {
  if (!milliseconds || isNaN(milliseconds)) return '-';
  
  const seconds = Math.floor(milliseconds / 1000);
  
  if (seconds < 60) return `${seconds}s`;
  
  const minutes = Math.floor(seconds / 60);
  const remainingSeconds = seconds % 60;
  
  if (minutes < 60) {
    return `${minutes}m ${remainingSeconds}s`;
  }
  
  const hours = Math.floor(minutes / 60);
  const remainingMinutes = minutes % 60;
  
  if (hours < 24) {
    return `${hours}h ${remainingMinutes}m`;
  }
  
  const days = Math.floor(hours / 24);
  const remainingHours = hours % 24;
  
  return `${days}d ${remainingHours}h`;
};

/**
 * Format a number of bytes to a human-readable file size
 * @param {number} bytes - Number of bytes
 * @param {number} decimals - Number of decimal places to show
 * @returns {string} - Formatted file size string
 */
export const formatBytes = (bytes, decimals = 2) => {
  if (!bytes || isNaN(bytes) || bytes === 0) return '0 Bytes';
  
  const k = 1024;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(decimals)) + ' ' + sizes[i];
};

/**
 * Truncate a string to a maximum length with ellipsis
 * @param {string} str - String to truncate
 * @param {number} maxLength - Maximum length before truncation
 * @returns {string} - Truncated string
 */
export const truncateString = (str, maxLength = 50) => {
  if (!str) return '';
  
  if (str.length <= maxLength) return str;
  
  return str.substring(0, maxLength) + '...';
};

/**
 * Format a URL to be more readable by removing protocol and trailing slash
 * @param {string} url - URL to format
 * @returns {string} - Formatted URL
 */
export const formatUrl = (url) => {
  if (!url) return '';
  
  // Remove protocol
  let formatted = url.replace(/^(https?:\/\/)/, '');
  
  // Remove trailing slash
  formatted = formatted.replace(/\/$/, '');
  
  return formatted;
};