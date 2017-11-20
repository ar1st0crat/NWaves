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
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mf" + i);

        /// <summary>
        /// Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        private readonly double[][] _filterBank;

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
        /// Size of overlap
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Size of FFT applied to signal envelopes
        /// </summary>
        private readonly int _modulationFftSize;

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
        /// <param name="filterbank"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public MsExtractor(int samplingRate, 
                           double windowSize = 0.0256, double overlapSize = 0.010,
                           int modulationFftSize = 64, int modulationOverlapSize = 4,
                           double[][] filterbank = null, double preEmphasis = 0.0,
                           WindowTypes window = WindowTypes.Rectangular)
        {
            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = MathUtils.NextPowerOfTwo(windowLength);
            _hopSize = (int)(samplingRate * overlapSize);

            _modulationFftSize = modulationFftSize;
            _modulationHopSize = modulationOverlapSize;
            _samplingRate = samplingRate;
            
            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            _filterBank = filterbank ?? FilterBanks.Mel(18, _fftSize, samplingRate, 100, 4200);

            FeatureCount = _filterBank.Length;
        }

        /// <summary>
        /// Method for computing modulation spectra.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();

            _envelopes = new double[_filterBank.Length][];
            for (var n = 0; n < _envelopes.Length; n++)
            {
                _envelopes[n] = new double[signal.Length / _hopSize];
            }

            var filteredSpectrum = new double[_filterBank.Length];
            
            
            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;


            // ===================== compute local FFTs (do STFT) =======================

            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize - _windowSamples.Length];

            var en = 0;
            var i = 0;
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

                var spectrum = Transform.PowerSpectrum(block, _fftSize);
                
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

            // =========================== modulation analysis =======================

            var envelopeLength = en;

            // log-term avg. normalization

            foreach (var envelope in _envelopes)
            {
                var avg = 0.0;
                for (var k = 0; k < envelopeLength; k++)
                {
                    avg += envelope[k];
                }
                avg /= envelopeLength;
                for (var k = 0; k < envelopeLength; k++)
                {
                    envelope[k] /= avg;
                }
            }

            var vector = new double[_envelopes.Length * (_modulationFftSize / 2 + 1)];
            var offset = 0;

            i = 0;
            while (i + _modulationFftSize < envelopeLength)
            {
                offset = 0;
                foreach (var envelope in _envelopes)
                {
                    var x = FastCopy.ArrayFragment(envelope, _modulationFftSize, i);

                    var spectrum = Transform.PowerSpectrum(x, _modulationFftSize);
                    FastCopy.ToExistingArray(spectrum, vector, spectrum.Length, 0, offset);

                    offset += spectrum.Length;
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = vector,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += _modulationHopSize;
            }

            // process last portion of data (that needs to be zero-padded):

            offset = 0;
            foreach (var envelope in _envelopes)
            {
                var x = new double[_modulationFftSize];
                FastCopy.ToExistingArray(envelope, x, envelopeLength - i, i);

                var spectrum = Transform.PowerSpectrum(x, _modulationFftSize);
                FastCopy.ToExistingArray(spectrum, vector, spectrum.Length, 0, offset);

                offset += spectrum.Length;
            }

            featureVectors.Add(new FeatureVector
            {
                Features = vector,
                TimePosition = (double)i / signal.SamplingRate
            });

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
            var spectrum = new double[_filterBank.Length][];
            var specSize = _modulationFftSize / 2 + 1;

            var offset = 0;
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = FastCopy.ArrayFragment(featureVectors.ElementAt(idx).Features, specSize, offset);
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
