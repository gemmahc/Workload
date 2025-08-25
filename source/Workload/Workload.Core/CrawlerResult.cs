using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolyPoly.Core
{
    public class CrawlerResult
    {
        public CrawlerResult()
        {
            Result = Result.None;
            ToDispatch = new Dictionary<Uri, Type>();
        }

        public CrawlerResult(Result result) : this()
        {
            Result = result;
        }

        /// <summary>
        /// The final result of the crawler's execution.
        /// </summary>
        public Result Result { get; set; }

        /// <summary>
        /// Crawlers requested by the execution of this crawler.
        /// </summary>
        public IDictionary<Uri, Type> ToDispatch { get; private set; }
    }
}
