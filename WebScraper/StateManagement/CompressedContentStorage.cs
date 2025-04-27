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
                using var memoryStream = new MemoryStream();
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                using (var writer = new StreamWriter(gzipStream))
                {
                    await writer.WriteAsync(content);
                }

                return Convert.ToBase64String(memoryStream.ToArray());
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
                var compressedBytes = Convert.FromBase64String(compressedContent);
                
                using var memoryStream = new MemoryStream(compressedBytes);
                using var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress);
                using var reader = new StreamReader(gzipStream);
                
                return await reader.ReadToEndAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decompressing content - possibly not compressed data");
                return compressedContent; // Return the original content in case of error
            }
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
                    return;
                    
                // Skip if already compressed
                if (Path.GetExtension(filePath).Equals(".gz", StringComparison.OrdinalIgnoreCase))
                    return;
                    
                // Read file content
                var content = await File.ReadAllTextAsync(filePath);
                
                // Only compress if it's worth it
                if (!ShouldCompress(content))
                    return;
                    
                // Compress content
                var compressedContent = await CompressContentAsync(content);
                
                // Write compressed content with .gz extension
                var compressedPath = filePath + ".gz";
                await File.WriteAllTextAsync(compressedPath, compressedContent);
                
                // Delete original file if compression was successful
                if (File.Exists(compressedPath))
                    File.Delete(filePath);
                    
                _logger.LogInformation($"Successfully compressed {filePath}");
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
                if (!File.Exists(compressedPath) || !compressedPath.EndsWith(".gz", StringComparison.OrdinalIgnoreCase))
                    return compressedPath;
                    
                // Read compressed content
                var compressedContent = await File.ReadAllTextAsync(compressedPath);
                
                // Decompress content
                var content = await DecompressContentAsync(compressedContent);
                
                // Write decompressed content to original filename without .gz extension
                var originalPath = compressedPath.Substring(0, compressedPath.Length - 3);
                await File.WriteAllTextAsync(originalPath, content);
                
                _logger.LogInformation($"Successfully decompressed {compressedPath}");
                
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