using System;
using NWaves.Filters.Base;
using NWaves.Filters.BiQuad;
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
        public PhaserEffect(float lfoFrequency = 1.0f, float minFrequency = 300, float maxFrequency = 3000, float q = 0.5f)
        {
            LfoFrequency = lfoFrequency;
            MinFrequency = minFrequency;
            MaxFrequency = maxFrequency;
            Q = q;
        }

        /// <summary>
        /// Method implements simple phaser effect
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var x = signal.Samples;
            var samplingRateInverted = 1.0 / signal.SamplingRate;

            var lfo = new TriangleWaveBuilder()
                                    .SetParameter("lo", MinFrequency)
                                    .SetParameter("hi", MaxFrequency)
                                    .SetParameter("freq", LfoFrequency)
                                    .OfLength(signal.Length)
                                    .SampledAt(signal.SamplingRate)
                                    .Build();

            var y = new float[x.Length];

            for (var i = 2; i < signal.Length; i++)
            {
                var filter = new NotchFilter((float)(lfo[i] * samplingRateInverted), Q);

                var b = filter.Tf.Numerator;
                var a = filter.Tf.Denominator;

                y[i] = (float)(b[0] * x[i] + b[1] * x[i - 1] + b[2] * x[i - 2] - (a[1] * y[i - 1] + a[2] * y[i - 2]));
            }

            return new DiscreteSignal(signal.SamplingRate, y);
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
