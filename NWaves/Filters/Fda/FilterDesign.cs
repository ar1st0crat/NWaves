using System;
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
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static FirFilter LpToHp(FirFilter filter)
        {
            return null;
        }
    }
}
