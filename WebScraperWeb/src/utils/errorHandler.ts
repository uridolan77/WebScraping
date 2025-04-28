// src/utils/errorHandler.ts
import { AxiosError } from 'axios';

interface ApiErrorResponse {
  message: string;
  details: any;
  status: number | null;
  type: string;
}

/**
 * Handles API errors and returns a standardized error object
 * @param error - The error object from the API call
 * @param defaultMessage - Default message to show if no specific error message is available
 * @returns Standardized error object
 */
export const handleApiError = (error: any, defaultMessage = 'An error occurred'): ApiErrorResponse => {
  // Check if it's an Axios error with a response
  if (error.response) {
    const { status, data } = error.response;

    // Handle different status codes
    switch (status) {
      case 400:
        return {
          message: data.message || 'Invalid request',
          details: data.errors || data.detail || null,
          status,
          type: 'validation'
        };
      case 401:
        return {
          message: 'Authentication required',
          details: null,
          status,
          type: 'auth'
        };
      case 403:
        return {
          message: 'You do not have permission to perform this action',
          details: null,
          status,
          type: 'permission'
        };
      case 404:
        return {
          message: data.message || 'Resource not found',
          details: null,
          status,
          type: 'not_found'
        };
      case 409:
        return {
          message: data.message || 'Conflict with current state',
          details: null,
          status,
          type: 'conflict'
        };
      case 422:
        return {
          message: data.message || 'Validation error',
          details: data.errors || data.detail || null,
          status,
          type: 'validation'
        };
      case 429:
        return {
          message: 'Too many requests, please try again later',
          details: null,
          status,
          type: 'rate_limit'
        };
      case 500:
      case 502:
      case 503:
      case 504:
        return {
          message: 'Server error, please try again later',
          details: null,
          status,
          type: 'server'
        };
      default:
        return {
          message: data.message || defaultMessage,
          details: data.errors || data.detail || null,
          status,
          type: 'unknown'
        };
    }
  }

  // Handle network errors
  if (error.request && !error.response) {
    return {
      message: 'Network error, please check your connection',
      details: null,
      status: 0,
      type: 'network'
    };
  }

  // Handle other errors
  return {
    message: error.message || defaultMessage,
    details: null,
    status: null,
    type: 'unknown'
  };
};

interface ValidationError {
  field?: string;
  message?: string;
  [key: string]: any;
}

/**
 * Formats validation errors into a user-friendly object
 * @param errors - Validation errors object
 * @returns Formatted errors object
 */
export const formatValidationErrors = (errors: any): Record<string, string> => {
  if (!errors) return {};

  // If errors is already in the right format, return it
  if (typeof errors === 'object' && !Array.isArray(errors)) {
    return errors as Record<string, string>;
  }

  // If errors is an array of objects with field and message properties
  if (Array.isArray(errors)) {
    return errors.reduce<Record<string, string>>((acc, error: ValidationError) => {
      if (error.field && error.message) {
        acc[error.field] = error.message;
      }
      return acc;
    }, {});
  }

  // If we can't format the errors, return an empty object
  return {};
};

/**
 * Logs errors to the console and optionally to an error reporting service
 * @param error - The error object
 * @param context - The context where the error occurred
 * @param additionalData - Additional data to log
 */
export const logError = (error: any, context = '', additionalData: Record<string, any> = {}): void => {
  // Log to console
  console.error(`Error in ${context}:`, error);

  if (additionalData && Object.keys(additionalData).length > 0) {
    console.error('Additional data:', additionalData);
  }

  // Here you would integrate with an error reporting service like Sentry
  // Example:
  // if (window.Sentry) {
  //   window.Sentry.captureException(error, {
  //     tags: { context },
  //     extra: additionalData
  //   });
  // }
};

/**
 * Checks if an error is a network error
 * @param error - The error object
 * @returns Whether the error is a network error
 */
export const isNetworkError = (error: any): boolean => {
  return (
    error.message === 'Network Error' ||
    (error.request && !error.response) ||
    error.code === 'ECONNABORTED'
  );
};

/**
 * Checks if an error is an authentication error
 * @param error - The error object
 * @returns Whether the error is an authentication error
 */
export const isAuthError = (error: any): boolean => {
  return error.response && (error.response.status === 401 || error.response.status === 403);
};

/**
 * Gets a user-friendly error message
 * @param error - The error object
 * @param fallback - Fallback message if no specific message is available
 * @returns User-friendly error message
 */
export const getUserFriendlyErrorMessage = (error: any, fallback = 'An error occurred'): string => {
  if (!error) return fallback;

  // If it's already a string, return it
  if (typeof error === 'string') return error;

  // If it's an error object from our handleApiError function
  if (error.message) return error.message;

  // If it's an Axios error with a response
  if (error.response && error.response.data) {
    const { data } = error.response;
    return data.message || data.error || fallback;
  }

  // If it's a standard Error object
  if (error instanceof Error) return error.message;

  // Fallback
  return fallback;
};
