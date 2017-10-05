using System;
using System.Collections.Generic;
using System.Linq;

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
        /// <returns>Spectrogram of the signal</returns>
        public static List<double[]> Stft(double[] samples, int fftSize = 512, int hopSize = 256)
        {
            var spectrogram = new List<double[]>();

            var start = 0;
            for (; start + fftSize < samples.Length; start += hopSize)
            {
                var segment = samples.Skip(start).Take(fftSize).ToArray();
                
                spectrogram.Add(MagnitudeSpectrum(segment, fftSize));
            }

            // if we need to process the last (not full) portion of data, 
            // then we should pad it with zeros:

            var lastSegment = samples.Skip(start).Take(fftSize).ToArray();
            Array.Resize(ref lastSegment, fftSize);

            spectrogram.Add(MagnitudeSpectrum(lastSegment, fftSize));

            return spectrogram;
        }
    }
}
