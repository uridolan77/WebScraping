using System;

namespace WebScraper.StateManagement
{
    /// <summary>
    /// Implementation of ContentItem for state management purposes
    /// </summary>
    public class ContentItemImpl : ContentItem
    {
        // ContentItem already implements all the required interface members
        // We can add additional functionality specific to ContentItemImpl if needed
        
        /// <summary>
        /// Gets or sets the extracted text content
        /// </summary>
        public new string TextContent { get; set; }
        
        /// <summary>
        /// Constructor
        /// </summary>
        public ContentItemImpl()
        {
            CapturedAt = DateTime.Now;
        }
        
        /// <summary>
        /// Creates a copy of ContentItemImpl from a ContentItem instance
        /// </summary>
        public static ContentItemImpl FromContentItem(ContentItem item)
        {
            if (item == null)
                return null;
                
            if (item is ContentItemImpl impl)
                return impl;
                
            return new ContentItemImpl
            {
                Url = item.Url,
                Title = item.Title,
                ScraperId = item.ScraperId,
                LastStatusCode = item.LastStatusCode,
                ContentType = item.ContentType,
                IsReachable = item.IsReachable,
                RawContent = item.RawContent,
                ContentHash = item.ContentHash,
                IsRegulatoryContent = item.IsRegulatoryContent,
                CapturedAt = DateTime.Now
            };
        }
    }
}