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

            var coreState = new CoreModels.ScraperState();

            // Set properties using reflection to handle missing properties gracefully
            var coreType = typeof(CoreModels.ScraperState);

            // Set basic properties
            coreType.GetProperty("ScraperId")?.SetValue(coreState, apiState.Id);
            coreType.GetProperty("Status")?.SetValue(coreState, apiState.Status ?? (apiState.IsRunning ? "Running" : "Idle"));
            coreType.GetProperty("LastRunStartTime")?.SetValue(coreState, apiState.LastRunStartTime ?? apiState.StartTime ?? DateTime.Now);
            coreType.GetProperty("LastRunEndTime")?.SetValue(coreState, apiState.LastRunEndTime ?? apiState.EndTime);

            // Set progress data
            var progressData = apiState.ProgressData;
            if (string.IsNullOrEmpty(progressData) && apiState.PipelineMetrics != null)
            {
                progressData = System.Text.Json.JsonSerializer.Serialize(apiState.PipelineMetrics);
            }
            coreType.GetProperty("ProgressData")?.SetValue(coreState, progressData ?? "{}");

            return coreState;
        }

        /// <summary>
        /// Converts a Core ScraperState to an API ScraperState
        /// </summary>
        public static ScraperState ToApiScraperState(this CoreModels.ScraperState coreState)
        {
            if (coreState == null) return new ScraperState();

            var apiState = new ScraperState();

            // Set properties using reflection to handle missing properties gracefully
            var coreType = typeof(CoreModels.ScraperState);

            // Get basic properties
            var scraperId = coreType.GetProperty("ScraperId")?.GetValue(coreState)?.ToString() ?? "";
            var status = coreType.GetProperty("Status")?.GetValue(coreState)?.ToString() ?? "Idle";
            var lastRunStartTime = coreType.GetProperty("LastRunStartTime")?.GetValue(coreState) as DateTime?;
            var lastRunEndTime = coreType.GetProperty("LastRunEndTime")?.GetValue(coreState) as DateTime?;
            var progressData = coreType.GetProperty("ProgressData")?.GetValue(coreState)?.ToString() ?? "{}";

            // Set API state properties
            apiState.Id = scraperId;
            apiState.ScraperId = scraperId;
            apiState.Status = status;
            apiState.IsRunning = status == "Running";
            apiState.StartTime = lastRunStartTime;
            apiState.EndTime = lastRunEndTime;
            apiState.LastRunStartTime = lastRunStartTime;
            apiState.LastRunEndTime = lastRunEndTime;
            apiState.Message = status;
            apiState.ProgressData = progressData;

            return apiState;
        }

        /// <summary>
        /// Converts a StateManagement ScraperState to an API ScraperState
        /// </summary>
        public static ScraperState ToApiScraperState(this StateManagement.ScraperState stateManagementState)
        {
            if (stateManagementState == null) return new ScraperState();

            var apiState = new ScraperState();

            // Set properties using reflection to handle missing properties gracefully
            var stateType = typeof(StateManagement.ScraperState);

            // Get basic properties
            var scraperId = stateType.GetProperty("ScraperId")?.GetValue(stateManagementState)?.ToString() ?? "";
            var status = stateType.GetProperty("Status")?.GetValue(stateManagementState)?.ToString() ?? "Idle";
            var lastRunStartTime = stateType.GetProperty("LastRunStartTime")?.GetValue(stateManagementState) as DateTime?;
            var lastRunEndTime = stateType.GetProperty("LastRunEndTime")?.GetValue(stateManagementState) as DateTime?;
            var progressData = stateType.GetProperty("ProgressData")?.GetValue(stateManagementState)?.ToString() ?? "{}";

            // Set API state properties
            apiState.Id = scraperId;
            apiState.ScraperId = scraperId;
            apiState.Status = status;
            apiState.IsRunning = status == "Running";
            apiState.StartTime = lastRunStartTime;
            apiState.EndTime = lastRunEndTime;
            apiState.LastRunStartTime = lastRunStartTime;
            apiState.LastRunEndTime = lastRunEndTime;
            apiState.Message = status;
            apiState.ProgressData = progressData;

            return apiState;
        }

        /// <summary>
        /// Converts an API ScraperState to a StateManagement ScraperState
        /// </summary>
        public static StateManagement.ScraperState ToStateManagementScraperState(this ScraperState apiState)
        {
            if (apiState == null) return new StateManagement.ScraperState();

            var stateManagementState = new StateManagement.ScraperState();

            // Set properties using reflection to handle missing properties gracefully
            var stateType = typeof(StateManagement.ScraperState);

            // Set basic properties
            stateType.GetProperty("ScraperId")?.SetValue(stateManagementState, apiState.Id);
            stateType.GetProperty("Status")?.SetValue(stateManagementState, apiState.Status ?? (apiState.IsRunning ? "Running" : "Idle"));
            stateType.GetProperty("LastRunStartTime")?.SetValue(stateManagementState, apiState.LastRunStartTime ?? apiState.StartTime ?? DateTime.Now);
            stateType.GetProperty("LastRunEndTime")?.SetValue(stateManagementState, apiState.LastRunEndTime ?? apiState.EndTime);
            stateType.GetProperty("UpdatedAt")?.SetValue(stateManagementState, DateTime.UtcNow);

            // Set progress data
            var progressData = apiState.ProgressData;
            if (string.IsNullOrEmpty(progressData) && apiState.PipelineMetrics != null)
            {
                progressData = System.Text.Json.JsonSerializer.Serialize(apiState.PipelineMetrics);
            }
            stateType.GetProperty("ProgressData")?.SetValue(stateManagementState, progressData ?? "{}");

            return stateManagementState;
        }
    }
}