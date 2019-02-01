using NWaves.Operations;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
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
            
            float maxPositive = signal.Samples.Where(s => s > 0).Max();
            float minNegative = signal.Samples.Where(s => s < 0).Min();
            
            float highThreshold = highSchmittThreshold < 1e9f ? highSchmittThreshold : 0.75f * maxPositive;
            float lowThreshold = lowSchmittThreshold > -1e9f ? lowSchmittThreshold : 0.75f * minNegative;

            var zcr = 0;
            var firstCrossed = endPos;
            var lastCrossed = startPos;

            // Schmitt trigger:

            var isCurrentHigh = false;

            var j = startPos;
            for (; j < endPos - 1; j++)
            {
                if (signal[j] < highThreshold && signal[j + 1] >= highThreshold && !isCurrentHigh)
                {
                    isCurrentHigh = true;
                    firstCrossed = j;
                    break;
                }
                if (signal[j] > lowThreshold && signal[j + 1] <= lowThreshold && isCurrentHigh)
                {
                    isCurrentHigh = false;
                    firstCrossed = j;
                    break;
                }
            }

            for (; j < endPos - 1; j++)
            {
                if (signal[j] < highThreshold && signal[j + 1] >= highThreshold && !isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = true;
                    lastCrossed = j;
                }
                if (signal[j] > lowThreshold && signal[j + 1] <= lowThreshold && isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = false;
                    lastCrossed = j;
                }
            }

            return zcr > 0 && lastCrossed > firstCrossed ? (float)zcr * signal.SamplingRate / 2 / (lastCrossed - firstCrossed) : 0;
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
                                    int endPos = -1,
                                    float low = 80,
                                    float high = 400)
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
        /// Pitch estimation: Harmonic Sum Spectrum
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public static float FromHss(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80,
                                    float high = 400,
                                    int fftSize = 0)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            signal = signal[startPos, endPos];

            signal.ApplyWindow(WindowTypes.Hann);

            var size = fftSize > 0 ? fftSize : MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new Fft(size);

            var spectrum = fft.PowerSpectrum(signal, false).Samples;
            var sumSpectrum = spectrum.FastCopy();

            var startIdx = (int)(low * fft.Size / signal.SamplingRate) + 1;
            var endIdx = (int)(high * fft.Size / signal.SamplingRate);
            var decimations = Math.Min(spectrum.Length / endIdx, 10);

            var hssIndex = 0;
            var maxHss = 0.0f;

            for (var j = startIdx; j < endIdx; j++)
            {
                sumSpectrum[j] *= 1.5f;         // slightly emphasize 1st component

                for (var k = 2; k < decimations; k++)
                {
                    sumSpectrum[j] += (spectrum[j * k - 1] + spectrum[j * k] + spectrum[j * k + 1]) / 3;
                }

                if (sumSpectrum[j] > maxHss)
                {
                    maxHss = sumSpectrum[j];
                    hssIndex = j;
                }
            }

            return (float)hssIndex * signal.SamplingRate / fft.Size;
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
                                    int endPos = -1,
                                    float low = 80,
                                    float high = 400,
                                    int fftSize = 0)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            signal = signal[startPos, endPos];

            signal.ApplyWindow(WindowTypes.Hann);

            var size = fftSize > 0 ? fftSize : MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new Fft(size);

            var spectrum = fft.PowerSpectrum(signal, false).Samples;
            var sumSpectrum = spectrum.FastCopy();

            var startIdx = (int)(low * fft.Size / signal.SamplingRate) + 1;
            var endIdx = (int)(high * fft.Size / signal.SamplingRate);
            var decimations = Math.Min(spectrum.Length / endIdx, 10);

            var hpsIndex = 0;
            var maxHps = 0.0f;

            for (var j = startIdx; j < endIdx; j++)
            {
                for (var k = 2; k < decimations; k++)
                {
                    sumSpectrum[j] *= (spectrum[j * k - 1] + spectrum[j * k] + spectrum[j * k + 1]) / 3;
                }

                if (sumSpectrum[j] > maxHps)
                {
                    maxHps = sumSpectrum[j];
                    hpsIndex = j;
                }
            }

            return (float)hpsIndex * signal.SamplingRate / fft.Size;
        }
    }
}
