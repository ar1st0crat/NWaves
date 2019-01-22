using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// Extractor of time-domain features
    /// </summary>
    public class TimeDomainFeaturesExtractor : FeatureExtractor
    {
        public const string FeatureSet = "energy, rms, zcr, entropy";

        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        public override int FeatureCount => FeatureDescriptions.Count;

        /// <summary>
        /// Extractor functions
        /// </summary>
        private List<Func<DiscreteSignal, int, int, float>> _extractors;

        /// <summary>
        /// Parameters
        /// </summary>
        private IReadOnlyDictionary<string, object> _parameters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureList"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="parameters"></param>
        public TimeDomainFeaturesExtractor(int samplingRate,
                                           string featureList,
                                           double frameDuration = 0.0256/*sec*/,
                                           double hopDuration = 0.010/*sec*/,
                                           IReadOnlyDictionary<string, object> parameters = null)

            : base(samplingRate, frameDuration, hopDuration)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':');

            _extractors = features.Select<string, Func<DiscreteSignal, int, int, float>>(f =>
            {
                var feature = f.Trim().ToLower();
                switch (feature)
                {
                    case "e":
                    case "en":
                    case "energy":
                        return (signal, start, end) => signal.Energy(start, end);

                    case "rms":
                        return (signal, start, end) => signal.Rms(start, end);

                    case "zcr":
                    case "zero-crossing-rate":
                        return (signal, start, end) => signal.ZeroCrossingRate(start, end);

                    case "entropy":
                        return (signal, start, end) => signal.Entropy(start, end);

                    default:
                        return null;
                }
            }).ToList();

            FeatureDescriptions = features.ToList();
        }

        /// <summary>
        /// Add one more feature with routine for its calculation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="algorithm"></param>
        public void AddFeature(string name, Func<DiscreteSignal, int, int, float> algorithm)
        {
            FeatureDescriptions.Add(name);
            _extractors.Add(algorithm);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>Sequence of feature vectors</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            var nullExtractorPos = _extractors.IndexOf(null);
            if (nullExtractorPos >= 0)
            {
                throw new ArgumentException($"Unknown feature: {FeatureDescriptions[nullExtractorPos]}");
            }

            Guard.AgainstInequality(SamplingRate, signal.SamplingRate, "Feature extractor sampling rate", "signal sampling rate");

            var featureVectors = new List<FeatureVector>();
            var featureCount = FeatureCount;
            
            var i = startSample;
            while (i + FrameSize < endSample)
            {
                var featureVector = new float[featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    featureVector[j] = _extractors[j](signal, i, i + FrameSize);
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = featureVector,
                    TimePosition = (double) i / SamplingRate
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
            var copy = new TimeDomainFeaturesExtractor(SamplingRate, featureset, FrameDuration, HopDuration, _parameters)
            {
                _extractors = _extractors,
            };
            return copy;
        }
    }
}
