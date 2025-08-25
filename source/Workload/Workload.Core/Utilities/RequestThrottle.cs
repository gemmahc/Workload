using System.Threading.RateLimiting;

namespace RolyPoly.Core.Utilities
{
    public class RequestThrottle : IDisposable
    {
        private RateLimiter _limiter;
        private bool disposedValue;
        private readonly RequestThrottleOptions _options;

        /// <summary>
        /// Creates a new request throttle to manage concurrent requests using a replenishing Token Bucket mechanism
        /// </summary>
        /// <param name="options">The options that define the operating parameters of the throttle.</param>
        public RequestThrottle(RequestThrottleOptions options)
        {
            _options = options;

            var opts = new TokenBucketRateLimiterOptions()
            {
                ReplenishmentPeriod = _options.Interval,
                TokensPerPeriod = _options.MaxConcurrentRequests,
                TokenLimit = _options.MaxConcurrentRequests,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = _options.MaxQueuedRequests,
                AutoReplenishment = true
            };

            _limiter = new TokenBucketRateLimiter(opts);
        }

        /// <summary>
        /// Executes the specified unit of work when the throttle allows.
        /// </summary>
        /// <param name="unitOfWork">The unit of work to execute</param>
        /// <returns>The result of the unit of work</returns>
        /// <exception cref="RequestThrottleException">Throws when the total queued requests exceeds the maximum allowed for this throttle instance.</exception>
        public async Task<T> RunAsync<T>(Func<Task<T>> unitOfWork)
        {
            using (var lease = await _limiter.AcquireAsync())
            {
                if (!lease.IsAcquired)
                {
                    throw new RequestThrottleException($"Throttle request queue exceeded limit of [{_options.MaxQueuedRequests}] requests");
                }

                return await Task.Run(() => unitOfWork());
            }
        }

        public TimeSpan? IdleDuration
        {
            get
            {
                return _limiter.IdleDuration;
            }
        }

        #region IDisposable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_limiter != null)
                    {

                        _limiter.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        _limiter = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
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
