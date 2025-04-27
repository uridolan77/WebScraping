using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebScraperApi.Models;

namespace WebScraperApi.Services.Configuration
{
    /// <summary>
    /// Implementation of the scraper configuration service
    /// </summary>
    public class ScraperConfigurationService : IScraperConfigurationService
    {
        private readonly ILogger<ScraperConfigurationService> _logger;
        private readonly string _configFilePath;
        private List<ScraperConfigModel> _configurations;
        private readonly object _lock = new object();

        public ScraperConfigurationService(ILogger<ScraperConfigurationService> logger)
        {
            _logger = logger;
            _configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scraperConfigs.json");
            _configurations = new List<ScraperConfigModel>();
        }

        /// <summary>
        /// Loads scraper configurations from the JSON file
        /// </summary>
        public async Task<List<ScraperConfigModel>> LoadScraperConfigurationsAsync()
        {
            try
            {
                _logger.LogInformation($"Loading scraper configs from: {_configFilePath}");
                
                if (!File.Exists(_configFilePath))
                {
                    _logger.LogWarning($"Config file not found at {_configFilePath}. Creating new config file.");
                    await SaveConfigurationsAsync(new List<ScraperConfigModel>());
                    return new List<ScraperConfigModel>();
                }

                string json = await File.ReadAllTextAsync(_configFilePath);
                var configs = JsonSerializer.Deserialize<List<ScraperConfigModel>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Ensure all configs have IDs
                foreach (var config in configs)
                {
                    if (string.IsNullOrEmpty(config.Id))
                    {
                        config.Id = Guid.NewGuid().ToString();
                    }
                }

                lock (_lock)
                {
                    _configurations = configs;
                }

                return configs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error loading scraper configurations from {_configFilePath}");
                return new List<ScraperConfigModel>();
            }
        }

        /// <summary>
        /// Saves configurations back to the JSON file
        /// </summary>
        private async Task SaveConfigurationsAsync(List<ScraperConfigModel> configurations)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_configFilePath);
                if (!Directory.Exists(directoryPath) && !string.IsNullOrEmpty(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonSerializer.Serialize(configurations, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(_configFilePath, json);
                _logger.LogInformation($"Saved {configurations.Count} scraper configurations to {_configFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error saving scraper configurations to {_configFilePath}");
                throw;
            }
        }

        /// <summary>
        /// Gets all scraper configurations
        /// </summary>
        public async Task<IEnumerable<ScraperConfigModel>> GetAllScraperConfigsAsync()
        {
            if (_configurations.Count == 0)
            {
                await LoadScraperConfigurationsAsync();
            }

            lock (_lock)
            {
                return _configurations.ToList();
            }
        }

        /// <summary>
        /// Gets a specific scraper configuration by ID
        /// </summary>
        public async Task<ScraperConfigModel> GetScraperConfigAsync(string id)
        {
            if (_configurations.Count == 0)
            {
                await LoadScraperConfigurationsAsync();
            }

            lock (_lock)
            {
                return _configurations.FirstOrDefault(c => c.Id == id);
            }
        }

        /// <summary>
        /// Creates a new scraper configuration
        /// </summary>
        public async Task<ScraperConfigModel> CreateScraperConfigAsync(ScraperConfigModel config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            // Ensure the new config has an ID
            if (string.IsNullOrEmpty(config.Id))
            {
                config.Id = Guid.NewGuid().ToString();
            }

            // Set defaults for new configurations
            config.CreatedAt = DateTime.Now;
            config.LastModified = DateTime.Now;
            config.RunCount = 0;

            lock (_lock)
            {
                _configurations.Add(config);
            }

            await SaveConfigurationsAsync(_configurations);
            return config;
        }

        /// <summary>
        /// Updates an existing scraper configuration
        /// </summary>
        public async Task<bool> UpdateScraperConfigAsync(string id, ScraperConfigModel config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            lock (_lock)
            {
                var existingIndex = _configurations.FindIndex(c => c.Id == id);
                if (existingIndex == -1)
                {
                    return false;
                }

                // Preserve certain fields from the existing config
                var existing = _configurations[existingIndex];
                config.Id = id; // Ensure ID doesn't change
                config.CreatedAt = existing.CreatedAt;
                config.LastModified = DateTime.Now;
                config.RunCount = existing.RunCount;

                _configurations[existingIndex] = config;
            }

            await SaveConfigurationsAsync(_configurations);
            return true;
        }

        /// <summary>
        /// Deletes a scraper configuration
        /// </summary>
        public async Task<bool> DeleteScraperConfigAsync(string id)
        {
            bool removed = false;

            lock (_lock)
            {
                var existingConfig = _configurations.FirstOrDefault(c => c.Id == id);
                if (existingConfig != null)
                {
                    removed = _configurations.Remove(existingConfig);
                }
            }

            if (removed)
            {
                await SaveConfigurationsAsync(_configurations);
            }

            return removed;
        }

        /// <summary>
        /// Saves scraper configurations to a file (used as backup)
        /// </summary>
        public void SaveScraperConfigurationsToFile(IEnumerable<ScraperConfigModel> configs)
        {
            if (configs == null)
            {
                throw new ArgumentNullException(nameof(configs));
            }

            lock (_lock)
            {
                _configurations = configs.ToList();
            }

            _ = SaveConfigurationsAsync(_configurations);
        }
    }
}