using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Features;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// <para><see cref="Mpeg7SpectralFeaturesExtractor"/> follows MPEG-7 recommendations to evaluate the following features:</para>
    /// <list type="bullet">
    ///     <item>Spectral features (MPEG-7)</item>
    ///     <item>Harmonic features</item>
    ///     <item>Perceptual features</item>
    /// </list>
    /// <para>It's a flexible extractor that allows varying almost everything.</para>
    /// <para>
    /// The difference between <see cref="Mpeg7SpectralFeaturesExtractor"/> and <see cref="SpectralFeaturesExtractor"/> 
    /// is that former calculates spectral features from total energy in frequency BANDS 
    /// while latter analyzes signal energy at particular frequencies (spectral bins).
    /// </para>
    /// </summary>
    public class Mpeg7SpectralFeaturesExtractor : FeatureExtractor
    {
        /// <summary>
        /// Full set of supported spectral features.
        /// </summary>
        public const string FeatureSet = "centroid, spread, flatness, noiseness, rolloff, crest, entropy, decrease, loudness, sharpness";

        /// <summary>
        /// Full set of supported harmonic features.
        /// </summary>
        public const string HarmonicSet = "hcentroid, hspread, inharmonicity, oer, t1+t2+t3";

        /// <summary>
        /// String annotations (or simply names) of features.
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Filterbank from frequency bands.
        /// </summary>
        protected readonly float[][] _filterbank;

        /// <summary>
        /// Internal buffer for frequency bands.
        /// </summary>
        protected readonly (double, double, double)[] _frequencyBands;

        /// <summary>
        /// Internal buffer for central frequencies.
        /// </summary>
        protected readonly float[] _frequencies;

        /// <summary>
        /// Internal buffer for harmonic peak frequencies (optional).
        /// </summary>
        protected float[] _peakFrequencies;

        /// <summary>
        /// Internal buffer for spectral positions of harmonic peaks (optional).
        /// </summary>
        protected int[] _peaks;

        /// <summary>
        /// Extractor functions.
        /// </summary>
        protected List<Func<float[], float[], float>> _extractors;

        /// <summary>
        /// Extractor parameters.
        /// </summary>
        protected readonly Dictionary<string, object> _parameters;

        /// <summary>
        /// Harmonic extractor functions (optional).
        /// </summary>
        protected List<Func<float[], int[], float[], float>> _harmonicExtractors;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Internal buffer for magnitude spectrum.
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for total energies in frequency bands.
        /// </summary>
        protected readonly float[] _mappedSpectrum;

        /// <summary>
        /// Pitch estimator function (optional).
        /// </summary>
        protected Func<float[], float> _pitchEstimator;

        /// <summary>
        /// Array of precomputed pitches (optional).
        /// </summary>
        protected float[] _pitchTrack;

        /// <summary>
        /// Current position in pitch track.
        /// </summary>
        protected int _pitchPos;

        /// <summary>
        /// Harmonic peaks detector function (optional).
        /// </summary>
        protected Action<float[], int[], float[], int, float> _peaksDetector;

        /// <summary>
        /// Construct extractor from configuration options.
        /// </summary>
        /// <param name="options">Extractor configuration options</param>
        public Mpeg7SpectralFeaturesExtractor(MultiFeatureOptions options) : base(options)
        {
            var featureList = options.FeatureList;

            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':')
                                      .Select(f => f.Trim().ToLower())
                                      .ToList();

            _parameters = options.Parameters;

            _extractors = features.Select<string, Func<float[], float[], float>>(feature =>
            {
                switch (feature)
                {
                    case "sc":
                    case "centroid":
                        return Spectral.Centroid;

                    case "ss":
                    case "spread":
                        return Spectral.Spread;

                    case "sfm":
                    case "flatness":
                        if (_parameters?.ContainsKey("minLevel") ?? false)
                        {
                            var minLevel = (float)_parameters["minLevel"];
                            return (spectrum, freqs) => Spectral.Flatness(spectrum, minLevel);
                        }
                        else
                        {
                            return (spectrum, freqs) => Spectral.Flatness(spectrum);
                        }

                    case "sn":
                    case "noiseness":
                        if (_parameters?.ContainsKey("noiseFrequency") ?? false)
                        {
                            var noiseFrequency = (float)_parameters["noiseFrequency"];
                            return (spectrum, freqs) => Spectral.Noiseness(spectrum, freqs, noiseFrequency);
                        }
                        else
                        {
                            return (spectrum, freqs) => Spectral.Noiseness(spectrum, freqs);
                        }

                    case "rolloff":
                        if (_parameters?.ContainsKey("rolloffPercent") ?? false)
                        {
                            var rolloffPercent = (float)_parameters["rolloffPercent"];
                            return (spectrum, freqs) => Spectral.Rolloff(spectrum, freqs, rolloffPercent);
                        }
                        else
                        {
                            return (spectrum, freqs) => Spectral.Rolloff(spectrum, freqs);
                        }

                    case "crest":
                        return (spectrum, freqs) => Spectral.Crest(spectrum);

                    case "entropy":
                    case "ent":
                        return (spectrum, freqs) => Spectral.Entropy(spectrum);

                    case "sd":
                    case "decrease":
                        return (spectrum, freqs) => Spectral.Decrease(spectrum);

                    case "loud":
                    case "loudness":
                        return (spectrum, freqs) => Perceptual.Loudness(spectrum);

                    case "sharp":
                    case "sharpness":
                        return (spectrum, freqs) => Perceptual.Sharpness(spectrum);

                    default:
                        return (spectrum, freqs) => 0;
                }
            }).ToList();

            FeatureCount = features.Count;
            FeatureDescriptions = features;

            _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);
            _fft = new RealFft(_blockSize);

            _frequencyBands = options.FrequencyBands ?? FilterBanks.OctaveBands(6, SamplingRate);
            _filterbank = FilterBanks.Rectangular(_blockSize, SamplingRate, _frequencyBands);

            var cfs = _frequencyBands.Select(b => b.Item2).ToList();
            // insert zero frequency so that it'll be ignored during calculations
            // just like in case of FFT spectrum (0th DC component)
            cfs.Insert(0, 0);
            _frequencies = cfs.ToFloats();

            // reserve memory for reusable blocks

            _spectrum = new float[_blockSize / 2 + 1];              // buffer for magnitude spectrum
            _mappedSpectrum = new float[_filterbank.Length + 1];    // buffer for total energies in bands
        }

        /// <summary>
        /// <para>Add set of harmonic features to extractor's list.</para>
        /// <para>
        /// <paramref name="pitchEstimator"/> is the function that should be used for pitch estimation. 
        /// By default, method <see cref="Pitch.FromSpectralPeaks(float[], int, float, float)"/> is called.
        /// </para>
        /// <para>
        /// <paramref name="peaksDetector"/> is the function that should be used for peak detection. 
        /// By default, method <see cref="Harmonic.Peaks(float[], int[], float[], int, float)"/> is called.
        /// </para>
        /// </summary>
        /// <param name="featureList">String names/annotations of newly added harmonic features</param>
        /// <param name="peakCount">Max number of harmonic peaks</param>
        /// <param name="pitchEstimator">Function that should be used for pitch estimation</param>
        /// <param name="peaksDetector">Function that should be used for peak detection</param>
        /// <param name="lowPitch">Lower frequency of expected pitch range</param>
        /// <param name="highPitch">Upper frequency of expected pitch range</param>
        public void IncludeHarmonicFeatures(string featureList,
                                            int peakCount = 10,
                                            Func<float[], float> pitchEstimator = null,
                                            Action<float[], int[], float[], int, float> peaksDetector = null,
                                            float lowPitch = 80/*Hz*/,
                                            float highPitch = 400/*Hz*/)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = HarmonicSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':')
                                      .Select(f => f.Trim().ToLower())
                                      .ToList();

            _harmonicExtractors = features.Select<string, Func<float[], int[], float[], float>>(feature =>
            {
                switch (feature)
                {
                    case "hc":
                    case "hcentroid":
                        return Harmonic.Centroid;

                    case "hs":
                    case "hspread":
                        return Harmonic.Spread;

                    case "inh":
                    case "inharmonicity":
                        return Harmonic.Inharmonicity;

                    case "oer":
                    case "oddevenratio":
                        return (spectrum, peaks, freqs) => Harmonic.OddToEvenRatio(spectrum, peaks);

                    case "t1":
                    case "t2":
                    case "t3":
                        return (spectrum, peaks, freqs) => Harmonic.Tristimulus(spectrum, peaks, int.Parse(feature.Substring(1)));

                    default:
                        return (spectrum, peaks, freqs) => 0;
                }
            }).ToList();

            FeatureCount += features.Count;
            FeatureDescriptions.AddRange(features);

            if (pitchEstimator is null)
            {
                _pitchEstimator = spectrum => Pitch.FromSpectralPeaks(spectrum, SamplingRate, lowPitch, highPitch);
            }
            else
            {
                _pitchEstimator = pitchEstimator;
            }

            if (peaksDetector is null)
            {
                _peaksDetector = Harmonic.Peaks;
            }
            else
            {
                _peaksDetector = peaksDetector;
            }

            _peaks = new int[peakCount];
            _peakFrequencies = new float[peakCount];
        }

        /// <summary>
        /// Add user-defined harmonic feature to extractor's list (and the routine for its calculation).
        /// </summary>
        /// <param name="name">Feature name/annotation</param>
        /// <param name="algorithm">Routine for calculation of the feature</param>
        public void AddHarmonicFeature(string name, Func<float[], int[], float[], float> algorithm)
        {
            if (_harmonicExtractors is null)
            {
                return;
            }

            FeatureCount++;
            FeatureDescriptions.Add(name);
            _harmonicExtractors.Add(algorithm);
        }

        /// <summary>
        /// Set array of precomputed pitches
        /// </summary>
        /// <param name="pitchTrack">Array of pitches computed elsewhere</param>
        public void SetPitchTrack(float[] pitchTrack)
        {
            _pitchTrack = pitchTrack;
        }

        /// <summary>
        /// <para>Compute MPEG-7 feature vectors from <paramref name="samples"/> and store them in <paramref name="vectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        /// <param name="vectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)
        {
            _pitchPos = 0;

            return base.ComputeFrom(samples, startSample, endSample, vectors);
        }

        /// <summary>
        /// Compute MPEG-7 features in one frame
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // compute and prepare spectrum

            _fft.MagnitudeSpectrum(block, _spectrum);

            // apply filterbank (ignoring 0th coefficient)

            for (var k = 0; k < _filterbank.Length; k++)
            {
                _mappedSpectrum[k + 1] = 0.0f;

                for (var j = 0; j < _spectrum.Length; j++)
                {
                    _mappedSpectrum[k + 1] += _filterbank[k][j] * _spectrum[j];
                }
            }

            // extract spectral features

            for (var j = 0; j < _extractors.Count; j++)
            {
                features[j] = _extractors[j](_mappedSpectrum, _frequencies);
            }

            // ...and maybe harmonic features

            if (_harmonicExtractors != null)
            {
                var pitch = _pitchTrack is null ? _pitchEstimator(_spectrum) : _pitchTrack[_pitchPos++];

                _peaksDetector(_spectrum, _peaks, _peakFrequencies, SamplingRate, pitch);

                var offset = _extractors.Count;
                for (var j = 0; j < _harmonicExtractors.Count; j++)
                {
                    features[j + offset] = _harmonicExtractors[j](_spectrum, _peaks, _peakFrequencies);
                }
            }
        }

        /// <summary>
        /// <para>Does the extractor support parallelization.</para>
        /// <para>
        /// Returns false if array of pitches was set manually. 
        /// Returns true in all other cases.
        /// </para>
        /// </summary>
        public override bool IsParallelizable() => _pitchTrack is null;

        /// <summary>
        /// Thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy()
        {
            var spectralFeatureSet = string.Join(",", FeatureDescriptions.Take(_extractors.Count));
            var options = new MultiFeatureOptions
            {
                SamplingRate = SamplingRate,
                FeatureList = spectralFeatureSet,
                FrameDuration = FrameDuration,
                HopDuration = HopDuration,
                FftSize = _blockSize,
                FrequencyBands = _frequencyBands,
                PreEmphasis = _preEmphasis,
                Window = _window,
                Parameters = _parameters
            };

            var copy = new Mpeg7SpectralFeaturesExtractor(options)
            {
                _extractors = _extractors,
                _pitchTrack = _pitchTrack
            };

            if (_harmonicExtractors != null)
            {
                var harmonicFeatureSet = string.Join(",", FeatureDescriptions.Skip(_extractors.Count));
                copy.IncludeHarmonicFeatures(harmonicFeatureSet, _peaks.Length, _pitchEstimator);
            }

            return copy;
        }
    }
}
