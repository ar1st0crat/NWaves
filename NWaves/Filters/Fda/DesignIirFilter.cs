using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Static class providing basic methods for filter design and analysis
    /// </summary>
    public static partial class DesignFilter
    {
        #region Iir(Notch|Peak|Comb) filter design

        /// <summary>
        /// Design IIR notch filter
        /// </summary>
        /// <param name="freq">Normalized frequency (must be in range [0, 0.5])</param>
        /// <param name="q">Quality factor (characterizes notch filter -3dB bandwidth relative to its center frequency)</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirNotch(double freq, double q = 20.0)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var w0 = 2 * freq * Math.PI;
            var bw = w0 / q;
            var gb = 1 / Math.Sqrt(2);

            var beta = Math.Sqrt(1 - gb * gb) / gb * Math.Tan(bw / 2);

            var gain = 1 / (1 + beta);

            var num = new[] { gain, -2 * Math.Cos(w0) * gain, gain };
            var den = new[] { 1, -2 * Math.Cos(w0) * gain, 2 * gain - 1 };

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Design IIR peak filter
        /// </summary>
        /// <param name="freq">Normalized frequency (must be in range [0, 0.5])</param>
        /// <param name="q">Quality factor (characterizes peak filter -3dB bandwidth relative to its center frequency)</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirPeak(double freq, double q = 20.0)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var w0 = 2 * freq * Math.PI;
            var bw = w0 / q;
            var gb = 1 / Math.Sqrt(2);

            var beta = gb / Math.Sqrt(1 - gb * gb) * Math.Tan(bw / 2);

            var gain = 1 / (1 + beta);

            var num = new[] { 1 - gain, 0, gain - 1 };
            var den = new[] { 1, -2 * Math.Cos(w0) * gain, 2 * gain - 1 };

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Design IIR comb notch filter
        /// </summary>
        /// <param name="freq">Normalized frequency (must be in range [0, 0.5])</param>
        /// <param name="q">Quality factor (characterizes notch filter -3dB bandwidth relative to its center frequency)</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirCombNotch(double freq, double q = 20.0)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var w0 = 2 * freq * Math.PI;
            var bw = w0 / q;
            var gb = 1 / Math.Sqrt(2);

            var N = (int)(1 / freq);

            var beta = Math.Sqrt((1 - gb * gb) / (gb * gb)) * Math.Tan(N * bw / 4);

            var num = new double [N + 1];
            var den = new double [N + 1];

            num[0] = 1 / (1 + beta);
            num[num.Length - 1] = -1 / (1 + beta);

            den[0] = 1;
            den[den.Length - 1] = -(1 - beta) / (1 + beta);

            return new TransferFunction(num, den);
        }

        /// <summary>
        /// Design IIR comb peak filter
        /// </summary>
        /// <param name="freq">Normalized frequency (must be in range [0, 0.5])</param>
        /// <param name="q">Quality factor (characterizes peak filter -3dB bandwidth relative to its center frequency)</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirCombPeak(double freq, double q = 20.0)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var w0 = 2 * freq * Math.PI;
            var bw = w0 / q;
            var gb = 1 / Math.Sqrt(2);

            var N = (int)(1 / freq);

            var beta = Math.Sqrt(gb * gb / (1 - gb * gb)) * Math.Tan(N * bw / 4);

            var num = new double[N + 1];
            var den = new double[N + 1];

            num[0] = beta / (1 + beta);
            num[num.Length - 1] = -beta / (1 + beta);

            den[0] = 1;
            den[den.Length - 1] = (1 - beta) / (1 + beta);

            return new TransferFunction(num, den);
        }

        #endregion


        #region design transfer functions for IIR pole filters (Butterworth, Chebyshev, etc.)

        /// <summary>
        /// Design TF for low-pass pole filter
        /// </summary>
        /// <param name="freq">Cutoff frequency in range [0, 0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirLpTf(double freq, Complex[] poles, Complex[] zeros = null)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var pre = new double[poles.Length];
            var pim = new double[poles.Length];

            var warpedFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var p = warpedFreq * poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            double[] zre, zim;

            if (zeros != null)
            {
                zre = new double[zeros.Length];
                zim = new double[zeros.Length];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var z = warpedFreq * zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to -1
            else
            {
                zre = Enumerable.Repeat(-1.0, poles.Length).ToArray();
                zim = new double[poles.Length];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(0);

            return tf;
        }

        /// <summary>
        /// Design TF for high-pass pole filter
        /// </summary>
        /// <param name="freq">Cutoff frequency in range [0, 0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirHpTf(double freq, Complex[] poles, Complex[] zeros = null)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var pre = new double[poles.Length];
            var pim = new double[poles.Length];

            var warpedFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var p = warpedFreq / poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            double[] zre, zim;

            if (zeros != null)
            {
                zre = new double[zeros.Length];
                zim = new double[zeros.Length];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var z = warpedFreq / zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to -1
            else
            {
                zre = Enumerable.Repeat(1.0, poles.Length).ToArray();
                zim = new double[poles.Length];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(Math.PI);

            return tf;
        }

        /// <summary>
        /// Design TF for band-pass pole filter
        /// </summary>
        /// <param name="freq1">Left cutoff frequency in range [0, 0.5]</param>
        /// <param name="freq2">Right cutoff frequency in range [0, 0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirBpTf(double freq1, double freq2, Complex[] poles, Complex[] zeros = null)
        {
            Guard.AgainstInvalidRange(freq1, 0, 0.5, "lower frequency");
            Guard.AgainstInvalidRange(freq2, 0, 0.5, "upper frequency");
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            var pre = new double[poles.Length * 2];
            var pim = new double[poles.Length * 2];

            var centerFreq = 2 * Math.PI * (freq1 + freq2) / 2;

            var warpedFreq1 = Math.Tan(Math.PI * freq1);
            var warpedFreq2 = Math.Tan(Math.PI * freq2);

            var f0 = Math.Sqrt(warpedFreq1 * warpedFreq2);
            var bw = warpedFreq2 - warpedFreq1;

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var alpha = bw / 2 * poles[k];
                var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[poles.Length + k] = p2.Real;
                pim[poles.Length + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            double[] zre, zim;

            if (zeros != null)
            {
                zre = new double[zeros.Length * 2];
                zim = new double[zeros.Length * 2];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var alpha = bw / 2 * zeros[k];
                    var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[zeros.Length + k] = z2.Real;
                    zim[zeros.Length + k] = z2.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set them all to [-1, -1, -1, ..., 1, 1, 1]
            else
            {
                zre = Enumerable.Repeat(-1.0, poles.Length)
                                .Concat(Enumerable.Repeat(1.0, poles.Length))
                                .ToArray();
                zim = new double[poles.Length * 2];
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(centerFreq);

            return tf;
        }

        /// <summary>
        /// Design TF for band-reject pole filter
        /// </summary>
        /// <param name="freq1">Left cutoff frequency in range [0, 0.5]</param>
        /// <param name="freq2">Right cutoff frequency in range [0, 0.5]</param>
        /// <param name="poles">Analog prototype poles</param>
        /// <param name="zeros">Analog prototype zeros</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction IirBsTf(double freq1, double freq2, Complex[] poles, Complex[] zeros = null)
        {
            Guard.AgainstInvalidRange(freq1, 0, 0.5, "lower frequency");
            Guard.AgainstInvalidRange(freq2, 0, 0.5, "upper frequency");
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1131.php

            var pre = new double[poles.Length * 2];
            var pim = new double[poles.Length * 2];

            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = Math.Tan(Math.PI * freq2);
            var f0 = Math.Sqrt(f1 * f2);
            var bw = f2 - f1;

            var centerFreq = 2 * Math.Atan(f0);


            // 1) poles and zeros of analog filter (scaled)

            for (var k = 0; k < poles.Length; k++)
            {
                var alpha = bw / 2 / poles[k];
                var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[poles.Length + k] = p2.Real;
                pim[poles.Length + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            double[] zre, zim;

            if (zeros != null)
            {
                zre = new double[zeros.Length * 2];
                zim = new double[zeros.Length * 2];

                for (var k = 0; k < zeros.Length; k++)
                {
                    var alpha = bw / 2 / zeros[k];
                    var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[zeros.Length + k] = z2.Real;
                    zim[zeros.Length + k] = z2.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise create zeros (same amount as poles) and set the following values:
            else
            {
                zre = new double[poles.Length * 2];
                zim = new double[poles.Length * 2];

                for (var k = 0; k < poles.Length; k++)
                {
                    zre[k] = Math.Cos(centerFreq);
                    zim[k] = Math.Sin(centerFreq);
                    zre[poles.Length + k] = Math.Cos(-centerFreq);
                    zim[poles.Length + k] = Math.Sin(-centerFreq);
                }
            }

            // ===


            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(0);

            return tf;
        }

        #endregion


        #region second order sections

        private static readonly Func<Complex, bool> Any = c => true;
        private static readonly Func<Complex, bool> IsReal = c => Math.Abs(c.Imaginary) < 1e-10;
        private static readonly Func<Complex, bool> IsComplex = c => Math.Abs(c.Imaginary) > 1e-10;

        /// <summary>
        /// Convert second-order sections to zpk (TF zeros-poles-gain).
        /// </summary>
        /// <param name="sos">Array of SOS transfer functions</param>
        /// <returns>Transfer function</returns>
        public static TransferFunction SosToTf(TransferFunction[] sos)
        {
            return sos.Aggregate((tf, s) => tf * s);
        }

        /// <summary>
        /// Convert zpk (TF zeros-poles-gain) to second-order sections.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        /// <returns>Array of SOS transfer functions</returns>
        public static TransferFunction[] TfToSos(TransferFunction tf)
        {
            var zeros = tf.Zeros.ToList();
            var poles = tf.Poles.ToList();

            if (zeros.Count != poles.Count)
            {
                if (zeros.Count > poles.Count) poles.AddRange(new Complex[zeros.Count - poles.Count]);
                if (zeros.Count < poles.Count) zeros.AddRange(new Complex[poles.Count - zeros.Count]);
            }

            var sosCount = (poles.Count + 1) / 2;

            if (poles.Count % 2 == 1)
            {
                zeros.Add(Complex.Zero);
                poles.Add(Complex.Zero);
            }

            RemoveConjugated(zeros);
            RemoveConjugated(poles);

            var gains = new double[sosCount];
            gains[0] = tf.Gain;
            for (var i = 1; i < gains.Length; i++) gains[i] = 1;

            var sos = new TransferFunction[sosCount];

            // reverse order of sections

            for (var i = sosCount - 1; i >= 0; i--)
            {
                Complex z1, z2, p1, p2;

                // Select the next pole closest to unit circle

                var pos = ClosestToUnitCircle(poles, Any);
                p1 = poles[pos];
                poles.RemoveAt(pos);

                if (IsReal(p1) && poles.All(IsComplex))
                {
                    pos = ClosestToComplexValue(zeros, p1, IsReal);     // closest to pole p1
                    z1 = zeros[pos];
                    zeros.RemoveAt(pos);

                    p2 = Complex.Zero;
                    z2 = Complex.Zero;
                }
                else
                {
                    if (IsComplex(p1) && zeros.Count(IsReal) == 1)
                    {
                        pos = ClosestToComplexValue(zeros, p1, IsComplex);
                    }
                    else
                    {
                        pos = ClosestToComplexValue(zeros, p1, Any);
                    }

                    z1 = zeros[pos];
                    zeros.RemoveAt(pos);

                    if (IsComplex(p1))
                    {
                        p2 = Complex.Conjugate(p1);

                        if (IsComplex(z1))
                        {
                            z2 = Complex.Conjugate(z1);
                        }
                        else
                        {
                            pos = ClosestToComplexValue(zeros, p1, IsReal);
                            z2 = zeros[pos];
                            zeros.RemoveAt(pos);
                        }
                    }
                    else
                    {
                        if (IsComplex(z1))
                        {
                            z2 = Complex.Conjugate(z1);

                            pos = ClosestToComplexValue(poles, z1, IsReal);
                            p2 = poles[pos];
                            poles.RemoveAt(pos);
                        }
                        else
                        {
                            pos = ClosestToUnitCircle(poles, IsReal);
                            p2 = poles[pos];
                            poles.RemoveAt(pos);

                            pos = ClosestToComplexValue(zeros, p2, IsReal);
                            z2 = zeros[pos];
                            zeros.RemoveAt(pos);
                        }
                    }
                }

                sos[i] = new TransferFunction(new[] { z1, z2 }, new[] { p1, p2 }, gains[i]);
            }

            return sos;
        }

        private static int ClosestToComplexValue(List<Complex> arr, Complex value, Func<Complex, bool> condition)
        {
            var pos = 0;
            var minDistance = double.MaxValue;

            for (var i = 0; i < arr.Count; i++)
            {
                if (!condition(arr[i])) continue;

                var distance = Complex.Abs(arr[i] - value);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    pos = i;
                }
            }

            return pos;
        }

        private static int ClosestToUnitCircle(List<Complex> arr, Func<Complex, bool> condition)
        {
            var pos = 0;
            var minDistance = double.MaxValue;

            for (var i = 0; i < arr.Count; i++)
            {
                if (!condition(arr[i])) continue;

                var distance = Complex.Abs(Complex.Abs(arr[i]) - 1.0);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    pos = i;
                }
            }

            return pos;
        }

        /// <summary>
        /// Leave only one of two conjugated numbers in the list of complex numbers
        /// </summary>
        /// <param name="c">List of complex numbers</param>
        private static void RemoveConjugated(List<Complex> c)
        {
            for (var i = 0; i < c.Count; i++)
            {
                if (IsReal(c[i])) continue;

                var j = i + 1;
                for (; j < c.Count; j++)
                {
                    if (Math.Abs(c[i].Real - c[j].Real) < 1e-10 &&
                        Math.Abs(c[i].Imaginary + c[j].Imaginary) < 1e-10)
                    {
                        break;
                    }
                }

                if (j == c.Count)
                {
                    throw new ArgumentException($"Complex array does not contain conjugated pair for {c[i]}");
                }

                c.RemoveAt(j);
            }
        }

        #endregion
    }
}
