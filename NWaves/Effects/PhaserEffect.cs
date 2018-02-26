using System;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for phaser effect
    /// </summary>
    public class PhaserEffect : IFilter
    {
        /// <summary>
        /// Method implements simple phaser effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var damp = 0.75;

            var x = signal.Samples;
            var samplingRate = signal.SamplingRate;

            var triangle = new TriangleWaveBuilder()
                                    .SetParameter("lo", 300.0)
                                    .SetParameter("hi", 3000.0)
                                    .SetParameter("freq", 1.5)
                                    .OfLength(signal.Length)
                                    .SampledAt(samplingRate)
                                    .Build();

            //var q = 2 * damp;

            var f = 2 * Math.PI * triangle[0] / samplingRate;
            var c = (Math.Tan(f) - 1) / (Math.Tan(f) + 1);
            var d = -Math.Cos(f);

            var y = new double[x.Length];
            var yl = new double[x.Length];

            //y[0] = x[0] * (1 - c) / 2;
            //y[1] = ;

            for (var i = 2; i < signal.Length; i++)
            {
                yl[i] = -c * x[i] + d * (1 - c) * x[i - 1] + x[i - 2] - d * (1 - c) * yl[i - 1] + c * yl[i - 2];
                y[i] = (yl[i] + x[i]) / 2;

                f = 2 * Math.PI * triangle[i] / samplingRate;
                c = (Math.Tan(f) - 1) / (Math.Tan(f) + 1);
                d = -Math.Cos(f);
            }

            return new DiscreteSignal(samplingRate, y);
        }
    }
}
