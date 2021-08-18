using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;
using System;
using System.Linq;

namespace NWaves.Benchmarks
{
    class TestOutputConsistency
    {
        private const int N = 5000000;

        private readonly DiscreteSignal _signal;

        private readonly IirFilterV4 _filterV4BiQuad;
        private readonly IirFilter _filterV5BiQuad;
        private readonly ZiFilter _filterZiBiQuad;

        private readonly IirFilterV4 _filterV4Butterworth6;
        private readonly IirFilter _filterV5Butterworth6;
        private readonly ZiFilter _filterZiButterworth6;

        public TestOutputConsistency()
        {
            _signal = new WhiteNoiseBuilder().OfLength(N).Build();

            var biquad = new Filters.BiQuad.LowPassFilter(0.1);
            var butter = new Filters.Butterworth.LowPassFilter(0.1, 6);

            _filterV4BiQuad = new IirFilterV4(biquad.Tf);
            _filterV5BiQuad = new IirFilter(biquad.Tf);
            _filterZiBiQuad = new ZiFilter(biquad.Tf);
            _filterV4Butterworth6 = new IirFilterV4(butter.Tf);
            _filterV5Butterworth6 = new IirFilter(butter.Tf);
            _filterZiButterworth6 = new ZiFilter(butter.Tf);
        }

        public void Run()
        {
            var output2 = new float[_signal.Length];
            var output4 = new float[_signal.Length];
            var output5 = new float[_signal.Length];
            var outputZi = new float[_signal.Length];

            var samples = _signal.Samples;

            for (var i = 0; i < samples.Length; i++)
            {
                output4[i] = _filterV4BiQuad.Process(samples[i]);
                output5[i] = _filterV5BiQuad.Process(samples[i]);
                outputZi[i] = _filterZiBiQuad.Process(samples[i]);
            }

            var diffAverageV4 = output5.Zip(output4, (o5, o4) => o5 - o4).Sum() / N;
            var diffAverageZi = output5.Zip(outputZi, (o5, zi) => o5 - zi).Sum() / N;

            Console.WriteLine($"Average difference Ver.0.9.5 vs. Ver.0.9.4 : {diffAverageV4}");
            Console.WriteLine($"Average difference IirFilter vs. ZiFilter : {diffAverageZi}");

            for (var i = 0; i < samples.Length; i++)
            {
                output4[i] = _filterV4Butterworth6.Process(samples[i]);
                output5[i] = _filterV5Butterworth6.Process(samples[i]);
                outputZi[i] = _filterZiButterworth6.Process(samples[i]);
            }

            diffAverageV4 = output5.Zip(output4, (o5, o4) => o5 - o4).Sum() / N;
            diffAverageZi = output5.Zip(outputZi, (o5, zi) => o5 - zi).Sum() / N;

            Console.WriteLine($"Average difference Ver.0.9.5 vs. Ver.0.9.4 : {diffAverageV4}");
            Console.WriteLine($"Average difference IirFilter vs. ZiFilter : {diffAverageZi}");
        }
    }
}
