using System;
using System.Linq;
using System.Numerics;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Static class providing basic methods for filter design & analysis
    /// </summary>
    public static partial class DesignFilter
    {
        #region FirWin functions

        ///
        /// FirWin(Lp|Hp|Bp|Bs) functions:
        /// 
        /// as of ver.0.9.5,
        /// they're coded as the special case of fractional-delay FIR filter design
        /// with either delay=0 (odd order) or delay=0.5 (even order)
        /// 

        /// <summary>
        /// Method for ideal lowpass FIR filter design using sinc-window method.
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="freq">Cutoff frequency (normalized: fc = f/fs)</param>
        /// <param name="window">Window</param>
        /// <returns>LP filter kernel</returns>
        public static double[] FirWinLp(int order, double freq, WindowTypes window = WindowTypes.Blackman)
        {
            return FirWinFdLp(order, freq, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal highpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinHp(int order, double freq, WindowTypes window = WindowTypes.Blackman)
        {
            return FirWinFdHp(order, freq, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal bandpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinBp(int order, double freq1, double freq2, WindowTypes window = WindowTypes.Blackman)
        {
            return FirWinFdBp(order, freq1, freq2, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal bandstop FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinBs(int order, double freq1, double freq2, WindowTypes window = WindowTypes.Blackman)
        {
            return FirWinFdBs(order, freq1, freq2, (order + 1) % 2 * 0.5, window);
        }

        #endregion


        #region fractional delay FIR filter design

        /// <summary>
        /// Method for ideal lowpass fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="delay"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinFdLp(int order, double freq, double delay, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var kernel = new double[order];

            var middle = (order - 1) / 2;
            var freq2Pi = 2 * Math.PI * freq;

            for (var i = 0; i < order; i++)
            {
                var d = i - delay - middle;

                kernel[i] = d == 0 ? 2 * freq : Math.Sin(freq2Pi * d) / (Math.PI * d);
            }

            kernel.ApplyWindow(window);

            NormalizeKernel(kernel);

            return kernel;
        }

        /// <summary>
        /// Method for ideal highpass fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="delay"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinFdHp(int order, double freq, double delay, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq, 0, 0.5, "Filter frequency");

            var kernel = new double[order];

            var middle = (order - 1) / 2;
            var freq2Pi = 2 * Math.PI * (0.5 - freq);

            var sign = -1;

            for (var i = 0; i < order; i++)
            {
                var d = i - delay - middle;

                kernel[i] = d == 0 ? 2 * (0.5 - freq) : sign * Math.Sin(freq2Pi * d) / (Math.PI * d);

                sign = -sign;
            }

            kernel.ApplyWindow(window);

            NormalizeKernel(kernel, Math.PI);

            return kernel;
        }

        /// <summary>
        /// Method for ideal bandpass fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="delay"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinFdBp(int order, double freq1, double freq2, double delay, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq1, 0, 0.5, "lower frequency");
            Guard.AgainstInvalidRange(freq2, 0, 0.5, "upper frequency");
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            var kernel = new double[order];

            var middle = (order - 1) / 2;
            var freq12Pi = 2 * Math.PI * freq1;
            var freq22Pi = 2 * Math.PI * freq2;

            for (var i = 0; i < order; i++)
            {
                var d = i - delay - middle;

                kernel[i] = d == 0 ? 2 * (freq2 - freq1) : (Math.Sin(freq22Pi * d) - Math.Sin(freq12Pi * d)) / (Math.PI * d);
            }

            kernel.ApplyWindow(window);

            NormalizeKernel(kernel, 2 * Math.PI * (freq1 + freq2) / 2);

            return kernel;
        }

        /// <summary>
        /// Method for ideal bandstop fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq1"></param>
        /// <param name="freq2"></param>
        /// <param name="delay"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinFdBs(int order, double freq1, double freq2, double delay, WindowTypes window = WindowTypes.Blackman)
        {
            Guard.AgainstInvalidRange(freq1, 0, 0.5, "lower frequency");
            Guard.AgainstInvalidRange(freq2, 0, 0.5, "upper frequency");
            Guard.AgainstInvalidRange(freq1, freq2, "lower frequency", "upper frequency");

            var kernel = new double[order];

            var middle = (order - 1) / 2;
            var freq12Pi = 2 * Math.PI * freq1;
            var freq22Pi = 2 * Math.PI * (0.5 - freq2);

            var sign = 1;

            for (var i = 0; i < order; i++)
            {
                var d = i - delay - middle;

                kernel[i] = d == 0 ? 2 * (0.5 - freq2 + freq1) : (Math.Sin(freq12Pi * d) + sign * Math.Sin(freq22Pi * d)) / (Math.PI * d);

                sign = -sign;
            }

            kernel.ApplyWindow(window);

            NormalizeKernel(kernel);

            return kernel;
        }

        /// <summary>
        /// Method for all-pass fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="delay"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static double[] FirWinFdAp(int order, double delay, WindowTypes window = WindowTypes.Blackman)
        {
            var kernel = new double[order];

            var middle = (order - 1) / 2;

            for (var i = 0; i < order; i++)
            {
                kernel[i] = MathUtils.Sinc(i - delay - middle);
            }

            kernel.ApplyWindow(window);

            NormalizeKernel(kernel);

            return kernel;
        }

        /// <summary>
        /// Normalize frequency response at given frequency
        /// (normalize kernel coefficients to map frequency response onto [0, 1])
        /// </summary>
        /// <param name="kernel">Kernel</param>
        public static void NormalizeKernel(double[] kernel, double freq = 0)
        {
            var w = Complex.FromPolarCoordinates(1, freq);

            var gain = Complex.Abs(1 / MathUtils.EvaluatePolynomial(kernel, w));

            for (var i = 0; i < kernel.Length; i++)
            {
                kernel[i] *= gain;
            }
        }

        #endregion


        #region equiripple FIR filter

        /// <summary>
        /// Design equiripple LP FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="fp">Passband edge frequency</param>
        /// <param name="fa">Stopband edge frequency</param>
        /// <param name="wp">Passband weight</param>
        /// <param name="wa">Stopband weight</param>
        /// <returns>Filter kernel</returns>
        public static double[] FirEquirippleLp(int order, double fp, double fa, double wp, double wa)
        {
            return new Remez(order, new[] { 0, fp, fa, 0.5 }, new[] { 1, 0.0 }, new[] { wp, wa }).Design();
        }

        /// <summary>
        /// Design equiripple HP FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="fa">Stopband edge frequency</param>
        /// <param name="fp">Passband edge frequency</param>
        /// <param name="wa">Stopband weight</param>
        /// <param name="wp">Passband weight</param>
        /// <returns>Filter kernel</returns>
        public static double[] FirEquirippleHp(int order, double fa, double fp, double wa, double wp)
        {
            return new Remez(order, new[] { 0, fa, fp, 0.5 }, new[] { 0, 1.0 }, new[] { wa, wp }).Design();
        }

        /// <summary>
        /// Design equiripple BP FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="fa1">Left stopband edge frequency</param>
        /// <param name="fp1">Passband left edge frequency</param>
        /// <param name="fp2">Passband right edge frequency</param>
        /// <param name="fa2">Right stopband edge frequency</param>
        /// <param name="wa1">Left stopband weight</param>
        /// <param name="wp">Passband weight</param>
        /// <param name="wa2">Right stopband weight</param>
        /// <returns>Filter kernel</returns>
        public static double[] FirEquirippleBp(int order, double fa1, double fp1, double fp2, double fa2, double wa1, double wp, double wa2)
        {
            return new Remez(order, new[] { 0, fa1, fp1, fp2, fa2, 0.5 }, new[] { 0, 1.0, 0 }, new[] { wa1, wp, wa2 }).Design();
        }

        /// <summary>
        /// Design equiripple BS FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="fp1">Left passband edge frequency</param>
        /// <param name="fa1">Stopband left edge frequency</param>
        /// <param name="fa2">Stopband right edge frequency</param>
        /// <param name="fp2">Right passband edge frequency</param>
        /// <param name="wp1">Left passband weight</param>
        /// <param name="wa">Stopband weight</param>
        /// <param name="wp2">Right passband weight</param>
        /// <returns>Filter kernel</returns>
        public static double[] FirEquirippleBs(int order, double fp1, double fa1, double fa2, double fp2, double wp1, double wa, double wp2)
        {
            return new Remez(order, new[] { 0, fp1, fa1, fa2, fp2, 0.5 }, new[] { 1, 0.0, 1 }, new[] { wp1, wa, wp2 }).Design();
        }

        #endregion


        #region Fir functions (frequency sampling method)

        /// <summary>
        /// FIR filter design using frequency sampling method (like firwin2 in sciPy).
        /// 
        /// This method doesn't do any interpolation of the magnitude response,
        /// so you need to take care of it before calling the method.
        /// Usage example:
        /// 
        /// var zeros = Enumerable.Repeat(0.0, 80);
        /// var ones1 = Enumerable.Repeat(1.0, 80);
        /// var ones2 = Enumerable.Repeat(1.0, 40);
        /// var magnitudes = ones1.Concat(zeros).Concat(ones2).ToArray();
        ///  // 80 ones + 80 zeros + 40 ones = 200 magnitude values
        ///  // 56 zero magnitude values will be added for closest power of two (256)
        /// 
        /// var kernel = DesignFilter.Fir(101, magnitudes);
        ///
        /// var filter = new FirFilter(kernel);
        /// 
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="magnitudeResponse">Magnitude response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static double[] Fir(int order,
                                   double[] magnitudeResponse,
                                   WindowTypes window = WindowTypes.Blackman)
        {
            // 2x because we reserve space for symmetric part

            var fftSize = 2 * MathUtils.NextPowerOfTwo(magnitudeResponse.Length);

            var complexResponse = new Complex[fftSize];

            for (var i = 0; i < magnitudeResponse.Length; i++)
            {
                complexResponse[i] = magnitudeResponse[i] * Complex.Exp(new Complex(0, -(order - 1) / 2.0 * 2 * Math.PI * i / fftSize));
            }

            var real = complexResponse.Select(c => c.Real).ToArray();
            var imag = complexResponse.Select(c => c.Imaginary).ToArray();
            var kernel = new double[fftSize];

            var fft = new RealFft64(fftSize);
            fft.Inverse(real, imag, real);

            kernel = real.Take(order).Select(s => s / fftSize).ToArray();

            kernel.ApplyWindow(window);

            return kernel;
        }

        /// <summary>
        /// FIR filter design using frequency sampling method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="frequencyResponse">Complex frequency response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static double[] Fir(int order, ComplexDiscreteSignal frequencyResponse, WindowTypes window = WindowTypes.Blackman)
        {
            return Fir(order, frequencyResponse.Real, window);
        }

        /// <summary>
        /// FIR filter design using frequency sampling method (32-bit precision)
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="magnitudeResponse">Magnitude response</param>
        /// <param name="window">Window</param>
        /// <returns>FIR filter kernel</returns>
        public static double[] Fir(int order,
                                   float[] magnitudeResponse,
                                   WindowTypes window = WindowTypes.Blackman)
        {
            return Fir(order, magnitudeResponse.ToDoubles(), window);
        }

        #endregion


        #region convert LowPass FIR filter kernel between band forms

        /// <summary>
        /// Method for making HP filter from the linear-phase LP filter.
        /// This method works only for odd-sized kernels.
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static double[] FirLpToHp(double[] kernel)
        {
            Guard.AgainstEvenNumber(kernel.Length, "The order of the filter");

            var kernelHp = kernel.Select(k => -k).ToArray();
            kernelHp[kernelHp.Length / 2] += 1.0;
            return kernelHp;
        }

        /// <summary>
        /// Method for making LP filter from the linear-phase HP filter
        /// (not different from FirLpToHp method)
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static double[] FirHpToLp(double[] kernel) => FirLpToHp(kernel);

        /// <summary>
        /// Method for making BS filter from the linear-phase BP filter
        /// (not different from FirLpToHp method)
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static double[] FirBpToBs(double[] kernel) => FirLpToHp(kernel);

        /// <summary>
        /// Method for making BP filter from the linear-phase BS filter
        /// (not different from FirLpToHp method)
        /// </summary>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static double[] FirBsToBp(double[] kernel) => FirLpToHp(kernel);

        #endregion
    }
}
