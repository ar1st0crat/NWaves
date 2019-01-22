using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Signals;
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
        private readonly int _featureCount;

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
        private List<string> _featureDescriptions;

        /// <summary>
        /// The "featuregram": the sequence of (feature) vectors;
        /// if this sequence is given, then AmsExtractor computes 
        /// modulation spectral coefficients from sequences in each 'feature channel'.
        /// </summary>
        private readonly float[][] _featuregram;

        /// <summary>
        /// Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        private float[][] _filterbank;
        public float[][] Filterbank => _filterbank;
        
        /// <summary>
        /// Signal envelopes in different frequency bands
        /// </summary>
        private float[][] _envelopes;
        public float[][] Envelopes => _envelopes;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// FFT transformer for modulation spectrum
        /// </summary>
        private readonly Fft _modulationFft;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Size of FFT applied to signal envelopes
        /// </summary>
        private readonly int _modulationFftSize;

        /// <summary>
        /// Hop size for analysis of signal envelopes
        /// </summary>
        private readonly int _modulationHopSize;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

        /// <summary>
        /// Internal buffer for a signal block at each step
        /// </summary>
        private float[] _block;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private float[] _spectrum;

        /// <summary>
        /// Internal buffer for filtered spectrum
        /// </summary>
        private float[] _filteredSpectrum;

        /// <summary>
        /// 
        /// </summary>
        private float[] _modBlock;
            
        /// <summary>
        /// 
        /// </summary>
        private float[] _modSpectrum;

        /// <summary>
        /// Internal buffer of zeros for quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Another internal buffer of zeros for quick memset
        /// </summary>
        private readonly float[] _zeroModblock;


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
                            double preEmphasis = 0.0,
                            WindowTypes window = WindowTypes.Rectangular)

            : base(samplingRate, frameDuration, hopDuration)
        {
            _modulationFftSize = modulationFftSize;
            _modulationHopSize = modulationHopSize;
            _modulationFft = new Fft(_modulationFftSize);

            _featuregram = featuregram?.ToArray();

            if (featuregram != null)
            {
                _featureCount = _featuregram[0].Length * (_modulationFftSize / 2 + 1);
            }
            else
            {
                if (_filterbank == null)
                {
                    _fftSize = _fftSize > FrameSize ? _fftSize : MathUtils.NextPowerOfTwo(FrameSize);

                    _filterbank = FilterBanks.Triangular(_fftSize, samplingRate,
                                     FilterBanks.MelBands(12, _fftSize, samplingRate, 100, 3200));
                }
                else
                {
                    _filterbank = filterbank;
                    _fftSize = 2 * (filterbank[0].Length - 1);
                }

                _fft = new Fft(_fftSize);
                
                _featureCount = _filterbank.Length * (_modulationFftSize / 2 + 1);

                _window = window;
                if (_window != WindowTypes.Rectangular)
                {
                    _windowSamples = Window.OfType(_window, FrameSize);
                }

                _spectrum = new float[_fftSize / 2 + 1];
                _filteredSpectrum = new float[_filterbank.Length];
                _block = new float[_fftSize];
                _zeroblock = new float[_fftSize];
            }

            _preEmphasis = (float) preEmphasis;

            _modBlock = new float[_modulationFftSize];
            _zeroModblock = new float[_modulationFftSize];
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
        /// <param name="signal">Signal under analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of flattened modulation spectra</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            Guard.AgainstInequality(SamplingRate, signal.SamplingRate, "Feature extractor sampling rate", "signal sampling rate");

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
                    _envelopes[n] = new float[signal.Length / hopSize];
                }

                var prevSample = startSample > 0 ? signal[startSample - 1] : 0.0f;

                // ===================== compute local FFTs (do STFT) =======================

                while (i + frameSize < endSample)
                {
                    _zeroblock.FastCopyTo(_block, _zeroblock.Length);
                    signal.Samples.FastCopyTo(_block, frameSize, i);

                    // 0) pre-emphasis (if needed)

                    if (_preEmphasis > 0.0)
                    {
                        for (var k = 0; k < frameSize; k++)
                        {
                            var y = _block[k] - prevSample * _preEmphasis;
                            prevSample = _block[k];
                            _block[k] = y;
                        }
                        prevSample = signal[i + hopSize - 1];
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

                    i += hopSize;
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

                if (avg >= 1e-10)   // this happens more frequently
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
                    _zeroModblock.FastCopyTo(_modBlock, _modulationFftSize);
                    envelope.FastCopyTo(_modBlock, Math.Min(_modulationFftSize, envelopeLength - i), i);

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
        /// True if computations can be done in parallel
        /// </summary>
        /// <returns></returns>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() =>
            new AmsExtractor(SamplingRate,
                             FrameDuration,
                             HopDuration, 
                             _modulationFftSize, 
                             _modulationHopSize, 
                             _fftSize, 
                             _featuregram, 
                             _filterbank, 
                             _preEmphasis, 
                             _window);
    }
}
