using NWaves.Filters.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Effect for speech robotization.
    /// Currently it's based on the phase vocoder technique.
    /// 
    /// fftSize = 512
    /// hopSize = 90 .. 270
    /// 
    /// </summary>
    public class RobotEffect : OverlapAddFilter
    {
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public RobotEffect(int hopSize, int fftSize = 0) : base(hopSize, fftSize)
        {
            _gain *= (float)Math.PI;
        }

        /// <summary>
        /// Process one spectrum at each STFT step (simply set phases to 0)
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        public override void ProcessSpectrum(float[] re, float[] im,
                                             float[] filteredRe, float[] filteredIm)
        {
            for (var j = 0; j <= _fftSize / 2; j++)
            {
                filteredRe[j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                filteredIm[j] = 0;
            }
        }
    }
}
