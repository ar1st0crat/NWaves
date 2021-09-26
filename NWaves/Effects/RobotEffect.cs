using NWaves.Filters.Base;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// <para>Class representing audio effect of speech robotization.</para>
    /// <para>Usual settings: 
    /// <list type="bullet">
    ///     <item>fftSize = 512, hopSize = 70 .. 270</item>
    ///     <item>fftSize = 1024, hopSize = 140 .. 540</item>
    ///     <item>fftSize = 2048, hopSize = 280 .. 1080</item>
    ///     <item>etc.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class RobotEffect : OverlapAddFilter
    {
        /// <summary>
        /// Construct <see cref="RobotEffect"/>.
        /// </summary>
        /// <param name="hopSize">Hop size (hop length, number of samples)</param>
        /// <param name="fftSize">FFT size</param>
        public RobotEffect(int hopSize, int fftSize = 0) : base(hopSize, fftSize)
        {
            _gain *= (float)Math.PI;
        }

        /// <summary>
        /// Process one spectrum at each Overlap-Add STFT step (simply set phases to 0).
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
            for (var j = 0; j <= _fftSize / 2; j++)
            {
                filteredRe[j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                filteredIm[j] = 0;
            }
        }
    }
}
