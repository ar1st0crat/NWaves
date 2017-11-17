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
    public static class FilterDesign
    {
        /// <summary>
        /// Method for FIR filter design using window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="magnitudeResponse"></param>
        /// <param name="phaseResponse"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter DesignFirFilter(int order, double[] magnitudeResponse, double[] phaseResponse = null, WindowTypes window = WindowTypes.Hamming)
        {
            if (order % 2 == 0)
            {
                throw new ArgumentException("The order of a filter must be an odd number!");
            }

            var fftSize = MathUtils.NextPowerOfTwo(magnitudeResponse.Length);

            double[] real, imag;

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
                imag = new double[fftSize];
            }

            Transform.Ifft(real, imag, fftSize);

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
        /// Method for ideal lowpass FIR filter design using window method
        /// </summary>
        /// <param name="order"></param>
        /// <param name="freq"></param>
        /// <param name="window"></param>
        /// <returns></returns>
        public static FirFilter DesignFirLowPassFilter(int order, double freq, WindowTypes window = WindowTypes.Hamming)
        {
            const int fftSize = 512;

            var magnitudeResponse = new double[fftSize];
            var phaseResponse = new double[fftSize];

            var cutoffPos = (int)(freq * fftSize);
            for (var i = 0; i < cutoffPos; i++)
            {
                magnitudeResponse[i] = 1.0;
            }

            return DesignFirFilter(order, magnitudeResponse, phaseResponse, window);
        }

        /// <summary>
        /// Method for making HP filter from the linear-phase LP filter
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FirFilter LpToHp(FirFilter filter)
        {
            var kernel = filter.Kernel.Select(k => -k).ToArray();
            kernel[kernel.Length / 2] += 1.0;
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
