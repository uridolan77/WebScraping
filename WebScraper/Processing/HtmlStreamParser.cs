using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HtmlAgilityPack;

namespace WebScraper.Processing
{
    /// <summary>
    /// Parses HTML content in a streaming fashion to reduce memory usage
    /// </summary>
    public class HtmlStreamParser
    {
        private readonly int _bufferSize;
        private readonly int _processingThreshold;

        /// <summary>
        /// Creates a new instance of the HtmlStreamParser
        /// </summary>
        /// <param name="bufferSize">Size of the read buffer in characters</param>
        /// <param name="processingThreshold">Minimum buffer size before processing</param>
        public HtmlStreamParser(int bufferSize = 4096, int processingThreshold = 8192)
        {
            _bufferSize = bufferSize;
            _processingThreshold = processingThreshold;
        }

        /// <summary>
        /// Parses HTML content from a stream and yields text blocks
        /// </summary>
        /// <param name="reader">TextReader to read HTML from</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of text blocks</returns>
        public async IAsyncEnumerable<string> ParseStreamAsync(
            TextReader reader,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var buffer = new StringBuilder();
            char[] readBuffer = new char[_bufferSize];
            int charsRead;

            while ((charsRead = await reader.ReadAsync(readBuffer, 0, readBuffer.Length)) > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                buffer.Append(readBuffer, 0, charsRead);

                // Process complete elements when we have enough data
                if (buffer.Length > _processingThreshold)
                {
                    foreach (var textBlock in ProcessBuffer(buffer))
                    {
                        yield return textBlock;
                    }
                }
            }

            // Process any remaining content
            foreach (var textBlock in ProcessBuffer(buffer, true))
            {
                yield return textBlock;
            }
        }

        /// <summary>
        /// Parses HTML content from a URL and yields text blocks
        /// </summary>
        /// <param name="url">URL to fetch and parse</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Async enumerable of text blocks</returns>
        public async IAsyncEnumerable<string> ParseUrlAsync(
            string url,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            using var client = new System.Net.Http.HttpClient();
            using var response = await client.GetAsync(
                url,
                System.Net.Http.HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            await foreach (var textBlock in ParseStreamAsync(reader, cancellationToken))
            {
                yield return textBlock;
            }
        }

        /// <summary>
        /// Process the buffer and extract complete HTML elements
        /// </summary>
        /// <param name="buffer">StringBuilder containing HTML</param>
        /// <param name="isComplete">Whether this is the final buffer</param>
        /// <returns>Enumerable of text blocks</returns>
        private IEnumerable<string> ProcessBuffer(StringBuilder buffer, bool isComplete = false)
        {
            if (buffer.Length == 0)
                yield break;

            // For a real implementation, we would use a proper HTML parser that can handle
            // streaming content. For this example, we'll use a simplified approach.

            // Create an HTML document from the buffer
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(buffer.ToString());

            // Extract text from specific elements
            var textElements = htmlDoc.DocumentNode.SelectNodes("//p|//h1|//h2|//h3|//h4|//h5|//h6|//li");
            if (textElements != null)
            {
                foreach (var element in textElements)
                {
                    // Skip empty elements
                    string text = element.InnerText.Trim();
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        yield return text;
                    }
                }
            }

            // If this is the final buffer, we're done
            if (isComplete)
            {
                buffer.Clear();
                yield break;
            }

            // For a real implementation, we would keep track of incomplete elements
            // and only clear the buffer up to the last complete element.
            // For this example, we'll just clear the buffer.
            buffer.Clear();
        }
    }
}
