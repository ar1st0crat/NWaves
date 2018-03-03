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
        public override string[] FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        public override int FeatureCount => FeatureDescriptions.Length;

        /// <summary>
        /// Length of overlap (in ms)
        /// </summary>
        private readonly double _hopSize;

        /// <summary>
        /// Length of analysis window (in ms)
        /// </summary>
        private readonly double _windowSize;

        /// <summary>
        /// Extractor functions
        /// </summary>
        private readonly Func<int, int, double>[] _extractors;

        /// <summary>
        /// Currently processed signal
        /// </summary>
        private DiscreteSignal _signal;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="featureList"></param>
        /// <param name="parameters"></param>
        /// <param name="windowSize"></param>
        /// <param name="hopSize"></param>
        public TimeDomainFeaturesExtractor(string featureList,
                                           double windowSize = 0.0256, double hopSize = 0.010,
                                           IReadOnlyDictionary<string, object> parameters = null)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':');

            _extractors = features.Select<string, Func<int, int, double>>(f =>
            {
                var parameter = f.Trim().ToLower();
                switch (parameter)
                {
                    case "e":
                    case "en":
                    case "energy":
                        return _signal.Energy;

                    case "rms":
                        return _signal.Rms;

                    case "zcr":
                    case "zero-crossing-rate":
                        return _signal.ZeroCrossingRate;

                    case "entropy":
                        return _signal.Entropy;

                    default:
                        throw new ArgumentException($"Unknown parameter: {parameter}");
                }
            }).ToArray();

            FeatureDescriptions = features;

            _windowSize = windowSize;
            _hopSize = hopSize;
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
            var windowSize = (int)(signal.SamplingRate * _windowSize);
            var hopSize = (int)(signal.SamplingRate * _hopSize);

            _signal = signal;

            var featureVectors = new List<FeatureVector>();
            var featureCount = FeatureCount;
            
            var block = new double[windowSize];     // buffer for currently processed block
            
            var i = startSample;
            while (i + windowSize < endSample)
            {
                FastCopy.ToExistingArray(signal.Samples, block, windowSize, i);

                var featureVector = new double[featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    featureVector[j] = _extractors[j](i, i + windowSize);
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