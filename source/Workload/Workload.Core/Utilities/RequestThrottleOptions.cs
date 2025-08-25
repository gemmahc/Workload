namespace RolyPoly.Core.Utilities
{
    public class RequestThrottleOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of concurrent requests allowed to execute in the set interval.
        /// </summary>
        public int MaxConcurrentRequests { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of requests allowed in the throttle queue.
        /// </summary>
        public int MaxQueuedRequests { get; set; }

        /// <summary>
        /// Gets or sets the time interval to throttle within.
        /// </summary>
        public TimeSpan Interval { get; set; }
    }
}
