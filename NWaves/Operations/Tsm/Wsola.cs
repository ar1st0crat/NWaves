using System;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Waveform-Synchronized Overlap-Add
    /// </summary>
    public class Wsola : IFilter
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
        private readonly float _stretch;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

        /// <summary>
        /// Normalization coefficient for inverse STFT
        /// </summary>
        private readonly float _norm;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hopAnalysis"></param>
        /// <param name="hopSynthesis"></param>
        /// <param name="fftSize"></param>
        public Wsola(int hopAnalysis, int hopSynthesis, int fftSize = 0)
        {
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = hopSynthesis;
            _fftSize = (fftSize > 0) ? fftSize : 4 * Math.Max(hopAnalysis, hopSynthesis);

            _stretch = (float)_hopSynthesis / _hopAnalysis;

            _window = Window.OfType(WindowTypes.Hann, _fftSize);

            var ratio = _fftSize / (2.0f * _hopAnalysis);
            _norm = 4.0f / (_fftSize * ratio);
        }

        /// <summary>
        /// WSOLA algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var input = signal.Samples;
            var output = new float[(int)(input.Length * _stretch) + _fftSize];

            var re = new float[_fftSize];
            var im = new float[_fftSize];
            var cc = new float[_fftSize];

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
            {
                input.FastCopyTo(re, _fftSize, posAnalysis);

                re.ApplyWindow(_window);

                Operation.CrossCorrelate(re, im, re, im, cc);

                for (var j = 0; j < re.Length; j++)
                {
                    output[posSynthesis + j] += re[j] * _window[j] * _norm;
                }

                posSynthesis += _hopSynthesis;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
        }
    }
}
