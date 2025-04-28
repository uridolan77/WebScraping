import {
  handleApiError,
  formatValidationErrors,
  isNetworkError,
  isAuthError,
  getUserFriendlyErrorMessage
} from './errorHandler';

describe('Error Handler Utilities', () => {
  describe('handleApiError', () => {
    test('handles Axios error with 400 status', () => {
      const error = {
        response: {
          status: 400,
          data: {
            message: 'Invalid request data',
            errors: { name: 'Name is required' }
          }
        }
      };
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Invalid request data',
        details: { name: 'Name is required' },
        status: 400,
        type: 'validation'
      });
    });
    
    test('handles Axios error with 401 status', () => {
      const error = {
        response: {
          status: 401,
          data: {}
        }
      };
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Authentication required',
        details: null,
        status: 401,
        type: 'auth'
      });
    });
    
    test('handles Axios error with 404 status', () => {
      const error = {
        response: {
          status: 404,
          data: { message: 'Scraper not found' }
        }
      };
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Scraper not found',
        details: null,
        status: 404,
        type: 'not_found'
      });
    });
    
    test('handles Axios error with 500 status', () => {
      const error = {
        response: {
          status: 500,
          data: {}
        }
      };
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Server error, please try again later',
        details: null,
        status: 500,
        type: 'server'
      });
    });
    
    test('handles network error', () => {
      const error = {
        request: {},
        response: null
      };
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Network error, please check your connection',
        details: null,
        status: 0,
        type: 'network'
      });
    });
    
    test('handles generic error', () => {
      const error = new Error('Something went wrong');
      
      const result = handleApiError(error);
      
      expect(result).toEqual({
        message: 'Something went wrong',
        details: null,
        status: null,
        type: 'unknown'
      });
    });
    
    test('uses default message when no specific message is available', () => {
      const error = {};
      const defaultMessage = 'Custom default message';
      
      const result = handleApiError(error, defaultMessage);
      
      expect(result.message).toBe(defaultMessage);
    });
  });
  
  describe('formatValidationErrors', () => {
    test('returns the object as is if it is already in the right format', () => {
      const errors = {
        name: 'Name is required',
        email: 'Invalid email format'
      };
      
      const result = formatValidationErrors(errors);
      
      expect(result).toEqual(errors);
    });
    
    test('formats array of errors with field and message properties', () => {
      const errors = [
        { field: 'name', message: 'Name is required' },
        { field: 'email', message: 'Invalid email format' }
      ];
      
      const result = formatValidationErrors(errors);
      
      expect(result).toEqual({
        name: 'Name is required',
        email: 'Invalid email format'
      });
    });
    
    test('returns empty object for null or undefined input', () => {
      expect(formatValidationErrors(null)).toEqual({});
      expect(formatValidationErrors(undefined)).toEqual({});
    });
    
    test('returns empty object for unrecognized format', () => {
      expect(formatValidationErrors('Invalid input')).toEqual({});
      expect(formatValidationErrors([1, 2, 3])).toEqual({});
    });
  });
  
  describe('isNetworkError', () => {
    test('identifies network error by message', () => {
      const error = new Error('Network Error');
      expect(isNetworkError(error)).toBe(true);
    });
    
    test('identifies network error by request without response', () => {
      const error = { request: {}, response: null };
      expect(isNetworkError(error)).toBe(true);
    });
    
    test('identifies network error by ECONNABORTED code', () => {
      const error = { code: 'ECONNABORTED' };
      expect(isNetworkError(error)).toBe(true);
    });
    
    test('returns false for non-network errors', () => {
      const error = new Error('Some other error');
      expect(isNetworkError(error)).toBe(false);
      
      const axiosError = { response: { status: 500 } };
      expect(isNetworkError(axiosError)).toBe(false);
    });
  });
  
  describe('isAuthError', () => {
    test('identifies 401 status as auth error', () => {
      const error = { response: { status: 401 } };
      expect(isAuthError(error)).toBe(true);
    });
    
    test('identifies 403 status as auth error', () => {
      const error = { response: { status: 403 } };
      expect(isAuthError(error)).toBe(true);
    });
    
    test('returns false for non-auth errors', () => {
      const error = new Error('Some other error');
      expect(isAuthError(error)).toBe(false);
      
      const axiosError = { response: { status: 500 } };
      expect(isAuthError(axiosError)).toBe(false);
    });
  });
  
  describe('getUserFriendlyErrorMessage', () => {
    test('returns the error string if it is already a string', () => {
      const error = 'Something went wrong';
      expect(getUserFriendlyErrorMessage(error)).toBe(error);
    });
    
    test('returns the message property if available', () => {
      const error = { message: 'Error message' };
      expect(getUserFriendlyErrorMessage(error)).toBe('Error message');
    });
    
    test('extracts message from Axios error response', () => {
      const error = {
        response: {
          data: { message: 'Server error message' }
        }
      };
      expect(getUserFriendlyErrorMessage(error)).toBe('Server error message');
    });
    
    test('extracts error from Axios error response if message is not available', () => {
      const error = {
        response: {
          data: { error: 'Server error' }
        }
      };
      expect(getUserFriendlyErrorMessage(error)).toBe('Server error');
    });
    
    test('returns message from Error object', () => {
      const error = new Error('Error object message');
      expect(getUserFriendlyErrorMessage(error)).toBe('Error object message');
    });
    
    test('returns fallback message if no specific message is available', () => {
      const error = {};
      const fallback = 'Fallback message';
      expect(getUserFriendlyErrorMessage(error, fallback)).toBe(fallback);
    });
    
    test('returns default fallback if no fallback is provided', () => {
      const error = {};
      expect(getUserFriendlyErrorMessage(error)).toBe('An error occurred');
    });
    
    test('returns fallback for null or undefined input', () => {
      expect(getUserFriendlyErrorMessage(null, 'Fallback')).toBe('Fallback');
      expect(getUserFriendlyErrorMessage(undefined, 'Fallback')).toBe('Fallback');
    });
  });
});
