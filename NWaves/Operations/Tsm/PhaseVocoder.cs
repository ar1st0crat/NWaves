using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Conventional Phase Vocoder
    /// </summary>
    public class PhaseVocoder : IFilter
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
        /// Linearly spaced frequencies
        /// </summary>
        private readonly double[] _omega;

        /// <summary>
        /// Internal buffer for real parts of analyzed block
        /// </summary>
        private readonly float[] _re;

        /// <summary>
        /// Internal buffer for imaginary parts of analyzed block
        /// </summary>
        private readonly float[] _im;

        /// <summary>
        /// Array of phases computed at previous step
        /// </summary>
        private readonly double[] _prevPhase;

        /// <summary>
        /// Array of new synthesized phases
        /// </summary>
        private readonly double[] _phaseTotal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch"></param>
        /// <param name="hopAnalysis"></param>
        /// <param name="fftSize"></param>
        public PhaseVocoder(double stretch, int hopAnalysis, int fftSize = 0)
        {
            _stretch = stretch;
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = (int)(hopAnalysis * stretch);
            _fftSize = (fftSize > 0) ? fftSize : 8 * Math.Max(_hopAnalysis, _hopSynthesis);
            
            _fft = new RealFft(_fftSize);

            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            _gain = 2 / (_fftSize * _window.Select(w => w * w).Sum() / _hopSynthesis);

            _omega = Enumerable.Range(0, _fftSize / 2 + 1)
                               .Select(f => 2 * Math.PI * f / _fftSize)
                               .ToArray();

            _re = new float[_fftSize];
            _im = new float[_fftSize];

            _prevPhase = new double[_fftSize / 2 + 1];
            _phaseTotal = new double[_fftSize / 2 + 1];
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

                for (var j = 0; j <= _fftSize / 2; j++)
                {
                    var mag = Math.Sqrt(_re[j] * _re[j] + _im[j] * _im[j]);
                    var phase = Math.Atan2(_im[j], _re[j]);

                    var delta = phase - _prevPhase[j];

                    var deltaUnwrapped = delta - _hopAnalysis * _omega[j];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = _omega[j] + deltaWrapped / _hopAnalysis;

                    _phaseTotal[j] += _hopSynthesis * freq;
                    _prevPhase[j] = phase;

                    _re[j] = (float)(mag * Math.Cos(_phaseTotal[j]));
                    _im[j] = (float)(mag * Math.Sin(_phaseTotal[j]));
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

        public void Reset()
        {
            Array.Clear(_phaseTotal, 0, _phaseTotal.Length);
            Array.Clear(_prevPhase, 0, _prevPhase.Length);
        }

        /*
        /// <summary>
        /// Phase Vocoder algorithm (slower, but more readable for tutorial)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            var stftAnalysis = new Stft(_fftSize, _hopAnalysis);
            var frames = stftAnalysis.Direct(signal);

            var omega = Enumerable.Range(0, _fftSize / 2 + 1)
                                  .Select(f => 2 * Math.PI * f / _fftSize)
                                  .ToArray();

            var prevPhase = new float[_fftSize / 2 + 1];
            var phaseTotal = new float[_fftSize / 2 + 1];

            for (var i = 0; i < frames.Count; i++)
            {
                var mag = frames[i].Magnitude;
                var phase = frames[i].Phase;

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var delta = phase[j] - prevPhase[j];
                    
                    var deltaUnwrapped = delta - _hopAnalysis * omega[j];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = omega[j] + deltaWrapped / _hopAnalysis;
                    
                    phaseTotal[j] += _hopSynthesis * freq;
                    prevPhase[j] = phase[j];
                }

                var re = new float[_fftSize];
                var im = new float[_fftSize];

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    re[j] = mag[j] * Math.Cos(phaseTotal[j]);
                    im[j] = mag[j] * Math.Sin(phaseTotal[j]);
                }

                frames[i] = new ComplexDiscreteSignal(1, re, im);
            }

            var stftSynthesis = new Stft(_fftSize, _hopSynthesis);
            return new DiscreteSignal(signal.SamplingRate, stftSynthesis.Inverse(frames));
        }
        */
    }
}
