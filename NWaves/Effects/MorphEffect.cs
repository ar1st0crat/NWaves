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
    /// 
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
        public MorphEffect(int hopSize, int fftSize = 0)
        {
            _hopSize = hopSize;
            _fftSize = (fftSize > 0) ? fftSize : 8 * hopSize;

            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);
            _windowSquared = _window.Select(w => w * w).ToArray();

            _re = new float[_fftSize];
            _im = new float[_fftSize];
            _zeroblock = new float[_fftSize];
        }

        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[input.Length];

            var windowSum = new float[output.Length];

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
                    output[posSynthesis + j] += (float)(_re[j] * 2 * Math.PI * _window[j]);
                    windowSum[posSynthesis + j] += _windowSquared[j];
                }

                posSynthesis += _hopSize;
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-3) continue;
                output[j] /= windowSum[j] * _fftSize / 2;
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
