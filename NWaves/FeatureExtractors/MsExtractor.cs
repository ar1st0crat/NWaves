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
        /// Feature descriptions.
        /// Initialized in constructor in the following manner (example):
        /// 
        ///     band_1_mf_0.5_Hz   band_1_mf_1.0_Hz   ...    band_1_mf_8.0_Hz
        ///     band_2_mf_0.5_Hz   band_2_mf_1.0_Hz   ...    band_2_mf_8.0_Hz
        ///     ...
        ///     band_32_mf_0.5_Hz  band_32_mf_1.0_Hz  ...    band_32_mf_8.0_Hz
        /// 
        /// </summary>
        public override string[] FeatureDescriptions { get; }

        /// <summary>
        /// The "featuregram": the sequence of (feature) vectors;
        /// if this sequence is given, then MsExtractor computes 
        /// modulation spectral coefficients from sequences in each 'feature channel'.
        /// </summary>
        private readonly double[][] _featuregram;

        /// <summary>
        /// Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        private readonly double[][] _filterbank;
        public double[][] Filterbank => _filterbank;
        
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
        /// <param name="modulationOverlapSize">In seconds</param>
        /// <param name="fftSize">In samples</param>
        /// <param name="featuregram"></param>
        /// <param name="filterbank"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MsExtractor(int samplingRate, 
                           double windowSize = 0.0256, double overlapSize = 0.010, 
                           int modulationFftSize = 64, int modulationOverlapSize = 4, int fftSize = 0,
                           IEnumerable<double[]> featuregram = null, double[][] filterbank = null,
                           double preEmphasis = 0.0, WindowTypes window = WindowTypes.Rectangular)
        {
            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = fftSize >= windowLength ? fftSize : MathUtils.NextPowerOfTwo(windowLength);
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
                _filterbank = filterbank ??
                              FilterBanks.Triangular(_fftSize, samplingRate,
                                  FilterBanks.MelBands(12, _fftSize, samplingRate, 100, 3200));
                FeatureCount = _filterbank.Length * (_modulationFftSize / 2 + 1);
            }
            else
            {
                _featuregram = featuregram.ToArray();
                FeatureCount = _featuregram[0].Length * (_modulationFftSize / 2 + 1);
            }

            var length = _filterbank?.Length ?? _featuregram[0].Length;

            var modulationSamplingRate = (double)_samplingRate / _hopSize;
            var resolution = modulationSamplingRate / _modulationFftSize;

            var featureNames = new string[length * (_modulationFftSize / 2 + 1)];
            var idx = 0;
            for (var i = 0; i < length; i++)
            {
                for (var j = 0; j <= _modulationFftSize / 2; j++)
                {
                    featureNames[idx++] = string.Format("band_{0}_mf_{1:F2}_Hz", i + 1, j * resolution);
                }
            }
            FeatureDescriptions = featureNames;
        }

        /// <summary>
        /// Method for computing modulation spectra.
        /// Each vector representing one modulation spectrum is a flattened version of 2D spectrum.
        /// </summary>
        /// <param name="signal">Signal under analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of flattened modulation spectra</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            var featureVectors = new List<FeatureVector>();
            

            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;

            var en = 0;
            var i = startSample;

            if (_featuregram == null)
            {
                _envelopes = new double[_filterbank.Length][];
                for (var n = 0; n < _envelopes.Length; n++)
                {
                    _envelopes[n] = new double[signal.Length / _hopSize];
                }

                // ===================== compute local FFTs (do STFT) =======================

                var spectrum = new double[_fftSize / 2 + 1];
                var filteredSpectrum = new double[_filterbank.Length];

                var block = new double[_fftSize];           // buffer for currently processed signal block at each step
                var zeroblock = new double[_fftSize];       // buffer of zeros for quick memset

                while (i + _windowSamples.Length < endSample)
                {
                    FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length);
                    FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, i);
                    

                    // 1) apply window

                    if (_window != WindowTypes.Rectangular)
                    {
                        block.ApplyWindow(_windowSamples);
                    }

                    // 2) calculate power spectrum

                    _fft.PowerSpectrum(block, spectrum);

                    // 3) apply filterbank...

                    FilterBanks.Apply(_filterbank, spectrum, filteredSpectrum);

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

            // long-term AVG-normalization

            foreach (var envelope in _envelopes)
            {
                var avg = 0.0;
                for (var k = 0; k < envelopeLength; k++)
                {
                    avg += (k >= 0) ? envelope[k] : -envelope[k];
                }
                avg /= envelopeLength;

                if (avg >= 1e-10)   // this happens more frequently
                {
                    for (var k = 0; k < envelopeLength; k++)
                    {
                        envelope[k] /= avg;
                    }
                }
            }

            var modBlock = new double[_modulationFftSize];
            var zeroModblock = new double[_modulationFftSize];
            var modSpectrum = new double[_modulationFftSize / 2 + 1];

            i = 0;
            while (i < envelopeLength)
            {
                var vector = new double[_envelopes.Length * (_modulationFftSize / 2 + 1)];
                var offset = 0;

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
                    TimePosition = (double)i * _hopSize / signal.SamplingRate
                });

                i += _modulationHopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Get 2D modulation spectrum from its flattened version.
        /// Axes are: [short-time-frequency] x [modulation-frequency].
        /// </summary>
        /// <param name="featureVector"></param>
        /// <returns></returns>
        public double[][] MakeSpectrum2D(FeatureVector featureVector)
        {
            var length = _filterbank?.Length ?? _featuregram[0].Length;

            var spectrum = new double[length][];
            var spectrumSize = _modulationFftSize / 2 + 1;

            var offset = 0;
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = FastCopy.ArrayFragment(featureVector.Features, spectrumSize, offset);
                offset += spectrumSize;
            }

            return spectrum;
        }
        
        /// <summary>
        /// Get sequence of short-time spectra corresponding to particular modulation frequency
        /// (by default, the most perceptually important modulation frequency of 4 Hz).
        /// </summary>
        /// <param name="featureVectors"></param>
        /// <param name="herz"></param>
        /// <returns></returns>
        public List<double[]> VectorsAtHerz(IList<FeatureVector> featureVectors, double herz = 4)
        {
            var length = _filterbank?.Length ?? _featuregram[0].Length;

            var modulationSamplingRate = (double) _samplingRate / _hopSize;
            var resolution = modulationSamplingRate / _modulationFftSize;
            var freq = (int)Math.Round(herz / resolution);

            var spectrumSize = _modulationFftSize / 2 + 1;
            
            var freqVectors = new List<double[]>();
            foreach (var vector in featureVectors)
            {
                var spectrum = new double[length];
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = vector.Features[freq + i * spectrumSize];
                }
                freqVectors.Add(spectrum);
            }

            return freqVectors;
        }
    }
}
