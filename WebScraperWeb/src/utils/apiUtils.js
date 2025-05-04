// src/utils/apiUtils.js

/**
 * Handle API response
 * @param {Promise} promise - The API promise
 * @returns {Promise<any>} The response data
 */
export const handleResponse = async (promise) => {
  try {
    const response = await promise;
    return response.data;
  } catch (error) {
    throw handleApiError(error);
  }
};

/**
 * Handle API error
 * @param {Error} error - The error object
 * @param {string} defaultMessage - Default error message
 * @returns {Error} Formatted error
 */
export const handleApiError = (error, defaultMessage = 'An error occurred') => {
  // Check if it's an axios error with a response
  if (error.response) {
    const { status, data } = error.response;
    
    // Format the error message based on the response
    let message = defaultMessage;
    
    if (data) {
      if (typeof data === 'string') {
        message = data;
      } else if (data.message) {
        message = data.message;
      } else if (data.error) {
        message = data.error;
      } else if (data.title) {
        message = data.title;
      }
    }
    
    // Create a formatted error object
    const formattedError = new Error(message);
    formattedError.status = status;
    formattedError.data = data;
    
    return formattedError;
  }
  
  // Network errors or other errors
  if (error.request) {
    return new Error('Network error - no response received');
  }
  
  // Default error handling
  return error;
};

/**
 * Get a user-friendly error message
 * @param {Error} error - The error object
 * @returns {string} User-friendly error message
 */
export const getUserFriendlyErrorMessage = (error) => {
  if (!error) {
    return 'An unknown error occurred';
  }
  
  // Check if it's our formatted error
  if (error.status) {
    switch (error.status) {
      case 400:
        return `Bad request: ${error.message}`;
      case 401:
        return 'You need to be logged in to perform this action';
      case 403:
        return 'You do not have permission to perform this action';
      case 404:
        return 'The requested resource was not found';
      case 500:
        return 'A server error occurred. Please try again later.';
      default:
        return error.message || `Error ${error.status}`;
    }
  }
  
  // Network errors
  if (error.message === 'Network Error') {
    return 'Unable to connect to the server. Please check your internet connection.';
  }
  
  // Default message
  return error.message || 'An unknown error occurred';
};
