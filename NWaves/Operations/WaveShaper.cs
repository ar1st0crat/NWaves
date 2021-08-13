using NWaves.Filters.Base;
using NWaves.Signals;
using System;
using System.Linq;

namespace NWaves.Operations
{
    public class WaveShaper : IFilter, IOnlineFilter
    {
        private readonly Func<float, float> _waveShaperFunction;

        public WaveShaper(Func<float, float> waveShaperFunction)
        {
            _waveShaperFunction = waveShaperFunction;
        }

        public float Process(float input) => _waveShaperFunction(input);

        public void Reset()
        {
        }

        public DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => Process(s)));
        }
    }
}
