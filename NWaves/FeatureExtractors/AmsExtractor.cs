using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Amplitude modulation spectra (AMS) extractor
    /// </summary>
    public class AmsExtractor : FeatureExtractor
    {
        /// <summary>
        /// Total number of coefficients in amplitude modulation spectrum
        /// </summary>
        public override int FeatureCount => _featureCount;
        protected readonly int _featureCount;

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
        public override List<string> FeatureDescriptions => _featureDescriptions;
        protected readonly List<string> _featureDescriptions;

        /// <summary>
        /// The "featuregram": the sequence of (feature) vectors;
        /// if this sequence is given, then AmsExtractor computes 
        /// modulation spectral coefficients from sequences in each 'feature channel'.
        /// </summary>
        protected readonly float[][] _featuregram;

        /// <summary>
        /// Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        protected readonly float[][] _filterbank;
        public float[][] Filterbank => _filterbank;
        
        /// <summary>
        /// Signal envelopes in different frequency bands
        /// </summary>
        protected float[][] _envelopes;
        public float[][] Envelopes => _envelopes;

        /// <summary>
        /// Size of FFT
        /// </summary>
        protected readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// FFT transformer for modulation spectrum
        /// </summary>
        protected readonly RealFft _modulationFft;

        /// <summary>
        /// Size of FFT applied to signal envelopes
        /// </summary>
        protected readonly int _modulationFftSize;

        /// <summary>
        /// Hop size for analysis of signal envelopes
        /// </summary>
        protected readonly int _modulationHopSize;

        /// <summary>
        /// Internal buffer for a signal block at each step
        /// </summary>
        protected readonly float[] _block;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for filtered spectrum
        /// </summary>
        protected readonly float[] _filteredSpectrum;

        /// <summary>
        /// Internal buffer for modulation spectrum analysis
        /// </summary>
        protected readonly float[] _modBlock;
            
        /// <summary>
        /// Modulation spectrum (in one band)
        /// </summary>
        protected readonly float[] _modSpectrum;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration">In seconds</param>
        /// <param name="hopDuration">In seconds</param>
        /// <param name="modulationFftSize">In samples</param>
        /// <param name="modulationHopSize">In samples</param>
        /// <param name="fftSize">In samples</param>
        /// <param name="featuregram"></param>
        /// <param name="filterbank"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public AmsExtractor(int samplingRate,
                            double frameDuration = 0.0256/*sec*/,
                            double hopDuration = 0.010/*sec*/, 
                            int modulationFftSize = 64,
                            int modulationHopSize = 4,
                            int fftSize = 0,
                            IEnumerable<float[]> featuregram = null,
                            float[][] filterbank = null,
                            double preEmphasis = 0,
                            WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration, preEmphasis, window)
        {
            _modulationFftSize = modulationFftSize;
            _modulationHopSize = modulationHopSize;
            _modulationFft = new RealFft(_modulationFftSize);

            _featuregram = featuregram?.ToArray();

            if (featuregram != null)
            {
                _featureCount = _featuregram[0].Length * (_modulationFftSize / 2 + 1);
            }
            else
            {
                if (_filterbank == null)
                {
                    _fftSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);

                    _filterbank = FilterBanks.Triangular(_fftSize, samplingRate,
                                     FilterBanks.MelBands(12, _fftSize, samplingRate, 100, 3200));
                }
                else
                {
                    _filterbank = filterbank;
                    _fftSize = 2 * (filterbank[0].Length - 1);

                    Guard.AgainstExceedance(FrameSize, _fftSize, "frame size", "FFT size");
                }

                _fft = new RealFft(_fftSize);
                
                _featureCount = _filterbank.Length * (_modulationFftSize / 2 + 1);

                _spectrum = new float[_fftSize / 2 + 1];
                _filteredSpectrum = new float[_filterbank.Length];
                _block = new float[_fftSize];
            }

            _modBlock = new float[_modulationFftSize];
            _modSpectrum = new float[_modulationFftSize / 2 + 1];

            // feature descriptions

            int length;
            if (_featuregram != null)
            {
                length = _featuregram[0].Length;
            }
            else
            {
                length = _filterbank.Length;
            }

            _featureDescriptions = new List<string>();

            var modulationSamplingRate = (float)samplingRate / HopSize;
            var resolution = modulationSamplingRate / _modulationFftSize;

            for (var fi = 0; fi < length; fi++)
            {
                for (var fj = 0; fj <= _modulationFftSize / 2; fj++)
                {
                    _featureDescriptions.Add(string.Format("band_{0}_mf_{1:F2}_Hz", fi + 1, fj * resolution));
                }
            }
        }

        /// <summary>
        /// Method for computing modulation spectra.
        /// Each vector representing one modulation spectrum is a flattened version of 2D spectrum.
        /// </summary>
        /// <param name="samples">Samples for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of flattened modulation spectra</returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var frameSize = FrameSize;
            var hopSize = HopSize;

            var featureVectors = new List<FeatureVector>();

            var en = 0;
            var i = startSample;

            if (_featuregram == null)
            {
                _envelopes = new float[_filterbank.Length][];
                for (var n = 0; n < _envelopes.Length; n++)
                {
                    _envelopes[n] = new float[samples.Length / hopSize];
                }

                var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;

                var lastSample = endSample - Math.Max(frameSize, hopSize);

                // ===================== compute local FFTs (do STFT) =======================

                for (i = startSample; i < lastSample; i += hopSize)
                {
                    // copy frameSize samples
                    samples.FastCopyTo(_block, frameSize, i);
                    // fill zeros to fftSize if frameSize < fftSize
                    for (var k = frameSize; k < _block.Length; _block[k++] = 0) ;


                    // 0) pre-emphasis (if needed)

                    if (_preEmphasis > 1e-10)
                    {
                        for (var k = 0; k < frameSize; k++)
                        {
                            var y = _block[k] - prevSample * _preEmphasis;
                            prevSample = _block[k];
                            _block[k] = y;
                        }
                        prevSample = samples[i + hopSize - 1];
                    }
                    
                    // 1) apply window

                    if (_window != WindowTypes.Rectangular)
                    {
                        _block.ApplyWindow(_windowSamples);
                    }

                    // 2) calculate power spectrum

                    _fft.PowerSpectrum(_block, _spectrum);

                    // 3) apply filterbank...

                    FilterBanks.Apply(_filterbank, _spectrum, _filteredSpectrum);

                    // ...and save results for future calculations

                    for (var n = 0; n < _envelopes.Length; n++)
                    {
                        _envelopes[n][en] = _filteredSpectrum[n];
                    }
                    en++;
                }
            }
            else
            {
                en = _featuregram.Length;
                _envelopes = new float[_featuregram[0].Length][];

                for (var n = 0; n < _envelopes.Length; n++)
                {
                    _envelopes[n] = new float[en];
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
                var avg = 0.0f;
                for (var k = 0; k < envelopeLength; k++)
                {
                    avg += (k >= 0) ? envelope[k] : -envelope[k];
                }
                avg /= envelopeLength;

                if (avg >= 1e-10f)   // this happens more frequently
                {
                    for (var k = 0; k < envelopeLength; k++)
                    {
                        envelope[k] /= avg;
                    }
                }
            }

            i = 0;
            while (i < envelopeLength)
            {
                var vector = new float[_envelopes.Length * (_modulationFftSize / 2 + 1)];
                var offset = 0;

                foreach (var envelope in _envelopes)
                {
                    // copy modFftSize samples (or envelopeLength - i in the end)
                    var len = Math.Min(_modulationFftSize, envelopeLength - i);
                    envelope.FastCopyTo(_modBlock, len, i);
                    // fill zeros to modFftSize if len < modFftSize
                    for (var k = len; k < _modBlock.Length; _modBlock[k++] = 0) ;

                    _modulationFft.PowerSpectrum(_modBlock, _modSpectrum);
                    _modSpectrum.FastCopyTo(vector, _modSpectrum.Length, 0, offset);

                    offset += _modSpectrum.Length;
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = vector,
                    TimePosition = (double)i * hopSize / SamplingRate
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
        public float[][] MakeSpectrum2D(FeatureVector featureVector)
        {
            var length = _filterbank?.Length ?? _featuregram[0].Length;

            var spectrum = new float[length][];
            var spectrumSize = _modulationFftSize / 2 + 1;

            var offset = 0;
            for (var i = 0; i < spectrum.Length; i++)
            {
                spectrum[i] = featureVector.Features.FastCopyFragment(spectrumSize, offset);
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
        /// <returns>Short-time spectra corresponding to particular modulation frequency</returns>
        public List<float[]> VectorsAtHerz(IList<FeatureVector> featureVectors, float herz = 4)
        {
            var length = _filterbank?.Length ?? _featuregram[0].Length;
            var modulationSamplingRate = (float) SamplingRate / HopSize;
            var resolution = modulationSamplingRate / _modulationFftSize;
            var freq = (int) Math.Round(herz / resolution);

            var spectrumSize = _modulationFftSize / 2 + 1;
            
            var freqVectors = new List<float[]>();
            foreach (var vector in featureVectors)
            {
                var spectrum = new float[length];
                for (var i = 0; i < spectrum.Length; i++)
                {
                    spectrum[i] = vector.Features[freq + i * spectrumSize];
                }
                freqVectors.Add(spectrum);
            }

            return freqVectors;
        }

        /// <summary>
        /// All logic is fully implemented in ComputeFrom() method
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public override float[] ProcessFrame(float[] block)
        {
            throw new NotImplementedException("AmsExtractor does not provide this function. Please call ComputeFrom() method");
        }
    }
}
