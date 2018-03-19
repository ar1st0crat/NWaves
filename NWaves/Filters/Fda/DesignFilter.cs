using System;
using System.Linq;
using NWaves.Filters.Base;
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
        /// Method for FIR filter design using window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="magnitudeResponse"></param>
        /// <param name="phaseResponse"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter Fir(int order, float[] magnitudeResponse, float[] phaseResponse = null, WindowTypes window = WindowTypes.Blackman)
        {
            if (order % 2 == 0)
            {
                throw new ArgumentException("The order of a filter must be an odd number!");
            }

            var fftSize = MathUtils.NextPowerOfTwo(magnitudeResponse.Length);
            var fft = new Fft(fftSize);

            float[] real, imag;

            real = fftSize != magnitudeResponse.Length ? 
                   FastCopy.PadZeros(magnitudeResponse, fftSize) : 
                   FastCopy.EntireArray(magnitudeResponse);

            if (phaseResponse != null)
            {
                imag = fftSize != phaseResponse.Length ?
                       FastCopy.PadZeros(phaseResponse, fftSize) :
                       FastCopy.EntireArray(phaseResponse);
            }
            else
            {
                imag = new float[fftSize];
            }

            fft.Inverse(real, imag);

            var kernel = new float[order];

            var compensation = 2.0f / fftSize;
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

            var magnitudeResponse = new float[fftSize];
            var phaseResponse = new float[fftSize];

            var cutoffPos = (int)(freq * fftSize);
            for (var i = 0; i < cutoffPos; i++)
            {
                magnitudeResponse[i] = 1.0f;
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

            var kernel = new float[order];

            var middle = order / 2;
            var freq2Pi = 2 * Math.PI * freq;

            kernel[middle] = (float)(2 * freq);
            for (var i = 1; i <= middle; i++)
            {
                kernel[middle - i] = 
                kernel[middle + i] = (float)(Math.Sin(freq2Pi * i) / (Math.PI * i));
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
            var kernel = filter.Kernel.Select(k => -k).ToArray();
            kernel[kernel.Length / 2] += 1.0f;
            return new FirFilter(kernel);
        }

        /// <summary>
        /// Method for making LP filter from the linear-phase HP filter
        /// (no different from LpToHp method)
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FirFilter HpToLp(FirFilter filter)
        {
            return LpToHp(filter);
        }
    }
}
