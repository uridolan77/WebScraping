// src/utils/formatters.js
import { format, formatDistance, formatRelative } from 'date-fns';

/**
 * Format a date to a readable string
 * @param {Date|string} date - The date to format
 * @param {string} formatString - The format string to use
 * @returns {string} The formatted date
 */
export const formatDate = (date, formatString = 'MMM d, yyyy') => {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return format(dateObj, formatString);
  } catch (error) {
    console.error('Error formatting date:', error);
    return 'Invalid Date';
  }
};

/**
 * Format a date to a relative time string (e.g., "2 hours ago")
 * @param {Date|string} date - The date to format
 * @param {Date} baseDate - The base date to compare against (defaults to now)
 * @returns {string} The relative time string
 */
export const formatRelativeTime = (date, baseDate = new Date()) => {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return formatDistance(dateObj, baseDate, { addSuffix: true });
  } catch (error) {
    console.error('Error formatting relative time:', error);
    return 'Invalid Date';
  }
};

/**
 * Format a date to a relative date string (e.g., "yesterday at 2:30 PM")
 * @param {Date|string} date - The date to format
 * @param {Date} baseDate - The base date to compare against (defaults to now)
 * @returns {string} The relative date string
 */
export const formatRelativeDate = (date, baseDate = new Date()) => {
  if (!date) return 'N/A';
  
  try {
    const dateObj = typeof date === 'string' ? new Date(date) : date;
    return formatRelative(dateObj, baseDate);
  } catch (error) {
    console.error('Error formatting relative date:', error);
    return 'Invalid Date';
  }
};

/**
 * Format a number with commas as thousands separators
 * @param {number} number - The number to format
 * @returns {string} The formatted number
 */
export const formatNumber = (number) => {
  if (number === null || number === undefined) return 'N/A';
  
  try {
    return number.toLocaleString();
  } catch (error) {
    console.error('Error formatting number:', error);
    return 'Invalid Number';
  }
};

/**
 * Format a file size in bytes to a human-readable string
 * @param {number} bytes - The file size in bytes
 * @param {number} decimals - The number of decimal places to show
 * @returns {string} The formatted file size
 */
export const formatFileSize = (bytes, decimals = 2) => {
  if (bytes === 0) return '0 Bytes';
  if (!bytes) return 'N/A';
  
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
  
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  
  return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
};

/**
 * Format a duration in milliseconds to a human-readable string
 * @param {number} milliseconds - The duration in milliseconds
 * @returns {string} The formatted duration
 */
export const formatDuration = (milliseconds) => {
  if (!milliseconds && milliseconds !== 0) return 'N/A';
  
  const seconds = Math.floor(milliseconds / 1000);
  const minutes = Math.floor(seconds / 60);
  const hours = Math.floor(minutes / 60);
  
  if (hours > 0) {
    return `${hours}h ${minutes % 60}m ${seconds % 60}s`;
  } else if (minutes > 0) {
    return `${minutes}m ${seconds % 60}s`;
  } else {
    return `${seconds}s`;
  }
};

/**
 * Truncate a string to a maximum length and add an ellipsis if needed
 * @param {string} str - The string to truncate
 * @param {number} maxLength - The maximum length of the string
 * @returns {string} The truncated string
 */
export const truncateString = (str, maxLength = 50) => {
  if (!str) return '';
  
  if (str.length <= maxLength) return str;
  
  return str.substring(0, maxLength) + '...';
};
