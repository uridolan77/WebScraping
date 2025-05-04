using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Resilience;

namespace WebScraper.Processing
{
    /// <summary>
    /// Extracts content from HTML in a streaming fashion to reduce memory usage
    /// </summary>
    public class StreamingContentExtractor
    {
        private readonly ILogger _logger;
        private readonly HtmlStreamParser _streamParser;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Creates a new instance of the StreamingContentExtractor
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public StreamingContentExtractor(ILogger logger)
        {
            _logger = logger;
            _streamParser = new HtmlStreamParser();
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        /// <summary>
        /// Extracts text content from a URL in a streaming fashion
        /// </summary>
        /// <param name="url">URL to extract content from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of text blocks</returns>
        public async IAsyncEnumerable<string> StreamTextContentAsync(
            string url,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Get the response first
            HttpResponseMessage response = await GetResponseAsync(url, cancellationToken);

            // If no response or not HTML, yield break
            if (response == null)
            {
                yield break;
            }

            // Check if the content is HTML
            var contentType = response.Content.Headers.ContentType?.MediaType;
            if (contentType == null || !contentType.Contains("html"))
            {
                _logger.LogWarning($"Content type {contentType} is not HTML, cannot extract text");
                response.Dispose();
                yield break;
            }

            // Process the response
            using (response)
            {
                // Get the stream
                Stream stream;
                try
                {
                    stream = await response.Content.ReadAsStreamAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error reading response stream for URL: {url}");
                    throw new ContentExtractionException(url, $"Error reading response stream: {ex.Message}", ex);
                }

                // Create the reader
                using var reader = new StreamReader(stream);

                // Get the text blocks
                IAsyncEnumerable<string> textBlocks;
                try
                {
                    textBlocks = _streamParser.ParseStreamAsync(reader, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error initializing stream parser for URL: {url}");
                    throw new ContentExtractionException(url, $"Error initializing stream parser: {ex.Message}", ex);
                }

                // Return the text blocks
                await foreach (var textBlock in textBlocks)
                {
                    yield return textBlock;
                }
            }
        }

        /// <summary>
        /// Gets the HTTP response for a URL
        /// </summary>
        private async Task<HttpResponseMessage> GetResponseAsync(string url, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _httpClient.GetAsync(
                    url,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                response.EnsureSuccessStatusCode();
                return response;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, $"HTTP request failed for URL: {url}");
                throw new RequestFailedException(url, ex.StatusCode, $"HTTP request failed: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogError(ex, $"Request timed out for URL: {url}");
                throw new RequestFailedException(url, null, $"Request timed out: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error streaming content from URL: {url}");
                throw new ContentExtractionException(url, $"Error streaming content: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts text content from HTML in a streaming fashion
        /// </summary>
        /// <param name="html">HTML content</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of text blocks</returns>
        public async IAsyncEnumerable<string> StreamTextContentFromHtmlAsync(
            string html,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Validate input
            if (string.IsNullOrEmpty(html))
            {
                _logger.LogWarning("Empty HTML content provided");
                yield break;
            }

            // Get the text blocks
            IAsyncEnumerable<string> textBlocks = GetTextBlocksFromHtml(html, cancellationToken);

            // Return the text blocks
            await foreach (var textBlock in textBlocks)
            {
                yield return textBlock;
            }
        }

        /// <summary>
        /// Gets text blocks from HTML content
        /// </summary>
        private IAsyncEnumerable<string> GetTextBlocksFromHtml(string html, CancellationToken cancellationToken)
        {
            // Create the reader
            StringReader reader;
            try
            {
                reader = new StringReader(html);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reader for HTML content");
                throw new ContentExtractionException("html-content", $"Error creating reader: {ex.Message}", ex);
            }

            // Get the text blocks
            try
            {
                return _streamParser.ParseStreamAsync(reader, cancellationToken);
            }
            catch (Exception ex)
            {
                reader.Dispose();
                _logger.LogError(ex, "Error initializing HTML stream parser");
                throw new ContentExtractionException("html-content", $"Error initializing parser: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Extracts all text content from a URL and returns as a single string
        /// </summary>
        /// <param name="url">URL to extract content from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Extracted text content</returns>
        public async Task<string> ExtractAllTextContentAsync(
            string url,
            CancellationToken cancellationToken = default)
        {
            var textBlocks = new List<string>();

            await foreach (var block in StreamTextContentAsync(url, cancellationToken))
            {
                textBlocks.Add(block);
            }

            return string.Join(Environment.NewLine + Environment.NewLine, textBlocks);
        }
    }
}
