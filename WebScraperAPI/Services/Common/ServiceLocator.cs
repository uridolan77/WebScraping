using System;
using System.Collections.Generic;

namespace WebScraperApi.Services.Common
{
    /// <summary>
    /// Simple service locator to resolve circular dependencies
    /// Note: This is a temporary solution. In production code, consider using
    /// a more robust dependency injection approach or event-based communication.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        /// <summary>
        /// Register a service with the service locator
        /// </summary>
        /// <param name="serviceType">Type of the service</param>
        /// <param name="implementation">Implementation instance</param>
        public static void RegisterService(Type serviceType, object implementation)
        {
            if (_services.ContainsKey(serviceType))
            {
                _services[serviceType] = implementation;
            }
            else
            {
                _services.Add(serviceType, implementation);
            }
        }

        /// <summary>
        /// Get a service from the service locator
        /// </summary>
        /// <param name="serviceType">Type of service to retrieve</param>
        /// <returns>Service implementation or null if not found</returns>
        public static object GetService(Type serviceType)
        {
            if (_services.TryGetValue(serviceType, out var service))
            {
                return service;
            }
            return null;
        }

        /// <summary>
        /// Get a service from the service locator with specific type
        /// </summary>
        /// <typeparam name="T">Type of service to retrieve</typeparam>
        /// <returns>Service implementation or null if not found</returns>
        public static T GetService<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
            {
                return service as T;
            }
            return null;
        }

        /// <summary>
        /// Clear all registered services
        /// </summary>
        public static void Clear()
        {
            _services.Clear();
        }
    }
}