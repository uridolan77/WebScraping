using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Provides compression and decompression functionality for storing scraped content efficiently
    /// </summary>
    public class CompressedContentStorage
    {
        private readonly ILogger<CompressedContentStorage> _logger;

        public CompressedContentStorage(ILogger<CompressedContentStorage> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Compresses content using GZip and returns as a Base64-encoded string
        /// </summary>
        public async Task<string> CompressContentAsync(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            try
            {
                // Convert string to bytes first for more efficient compression
                byte[] contentBytes = Encoding.UTF8.GetBytes(content);

                using var outputMemoryStream = new MemoryStream();
                // Use a buffer size that's a multiple of 4096 for better performance
                using (var gzipStream = new GZipStream(outputMemoryStream, CompressionLevel.Optimal, true))
                {
                    await gzipStream.WriteAsync(contentBytes, 0, contentBytes.Length);
                }

                // Convert compressed bytes to Base64
                return Convert.ToBase64String(outputMemoryStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error compressing content");
                return content; // Return original content in case of error
            }
        }

        /// <summary>
        /// Decompresses Base64-encoded GZip content back to original string
        /// </summary>
        public async Task<string> DecompressContentAsync(string compressedContent)
        {
            if (string.IsNullOrEmpty(compressedContent))
                return compressedContent;

            try
            {
                // Try to detect if the content is actually compressed
                if (!IsBase64GzipCompressed(compressedContent))
                {
                    _logger.LogWarning("Content does not appear to be Base64-encoded GZip data");
                    return compressedContent;
                }

                // Convert from Base64 to bytes
                byte[] compressedBytes = Convert.FromBase64String(compressedContent);

                using var inputStream = new MemoryStream(compressedBytes);
                using var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);

                // Use a memory stream to collect the decompressed data
                using var resultStream = new MemoryStream();

                // Use a buffer for more efficient copying
                byte[] buffer = new byte[4096];
                int bytesRead;

                while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    await resultStream.WriteAsync(buffer, 0, bytesRead);
                }

                // Convert the decompressed bytes back to a string
                return Encoding.UTF8.GetString(resultStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decompressing content - possibly not compressed data");
                return compressedContent; // Return the original content in case of error
            }
        }

        /// <summary>
        /// Checks if the content is likely Base64-encoded GZip data
        /// </summary>
        private bool IsBase64GzipCompressed(string content)
        {
            try
            {
                // Check if it's valid Base64
                if (content.Length % 4 != 0 || !IsBase64String(content))
                    return false;

                // Try to decode the first few bytes to check for GZip header
                byte[] bytes = Convert.FromBase64String(content);

                // GZip header is at least 10 bytes
                if (bytes.Length < 10)
                    return false;

                // Check for GZip magic number (first two bytes should be 0x1F, 0x8B)
                return bytes[0] == 0x1F && bytes[1] == 0x8B;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Checks if a string is valid Base64
        /// </summary>
        private bool IsBase64String(string content)
        {
            // Quick check for Base64 characters only
            return content.All(c =>
                (c >= 'A' && c <= 'Z') ||
                (c >= 'a' && c <= 'z') ||
                (c >= '0' && c <= '9') ||
                c == '+' || c == '/' || c == '=');
        }

        /// <summary>
        /// Determines if content should be compressed based on size threshold
        /// </summary>
        public bool ShouldCompress(string content)
        {
            // Only compress content that's larger than 1 KB
            return content != null && content.Length > 1024;
        }

        /// <summary>
        /// Compresses a file and replaces it with the compressed version
        /// </summary>
        public async Task CompressFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    _logger.LogWarning($"File not found for compression: {filePath}");
                    return;
                }

                // Skip if already compressed
                if (Path.GetExtension(filePath).Equals(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug($"File is already compressed: {filePath}");
                    return;
                }

                // Get file info to check size
                var fileInfo = new FileInfo(filePath);

                // Only compress if file is larger than 1KB
                if (fileInfo.Length <= 1024)
                {
                    _logger.LogDebug($"File too small to compress: {filePath} ({fileInfo.Length} bytes)");
                    return;
                }

                // Use direct file-to-file compression for better performance
                var compressedPath = filePath + ".gz";

                using (var inputFileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                using (var outputFileStream = new FileStream(compressedPath, FileMode.Create))
                using (var gzipStream = new GZipStream(outputFileStream, CompressionLevel.Optimal))
                {
                    await inputFileStream.CopyToAsync(gzipStream);
                }

                // Check if compression was successful and actually saved space
                var compressedFileInfo = new FileInfo(compressedPath);

                if (compressedFileInfo.Exists && compressedFileInfo.Length < fileInfo.Length)
                {
                    // Delete original file if compression was successful and saved space
                    File.Delete(filePath);
                    _logger.LogInformation($"Successfully compressed {filePath} - Reduced from {fileInfo.Length} to {compressedFileInfo.Length} bytes ({(100 - (compressedFileInfo.Length * 100 / fileInfo.Length))}% reduction)");
                }
                else
                {
                    // If compression didn't save space, delete the compressed file
                    File.Delete(compressedPath);
                    _logger.LogInformation($"Compression didn't save space for {filePath} - keeping original");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error compressing file {filePath}");
            }
        }

        /// <summary>
        /// Decompresses a .gz file and returns the path to the decompressed file
        /// </summary>
        public async Task<string> DecompressFileAsync(string compressedPath)
        {
            try
            {
                if (!File.Exists(compressedPath))
                {
                    _logger.LogWarning($"File not found for decompression: {compressedPath}");
                    return compressedPath;
                }

                if (!compressedPath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogDebug($"File is not a .gz file: {compressedPath}");
                    return compressedPath;
                }

                // Get the original file path by removing the .gz extension
                var originalPath = compressedPath.Substring(0, compressedPath.Length - 3);

                // Use direct file-to-file decompression for better performance
                using (var compressedFileStream = new FileStream(compressedPath, FileMode.Open, FileAccess.Read))
                using (var gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
                using (var outputFileStream = new FileStream(originalPath, FileMode.Create))
                {
                    // Use a buffer for more efficient copying
                    byte[] buffer = new byte[4096];
                    int bytesRead;

                    while ((bytesRead = await gzipStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        await outputFileStream.WriteAsync(buffer, 0, bytesRead);
                    }
                }

                _logger.LogInformation($"Successfully decompressed {compressedPath} to {originalPath}");

                return originalPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error decompressing file {compressedPath}");
                return compressedPath; // Return original path in case of error
            }
        }
    }
}