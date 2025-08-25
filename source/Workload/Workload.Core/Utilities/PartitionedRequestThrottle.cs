using System.Collections.Concurrent;

namespace RolyPoly.Core.Utilities
{
    /// <summary>
    /// Class to maintain separate throttles partitioned by key
    /// </summary>
    public class PartitionedRequestThrottle : IDisposable
    {
        private ConcurrentDictionary<string, RequestThrottle> _throttles;
        private bool _disposedValue;
        private RequestThrottleOptions _options;

        // Resources used for the packground task cleaning up idle throttle partitions.
        private TimeSpan? _partitionIdleTimeout;
        private Task? _partitionClearnupWorker;
        private CancellationTokenSource? _tokenSource;

        public PartitionedRequestThrottle(RequestThrottleOptions options, TimeSpan? partitionIdleTimeout = null)
        {
            _options = options;
            _throttles = new ConcurrentDictionary<string, RequestThrottle>();
            _partitionIdleTimeout = partitionIdleTimeout;

            if (_partitionIdleTimeout != null)
            {
                StartPartitionCleanupWorker();
            }
        }

        public async Task<T> RunAsync<T>(string partitionKey, Func<Task<T>> unitOfWork)
        {
            if (!_throttles.ContainsKey(partitionKey))
            {
                Console.WriteLine($"Adding throttle for key [{partitionKey}]");
                _throttles.AddOrUpdate(
                    partitionKey,
                    new RequestThrottle(_options),
                    (key, existing) => { return existing; });
            }

            return await _throttles[partitionKey].RunAsync(unitOfWork);
        }

        private void StartPartitionCleanupWorker()
        {
            _tokenSource = new CancellationTokenSource();
            _partitionClearnupWorker = Task.Run(() => 
            {
                try
                {
                    while (!_tokenSource.IsCancellationRequested)
                    {
                        Task.Delay(TimeSpan.FromSeconds(10), _tokenSource.Token).GetAwaiter().GetResult();
                        var expired = _throttles.Where(kvp => kvp.Value.IdleDuration > _partitionIdleTimeout).ToList();

                        foreach (var kvp in expired)
                        {
                            Console.WriteLine($"Removing partition throttle [{kvp.Key}]");
                            if (_throttles.Remove(kvp.Key, out var throttle))
                            {
                                throttle.Dispose();
                            }

                        }

                    }
                }
                catch (TaskCanceledException) { }
            }, _tokenSource.Token);
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    if (_partitionClearnupWorker != null)
                    {
                        _tokenSource?.Cancel();
                        _partitionClearnupWorker.GetAwaiter().GetResult();
                        _partitionClearnupWorker.Dispose();
                    }

                    if (_throttles != null)
                    {
                        foreach (var kvp in _throttles)
                        {
                            kvp.Value.Dispose();
                        }
                    }
                    if (_partitionClearnupWorker != null)
                    {
                        _partitionClearnupWorker.Dispose();
                    }

                    _throttles?.Clear();
                    _tokenSource?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
