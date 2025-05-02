using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;

namespace WebScraper.RegulatoryFramework.Interfaces
{
    /// <summary>
    /// Core interfaces for the regulatory scraping framework
    /// </summary>

    /// <summary>
    /// Strategy for prioritizing and managing crawl operations
    /// </summary>
    public interface ICrawlStrategy
    {
        void LoadMetadata(Dictionary<string, object> metadata);
        IEnumerable<string> PrioritizeUrls(List<string> urls, int maxUrls = 10);
        void UpdatePageMetadata(string url, HtmlDocument document, string textContent);
        Dictionary<string, object> GetPageMetadata();
        bool ShouldCrawl(string url);
    }

    /// <summary>
    /// Extracts content from HTML documents
    /// </summary>
    public interface IContentExtractor
    {
        string ExtractTextContent(HtmlDocument document);
        List<WebScraper.ContentNode> ExtractStructuredContent(HtmlDocument document);
    }

    /// <summary>
    /// Processes documents like PDFs and Office files
    /// </summary>
    public interface IDocumentProcessor
    {
        Task<WebScraper.RegulatoryFramework.Implementation.DocumentMetadata> ProcessDocumentAsync(string url, string title, byte[] content);
        Task ProcessLinkedDocumentsAsync(string pageUrl, HtmlDocument document);
    }

    /// <summary>
    /// Detects and analyzes changes in content
    /// </summary>
    public interface IChangeDetector
    {
        ChangeAnalysisResult AnalyzeChanges(string oldContent, string newContent);
        SignificantChangesResult DetectSignificantChanges(string oldContent, string newContent);
        Task<PageVersion> TrackPageVersionAsync(string url, string content, string textContent);
    }

    /// <summary>
    /// Classifies content into regulatory categories
    /// </summary>
    public interface IContentClassifier
    {
        ClassificationResult ClassifyContent(string url, string textContent, HtmlDocument document = null);
    }

    /// <summary>
    /// Renders dynamic content using a headless browser
    /// </summary>
    public interface IDynamicContentRenderer
    {
        Task<string> GetRenderedHtmlAsync(string url);
        Task<HtmlDocument> GetRenderedDocumentAsync(string url);
    }

    /// <summary>
    /// Sends alerts for significant changes
    /// </summary>
    public interface IAlertService
    {
        Task ProcessAlertAsync(string url, SignificantChangesResult changes);
        Task SendNotificationAsync(string subject, string message, AlertImportance importance);
    }

    /// <summary>
    /// Stores state information for the scraper
    /// </summary>
    public interface IStateStore
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value);
        Task<PageVersion> GetLatestVersionAsync(string url);
        Task SaveVersionAsync(PageVersion version);
        Task<List<PageVersion>> GetVersionHistoryAsync(string url, int maxVersions = 10);
    }

    /// <summary>
    /// Enum for state store types
    /// </summary>
    public enum StateStoreType
    {
        Memory,
        File,
        Database
    }

    /// <summary>
    /// Importance level for alerts
    /// </summary>
    public enum AlertImportance
    {
        Low,
        Medium,
        High,
        Critical
    }
}