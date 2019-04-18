using System;
using System.Linq;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Effects
{
    /// <summary>
    /// Pitch Shift effect based on phase vocoder and processing in frequency domain
    /// </summary>
    public class PitchShiftVocoderEffect : AudioEffect
    {
        /// <summary>
        /// Shift ratio
        /// </summary>
        private readonly float _shift;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Hop size
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of frame overlap
        /// </summary>
        private readonly int _overlapSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Frequency resolution
        /// </summary>
        private readonly float _freqResolution;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

        /// <summary>
        /// ISTFT normalization gain
        /// </summary>
        private readonly float _gain;

        /// <summary>
        /// Delay line
        /// </summary>
        private float[] _dl;

        /// <summary>
        /// Offset in the input delay line
        /// </summary>
        private int _inOffset;

        /// <summary>
        /// Offset in the output buffer
        /// </summary>
        private int _outOffset;

        /// <summary>
        /// Internal buffers
        /// </summary>
        private float[] _re;
        private float[] _im;
        private float[] _filteredRe;
        private float[] _filteredIm;
        private float[] _zeroblock;
        private float[] _lastSaved;

        /// <summary>
        /// Array of spectrum magnitudes (at current step)
        /// </summary>
        private float[] _mag;

        /// <summary>
        /// Array of spectrum phases (at current step)
        /// </summary>
        private float[] _phase;

        /// <summary>
        /// Array of phases computed at previous step
        /// </summary>
        private float[] _prevPhase;

        /// <summary>
        /// Array of new synthesized phases
        /// </summary>
        private float[] _phaseTotal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="shift"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        public PitchShiftVocoderEffect(int samplingRate, double shift, int fftSize = 1024, int hopSize = 64)
        {
            _shift = (float)shift;
            _fftSize = fftSize;
            _hopSize = hopSize;
            _overlapSize = _fftSize - _hopSize;

            Guard.AgainstInvalidRange(_hopSize, _fftSize, "Hop size", "FFT size");

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = (float)(2 * Math.PI / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize));

            _freqResolution = samplingRate / _fftSize;

            _dl = new float[_fftSize];
            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
            _zeroblock = new float[_fftSize];
            _lastSaved = new float[_overlapSize];

            _mag = new float[_fftSize / 2 + 1];
            _phase = new float[_fftSize / 2 + 1];
            _prevPhase = new float[_fftSize / 2 + 1];
            _phaseTotal = new float[_fftSize / 2 + 1];
        }

        /// <summary>
        /// Process one frame (block)
        /// </summary>
        public void ProcessFrame()
        {
            _zeroblock.FastCopyTo(_im, _fftSize);
            _dl.FastCopyTo(_re, _fftSize);

            _re.ApplyWindow(_window);

            _fft.Direct(_re, _im);

            var nextPhase = (float)(2 * Math.PI * _hopSize / _fftSize);

            for (var j = 0; j < _fftSize / 2 + 1; j++)
            {
                _mag[j] = (float)Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                _phase[j] = (float)Math.Atan2(_im[j], _re[j]);
                
                var delta = _phase[j] - _prevPhase[j];

                _prevPhase[j] = _phase[j];

                delta -= j * nextPhase;
                var deltaWrapped = MathUtils.Mod(delta + Math.PI, 2 * Math.PI) - Math.PI;

                _phase[j] = _freqResolution * (j + (float)deltaWrapped / nextPhase);
            }

            _zeroblock.FastCopyTo(_re, _fftSize);
            _zeroblock.FastCopyTo(_im, _fftSize);

            // "stretch" spectrum:

            var stretchPos = 0;
            for (var j = 0; j <= _fftSize / 2 && stretchPos <= _fftSize / 2; j++)
            {
                _re[stretchPos] += _mag[j];
                _im[stretchPos] = _phase[j] * _shift;

                stretchPos = (int)(j * _shift);
            }
            
            for (var j = 0; j <= _fftSize / 2; j++)
            {
                var mag = _re[j];
                var freqIndex = (_im[j] - j * _freqResolution) / _freqResolution;

                _phaseTotal[j] += nextPhase * (freqIndex + j);

                _filteredRe[j] = (float)(mag * Math.Cos(_phaseTotal[j]));
                _filteredIm[j] = (float)(mag * Math.Sin(_phaseTotal[j]));
            }

            for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
            {
                _filteredRe[j] = _filteredIm[j] = 0.0f;
            }

            _fft.Inverse(_filteredRe, _filteredIm);

            _filteredRe.ApplyWindow(_window);

            for (var j = 0; j < _overlapSize; j++)
            {
                _filteredRe[j] += _lastSaved[j];
            }

            _filteredRe.FastCopyTo(_lastSaved, _overlapSize, _hopSize);

            for (var i = 0; i < _filteredRe.Length; i++)        // Wet / Dry mix
            {
                _filteredRe[i] *= Wet * _gain;
                _filteredRe[i] += _dl[i] * Dry;
            }

            _dl.FastCopyTo(_dl, _overlapSize, _hopSize);

            _inOffset = _overlapSize;
            _outOffset = 0;
        }

        /// <summary>
        /// Online processing (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <returns></returns>
        public override float Process(float sample)
        {
            _dl[_inOffset++] = sample;

            if (_inOffset == _fftSize)
            {
                ProcessFrame();
            }

            return _filteredRe[_outOffset++];
        }

        /// <summary>
        /// Reset filter internals
        /// </summary>
        public override void Reset()
        {
            _inOffset = _overlapSize;
            _outOffset = 0;

            _zeroblock.FastCopyTo(_dl, _dl.Length);
            _zeroblock.FastCopyTo(_re, _re.Length);
            _zeroblock.FastCopyTo(_im, _im.Length);
            _zeroblock.FastCopyTo(_filteredRe, _filteredRe.Length);
            _zeroblock.FastCopyTo(_filteredIm, _filteredIm.Length);
            _zeroblock.FastCopyTo(_lastSaved, _lastSaved.Length);
            _zeroblock.FastCopyTo(_prevPhase, _prevPhase.Length);
            _zeroblock.FastCopyTo(_phaseTotal, _phaseTotal.Length);
        }
    }
}
