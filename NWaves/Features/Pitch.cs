using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;

namespace NWaves.Features
{
    /// <summary>
    /// Class providing methods for pitch estimation and tracking.
    /// </summary>
    public static class Pitch
    {
        #region time-domain methods

        /// <summary>
        /// Estimate pitch from <paramref name="samples"/> using autocorrelation method.
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="startPos">Index of the first sample in array for processing</param>
        /// <param name="endPos">Index of the last sample in array for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        public static float FromAutoCorrelation(float[] samples,
                                                int samplingRate,
                                                int startPos = 0,
                                                int endPos = -1,
                                                float low = 80/*Hz*/,
                                                float high = 400/*Hz*/)
        {
            if (endPos == -1)
            {
                endPos = samples.Length;
            }

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz

            var block = new DiscreteSignal(samplingRate, samples)[startPos, endPos].Samples;

            var fftSize = MathUtils.NextPowerOfTwo(2 * block.Length - 1);

            var cc = new float[fftSize];

            new Convolver(fftSize).CrossCorrelate(block, block.FastCopy(), cc);

            var start = pitch1 + block.Length - 1;
            var end = Math.Min(start + pitch2, cc.Length);

            var max = start < cc.Length ? cc[start] : 0;

            var peakIndex = start;
            for (var k = start; k < end; k++)
            {
                if (cc[k] > max)
                {
                    max = cc[k];
                    peakIndex = k - block.Length + 1;
                }
            }

            return max > 1.0f ? (float)samplingRate / peakIndex : 0;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> using autocorrelation method.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        public static float FromAutoCorrelation(DiscreteSignal signal,
                                                int startPos = 0,
                                                int endPos = -1,
                                                float low = 80,
                                                float high = 400)
        {
            return FromAutoCorrelation(signal.Samples, signal.SamplingRate, startPos, endPos, low, high);
        }

        /// <summary>
        /// Estimate pitch from <paramref name="samples"/> based on zero crossing rate and Schmitt trigger.
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="startPos">Index of the first sample in array for processing</param>
        /// <param name="endPos">Index of the last sample in array for processing</param>
        /// <param name="lowSchmittThreshold">Lower threshold in Schmitt trigger</param>
        /// <param name="highSchmittThreshold">Upper threshold in Schmitt trigger</param>
        public static float FromZeroCrossingsSchmitt(float[] samples,
                                                     int samplingRate,
                                                     int startPos = 0,
                                                     int endPos = -1,
                                                     float lowSchmittThreshold = -1e10f,
                                                     float highSchmittThreshold = 1e10f)
        {
            if (endPos == -1)
            {
                endPos = samples.Length;
            }

            var maxPositive = 0.0f;
            var minNegative = 0.0f;

            for (var i = startPos; i < endPos; i++)
            {
                if (samples[i] > 0 && samples[i] > maxPositive) maxPositive = samples[i];
                if (samples[i] < 0 && samples[i] < minNegative) minNegative = samples[i];
            }

            var highThreshold = highSchmittThreshold < 1e9f ? highSchmittThreshold : 0.75f * maxPositive;
            var lowThreshold = lowSchmittThreshold > -1e9f ? lowSchmittThreshold : 0.75f * minNegative;

            var zcr = 0;
            var firstCrossed = endPos;
            var lastCrossed = startPos;

            // Schmitt trigger:

            var isCurrentHigh = false;

            var j = startPos;
            for (; j < endPos - 1; j++)
            {
                if (samples[j] < highThreshold && samples[j + 1] >= highThreshold && !isCurrentHigh)
                {
                    isCurrentHigh = true;
                    firstCrossed = j;
                    break;
                }
                if (samples[j] > lowThreshold && samples[j + 1] <= lowThreshold && isCurrentHigh)
                {
                    isCurrentHigh = false;
                    firstCrossed = j;
                    break;
                }
            }

            for (; j < endPos - 1; j++)
            {
                if (samples[j] < highThreshold && samples[j + 1] >= highThreshold && !isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = true;
                    lastCrossed = j;
                }
                if (samples[j] > lowThreshold && samples[j + 1] <= lowThreshold && isCurrentHigh)
                {
                    zcr++;
                    isCurrentHigh = false;
                    lastCrossed = j;
                }
            }

            return zcr > 0 && lastCrossed > firstCrossed ? (float)zcr * samplingRate / 2 / (lastCrossed - firstCrossed) : 0;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> based on zero crossing rate and Schmitt trigger.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in array for processing</param>
        /// <param name="endPos">Index of the last sample in array for processing</param>
        /// <param name="lowSchmittThreshold">Lower threshold in Schmitt trigger</param>
        /// <param name="highSchmittThreshold">Upper threshold in Schmitt trigger</param>
        public static float FromZeroCrossingsSchmitt(DiscreteSignal signal,
                                                     int startPos = 0,
                                                     int endPos = -1,
                                                     float lowSchmittThreshold = -1e10f,
                                                     float highSchmittThreshold = 1e10f)
        {
            return FromZeroCrossingsSchmitt(signal.Samples,
                                            signal.SamplingRate,
                                            startPos,
                                            endPos,
                                            lowSchmittThreshold,
                                            highSchmittThreshold);
        }

        /// <summary>
        /// <para>Estimate pitch from <paramref name="samples"/> using YIN algorithm:</para>
        /// <para>
        /// De Cheveigne, A., Kawahara, H. YIN, a fundamental frequency estimator for speech and music. 
        /// The Journal of the Acoustical Society of America, 111(4). - 2002.
        /// </para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="startPos">Index of the first sample in array for processing</param>
        /// <param name="endPos">Index of the last sample in array for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="cmdfThreshold">CMDF threshold</param>
        public static float FromYin(float[] samples,
                                    int samplingRate,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    float cmdfThreshold = 0.2f)
        {
            if (endPos == -1)
            {
                endPos = samples.Length;
            }

            var pitch1 = (int)(1.0 * samplingRate / high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(1.0 * samplingRate / low);     // 12,5 ms = 80Hz

            var length = (endPos - startPos) / 2;

            // cumulative mean difference function (CMDF):

            var cmdf = new float[length];

            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j < length; j++)
                {
                    var diff = samples[j + startPos] - samples[i + j + startPos];
                    cmdf[i] += diff * diff;
                }
            }

            cmdf[0] = 1;

            var sum = 0.0f;
            for (var i = 1; i < length; i++)
            {
                sum += cmdf[i];
                cmdf[i] *= i / sum;
            }

            // adjust t according to some threshold:

            var t = pitch1;     // focusing on range [pitch1 .. pitch2]

            for (; t < pitch2; t++)
            {
                if (cmdf[t] < cmdfThreshold)
                {
                    while (t + 1 < pitch2 && cmdf[t + 1] < cmdf[t]) t++;
                    break;
                }
            }

            // no pitch

            if (t == pitch2 || cmdf[t] >= cmdfThreshold)
            {
                return 0.0f;
            }

            // parabolic interpolation:

            var x1 = t < 1 ? t : t - 1;
            var x2 = t + 1 < length ? t + 1 : t;

            if (t == x1)
            {
                if (cmdf[t] > cmdf[x2])
                {
                    t = x2;
                }
            }
            else if (t == x2)
            {
                if (cmdf[t] > cmdf[x1])
                {
                    t = x1;
                }
            }
            else
            {
                t = (int)(t + (cmdf[x2] - cmdf[x1]) / (2 * cmdf[t] - cmdf[x2] - cmdf[x1]) / 2);
            }

            return samplingRate / t;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> using YIN algorithm.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="cmdfThreshold">CMDF threshold</param>
        public static float FromYin(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    float cmdfThreshold = 0.2f)
        {
            return FromYin(signal.Samples, signal.SamplingRate, startPos, endPos, low, high, cmdfThreshold);
        }

        #endregion

        #region frequency-domain methods

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> using Harmonic Sum Spectrum (HSS) method.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="fftSize">FFT size</param>
        public static float FromHss(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    int fftSize = 0)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            if (startPos != 0 || endPos != signal.Length)
            {
                signal = signal[startPos, endPos];
            }

            signal.ApplyWindow(WindowType.Hann);

            var size = fftSize > 0 ? fftSize : MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new RealFft(size);

            var spectrum = fft.PowerSpectrum(signal, false).Samples;

            return FromHss(spectrum, signal.SamplingRate, low, high);
        }

        /// <summary>
        /// Estimate pitch from <paramref name="spectrum"/> using Harmonic Sum Spectrum (HSS) method.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        public static float FromHss(float[] spectrum,
                                    int samplingRate,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/)
        {
            var sumSpectrum = spectrum.FastCopy();

            var fftSize = (spectrum.Length - 1) * 2;

            var startIdx = (int)(low * fftSize / samplingRate) + 1;
            var endIdx = (int)(high * fftSize / samplingRate) + 1;
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

            return (float)hssIndex * samplingRate / fftSize;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> using Harmonic Product Spectrum (HPS) method.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="fftSize">FFT size</param>
        public static float FromHps(DiscreteSignal signal,
                                    int startPos = 0,
                                    int endPos = -1,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/,
                                    int fftSize = 0)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            if (startPos != 0 || endPos != signal.Length)
            {
                signal = signal[startPos, endPos];
            }

            signal.ApplyWindow(WindowType.Hann);

            var size = fftSize > 0 ? fftSize : MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new RealFft(size);

            var spectrum = fft.PowerSpectrum(signal, false).Samples;

            return FromHps(spectrum, signal.SamplingRate, low, high);
        }

        /// <summary>
        /// Estimate pitch from <paramref name="spectrum"/> using Harmonic Product Spectrum (HPS) method.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        public static float FromHps(float[] spectrum,
                                    int samplingRate,
                                    float low = 80/*Hz*/,
                                    float high = 400/*Hz*/)
        {
            var sumSpectrum = spectrum.FastCopy();

            var fftSize = (spectrum.Length - 1) * 2;

            var startIdx = (int)(low * fftSize / samplingRate) + 1;
            var endIdx = (int)(high * fftSize / samplingRate) + 1;
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

            return (float)hpsIndex * samplingRate / fftSize;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> based on spectral peaks.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="fftSize">FFT size</param>
        public static float FromSpectralPeaks(DiscreteSignal signal,
                                              int startPos = 0,
                                              int endPos = -1,
                                              float low = 80/*Hz*/,
                                              float high = 400/*Hz*/,
                                              int fftSize = 0)
        {
            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            if (startPos != 0 || endPos != signal.Length)
            {
                signal = signal[startPos, endPos];
            }

            signal.ApplyWindow(WindowType.Hann);

            var size = fftSize > 0 ? fftSize : MathUtils.NextPowerOfTwo(signal.Length);
            var fft = new RealFft(size);

            var spectrum = fft.PowerSpectrum(signal, false).Samples;

            return FromSpectralPeaks(spectrum, signal.SamplingRate, low, high);
        }

        /// <summary>
        /// Estimate pitch from <paramref name="spectrum"/> based on spectral peaks.
        /// </summary>
        /// <param name="spectrum">Spectrum</param>
        /// <param name="samplingRate">Sampling rate</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        public static float FromSpectralPeaks(float[] spectrum,
                                              int samplingRate,
                                              float low = 80/*Hz*/,
                                              float high = 400/*Hz*/)
        {
            var fftSize = (spectrum.Length - 1) * 2;

            var startIdx = (int)(low * fftSize / samplingRate) + 1;
            var endIdx = (int)(high * fftSize / samplingRate) + 1;

            for (var k = startIdx + 1; k < endIdx; k++)
            {
                if (spectrum[k] > spectrum[k - 1] && spectrum[k] > spectrum[k - 2] &&
                    spectrum[k] > spectrum[k + 1] && spectrum[k] > spectrum[k + 2])
                {
                    return (float)k * samplingRate / fftSize;
                }
            }

            return (float)startIdx * samplingRate / fftSize;
        }

        /// <summary>
        /// Estimate pitch from <paramref name="signal"/> based on its cepstrum.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startPos">Index of the first sample in signal for processing</param>
        /// <param name="endPos">Index of the last sample in signal for processing</param>
        /// <param name="low">Lower frequency of expected pitch range</param>
        /// <param name="high">Upper frequency of expected pitch range</param>
        /// <param name="cepstrumSize">Size of cepstrum</param>
        /// <param name="fftSize">FFT size</param>
        public static float FromCepstrum(DiscreteSignal signal,
                                         int startPos = 0,
                                         int endPos = -1,
                                         float low = 80/*Hz*/,
                                         float high = 400/*Hz*/,
                                         int cepstrumSize = 256,
                                         int fftSize = 1024)
        {
            var samplingRate = signal.SamplingRate;

            if (endPos == -1)
            {
                endPos = signal.Length;
            }

            if (startPos != 0 || endPos != signal.Length)
            {
                signal = signal[startPos, endPos];
            }

            var pitch1 = (int)(1.0 * samplingRate / high);                              // 2,5 ms = 400Hz
            var pitch2 = Math.Min(cepstrumSize - 1, (int)(1.0 * samplingRate / low));   // 12,5 ms = 80Hz

            var cepstrum = new float[cepstrumSize];

            var cepstralTransform = new CepstralTransform(cepstrumSize, fftSize);
            cepstralTransform.RealCepstrum(signal.Samples, cepstrum);

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

            return (float)samplingRate / peakIndex;
        }

        #endregion
    }
}
