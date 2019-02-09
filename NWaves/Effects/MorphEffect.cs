using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;
using System.Linq;

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
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

        /// <summary>
        /// Window coefficients squared
        /// </summary>
        private readonly float[] _windowSquared;

        /// <summary>
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private float[] _re1;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private float[] _im1;

        /// <summary>
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private float[] _re2;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private float[] _im2;

        /// <summary>
        /// Internal array of zeros for a quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public MorphEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);
            _windowSquared = _window.Select(w => w * w).ToArray();

            _re1 = new float[_fftSize];
            _im1 = new float[_fftSize];
            _re2 = new float[_fftSize];
            _im2 = new float[_fftSize];
            _zeroblock = new float[_fftSize];
        }

        public DiscreteSignal ApplyTo(DiscreteSignal signal1,
                                      DiscreteSignal signal2,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            Guard.AgainstInequality(signal1.SamplingRate, signal2.SamplingRate, "1st signal sampling rate", "2nd signal sampling rate");

            var input1 = signal1.Samples;
            var input2 = signal2.Samples;
            var output = new float[input1.Length];

            var windowSum = new float[output.Length];

            var posMorph = 0;
            var endMorph = signal2.Length - _fftSize;

            var posSynthesis = 0;

            for (var posAnalysis = 0; posAnalysis + _fftSize < input1.Length; posAnalysis += _hopSize, posMorph += _hopSize)
            {
                input1.FastCopyTo(_re1, _fftSize, posAnalysis);
                _zeroblock.FastCopyTo(_im1, _fftSize);

                if (posMorph > endMorph)
                {
                    posMorph = 0;
                }

                input2.FastCopyTo(_re2, _fftSize, posMorph);
                _zeroblock.FastCopyTo(_im2, _fftSize);

                _re1.ApplyWindow(_window);
                _re2.ApplyWindow(_window);

                _fft.Direct(_re1, _im1);
                _fft.Direct(_re2, _im2);

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var mag1 = Math.Sqrt(_re1[j] * _re1[j] + _im1[j] * _im1[j]);
                    var phase1 = Math.Atan2(_im1[j], _re1[j]);
                    var mag2 = Math.Sqrt(_re2[j] * _re2[j] + _im2[j] * _im2[j]);
                    var phase2 = Math.Atan2(_im2[j], _re2[j]);

                    _re1[j] = (float)(mag1 * Math.Cos(phase2));
                    _im1[j] = (float)(mag1 * Math.Sin(phase2));
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    _re1[j] = _im1[j] = 0.0f;
                }

                _fft.Inverse(_re1, _im1);

                for (var j = 0; j < _re1.Length; j++)
                {
                    output[posSynthesis + j] += _re1[j] * _window[j];
                }

                posSynthesis += _hopSize;
            }

            posMorph = 0;
            for (var j = 0; j < output.Length; j++, posMorph++)
            {
                output[j] /= _fftSize;

                if (posMorph > endMorph)
                {
                    posMorph = 0;
                }

                output[j] = Wet * output[j] + Dry * signal2[posMorph];
            }

            return new DiscreteSignal(signal1.SamplingRate, output);
        }

        public override float Process(float sample)
        {
            throw new NotImplementedException();
        }

        public override void Reset()
        {
        }
    }
}
