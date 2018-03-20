using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
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
        public override string[] FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        public override int FeatureCount => FeatureDescriptions.Length;

        /// <summary>
        /// Extractor functions
        /// </summary>
        private readonly Func<DiscreteSignal, int, int, float>[] _extractors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="featureList"></param>
        /// <param name="parameters"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        public TimeDomainFeaturesExtractor(string featureList,
                                           double frameSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/,
                                           IReadOnlyDictionary<string, object> parameters = null)
            : base(frameSize, hopSize)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':');

            _extractors = features.Select<string, Func<DiscreteSignal, int, int, float>>(f =>
            {
                var parameter = f.Trim().ToLower();
                switch (parameter)
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
                        throw new ArgumentException($"Unknown parameter: {parameter}");
                }
            }).ToArray();

            FeatureDescriptions = features;
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
            var frameSize = (int)(signal.SamplingRate * FrameSize);
            var hopSize = (int)(signal.SamplingRate * HopSize);

            var featureVectors = new List<FeatureVector>();
            var featureCount = FeatureCount;
            
            var i = startSample;
            while (i + frameSize < endSample)
            {
                var featureVector = new float[featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    featureVector[j] = _extractors[j](signal, i, i + frameSize);
                }

                featureVectors.Add(new FeatureVector
                {
                    Features = featureVector,
                    TimePosition = (double)i / signal.SamplingRate
                });

                i += hopSize;
            }

            return featureVectors;
        }
    }
}