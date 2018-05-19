using System;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Waveform-Synchronized Overlap-Add
    /// </summary>
    public class Wsola : IFilter
    {
        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private int _windowSize;

        /// <summary>
        /// Stretch ratio
        /// </summary>
        private readonly double _stretch;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch">Stretch ratio</param>
        /// <param name="windowSize"></param>
        public Wsola(double stretch, int windowSize = 0)
        {
            _stretch = stretch;
            _windowSize = windowSize;
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
            _windowSize = (int)(signal.SamplingRate * 0.08);      // 80 msec
            var overlapSize = (int)(signal.SamplingRate * 0.03);  // 30 msec
            var deltaSize = (int)(signal.SamplingRate * 0.02);    // 20 msec (interval to look for optimal shift)
            var middleSize = _windowSize - 2 * overlapSize;
            var hopSize = (int)((_windowSize - overlapSize) / _stretch);

            var input = signal.Samples;
            var output = new float[(int)(_stretch * (input.Length + _windowSize))];

            var offset = 0;
            var inputOffset = 0;
            var outputOffset = 0;

            var pos = 0;
            while (pos + hopSize + deltaSize < input.Length)
            {
                input.FastCopyTo(output, middleSize, offset, outputOffset);
                inputOffset += hopSize - overlapSize;

                // optimal overlap offset is the argmax of cross-correlation signal

                var endOffset = offset + middleSize;

                var optimalShift = 0;
                var maxCorrelation = 0.0f;
                
                for (var i = 0; i < deltaSize; i++)
                {
                    var xcorr = 0.0f;

                    for (var j = 0; j < overlapSize; j++)
                    {
                        xcorr += input[inputOffset + i + j] * input[endOffset + j];
                    }

                    if (xcorr > maxCorrelation)
                    {
                        maxCorrelation = xcorr;
                        optimalShift = i;
                    }
                }

                // =================================================================

                offset = inputOffset + optimalShift;

                for (var i = 0; i < overlapSize; i++)
                {
                    output[outputOffset + middleSize + i] =
                        (input[endOffset + i] * (overlapSize - i) + input[offset + i] * i) / overlapSize;
                }

                offset += overlapSize;
                inputOffset += overlapSize;
                outputOffset += _windowSize - overlapSize;

                pos += hopSize;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Online filtering (frame-by-frame)
        /// </summary>
        /// <param name="input">Input frame</param>
        /// <param name="filteringOptions">Filtering options</param>
        /// <returns>Processed frame</returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public void Reset()
        {
        }
    }
}


//var re = new float[_fftSize];
//var im = new float[_fftSize];
//var cc = new float[_fftSize];

//var posSynthesis = 0;
//for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
//{
//    input.FastCopyTo(re, _fftSize, posAnalysis);

//    re.ApplyWindow(_window);

//    Operation.CrossCorrelate(re, im, re, im, cc);

//    for (var j = 0; j < re.Length; j++)
//    {
//        output[posSynthesis + j] += re[j] * _window[j] * _norm;
//    }

//    posSynthesis += _hopSynthesis;
//}