using BenchmarkDotNet.Attributes;
using NWaves.Filters;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class MedianFilters
    {
        private const int N = 100000;

        private readonly DiscreteSignal _signal;
        private readonly MedianFilter _filter;
        private readonly MedianFilter2 _filter2;

        public MedianFilters()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            _filter = new MedianFilter(15);
            _filter2 = new MedianFilter2(15);
        }

        [Benchmark]
        public void MedianFilter()
        {
            var output = _filter.ApplyTo(_signal);
        }

        [Benchmark]
        public void MedianFilter2()
        {
            var output = _filter2.ApplyTo(_signal);
        }
    }
}
