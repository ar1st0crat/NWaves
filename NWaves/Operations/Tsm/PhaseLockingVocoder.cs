using NWaves.Utils;
using System;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Represents Phase Vocoder with identity phase locking [Puckette].
    /// </summary>
    public class PhaseLockingVocoder : PhaseVocoder
    {
        /// <summary>
        /// Array of spectrum magnitudes (at current step).
        /// </summary>
        private readonly double[] _mag;

        /// <summary>
        /// Array of spectrum phases (at current step).
        /// </summary>
        private readonly double[] _phase;

        /// <summary>
        /// Array of phase deltas.
        /// </summary>
        private readonly double[] _delta;

        /// <summary>
        /// Array of peak positions (indices).
        /// </summary>
        private readonly int[] _peaks;

        /// <summary>
        /// Constructs <see cref="PhaseLockingVocoder"/>.
        /// </summary>
        /// <param name="stretch">Stretch ratio</param>
        /// <param name="hopAnalysis">Hop length at analysis stage</param>
        /// <param name="fftSize">FFT size</param>
        public PhaseLockingVocoder(double stretch, int hopAnalysis, int fftSize = 0) : base(stretch, hopAnalysis, fftSize)
        {
            _mag = new double[_fftSize / 2 + 1];
            _phase = new double[_fftSize / 2 + 1];
            _delta = new double[_fftSize / 2 + 1];
            _peaks = new int[_fftSize / 4];
        }

        /// <summary>
        /// Processes spectrum with phase-locking at each STFT step.
        /// </summary>
        protected override void ProcessSpectrum()
        {
            for (var j = 0; j < _mag.Length; j++)
            {
                _mag[j] = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                _phase[j] = Math.Atan2(_im[j], _re[j]);
            }

            // spectral peaks in magnitude spectrum

            var peakCount = 0;

            for (var j = 2; j < _mag.Length - 3; j++)
            {
                if (_mag[j] <= _mag[j - 1] || _mag[j] <= _mag[j - 2] ||
                    _mag[j] <= _mag[j + 1] || _mag[j] <= _mag[j + 2])
                {
                    continue;   // if not a peak
                }

                _peaks[peakCount++] = j;
            }

            _peaks[peakCount++] = _mag.Length - 1;

            // assign phases at peaks to all neighboring frequency bins

            var leftPos = 1;

            for (var j = 0; j < peakCount - 1; j++)
            {
                var peakPos = _peaks[j];
                var peakPhase = _phase[peakPos];

                _delta[peakPos] = peakPhase - _prevPhase[peakPos];

                var deltaUnwrapped = _delta[peakPos] - _hopAnalysis * _omega[peakPos];
                var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                var freq = _omega[peakPos] + deltaWrapped / _hopAnalysis;

                _phaseTotal[peakPos] = _phaseTotal[peakPos] + _hopSynthesis * freq;

                var rightPos = (_peaks[j] + _peaks[j + 1]) / 2;

                for (var k = leftPos; k < rightPos; k++)
                {
                    _phaseTotal[k] = _phaseTotal[peakPos] + _phase[k] - _phase[peakPos];

                    _prevPhase[k] = _phase[k];

                    _re[k] = (float)(_mag[k] * Math.Cos(_phaseTotal[k]));
                    _im[k] = (float)(_mag[k] * Math.Sin(_phaseTotal[k]));
                }

                leftPos = rightPos;
            }
        }
    }
}
