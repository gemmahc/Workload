using RolyPoly.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RolyPoly.Core
{
    public interface IPage
    {
        public Task Init(PartitionedRequestThrottle throttle);

        public Task<T> Extract<T>();
    }
}
