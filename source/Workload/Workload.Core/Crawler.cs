using System.Collections.Concurrent;

namespace RolyPoly.Core
{
    public abstract class Crawler
    {
        private const int MAX_VISIT_RETRIES = 3;

        private static ConcurrentDictionary<string, Visit> _visited = new ConcurrentDictionary<string, Visit>(StringComparer.OrdinalIgnoreCase);

        private Dictionary<Uri, Type> _toDispatch;

        protected static ConcurrentDictionary<string, Visit> Visited
        {
            get
            {
                if (_visited == null)
                {
                    _visited = new ConcurrentDictionary<string, Visit>(StringComparer.OrdinalIgnoreCase);
                }

                return _visited;
            }
        }


        public Uri Entrypoint { get; }

        protected Crawler(Uri entrypoint)
        {
            _toDispatch = new Dictionary<Uri, Type>();
            Entrypoint = entrypoint;
        }

        public async Task<CrawlerResult> RunAsync()
        {
            Visit visit = new Visit()
            {
                Visits = 1,
                LastResult = Result.Pending
            };

            if (Visited.ContainsKey(Entrypoint.ToString()))
            {
                visit = Visited[Entrypoint.ToString()];

                if (visit.LastResult == Result.Success || visit.LastResult == Result.Pending)
                {
                    // either already crawled or are in the process of it. Skip here.
                    return new CrawlerResult(Result.Duplicate);
                }
                else if (visit.LastResult == Result.Failure && visit.Visits >= MAX_VISIT_RETRIES)
                {
                    // already attempted and failed to run the crawler for this site. Log, and skip
                    return new CrawlerResult(Result.Failure);
                }
            }

            // Set to pending before running the crawler
            Visited.AddOrUpdate(Entrypoint.ToString(), visit, (key, old) =>
            {
                old.Visits = old.Visits + 1;
                old.LastResult = Result.Pending;
                return old;
            });


            try
            {
                await RunCrawlerAsync();

                visit.LastResult = Result.Success;
                var result = new CrawlerResult(visit.LastResult);
                foreach(var kvp in _toDispatch)
                {
                    result.ToDispatch.Add(kvp.Key, kvp.Value);
                }

                return result;
            }
            catch (Exception)
            {
                visit.LastResult = Result.Failure;
                return new CrawlerResult(visit.LastResult);
            }
            finally
            {
                Visited.AddOrUpdate(Entrypoint.ToString(), visit, (key, old) =>
                {
                    old.LastResult = visit.LastResult;
                    return old;
                });
            }
        }

        protected void DispatchChild<T>(Uri endpoint) where T : Crawler
        {
            if(_toDispatch.ContainsKey(endpoint))
            {
                if (_toDispatch[endpoint] != typeof(T))
                {
                    throw new AmbiguiousCrawlerException($"Cannot dispatch crawler on [{endpoint}] of type [{typeof(T).Name}]. Crawler [{_toDispatch[endpoint].Name}] is already requested for that location.");
                }
            }
            else
            {
                _toDispatch.Add(endpoint, typeof(T));
            }
        }

        protected abstract Task RunCrawlerAsync();
    }
}
