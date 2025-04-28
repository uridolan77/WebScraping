import {
  extractDomain,
  extractBaseUrl,
  joinUrl,
  isAbsoluteUrl,
  isSameDomain,
  normalizeUrl,
  extractQueryParams,
  addQueryParams,
  extractPath,
  extractFilename
} from './urlUtils';

describe('URL Utilities', () => {
  describe('extractDomain', () => {
    test('extracts domain from valid URLs', () => {
      expect(extractDomain('https://example.com')).toBe('example.com');
      expect(extractDomain('http://sub.example.com/path')).toBe('sub.example.com');
      expect(extractDomain('https://example.co.uk/path?query=value')).toBe('example.co.uk');
      expect(extractDomain('http://localhost:3000')).toBe('localhost');
    });
    
    test('returns null for invalid URLs', () => {
      expect(extractDomain('')).toBeNull();
      expect(extractDomain('invalid-url')).toBeNull();
      expect(extractDomain(null)).toBeNull();
      expect(extractDomain(undefined)).toBeNull();
    });
  });
  
  describe('extractBaseUrl', () => {
    test('extracts base URL from valid URLs', () => {
      expect(extractBaseUrl('https://example.com')).toBe('https://example.com');
      expect(extractBaseUrl('http://sub.example.com/path')).toBe('http://sub.example.com');
      expect(extractBaseUrl('https://example.co.uk/path?query=value')).toBe('https://example.co.uk');
      expect(extractBaseUrl('http://localhost:3000')).toBe('http://localhost');
    });
    
    test('returns null for invalid URLs', () => {
      expect(extractBaseUrl('')).toBeNull();
      expect(extractBaseUrl('invalid-url')).toBeNull();
      expect(extractBaseUrl(null)).toBeNull();
      expect(extractBaseUrl(undefined)).toBeNull();
    });
  });
  
  describe('joinUrl', () => {
    test('joins base URL and path correctly', () => {
      expect(joinUrl('https://example.com', 'path')).toBe('https://example.com/path');
      expect(joinUrl('https://example.com/', 'path')).toBe('https://example.com/path');
      expect(joinUrl('https://example.com', '/path')).toBe('https://example.com/path');
      expect(joinUrl('https://example.com/', '/path')).toBe('https://example.com/path');
      expect(joinUrl('https://example.com/base', 'path')).toBe('https://example.com/base/path');
    });
    
    test('handles empty or invalid inputs', () => {
      expect(joinUrl('https://example.com', '')).toBe('https://example.com/');
      expect(joinUrl('', 'path')).toBe('/path');
      expect(joinUrl('', '')).toBe('/');
    });
  });
  
  describe('isAbsoluteUrl', () => {
    test('returns true for absolute URLs', () => {
      expect(isAbsoluteUrl('https://example.com')).toBe(true);
      expect(isAbsoluteUrl('http://example.com')).toBe(true);
      expect(isAbsoluteUrl('ftp://example.com')).toBe(true);
      expect(isAbsoluteUrl('//example.com')).toBe(true); // Protocol-relative URL
    });
    
    test('returns false for relative URLs and invalid inputs', () => {
      expect(isAbsoluteUrl('/path')).toBe(false);
      expect(isAbsoluteUrl('path')).toBe(false);
      expect(isAbsoluteUrl('./path')).toBe(false);
      expect(isAbsoluteUrl('../path')).toBe(false);
      expect(isAbsoluteUrl('')).toBe(false);
      expect(isAbsoluteUrl(null)).toBe(false);
      expect(isAbsoluteUrl(undefined)).toBe(false);
    });
  });
  
  describe('isSameDomain', () => {
    test('returns true for URLs from the same domain', () => {
      expect(isSameDomain('https://example.com', 'https://example.com/path')).toBe(true);
      expect(isSameDomain('https://example.com/path1', 'http://example.com/path2')).toBe(true);
      expect(isSameDomain('https://example.com', 'https://example.com:8080')).toBe(true);
    });
    
    test('returns false for URLs from different domains', () => {
      expect(isSameDomain('https://example.com', 'https://example.org')).toBe(false);
      expect(isSameDomain('https://sub1.example.com', 'https://sub2.example.com')).toBe(false);
      expect(isSameDomain('https://example.com', 'https://www.example.com')).toBe(false);
    });
    
    test('returns false for invalid URLs', () => {
      expect(isSameDomain('https://example.com', 'invalid-url')).toBe(false);
      expect(isSameDomain('invalid-url', 'https://example.com')).toBe(false);
      expect(isSameDomain('', '')).toBe(false);
      expect(isSameDomain(null, null)).toBe(false);
    });
  });
  
  describe('normalizeUrl', () => {
    test('normalizes URLs correctly', () => {
      expect(normalizeUrl('https://example.com/')).toBe('https://example.com');
      expect(normalizeUrl('https://example.com/path/')).toBe('https://example.com/path');
      expect(normalizeUrl('https://example.com/path#fragment')).toBe('https://example.com/path');
      expect(normalizeUrl('https://example.com/path?query=value#fragment')).toBe('https://example.com/path?query=value');
      expect(normalizeUrl('https://example.com//')).toBe('https://example.com/');
    });
    
    test('returns null for invalid URLs', () => {
      expect(normalizeUrl('')).toBeNull();
      expect(normalizeUrl('invalid-url')).toBeNull();
      expect(normalizeUrl(null)).toBeNull();
      expect(normalizeUrl(undefined)).toBeNull();
    });
  });
  
  describe('extractQueryParams', () => {
    test('extracts query parameters correctly', () => {
      expect(extractQueryParams('https://example.com?param1=value1&param2=value2')).toEqual({
        param1: 'value1',
        param2: 'value2'
      });
      expect(extractQueryParams('https://example.com/path?param=value')).toEqual({
        param: 'value'
      });
      expect(extractQueryParams('https://example.com')).toEqual({});
      expect(extractQueryParams('https://example.com?')).toEqual({});
    });
    
    test('returns null for invalid URLs', () => {
      expect(extractQueryParams('')).toBeNull();
      expect(extractQueryParams('invalid-url')).toBeNull();
      expect(extractQueryParams(null)).toBeNull();
      expect(extractQueryParams(undefined)).toBeNull();
    });
  });
  
  describe('addQueryParams', () => {
    test('adds query parameters correctly', () => {
      expect(addQueryParams('https://example.com', { param: 'value' })).toBe('https://example.com/?param=value');
      expect(addQueryParams('https://example.com?existing=value', { param: 'value' })).toBe('https://example.com/?existing=value&param=value');
      expect(addQueryParams('https://example.com', {})).toBe('https://example.com/');
    });
    
    test('returns null for invalid URLs', () => {
      expect(addQueryParams('', { param: 'value' })).toBeNull();
      expect(addQueryParams('invalid-url', { param: 'value' })).toBeNull();
      expect(addQueryParams(null, { param: 'value' })).toBeNull();
      expect(addQueryParams(undefined, { param: 'value' })).toBeNull();
    });
  });
  
  describe('extractPath', () => {
    test('extracts path correctly', () => {
      expect(extractPath('https://example.com')).toBe('/');
      expect(extractPath('https://example.com/')).toBe('/');
      expect(extractPath('https://example.com/path')).toBe('/path');
      expect(extractPath('https://example.com/path/to/resource')).toBe('/path/to/resource');
      expect(extractPath('https://example.com/path?query=value')).toBe('/path');
      expect(extractPath('https://example.com/path#fragment')).toBe('/path');
    });
    
    test('returns null for invalid URLs', () => {
      expect(extractPath('')).toBeNull();
      expect(extractPath('invalid-url')).toBeNull();
      expect(extractPath(null)).toBeNull();
      expect(extractPath(undefined)).toBeNull();
    });
  });
  
  describe('extractFilename', () => {
    test('extracts filename correctly', () => {
      expect(extractFilename('https://example.com/file.txt')).toBe('file.txt');
      expect(extractFilename('https://example.com/path/to/file.txt')).toBe('file.txt');
      expect(extractFilename('https://example.com/file.txt?query=value')).toBe('file.txt');
      expect(extractFilename('https://example.com/file.txt#fragment')).toBe('file.txt');
    });
    
    test('returns null for URLs without a filename', () => {
      expect(extractFilename('https://example.com')).toBeNull();
      expect(extractFilename('https://example.com/')).toBeNull();
      expect(extractFilename('https://example.com/path/')).toBeNull();
    });
    
    test('returns null for invalid URLs', () => {
      expect(extractFilename('')).toBeNull();
      expect(extractFilename('invalid-url')).toBeNull();
      expect(extractFilename(null)).toBeNull();
      expect(extractFilename(undefined)).toBeNull();
    });
  });
});
