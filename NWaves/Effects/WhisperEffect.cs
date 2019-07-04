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
    /// Hint. Choose relatively small fft and hop sizes (e.g., 256 and 40).
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
        private readonly RealFft _fft;

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
        private readonly float[] _dl;

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
        private readonly float[] _re;
        private readonly float[] _im;
        private readonly float[] _filteredRe;
        private readonly float[] _filteredIm;
        private readonly float[] _lastSaved;

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

            _fft = new RealFft(_fftSize);

            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = 2f / _fftSize;

            _dl = new float[_fftSize];
            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
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
            Array.Clear(_im, 0, _fftSize);
            _dl.FastCopyTo(_re, _fftSize);

            _re.ApplyWindow(_window);

            _fft.Direct(_re, _re, _im);

            for (var j = 0; j <= _fftSize / 2; j++)
            {
                var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                var phase = 2 * Math.PI * _rand.NextDouble();

                _filteredRe[j] = (float)(mag * Math.Cos(phase));
                _filteredIm[j] = (float)(mag * Math.Sin(phase));
            }
            _filteredIm[0] = 0;


            _fft.Inverse(_filteredRe, _filteredIm, _filteredRe);

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
        /// Reset filter internals
        /// </summary>
        public override void Reset()
        {
            _inOffset = _overlapSize;
            _outOffset = 0;

            Array.Clear(_dl, 0, _dl.Length);
            Array.Clear(_re, 0, _re.Length);
            Array.Clear(_im, 0, _im.Length);
            Array.Clear(_filteredRe, 0, _filteredRe.Length);
            Array.Clear(_filteredIm, 0, _filteredIm.Length);
            Array.Clear(_lastSaved, 0, _lastSaved.Length);
        }
    }
}
