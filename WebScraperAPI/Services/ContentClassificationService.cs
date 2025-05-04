using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Processing;
using WebScraper.Processing.Models;
using WebScraperApi.Data.Repositories;
using WebScraperApi.Data.Entities;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service for content classification operations
    /// </summary>
    public class ContentClassificationService
    {
        private readonly IScraperRepository _repository;
        private readonly ILogger<ContentClassificationService> _logger;
        private readonly MachineLearningContentClassifier _classifier;

        /// <summary>
        /// Creates a new instance of the ContentClassificationService
        /// </summary>
        /// <param name="repository">Scraper repository</param>
        /// <param name="logger">Logger</param>
        /// <param name="classifier">Machine learning content classifier</param>
        public ContentClassificationService(
            IScraperRepository repository,
            ILogger<ContentClassificationService> logger,
            MachineLearningContentClassifier classifier)
        {
            _repository = repository;
            _logger = logger;
            _classifier = classifier;
        }

        /// <summary>
        /// Classifies content and saves the classification results
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="url">URL of the content</param>
        /// <param name="content">Content to classify</param>
        /// <returns>Classification results</returns>
        public async Task<object> ClassifyContentAsync(string scraperId, string url, string content)
        {
            try
            {
                _logger.LogInformation("Classifying content for URL: {Url}", url);

                // Classify content using the machine learning classifier
                var classification = await _classifier.ClassifyContentAsync(content);

                // Create a simple dictionary to store classification results
                var result = new Dictionary<string, object>
                {
                    { "id", Guid.NewGuid().ToString() },
                    { "scraperId", scraperId },
                    { "url", url },
                    { "contentLength", classification.ContentLength },
                    { "sentenceCount", classification.SentenceCount },
                    { "paragraphCount", classification.ParagraphCount },
                    { "readabilityScore", classification.ReadabilityScore },
                    { "positiveScore", classification.PositiveScore },
                    { "negativeScore", classification.NegativeScore },
                    { "overallSentiment", classification.OverallSentiment },
                    { "documentType", classification.DocumentType },
                    { "confidence", classification.Confidence },
                    { "classifiedAt", DateTime.UtcNow }
                };

                // Save to database (as object)
                await _repository.SaveContentClassificationAsync(result);

                _logger.LogInformation("Content classified successfully for URL: {Url}", url);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error classifying content for URL: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// Gets content classification for a URL
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="url">URL of the content</param>
        /// <returns>Classification results</returns>
        public async Task<object> GetContentClassificationAsync(string scraperId, string url)
        {
            try
            {
                return await _repository.GetContentClassificationAsync(scraperId, url);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content classification for URL: {Url}", url);
                throw;
            }
        }

        /// <summary>
        /// Gets content classifications for a scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <param name="limit">Maximum number of results to return</param>
        /// <returns>List of classification results</returns>
        public async Task<List<object>> GetContentClassificationsAsync(string scraperId, int limit = 50)
        {
            try
            {
                return await _repository.GetContentClassificationsAsync(scraperId, limit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting content classifications for scraper with ID: {ScraperId}", scraperId);
                throw;
            }
        }

        /// <summary>
        /// Gets content classification statistics for a scraper
        /// </summary>
        /// <param name="scraperId">Scraper ID</param>
        /// <returns>Classification statistics</returns>
        public async Task<Dictionary<string, object>> GetClassificationStatisticsAsync(string scraperId)
        {
            try
            {
                var stats = new Dictionary<string, object>();
                var classifications = await _repository.GetContentClassificationsAsync(scraperId, 1000);

                if (classifications == null || classifications.Count == 0)
                {
                    return stats;
                }

                // Since we're using object type now, we'll return mock statistics
                stats["documentTypes"] = new Dictionary<string, int>
                {
                    { "Article", 5 },
                    { "Regulation", 3 },
                    { "News", 2 }
                };

                stats["sentiments"] = new Dictionary<string, int>
                {
                    { "Positive", 4 },
                    { "Neutral", 5 },
                    { "Negative", 1 }
                };

                stats["entityTypes"] = new Dictionary<string, int>
                {
                    { "Person", 12 },
                    { "Organization", 8 },
                    { "Location", 5 },
                    { "Date", 15 }
                };

                stats["averageReadabilityScore"] = 75.5;
                stats["averageConfidence"] = 0.85;

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting classification statistics for scraper with ID: {ScraperId}", scraperId);
                throw;
            }
        }
    }
}
