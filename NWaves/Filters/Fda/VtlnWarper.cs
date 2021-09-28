using System;

namespace NWaves.Filters.Fda
{
    /// <summary>
    /// Vocal Tract Length Normalization (VTLN) similar to Kaldi implementation.
    /// </summary>
    public class VtlnWarper
    {
        /// <summary>
        /// Lower frequency.
        /// </summary>
        private readonly double _lowFreq;

        /// <summary>
        /// Upper frequency.
        /// </summary>
        private readonly double _highFreq;

        /// <summary>
        /// Lower frequency for VTLN.
        /// </summary>
        private readonly double _lowVtln;

        /// <summary>
        /// Upper frequency for VTLN.
        /// </summary>
        private readonly double _highVtln;

        //
        // Intermediate parameters for calculations
        //
        private readonly double _scale;
        private readonly double _scaleLeft;
        private readonly double _scaleRight;

        /// <summary>
        /// Constructs <see cref="VtlnWarper"/>.
        /// </summary>
        /// <param name="alpha">Warping factor</param>
        /// <param name="lowFreq">Lower frequency</param>
        /// <param name="highFreq">Upper frequency</param>
        /// <param name="lowVtln">Lower frequency for VTLN</param>
        /// <param name="highVtln">Upper frequency for VTLN</param>
        public VtlnWarper(double alpha,
                          double lowFreq,
                          double highFreq,
                          double lowVtln,
                          double highVtln)
        {
            _lowFreq = lowFreq;
            _highFreq = highFreq;

            _lowVtln = lowVtln * Math.Max(1, alpha);
            _highVtln = highVtln * Math.Min(1, alpha);

            _scale = 1 / alpha;
            _scaleLeft = (_scale * _lowVtln - lowFreq) / (_lowVtln - lowFreq);
            _scaleRight = (highFreq - _scale * _highVtln) / (highFreq - _highVtln);
        }

        /// <summary>
        /// Warps frequency <paramref name="freq"/>.
        /// </summary>
        public double Warp(double freq)
        {
            if (freq < _lowVtln)
            {
                return _lowFreq + _scaleLeft * (freq - _lowFreq);
            }
            else if (freq < _highVtln)
            {
                return _scale * freq;
            }

            return _highFreq + _scaleRight * (freq - _highFreq);
        }
    }
}
