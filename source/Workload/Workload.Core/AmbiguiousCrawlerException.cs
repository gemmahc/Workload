using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolyPoly.Core
{
    internal class AmbiguiousCrawlerException : Exception
    {
        public AmbiguiousCrawlerException()
        {
        }

        public AmbiguiousCrawlerException(string? message) : base(message)
        {
        }
    }
}
