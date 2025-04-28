// src/utils/urlUtils.js

/**
 * Extracts the domain from a URL
 * @param {string} url - The URL to extract the domain from
 * @returns {string|null} The domain or null if the URL is invalid
 */
export const extractDomain = (url) => {
  try {
    const urlObj = new URL(url);
    return urlObj.hostname;
  } catch (error) {
    return null;
  }
};

/**
 * Extracts the base URL (protocol + domain) from a URL
 * @param {string} url - The URL to extract the base URL from
 * @returns {string|null} The base URL or null if the URL is invalid
 */
export const extractBaseUrl = (url) => {
  try {
    const urlObj = new URL(url);
    return `${urlObj.protocol}//${urlObj.hostname}`;
  } catch (error) {
    return null;
  }
};

/**
 * Joins a base URL and a relative path
 * @param {string} baseUrl - The base URL
 * @param {string} path - The relative path
 * @returns {string|null} The joined URL or null if the base URL is invalid
 */
export const joinUrl = (baseUrl, path) => {
  try {
    // Remove trailing slash from baseUrl if present
    const base = baseUrl.endsWith('/') ? baseUrl.slice(0, -1) : baseUrl;
    
    // Remove leading slash from path if present
    const relativePath = path.startsWith('/') ? path.slice(1) : path;
    
    return `${base}/${relativePath}`;
  } catch (error) {
    return null;
  }
};

/**
 * Checks if a URL is absolute (has a protocol)
 * @param {string} url - The URL to check
 * @returns {boolean} Whether the URL is absolute
 */
export const isAbsoluteUrl = (url) => {
  if (!url) return false;
  
  return /^(?:[a-z+]+:)?\/\//i.test(url);
};

/**
 * Checks if a URL is from the same domain as another URL
 * @param {string} url1 - The first URL
 * @param {string} url2 - The second URL
 * @returns {boolean} Whether the URLs are from the same domain
 */
export const isSameDomain = (url1, url2) => {
  const domain1 = extractDomain(url1);
  const domain2 = extractDomain(url2);
  
  return domain1 !== null && domain2 !== null && domain1 === domain2;
};

/**
 * Normalizes a URL by removing trailing slashes, fragments, etc.
 * @param {string} url - The URL to normalize
 * @returns {string|null} The normalized URL or null if the URL is invalid
 */
export const normalizeUrl = (url) => {
  try {
    const urlObj = new URL(url);
    
    // Remove fragment
    urlObj.hash = '';
    
    // Remove trailing slash if present
    let normalized = urlObj.toString();
    if (normalized.endsWith('/') && urlObj.pathname !== '/') {
      normalized = normalized.slice(0, -1);
    }
    
    return normalized;
  } catch (error) {
    return null;
  }
};

/**
 * Extracts query parameters from a URL
 * @param {string} url - The URL to extract query parameters from
 * @returns {Object|null} An object with the query parameters or null if the URL is invalid
 */
export const extractQueryParams = (url) => {
  try {
    const urlObj = new URL(url);
    const params = {};
    
    for (const [key, value] of urlObj.searchParams.entries()) {
      params[key] = value;
    }
    
    return params;
  } catch (error) {
    return null;
  }
};

/**
 * Adds query parameters to a URL
 * @param {string} url - The URL to add query parameters to
 * @param {Object} params - The query parameters to add
 * @returns {string|null} The URL with the added query parameters or null if the URL is invalid
 */
export const addQueryParams = (url, params) => {
  try {
    const urlObj = new URL(url);
    
    for (const [key, value] of Object.entries(params)) {
      urlObj.searchParams.append(key, value);
    }
    
    return urlObj.toString();
  } catch (error) {
    return null;
  }
};

/**
 * Extracts the path from a URL
 * @param {string} url - The URL to extract the path from
 * @returns {string|null} The path or null if the URL is invalid
 */
export const extractPath = (url) => {
  try {
    const urlObj = new URL(url);
    return urlObj.pathname;
  } catch (error) {
    return null;
  }
};

/**
 * Extracts the filename from a URL
 * @param {string} url - The URL to extract the filename from
 * @returns {string|null} The filename or null if the URL is invalid or has no filename
 */
export const extractFilename = (url) => {
  try {
    const urlObj = new URL(url);
    const path = urlObj.pathname;
    const parts = path.split('/');
    const filename = parts[parts.length - 1];
    
    return filename || null;
  } catch (error) {
    return null;
  }
};
