using NWaves.Filters.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Effect for speech whisperization.
    /// 
    /// Hint. Choose relatively small fft and hop sizes (e.g., 256 and 40).
    /// 
    /// </summary>
    public class WhisperEffect : OverlapAddFilter
    {
        /// <summary>
        /// Phase randomizer
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public WhisperEffect(int hopSize, int fftSize = 0) : base(hopSize, fftSize)
        {
            _gain = 1f / _fftSize;  // slightly correct the ISTFT gain
        }

        /// <summary>
        /// Process one spectrum at each STFT step
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        public override void ProcessSpectrum(float[] re, float[] im,
                                             float[] filteredRe, float[] filteredIm)
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
