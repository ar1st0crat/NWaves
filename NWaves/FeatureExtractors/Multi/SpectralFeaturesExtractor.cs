using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Features;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// <para>Represents extractor of spectral features.</para>
    /// <para>
    /// It's a flexible extractor that allows setting frequencies of interest. 
    /// At least one spectral feature must be specified.
    /// </para>
    /// <para>
    /// Check FeatureSet to see the full list of supported features.
    /// </para>
    /// </summary>
    public class SpectralFeaturesExtractor : FeatureExtractor
    {
        /// <summary>
        /// Full set of supported spectral features.
        /// </summary>
        public const string FeatureSet = "centroid, spread, flatness, noiseness, rolloff, crest, entropy, decrease, c1+c2+c3+c4+c5+c6";

        /// <summary>
        /// Gets string annotations (or simply names) of features.
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Extractor functions.
        /// </summary>
        protected List<Func<float[], float[], float>> _extractors;

        /// <summary>
        /// Extractor parameters.
        /// </summary>
        protected readonly Dictionary<string, object> _parameters;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Center frequencies (uniform in Herz scale by default; could be uniform in mel-scale or octave-scale, for example).
        /// </summary>
        protected readonly float[] _frequencies;

        /// <summary>
        /// Internal buffer for magnitude spectrum.
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for magnitude spectrum taken only at frequencies of interest.
        /// </summary>
        protected float[] _mappedSpectrum;

        /// <summary>
        /// Internal buffer for spectral positions of frequencies of interest.
        /// </summary>
        protected readonly int[] _frequencyPositions;

        /// <summary>
        /// Constructs extractor from configuration <paramref name="options"/>.
        /// </summary>
        public SpectralFeaturesExtractor(MultiFeatureOptions options) : base(options)
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

                    case "c1":
                    case "c2":
                    case "c3":
                    case "c4":
                    case "c5":
                    case "c6":
                        return (spectrum, freqs) => Spectral.Contrast(spectrum, freqs, int.Parse(feature.Substring(1)));

                    default:
                        return (spectrum, freqs) => 0;
                }
            }).ToList();

            FeatureCount = features.Count;
            FeatureDescriptions = features;
            
            _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);
            _fft = new RealFft(_blockSize);

            var frequencies = options.Frequencies;
            var resolution = (float)SamplingRate / _blockSize;

            if (frequencies is null)
            {
                _frequencies = Enumerable.Range(0, _blockSize / 2 + 1)
                                         .Select(f => f * resolution)
                                         .ToArray();
            }
            else if (frequencies.Length == _blockSize / 2 + 1)
            {
                _frequencies = frequencies;
            }
            else
            {
                _frequencies = new float[frequencies.Length + 1];
                frequencies.FastCopyTo(_frequencies, frequencies.Length, 0, 1);
                _mappedSpectrum = new float[_frequencies.Length];
                _frequencyPositions = new int[_frequencies.Length];

                for (var i = 1; i < _frequencies.Length; i++)
                {
                    _frequencyPositions[i] = (int)(_frequencies[i] / resolution) + 1;
                }
            }

            _spectrum = new float[_blockSize / 2 + 1];  // buffer for magnitude spectrum
        }

        /// <summary>
        /// Adds user-defined spectral feature to extractor's list (and the routine for its calculation).
        /// </summary>
        /// <param name="name">Feature name/annotation</param>
        /// <param name="algorithm">Routine for calculation of the feature</param>
        public void AddFeature(string name, Func<float[], float[], float> algorithm)
        {
            FeatureCount++;
            FeatureDescriptions.Insert(_extractors.Count, name);
            _extractors.Add(algorithm);
        }

        /// <summary>
        /// Computes spectral features in one frame.
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // compute and prepare spectrum

            _fft.MagnitudeSpectrum(block, _spectrum);

            if (_spectrum.Length == _frequencies.Length)
            {
                _mappedSpectrum = _spectrum;
            }
            else
            {
                for (var j = 0; j < _mappedSpectrum.Length; j++)
                {
                    _mappedSpectrum[j] = _spectrum[_frequencyPositions[j]];
                }
            }

            // extract spectral features

            for (var j = 0; j < _extractors.Count; j++)
            {
                features[j] = _extractors[j](_mappedSpectrum, _frequencies);
            }
        }

        /// <summary>
        /// Returns true, since <see cref="SpectralFeaturesExtractor"/> always supports parallelization.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Creates thread-safe copy of the extractor for parallel computations.
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
                Frequencies = _frequencies,
                PreEmphasis = _preEmphasis,
                Window = _window,
                Parameters = _parameters
            };

            return new SpectralFeaturesExtractor(options) { _extractors = _extractors };
        }
    }
}
