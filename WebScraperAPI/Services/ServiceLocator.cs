using Microsoft.Extensions.DependencyInjection;
using System;

namespace WebScraperApi.Services
{
    /// <summary>
    /// Service locator for accessing services from non-DI contexts
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;

        /// <summary>
        /// Initialize the service locator with a service provider
        /// </summary>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <summary>
        /// Get a service of the specified type
        /// </summary>
        public static T GetService<T>() where T : class
        {
            if (_serviceProvider == null)
            {
                return null;
            }

            return _serviceProvider.GetService<T>();
        }
    }
}
