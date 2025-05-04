using System;
using System.Net;

namespace WebScraper.Resilience
{
    /// <summary>
    /// Base exception for all WebScraper-specific exceptions
    /// </summary>
    public class WebScraperException : Exception
    {
        /// <summary>
        /// Creates a new WebScraperException
        /// </summary>
        public WebScraperException() : base() { }

        /// <summary>
        /// Creates a new WebScraperException with a message
        /// </summary>
        public WebScraperException(string message) : base(message) { }

        /// <summary>
        /// Creates a new WebScraperException with a message and inner exception
        /// </summary>
        public WebScraperException(string message, Exception innerException) 
            : base(message, innerException) { }
    }

    /// <summary>
    /// Exception thrown when a rate limit is exceeded
    /// </summary>
    public class RateLimitException : WebScraperException
    {
        /// <summary>
        /// Domain that rate limited the request
        /// </summary>
        public string Domain { get; }

        /// <summary>
        /// Time to wait before retrying
        /// </summary>
        public TimeSpan RetryAfter { get; }

        /// <summary>
        /// Creates a new RateLimitException
        /// </summary>
        /// <param name="domain">Domain that rate limited the request</param>
        /// <param name="retryAfter">Time to wait before retrying</param>
        /// <param name="message">Exception message</param>
        public RateLimitException(string domain, TimeSpan retryAfter, string message = null)
            : base(message ?? $"Rate limited by {domain}, retry after {retryAfter}")
        {
            Domain = domain;
            RetryAfter = retryAfter;
        }
    }

    /// <summary>
    /// Exception thrown when content extraction fails
    /// </summary>
    public class ContentExtractionException : WebScraperException
    {
        /// <summary>
        /// URL that failed extraction
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Creates a new ContentExtractionException
        /// </summary>
        /// <param name="url">URL that failed extraction</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public ContentExtractionException(string url, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Url = url;
        }
    }

    /// <summary>
    /// Exception thrown when a request fails
    /// </summary>
    public class RequestFailedException : WebScraperException
    {
        /// <summary>
        /// URL that failed
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// HTTP status code
        /// </summary>
        public HttpStatusCode? StatusCode { get; }

        /// <summary>
        /// Creates a new RequestFailedException
        /// </summary>
        /// <param name="url">URL that failed</param>
        /// <param name="statusCode">HTTP status code</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public RequestFailedException(string url, HttpStatusCode? statusCode, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Url = url;
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Exception thrown when a document processing fails
    /// </summary>
    public class DocumentProcessingException : WebScraperException
    {
        /// <summary>
        /// URL of the document
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Type of document
        /// </summary>
        public string DocumentType { get; }

        /// <summary>
        /// Creates a new DocumentProcessingException
        /// </summary>
        /// <param name="url">URL of the document</param>
        /// <param name="documentType">Type of document</param>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public DocumentProcessingException(string url, string documentType, string message, Exception innerException = null)
            : base(message, innerException)
        {
            Url = url;
            DocumentType = documentType;
        }
    }

    /// <summary>
    /// Exception thrown when a scraper configuration is invalid
    /// </summary>
    public class ConfigurationException : WebScraperException
    {
        /// <summary>
        /// Creates a new ConfigurationException
        /// </summary>
        /// <param name="message">Exception message</param>
        public ConfigurationException(string message) : base(message) { }
    }
}
