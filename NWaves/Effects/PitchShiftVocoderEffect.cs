using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing pitch shift audio effect 
    /// based on overlap-add filtering and pitch shifting in frequency domain.
    /// </summary>
    public class PitchShiftVocoderEffect : OverlapAddFilter
    {
        /// <summary>
        /// Gets or sets pitch shift ratio.
        /// </summary>
        public float Shift { get; set; }

        /// <summary>
        /// Frequency resolution.
        /// </summary>
        private readonly float _freqResolution;

        /// <summary>
        /// Array of spectrum magnitudes (at the current step).
        /// </summary>
        private readonly float[] _mag;

        /// <summary>
        /// Array of spectrum phases (at the current step).
        /// </summary>
        private readonly float[] _phase;

        /// <summary>
        /// Array of phases computed at the previous step.
        /// </summary>
        private readonly float[] _prevPhase;

        /// <summary>
        /// Array of new synthesized phases (at the current step).
        /// </summary>
        private readonly float[] _phaseTotal;

        /// <summary>
        /// Construct <see cref="PitchShiftVocoderEffect"/>.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="shift">Pitch shift ratio</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="hopSize">Hop length</param>
        public PitchShiftVocoderEffect(int samplingRate, double shift, int fftSize = 1024, int hopSize = 64) : base(hopSize, fftSize)
        {
            Shift = (float)shift;

            _gain = (float)(2 * Math.PI / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize));

            _freqResolution = samplingRate / _fftSize;

            _mag = new float[_fftSize / 2 + 1];
            _phase = new float[_fftSize / 2 + 1];
            _prevPhase = new float[_fftSize / 2 + 1];
            _phaseTotal = new float[_fftSize / 2 + 1];
        }

        /// <summary>
        /// Process one spectrum at each Overlap-Add STFT step.
        /// </summary>
        /// <param name="re">Real parts of input spectrum</param>
        /// <param name="im">Imaginary parts of input spectrum</param>
        /// <param name="filteredRe">Real parts of output spectrum</param>
        /// <param name="filteredIm">Imaginary parts of output spectrum</param>
        protected override void ProcessSpectrum(float[] re, float[] im, float[] filteredRe, float[] filteredIm)
        {
            var nextPhase = (float)(2 * Math.PI * _hopSize / _fftSize);

            for (var j = 1; j <= _fftSize / 2; j++)
            {
                _mag[j] = (float)Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                _phase[j] = (float)Math.Atan2(im[j], re[j]);

                var delta = _phase[j] - _prevPhase[j];

                _prevPhase[j] = _phase[j];

                delta -= j * nextPhase;
                var deltaWrapped = MathUtils.Mod(delta + Math.PI, 2 * Math.PI) - Math.PI;

                _phase[j] = _freqResolution * (j + (float)deltaWrapped / nextPhase);
            }

            Array.Clear(re, 0, _fftSize);
            Array.Clear(im, 0, _fftSize);

            // "stretch" spectrum:

            var stretchPos = 0;
            for (var j = 0; j <= _fftSize / 2 && stretchPos <= _fftSize / 2; j++)
            {
                re[stretchPos] += _mag[j];
                im[stretchPos] = _phase[j] * Shift;

                stretchPos = (int)(j * Shift);
            }

            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag = re[j];
                var freqIndex = (im[j] - j * _freqResolution) / _freqResolution;

                _phaseTotal[j] += nextPhase * (freqIndex + j);

                filteredRe[j] = (float)(mag * Math.Cos(_phaseTotal[j]));
                filteredIm[j] = (float)(mag * Math.Sin(_phaseTotal[j]));
            }
        }

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
            base.Reset();
            Array.Clear(_prevPhase, 0, _prevPhase.Length);
            Array.Clear(_phaseTotal, 0, _phaseTotal.Length);
        }
    }
}
