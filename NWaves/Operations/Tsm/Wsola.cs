using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Utils;
using NWaves.Windows;
using System;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Waveform-Synchronized Overlap-Add
    /// </summary>
    public class Wsola : IFilter
    {
        /// <summary>
        /// Stretch ratio
        /// </summary>
        protected readonly double _stretch;

        /// <summary>
        /// Window size
        /// </summary>
        protected int _windowSize;

        /// <summary>
        /// Hop size at analysis stage (STFT decomposition)
        /// </summary>
        private int _hopAnalysis;

        /// <summary>
        /// Hop size at synthesis stage (STFT merging)
        /// </summary>
        protected int _hopSynthesis;

        /// <summary>
        /// Maximum length of the fragment for search of the most similar waveform
        /// </summary>
        protected int _maxDelta;

        /// <summary>
        /// True if parameters were set by user (not by default)
        /// </summary>
        private bool _userParameters;

        /// <summary>
        /// Internal convolver
        /// (will be used for evaluating auto-correlation if the window size is too big)
        /// </summary>
        private Convolver _convolver;

        /// <summary>
        /// Cross-correlation signal
        /// </summary>
        private float[] _cc;


        /// <summary>
        /// Constructor with detailed WSOLA settings
        /// </summary>
        /// <param name="stretch">Stretch ratio</param>
        /// <param name="windowSize"></param>
        public Wsola(double stretch, int windowSize, int hopAnalysis, int maxDelta = 0)
        {
            _stretch = stretch;
            _windowSize = Math.Max(windowSize, 32);
            _hopAnalysis = Math.Max(hopAnalysis, 10);
            _hopSynthesis = (int)(_hopAnalysis * stretch);
            _maxDelta = maxDelta > 2 ? maxDelta : _hopSynthesis + _hopSynthesis % 1;

            _userParameters = true;

            PrepareConvolver();
        }

        /// <summary>
        /// Constructor with smart parameter autoderivation 
        /// </summary>
        /// <param name="stretch"></param>
        public Wsola(double stretch)
        {
            _stretch = stretch;

            // IMO these are good parameters for different stretch ratios

            if (_stretch > 1.5)        // parameters are for 22.05 kHz sampling rate, so they will be adjusted for an input signal
            {
                _windowSize = 1024;     // 46,4 ms
                _hopAnalysis = 128;     //  5,8 ms
            }
            else if (_stretch > 1.1)   
            {
                _windowSize = 1536;     // 69,7 ms
                _hopAnalysis = 256;     // 10,6 ms
            }
            else if (_stretch > 0.6)
            {
                _windowSize = 1536;     // 69,7 ms
                _hopAnalysis = 690;     // 31,3 ms
            }
            else
            {
                _windowSize = 1024;     // 46,4 ms
                _hopAnalysis = 896;     // 40,6 ms
            }

            _hopSynthesis = (int)(_hopAnalysis * stretch);
            _maxDelta = _hopSynthesis + _hopSynthesis % 1;

            PrepareConvolver();
        }

        /// <summary>
        /// For large window sizes prepare the internal convolver
        /// </summary>
        private void PrepareConvolver()
        {
            var fftSize = MathUtils.NextPowerOfTwo(_windowSize + _maxDelta - 1);

            if (fftSize >= 512)
            {
                _convolver = new Convolver(fftSize);
                _cc = new float[fftSize];
            }
        }

        /// <summary>
        /// WSOLA algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            // adjust default parameters for a new sampling rate

            if (signal.SamplingRate != 22050 && !_userParameters)
            {
                var factor = (float) signal.SamplingRate / 22050;

                _windowSize = (int)(_windowSize * factor);
                _hopAnalysis = (int)(_hopAnalysis * factor);
                _hopSynthesis = (int)(_hopAnalysis * _stretch);
                _maxDelta = (int)(_maxDelta * factor);

                PrepareConvolver();
            }

            // and now WSOLA:

            var input = signal.Samples;
            var output = new float[(int)(_stretch * (input.Length + _windowSize))];

            var window = Window.OfType(WindowTypes.Hann, _windowSize);
            var windowSum = new float[output.Length];

            var current = new float[_windowSize + _maxDelta];
            var prev = new float[_windowSize];

            for (int posAnalysis = 0,
                     posSynthesis = 0;
                     posAnalysis + _windowSize + _maxDelta + _hopSynthesis < input.Length;
                     posAnalysis += _hopAnalysis,
                     posSynthesis += _hopSynthesis)
            {
                int delta = 0;

                if (posAnalysis > _maxDelta / 2)
                {
                    input.FastCopyTo(current, _windowSize + _maxDelta, posAnalysis - _maxDelta / 2);

                    delta = WaveformSimilarityPos(current, prev, _maxDelta);
                }
                else
                {
                    input.FastCopyTo(current, _windowSize + _maxDelta, posAnalysis);
                }

                int size = Math.Min(_windowSize, output.Length - posSynthesis);

                for (var j = 0; j < size; j++)
                {
                    current[delta + j] *= window[j];
                    output[posSynthesis + j] += current[delta + j];
                    windowSum[posSynthesis + j] += window[j];
                }

                input.FastCopyTo(prev, _windowSize, posAnalysis + delta - _maxDelta / 2 + _hopSynthesis);
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-2) continue;
                output[j] /= windowSum[j];
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Position of the best found waveform similarity
        /// </summary>
        /// <param name="current"></param>
        /// <param name="prev"></param>
        /// <param name="maxDelta"></param>
        /// <returns></returns>
        public int WaveformSimilarityPos(float[] current, float[] prev, int maxDelta)
        {
            var optimalShift = 0;
            var maxCorrelation = 0.0f;

            // for small window sizes cross-correlate directly:

            if (_convolver == null)
            {
                for (var i = 0; i < maxDelta; i++)
                {
                    var xcorr = 0.0f;

                    for (var j = 0; j < prev.Length; j++)
                    {
                        xcorr += current[i + j] * prev[j];
                    }

                    if (xcorr > maxCorrelation)
                    {
                        maxCorrelation = xcorr;
                        optimalShift = i;
                    }
                }
            }
            // for very large window sizes better use FFT convolution:
            else
            {
                _convolver.CrossCorrelate(current, prev, _cc);

                for (int i = prev.Length - 1, j = 0; i < prev.Length + _maxDelta - 1; i++, j++)
                {
                    if (_cc[i] > maxCorrelation)
                    {
                        maxCorrelation = _cc[i];
                        optimalShift = j;
                    }
                }
            }

            return optimalShift;
        }
    }
}
