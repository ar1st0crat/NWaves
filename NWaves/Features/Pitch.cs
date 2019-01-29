using System.Collections.Generic;
using NWaves.Operations;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Features
{
    /// <summary>
    /// Class for pitch estimation and tracking
    /// </summary>
    public class Pitch
    {
        /// <summary>
        /// Length of analysis window (in seconds)
        /// </summary>
        private readonly double _windowSize;

        /// <summary>
        /// Hop length (in seconds)
        /// </summary>
        private readonly double _hopSize;

        /// <summary>
        /// Upper pitch frequency
        /// </summary>
        private readonly double _high;

        /// <summary>
        /// Lower pitch frequency
        /// </summary>
        private readonly double _low;

        /// <summary>
        /// Internal convolver
        /// </summary>
        private Convolver _convolver;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        public Pitch(double windowSize = 0.0256/*sec*/,
                     double hopSize = 0.010/*sec*/,
                     double low = 80,
                     double high = 400)
        {
            _windowSize = windowSize;
            _hopSize = hopSize;
            _low = low;
            _high = high;
        }

        /// <summary>
        /// Pitch tracking
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public List<float> Track(DiscreteSignal signal)
        {
            var samplingRate = signal.SamplingRate;
            var hopSize = (int)(samplingRate * _hopSize);
            var windowSize = (int)(samplingRate * _windowSize);
            var fftSize = MathUtils.NextPowerOfTwo(2 * windowSize - 1);
            _convolver = new Convolver(fftSize);
            
            var pitches = new List<float>();

            var pitch1 = (int)(1.0 * samplingRate / _high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / _low);     // 12,5 ms = 80Hz

            var block = new float[windowSize];    // buffer for the currently processed block
            var reversed = new float[windowSize]; // buffer for the currently processed block

            var cc = new float[fftSize];          // buffer for cross-correlation signal

            var pos = 0;
            while (pos + windowSize < signal.Length)
            {
                signal.Samples.FastCopyTo(block, windowSize, pos);
                signal.Samples.FastCopyTo(reversed, windowSize, pos);

                _convolver.CrossCorrelate(block, reversed, cc);

                var startPos = pitch1 + windowSize - 1;

                var max = cc[startPos];
                var peakIndex = startPos;
                for (var k = startPos + 1; k <= pitch2 + startPos; k++)
                {
                    if (cc[k] > max)
                    {
                        max = cc[k];
                        peakIndex = k;
                    }
                }

                peakIndex -= (windowSize - 1);

                pitches.Add((float)samplingRate / peakIndex);

                pos += hopSize;
            }

            return pitches;
        }

        /// <summary>
        /// Pitch estimation by autocorrelation method
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static float AutoCorrelation(DiscreteSignal signal,
                                            float low = 80,
                                            float high = 400)
        {
            var samplingRate = signal.SamplingRate;

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz

            var cc = Operation.CrossCorrelate(signal, signal).Last(signal.Length);

            //block.ApplyWindow(WindowTypes.Hamming);
            //func = new CepstralTransform(256).Direct(signal);

            var max = cc[pitch1];
            var peakIndex = pitch1;
            for (var k = pitch1 + 1; k <= pitch2; k++)
            {
                if (cc[k] > max)
                {
                    max = cc[k];
                    peakIndex = k;
                }
            }

            return (float)samplingRate / peakIndex;
        }

        /// <summary>
        /// Pitch estimation by autocorrelation method
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="samplingRate"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static float AutoCorrelation(float[] samples,
                                            int samplingRate,
                                            float low = 80,
                                            float high = 400)
        {
            return AutoCorrelation(new DiscreteSignal(samplingRate, samples), low, high);
        }
    }
}
