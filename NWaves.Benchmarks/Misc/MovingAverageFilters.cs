using BenchmarkDotNet.Attributes;
using NWaves.Filters;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class MovingAverageFilters
    {
        private const int N = 1000000;

        private readonly DiscreteSignal _signal;
        private readonly MovingAverageFilter _filter;
        private readonly MovingAverageRecursiveFilter _filterRec;

        public MovingAverageFilters()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            _filter = new MovingAverageFilter(7);
            _filterRec = new MovingAverageRecursiveFilter(7);
        }

        [Benchmark]
        public void MovingAverageFilter()
        {
            var output = _filter.ApplyTo(_signal);
        }

        [Benchmark]
        public void MovingAverageFilterRecursive()
        {
            var output = _filterRec.ApplyTo(_signal);
        }
    }
}
