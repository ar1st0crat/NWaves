using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;
using System;
using System.Linq;

namespace NWaves.Features
{
    /// <summary>
    /// Class for pitch estimation and tracking
    /// </summary>
    public static class Pitch
    {
        /// <summary>
        /// Pitch estimation by autocorrelation method
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static float FromAutoCorrelation(DiscreteSignal signal,
                                                int startPos = 0,
                                                int endPos = -1,
                                                float low = 80,
                                                float high = 400)
        {
            var samplingRate = signal.SamplingRate;

            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz

            signal = signal[startPos, endPos];
            
            var cc = Operation.CrossCorrelate(signal, signal).Last(signal.Length);

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
        public static float FromAutoCorrelation(float[] samples,
                                                int samplingRate,
                                                int startPos = 0,
                                                int endPos = -1,
                                                float low = 80,
                                                float high = 400)
        {
            return FromAutoCorrelation(
                            new DiscreteSignal(samplingRate, samples),
                            startPos, endPos,
                            low, high);
        }

        /// <summary>
        /// Pitch estimation from zero crossing rate
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public static float FromZeroCrossingsSchmitt(DiscreteSignal signal,
                                                     int startPos = 0,
                                                     int endPos = -1,
                                                     float lowSchmittThreshold = -1e10f,
                                                     float highSchmittThreshold = 1e10f)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }
                        
            var f0 = 0.0f;

            float maxPositive = signal.Samples.Where(s => s > 0).Max();
            float minNegative = signal.Samples.Where(s => s < 0).Min();
            
            float highThreshold = highSchmittThreshold < 1e9f ? highSchmittThreshold : 0.15f * maxPositive;
            float lowThreshold = lowSchmittThreshold > -1e9f ? lowSchmittThreshold : 0.15f * minNegative;

            var zcr = 0;
            var firstCrossed = startPos;
            var lastCrossed = endPos - 1;

            // Schmitt trigger:

            var isCurrentHigh = false;

            for (var j = startPos; j < endPos - 1; j++)
            {
                if (signal[j] < highThreshold && signal[j + 1] >= highThreshold && !isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = true;
                }
                if (signal[j] > lowThreshold && signal[j + 1] <= lowThreshold && isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = false;
                }
            }

            return zcr > 0 ? (float)zcr * signal.SamplingRate / 2 / (endPos - startPos) : f0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public static float FromYin(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1)
        {
            return 0f;
        }

        /// <summary>
        /// Pitch estimation from signal cepstrum
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="low"></param>
        /// <param name="high"></param>
        /// <returns></returns>
        public static float FromCepstrum(DiscreteSignal signal,
                                         int startPos = 0,
                                         int endPos = -1,
                                         float low = 80,
                                         float high = 400,
                                         int cepstrumSize = 256,
                                         int fftSize = 512)
        {
            var samplingRate = signal.SamplingRate;

            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            signal = signal[startPos, endPos];

            var pitch1 = (int)(1.0 * samplingRate / high);                              // 2,5 ms = 400Hz
            var pitch2 = Math.Min(cepstrumSize - 1, (int)(1.0 * samplingRate / low));   // 12,5 ms = 80Hz

            var cepstralTransform = new CepstralTransform(cepstrumSize, fftSize);
            var cepstrum = cepstralTransform.Direct(signal);

            var max = cepstrum[pitch1];
            var peakIndex = pitch1;
            for (var k = pitch1 + 1; k <= pitch2; k++)
            {
                if (cepstrum[k] > max)
                {
                    max = cepstrum[k];
                    peakIndex = k;
                }
            }

            return (float) samplingRate / peakIndex;
        }

        /// <summary>
        /// Pitch estimation: Harmonic Product Spectrum
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public static float FromHps(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1)
        {
            return 0f;
        }
    }
}
