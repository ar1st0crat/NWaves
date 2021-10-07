using BenchmarkDotNet.Attributes;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class OfflineFilterForLoopVsLinq
    {
        private const int N = 16000 * 300;

        private readonly DiscreteSignal _signal;
        private readonly IirFilter _filter;
        private readonly IirFilterLinq _filterLinq;

        public OfflineFilterForLoopVsLinq()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            _filter = new IirFilter(new[] { 1, 0.2, -0.3, 0.1 }, new[] { 1, -0.7, 0.4 });
            _filterLinq = new IirFilterLinq(new[] { 1, 0.2, -0.3, 0.1 }, new[] { 1, -0.7, 0.4 });
        }

        [Benchmark]
        public void OfflineFilterForLoop()
        {
            var output = _filter.ApplyTo(_signal);
        }

        [Benchmark]
        public void OfflineFilterLinq()
        {
            var output = _filterLinq.ApplyTo(_signal);
        }

        //[Benchmark]
        //public void OfflineFilterArrayForLoop()
        //{
        //    var output = new float[_signal.Length];
        //    var samples = _signal.Samples;

        //    for (var i = 0; i < samples.Length; i++)
        //    {
        //        output[i] = _filter.Process(samples[i]);
        //    }
        //}

        //[Benchmark]
        //public void OfflineFilterArrayForeach()
        //{
        //    var output = new float[_signal.Length];
        //    var samples = _signal.Samples;

        //    var i = 0;
        //    foreach (var s in samples)
        //    {
        //        output[i++] = _filter.Process(s);
        //    }
        //}

        //[Benchmark]
        //public void OfflineFilterLinq()
        //{
        //    var output = _signal.Samples.Select(s => _filter.Process(s)).ToArray();
        //}
    }
}
