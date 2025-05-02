using System;
using System.Collections.Generic;

namespace WebScraper.Processing
{
    /// <summary>
    /// Represents the result of processing a document, containing extracted text and metadata.
    /// </summary>
    public class DocumentProcessingResult
    {
        /// <summary>
        /// The extracted text content from the document.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// The document's title, if available.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The document's author, if available.
        /// </summary>
        public string? Author { get; set; }

        /// <summary>
        /// The date the document was created, if available.
        /// </summary>
        public DateTime? CreationDate { get; set; }

        /// <summary>
        /// The date the document was last modified, if available.
        /// </summary>
        public DateTime? ModificationDate { get; set; }

        /// <summary>
        /// The number of pages in the document, if applicable.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Keywords associated with the document, if available.
        /// </summary>
        public List<string> Keywords { get; set; } = new List<string>();

        /// <summary>
        /// Additional metadata properties that may be specific to certain document types.
        /// </summary>
        public Dictionary<string, string> AdditionalMetadata { get; set; } = new Dictionary<string, string>();
    }
}