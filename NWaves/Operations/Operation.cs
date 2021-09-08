using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations
{
    /// <summary>
    /// Static class for DSP/audio operations.
    /// 
    /// Main operations implemented:
    /// 
    ///     - convolution
    ///     - cross-correlation
    ///     - block convolution
    ///     - deconvolution
    ///     - resampling
    ///     - time-stretching
    ///     - rectification
    ///     - envelope detection
    ///     - spectral subtraction
    ///     - normalization (peak / RMS)
    ///     - periodogram (Welch / Lomb-Scargle)
    /// 
    /// </summary>
    public static class Operation
    {
        /// <summary>
        /// Fast convolution via FFT of real-valued signals.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Convolution kernel</param>
        /// <returns>Convolution of signal with kernel</returns>
        public static DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)
        {
            return new Convolver().Convolve(signal, kernel);
        }

        /// <summary>
        /// Fast convolution via FFT for general complex-valued case
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Convolution kernel</param>
        /// <returns>Convolution of signal with kernel</returns>
        public static ComplexDiscreteSignal Convolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)
        {
            return new ComplexConvolver().Convolve(signal, kernel);
        }

        /// <summary>
        /// Fast convolution for double arrays (used mainly in filter design)
        /// </summary>
        /// <param name="input">Array of samples</param>
        /// <param name="kernel">Convolution kernel</param>
        /// <returns>Convolution of signal with kernel</returns>
        public static double[] Convolve(double[] input, double[] kernel)
        {
            return Convolve(new ComplexDiscreteSignal(1, input), 
                            new ComplexDiscreteSignal(1, kernel)).Real;
        }

        /// <summary>
        /// Fast cross-correlation via FFT
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        /// <returns>Cross-correlation between the first and the second signal</returns>
        public static DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            return new Convolver().CrossCorrelate(signal1, signal2);
        }

        /// <summary>
        /// Fast complex cross-correlation via FFT
        /// </summary>
        /// <param name="signal1">First signal</param>
        /// <param name="signal2">Second signal</param>
        /// <returns>Cross-correlation between the first and the second signal</returns>
        public static ComplexDiscreteSignal CrossCorrelate(ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            return new ComplexConvolver().CrossCorrelate(signal1, signal2);
        }

        /// <summary>
        /// Method implements block convolution of signals (using either OLA or OLS algorithm)
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Convolution kernel</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="method">Block convolution method (OverlapAdd / OverlapSave)</param>
        /// <returns>Result of block convolution of signal with kernel</returns>
        public static DiscreteSignal BlockConvolve(DiscreteSignal signal,
                                                   DiscreteSignal kernel,
                                                   int fftSize,
                                                   FilteringMethod method = FilteringMethod.OverlapSave)
        {
            IFilter blockConvolver;

            if (method == FilteringMethod.OverlapAdd)
            {
                blockConvolver = new OlaBlockConvolver(kernel.Samples, fftSize);
            }
            else
            {
                blockConvolver = new OlsBlockConvolver(kernel.Samples, fftSize);
            }

            return blockConvolver.ApplyTo(signal);
        }
        
        /// <summary>
        /// Deconvolution via FFT for general complex-valued case.
        ///  
        /// NOTE!
        /// 
        /// Deconvolution is an experimental feature.
        /// It's problematic due to division by zero.
        /// 
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Kernel</param>
        /// <returns>Deconvolved signal</returns>
        public static ComplexDiscreteSignal Deconvolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)
        {
            return new ComplexConvolver().Deconvolve(signal, kernel);
        }

        /// <summary>
        /// Interpolation followed by low-pass filtering
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Interpolation factor (e.g. factor=2 if 8000 Hz -> 16000 Hz)</param>
        /// <param name="filter">Low-pass anti-aliasing filter</param>
        /// <returns>Interpolated signal</returns>
        public static DiscreteSignal Interpolate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            return new Resampler().Interpolate(signal, factor, filter);
        }

        /// <summary>
        /// Decimation preceded by low-pass filtering
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Decimation factor (e.g. factor=2 if 16000 Hz -> 8000 Hz)</param>
        /// <param name="filter">Low-pass anti-aliasing filter</param>
        /// <returns>Decimated signal</returns>
        public static DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            return new Resampler().Decimate(signal, factor, filter);
        }

        /// <summary>
        /// Band-limited resampling
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="newSamplingRate">Desired sampling rate</param>
        /// <param name="filter">Low-pass anti-aliasing filter</param>
        /// <returns>Resampled signal</returns>
        public static DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate, FirFilter filter = null)
        {
            return new Resampler().Resample(signal, newSamplingRate, filter);
        }

        /// <summary>
        /// Simple resampling (as the combination of interpolation and decimation)
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="up">Interpolation factor</param>
        /// <param name="down">Decimation factor</param>
        /// <returns>Resampled signal</returns>
        public static DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down)
        {
            return new Resampler().ResampleUpDown(signal, up, down);
        }

        /// <summary>
        /// Time stretching with parameters set by user
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="stretch">Stretch factor (ratio)</param>
        /// <param name="windowSize">Window size (for vocoders - FFT size)</param>
        /// <param name="hopSize">Hop size</param>
        /// <param name="algorithm">Algorithm for TSM (optional)</param>
        /// <returns>Time stretched signal</returns>
        public static DiscreteSignal TimeStretch(DiscreteSignal signal,
                                                 double stretch,
                                                 int windowSize,
                                                 int hopSize,
                                                 TsmAlgorithm algorithm = TsmAlgorithm.PhaseVocoderPhaseLocking)
        {
            if (Math.Abs(stretch - 1.0) < 1e-10)
            {
                return signal.Copy();
            }

            IFilter stretchFilter;

            switch (algorithm)
            {
                case TsmAlgorithm.PhaseVocoder:
                    stretchFilter = new PhaseVocoder(stretch, hopSize, windowSize);
                    break;
                case TsmAlgorithm.PhaseVocoderPhaseLocking:
                    stretchFilter = new PhaseLockingVocoder(stretch, hopSize, windowSize);
                    break;
                case TsmAlgorithm.PaulStretch:
                    stretchFilter = new PaulStretch(stretch, hopSize, windowSize);
                    break;
                default:
                    stretchFilter = new Wsola(stretch, windowSize, hopSize);
                    break;
            }

            return stretchFilter.ApplyTo(signal, FilteringMethod.Auto);
        }

        /// <summary>
        /// Time stretching with auto-derived parameters
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="stretch">Stretch factor (ratio)</param>
        /// <param name="algorithm">Algorithm for TSM (optional)</param>
        /// <returns>Time stretched signal</returns>
        public static DiscreteSignal TimeStretch(DiscreteSignal signal,
                                                 double stretch,
                                                 TsmAlgorithm algorithm = TsmAlgorithm.PhaseVocoderPhaseLocking)
        {
            if (Math.Abs(stretch - 1.0) < 1e-10)
            {
                return signal.Copy();
            }

            IFilter stretchFilter;

            var frameSize = MathUtils.NextPowerOfTwo(1024 * signal.SamplingRate / 16000);

            switch (algorithm)
            {
                case TsmAlgorithm.PhaseVocoder:
                    stretchFilter = new PhaseVocoder(stretch, frameSize / 10, frameSize);
                    break;
                case TsmAlgorithm.PhaseVocoderPhaseLocking:
                    stretchFilter = new PhaseLockingVocoder(stretch, frameSize / 8, frameSize);
                    break;
                case TsmAlgorithm.PaulStretch:
                    stretchFilter = new PaulStretch(stretch, frameSize / 10, frameSize * 4);
                    break;
                default:
                    stretchFilter = new Wsola(stretch);
                    break;
            }

            return stretchFilter.ApplyTo(signal, FilteringMethod.Auto);
        }

        /// <summary>
        /// Method for extracting the envelope of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="attackTime">Attack time (in seconds)</param>
        /// <param name="releaseTime">Release time (in seconds)</param>
        /// <returns>Signal envelope</returns>
        public static DiscreteSignal Envelope(DiscreteSignal signal, float attackTime = 0.01f, float releaseTime = 0.05f)
        {
            var envelopeFollower = new EnvelopeFollower(signal.SamplingRate, attackTime, releaseTime);

            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => envelopeFollower.Process(s)));
        }

        /// <summary>
        /// Full rectification
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Fully rectified signal</returns>
        public static DiscreteSignal FullRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate,
                                      signal.Samples.Select(s => s < 0 ? -s : s));
        }

        /// <summary>
        /// Half rectification
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Half rectified signal</returns>
        public static DiscreteSignal HalfRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate,
                                      signal.Samples.Select(s => s < 0 ? 0 : s));
        }

        /// <summary>
        /// Spectral subtraction
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="noise">Noise signal</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="hopSize">Hop size (number of samples)</param>
        /// <returns>De-noised signal</returns>
        public static DiscreteSignal SpectralSubtract(DiscreteSignal signal,
                                                      DiscreteSignal noise,
                                                      int fftSize = 1024,
                                                      int hopSize = 410)
        {
            return new SpectralSubtractor(noise, fftSize, hopSize).ApplyTo(signal);
        }

        /// <summary>
        /// Peak normalization
        /// </summary>
        /// <param name="samples">Samples</param>
        /// <param name="peakDb">Peak level in decibels (dbFS), e.g. -1dB, -3dB, etc.</param>
        public static void NormalizePeak(float[] samples, double peakDb)
        {
            var norm = (float)Scale.FromDecibel(peakDb) / samples.Max(x => Math.Abs(x));

            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= norm;
            }
        }

        /// <summary>
        /// Peak normalization
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="peakDb">Peak level in decibels (dBFS), e.g. -1dB, -3dB, etc.</param>
        public static DiscreteSignal NormalizePeak(DiscreteSignal signal, double peakDb)
        {
            var normalized = signal.Copy();
            NormalizePeak(normalized.Samples, peakDb);
            return normalized;
        }

        /// <summary>
        /// Change peak level relatively to input samples (in-place)
        /// </summary>
        /// <param name="samples">Samples</param>
        /// <param name="peakDb">Peak change in decibels, e.g. -6dB - decrease peak level twice</param>
        public static void ChangePeak(float[] samples, double peakDb)
        {
            var norm = (float)Scale.FromDecibel(peakDb);

            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= norm;
            }
        }

        /// <summary>
        /// Change peak level relatively to input signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="peakDb">Peak change in decibels, e.g. -6dB - decrease peak level twice</param>
        public static DiscreteSignal ChangePeak(DiscreteSignal signal, double peakDb)
        {
            var modified = signal.Copy();
            ChangeRms(modified.Samples, peakDb);
            return modified;
        }

        /// <summary>
        /// RMS normalization
        /// </summary>
        /// <param name="samples">Samples</param>
        /// <param name="rmsDb">RMS in decibels (dBFS), e.g. -6dB, -18dB, -26dB, etc.</param>
        public static void NormalizeRms(float[] samples, double rmsDb)
        {
            var sum = 0f;

            for (var i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }

            var norm = (float)Math.Sqrt(samples.Length * Math.Pow(10, rmsDb / 10) / sum);

            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= norm;
            }
        }

        /// <summary>
        /// RMS normalization
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="rmsDb">RMS in decibels (dBFS), e.g. -6dB, -18dB, -26dB, etc.</param>
        public static DiscreteSignal NormalizeRms(DiscreteSignal signal, double rmsDb)
        {
            var normalized = signal.Copy();
            NormalizeRms(normalized.Samples, rmsDb);
            return normalized;
        }

        /// <summary>
        /// Change RMS relatively to input samples
        /// </summary>
        /// <param name="samples">Samples</param>
        /// <param name="rmsDb">RMS change in decibels, e.g. -6dB - decrease RMS twice</param>
        public static void ChangeRms(float[] samples, double rmsDb)
        {
            var sum = 0f;

            for (var i = 0; i < samples.Length; i++)
            {
                sum += samples[i] * samples[i];
            }

            var rmsDbActual = -20 * Math.Log10(Math.Sqrt(sum / samples.Length));

            rmsDb -= rmsDbActual;

            var norm = (float)Math.Sqrt(samples.Length * Math.Pow(10, rmsDb / 10) / sum);
            
            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= norm;
            }
        }

        /// <summary>
        /// Change RMS relatively to input signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="rmsDb">RMS change in decibels, e.g. -6dB - decrease RMS twice</param>
        public static DiscreteSignal ChangeRms(DiscreteSignal signal, double rmsDb)
        {
            var modified = signal.Copy();
            ChangeRms(modified.Samples, rmsDb);
            return modified;
        }

        /// <summary>
        /// Welch periodogram
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="windowSize">Window size (number of samples)</param>
        /// <param name="hopSize">Hop size (number of samples)</param>
        /// <param name="window">Windowing function</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="samplingRate">If sampling rate=0 then power spectrum is evaluated, otherwise power spectral density is evaluated</param>
        /// <returns>Welch periodogram</returns>
        public static float[] Welch(DiscreteSignal signal,
                                    int windowSize = 1024,
                                    int hopSize = 256,
                                    WindowType window = WindowType.Hann,
                                    int fftSize = 0,
                                    int samplingRate = 0)
        {
            var stft = new Stft(windowSize, hopSize, window, fftSize);

            var periodogram = stft.AveragePeriodogram(signal.Samples);

            // scaling is compliant with sciPy function welch():

            float scale;

            if (samplingRate > 0)       // a.k.a. 'density'
            {
                var ws = Window.OfType(window, windowSize).Select(w => w * w).Sum();
                scale = 2 / (ws * samplingRate);
            }
            else                        // a.k.a. 'spectrum'
            {
                var ws = Window.OfType(window, windowSize).Sum();
                scale = 2 / (ws * ws);
            }

            for (var j = 0; j < periodogram.Length; j++)
            {
                periodogram[j] *= scale;
            }

            return periodogram;
        }

        /// <summary>
        /// Lomb-Scargle periodogram
        /// </summary>
        /// <param name="x">Sample times</param>
        /// <param name="y">Signal values at sample times</param>
        /// <param name="freqs">Angular frequencies for output periodogram</param>
        /// <param name="subtractMean">Subtract mean from values before periodogram evaluation</param>
        /// <param name="normalize">Normalize periodogram by the residuals of the data around a constant reference model(at zero)</param>
        /// <returns>Lomb-Scargle periodogram</returns>
        public static float[] LombScargle(float[] x,
                                          float[] y,
                                          float[] freqs,
                                          bool subtractMean = false,
                                          bool normalize = false)
        {
            Guard.AgainstInequality(x.Length, y.Length, "X array size", "Y array size");

            var periodogram = new float[freqs.Length];

            if (subtractMean)
            {
                var mean = y.Average();

                for (var i = 0; i < y.Length; i++)
                {
                    y[i] -= mean;
                }
            }

            var c = new float[x.Length];
            var s = new float[x.Length];

            for (var i = 0; i < freqs.Length; i++)
            {
                float xc = 0, xs = 0, cc = 0, ss = 0, cs = 0;

                for (var j = 0; j < x.Length; j++)
                {
                    c[j] = (float)Math.Cos(freqs[i] * x[j]);
                    s[j] = (float)Math.Sin(freqs[i] * x[j]);

                    xc += y[j] * c[j];
                    xs += y[j] * s[j];
                    cc += c[j] * c[j];
                    ss += s[j] * s[j];
                    cs += c[j] * s[j];

                    var tau = (float)Math.Atan2(2 * cs, cc - ss) / (2 * freqs[i]);
                    var cTau = (float)Math.Cos(freqs[i] * tau);
                    var sTau = (float)Math.Sin(freqs[i] * tau);
                    var cTau2 = cTau * cTau;
                    var sTau2 = sTau * sTau;
                    var csTau = 2 * cTau * sTau;

                    periodogram[i] = 0.5f * (((cTau * xc + sTau * xs) * (cTau * xc + sTau * xs) /
                                              (cTau2 * cc + csTau * cs + sTau2 * ss)) +
                                             ((cTau * xs - sTau * xc) * (cTau * xs - sTau * xc) /
                                              (cTau2 * ss - csTau * cs + sTau2 * cc)));
                }
            }

            if (normalize)
            {
                var norm = 2 / y.Sum(v => v * v);

                for (var i = 0; i < periodogram.Length; i++)
                {
                    periodogram[i] *= norm;
                }
            }

            return periodogram;
        }


#if DEBUG

        /****************************************************************************
         * 
         *    The following methods are included mainly for educational purposes
         * 
         ***************************************************************************/

        /// <summary>
        /// Direct convolution by formula in time domain
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal ConvolveDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var a = signal1.Samples;
            var b = signal2.Samples;
            var length = a.Length + b.Length - 1;

            var conv = new float[length];

            for (var n = 0; n < length; n++)
            {
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k && n - k < a.Length)
                    {
                        conv[n] += a[n - k] * b[k];
                    }
                }
            }

            return new DiscreteSignal(signal1.SamplingRate, conv);
        }

        /// <summary>
        /// Direct cross-correlation by formula in time domain
        /// </summary>
        /// <param name="signal1"></param>
        /// <param name="signal2"></param>
        /// <returns></returns>
        public static DiscreteSignal CrossCorrelateDirect(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            var a = signal1.Samples;
            var b = signal2.Samples;
            var length = a.Length + b.Length - 1;

            var corr = new float[length];

            for (var n = 0; n < length; n++)
            {
                var pos = b.Length - 1;
                for (var k = 0; k < b.Length; k++)
                {
                    if (n >= k && n - k < a.Length)
                    {
                        corr[n] += a[n - k] * b[pos];
                    }
                    pos--;
                }
            }

            return new DiscreteSignal(signal1.SamplingRate, corr);
        }
#endif
    }
}
