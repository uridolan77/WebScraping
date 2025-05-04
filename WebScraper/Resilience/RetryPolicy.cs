using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScraper.Resilience
{
    /// <summary>
    /// Provides retry functionality with exponential backoff
    /// </summary>
    public static class RetryPolicy
    {
        /// <summary>
        /// Executes an action with retry and exponential backoff
        /// </summary>
        /// <typeparam name="T">Return type of the action</typeparam>
        /// <param name="action">Action to execute</param>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="shouldRetry">Function to determine if a retry should be attempted based on the exception</param>
        /// <param name="logger">Optional logger</param>
        /// <returns>Result of the action</returns>
        public static async Task<T> ExecuteWithRetryAsync<T>(
            Func<Task<T>> action,
            int maxRetries = 3,
            Func<Exception, int, bool> shouldRetry = null,
            ILogger logger = null)
        {
            shouldRetry ??= (ex, _) => !(ex is ArgumentException);
            
            int retryCount = 0;
            Exception lastException = null;
            
            while (retryCount <= maxRetries)
            {
                try
                {
                    if (retryCount > 0)
                    {
                        // Exponential backoff: 2^retryCount * 100ms
                        int delay = (int)Math.Pow(2, retryCount) * 100;
                        logger?.LogInformation($"Retrying after {delay}ms (Attempt {retryCount}/{maxRetries})");
                        await Task.Delay(delay);
                    }
                    
                    return await action();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    logger?.LogWarning(ex, $"Attempt {retryCount + 1}/{maxRetries + 1} failed");
                    
                    if (!shouldRetry(ex, retryCount) || retryCount >= maxRetries)
                        break;
                    
                    retryCount++;
                }
            }
            
            logger?.LogError(lastException, $"All {maxRetries + 1} attempts failed");
            throw lastException;
        }

        /// <summary>
        /// Executes an action with retry and exponential backoff
        /// </summary>
        /// <param name="action">Action to execute</param>
        /// <param name="maxRetries">Maximum number of retries</param>
        /// <param name="shouldRetry">Function to determine if a retry should be attempted based on the exception</param>
        /// <param name="logger">Optional logger</param>
        public static async Task ExecuteWithRetryAsync(
            Func<Task> action,
            int maxRetries = 3,
            Func<Exception, int, bool> shouldRetry = null,
            ILogger logger = null)
        {
            await ExecuteWithRetryAsync(
                async () => 
                {
                    await action();
                    return true;
                },
                maxRetries,
                shouldRetry,
                logger);
        }

        /// <summary>
        /// Creates a default retry predicate for HTTP requests
        /// </summary>
        /// <returns>A function that determines if a retry should be attempted</returns>
        public static Func<Exception, int, bool> DefaultHttpRetryPredicate()
        {
            return (ex, retryCount) =>
            {
                // Don't retry on argument exceptions or other client errors
                if (ex is ArgumentException)
                    return false;

                // Always retry on rate limit exceptions, but respect the retry-after header
                if (ex is RateLimitException rateLimitEx)
                {
                    // If retry-after is more than 30 seconds, don't retry
                    return rateLimitEx.RetryAfter.TotalSeconds <= 30;
                }

                // Retry on request failures that might be temporary
                if (ex is RequestFailedException requestEx)
                {
                    // Retry on 5xx errors or null status code (likely network error)
                    if (requestEx.StatusCode == null)
                        return true;

                    int statusCode = (int)requestEx.StatusCode;
                    return statusCode >= 500 && statusCode < 600;
                }

                // Retry on network-related exceptions
                if (ex is System.Net.Http.HttpRequestException)
                    return true;

                // Retry on timeouts
                if (ex is TimeoutException)
                    return true;

                // Retry on task canceled exceptions (might be due to timeouts)
                if (ex is TaskCanceledException)
                    return true;

                // Don't retry on other exceptions
                return false;
            };
        }
    }
}
