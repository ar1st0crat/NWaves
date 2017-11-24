using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
using NWaves.Filters.Fda;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Modulation spectra extractor
    /// </summary>
    public class MsExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients in modulation spectrum
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "mf0", "mf1", etc.)
        /// </summary>
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mf" + i).ToArray();

        /// <summary>
        /// Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        private readonly double[][] _filterBank;

        /// <summary>
        /// The "featuregram": the sequence of (feature) vectors;
        /// if this sequence is given, then MsExtractor computes 
        /// modulation spectral coefficients from sequences in each 'feature channel'.
        /// </summary>
        private readonly double[][] _featuregram;

        /// <summary>
        /// Signal envelopes in different frequency bands
        /// </summary>
        private double[][] _envelopes;
        public double[][] Envelopes => _envelopes;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Size of overlap
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT applied to signal envelopes
        /// </summary>
        private readonly int _modulationFftSize;

        /// <summary>
        /// Internal FFT transformer for modulation FFT analysis
        /// </summary>
        private readonly Fft _modulationFft;

        /// <summary>
        /// Size of overlap during the analysis of signal envelopes
        /// </summary>
        private readonly int _modulationHopSize;

        /// <summary>
        /// Sampling rate
        /// </summary>
        private readonly int _samplingRate;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Samples of the weighting window
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="windowSize">In seconds</param>
        /// <param name="overlapSize">In seconds</param>
        /// <param name="modulationFftSize">In samples</param>
        /// <param name="modulationOverlapSize">In samples</param>
        /// <param name="featuregram"></param>
        /// <param name="filterbank"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MsExtractor(int samplingRate, 
                           double windowSize = 0.0256, double overlapSize = 0.010,
                           int modulationFftSize = 64, int modulationOverlapSize = 4,
                           IEnumerable<double[]> featuregram = null, double[][] filterbank = null,
                           double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = MathUtils.NextPowerOfTwo(windowLength);
            _hopSize = (int)(samplingRate * overlapSize);
            _fft = new Fft(_fftSize);

            _modulationFftSize = modulationFftSize;
            _modulationHopSize = modulationOverlapSize;
            _modulationFft = new Fft(_modulationFftSize);

            _samplingRate = samplingRate;
            
            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            if (featuregram == null)
            {
                _filterBank = filterbank ?? FilterBanks.Mel(18, _fftSize, samplingRate, 100, 4200);
                FeatureCount = _filterBank.Length;
            }
            else
            {
                _featuregram = featuregram.ToArray();
                FeatureCount = _featuregram[0].Length;
            }
        }

        /// <summary>
        /// Method for computing modulation spectra.
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns></returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            var featureVectors = new List<FeatureVector>();

            var spectrum = new double[_fftSize / 2 + 1];
            
            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var en = 0;
            var i = 0;

            if (_featuregram == null)
            {
                _envelopes = new double[_filterBank.Length][];
                for (var n = 0; n < _envelopes.Length; n++)
                {
                    _envelopes[n] = new double[signal.Length / _hopSize];
                }

                // ===================== compute local FFTs (do STFT) =======================

                var filteredSpectrum = new double[_filterBank.Length];

                var block = new double[_fftSize];
                var zeroblock = new double[_fftSize - _windowSamples.Length];

                while (i + _fftSize < filtered.Length)
                {
                    FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, i);
                    FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, _windowSamples.Length);

                    // 1) apply window

                    if (_window != WindowTypes.Rectangular)
                    {
                        block.ApplyWindow(_windowSamples);
                    }

                    // 2) calculate power spectrum

                    _fft.PowerSpectrum(block, spectrum);

                    // 3) apply filterbank...

                    ApplyFilterbank(spectrum, filteredSpectrum);

                    // ...and save results for future calculations

                    for (var n = 0; n < _envelopes.Length; n++)
                    {
                        _envelopes[n][en] = filteredSpectrum[n];
                    }
                    en++;

                    i += _hopSize;
                }
            }
            else
            {
                en = _featuregram.Length;
                _envelopes = new double[_featuregram[0].Length][];

                for (var n = 0; n < _envelopes.Length; n++)
                {
                    _envelopes[n] = new double[en];
                    for (i = 0; i < en; i++)
                    {
                        _envelopes[n][i] = _featuregram[i][n];
                    }
                }
            }

            // =========================== modulation analysis =======================

            var envelopeLength = en;

            // long-term avg. normalization

            foreach (var envelope in _envelopes)
            {
                var avg = 0.0;
                for (var k = 0; k < envelopeLength; k++)
                {
                    avg += (k >= 0) ? envelope[k] : -envelope[k];
                }
                avg /= envelopeLength;
                for (var k = 0; k < envelopeLength; k++)
                {
                    envelope[k] /= avg;
                }
            }

            var modBlock = new double[_modulationFftSize];
            var zeroModblock = new double[_modulationFftSize];
            var modSpectrum = new double[_modulationFftSize / 2 + 1];

            var vector = new double[_envelopes.Length * (_modulationFftSize / 2 + 1)];
            var offset = 0;

            i = 0;
            while (i < envelopeLength)
            {
                offset = 0;
                foreach (var envelope in _envelopes)
                {
                    FastCopy.ToExistingArray(zeroModblock, modBlock, _modulationFftSize);
                    FastCopy.ToExistingArray(envelope, modBlock, Math.Min(_modulationFftSize, envelopeLength - i), i);

                    _modulationFft.PowerSpectrum(modBlock, modSpectrum);
                    FastCopy.ToExistingArray(modSpectrum, vector, modSpectrum.Length, 0, offset);

                    offset += modSpectrum.Length;
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = vector,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _modulationHopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Method applies filters to spectrum.
        /// </summary>
        /// <param name="spectrum">Original spectrum</param>
        /// <param name="filteredSpectrum">Output filtered spectrum</param>
        private void ApplyFilterbank(double[] spectrum, double[] filteredSpectrum)
        {
            for (var i = 0; i < _filterBank.Length; i++)
            {
                filteredSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    filteredSpectrum[i] += _filterBank[i][j] * spectrum[j];
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureVectors"></param>
        /// <param name="idx"></param>
        /// <returns></returns>
        public double[][] MakeSpectrum2D(IEnumerable<FeatureVector> featureVectors, int idx = 0)
        {
            var length = _filterBank?.Length ?? _featuregram[0].Length;

            var spectrum = new double[length][];
            var specSize = _modulationFftSize / 2 + 1;

            var fv = featureVectors.ToArray();

            var offset = 0;
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = FastCopy.ArrayFragment(fv[idx].Features, specSize, offset);
                offset += specSize;
            }

            return spectrum;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureVectors"></param>
        /// <param name="herz"></param>
        /// <returns></returns>
        public List<FeatureVector> VectorsAtHerz(List<FeatureVector> featureVectors, int herz = 4)
        {
            var resolution = (double)_samplingRate / _hopSize / _modulationHopSize;
            var freqAtHz = (int)(herz / resolution);

            var specSize = _modulationFftSize / 2 + 1;

            var freqVectors = new List<FeatureVector>();
            foreach (var vector in featureVectors)
            {
                var spectrum = new double[_filterBank.Length];
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = vector.Features[freqAtHz + i * specSize];
                }
                freqVectors.Add(new FeatureVector
                {
                    Features = spectrum,
                    TimePosition = vector.TimePosition
                });
            }

            return freqVectors;
        }
    }
}
