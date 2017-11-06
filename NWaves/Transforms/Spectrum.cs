using System;
using NWaves.Utils;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// Magnitude spectrum:
        /// 
        ///     spectrum = sqrt(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <param name="normalize">Normalization flag</param>
        /// <returns>Left half of the magnitude spectrum</returns>
        public static double[] MagnitudeSpectrum(double[] samples, int fftSize = 512, bool normalize = false)
        {
            double[] real, imag;

            ComplexSpectrum(samples, out real, out imag, fftSize);

            var spectrum = new double[real.Length];
            if (normalize)
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]) / fftSize;
                }
            }
            else
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]);
                }
            }
            return spectrum;
        }

        /// <summary>
        /// Power spectrum (normalized):
        /// 
        ///     spectrum =   (re * re + im * im) / fftSize
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <param name="normalize">Normalization flag</param>
        /// <returns>Left half of the magnitude spectrum</returns>
        public static double[] PowerSpectrum(double[] samples, int fftSize = 512, bool normalize = true)
        {
            double[] real, imag;

            ComplexSpectrum(samples, out real, out imag, fftSize);

            var spectrum = new double[real.Length];
            if (normalize)
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = (real[i] * real[i] + imag[i] * imag[i]) / fftSize;
                }
            }
            else
            {
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = real[i] * real[i] + imag[i] * imag[i];
                }
            }
            return spectrum;
        }

        /// <summary>
        /// Log power spectrum:
        /// 
        ///     spectrum = 20 * log10(re * re + im * im)
        /// 
        /// </summary>
        /// <param name="samples">Array of samples (samples parts)</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <returns>Left half of the log-power spectrum</returns>
        public static double[] LogPowerSpectrum(double[] samples, int fftSize = 512)
        {
            double[] real, imag;

            ComplexSpectrum(samples, out real, out imag, fftSize);

            var spectrum = new double[real.Length];
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = 20 * Math.Log10(real[i] * real[i] + imag[i] * imag[i] + double.Epsilon);
            }
            return spectrum;
        }

        /// <summary>
        /// Method for computing complex spectrum without any post-processing
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="realSpectrum"></param>
        /// <param name="imagSpectrum"></param>
        /// <param name="fftSize"></param>
        public static void ComplexSpectrum(double[] samples, out double[] realSpectrum, out double[] imagSpectrum, int fftSize = 512)
        {
            if (samples.Length < fftSize)
            {
                samples = FastCopy.PadZeros(samples, fftSize);
            }
            var imag = new double[samples.Length];

            Fft(samples, imag, fftSize);

            realSpectrum = FastCopy.ArrayFragment(samples, fftSize / 2);
            imagSpectrum = FastCopy.ArrayFragment(imag, fftSize / 2);
        }
    }
}
