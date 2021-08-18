using BenchmarkDotNet.Attributes;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Benchmarks
{
    [MemoryDiagnoser]
    public class IirFiltersVersion2Vs4Vs5VsZi
    {
        private const int N = 5000000;

        private readonly DiscreteSignal _signal;

        private readonly IirFilterV2 _filterV2BiQuad;
        private readonly IirFilterV4 _filterV4BiQuad;
        private readonly IirFilter _filterV5BiQuad;
        private readonly ZiFilter _filterZiBiQuad;

        private readonly IirFilterV2 _filterV2Butterworth6;
        private readonly IirFilterV4 _filterV4Butterworth6;
        private readonly IirFilter _filterV5Butterworth6;
        private readonly ZiFilter _filterZiButterworth6;

        public IirFiltersVersion2Vs4Vs5VsZi()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            var biquad = new Filters.BiQuad.LowPassFilter(0.1);
            var butter = new Filters.Butterworth.LowPassFilter(0.1, 6);

            _filterV2BiQuad = new IirFilterV2(biquad.Tf);
            _filterV4BiQuad = new IirFilterV4(biquad.Tf);
            _filterV5BiQuad = new IirFilter(biquad.Tf);
            _filterZiBiQuad = new ZiFilter(biquad.Tf);
            _filterV2Butterworth6 = new IirFilterV2(butter.Tf);
            _filterV4Butterworth6 = new IirFilterV4(butter.Tf);
            _filterV5Butterworth6 = new IirFilter(butter.Tf);
            _filterZiButterworth6 = new ZiFilter(butter.Tf);
        }

        [Benchmark]
        public void FilterVersion092BiQuad()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV2BiQuad.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion094BiQuad()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV4BiQuad.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion095BiQuad()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV5BiQuad.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterZiBiQuad()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterZiBiQuad.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion092Butterworth6()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV2Butterworth6.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion094Butterworth6()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV4Butterworth6.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterVersion095Butterworth6()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterV5Butterworth6.Process(samples[i]);
            }
        }

        [Benchmark]
        public void FilterZiButterworth6()
        {
            var output = new float[_signal.Length];
            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output[i] = _filterZiButterworth6.Process(samples[i]);
            }
        }
    }
}
