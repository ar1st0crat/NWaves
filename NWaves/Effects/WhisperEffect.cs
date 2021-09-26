using NWaves.Filters.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// <para>Class representing audio effect of speech whisperization.</para>
    /// <para>
    /// Hint. Choose relatively small fft and hop sizes (e.g., 256 and 40).
    /// </para>
    /// </summary>
    public class WhisperEffect : OverlapAddFilter
    {
        /// <summary>
        /// Phase randomizer.
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Construct <see cref="WhisperEffect"/>.
        /// </summary>
        /// <param name="hopSize">Hop size (hop length, number of samples)</param>
        /// <param name="fftSize">FFT size</param>
        public WhisperEffect(int hopSize, int fftSize = 0) : base(hopSize, fftSize)
        {
            _gain = 1f / _fftSize;  // slightly correct the ISTFT gain
        }

        /// <summary>
        /// Process one spectrum at each Overlap-Add STFT step.
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        protected override void ProcessSpectrum(float[] re,
                                                float[] im,
                                                float[] filteredRe,
                                                float[] filteredIm)
        {
            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag = Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                var phase = 2 * Math.PI * _rand.NextDouble();

                filteredRe[j] = (float)(mag * Math.Cos(phase));
                filteredIm[j] = (float)(mag * Math.Sin(phase));
            }
        }
    }
}
