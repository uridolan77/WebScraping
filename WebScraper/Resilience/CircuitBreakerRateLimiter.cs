using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WebScraper.Resilience
{
    /// <summary>
    /// Implements the Circuit Breaker pattern to prevent cascading failures
    /// when external services are experiencing issues
    /// </summary>
    public class CircuitBreakerRateLimiter
    {
        private readonly ILogger _logger;
        private readonly int _failureThreshold;
        private readonly TimeSpan _resetTimeout;
        private readonly ConcurrentDictionary<string, DomainCircuitState> _domainStates = new();

        /// <summary>
        /// Creates a new instance of the CircuitBreakerRateLimiter
        /// </summary>
        /// <param name="logger">Logger instance</param>
        /// <param name="failureThreshold">Number of failures before opening the circuit</param>
        /// <param name="resetTimeoutSeconds">Seconds to wait before trying to close the circuit</param>
        public CircuitBreakerRateLimiter(
            ILogger logger,
            int failureThreshold = 5, 
            int resetTimeoutSeconds = 30)
        {
            _logger = logger;
            _failureThreshold = failureThreshold;
            _resetTimeout = TimeSpan.FromSeconds(resetTimeoutSeconds);
        }

        /// <summary>
        /// Executes an action with circuit breaker protection
        /// </summary>
        /// <param name="domain">Domain being accessed</param>
        /// <param name="action">Action to execute</param>
        /// <returns>True if the action was executed, false if the circuit is open</returns>
        public async Task<bool> ExecuteAsync(string domain, Func<Task> action)
        {
            var state = _domainStates.GetOrAdd(domain, _ => new DomainCircuitState());
            
            if (!CanExecute(state))
            {
                _logger.LogWarning($"Circuit is open for domain {domain}, request blocked");
                return false;
            }
                
            try
            {
                await action();
                OnSuccess(state);
                return true;
            }
            catch (Exception ex)
            {
                OnFailure(state, domain, ex);
                throw;
            }
        }

        /// <summary>
        /// Executes an action with circuit breaker protection and returns a result
        /// </summary>
        /// <typeparam name="T">Type of result</typeparam>
        /// <param name="domain">Domain being accessed</param>
        /// <param name="action">Action to execute</param>
        /// <returns>Result of the action, or default if the circuit is open</returns>
        public async Task<(bool Executed, T Result)> ExecuteAsync<T>(string domain, Func<Task<T>> action)
        {
            var state = _domainStates.GetOrAdd(domain, _ => new DomainCircuitState());
            
            if (!CanExecute(state))
            {
                _logger.LogWarning($"Circuit is open for domain {domain}, request blocked");
                return (false, default);
            }
                
            try
            {
                var result = await action();
                OnSuccess(state);
                return (true, result);
            }
            catch (Exception ex)
            {
                OnFailure(state, domain, ex);
                throw;
            }
        }

        /// <summary>
        /// Gets the current state of a domain's circuit
        /// </summary>
        /// <param name="domain">Domain to check</param>
        /// <returns>Circuit state information</returns>
        public CircuitStatus GetCircuitStatus(string domain)
        {
            if (!_domainStates.TryGetValue(domain, out var state))
            {
                return new CircuitStatus
                {
                    Domain = domain,
                    State = CircuitState.Closed,
                    FailureCount = 0,
                    OpenUntil = null
                };
            }

            return new CircuitStatus
            {
                Domain = domain,
                State = state.CurrentState,
                FailureCount = state.FailureCount,
                OpenUntil = state.CurrentState == CircuitState.Open ? state.OpenUntil : null
            };
        }

        /// <summary>
        /// Manually reset the circuit for a domain
        /// </summary>
        /// <param name="domain">Domain to reset</param>
        public void ResetCircuit(string domain)
        {
            if (_domainStates.TryGetValue(domain, out var state))
            {
                state.CurrentState = CircuitState.Closed;
                state.FailureCount = 0;
                state.OpenUntil = DateTime.MinValue;
                _logger.LogInformation($"Circuit manually reset for domain {domain}");
            }
        }

        /// <summary>
        /// Determines if an action can be executed based on circuit state
        /// </summary>
        private bool CanExecute(DomainCircuitState state)
        {
            return state.CurrentState switch
            {
                CircuitState.Closed => true,
                CircuitState.Open => DateTime.Now > state.OpenUntil 
                    ? (state.CurrentState = CircuitState.HalfOpen) == CircuitState.HalfOpen 
                    : false,
                CircuitState.HalfOpen => true,
                _ => false
            };
        }

        /// <summary>
        /// Handles successful execution
        /// </summary>
        private void OnSuccess(DomainCircuitState state)
        {
            if (state.CurrentState == CircuitState.HalfOpen)
            {
                state.CurrentState = CircuitState.Closed;
                _logger.LogInformation("Circuit closed after successful test request");
            }
            state.FailureCount = 0;
        }

        /// <summary>
        /// Handles failed execution
        /// </summary>
        private void OnFailure(DomainCircuitState state, string domain, Exception ex)
        {
            state.FailureCount++;
            _logger.LogWarning($"Failure #{state.FailureCount} for domain {domain}: {ex.Message}");

            if (state.CurrentState == CircuitState.HalfOpen || state.FailureCount >= _failureThreshold)
            {
                state.CurrentState = CircuitState.Open;
                state.OpenUntil = DateTime.Now.Add(_resetTimeout);
                _logger.LogWarning($"Circuit opened for domain {domain} until {state.OpenUntil}");
            }
        }

        /// <summary>
        /// Represents the state of a domain's circuit
        /// </summary>
        private class DomainCircuitState
        {
            public CircuitState CurrentState { get; set; } = CircuitState.Closed;
            public int FailureCount { get; set; }
            public DateTime OpenUntil { get; set; }
        }
    }

    /// <summary>
    /// Possible states for a circuit
    /// </summary>
    public enum CircuitState
    {
        /// <summary>
        /// Circuit is closed, requests are allowed
        /// </summary>
        Closed,

        /// <summary>
        /// Circuit is open, requests are blocked
        /// </summary>
        Open,

        /// <summary>
        /// Circuit is testing if it can be closed
        /// </summary>
        HalfOpen
    }

    /// <summary>
    /// Status information for a circuit
    /// </summary>
    public class CircuitStatus
    {
        /// <summary>
        /// Domain this circuit is for
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Current state of the circuit
        /// </summary>
        public CircuitState State { get; set; }

        /// <summary>
        /// Number of consecutive failures
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// When the circuit will try to close again (if open)
        /// </summary>
        public DateTime? OpenUntil { get; set; }
    }
}
