using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Signals;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// Represents extractor of time-domain features (energy, rms, ZCR, entropy).
    /// </summary>
    public class TimeDomainFeaturesExtractor : FeatureExtractor
    {
        /// <summary>
        /// Full set of features.
        /// </summary>
        public const string FeatureSet = "energy, rms, zcr, entropy";

        /// <summary>
        /// Gets string annotations (or simply names) of features.
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Extractor functions.
        /// </summary>
        protected List<Func<DiscreteSignal, int, int, float>> _extractors;

        /// <summary>
        /// Parameters.
        /// </summary>
        protected readonly Dictionary<string, object> _parameters;

        /// <summary>
        /// Constructs extractor from configuration <paramref name="options"/>.
        /// </summary>
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
        /// Adds user-defined feature to extractor's list (and the routine for its calculation).
        /// </summary>
        /// <param name="name">Feature name/annotation</param>
        /// <param name="algorithm">Routine for calculation of the feature</param>
        public void AddFeature(string name, Func<DiscreteSignal, int, int, float> algorithm)
        {
            FeatureCount++;
            FeatureDescriptions.Add(name);
            _extractors.Add(algorithm);
        }

        /// <summary>
        /// <para>Computes feature vectors from <paramref name="samples"/> and stores them in <paramref name="vectors"/>.</para>
        /// <para>Returns the number of actually computed feature vectors.</para>
        /// </summary>
        /// <param name="samples">Array of samples</param>
        /// <param name="startSample">Index of the first sample in array for processing</param>
        /// <param name="endSample">Index of the last sample in array for processing</param>
        /// <param name="vectors">Pre-allocated sequence for storing the resulting feature vectors</param>
        public override int ComputeFrom(float[] samples, int startSample, int endSample, IList<float[]> vectors)
        {
            var ds = new DiscreteSignal(SamplingRate, samples);

            var fv = 0;

            for (var sample = startSample; sample + FrameSize < endSample; sample += HopSize, fv++)
            {
                var featureVector = vectors[fv];

                for (var j = 0; j < featureVector.Length; j++)
                {
                    featureVector[j] = _extractors[j](ds, sample, sample + FrameSize);
                }
            }

            return fv;
        }

        /// <summary>
        /// <para>Processes one frame in block of data at each step.</para>
        /// <para><see cref="TimeDomainFeaturesExtractor"/> does not provide this function.</para>
        /// <para>Call <see cref="ComputeFrom(float[], int, int, IList{float[]})"/> method instead.</para>
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            throw new NotImplementedException("TimeDomainFeaturesExtractor does not provide this function. Please call ComputeFrom() method");
        }

        /// <summary>
        /// Returns true, since <see cref="TimeDomainFeaturesExtractor"/> always supports parallelization.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Creates thread-safe copy of the extractor for parallel computations.
        /// </summary>
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
