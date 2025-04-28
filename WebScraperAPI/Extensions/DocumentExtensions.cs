using System;
using System.Collections.Generic;
using WebScraperApi.Models;

namespace WebScraperApi.Extensions
{
    /// <summary>
    /// Extension methods for document objects
    /// </summary>
    public static class DocumentExtensions
    {
        /// <summary>
        /// Converts a dynamic document object to a ProcessedDocument
        /// </summary>
        public static ProcessedDocument ToProcessedDocument(this object document)
        {
            var result = new ProcessedDocument();
            
            // Use reflection to get properties
            var type = document.GetType();
            
            // Try to get Id
            var idProp = type.GetProperty("Id");
            if (idProp != null)
            {
                result.Id = idProp.GetValue(document)?.ToString() ?? Guid.NewGuid().ToString();
            }
            
            // Try to get Url
            var urlProp = type.GetProperty("Url");
            if (urlProp != null)
            {
                result.Url = urlProp.GetValue(document)?.ToString() ?? string.Empty;
            }
            
            // Try to get Title
            var titleProp = type.GetProperty("Title");
            if (titleProp != null)
            {
                result.Title = titleProp.GetValue(document)?.ToString() ?? string.Empty;
            }
            
            // Try to get DocumentType
            var typeProp = type.GetProperty("DocumentType");
            if (typeProp != null)
            {
                result.DocumentType = typeProp.GetValue(document)?.ToString() ?? "HTML";
            }
            
            // Try to get ProcessedAt
            var processedAtProp = type.GetProperty("ProcessedAt");
            if (processedAtProp != null)
            {
                var processedAt = processedAtProp.GetValue(document);
                if (processedAt is DateTime dateTime)
                {
                    result.ProcessedAt = dateTime;
                }
            }
            
            // Try to get ContentSizeBytes
            var sizeProp = type.GetProperty("ContentSizeBytes");
            if (sizeProp != null)
            {
                var size = sizeProp.GetValue(document);
                if (size != null && long.TryParse(size.ToString(), out var sizeValue))
                {
                    result.ContentSizeBytes = sizeValue;
                }
            }
            
            // Try to get Metadata
            var metadataProp = type.GetProperty("Metadata");
            if (metadataProp != null)
            {
                var metadata = metadataProp.GetValue(document);
                if (metadata is Dictionary<string, string> dict)
                {
                    result.Metadata = dict;
                }
            }
            
            return result;
        }
    }
}
