import { useState, useCallback } from 'react';
import axios, { AxiosError, AxiosRequestConfig } from 'axios';
import { ApiError } from '../types';
import { getUserFriendlyErrorMessage } from '../utils/errorHandler';

interface UseApiOptions {
  onSuccess?: (data: any) => void;
  onError?: (error: ApiError) => void;
  initialData?: any;
}

/**
 * A custom hook for making API requests
 * @param options Options for the API hook
 * @returns API request methods and state
 */
export const useApi = (options: UseApiOptions = {}) => {
  const [data, setData] = useState<any>(options.initialData || null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<ApiError | null>(null);

  /**
   * Make a GET request
   * @param url The URL to request
   * @param config Axios request config
   * @returns The response data
   */
  const get = useCallback(
    async <T>(url: string, config?: AxiosRequestConfig): Promise<T | null> => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await axios.get<T>(url, config);
        const responseData = response.data;
        
        setData(responseData);
        options.onSuccess?.(responseData);
        
        return responseData;
      } catch (err) {
        const apiError = handleApiError(err);
        setError(apiError);
        options.onError?.(apiError);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [options]
  );

  /**
   * Make a POST request
   * @param url The URL to request
   * @param data The data to send
   * @param config Axios request config
   * @returns The response data
   */
  const post = useCallback(
    async <T>(url: string, data: any, config?: AxiosRequestConfig): Promise<T | null> => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await axios.post<T>(url, data, config);
        const responseData = response.data;
        
        setData(responseData);
        options.onSuccess?.(responseData);
        
        return responseData;
      } catch (err) {
        const apiError = handleApiError(err);
        setError(apiError);
        options.onError?.(apiError);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [options]
  );

  /**
   * Make a PUT request
   * @param url The URL to request
   * @param data The data to send
   * @param config Axios request config
   * @returns The response data
   */
  const put = useCallback(
    async <T>(url: string, data: any, config?: AxiosRequestConfig): Promise<T | null> => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await axios.put<T>(url, data, config);
        const responseData = response.data;
        
        setData(responseData);
        options.onSuccess?.(responseData);
        
        return responseData;
      } catch (err) {
        const apiError = handleApiError(err);
        setError(apiError);
        options.onError?.(apiError);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [options]
  );

  /**
   * Make a DELETE request
   * @param url The URL to request
   * @param config Axios request config
   * @returns The response data
   */
  const del = useCallback(
    async <T>(url: string, config?: AxiosRequestConfig): Promise<T | null> => {
      try {
        setLoading(true);
        setError(null);
        
        const response = await axios.delete<T>(url, config);
        const responseData = response.data;
        
        setData(responseData);
        options.onSuccess?.(responseData);
        
        return responseData;
      } catch (err) {
        const apiError = handleApiError(err);
        setError(apiError);
        options.onError?.(apiError);
        return null;
      } finally {
        setLoading(false);
      }
    },
    [options]
  );

  /**
   * Reset the hook state
   */
  const reset = useCallback(() => {
    setData(options.initialData || null);
    setLoading(false);
    setError(null);
  }, [options.initialData]);

  return {
    data,
    loading,
    error,
    get,
    post,
    put,
    delete: del,
    reset
  };
};

/**
 * Handle API errors
 * @param error The error to handle
 * @returns A formatted API error
 */
const handleApiError = (error: unknown): ApiError => {
  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError<any>;
    
    return {
      message: getUserFriendlyErrorMessage(axiosError),
      status: axiosError.response?.status || 500,
      errors: axiosError.response?.data?.errors
    };
  }
  
  return {
    message: error instanceof Error ? error.message : 'An unknown error occurred',
    status: 500
  };
};

export default useApi;
