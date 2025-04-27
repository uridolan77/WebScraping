// filepath: c:\dev\WebScraping\WebScraper\Processing\AsyncPipeline.cs
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace WebScraper.Processing
{
    /// <summary>
    /// A robust asynchronous processing pipeline with backpressure control for web scraping operations
    /// </summary>
    /// <typeparam name="TInput">The input type for the pipeline</typeparam>
    /// <typeparam name="TOutput">The output type for the pipeline</typeparam>
    public class AsyncPipeline<TInput, TOutput> : IDisposable
    {
        private readonly Func<TInput, Task<TOutput>> _processor;
        private readonly Action<TInput, Exception> _errorHandler;
        private readonly Action<string> _logger;
        private readonly int _maxDegreeOfParallelism;
        private readonly int _boundedCapacity;
        private readonly int _processingTimeoutMs;
        private readonly ExecutionDataflowBlockOptions _executionOptions;
        private readonly DataflowLinkOptions _linkOptions;
        private readonly BufferBlock<TInput> _inputBuffer;
        private readonly TransformBlock<TInput, TOutput> _processingBlock;
        private readonly BufferBlock<TOutput> _outputBuffer;
        private readonly ConcurrentDictionary<TInput, Stopwatch> _processingTimes = new ConcurrentDictionary<TInput, Stopwatch>();
        private readonly PipelineMetrics _metrics = new PipelineMetrics();
        private readonly Timer _metricsReportingTimer;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the AsyncPipeline class
        /// </summary>
        /// <param name="processor">The function that processes each input item</param>
        /// <param name="maxDegreeOfParallelism">Maximum number of items to process in parallel</param>
        /// <param name="boundedCapacity">Maximum number of items that can be queued</param>
        /// <param name="errorHandler">Handler for errors during processing</param>
        /// <param name="logger">Logger for pipeline events</param>
        /// <param name="processingTimeoutMs">Timeout for processing a single item in milliseconds</param>
        /// <param name="metricsIntervalMs">Interval for reporting metrics in milliseconds</param>
        public AsyncPipeline(
            Func<TInput, Task<TOutput>> processor,
            int maxDegreeOfParallelism = 4,
            int boundedCapacity = 100,
            Action<TInput, Exception> errorHandler = null,
            Action<string> logger = null,
            int processingTimeoutMs = 60000,
            int metricsIntervalMs = 10000)
        {
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _maxDegreeOfParallelism = Math.Max(1, maxDegreeOfParallelism);
            _boundedCapacity = Math.Max(1, boundedCapacity);
            _errorHandler = errorHandler ?? ((item, ex) => { });
            _logger = logger ?? Console.WriteLine;
            _processingTimeoutMs = Math.Max(1000, processingTimeoutMs);
            _cancellationTokenSource = new CancellationTokenSource();

            // Set up dataflow block options
            _executionOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = _boundedCapacity,
                MaxDegreeOfParallelism = _maxDegreeOfParallelism,
                CancellationToken = _cancellationTokenSource.Token,
                EnsureOrdered = false
            };

            _linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            // Create pipeline blocks
            _inputBuffer = new BufferBlock<TInput>(new DataflowBlockOptions 
            { 
                BoundedCapacity = _boundedCapacity,
                CancellationToken = _cancellationTokenSource.Token
            });

            // Processing block with error handling and timeout
            _processingBlock = new TransformBlock<TInput, TOutput>(
                async (input) =>
                {
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    _processingTimes[input] = stopwatch;
                    
                    try
                    {
                        // Create a task that completes after the timeout
                        var timeoutTask = Task.Delay(_processingTimeoutMs, _cancellationTokenSource.Token);
                        
                        // Create the processing task
                        var processingTask = _processor(input);
                        
                        // Wait for either the processing to complete or the timeout to occur
                        var completedTask = await Task.WhenAny(processingTask, timeoutTask);
                        
                        if (completedTask == timeoutTask)
                        {
                            throw new TimeoutException($"Processing timed out after {_processingTimeoutMs}ms");
                        }
                        
                        var result = await processingTask;
                        
                        // Update metrics
                        _metrics.IncrementProcessedCount();
                        _metrics.AddProcessingTime(stopwatch.ElapsedMilliseconds);
                        
                        return result;
                    }
                    catch (Exception ex)
                    {
                        _metrics.IncrementErrorCount();
                        _errorHandler(input, ex);
                        throw; // Let the block handle the error
                    }
                    finally
                    {
                        stopwatch.Stop();
                        _processingTimes.TryRemove(input, out _);
                    }
                },
                _executionOptions);

            // Output buffer with the same bounded capacity
            _outputBuffer = new BufferBlock<TOutput>(new DataflowBlockOptions 
            { 
                BoundedCapacity = _boundedCapacity,
                CancellationToken = _cancellationTokenSource.Token
            });

            // Link the blocks together
            _inputBuffer.LinkTo(_processingBlock, _linkOptions);
            _processingBlock.LinkTo(_outputBuffer, _linkOptions);

            // Set up metrics reporting timer
            if (metricsIntervalMs > 0)
            {
                _metricsReportingTimer = new Timer(ReportMetrics, null, metricsIntervalMs, metricsIntervalMs);
            }

            _logger($"Pipeline initialized with maxParallelism={_maxDegreeOfParallelism}, capacity={_boundedCapacity}");
        }

        /// <summary>
        /// Adds an item to the pipeline for processing
        /// </summary>
        /// <param name="input">The input item to process</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the item was added, false otherwise</returns>
        public async Task<bool> TryAddAsync(TInput input, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncPipeline<TInput, TOutput>));

            try
            {
                // Consider system load for more intelligent backpressure
                double systemLoad = GetSystemLoad();
                
                // Under high system load and significant buffer usage, add delay to naturally 
                // throttle the pipeline to prevent system overload
                if (systemLoad > 0.8 && _inputBuffer.Count > _boundedCapacity * 0.7)
                {
                    int delayMs = CalculateAdaptiveDelay(systemLoad, _inputBuffer.Count);
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                    
                    // After delay, check if we should still proceed (cancellation or disposal)
                    if (_disposed || cancellationToken.IsCancellationRequested || _cancellationTokenSource.IsCancellationRequested)
                    {
                        return false;
                    }
                }
                
                // Use a linked token to respect both the pipeline token and the caller's token
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cancellationTokenSource.Token, cancellationToken);
                
                // Offer the item with a timeout to avoid deadlocks
                bool accepted = await _inputBuffer.SendAsync(input, linkedCts.Token).ConfigureAwait(false);
                
                if (accepted)
                {
                    _metrics.IncrementEnqueuedCount();
                }
                
                return accepted;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Calculate how much delay to add based on system load and queue state
        /// </summary>
        /// <param name="systemLoad">Current system load (0.0-1.0)</param>
        /// <param name="currentQueueSize">Current queue size</param>
        /// <returns>Delay in milliseconds</returns>
        private int CalculateAdaptiveDelay(double systemLoad, int currentQueueSize)
        {
            // Base delay proportional to system load
            double loadFactor = Math.Pow(systemLoad, 2); // Non-linear increase with load
            
            // Queue pressure (how close we are to capacity)
            double queuePressure = (double)currentQueueSize / _boundedCapacity;
            
            // Calculate delay - minimum 50ms, maximum 2000ms (2 seconds)
            int delayMs = (int)(50 + (1950 * loadFactor * queuePressure));
            
            // Log significant delays for monitoring
            if (delayMs > 500)
            {
                _logger($"Backpressure activated: {delayMs}ms delay (load: {systemLoad:F2}, queue: {currentQueueSize}/{_boundedCapacity})");
            }
            
            return delayMs;
        }
        
        /// <summary>
        /// Gets the current system load as a value between 0.0 and 1.0
        /// </summary>
        /// <returns>System load factor</returns>
        private double GetSystemLoad()
        {
            try
            {
                // Use processor time as indicator of system load
                var process = Process.GetCurrentProcess();
                
                // Refresh to get current values
                process.Refresh();
                
                // CPU usage calculation - using total processor time instead of UpTime
                // Process.TotalProcessorTime divided by time since process start
                double cpuLoad = 0.5; // Default to medium load
                
                try {
                    var startTime = process.StartTime;
                    var totalProcessorTime = process.TotalProcessorTime.TotalMilliseconds;
                    var processLifetime = (DateTime.Now - startTime).TotalMilliseconds;
                    
                    if (processLifetime > 0)
                    {
                        cpuLoad = totalProcessorTime / (Environment.ProcessorCount * processLifetime);
                        cpuLoad = Math.Min(1.0, Math.Max(0.0, cpuLoad));
                    }
                }
                catch (Exception) {
                    // If we can't calculate CPU time (e.g. permissions issue), use default value
                }
                
                // Memory pressure is another factor - if we're using more than 80% of allocated memory
                double memoryUsage = 0.5; // Default to medium load
                try {
                    // Calculate memory usage as percentage of working set versus available memory
                    // Use Environment.WorkingSet instead of ComputerInfo which requires extra references
                    var currentProcess = Process.GetCurrentProcess();
                    long workingSet = currentProcess.WorkingSet64;
                    long availableMemory = Environment.SystemPageSize * Environment.ProcessorCount * 1024; // Rough estimate
                    
                    // For a more accurate but rougher calculation, use percentage of physical memory used
                    memoryUsage = currentProcess.WorkingSet64 / (double)(1024 * 1024 * 1024); // As GB
                    memoryUsage = Math.Min(1.0, memoryUsage / 4.0); // Assume 4GB as a reference point
                    
                    memoryUsage = Math.Min(1.0, Math.Max(0.0, memoryUsage));
                }
                catch (Exception) {
                    // If we can't calculate memory usage, use default value
                }
                
                // Combine factors, weighted toward CPU (70% CPU, 30% memory)
                double load = (cpuLoad * 0.7) + (memoryUsage * 0.3);
                
                // Ensure result is between 0.0 and 1.0
                return Math.Min(1.0, Math.Max(0.0, load));
            }
            catch (Exception ex)
            {
                _logger($"Error calculating system load: {ex.Message}. Assuming medium load.");
                return 0.5; // Default to medium load if calculation fails
            }
        }

        /// <summary>
        /// Receives a processed item from the output buffer
        /// </summary>
        /// <param name="timeout">Timeout in milliseconds</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The processed item or default if no item is available</returns>
        public async Task<OutputItem<TOutput>> TryReceiveAsync(
            int timeout = Timeout.Infinite, 
            CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(AsyncPipeline<TInput, TOutput>));

            try
            {
                // Use a linked token to respect both the pipeline token and the caller's token
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    _cancellationTokenSource.Token, cancellationToken);
                
                // If a timeout is specified, create a new CTS that will cancel after the timeout
                if (timeout != Timeout.Infinite)
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(linkedCts.Token);
                    timeoutCts.CancelAfter(timeout);
                    
                    if (await _outputBuffer.OutputAvailableAsync(timeoutCts.Token).ConfigureAwait(false))
                    {
                        if (_outputBuffer.TryReceive(out var output))
                        {
                            _metrics.IncrementDequeuedCount();
                            return new OutputItem<TOutput>(output, true);
                        }
                    }
                }
                else
                {
                    // No timeout specified, wait indefinitely
                    if (await _outputBuffer.OutputAvailableAsync(linkedCts.Token).ConfigureAwait(false))
                    {
                        if (_outputBuffer.TryReceive(out var output))
                        {
                            _metrics.IncrementDequeuedCount();
                            return new OutputItem<TOutput>(output, true);
                        }
                    }
                }
                
                return new OutputItem<TOutput>(default, false);
            }
            catch (OperationCanceledException)
            {
                return new OutputItem<TOutput>(default, false);
            }
        }

        /// <summary>
        /// Gets the current pipeline metrics
        /// </summary>
        /// <returns>The current metrics</returns>
        public PipelineStatus GetStatus()
        {
            return new PipelineStatus
            {
                InputQueueCount = _inputBuffer.Count,
                ProcessingCount = _processingBlock.InputCount,
                OutputQueueCount = _outputBuffer.Count,
                Metrics = _metrics.Clone(),
                ProcessingItems = _processingTimes.Count,
                IsBackpressureEngaged = _inputBuffer.Count >= _boundedCapacity * 0.9, // 90% full
                OldestProcessingItemMs = _processingTimes.Values
                    .Select(s => s.ElapsedMilliseconds)
                    .DefaultIfEmpty(0)
                    .Max(),
                Capacity = _boundedCapacity
            };
        }

        /// <summary>
        /// Reports metrics about the pipeline
        /// </summary>
        private void ReportMetrics(object state)
        {
            var status = GetStatus();
            
            _logger($"Pipeline status: In={status.InputQueueCount}, Processing={status.ProcessingCount}, " +
                   $"Out={status.OutputQueueCount}, Avg={status.Metrics.AverageProcessingTimeMs:F1}ms, " +
                   $"Errors={status.Metrics.ErrorCount}");
            
            if (status.IsBackpressureEngaged)
            {
                _logger("BACKPRESSURE ENGAGED: Pipeline is approaching capacity. Consider reducing input rate.");
            }
            
            if (status.OldestProcessingItemMs > _processingTimeoutMs * 0.8)
            {
                _logger($"WARNING: Some items are taking a long time to process ({status.OldestProcessingItemMs}ms)");
            }
            
            // Reset some metrics for the next interval
            _metrics.ResetInterval();
        }

        /// <summary>
        /// Completes the pipeline and waits for all items to be processed
        /// </summary>
        public async Task CompleteAsync()
        {
            if (_disposed)
                return;

            // Signal that no more items will be added
            _inputBuffer.Complete();
            
            // Wait for all blocks to complete
            try
            {
                await _outputBuffer.Completion.ConfigureAwait(false);
                _logger("Pipeline processing completed");
            }
            catch (Exception ex)
            {
                _logger($"Pipeline completed with errors: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancels the pipeline and stops processing
        /// </summary>
        public void Cancel()
        {
            if (!_disposed && !_cancellationTokenSource.IsCancellationRequested)
            {
                _logger("Pipeline cancellation requested");
                _cancellationTokenSource.Cancel();
            }
        }

        /// <summary>
        /// Disposes of resources used by the pipeline
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                
                _metricsReportingTimer?.Dispose();
                
                try
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource.Dispose();
                }
                catch (Exception ex)
                {
                    _logger($"Error disposing pipeline: {ex.Message}");
                }
                
                _logger("Pipeline disposed");
            }
        }
    }

    /// <summary>
    /// Metrics for the pipeline
    /// </summary>
    public class PipelineMetrics
    {
        private long _enqueuedCount;
        private long _dequeuedCount;
        private long _processedCount;
        private long _errorCount;
        private long _totalProcessingTimeMs;
        private long _totalItemsProcessed;
        
        // Interval statistics
        private long _intervalProcessedCount;
        private long _intervalErrorCount;
        private long _intervalProcessingTimeMs;

        /// <summary>
        /// Number of items enqueued in the pipeline
        /// </summary>
        public long EnqueuedCount => _enqueuedCount;
        
        /// <summary>
        /// Number of items dequeued from the pipeline
        /// </summary>
        public long DequeuedCount => _dequeuedCount;
        
        /// <summary>
        /// Number of items successfully processed
        /// </summary>
        public long ProcessedCount => _processedCount;
        
        /// <summary>
        /// Number of errors encountered during processing
        /// </summary>
        public long ErrorCount => _errorCount;
        
        /// <summary>
        /// Average processing time in milliseconds
        /// </summary>
        public double AverageProcessingTimeMs => _totalItemsProcessed > 0 
            ? (double)_totalProcessingTimeMs / _totalItemsProcessed 
            : 0;
        
        /// <summary>
        /// Average processing time for the current interval in milliseconds
        /// </summary>
        public double IntervalAverageProcessingTimeMs => _intervalProcessedCount > 0 
            ? (double)_intervalProcessingTimeMs / _intervalProcessedCount 
            : 0;
        
        /// <summary>
        /// Number of items processed in the current interval
        /// </summary>
        public long IntervalProcessedCount => _intervalProcessedCount;
        
        /// <summary>
        /// Number of errors encountered in the current interval
        /// </summary>
        public long IntervalErrorCount => _intervalErrorCount;

        internal void IncrementEnqueuedCount()
        {
            Interlocked.Increment(ref _enqueuedCount);
        }

        internal void IncrementDequeuedCount()
        {
            Interlocked.Increment(ref _dequeuedCount);
        }

        internal void IncrementProcessedCount()
        {
            Interlocked.Increment(ref _processedCount);
            Interlocked.Increment(ref _intervalProcessedCount);
        }

        internal void IncrementErrorCount()
        {
            Interlocked.Increment(ref _errorCount);
            Interlocked.Increment(ref _intervalErrorCount);
        }

        internal void AddProcessingTime(long processingTimeMs)
        {
            Interlocked.Add(ref _totalProcessingTimeMs, processingTimeMs);
            Interlocked.Add(ref _intervalProcessingTimeMs, processingTimeMs);
            Interlocked.Increment(ref _totalItemsProcessed);
        }

        /// <summary>
        /// Resets interval statistics
        /// </summary>
        internal void ResetInterval()
        {
            Interlocked.Exchange(ref _intervalProcessedCount, 0);
            Interlocked.Exchange(ref _intervalErrorCount, 0);
            Interlocked.Exchange(ref _intervalProcessingTimeMs, 0);
        }

        /// <summary>
        /// Creates a clone of the metrics
        /// </summary>
        /// <returns>A copy of the current metrics</returns>
        internal PipelineMetrics Clone()
        {
            return new PipelineMetrics
            {
                _enqueuedCount = this._enqueuedCount,
                _dequeuedCount = this._dequeuedCount,
                _processedCount = this._processedCount,
                _errorCount = this._errorCount,
                _totalProcessingTimeMs = this._totalProcessingTimeMs,
                _totalItemsProcessed = this._totalItemsProcessed,
                _intervalProcessedCount = this._intervalProcessedCount,
                _intervalErrorCount = this._intervalErrorCount,
                _intervalProcessingTimeMs = this._intervalProcessingTimeMs
            };
        }
    }

    /// <summary>
    /// Status information for the pipeline
    /// </summary>
    public class PipelineStatus
    {
        /// <summary>
        /// Number of items in the input queue
        /// </summary>
        public int InputQueueCount { get; set; }
        
        /// <summary>
        /// Number of items currently being processed
        /// </summary>
        public int ProcessingCount { get; set; }
        
        /// <summary>
        /// Number of items in the output queue
        /// </summary>
        public int OutputQueueCount { get; set; }
        
        /// <summary>
        /// Current metrics for the pipeline
        /// </summary>
        public PipelineMetrics Metrics { get; set; }
        
        /// <summary>
        /// Number of items currently being processed
        /// </summary>
        public int ProcessingItems { get; set; }
        
        /// <summary>
        /// Age of the oldest item currently being processed in milliseconds
        /// </summary>
        public long OldestProcessingItemMs { get; set; }
        
        /// <summary>
        /// Whether backpressure is currently engaged
        /// </summary>
        public bool IsBackpressureEngaged { get; set; }
        
        /// <summary>
        /// Maximum capacity of the pipeline
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Whether the pipeline is currently running
        /// </summary>
        public bool IsRunning { get; set; } = true;
        
        /// <summary>
        /// Total number of items processed through the pipeline
        /// </summary>
        public int TotalItems { get; set; }
        
        /// <summary>
        /// Number of successfully processed items
        /// </summary>
        public int ProcessedItems { get; set; }
        
        /// <summary>
        /// Number of failed items
        /// </summary>
        public int FailedItems { get; set; }
        
        /// <summary>
        /// Current operation description
        /// </summary>
        public string CurrentOperation { get; set; }
        
        /// <summary>
        /// Time when the pipeline started processing
        /// </summary>
        public DateTime StartTime { get; set; } = DateTime.Now;
        
        /// <summary>
        /// Time when the pipeline finished processing
        /// </summary>
        public DateTime? EndTime { get; set; }
    }

    /// <summary>
    /// Output item from the pipeline
    /// </summary>
    /// <typeparam name="T">Type of the item</typeparam>
    public readonly struct OutputItem<T>
    {
        /// <summary>
        /// Item value
        /// </summary>
        public readonly T Value { get; }
        
        /// <summary>
        /// Whether an item was received
        /// </summary>
        public readonly bool Success { get; }

        /// <summary>
        /// Initializes a new instance of the OutputItem struct
        /// </summary>
        /// <param name="value">The output value</param>
        /// <param name="success">Whether the output was successfully received</param>
        public OutputItem(T value, bool success)
        {
            Value = value;
            Success = success;
        }
    }
    
    /// <summary>
    /// Helper methods to convert between different PipelineStatus types
    /// </summary>
    public static class PipelineStatusExtensions
    {
        /// <summary>
        /// Convert from Processing namespace PipelineStatus to the WebScraper namespace version
        /// </summary>
        public static WebScraper.PipelineStatus ToLegacyPipelineStatus(this PipelineStatus status)
        {
            if (status == null) return null;
            
            return new WebScraper.PipelineStatus
            {
                IsRunning = status.IsRunning,
                TotalItems = status.TotalItems,
                ProcessedItems = status.ProcessedItems,
                FailedItems = status.FailedItems,
                CurrentOperation = status.CurrentOperation,
                StartTime = status.StartTime,
                EndTime = status.EndTime
            };
        }
        
        /// <summary>
        /// Convert from WebScraper namespace PipelineStatus to the Processing namespace version
        /// </summary>
        public static PipelineStatus ToProcessingPipelineStatus(this WebScraper.PipelineStatus status)
        {
            if (status == null) return null;
            
            return new PipelineStatus
            {
                IsRunning = status.IsRunning,
                TotalItems = status.TotalItems,
                ProcessedItems = status.ProcessedItems,
                FailedItems = status.FailedItems,
                CurrentOperation = status.CurrentOperation,
                StartTime = status.StartTime,
                EndTime = status.EndTime
            };
        }
    }
}