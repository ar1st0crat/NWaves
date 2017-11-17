using System;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Transforms
{
    public static partial class Transform
    {
        /// <summary>
        /// Method for computing real cepstrum from array of samples
        /// </summary>
        /// <param name="real"></param>
        /// <param name="cepstrumSize"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static double[] Cepstrum(double[] real, int cepstrumSize, int fftSize = 512)
        {
            if (real.Length < fftSize)
            {
                real = FastCopy.PadZeros(real, fftSize);
            }
            var imag = new double[real.Length];

            // complex fft
            Fft(real, imag, fftSize);

            // complex logarithm
            for (var i = 0; i < fftSize; i++)
            {
                real[i] = Math.Log(Math.Sqrt(real[i] * real[i] + imag[i] * imag[i]));
            }

            // complex ifft
            Ifft(real, imag, fftSize);

            // take real truncated part
            return FastCopy.ArrayFragment(real, cepstrumSize);
        }

        /// <summary>
        /// Method for computing real cepstrum of a signal
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="cepstrumSize"></param>
        /// <param name="fftSize"></param>
        /// <returns></returns>
        public static double[] Cepstrum(DiscreteSignal signal, int cepstrumSize, int fftSize = 512)
        {
            return Cepstrum(signal.Samples, cepstrumSize, fftSize);
        }
    }
}
