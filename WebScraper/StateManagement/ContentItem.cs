using System;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Represents content scraped from a website with state management support
    /// </summary>
    public class ContentItem
    {
        /// <summary>
        /// Gets or sets the URL the content was scraped from
        /// </summary>
        public string Url { get; set; }
        
        /// <summary>
        /// Gets or sets the title of the page
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Gets or sets the ID of the scraper that scraped this content
        /// </summary>
        public string ScraperId { get; set; }
        
        /// <summary>
        /// Gets or sets the HTTP status code from the last request
        /// </summary>
        public int LastStatusCode { get; set; }
        
        /// <summary>
        /// Gets or sets the content type of the response
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Gets or sets whether the URL is reachable
        /// </summary>
        public bool IsReachable { get; set; }
        
        /// <summary>
        /// Gets or sets the raw content of the page
        /// </summary>
        public string RawContent { get; set; }
        
        /// <summary>
        /// Gets or sets the extracted text content
        /// </summary>
        public string TextContent { get; set; }
        
        /// <summary>
        /// Gets or sets the hash of the content
        /// </summary>
        public string ContentHash { get; set; }
        
        /// <summary>
        /// Gets or sets whether this is regulatory content
        /// </summary>
        public bool IsRegulatoryContent { get; set; }
        
        /// <summary>
        /// Gets or sets when this content was captured
        /// </summary>
        public DateTime CapturedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Gets the folder path for versions of this content
        /// </summary>
        /// <returns>The folder path</returns>
        public string GetVersionFolder()
        {
            return System.IO.Path.Combine("content", ComputeUrlHash(Url));
        }
        
        /// <summary>
        /// Compute a hash of the URL for folder names
        /// </summary>
        private string ComputeUrlHash(string url)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(url));
                return Convert.ToBase64String(bytes)
                    .Replace("/", "_")
                    .Replace("+", "-")
                    .Replace("=", "");
            }
        }
    }
}