using System;
using WebScraperApi.Models;
using CoreModels = WebScraper;
using StateManagement = WebScraper.StateManagement;

namespace WebScraperApi.Services.Common
{
    /// <summary>
    /// Extension methods for model type conversions
    /// </summary>
    public static class ModelExtensions
    {
        /// <summary>
        /// Converts an API ScraperState to a Core ScraperState
        /// </summary>
        public static CoreModels.ScraperState ToCoreScraperState(this ScraperState apiState)
        {
            if (apiState == null) return null;
            
            return new CoreModels.ScraperState
            {
                ScraperId = apiState.Id,
                Status = apiState.IsRunning ? "Running" : "Idle",
                LastRunStartTime = apiState.StartTime ?? DateTime.Now,
                LastRunEndTime = apiState.EndTime,
                // Convert other properties as needed
                ProgressData = apiState.PipelineMetrics != null ? 
                    System.Text.Json.JsonSerializer.Serialize(apiState.PipelineMetrics) : "{}"
            };
        }
        
        /// <summary>
        /// Converts a Core ScraperState to an API ScraperState
        /// </summary>
        public static ScraperState ToApiScraperState(this CoreModels.ScraperState coreState)
        {
            if (coreState == null) return null;
            
            return new ScraperState
            {
                Id = coreState.ScraperId,
                IsRunning = coreState.Status == "Running",
                StartTime = coreState.LastRunStartTime,
                EndTime = coreState.LastRunEndTime,
                // Convert other properties as needed
                Message = coreState.Status
            };
        }
        
        /// <summary>
        /// Converts a StateManagement ScraperState to an API ScraperState
        /// </summary>
        public static ScraperState ToApiScraperState(this StateManagement.ScraperState stateManagementState)
        {
            if (stateManagementState == null) return null;
            
            return new ScraperState
            {
                Id = stateManagementState.ScraperId,
                IsRunning = stateManagementState.Status == "Running",
                StartTime = stateManagementState.LastRunStartTime,
                EndTime = stateManagementState.LastRunEndTime,
                // Convert other properties as needed
                Message = stateManagementState.Status
            };
        }
        
        /// <summary>
        /// Converts an API ScraperState to a StateManagement ScraperState
        /// </summary>
        public static StateManagement.ScraperState ToStateManagementScraperState(this ScraperState apiState)
        {
            if (apiState == null) return null;
            
            return new StateManagement.ScraperState
            {
                ScraperId = apiState.Id,
                Status = apiState.IsRunning ? "Running" : "Idle",
                LastRunStartTime = apiState.StartTime ?? DateTime.Now,
                LastRunEndTime = apiState.EndTime,
                // Convert other properties as needed
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}