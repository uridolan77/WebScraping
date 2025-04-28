import {
  isValidUrl,
  isValidEmail,
  isNotEmpty,
  isInRange,
  isValidId,
  isValidFilePath,
  isPositiveInteger,
  isNonNegativeInteger,
  hasMinLength,
  hasMaxLength,
  validateScraperConfig,
  validateWebhookConfig,
  validateScheduledTask,
  isValidCronExpression
} from './validators';

describe('Validator Utilities', () => {
  describe('isValidUrl', () => {
    test('returns true for valid URLs', () => {
      expect(isValidUrl('https://example.com')).toBe(true);
      expect(isValidUrl('http://example.com/path')).toBe(true);
      expect(isValidUrl('https://sub.domain.example.co.uk/path?query=value')).toBe(true);
      expect(isValidUrl('http://localhost:3000')).toBe(true);
      expect(isValidUrl('ftp://ftp.example.com')).toBe(true);
    });
    
    test('returns false for invalid URLs', () => {
      expect(isValidUrl('')).toBe(false);
      expect(isValidUrl(null)).toBe(false);
      expect(isValidUrl(undefined)).toBe(false);
      expect(isValidUrl('example.com')).toBe(false); // Missing protocol
      expect(isValidUrl('http:/example.com')).toBe(false); // Malformed protocol
      expect(isValidUrl('http:///example.com')).toBe(false); // Extra slashes
    });
  });
  
  describe('isValidEmail', () => {
    test('returns true for valid email addresses', () => {
      expect(isValidEmail('user@example.com')).toBe(true);
      expect(isValidEmail('user.name@example.co.uk')).toBe(true);
      expect(isValidEmail('user+tag@example.com')).toBe(true);
      expect(isValidEmail('user-name@example.com')).toBe(true);
      expect(isValidEmail('user_name@example.com')).toBe(true);
      expect(isValidEmail('user123@example.com')).toBe(true);
    });
    
    test('returns false for invalid email addresses', () => {
      expect(isValidEmail('')).toBe(false);
      expect(isValidEmail(null)).toBe(false);
      expect(isValidEmail(undefined)).toBe(false);
      expect(isValidEmail('user@')).toBe(false);
      expect(isValidEmail('@example.com')).toBe(false);
      expect(isValidEmail('user@example')).toBe(false); // Missing TLD
      expect(isValidEmail('user@.com')).toBe(false); // Missing domain
      expect(isValidEmail('user@example..com')).toBe(false); // Double dot
      expect(isValidEmail('user name@example.com')).toBe(false); // Space in local part
    });
  });
  
  describe('isNotEmpty', () => {
    test('returns true for non-empty values', () => {
      expect(isNotEmpty('text')).toBe(true);
      expect(isNotEmpty([1, 2, 3])).toBe(true);
      expect(isNotEmpty({ key: 'value' })).toBe(true);
      expect(isNotEmpty(123)).toBe(true);
      expect(isNotEmpty(true)).toBe(true);
    });
    
    test('returns false for empty values', () => {
      expect(isNotEmpty('')).toBe(false);
      expect(isNotEmpty('   ')).toBe(false); // Whitespace only
      expect(isNotEmpty([])).toBe(false);
      expect(isNotEmpty({})).toBe(false);
      expect(isNotEmpty(null)).toBe(false);
      expect(isNotEmpty(undefined)).toBe(false);
    });
  });
  
  describe('isInRange', () => {
    test('returns true for values within range', () => {
      expect(isInRange(5, 1, 10)).toBe(true);
      expect(isInRange(1, 1, 10)).toBe(true); // Min boundary
      expect(isInRange(10, 1, 10)).toBe(true); // Max boundary
      expect(isInRange('5', 1, 10)).toBe(true); // String number
      expect(isInRange(5.5, 1, 10)).toBe(true); // Decimal
    });
    
    test('returns false for values outside range', () => {
      expect(isInRange(0, 1, 10)).toBe(false); // Below min
      expect(isInRange(11, 1, 10)).toBe(false); // Above max
      expect(isInRange('abc', 1, 10)).toBe(false); // Not a number
      expect(isInRange(null, 1, 10)).toBe(false);
      expect(isInRange(undefined, 1, 10)).toBe(false);
    });
  });
  
  describe('isValidId', () => {
    test('returns true for valid IDs', () => {
      expect(isValidId('valid-id')).toBe(true);
      expect(isValidId('valid_id')).toBe(true);
      expect(isValidId('validId123')).toBe(true);
      expect(isValidId('123')).toBe(true);
      expect(isValidId('a')).toBe(true); // Single character
    });
    
    test('returns false for invalid IDs', () => {
      expect(isValidId('')).toBe(false);
      expect(isValidId(null)).toBe(false);
      expect(isValidId(undefined)).toBe(false);
      expect(isValidId('invalid id')).toBe(false); // Contains space
      expect(isValidId('invalid.id')).toBe(false); // Contains dot
      expect(isValidId('invalid@id')).toBe(false); // Contains @
      expect(isValidId('invalid/id')).toBe(false); // Contains /
    });
  });
  
  describe('isValidFilePath', () => {
    test('returns true for valid file paths', () => {
      expect(isValidFilePath('path/to/file.txt')).toBe(true);
      expect(isValidFilePath('file.txt')).toBe(true);
      expect(isValidFilePath('path\\to\\file.txt')).toBe(true);
      expect(isValidFilePath('path/to/file with spaces.txt')).toBe(true);
      expect(isValidFilePath('path/to/file-with-hyphens.txt')).toBe(true);
    });
    
    test('returns false for invalid file paths', () => {
      expect(isValidFilePath('')).toBe(false);
      expect(isValidFilePath(null)).toBe(false);
      expect(isValidFilePath(undefined)).toBe(false);
      expect(isValidFilePath('path/to/file?.txt')).toBe(false); // Contains ?
      expect(isValidFilePath('path/to/file*.txt')).toBe(false); // Contains *
      expect(isValidFilePath('path/to/file<.txt')).toBe(false); // Contains <
      expect(isValidFilePath('path/to/file>.txt')).toBe(false); // Contains >
      expect(isValidFilePath('path/to/file:.txt')).toBe(false); // Contains :
      expect(isValidFilePath('path/to/file".txt')).toBe(false); // Contains "
      expect(isValidFilePath('path/to/file|.txt')).toBe(false); // Contains |
    });
  });
  
  describe('isPositiveInteger', () => {
    test('returns true for positive integers', () => {
      expect(isPositiveInteger(1)).toBe(true);
      expect(isPositiveInteger(100)).toBe(true);
      expect(isPositiveInteger('42')).toBe(true); // String number
    });
    
    test('returns false for non-positive integers', () => {
      expect(isPositiveInteger(0)).toBe(false); // Zero
      expect(isPositiveInteger(-1)).toBe(false); // Negative
      expect(isPositiveInteger(3.14)).toBe(false); // Decimal
      expect(isPositiveInteger('abc')).toBe(false); // Not a number
      expect(isPositiveInteger(null)).toBe(false);
      expect(isPositiveInteger(undefined)).toBe(false);
    });
  });
  
  describe('isNonNegativeInteger', () => {
    test('returns true for non-negative integers', () => {
      expect(isNonNegativeInteger(0)).toBe(true); // Zero
      expect(isNonNegativeInteger(1)).toBe(true);
      expect(isNonNegativeInteger(100)).toBe(true);
      expect(isNonNegativeInteger('42')).toBe(true); // String number
    });
    
    test('returns false for negative or non-integer values', () => {
      expect(isNonNegativeInteger(-1)).toBe(false); // Negative
      expect(isNonNegativeInteger(3.14)).toBe(false); // Decimal
      expect(isNonNegativeInteger('abc')).toBe(false); // Not a number
      expect(isNonNegativeInteger(null)).toBe(false);
      expect(isNonNegativeInteger(undefined)).toBe(false);
    });
  });
  
  describe('hasMinLength', () => {
    test('returns true for strings with at least the minimum length', () => {
      expect(hasMinLength('abc', 3)).toBe(true); // Exact minimum
      expect(hasMinLength('abcdef', 3)).toBe(true); // Longer than minimum
      expect(hasMinLength('123', 1)).toBe(true);
    });
    
    test('returns false for strings shorter than the minimum length', () => {
      expect(hasMinLength('ab', 3)).toBe(false);
      expect(hasMinLength('', 1)).toBe(false); // Empty string
      expect(hasMinLength(null, 1)).toBe(false);
      expect(hasMinLength(undefined, 1)).toBe(false);
    });
  });
  
  describe('hasMaxLength', () => {
    test('returns true for strings not exceeding the maximum length', () => {
      expect(hasMaxLength('abc', 3)).toBe(true); // Exact maximum
      expect(hasMaxLength('ab', 3)).toBe(true); // Shorter than maximum
      expect(hasMaxLength('', 3)).toBe(true); // Empty string
    });
    
    test('returns false for strings exceeding the maximum length', () => {
      expect(hasMaxLength('abcd', 3)).toBe(false);
      expect(hasMaxLength('123456', 5)).toBe(false);
    });
    
    test('handles null and undefined values', () => {
      expect(hasMaxLength(null, 3)).toBe(true); // Null is treated as empty
      expect(hasMaxLength(undefined, 3)).toBe(true); // Undefined is treated as empty
    });
  });
  
  describe('validateScraperConfig', () => {
    test('returns valid for a valid scraper configuration', () => {
      const config = {
        name: 'Test Scraper',
        startUrl: 'https://example.com',
        baseUrl: 'https://example.com',
        delayBetweenRequests: 1000,
        maxConcurrentRequests: 5,
        maxDepth: 10
      };
      
      const result = validateScraperConfig(config);
      expect(result.isValid).toBe(true);
      expect(result.errors).toEqual({});
    });
    
    test('returns errors for an invalid scraper configuration', () => {
      const config = {
        name: '', // Empty name
        startUrl: 'invalid-url', // Invalid URL
        baseUrl: 'https://example.com',
        delayBetweenRequests: 100000, // Out of range
        maxConcurrentRequests: 30, // Out of range
        maxDepth: 0 // Out of range
      };
      
      const result = validateScraperConfig(config);
      expect(result.isValid).toBe(false);
      expect(result.errors).toHaveProperty('name');
      expect(result.errors).toHaveProperty('startUrl');
      expect(result.errors).toHaveProperty('delayBetweenRequests');
      expect(result.errors).toHaveProperty('maxConcurrentRequests');
      expect(result.errors).toHaveProperty('maxDepth');
    });
  });
  
  describe('validateWebhookConfig', () => {
    test('returns valid for a valid webhook configuration', () => {
      const config = {
        url: 'https://example.com/webhook',
        secret: 'secretkey12345'
      };
      
      const result = validateWebhookConfig(config);
      expect(result.isValid).toBe(true);
      expect(result.errors).toEqual({});
    });
    
    test('returns errors for an invalid webhook configuration', () => {
      const config = {
        url: 'invalid-url', // Invalid URL
        secret: 'short', // Too short
        headers: {} // Empty object
      };
      
      const result = validateWebhookConfig(config);
      expect(result.isValid).toBe(false);
      expect(result.errors).toHaveProperty('url');
      expect(result.errors).toHaveProperty('secret');
      expect(result.errors).toHaveProperty('headers');
    });
  });
  
  describe('validateScheduledTask', () => {
    test('returns valid for a valid scheduled task configuration', () => {
      const config = {
        name: 'Daily Scrape',
        scraperId: 'test-scraper',
        schedule: '0 0 * * *', // Daily at midnight
        email: 'user@example.com',
        maxRuntime: 3600
      };
      
      const result = validateScheduledTask(config);
      expect(result.isValid).toBe(true);
      expect(result.errors).toEqual({});
    });
    
    test('returns errors for an invalid scheduled task configuration', () => {
      const config = {
        name: '', // Empty name
        scraperId: 'invalid id', // Invalid ID format
        schedule: 'invalid cron', // Invalid cron expression
        email: 'invalid-email', // Invalid email
        maxRuntime: -1 // Negative runtime
      };
      
      const result = validateScheduledTask(config);
      expect(result.isValid).toBe(false);
      expect(result.errors).toHaveProperty('name');
      expect(result.errors).toHaveProperty('scraperId');
      expect(result.errors).toHaveProperty('schedule');
      expect(result.errors).toHaveProperty('email');
      expect(result.errors).toHaveProperty('maxRuntime');
    });
  });
  
  describe('isValidCronExpression', () => {
    test('returns true for valid cron expressions', () => {
      expect(isValidCronExpression('* * * * *')).toBe(true); // Every minute
      expect(isValidCronExpression('0 0 * * *')).toBe(true); // Daily at midnight
      expect(isValidCronExpression('0 0 * * 0')).toBe(true); // Weekly on Sunday
      expect(isValidCronExpression('0 0 1 * *')).toBe(true); // Monthly on the 1st
      expect(isValidCronExpression('0 0 1 1 *')).toBe(true); // Yearly on Jan 1
      expect(isValidCronExpression('0 0 * * MON')).toBe(true); // Every Monday
      expect(isValidCronExpression('0 0 * JAN *')).toBe(true); // Every day in January
      expect(isValidCronExpression('0 0 1-15 * *')).toBe(true); // 1st-15th of every month
      expect(isValidCronExpression('0 0 */2 * *')).toBe(true); // Every other day
      expect(isValidCronExpression('0 0,12 * * *')).toBe(true); // Twice a day
      expect(isValidCronExpression('0 0 ? * *')).toBe(true); // Every day (? for day of month)
      expect(isValidCronExpression('0 0 * * ?')).toBe(true); // Every day (? for day of week)
      expect(isValidCronExpression('0 0 * * 1-5')).toBe(true); // Weekdays
      expect(isValidCronExpression('0 0 * * 6,0')).toBe(true); // Weekends
      expect(isValidCronExpression('0 0 15 * *')).toBe(true); // 15th of every month
      expect(isValidCronExpression('0 0 L * *')).toBe(false); // Last day of month (not supported in our implementation)
      expect(isValidCronExpression('0 0 * * L')).toBe(false); // Last day of week (not supported in our implementation)
      expect(isValidCronExpression('0 0 * * 1#1')).toBe(false); // First Monday (not supported in our implementation)
    });
    
    test('returns false for invalid cron expressions', () => {
      expect(isValidCronExpression('')).toBe(false); // Empty string
      expect(isValidCronExpression(null)).toBe(false);
      expect(isValidCronExpression(undefined)).toBe(false);
      expect(isValidCronExpression('* * *')).toBe(false); // Too few fields
      expect(isValidCronExpression('* * * * * * *')).toBe(false); // Too many fields
      expect(isValidCronExpression('60 * * * *')).toBe(false); // Invalid minute
      expect(isValidCronExpression('* 24 * * *')).toBe(false); // Invalid hour
      expect(isValidCronExpression('* * 32 * *')).toBe(false); // Invalid day of month
      expect(isValidCronExpression('* * * 13 *')).toBe(false); // Invalid month
      expect(isValidCronExpression('* * * * 7')).toBe(false); // Invalid day of week
      expect(isValidCronExpression('* * * ABC *')).toBe(false); // Invalid month name
      expect(isValidCronExpression('* * * * ABC')).toBe(false); // Invalid day name
      expect(isValidCronExpression('invalid')).toBe(false); // Not a cron expression
    });
  });
});
