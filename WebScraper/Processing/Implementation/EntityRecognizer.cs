using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WebScraper.Processing.Interfaces;
using WebScraper.Processing.Models;

namespace WebScraper.Processing.Implementation
{
    /// <summary>
    /// Recognizes entities in text
    /// </summary>
    public class EntityRecognizer : IEntityRecognizer
    {
        private readonly ILogger<EntityRecognizer> _logger;
        private readonly Dictionary<string, Regex> _entityPatterns;
        private readonly HashSet<string> _organizationKeywords;
        private readonly HashSet<string> _locationKeywords;
        
        /// <summary>
        /// Creates a new instance of the EntityRecognizer
        /// </summary>
        /// <param name="logger">Logger instance</param>
        public EntityRecognizer(ILogger<EntityRecognizer> logger)
        {
            _logger = logger;
            
            // Initialize entity patterns
            _entityPatterns = InitializeEntityPatterns();
            
            // Initialize organization keywords
            _organizationKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Commission", "Authority", "Agency", "Board", "Committee",
                "Department", "Ministry", "Office", "Bureau", "Council",
                "Association", "Federation", "Organization", "Organisation",
                "Institute", "Foundation", "Corporation", "Company", "Ltd",
                "Limited", "Inc", "Incorporated", "LLC", "LLP", "Group",
                "International", "National", "Federal", "State", "Regional",
                "Local", "Central", "Global", "Worldwide", "European",
                "American", "Asian", "African", "Australian", "Canadian"
            };
            
            // Initialize location keywords
            _locationKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "City", "Town", "Village", "County", "State", "Province",
                "Region", "District", "Territory", "Country", "Nation",
                "Republic", "Kingdom", "Empire", "Commonwealth", "Union",
                "Federation", "Confederation", "League", "Alliance", "Bloc",
                "Area", "Zone", "Sector", "Quarter", "Neighborhood", "Suburb",
                "Metropolitan", "Urban", "Rural", "Coastal", "Inland",
                "Northern", "Southern", "Eastern", "Western", "Central"
            };
        }
        
        /// <summary>
        /// Recognizes entities in text
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>List of recognized entities</returns>
        public Task<IEnumerable<RecognizedEntity>> RecognizeEntitiesAsync(string text)
        {
            _logger.LogInformation("Recognizing entities");
            
            try
            {
                var entities = new List<RecognizedEntity>();
                
                // Extract entities using patterns
                foreach (var pattern in _entityPatterns)
                {
                    var matches = pattern.Value.Matches(text);
                    foreach (Match match in matches)
                    {
                        var entity = new RecognizedEntity
                        {
                            Type = pattern.Key,
                            Value = match.Value,
                            Position = match.Index,
                            Length = match.Length,
                            Confidence = CalculateConfidence(pattern.Key, match.Value)
                        };
                        
                        entities.Add(entity);
                    }
                }
                
                // Extract organization names
                ExtractOrganizationNames(text, entities);
                
                // Extract location names
                ExtractLocationNames(text, entities);
                
                // Remove duplicates and overlaps
                entities = RemoveDuplicatesAndOverlaps(entities);
                
                return Task.FromResult<IEnumerable<RecognizedEntity>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recognizing entities");
                throw;
            }
        }
        
        /// <summary>
        /// Initializes entity patterns
        /// </summary>
        private Dictionary<string, Regex> InitializeEntityPatterns()
        {
            var patterns = new Dictionary<string, Regex>();
            
            // Date patterns
            patterns["Date"] = new Regex(@"\b(?:(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+\d{1,2},?\s+\d{4}|\d{1,2}\s+(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)[a-z]*\.?\s+\d{4}|\d{1,2}/\d{1,2}/\d{2,4}|\d{4}-\d{2}-\d{2})\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            // Money patterns
            patterns["Money"] = new Regex(@"\b(?:£|\$|€|¥)?\s*\d+(?:,\d{3})*(?:\.\d{2})?\s*(?:GBP|USD|EUR|JPY|pounds?|dollars?|euros?|yen)?\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            // Percentage patterns
            patterns["Percentage"] = new Regex(@"\b\d+(?:\.\d+)?%\b", RegexOptions.Compiled);
            
            // Email patterns
            patterns["Email"] = new Regex(@"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b", RegexOptions.Compiled);
            
            // URL patterns
            patterns["URL"] = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            // Phone number patterns
            patterns["Phone"] = new Regex(@"\b(?:\+\d{1,3}\s?)?(?:\(\d{1,4}\)\s?)?(?:\d{1,4}[-.\s]?){1,4}\d{1,4}\b", RegexOptions.Compiled);
            
            // Person name patterns (simplified)
            patterns["Person"] = new Regex(@"\b(?:[A-Z][a-z]+\s+){1,2}[A-Z][a-z]+\b", RegexOptions.Compiled);
            
            return patterns;
        }
        
        /// <summary>
        /// Extracts organization names
        /// </summary>
        private void ExtractOrganizationNames(string text, List<RecognizedEntity> entities)
        {
            // Look for capitalized phrases containing organization keywords
            var orgPattern = new Regex(@"\b(?:[A-Z][a-z]*\s+){0,3}(?:" + string.Join("|", _organizationKeywords) + @")\s+(?:of\s+)?(?:[A-Z][a-z]*\s*){0,3}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            var matches = orgPattern.Matches(text);
            foreach (Match match in matches)
            {
                // Check if the match is likely an organization
                if (IsLikelyOrganization(match.Value))
                {
                    var entity = new RecognizedEntity
                    {
                        Type = "Organization",
                        Value = match.Value.Trim(),
                        Position = match.Index,
                        Length = match.Length,
                        Confidence = CalculateConfidence("Organization", match.Value)
                    };
                    
                    entities.Add(entity);
                }
            }
        }
        
        /// <summary>
        /// Extracts location names
        /// </summary>
        private void ExtractLocationNames(string text, List<RecognizedEntity> entities)
        {
            // Look for capitalized phrases containing location keywords
            var locPattern = new Regex(@"\b(?:[A-Z][a-z]*\s+){0,2}(?:" + string.Join("|", _locationKeywords) + @")\s+(?:of\s+)?(?:[A-Z][a-z]*\s*){0,2}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            
            var matches = locPattern.Matches(text);
            foreach (Match match in matches)
            {
                // Check if the match is likely a location
                if (IsLikelyLocation(match.Value))
                {
                    var entity = new RecognizedEntity
                    {
                        Type = "Location",
                        Value = match.Value.Trim(),
                        Position = match.Index,
                        Length = match.Length,
                        Confidence = CalculateConfidence("Location", match.Value)
                    };
                    
                    entities.Add(entity);
                }
            }
        }
        
        /// <summary>
        /// Checks if a string is likely an organization name
        /// </summary>
        private bool IsLikelyOrganization(string text)
        {
            // Check if the text contains organization keywords
            foreach (var keyword in _organizationKeywords)
            {
                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            
            // Check if the text has capitalized words
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int capitalizedWords = words.Count(w => w.Length > 0 && char.IsUpper(w[0]));
            
            return capitalizedWords >= 2;
        }
        
        /// <summary>
        /// Checks if a string is likely a location name
        /// </summary>
        private bool IsLikelyLocation(string text)
        {
            // Check if the text contains location keywords
            foreach (var keyword in _locationKeywords)
            {
                if (text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }
            
            // Check if the text has capitalized words
            var words = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int capitalizedWords = words.Count(w => w.Length > 0 && char.IsUpper(w[0]));
            
            return capitalizedWords >= 1;
        }
        
        /// <summary>
        /// Calculates confidence for entity recognition
        /// </summary>
        private double CalculateConfidence(string entityType, string value)
        {
            // Base confidence by entity type
            double baseConfidence = 0.7;
            
            switch (entityType)
            {
                case "Date":
                case "Money":
                case "Percentage":
                case "Email":
                case "URL":
                case "Phone":
                    baseConfidence = 0.9; // Higher confidence for well-defined patterns
                    break;
                case "Person":
                    baseConfidence = 0.7;
                    break;
                case "Organization":
                case "Location":
                    baseConfidence = 0.6;
                    break;
                default:
                    baseConfidence = 0.5;
                    break;
            }
            
            // Adjust confidence based on value characteristics
            if (entityType == "Organization" || entityType == "Location")
            {
                // Higher confidence for longer names
                var words = value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 2)
                {
                    baseConfidence += 0.1;
                }
                
                // Higher confidence for more capitalized words
                int capitalizedWords = words.Count(w => w.Length > 0 && char.IsUpper(w[0]));
                if (capitalizedWords == words.Length)
                {
                    baseConfidence += 0.1;
                }
            }
            
            // Cap confidence at 0.95
            return Math.Min(0.95, baseConfidence);
        }
        
        /// <summary>
        /// Removes duplicate and overlapping entities
        /// </summary>
        private List<RecognizedEntity> RemoveDuplicatesAndOverlaps(List<RecognizedEntity> entities)
        {
            // Sort entities by position
            entities = entities.OrderBy(e => e.Position).ToList();
            
            var result = new List<RecognizedEntity>();
            
            for (int i = 0; i < entities.Count; i++)
            {
                var current = entities[i];
                bool overlaps = false;
                
                // Check for overlaps with existing entities in result
                foreach (var existing in result)
                {
                    if (DoEntitiesOverlap(current, existing))
                    {
                        overlaps = true;
                        
                        // If current entity has higher confidence, replace existing
                        if (current.Confidence > existing.Confidence)
                        {
                            result.Remove(existing);
                            result.Add(current);
                        }
                        
                        break;
                    }
                }
                
                // If no overlaps, add to result
                if (!overlaps)
                {
                    result.Add(current);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Checks if two entities overlap
        /// </summary>
        private bool DoEntitiesOverlap(RecognizedEntity a, RecognizedEntity b)
        {
            int aStart = a.Position;
            int aEnd = a.Position + a.Length;
            int bStart = b.Position;
            int bEnd = b.Position + b.Length;
            
            return (aStart <= bEnd && aEnd >= bStart);
        }
    }
}
