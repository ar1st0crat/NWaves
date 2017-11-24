﻿using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Base class for all kinds of LTI filters.
    /// Provides general algorithms for computing impulse and frequency responses
    /// and leaves method ApplyTo() abstract.
    /// </summary>
    public abstract class LtiFilter : IFilter
    {
        /// <summary>
        /// The filtering algorithm that should be implemented by particular subclass
        /// </summary>
        /// <param name="signal">Signal for filtering</param>
        /// <param name="filteringOptions">General filtering strategy</param>
        /// <returns>Filtered signal</returns>
        public abstract DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringOptions filteringOptions = FilteringOptions.Auto);

        /// <summary>
        /// Zeros of the transfer function
        /// </summary>
        public abstract ComplexDiscreteSignal Zeros { get; set; }

        /// <summary>
        /// Poles of the transfer function
        /// </summary>
        public abstract ComplexDiscreteSignal Poles { get; set; }

        /// <summary>
        /// Returns the complex frequency response of a filter.
        /// 
        /// Method calculates the Frequency Response of a filter
        /// by taking FFT of an impulse response (possibly truncated).
        /// </summary>
        /// <param name="length">Number of frequency response samples</param>
        public virtual ComplexDiscreteSignal FrequencyResponse(int length = 512)
        {
            var real = ImpulseResponse(length).Samples;
            var imag = new double[length];

            Fft.Direct(real, imag, length);

            return new ComplexDiscreteSignal(1, real, imag);
        }

        /// <summary>
        /// Returns the real-valued impulse response of a filter.
        /// 
        /// Method calculates the Impulse Response of a filter
        /// by feeding the unit impulse into it.
        /// </summary>
        /// <param name="length">
        /// The length of an impulse reponse.
        /// If the filter is IIR, then it's the length of truncated infinite impulse reponse.
        /// </param>
        public virtual DiscreteSignal ImpulseResponse(int length = 512)
        {
            var impulse = new DiscreteSignal(1, length) { [0] = 1.0 };
            return ApplyTo(impulse);
        }

        /// <summary>
        /// Method for converting zeros(poles) to TF numerator(denominator)
        /// </summary>
        /// <param name="zp"></param>
        /// <returns></returns>
        public static double[] ZpToTf(ComplexDiscreteSignal zp)
        {
            var re = zp.Real;
            var im = zp.Imag;

            var tf = new ComplexDiscreteSignal(1, new[] { 1.0, -re[0] }, new[] { 0.0, -im[0] });

            for (var k = 1; k < re.Length; k++)
            {
                tf = Operation.Convolve(tf, new ComplexDiscreteSignal(1, new[] { 1.0, -re[k] }, new[] { 0.0, -im[k] }));
            }

            return tf.Real;
        }

        /// <summary>
        /// Method for converting TF numerator(denominator) to zeros(poles)
        /// </summary>
        /// <param name="tf"></param>
        /// <returns></returns>
        public static ComplexDiscreteSignal TfToZp(double[] tf)
        {
            if (tf.Length <= 1)
            {
                return null;
            }

            var roots = MathUtils.PolynomialRoots(tf.Reverse().ToArray(), new double[tf.Length]);

            return new ComplexDiscreteSignal(1, roots.Item1, roots.Item2);
        }
    }
}
