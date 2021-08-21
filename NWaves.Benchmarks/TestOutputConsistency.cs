using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Filters.Base64;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Signals.Builders;
using NWaves.Utils;
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

            var diffAverageV4 = output5.Zip(output4, (o5, o4) => Math.Abs(o5 - o4)).Average();
            var diffAverageZi = output5.Zip(outputZi, (o5, zi) => Math.Abs(o5 - zi)).Average();

            Console.WriteLine($"Average difference Ver.0.9.5 vs. Ver.0.9.4 : {diffAverageV4}");
            Console.WriteLine($"Average difference IirFilter vs. ZiFilter : {diffAverageZi}");

            for (var i = 0; i < samples.Length; i++)
            {
                output4[i] = _filterV4Butterworth6.Process(samples[i]);
                output5[i] = _filterV5Butterworth6.Process(samples[i]);
                outputZi[i] = _filterZiButterworth6.Process(samples[i]);
            }

            diffAverageV4 = output5.Zip(output4, (o5, o4) => Math.Abs(o5 - o4)).Average();
            diffAverageZi = output5.Zip(outputZi, (o5, zi) => Math.Abs(o5 - zi)).Average();

            Console.WriteLine($"Average difference Ver.0.9.5 vs. Ver.0.9.4 : {diffAverageV4}");
            Console.WriteLine($"Average difference IirFilter vs. ZiFilter : {diffAverageZi}");


            // === MISC ====

            var med = new MedianFilter();
            var med2 = new MedianFilter2();

            var medOut = med.ApplyTo(_signal).Samples;
            var medOut2 = med2.ApplyTo(_signal).Samples;

            var diffAverageMed = medOut.Zip(medOut, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference MedianFilter vs. MedianFilter2 : {diffAverageMed}");


            var ma = new MovingAverageFilter();
            var maRec = new MovingAverageRecursiveFilter();

            var maOut = ma.ApplyTo(_signal).Samples;
            var maRecOut = maRec.ApplyTo(_signal).Samples;

            var diffAverageMa = maOut.Zip(maRecOut, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference MovingAverageFilter vs. MovingAverageRecursiveFilter : {diffAverageMa}");


            // 32bit vs. 64bit

            var fir32 = new FirFilter(DesignFilter.FirWinLp(7, 0.1));
            var fir64 = new FirFilter64(DesignFilter.FirWinLp(7, 0.1));

            var fir32Out = fir32.ApplyTo(_signal).Samples;
            var fir64Out = fir64.ApplyTo(_signal.Samples.ToDoubles());

            var diffAverageFir = fir64Out.Zip(fir32Out, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference FirFilter vs. FirFilter64 : {diffAverageFir}");


            var iir32 = new IirFilter(_filterV5Butterworth6.Tf);
            var iir64 = new IirFilter64(_filterV5Butterworth6.Tf);

            var iir32Out = iir32.ApplyTo(_signal).Samples;
            var iir64Out = iir64.ApplyTo(_signal.Samples.ToDoubles());

            var diffAverageIir = iir64Out.Zip(iir32Out, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference IirFilter vs. IirFilter64 : {diffAverageIir}");


            var zi32 = new ZiFilter(_filterV5Butterworth6.Tf);
            var zi64 = new ZiFilter64(_filterV5Butterworth6.Tf);

            var zi32Out = zi32.ApplyTo(_signal).Samples;
            var zi64Out = zi64.ApplyTo(_signal.Samples.ToDoubles());

            var diffAverageZis = zi64Out.Zip(zi32Out, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference ZiFilter vs. ZiFilter64 : {diffAverageZis}");

            zi32Out = zi32.ZeroPhase(_signal).Samples;
            zi64Out = zi64.ZeroPhase(_signal.Samples.ToDoubles());

            var diffAverageZiZeroPhase = zi64Out.Zip(zi32Out, (m1, m2) => Math.Abs(m1 - m2)).Average();
            Console.WriteLine($"Average difference ZiFilter vs. ZiFilter64 (zero-phase): {diffAverageZiZeroPhase}");
        }
    }
}
