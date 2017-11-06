using System.Collections.Generic;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// STFT (spectrogram) is essentially the list of spectra in time
        /// </summary>
        /// <param name="samples">The samples of signal</param>
        /// <param name="fftSize">Size of FFT</param>
        /// <param name="hopSize">Hop (overlap) size</param>
        /// <param name="window">Type of the window function to apply</param>
        /// <returns>Spectrogram of the signal</returns>
        public static List<double[]> Stft(double[] samples, int fftSize = 512, int hopSize = 256, WindowTypes window = WindowTypes.Rectangular)
        {
            var spectrogram = new List<double[]>();

            var start = 0;
            for (; start + fftSize < samples.Length; start += hopSize)
            {
                var segment = FastCopy.ArrayFragment(samples, fftSize, start);

                if (window != WindowTypes.Rectangular)
                {
                    segment.ApplyWindow(window);
                }
                
                spectrogram.Add(MagnitudeSpectrum(segment, fftSize));
            }

            // if we need to process the last (not full) portion of data, we should pad it with zeros:
            var lastSegment = new double[fftSize];
            FastCopy.ArrayFragment(samples, samples.Length - start, start);

            spectrogram.Add(MagnitudeSpectrum(lastSegment, fftSize));

            return spectrogram;
        }
    }
}
