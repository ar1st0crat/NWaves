using System;
using System.Linq;
using System.Numerics;
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
        #region fractional delay FIR filter design

        /// <summary>
        /// Method for ideal lowpass fractional-delay FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="freq">Cutoff frequency (normalized: fc = f/fs)</param>
        /// <param name="delay">Fractional delay</param>
        /// <param name="window">Window</param>
        /// <returns>LP filter kernel</returns>
        public static double[] FirWinFdLp(int order, double freq, double delay, WindowType window = WindowType.Blackman)
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
        /// <param name="order">Filter order</param>
        /// <param name="freq">Cutoff frequency (normalized: fc = f/fs)</param>
        /// <param name="delay">Fractional delay</param>
        /// <param name="window">Window</param>
        /// <returns>HP filter kernel</returns>
        public static double[] FirWinFdHp(int order, double freq, double delay, WindowType window = WindowType.Blackman)
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
        /// <param name="order">Filter order</param>
        /// <param name="freq1">Left edge cutoff frequency (normalized: fc1 = f1/fs)</param>
        /// <param name="freq2">Right edge cutoff frequency (normalized: fc2 = f2/fs)</param>
        /// <param name="delay">Fractional delay</param>
        /// <param name="window">Window</param>
        /// <returns>BP filter kernel</returns>
        public static double[] FirWinFdBp(int order, double freq1, double freq2, double delay, WindowType window = WindowType.Blackman)
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
        /// <param name="order">Filter order</param>
        /// <param name="freq1">Left edge cutoff frequency (normalized: fc1 = f1/fs)</param>
        /// <param name="freq2">Right edge cutoff frequency (normalized: fc2 = f2/fs)</param>
        /// <param name="delay">Fractional delay</param>
        /// <param name="window">Window</param>
        /// <returns>BS filter kernel</returns>
        public static double[] FirWinFdBs(int order, double freq1, double freq2, double delay, WindowType window = WindowType.Blackman)
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
        /// <param name="order">Filter order</param>
        /// <param name="delay">Fractional delay</param>
        /// <param name="window">Window</param>
        /// <returns>All-pass filter kernel</returns>
        public static double[] FirWinFdAp(int order, double delay, WindowType window = WindowType.Blackman)
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
        /// <param name="freq">Frequency</param>
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
        /// <param name="order">Filter order</param>
        /// <param name="freq">Cutoff frequency (normalized: fc = f/fs)</param>
        /// <param name="window">Window</param>
        /// <returns>LP filter kernel</returns>
        public static double[] FirWinLp(int order, double freq, WindowType window = WindowType.Blackman)
        {
            return FirWinFdLp(order, freq, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal highpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="freq">Cutoff frequency (normalized: fc = f/fs)</param>
        /// <param name="window">Window</param>
        /// <returns>HP filter kernel</returns>
        public static double[] FirWinHp(int order, double freq, WindowType window = WindowType.Blackman)
        {
            return FirWinFdHp(order, freq, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal bandpass FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="freq1">Left edge cutoff frequency (normalized: fc1 = f1/fs)</param>
        /// <param name="freq2">Right edge cutoff frequency (normalized: fc2 = f2/fs)</param>
        /// <param name="window">Window</param>
        /// <returns>BP filter kernel</returns>
        public static double[] FirWinBp(int order, double freq1, double freq2, WindowType window = WindowType.Blackman)
        {
            return FirWinFdBp(order, freq1, freq2, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// Method for ideal bandstop FIR filter design using sinc-window method
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="freq1">Left edge cutoff frequency (normalized: fc1 = f1/fs)</param>
        /// <param name="freq2">Right edge cutoff frequency (normalized: fc2 = f2/fs)</param>
        /// <param name="window">Window</param>
        /// <returns>BS filter kernel</returns>
        public static double[] FirWinBs(int order, double freq1, double freq2, WindowType window = WindowType.Blackman)
        {
            return FirWinFdBs(order, freq1, freq2, (order + 1) % 2 * 0.5, window);
        }

        /// <summary>
        /// FIR filter design using frequency sampling method
        /// (works identical to firwin2 in sciPy and fir2 in MATLAB).
        /// 
        /// Note. By default, the FFT size is auto-computed.
        ///       If it is set explicitly, then (fftSize/2 + 1) must exceed the filter order.
        /// 
        /// Note. Array of frequencies can be null. 
        ///       In this case the FFT size must be provided and size of gains array must be fftSize/2 + 1.
        ///       Frequencies will be uniformly sampled on range [0 .. 0.5].
        /// 
        /// </summary>
        /// <param name="order">Filter order</param>
        /// <param name="frequencies">Frequencies (frequency sampling points) in range [0, 0.5]</param>
        /// <param name="gain">Filter gains at the frequency sampling points</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="window">Window</param>
        /// <returns>Filter kernel</returns>
        public static double[] Fir(int order,
                                   double[] frequencies,
                                   double[] gain,
                                   int fftSize = 0,
                                   WindowType window = WindowType.Hamming)
        {
            if (fftSize == 0)
            {
                fftSize = 2 * MathUtils.NextPowerOfTwo(order);
            }

            var freqCount = fftSize / 2 + 1;

            if (frequencies is null)
            {
                frequencies = Enumerable.Range(0, freqCount)
                                        .Select(i => (double)i / fftSize)
                                        .ToArray();
            }

            if (order >= freqCount)
            {
                throw new ArgumentException($"Given that filter order is {order} the FFT size must be at least {2 * MathUtils.NextPowerOfTwo(order)}");
            }

            Guard.AgainstInequality(frequencies.Length, gain.Length, "Length of frequencies array", "length of gain array");
            Guard.AgainstNotOrdered(frequencies, "Array of frequencies");


            // linear interpolation

            var step = 1.0 / fftSize;
            var grid = Enumerable.Range(0, freqCount)
                                 .Select(f => f * step)
                                 .ToArray();

            var response = new double[grid.Length];
            var x = frequencies;
            var y = gain;

            var left = 0;
            var right = 1;

            for (var i = 0; i < grid.Length; i++)
            {
                while (grid[i] > x[right] && right < x.Length - 1)
                {
                    right++;
                    left++;
                }

                response[i] = y[left] + (y[right] - y[left]) * (grid[i] - x[left]) / (x[right] - x[left]);
            }

            // prepare complex frequency response

            var complexResponse = new Complex[fftSize];

            for (var i = 0; i < response.Length; i++)
            {
                complexResponse[i] = response[i] * Complex.Exp(new Complex(0, -(order - 1) / 2.0 * 2 * Math.PI * i / fftSize));
            }

            var real = complexResponse.Select(c => c.Real).ToArray();
            var imag = complexResponse.Select(c => c.Imaginary).ToArray();

            // IFFT

            var fft = new RealFft64(fftSize);
            fft.Inverse(real, imag, real);

            var kernel = real.Take(order).Select(s => s / fftSize).ToArray();

            kernel.ApplyWindow(window);

            return kernel;
        }

        #endregion


        #region equiripple FIR filter

        /// <summary>
        /// Design equiripple LP FIR filter using Remez (Parks-McClellan) algorithm
        /// </summary>
        /// <param name="order">Filter order</param>
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
        /// <param name="order">Filter order</param>
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
        /// <param name="order">Filter order</param>
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
        /// <param name="order">Filter order</param>
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


        #region convert LowPass FIR filter kernel between band forms

        /// <summary>
        /// Method for making HP filter from the linear-phase LP filter.
        /// This method works only for odd-sized kernels.
        /// </summary>
        /// <param name="kernel">Lowpass filter kernel</param>
        /// <returns>Highpass filter kernel</returns>
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
        /// <param name="kernel">Highpass filter kernel</param>
        /// <returns>Lowpass filter kernel</returns>
        public static double[] FirHpToLp(double[] kernel) => FirLpToHp(kernel);

        /// <summary>
        /// Method for making BS filter from the linear-phase BP filter
        /// (not different from FirLpToHp method)
        /// </summary>
        /// <param name="kernel">Bandpass filter kernel</param>
        /// <returns>Bandstop filter kernel</returns>
        public static double[] FirBpToBs(double[] kernel) => FirLpToHp(kernel);

        /// <summary>
        /// Method for making BP filter from the linear-phase BS filter
        /// (not different from FirLpToHp method)
        /// </summary>
        /// <param name="kernel">Bandstop filter kernel</param>
        /// <returns>Bandpass filter kernel</returns>
        public static double[] FirBsToBp(double[] kernel) => FirLpToHp(kernel);

        #endregion
    }
}
