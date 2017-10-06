using System.Collections.Generic;
using NWaves.Filters.Base;

namespace NWaves.Filters
{
    public class BiQuadFilter : IirFilter
    {
        public BiQuadFilter(IEnumerable<double> b,
                            IEnumerable<double> a,
                            int impulseResponseLength = 512)
            : base(b, a, impulseResponseLength)
        {
        }
    }
}
