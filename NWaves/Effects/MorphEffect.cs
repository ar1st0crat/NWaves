using NWaves.Effects.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;

namespace NWaves.Effects
{
    /// <summary>
    /// Represents effect of morphing (blending) two sound signals.
    /// </summary>
    public class MorphEffect : AudioEffect
    {
        /// <summary>
        /// Hop length.
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT for analysis and synthesis.
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of frame overlap.
        /// </summary>
        private readonly int _overlapSize;

        /// <summary>
        /// Internal FFT transformer.
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// Window coefficients.
        /// </summary>
        private readonly float[] _window;

        // Delay lines

        private readonly float[] _dl1;
        private readonly float[] _dl2;

        /// <summary>
        /// Offset in the input delay line.
        /// </summary>
        private int _inOffset;

        /// <summary>
        /// Offset in the output buffer.
        /// </summary>
        private int _outOffset;

        // Internal buffers

        private readonly float[] _re1;
        private readonly float[] _re2;
        private readonly float[] _im1;
        private readonly float[] _im2;
        private readonly float[] _filteredRe;
        private readonly float[] _filteredIm;
        private readonly float[] _lastSaved;

        /// <summary>
        /// Constructs <see cref="MorphEffect"/>.
        /// </summary>
        /// <param name="hopSize">Hop size (hop length, number of samples)</param>
        /// <param name="fftSize">FFT size</param>
        public MorphEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;
            _overlapSize = _fftSize - _hopSize;

            Guard.AgainstInvalidRange(_hopSize, _fftSize, "Hop size", "FFT size");

            _fft = new RealFft(_fftSize);
            _window = Window.OfType(WindowType.Hann, _fftSize);

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
        /// Processes one sample of input signal and one sample of the signal to be mixed.
        /// </summary>
        /// <param name="sample">Sample of input signal</param>
        /// <param name="mix">Sample of the signal to mix with input signal</param>
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
        /// Processes one frame (block).
        /// </summary>
        protected void ProcessFrame()
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
        /// Resets effect.
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
        /// Blends (mixes) entire input <paramref name="signal"/> with entire <paramref name="mix"/> signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="mix">Signal to mix with input signal</param>
        public DiscreteSignal ApplyTo(DiscreteSignal signal, DiscreteSignal mix)
        {
            Guard.AgainstInequality(signal.SamplingRate, mix.SamplingRate, "Input signal sampling rate", "Mix signal sampling rate");

            var filtered = new float[signal.Length];

            for (int i = 0, j = 0; i < filtered.Length; i++, j++)
            {
                if (j == mix.Length)
                {
                    j = 0;
                }

                filtered[i] = Process(signal[i], mix[j]);
            }

            return new DiscreteSignal(signal.SamplingRate, filtered);
        }

        /// <summary>
        /// Processes one sample. This method is not implemented in <see cref="MorphEffect"/> class.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            throw new NotImplementedException();
        }
    }
}
