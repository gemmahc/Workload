using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RolyPoly.Core
{
    public interface ICrawlerFactory
    {
        public T Create<T>(Uri endpoint) where T : Crawler;
    }
}
