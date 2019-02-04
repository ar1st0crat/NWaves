using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Features;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// Extractor of spectral features
    /// </summary>
    public class SpectralFeaturesExtractor : FeatureExtractor
    {
        public const string FeatureSet = "centroid, spread, flatness, rolloff, crest, entropy, decrease, c1+c2+c3+c4+c5+c6";

        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        public override int FeatureCount => FeatureDescriptions.Count;

        /// <summary>
        /// Size of used FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Center frequencies (uniform in Herz scale by default; could be uniform in mel-scale or octave-scale, for example)
        /// </summary>
        private readonly float[] _frequencies;

        /// <summary>
        /// Parameters
        /// </summary>
        private readonly IReadOnlyDictionary<string, object> _parameters;

        /// <summary>
        /// Internal buffer for magnitude spectrum
        /// </summary>
        float[] _spectrum;

        /// <summary>
        /// Internal buffer for currently processed block
        /// </summary>
        float[] _block;

        /// <summary>
        /// Internal block of zeros for a quick memset
        /// </summary>
        float[] _zeroblock;

        /// <summary>
        /// Extractor functions
        /// </summary>
        private List<Func<float[], float[], float>> _extractors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureList"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="fftSize"></param>
        /// <param name="parameters"></param>
        public SpectralFeaturesExtractor(int samplingRate,
                                         string featureList,
                                         double frameDuration = 0.0256/*sec*/,
                                         double hopDuration = 0.010/*sec*/,
                                         int fftSize = 0,
                                         float[] frequencies = null,
                                         IReadOnlyDictionary<string, object> parameters = null)

            : base(samplingRate, frameDuration, hopDuration)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':');

            _extractors = features.Select<string, Func<float[], float[], float>>(f =>
            {
                var feature = f.Trim().ToLower();
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
                        if (parameters?.ContainsKey("minLevel") ?? false)
                        {
                            var minLevel = (float) parameters["minLevel"];
                            return (spectrum, freqs) => Spectral.Flatness(spectrum, freqs, minLevel);
                        }
                        else
                        {
                            return (spectrum, freqs) => Spectral.Flatness(spectrum, freqs);
                        }

                    case "rolloff":
                        if (parameters?.ContainsKey("rolloffPercent") ?? false)
                        {
                            var rolloffPercent = (float) parameters["rolloffPercent"];
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

            FeatureDescriptions = features.ToList();

            _fftSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);
            _fft = new Fft(_fftSize);

            var resolution = (float) samplingRate / _fftSize;

            _frequencies = frequencies ?? Enumerable.Range(0, _fftSize + 1)
                                                    .Select(f => f * resolution)
                                                    .ToArray();
            _parameters = parameters;

            // reserve memory for reusable blocks

            _spectrum = new float[_fftSize / 2 + 1];  // buffer for magnitude spectrum
            _block = new float[_fftSize];             // buffer for currently processed block
            _zeroblock = new float[_fftSize];         // just a buffer of zeros for quick memset
        }

        /// <summary>
        /// Add one more feature with routine for its calculation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="algorithm"></param>
        public void AddFeature(string name, Func<float[], float[], float> algorithm)
        {
            FeatureDescriptions.Add(name);
            _extractors.Add(algorithm);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="samples">Signal</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>Sequence of feature vectors</returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var nullExtractorPos = _extractors.IndexOf(null);
            if (nullExtractorPos >= 0)
            {
                throw new ArgumentException($"Unknown feature: {FeatureDescriptions[nullExtractorPos]}");
            }
            
            var featureVectors = new List<FeatureVector>();
            var featureCount = FeatureCount;
            
            var i = startSample;
            while (i + FrameSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                _zeroblock.FastCopyTo(_block, _fftSize);
                samples.FastCopyTo(_block, FrameSize, i);

                _fft.MagnitudeSpectrum(_block, _spectrum);

                // =======
                // _spectrum -> reduced  _spectrum !
                // =======

                var featureVector = new float[featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    featureVector[j] = _extractors[j](_spectrum, _frequencies);
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = featureVector,
                    TimePosition = (double)i / SamplingRate
                });

                i += HopSize;
            }

            return featureVectors;
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
        public override FeatureExtractor ParallelCopy()
        {
            var featureset = string.Join(",", FeatureDescriptions);
            var copy = new SpectralFeaturesExtractor(SamplingRate, featureset, FrameDuration, HopDuration, _fftSize, _frequencies, _parameters)
            {
                _extractors = _extractors,
            };
            return copy;
        }
    }
}
