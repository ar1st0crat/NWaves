using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;

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
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="melFilterbanks"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        public MfccExtractor(int featureCount, int samplingRate, int melFilterbanks = 20, int fftSize = 512, int hopSize = 256)
        {
            FeatureCount = featureCount;
            _fftSize = fftSize;
            _hopSize = hopSize;
            CreateMelFilterbanks(samplingRate, melFilterbanks, fftSize);
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
            var centerSample = (int)Math.Floor(fftSize * MelToHerz(melFrequency) / samplingRate + 1);
            
            for (var i = 0; i < melFilterbanks; i++)
            {
                melFrequency += frequencyResolution;
                var rightSample = (int)Math.Floor(fftSize * MelToHerz(melFrequency) / samplingRate + 1);

                MelFilterBanks[i] = new double[fftSize / 2 + 1];

                for (var j = leftSample; j <= centerSample; j++)
                {
                    MelFilterBanks[i][j] = (double)(j - leftSample) / (centerSample - leftSample);
                }
                for (var j = centerSample + 1; j < rightSample; j++)
                {
                    MelFilterBanks[i][j] = (double)(rightSample - j) / (centerSample - leftSample);
                }

                leftSample = centerSample;
                centerSample = rightSample;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="spectrum"></param>
        /// <returns></returns>
        private double[] ApplyMelFilterbankLog(double[] spectrum)
        {
            var logMelSpectrum = new double[MelFilterBanks.Length];

            var halfSpectrumSize = _fftSize / 2;

            for (var i = 0; i < MelFilterBanks.Length; i++)
            {
                for (var j = 0; j < halfSpectrumSize; j++)
                {
                    logMelSpectrum[i] += MelFilterBanks[i][j] * spectrum[j];
                }

                logMelSpectrum[i] = Math.Log(logMelSpectrum[i] + double.Epsilon);
            }

            return logMelSpectrum;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            //var preemphasis = new PreEmphasisFilter();
            //var filtered = preemphasis.ApplyTo(signal, FilteringOptions.DifferenceEquation);

            var i = 0;
            while (i + _fftSize < signal.Samples.Length)
            {
                var x = signal[i, i + _fftSize];

                // 1) apply window
                // ...

                // 2)
                var spectrum = Transform.MagnitudeSpectrum(x.Samples, _fftSize);

                // 3)
                var logMelSpectrum = ApplyMelFilterbankLog(spectrum);

                // 4)
                var mfccs = new double[FeatureCount];
                Transform.Dct2(logMelSpectrum, mfccs, FeatureCount);

                // 5) optional liftering
                //Lifter(mfccs);

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
    }
}
