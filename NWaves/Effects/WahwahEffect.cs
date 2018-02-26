using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Effects
{
    /// <summary>
    /// Class for wah-wah effect
    /// </summary>
    public class WahwahEffect : IFilter
    {
        /// <summary>
        /// Method implements simple wah-wah effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var damp = 0.25;

            var x = signal.Samples;

            var triangle = new TriangleWaveBuilder()
                                    .SetParameter("lo", 300.0)
                                    .SetParameter("hi", 2000.0)
                                    .SetParameter("freq", 1.5)
                                    .OfLength(signal.Length)
                                    .SampledAt(signal.SamplingRate)
                                    .Build();

            var f = 2 * Math.Sin(2 * Math.PI * triangle[0] / signal.SamplingRate);
            var q = 2 * damp;

            var yh = new double[x.Length];
            var yb = new double[x.Length];
            var yl = new double[x.Length];

            yh[0] = x[0];
            yb[0] = f * yh[0];
            yl[0] = f * yb[0];

            for (var i = 1; i < signal.Length; i++)
            {
                yh[i] = x[i] - yl[i - 1] - q * yb[i - 1];
                yb[i] = f * yh[i] + yb[i - 1];
                yl[i] = f * yb[i] + yl[i - 1];
                f = 2 * Math.Sin(2 * Math.PI * triangle[i] / signal.SamplingRate);
            }

            var maxYb = yb.Max(y => Math.Abs(y));
            
            return new DiscreteSignal(signal.SamplingRate, yb.Select(y => y / maxYb));
        }
    }
}
