using System;
using Microsoft.Extensions.Logging;
using WebScraper;
using WebScraper.StateManagement;
using WebScraper.RegulatoryFramework.Interfaces;

namespace WebScraperApi.Services.Factories
{
    /// <summary>
    /// Factory for creating state management components
    /// </summary>
    public class StateManagementFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        
        public StateManagementFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }
        
        /// <summary>
        /// Creates a state store for persisting scraper state
        /// </summary>
        /// <param name="config">The scraper configuration</param>
        /// <param name="logAction">Action for logging messages</param>
        /// <returns>An IStateStore implementation</returns>
        public IStateStore CreateStateStore(ScraperConfig config, Action<string> logAction)
        {
            logAction("Setting up persistent state management");
            return (IStateStore)new PersistentStateManager(config.OutputDirectory);
        }
    }
}