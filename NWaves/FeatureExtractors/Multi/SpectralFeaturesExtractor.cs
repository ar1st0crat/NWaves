using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Features;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;

namespace NWaves.FeatureExtractors.Multi
{
    /// <summary>
    /// Extractor of spectral features
    /// </summary>
    public class SpectralFeaturesExtractor : FeatureExtractor
    {
        public const string FeatureSet = "centroid, spread, flatness, rolloff, crest, bandwidth, c1+c2+c3+c4+c5+c6";

        /// <summary>
        /// String annotations (or simply names) of features
        /// </summary>
        public override string[] FeatureDescriptions { get; }

        /// <summary>
        /// Number of features to extract
        /// </summary>
        public override int FeatureCount => FeatureDescriptions.Length;

        /// <summary>
        /// Size of used FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Extractor functions
        /// </summary>
        private readonly Func<float[], float[], float>[] _extractors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="featureList"></param>
        /// <param name="parameters"></param>
        /// <param name="frameSize"></param>
        /// <param name="hopSize"></param>
        /// <param name="fftSize"></param>
        public SpectralFeaturesExtractor(string featureList,
                                         double frameSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/, int fftSize = 0,
                                         IReadOnlyDictionary<string, object> parameters = null)
            : base(frameSize, hopSize)
        {
            if (featureList == "all" || featureList == "full")
            {
                featureList = FeatureSet;
            }

            var features = featureList.Split(',', '+', '-', ';', ':');

            _extractors = features.Select<string, Func<float[], float[], float>>(f =>
            {
                var parameter = f.Trim().ToLower();
                switch (parameter)
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

                    case "sbw":
                    case "bandwidth":
                        return (spectrum, freqs) => Spectral.Bandwidth(spectrum, freqs);

                    case "c1":
                    case "c2":
                    case "c3":
                    case "c4":
                    case "c5":
                    case "c6":
                        return (spectrum, freqs) => Spectral.Contrast(spectrum, freqs, int.Parse(parameter.Substring(1)));

                    default:
                        throw new ArgumentException($"Unknown parameter: {parameter}");
                }
            }).ToArray();

            FeatureDescriptions = features;

            _fftSize = fftSize;
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
            var fftSize = _fftSize >= frameSize ? _fftSize : MathUtils.NextPowerOfTwo(frameSize);

            var resolution = (float)signal.SamplingRate / fftSize;

            var frequencies = Enumerable.Range(0, fftSize + 1)
                                        .Select(f => f * resolution)
                                        .ToArray();

            var featureVectors = new List<FeatureVector>();
            var featureCount = FeatureCount;

            var fft = new Fft(fftSize);

            // reserve memory for reusable blocks

            var spectrum = new float[fftSize / 2 + 1];  // buffer for magnitude spectrum
            var block = new float[fftSize];             // buffer for currently processed block
            var zeroblock = new float[fftSize];         // just a buffer of zeros for quick memset

            var i = startSample;
            while (i + frameSize < endSample)
            {
                // prepare all blocks in memory for the current step:

                zeroblock.FastCopyTo(block, fftSize);
                signal.Samples.FastCopyTo(block, frameSize, i);

                fft.MagnitudeSpectrum(block, spectrum);

                var featureVector = new float[featureCount];

                for (var j = 0; j < featureCount; j++)
                {
                    featureVector[j] = _extractors[j](spectrum, frequencies);
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
