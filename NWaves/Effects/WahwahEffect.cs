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
        /// 
        /// </summary>
        public float Q { get; }

        /// <summary>
        /// 
        /// </summary>
        public float LfoFrequency { get; }

        /// <summary>
        /// 
        /// </summary>
        public float MinFrequency { get; }

        /// <summary>
        /// 
        /// </summary>
        public float MaxFrequency { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lfoFrequency"></param>
        /// <param name="minFrequency"></param>
        /// <param name="maxFrequency"></param>
        /// <param name="q"></param>
        public WahwahEffect(float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 3000, float q = 0.5f)
        {
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;
        }
        
        /// <summary>
        /// Method implements simple wah-wah effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var x = signal.Samples;
            var samplingRateInverted = 2 * Math.PI / signal.SamplingRate;

            var lfo = new TriangleWaveBuilder()
                                    .SetParameter("lo", MinFrequency)
                                    .SetParameter("hi", MaxFrequency)
                                    .SetParameter("freq", LfoFrequency)
                                    .OfLength(signal.Length)
                                    .SampledAt(signal.SamplingRate)
                                    .Build();

            var f = 2 * Math.Sin(lfo[0] * samplingRateInverted);
            
            var yh = new float[x.Length];
            var yb = new float[x.Length];
            var yl = new float[x.Length];

            yh[0] = x[0];
            yb[0] = (float)(f * yh[0]);
            yl[0] = (float)(f * yb[0]);

            for (var i = 1; i < signal.Length; i++)
            {
                yh[i] = x[i] - yl[i - 1] - Q * yb[i - 1];
                yb[i] = (float)(f * yh[i] + yb[i - 1]);
                yl[i] = (float)(f * yb[i] + yl[i - 1]);
                f = 2 * Math.Sin(lfo[i] * samplingRateInverted);
            }

            var maxYb = yb.Max(y => Math.Abs(y));
            
            return new DiscreteSignal(signal.SamplingRate, yb.Select(y => y / maxYb));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
        }
    }
}
