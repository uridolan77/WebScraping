using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using WebScraper.AdaptiveCrawling;

namespace WebScraper.RegulatoryContent
{
    public class UKGCCrawlStrategy : AdaptiveCrawlStrategy
    {
        // Important sections in UKGC site to prioritize
        private readonly List<string> _prioritySections = new List<string>
        {
            "/licensees-and-businesses/lccp",
            "/licensees-and-businesses/compliance",
            "/licensees-and-businesses/aml",
            "/licensees-and-businesses/enforcement",
            "/news/enforcement-action"
        };
        
        // Important content types to prioritize
        private readonly List<string> _priorityContentTypes = new List<string>
        {
            "guidance",
            "consultation-response",
            "regulatory-update",
            "strategy",
            "report"
        };
        
        // URLs to avoid or deprioritize
        private readonly List<string> _lowPriorityPatterns = new List<string>
        {
            "/careers",
            "/contact-us/feedback",
            "/cookies",
            "/accessibility",
            "/terms-of-use"
        };

        // Store UKGC-specific metadata
        private readonly Dictionary<string, Dictionary<string, object>> _ukgcMetadata = new Dictionary<string, Dictionary<string, object>>();

        public UKGCCrawlStrategy(Action<string> logger = null) : base(logger)
        {
            // Call the base constructor
        }

        /// <summary>
        /// UKGC-specific prioritization of URLs
        /// </summary>
        public new IEnumerable<string> PrioritizeUrls(List<string> urls, int maxUrls = 10)
        {
            if (urls == null || !urls.Any())
                return Enumerable.Empty<string>();

            var enhancedUrls = new List<(string Url, double ExtraScore)>();

            // First analyze which URLs should get extra scores
            foreach (var url in urls)
            {
                double extraScore = 0;
                
                // Boost score for priority sections
                if (_prioritySections.Any(section => url.Contains(section, StringComparison.OrdinalIgnoreCase)))
                {
                    extraScore += 3.0;
                }
                
                // Boost score for priority content types
                if (_priorityContentTypes.Any(contentType => url.Contains(contentType, StringComparison.OrdinalIgnoreCase)))
                {
                    extraScore += 2.0;
                }
                
                // Reduce score for low priority patterns
                if (_lowPriorityPatterns.Any(pattern => url.Contains(pattern, StringComparison.OrdinalIgnoreCase)))
                {
                    extraScore -= 3.0;
                }
                
                // Boost scores for URLs containing dates (likely news or updates)
                if (Regex.IsMatch(url, @"\b20\d{2}[-/]\d{1,2}[-/]\d{1,2}\b"))
                {
                    extraScore += 1.5;
                }
                
                // Boost scores for PDF documents (likely important regulatory documents)
                if (url.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    extraScore += 2.0;
                }
                
                enhancedUrls.Add((url, extraScore));
            }

            // Get base prioritization 
            var basePrioritized = base.PrioritizeUrls(urls, maxUrls).ToList();
            
            // If a URL with high UKGC priority isn't in the base list, insert it
            var finalList = new List<string>(basePrioritized);
            
            // Consider adding high-priority UKGC URLs if not already included
            foreach (var enhancedUrl in enhancedUrls.OrderByDescending(u => u.ExtraScore))
            {
                if (enhancedUrl.ExtraScore > 2.0 && !finalList.Contains(enhancedUrl.Url))
                {
                    // Add high priority URLs to the result, avoiding duplicates
                    if (finalList.Count >= maxUrls)
                    {
                        // Replace the lowest priority URL with this one
                        finalList[finalList.Count - 1] = enhancedUrl.Url;
                    }
                    else
                    {
                        finalList.Add(enhancedUrl.Url);
                    }
                }
            }
            
            // Return the enhanced list, respecting maxUrls
            return finalList.Take(maxUrls);
        }
        
        /// <summary>
        /// Enhanced page metadata extraction for UKGC pages
        /// </summary>
        public new void UpdatePageMetadata(string url, HtmlDocument document, string textContent)
        {
            // Call the base implementation first
            base.UpdatePageMetadata(url, document, textContent);
            
            try
            {
                // Initialize UKGC-specific metadata if needed
                if (!_ukgcMetadata.ContainsKey(url))
                {
                    _ukgcMetadata[url] = new Dictionary<string, object>();
                }
                
                var metadata = _ukgcMetadata[url];
                
                // Check for publication date
                var publishedDateNode = document.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'gcweb-meta')]//span[contains(@class, 'gcweb-body-s')]");
                if (publishedDateNode != null)
                {
                    var dateText = publishedDateNode.InnerText.Trim();
                    metadata["PublishedDate"] = dateText;
                }
                
                // Check for content type/category
                var contentTypeNode = document.DocumentNode.SelectSingleNode(
                    "//div[contains(@class, 'gcweb-meta')]//span");
                if (contentTypeNode != null)
                {
                    var contentType = contentTypeNode.InnerText.Trim();
                    metadata["ContentType"] = contentType;
                }
                
                // Check if the page is an enforcement action
                if (url.Contains("/news/article/") || url.Contains("/news/enforcement-action/"))
                {
                    metadata["IsEnforcementAction"] = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting UKGC-specific metadata: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Specialized method to extract regulatory changes
        /// </summary>
        public List<string> ExtractRegulatoryChanges(HtmlDocument document)
        {
            var changes = new List<string>();
            
            // Look for recent changes, publications, or enforcement actions
            var recentPublications = document.DocumentNode.SelectNodes(
                "//h2[contains(text(), 'Recent publication') or contains(text(), 'Recent update')]/following::ul[1]//li//a");
            
            if (recentPublications != null)
            {
                foreach (var publication in recentPublications)
                {
                    changes.Add(publication.InnerText.Trim());
                }
            }
            
            // Look for enforcement actions
            var enforcementActions = document.DocumentNode.SelectNodes(
                "//h2[contains(text(), 'Action')]/following::div[contains(@class, 'timeline')]//a");
            
            if (enforcementActions != null)
            {
                foreach (var action in enforcementActions)
                {
                    changes.Add(action.InnerText.Trim());
                }
            }
            
            return changes;
        }

        /// <summary>
        /// Gets gambling-related keywords to monitor in documents
        /// </summary>
        public Dictionary<string, List<string>> GetGamblingRegulationKeywords()
        {
            return new Dictionary<string, List<string>>
            {
                ["AML"] = new List<string> { "anti-money laundering", "money laundering", "terrorist financing", "suspicious activity", "customer due diligence", "KYC", "know your customer" },
                ["Licensing"] = new List<string> { "license", "licence", "licensing", "operating license", "personal management license", "application", "renewal" },
                ["Responsible Gambling"] = new List<string> { "responsible gambling", "safer gambling", "self-exclusion", "gambling limits", "player protection", "gambling harm", "vulnerable persons" },
                ["LCCP"] = new List<string> { "license conditions", "code of practice", "LCCP", "social responsibility", "regulatory returns", "reporting requirements" },
                ["Enforcement"] = new List<string> { "regulatory settlement", "enforcement action", "fine", "penalty", "license suspension", "license revocation", "investigation" }
            };
        }
        
        /// <summary>
        /// Get UKGC-specific metadata for a URL
        /// </summary>
        public Dictionary<string, object> GetUKGCMetadata(string url)
        {
            if (_ukgcMetadata.TryGetValue(url, out var metadata))
            {
                return metadata;
            }
            return new Dictionary<string, object>();
        }
    }
}