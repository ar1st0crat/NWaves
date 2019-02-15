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
    /// Effect for speech robotization.
    /// Currently it's based on the phase vocoder technique.
    /// </summary>
    public class RobotEffect : AudioEffect
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
        /// ISTFT normalization gain
        /// </summary>
        private readonly float _gain;

        /// <summary>
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private float[] _re;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private float[] _im;

        /// <summary>
        /// Internal array of zeros for a quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public RobotEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = (float)(2 * Math.PI / (_fftSize * _window.Select(w => w * w).Sum() / _hopSize));

            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _zeroblock = new float[_fftSize];
        }

        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopSize)
            {
                input.FastCopyTo(_re, _fftSize, posAnalysis);
                _zeroblock.FastCopyTo(_im, _fftSize);

                _re.ApplyWindow(_window);

                _fft.Direct(_re, _im);

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                    var phase = Math.Atan2(_im[j], _re[j]);

                    _re[j] = (float)mag;
                    _im[j] = 0;
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    _re[j] = _im[j] = 0.0f;
                }

                _fft.Inverse(_re, _im);

                for (var j = 0; j < _re.Length; j++)
                {
                    output[posSynthesis + j] += _re[j] * _window[j];
                }

                for (var j = 0; j < _hopSize; j++)
                {
                    output[posSynthesis + j] *= _gain;
                    output[j] = Wet * output[j] + Dry * input[j];
                }

                posSynthesis += _hopSize;
            }

            for (; posSynthesis < output.Length; posSynthesis++)
            {
                output[posSynthesis] *= _gain;
                output[posSynthesis] = Wet * output[posSynthesis] + Dry * input[posSynthesis];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
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
