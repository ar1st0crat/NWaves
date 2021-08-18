using BenchmarkDotNet.Attributes;
using NWaves.Filters.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class FirFiltersVersion2Vs5VsZi
    {
        private const int N = 1000000;

        private readonly DiscreteSignal _signal;
        private readonly FirFilter _filterV5Kernel5;
        private readonly FirFilterV2 _filterV2Kernel5;
        private readonly ZiFilter _filterZiKernel5;
        private readonly FirFilter _filterV5Kernel35;
        private readonly FirFilterV2 _filterV2Kernel35;
        private readonly ZiFilter _filterZiKernel35;

        public FirFiltersVersion2Vs5VsZi()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            _filterV5Kernel5 = new FirFilter(DesignFilter.FirWinLp(5, 0.1));
            _filterV2Kernel5 = new FirFilterV2(DesignFilter.FirWinLp(5, 0.1));
            _filterZiKernel5 = new ZiFilter(DesignFilter.FirWinLp(5, 0.1), new[] { 1.0 });
            _filterV5Kernel35 = new FirFilter(DesignFilter.FirWinLp(35, 0.1));
            _filterV2Kernel35 = new FirFilterV2(DesignFilter.FirWinLp(35, 0.1));
            _filterZiKernel35 = new ZiFilter(DesignFilter.FirWinLp(35, 0.1), new[] { 1.0 });
        }

        [Benchmark]
        public void FilterVersion092Kernel5()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV2Kernel5.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion095Kernel5()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV5Kernel5.Process(samples[i]);
            }
        }

        [Benchmark]
        public void ZiFilterKernel5()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterZiKernel5.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion092Kernel35()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV2Kernel35.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion095Kernel35()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV5Kernel35.Process(samples[i]);
            }
        }

        [Benchmark]
        public void ZiFilterKernel35()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterZiKernel35.Process(samples[i]);
            }
        }
    }
}
