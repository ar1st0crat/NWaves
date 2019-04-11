using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Effect for speech whisperization.
    /// Currently it's based on the phase vocoder technique.
    /// 
    /// Hint. Choose relatively small fft and hop sizes (e.g., 128 and 40).
    /// 
    /// </summary>
    public class WhisperEffect : AudioEffect
    {
        /// <summary>
        /// Hop size
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of frame overlap
        /// </summary>
        private readonly int _overlapSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

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
        /// Randomizer for phases
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public WhisperEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;
            _overlapSize = _fftSize - _hopSize;

            Guard.AgainstInvalidRange(_hopSize, _fftSize, "Hop size", "FFT size");

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _dl = new float[_fftSize];
            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
            _zeroblock = new float[_fftSize];
            _lastSaved = new float[_overlapSize];
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
        /// Process one frame (block)
        /// </summary>
        public void ProcessFrame()
        {
            _zeroblock.FastCopyTo(_im, _fftSize);
            _dl.FastCopyTo(_re, _fftSize);

            _re.ApplyWindow(_window);

            _fft.Direct(_re, _im);

            for (var j = 0; j <= _fftSize / 2; j++)
            {
                var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                var phase = 2 * Math.PI * _rand.NextDouble();

                _filteredRe[j] = (float)(mag * Math.Cos(phase));
                _filteredIm[j] = (float)(mag * Math.Sin(phase));
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
                _filteredRe[i] *= Wet * 2 / _fftSize;
                _filteredRe[i] += _dl[i] * Dry;
            }

            _dl.FastCopyTo(_dl, _overlapSize, _hopSize);

            _inOffset = _overlapSize;
            _outOffset = 0;
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
        }
    }
}
