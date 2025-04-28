// src/utils/helpers.js

/**
 * Generate a unique ID
 * @returns {string} A unique ID
 */
export const generateId = () => {
  return Math.random().toString(36).substring(2, 15) +
         Math.random().toString(36).substring(2, 15);
};

/**
 * Creates a debounced function that delays invoking func until after wait milliseconds have elapsed
 * since the last time the debounced function was invoked.
 *
 * @param {Function} func - The function to debounce
 * @param {number} wait - The number of milliseconds to delay
 * @param {boolean} immediate - Whether to invoke the function on the leading edge instead of the trailing edge
 * @returns {Function} The debounced function
 */
export const debounce = (func, wait = 300, immediate = false) => {
  let timeout;

  return function executedFunction(...args) {
    const context = this;

    const later = () => {
      timeout = null;
      if (!immediate) func.apply(context, args);
    };

    const callNow = immediate && !timeout;

    clearTimeout(timeout);

    timeout = setTimeout(later, wait);

    if (callNow) func.apply(context, args);
  };
};

/**
 * Creates a throttled function that only invokes func at most once per every wait milliseconds.
 *
 * @param {Function} func - The function to throttle
 * @param {number} wait - The number of milliseconds to throttle invocations to
 * @returns {Function} The throttled function
 */
export const throttle = (func, wait = 300) => {
  let inThrottle;
  let lastFunc;
  let lastTime;

  return function() {
    const context = this;
    const args = arguments;

    if (!inThrottle) {
      func.apply(context, args);
      lastTime = Date.now();
      inThrottle = true;
    } else {
      clearTimeout(lastFunc);
      lastFunc = setTimeout(function() {
        if (Date.now() - lastTime >= wait) {
          func.apply(context, args);
          lastTime = Date.now();
        }
      }, Math.max(wait - (Date.now() - lastTime), 0));
    }
  };
};

/**
 * Group an array of objects by a key
 * @param {Array} array - The array to group
 * @param {string} key - The key to group by
 * @returns {Object} The grouped object
 */
export const groupBy = (array, key) => {
  return array.reduce((result, item) => {
    const groupKey = item[key];
    if (!result[groupKey]) {
      result[groupKey] = [];
    }
    result[groupKey].push(item);
    return result;
  }, {});
};

/**
 * Sort an array of objects by a key
 * @param {Array} array - The array to sort
 * @param {string} key - The key to sort by
 * @param {boolean} ascending - Whether to sort in ascending order
 * @returns {Array} The sorted array
 */
export const sortBy = (array, key, ascending = true) => {
  return [...array].sort((a, b) => {
    if (a[key] < b[key]) return ascending ? -1 : 1;
    if (a[key] > b[key]) return ascending ? 1 : -1;
    return 0;
  });
};

/**
 * Filter an array of objects by a search term
 * @param {Array} array - The array to filter
 * @param {string} searchTerm - The search term
 * @param {Array} keys - The keys to search in
 * @returns {Array} The filtered array
 */
export const filterBySearchTerm = (array, searchTerm, keys) => {
  if (!searchTerm) return array;

  const term = searchTerm.toLowerCase();

  return array.filter(item => {
    return keys.some(key => {
      const value = item[key];
      if (value === null || value === undefined) return false;
      return value.toString().toLowerCase().includes(term);
    });
  });
};

/**
 * Deep clone an object
 * @param {Object} obj - The object to clone
 * @returns {Object} The cloned object
 */
export const deepClone = (obj) => {
  return JSON.parse(JSON.stringify(obj));
};

/**
 * Check if two objects are equal
 * @param {Object} obj1 - The first object
 * @param {Object} obj2 - The second object
 * @returns {boolean} Whether the objects are equal
 */
export const areObjectsEqual = (obj1, obj2) => {
  return JSON.stringify(obj1) === JSON.stringify(obj2);
};

/**
 * Extract domain from a URL
 * @param {string} url - The URL to extract from
 * @returns {string} The domain
 */
export const extractDomain = (url) => {
  if (!url) return '';

  try {
    const urlObj = new URL(url);
    return urlObj.hostname;
  } catch (error) {
    console.error('Error extracting domain:', error);
    return '';
  }
};

/**
 * Get a color based on a status
 * @param {string} status - The status
 * @returns {string} The color
 */
export const getStatusColor = (status) => {
  const statusMap = {
    running: 'success',
    completed: 'success',
    stopped: 'warning',
    failed: 'error',
    idle: 'info',
    paused: 'warning',
    error: 'error'
  };

  return statusMap[status.toLowerCase()] || 'default';
};
