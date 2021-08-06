using NWaves.Effects.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Effect of morphing two sound signals
    /// </summary>
    public class MorphEffect : AudioEffect
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
        /// Delay lines
        /// </summary>
        private float[] _dl1, _dl2;

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
        private float[] _re1, _re2;
        private float[] _im1, _im2;
        private float[] _filteredRe;
        private float[] _filteredIm;
        private float[] _lastSaved;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public MorphEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;
            _overlapSize = _fftSize - _hopSize;

            Guard.AgainstInvalidRange(_hopSize, _fftSize, "Hop size", "FFT size");

            _fft = new RealFft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _dl1 = new float[_fftSize];
            _re1 = new float[_fftSize];
            _im1 = new float[_fftSize];
            _dl2 = new float[_fftSize];
            _re2 = new float[_fftSize];
            _im2 = new float[_fftSize];
            _filteredRe = new float[_fftSize];
            _filteredIm = new float[_fftSize];
            _lastSaved = new float[_overlapSize];
        }

        /// <summary>
        /// Online processing (sample-by-sample)
        /// </summary>
        /// <param name="sample"></param>
        /// <param name="mix"></param>
        /// <returns></returns>
        public float Process(float sample, float mix)
        {
            _dl1[_inOffset] = sample;
            _dl2[_inOffset] = mix;
            _inOffset++;

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
            _dl1.FastCopyTo(_re1, _fftSize);
            _dl2.FastCopyTo(_re2, _fftSize);

            _re1.ApplyWindow(_window);
            _re2.ApplyWindow(_window);

            _fft.Direct(_re1, _re1, _im1);
            _fft.Direct(_re2, _re2, _im2);

            for (var j = 1; j <= _fftSize / 2; j++)
            {
                var mag1 = Math.Sqrt(_re1[j] * _re1[j] + _im1[j] * _im1[j]);
                var phase2 = Math.Atan2(_im2[j], _re2[j]);

                _filteredRe[j] = (float)(mag1 * Math.Cos(phase2));
                _filteredIm[j] = (float)(mag1 * Math.Sin(phase2));
            }

            _fft.Inverse(_filteredRe, _filteredIm, _filteredRe);

            _filteredRe.ApplyWindow(_window);

            for (var j = 0; j < _overlapSize; j++)
            {
                _filteredRe[j] += _lastSaved[j];
            }

            _filteredRe.FastCopyTo(_lastSaved, _overlapSize, _hopSize);

            for (var i = 0; i < _filteredRe.Length; i++)        // Wet / Dry mix
            {
                _filteredRe[i] *= Wet / _fftSize;
                _filteredRe[i] += _dl2[i] * Dry;
            }

            _dl1.FastCopyTo(_dl1, _overlapSize, _hopSize);
            _dl2.FastCopyTo(_dl2, _overlapSize, _hopSize);

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

            Array.Clear(_dl1, 0, _dl1.Length);
            Array.Clear(_re1, 0, _re1.Length);
            Array.Clear(_im1, 0, _im1.Length);
            Array.Clear(_dl2, 0, _dl2.Length);
            Array.Clear(_re2, 0, _re2.Length);
            Array.Clear(_im2, 0, _im2.Length);
            Array.Clear(_filteredRe, 0, _filteredRe.Length);
            Array.Clear(_filteredIm, 0, _filteredIm.Length);
            Array.Clear(_lastSaved, 0, _lastSaved.Length);
        }

        /// <summary>
        /// Offline processing
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate, "1st signal sampling rate", "2nd signal sampling rate");

            var filtered = new float[signal1.Length];

            for (int i = 0, j = 0; i < filtered.Length; i++, j++)
            {
                if (j == signal2.Length)
                {
                    j = 0;
                }

                filtered[i] = Process(signal1[i], signal2[j]);
            }

            return new DiscreteSignal(signal1.SamplingRate, filtered);
        }

        public override float Process(float sample)
        {
            throw new NotImplementedException();
        }
    }
}
