using BenchmarkDotNet.Attributes;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;
using System.Linq;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class OfflineFilterArrayVsLinq
    {
        private const int N = 1000000;

        private readonly DiscreteSignal _signal;
        private readonly FirFilter _filter;

        public OfflineFilterArrayVsLinq()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();
            _filter = new MovingAverageFilter(5);
        }

        [Benchmark]
        public void OfflineFilterArrayForLoop()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filter.Process(samples[i]);
            }
        }

        [Benchmark]
        public void OfflineFilterArrayForeach()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            var i = 0;
            foreach (var s in samples)
            {
                output[i++] = _filter.Process(s);
            }
        }

        [Benchmark]
        public void OfflineFilterLinq()
        {
            var output = _signal.Samples.Select(s => _filter.Process(s)).ToArray();
        }
    }
}
