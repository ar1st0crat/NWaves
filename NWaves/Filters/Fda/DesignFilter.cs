using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Static class providing basic methods for filter design & analysis
    /// </summary>
    public static class DesignFilter
    {
        /// <summary>
        /// FIR filter design using frequency sampling method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="magnitudeResponse">Magnitude response</param>
        /// <param name="phaseResponse">Phase response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static FirFilter Fir(int order, double[] magnitudeResponse, double[] phaseResponse = null, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstEvenNumber(order, "The order of the filter");

            var fftSize = MathUtils.NextPowerOfTwo(magnitudeResponse.Length);
            
            var real = phaseResponse == null ? 
                       magnitudeResponse.PadZeros(fftSize) :
                       magnitudeResponse.Zip(phaseResponse, (m, p) => m * Math.Cos(p)).ToArray();

            var imag = phaseResponse == null ? 
                       new double[fftSize] :
                       magnitudeResponse.Zip(phaseResponse, (m, p) => m * Math.Sin(p)).ToArray();

            var fft = new Fft64(fftSize);
            fft.Inverse(real, imag);

            var kernel = new double[order];

            var compensation = 2.0 / fftSize;
            var middle = order / 2;
            for (var i = 0; i <= middle; i++)
            {
                kernel[i] = real[middle - i] * compensation;
                kernel[i + middle] = real[i] * compensation;
            }
            
            kernel.ApplyWindow(window);

            return new FirFilter(kernel);
        }

        /// <summary>
        /// FIR filter design using frequency sampling method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyResponse">Complex frequency response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static FirFilter Fir(int order, ComplexDiscreteSignal frequencyResponse, WindowTypes window = WindowTypes.Blackman)
        {
            return Fir(order, frequencyResponse.Real, frequencyResponse.Imag, window);
        }

        /// <summary>
        /// Method for ideal lowpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter FirWin(int order, double freq, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstEvenNumber(order, "The order of the filter");

            var kernel = new double[order];

            var middle = order / 2;
            var freq2Pi = 2 * Math.PI * freq;

            kernel[middle] = 2 * freq;
            for (var i = 1; i <= middle; i++)
            {
                kernel[middle - i] = 
                kernel[middle + i] = Math.Sin(freq2Pi * i) / (Math.PI * i);
            }

            kernel.ApplyWindow(window);

            return new FirFilter(kernel);
        }

        /// <summary>
        /// Design equiripple FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="fp"></param>
        /// <param name="fa"></param>
        /// <param name="ripplePass"></param>
        /// <param name="rippleStop"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public static FirFilter FirEquiripple(double fp, double fa, double ripplePass = 1, double rippleStop = 20, int order = 0)
        {
            return new Remez(new[] { 0, fp, fa, 0.5 }, new[] { ripplePass, rippleStop }, order).Design();
        }
        

        #region Convert LowPass to other band forms

        /// <summary>
        /// Method for making HP filter from the linear-phase LP filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FirFilter LpToHp(FirFilter filter)
        {
            var kernel = filter.Tf.Numerator.Select(k => -k).ToArray();
            kernel[kernel.Length / 2] += 1.0;
            return new FirFilter(kernel);
        }

        /// <summary>
        /// Method for making LP filter from the linear-phase HP filter
        /// (not different from LpToHp method)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FirFilter HpToLp(FirFilter filter)
        {
            return LpToHp(filter);
        }

        /// <summary>
        /// Simple BandPass FIR filter design
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter FirBp(int order, double freq1, double freq2, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            var filter1 = LpToHp(FirWin(order, freq1, window));
            var filter2 = FirWin(order, freq2, window);

            var filter = filter1 * filter2;
            var kernel = filter.Tf.Numerator;

            return new FirFilter(kernel.Skip(order/2).Take(order));
        }

        /// <summary>
        /// Simple BandReject FIR filter design
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter FirBr(int order, double freq1, double freq2, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            var filter1 = FirWin(order, freq1, window);
            var filter2 = LpToHp(FirWin(order, freq2, window));
            return filter1 + filter2;
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
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1131.php

            var pre = new double[poles.Length * 2];
            var pim = new double[poles.Length * 2];
            
            var centerFreq = 2 * Math.PI * (freq1 + freq2) / 2;

            var f0 = Math.Tan(Math.PI * (freq1 + (freq2 - freq1) / 2));
            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = f0 * f0 / f1;
            var bw = f2 - f1;


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

        /// <summary>
        /// Second-order sections to zpk.
        /// </summary>
        /// <param name="sos"></param>
        /// <returns></returns>
        public static TransferFunction SosToTf(TransferFunction[] sos)
        {
            return sos.Aggregate((tf, s) => tf * s);
        }

        /// <summary>
        /// Zpk to second-order sections.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        /// <returns>Array of SOS transfer functions</returns>
        public static TransferFunction[] TfToSos(TransferFunction tf)
        {
            var zeros = tf.Zeros.ToComplexNumbers().ToList();
            var poles = tf.Poles.ToComplexNumbers().ToList();

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

                var zs = new ComplexDiscreteSignal(1, new[] { z1.Real, z2.Real }, new[] { z1.Imaginary, z2.Imaginary });
                var ps = new ComplexDiscreteSignal(1, new[] { p1.Real, p2.Real }, new[] { p1.Imaginary, p2.Imaginary });

                sos[i] = new TransferFunction(zs, ps, gains[i]);
            }

            return sos;
        }

        private static readonly Func<Complex, bool> Any = c => true;
        private static readonly Func<Complex, bool> IsReal = c => Math.Abs(c.Imaginary) < 1e-10;
        private static readonly Func<Complex, bool> IsComplex = c => Math.Abs(c.Imaginary) > 1e-10;

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
        /// <param name="arr"></param>
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
