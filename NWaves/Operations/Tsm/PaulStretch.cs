using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;
using System.Linq;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Paul stretch algorithm
    /// </summary>
    class PaulStretch : IFilter
    {
        /// <summary>
        /// Hop size at analysis stage (STFT decomposition)
        /// </summary>
        private readonly int _hopAnalysis;

        /// <summary>
        /// Hop size at synthesis stage (STFT merging)
        /// </summary>
        private readonly int _hopSynthesis;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Stretch ratio
        /// </summary>
        private readonly double _stretch;

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
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private readonly float[] _re;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Randomizer for phases
        /// </summary>
        private readonly Random _rand = new Random();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch"></param>
        /// <param name="hopAnalysis"></param>
        /// <param name="fftSize"></param>
        public PaulStretch(double stretch, int hopAnalysis, int fftSize = 0)
        {
            _stretch = stretch;
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = (int)(hopAnalysis * stretch);
            _fftSize = (fftSize > 0) ? fftSize : 8 * Math.Max(_hopAnalysis, _hopSynthesis);

            _fft = new RealFft(_fftSize);

            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = 2 / (_fftSize * _window.Select(w => w * w).Sum() / _hopSynthesis);

            _re = new float[_fftSize];
            _im = new float[_fftSize];
        }

        /// <summary>
        /// Phase Vocoder algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            var input = signal.Samples;
            var output = new float[(int)(input.Length * _stretch) + _fftSize];

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
            {
                input.FastCopyTo(_re, _fftSize, posAnalysis);
                Array.Clear(_im, 0, _fftSize);

                _re.ApplyWindow(_window);

                _fft.Direct(_re, _re, _im);

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                    var phase = 2 * Math.PI * _rand.NextDouble();

                    _re[j] = (float)(mag * Math.Cos(phase));
                    _im[j] = (float)(mag * Math.Sin(phase));
                }
                _im[0] = 0;

                _fft.Inverse(_re, _im, _re);

                for (var j = 0; j < _re.Length; j++)
                {
                    output[posSynthesis + j] += _re[j] * _window[j];
                }

                for (var j = 0; j < _hopSynthesis; j++)
                {
                    output[posSynthesis + j] *= _gain;
                }

                posSynthesis += _hopSynthesis;
            }

            for (; posSynthesis < output.Length; posSynthesis++)
            {
                output[posSynthesis] *= _gain;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
