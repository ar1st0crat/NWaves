using NWaves.Filters.Base;
using NWaves.Signals;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Benchmarks
{
    class IirFilterLinq : IirFilter
    {
        public IirFilterLinq(TransferFunction tf) : base(tf)
        {
        }

        public IirFilterLinq(IEnumerable<float> b, IEnumerable<float> a) : base(b, a)
        {
        }

        public IirFilterLinq(IEnumerable<double> b, IEnumerable<double> a) : base(b, a)
        {
        }

        public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
