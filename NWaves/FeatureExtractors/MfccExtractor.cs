using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Transforms.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Mel Frequency Cepstral Coefficients extractor
    /// </summary>
    public class MfccExtractor : IFeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (13 by default, including coeff #0)
        /// </summary>
        public int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "coefficient1", "coefficient2", etc.)
        /// </summary>
        public IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "Coefficient mfcc" + i);

        /// <summary>
        /// Mel Filterbanks matrix of dimension MFBANKS * (FFTSIZE/2 + 1)
        /// </summary>
        public double[][] MelFilterBanks { get; private set; }

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of overlap
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Number of liftering coefficients
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Samples of the window
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="melFilterbanks"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MfccExtractor(int featureCount, int samplingRate, int melFilterbanks = 20,
                             int fftSize = 512, int hopSize = 256, int lifterSize = 22,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;
            _fftSize = fftSize;
            _hopSize = hopSize;
            _lifterSize = lifterSize;

            _window = window;
            _windowSamples = Window.OfType(window, _fftSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            CreateMelFilterbanks(samplingRate, melFilterbanks, fftSize);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="herz"></param>
        /// <returns></returns>
        public static double HerzToMel(double herz)
        {
            return 1127.01048 * Math.Log(herz / 700 + 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mel"></param>
        /// <returns></returns>
        public static double MelToHerz(double mel)
        {
            return (Math.Exp(mel / 1127.01048) - 1) * 700;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="melFilterbanks"></param>
        /// <param name="fftSize"></param>
        private void CreateMelFilterbanks(int samplingRate, int melFilterbanks, int fftSize)
        {
            MelFilterBanks = new double[melFilterbanks][];
            
            var frequencyResolution = HerzToMel(samplingRate / 2.0) / (melFilterbanks + 1);

            var leftSample = 0;

            var melFrequency = frequencyResolution;
            var centerSample = (int)Math.Floor((fftSize + 1) * MelToHerz(melFrequency) / samplingRate);
            
            for (var i = 0; i < melFilterbanks; i++)
            {
                melFrequency += frequencyResolution;

                var rightSample = (int)Math.Floor((fftSize + 1) * MelToHerz(melFrequency) / samplingRate);

                MelFilterBanks[i] = new double[fftSize / 2];

                for (var j = leftSample; j < centerSample; j++)
                {
                    MelFilterBanks[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample; j < rightSample; j++)
                {
                    MelFilterBanks[i][j] = (double)(rightSample - j) / (rightSample - centerSample);
                }

                leftSample = centerSample;
                centerSample = rightSample;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrum"></param>
        /// <param name="logMelSpectrum">Output logarithmed mel-spectral array</param>
        private void ApplyMelFilterbankLog(double[] spectrum, double[] logMelSpectrum)
        {
            for (var i = 0; i < MelFilterBanks.Length; i++)
            {
                logMelSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    logMelSpectrum[i] += MelFilterBanks[i][j] * spectrum[j];
                }

                logMelSpectrum[i] = Math.Log10(logMelSpectrum[i] + double.Epsilon);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cepstrum"></param>
        /// <param name="l"></param>
        private static void Lifter(double[] cepstrum, int l = 22)
        {
            if (l <= 0)
            {
                return;
            }

            for (var i = 0; i < cepstrum.Length; i++)
            {
                cepstrum[i] *= (1 + l * Math.Sin(Math.PI * i / l) / 2);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            // prepare everything for dct

            var dct = new Dct();
            dct.Init(MelFilterBanks.Length, FeatureCount);


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;
            
            
            var logMelSpectrum = new double[MelFilterBanks.Length];

            var i = 0;
            while (i + _fftSize < filtered.Samples.Length)
            {
                var x = filtered[i, i + _fftSize].Samples;
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    x.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                var spectrum = Transform.PowerSpectrum(x, _fftSize);


                // 3) apply mel filterbanks and take log() of the result

                ApplyMelFilterbankLog(spectrum, logMelSpectrum);


                // 4) dct-II

                var mfccs = new double[FeatureCount];
                dct.Dct2(logMelSpectrum, mfccs);


                // 5) optional liftering

                Lifter(mfccs, _lifterSize);


                // add mfcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = mfccs,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal, int startPos, int endPos)
        {
            return ComputeFrom(signal[startPos, endPos]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(IEnumerable<double> samples)
        {
            return ComputeFrom(new DiscreteSignal(1, samples));
        }
    }
}
