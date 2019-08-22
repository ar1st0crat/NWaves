using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Signals;

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
        /// Extractor functions
        /// </summary>
        protected List<Func<DiscreteSignal, int, int, float>> _extractors;

        /// <summary>
        /// Parameters
        /// </summary>
        protected readonly Dictionary<string, object> _parameters;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Options</param>
        public TimeDomainFeaturesExtractor(MultiFeatureOptions options) : base(options)
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

            _extractors = features.Select<string, Func<DiscreteSignal, int, int, float>>(feature =>
            {
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
                        return (signal, start, end) => 0;
                }
            }).ToList();

            FeatureCount = features.Count;
            FeatureDescriptions = features;
        }

        /// <summary>
        /// Add one more feature with routine for its calculation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="algorithm"></param>
        public void AddFeature(string name, Func<DiscreteSignal, int, int, float> algorithm)
        {
            FeatureCount++;
            FeatureDescriptions.Add(name);
            _extractors.Add(algorithm);
        }

        /// <summary>
        /// Compute the sequence of feature vectors from some fragment of a signal
        /// </summary>
        /// <param name="samples">Signal</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <param name="vectors">Pre-allocated sequence of feature vectors</param>
        public override void ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)
        {
            var ds = new DiscreteSignal(SamplingRate, samples);

            for (int sample = startSample, fv = 0; sample + FrameSize < endSample; sample += HopSize, fv++)
            {
                var featureVector = vectors[fv];

                for (var j = 0; j < featureVector.Length; j++)
                {
                    featureVector[j] = _extractors[j](ds, sample, sample + FrameSize);
                }
            }
        }

        /// <summary>
        /// All logic is implemented in ComputeFrom() method
        /// </summary>
        /// <param name="block"></param>
        /// <param name="features"></param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            throw new NotImplementedException("TimeDomainExtractor does not provide this function. Please call ComputeFrom() method");
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
            var options = new MultiFeatureOptions
            {
                SamplingRate = SamplingRate,
                FrameDuration = FrameDuration,
                HopDuration = HopDuration,
                FeatureList = string.Join(",", FeatureDescriptions),
                Parameters = _parameters
            };

            return new TimeDomainFeaturesExtractor(options) { _extractors = _extractors };
        }
    }
}
