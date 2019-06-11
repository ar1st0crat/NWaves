using System;
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
        /// FIR filter design using window method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="magnitudeResponse">Magnitude response</param>
        /// <param name="phaseResponse">Phase response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static FirFilter Fir(int order, double[] magnitudeResponse, double[] phaseResponse = null, WindowTypes window = WindowTypes.Blackman)
        {
            if (order % 2 == 0)
            {
                throw new ArgumentException("The order of a filter must be an odd number!");
            }

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
        /// FIR filter design using window method
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
        /// Method for ideal lowpass FIR filter design using window method
        /// (and sinc-window by default).
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="sinc"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter FirLp(int order, double freq, bool sinc = true, WindowTypes window = WindowTypes.Blackman)
        {
            if (sinc)
            {
                return FirLpSinc(order, freq, window);
            }

            var fftSize = Math.Max(512, MathUtils.NextPowerOfTwo(order * 4));

            var magnitudeResponse = new double[fftSize];
            var phaseResponse = new double[fftSize];

            var cutoffPos = (int)(freq * fftSize);
            for (var i = 0; i < cutoffPos; i++)
            {
                magnitudeResponse[i] = 1.0;
            }

            return Fir(order, magnitudeResponse, phaseResponse, window);
        }

        /// <summary>
        /// Method for ideal lowpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        private static FirFilter FirLpSinc(int order, double freq, WindowTypes window = WindowTypes.Blackman)
        {
            if (order % 2 == 0)
            {
                throw new ArgumentException("The order of a filter must be an odd number!");
            }

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

            var filter1 = LpToHp(FirLpSinc(order, freq1, window));
            var filter2 = FirLpSinc(order, freq2, window);

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

            var filter1 = FirLpSinc(order, freq1, window);
            var filter2 = LpToHp(FirLpSinc(order, freq2, window));
            return filter1 + filter2;
        }


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
            var order = poles.Length;

            var pre = new double[order];
            var pim = new double[order];
            var zre = new double[order];
            var zim = new double[order];

            var warpedFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var p = warpedFreq * poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            if (zeros != null)
            {
                for (var k = 0; k < order; k++)
                {
                    var z = warpedFreq * zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise just set all to -1
            else
            {
                zre = Enumerable.Repeat(-1.0, order).ToArray();
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
            var order = poles.Length;

            var pre = new double[order];
            var pim = new double[order];
            var zre = new double[order];
            var zim = new double[order];

            var warpedFreq = Math.Tan(Math.PI * freq);

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var p = warpedFreq / poles[k];
                pre[k] = p.Real;
                pim[k] = p.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            if (zeros != null)
            {
                for (var k = 0; k < order; k++)
                {
                    var z = warpedFreq / zeros[k];
                    zre[k] = z.Real;
                    zim[k] = z.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise just set all to -1
            else
            {
                zre = Enumerable.Repeat(1.0, order).ToArray();
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
            var order = poles.Length;

            var pre = new double[order * 2];
            var pim = new double[order * 2];
            var zre = new double[order * 2];
            var zim = new double[order * 2];

            var centerFreq = 2 * Math.PI * (freq1 + freq2) / 2;

            var warpedFreq1 = Math.Tan(Math.PI * freq1);
            var warpedFreq2 = Math.Tan(Math.PI * freq2);

            var f0 = Math.Sqrt(warpedFreq1 * warpedFreq2);
            var bw = warpedFreq2 - warpedFreq1;

            // 1) poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var alpha = bw / 2 * poles[k];
                var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[order + k] = p2.Real;
                pim[order + k] = p2.Imaginary;
            }

            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);


            // === if zeros are also specified do the same steps 1-2 with zeros ===

            if (zeros != null)
            {
                for (var k = 0; k < order; k++)
                {
                    var alpha = bw / 2 * zeros[k];
                    var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[order + k] = z2.Real;
                    zim[order + k] = z2.Imaginary;
                }

                MathUtils.BilinearTransform(zre, zim);
            }
            // otherwise just set all to -1
            else
            {
                zre = Enumerable.Repeat(-1.0, order).Concat(Enumerable.Repeat(1.0, order)).ToArray();
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
            // Calculation of filter coefficients is based on Neil Robertson's post:
            // https://www.dsprelated.com/showarticle/1131.php
            
            var order = poles.Length;

            var pre = new double[order * 2];
            var pim = new double[order * 2];
            var zre = new double[order * 2];
            var zim = new double[order * 2];

            var centerFreq = 2 * Math.PI * (freq1 + freq2) / 2;

            var f0 = Math.Tan(Math.PI * (freq1 + (freq2 - freq1) / 2));
            var f1 = Math.Tan(Math.PI * freq1);
            var f2 = f0 * f0 / f1;
            var bw = f2 - f1;


            // 1) zeros and poles of analog filter (scaled)

            for (var k = 0; k < order; k++)
            {
                var alpha = bw / 2 / poles[k];
                var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                var p1 = alpha * (1 + beta);
                pre[k] = p1.Real;
                pim[k] = p1.Imaginary;

                var p2 = alpha * (1 - beta);
                pre[order + k] = p2.Real;
                pim[order + k] = p2.Imaginary;
            }


            if (zeros != null)
            {
                for (var k = 0; k < order; k++)
                {
                    var alpha = bw / 2 / zeros[k];
                    var beta = Complex.Sqrt(1 - Complex.Pow(f0 / alpha, 2));

                    var z1 = alpha * (1 + beta);
                    zre[k] = z1.Real;
                    zim[k] = z1.Imaginary;

                    var z2 = alpha * (1 - beta);
                    zre[order + k] = z2.Real;
                    zim[order + k] = z2.Imaginary;
                }
            }
            else
            {
                for (var k = 0; k < order; k++)
                {
                    zre[k] = Math.Cos(centerFreq);
                    zim[k] = Math.Sin(centerFreq);
                    zre[order + k] = Math.Cos(-centerFreq);
                    zim[order + k] = Math.Sin(-centerFreq);
                }
            }


            // 2) switch to z-domain

            MathUtils.BilinearTransform(pre, pim);
            MathUtils.BilinearTransform(zre, zim);

            // 3) return TF with normalized coefficients

            var tf = new TransferFunction(new ComplexDiscreteSignal(1, zre, zim),
                                          new ComplexDiscreteSignal(1, pre, pim));
            tf.NormalizeAt(0);

            return tf;
        }

        #endregion
    }
}
