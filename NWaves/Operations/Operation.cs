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
    /// Provides methods for various DSP/audio operations: 
    /// <list type="bullet">
    ///     <item>convolution</item>
    ///     <item>cross-correlation</item>
    ///     <item>block convolution</item>
    ///     <item>deconvolution</item>
    ///     <item>resampling</item>
    ///     <item>time-stretching</item>
    ///     <item>rectification</item>
    ///     <item>envelope detection</item>
    ///     <item>spectral subtraction</item>
    ///     <item>normalization (peak / RMS)</item>
    ///     <item>periodogram (Welch / Lomb-Scargle)</item>
    /// </list>
    /// </summary>
    public static class Operation
    {
        /// <summary>
        /// Does fast convolution of <paramref name="signal"/> with <paramref name="kernel"/> via FFT.
        /// </summary>
        public static DiscreteSignal Convolve(DiscreteSignal signal, DiscreteSignal kernel)
        {
            return new Convolver().Convolve(signal, kernel);
        }

        /// <summary>
        /// Does fast convolution of <paramref name="signal"/> with <paramref name="kernel"/> via FFT.
        /// </summary>
        public static ComplexDiscreteSignal Convolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)
        {
            return new ComplexConvolver().Convolve(signal, kernel);
        }

        /// <summary>
        /// Does fast convolution of <paramref name="signal"/> with <paramref name="kernel"/> via FFT.
        /// </summary>
        public static double[] Convolve(double[] signal, double[] kernel)
        {
            return Convolve(new ComplexDiscreteSignal(1, signal), 
                            new ComplexDiscreteSignal(1, kernel)).Real;
        }

        /// <summary>
        /// Does fast cross-correlation between <paramref name="signal1"/> and <paramref name="signal2"/> via FFT.
        /// </summary>
        public static DiscreteSignal CrossCorrelate(DiscreteSignal signal1, DiscreteSignal signal2)
        {
            return new Convolver().CrossCorrelate(signal1, signal2);
        }

        /// <summary>
        /// Does fast cross-correlation between <paramref name="signal1"/> and <paramref name="signal2"/> via FFT.
        /// </summary>
        public static ComplexDiscreteSignal CrossCorrelate(ComplexDiscreteSignal signal1, ComplexDiscreteSignal signal2)
        {
            return new ComplexConvolver().CrossCorrelate(signal1, signal2);
        }

        /// <summary>
        /// Does block convolution of <paramref name="signal"/> with <paramref name="kernel"/> 
        /// (using either Overlap-Add or Overlap-Save algorithm).
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Convolution kernel</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="method">Block convolution method (OverlapAdd / OverlapSave)</param>
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
        /// Deconvolves <paramref name="signal"/> and <paramref name="kernel"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="kernel">Kernel</param>
        public static ComplexDiscreteSignal Deconvolve(ComplexDiscreteSignal signal, ComplexDiscreteSignal kernel)
        {
            return new ComplexConvolver().Deconvolve(signal, kernel);
        }

        /// <summary>
        /// Does interpolation of <paramref name="signal"/> followed by lowpass filtering.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Interpolation factor (e.g. factor=2 if 8000 Hz -> 16000 Hz)</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        public static DiscreteSignal Interpolate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            return new Resampler().Interpolate(signal, factor, filter);
        }

        /// <summary>
        /// Does decimation of <paramref name="signal"/> preceded by lowpass filtering.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="factor">Decimation factor (e.g. factor=2 if 16000 Hz -> 8000 Hz)</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        public static DiscreteSignal Decimate(DiscreteSignal signal, int factor, FirFilter filter = null)
        {
            return new Resampler().Decimate(signal, factor, filter);
        }

        /// <summary>
        /// Does band-limited resampling of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="newSamplingRate">Desired sampling rate</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        /// <param name="order">Order</param>
        public static DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate, FirFilter filter = null, int order = 15)
        {
            return new Resampler().Resample(signal, newSamplingRate, filter, order);
        }

        /// <summary>
        /// Does simple resampling of <paramref name="signal"/> (as the combination of interpolation and decimation).
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="up">Interpolation factor</param>
        /// <param name="down">Decimation factor</param>
        /// <param name="filter">Lowpass anti-aliasing filter</param>
        public static DiscreteSignal ResampleUpDown(DiscreteSignal signal, int up, int down, FirFilter filter = null)
        {
            return new Resampler().ResampleUpDown(signal, up, down, filter);
        }

        /// <summary>
        /// Does time stretching of <paramref name="signal"/> with parameters set by user.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="stretch">Stretch factor (ratio)</param>
        /// <param name="windowSize">Window size (for vocoders - FFT size)</param>
        /// <param name="hopSize">Hop length</param>
        /// <param name="algorithm">Algorithm for TSM</param>
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
        /// Does time stretching of <paramref name="signal"/> with auto-derived parameters.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="stretch">Stretch factor (ratio)</param>
        /// <param name="algorithm">Algorithm for TSM</param>
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
        /// Extracts the envelope of <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="attackTime">Attack time (in seconds)</param>
        /// <param name="releaseTime">Release time (in seconds)</param>
        public static DiscreteSignal Envelope(DiscreteSignal signal, float attackTime = 0.01f, float releaseTime = 0.05f)
        {
            var envelopeFollower = new EnvelopeFollower(signal.SamplingRate, attackTime, releaseTime);

            return new DiscreteSignal(signal.SamplingRate, signal.Samples.Select(s => envelopeFollower.Process(s)));
        }

        /// <summary>
        /// Full-rectifies <paramref name="signal"/>.
        /// </summary>
        public static DiscreteSignal FullRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate,
                                      signal.Samples.Select(s => s < 0 ? -s : s));
        }

        /// <summary>
        /// Half-rectifies <paramref name="signal"/>.
        /// </summary>
        public static DiscreteSignal HalfRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate,
                                      signal.Samples.Select(s => s < 0 ? 0 : s));
        }

        /// <summary>
        /// De-noises <paramref name="signal"/> using spectral subtraction. 
        /// Subtracts <paramref name="noise"/> from <paramref name="signal"/>.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="noise">Noise signal</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="hopSize">Hop size (number of samples)</param>
        public static DiscreteSignal SpectralSubtract(DiscreteSignal signal,
                                                      DiscreteSignal noise,
                                                      int fftSize = 1024,
                                                      int hopSize = 256)
        {
            return new SpectralSubtractor(noise, fftSize, hopSize).ApplyTo(signal);
        }

        /// <summary>
        /// Normalizes peak level.
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
        /// Normalizes peak level.
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
        /// Changes peak level relatively to input <paramref name="samples"/> (in-place).
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
        /// Changes peak level relatively to input <paramref name="signal"/>.
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
        /// Normalizes RMS.
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
        /// Normalizes RMS.
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
        /// Changes RMS relatively to input <paramref name="samples"/>.
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
        /// Changes RMS relatively to input <paramref name="signal"/>.
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
        /// Computes periodogram using Welch's method. 
        /// If <paramref name="samplingRate"/>=0 then power spectrum is evaluated, otherwise power spectral density is evaluated. 
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="windowSize">Window size (number of samples)</param>
        /// <param name="hopSize">Hop size (number of samples)</param>
        /// <param name="window">Windowing function</param>
        /// <param name="fftSize">FFT size</param>
        /// <param name="samplingRate">If sampling rate=0 then power spectrum is evaluated, otherwise power spectral density is evaluated</param>
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
        /// Computes the Lomb-Scargle periodogram.
        /// </summary>
        /// <param name="x">Sample times</param>
        /// <param name="y">Signal values at sample times</param>
        /// <param name="freqs">Angular frequencies for output periodogram</param>
        /// <param name="subtractMean">Subtract mean from values before periodogram evaluation</param>
        /// <param name="normalize">Normalize periodogram by the residuals of the data around a constant reference model(at zero)</param>
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
