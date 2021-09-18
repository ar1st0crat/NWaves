using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Chroma features extractor.
    /// </summary>
    public class ChromaExtractor : FeatureExtractor
    {
        /// <summary>
        /// <para>Feature names</para>
        /// <para>"C", "C#", "D", "D#", etc. if chroma count == 12 and baseC == true; </para>
        /// <para>"A", "A#", "B", "C",  etc. if chroma count == 12 and baseC == false; </para>
        /// <para>"chroma1", "chroma2", etc. otherwise.</para>
        /// </summary>
        public override List<string> FeatureDescriptions
        {
            get
            {
                return FeatureCount == 12
                    ? _options.BaseC
                        ? new[] { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" }.ToList()
                        : new[] { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }.ToList()
                    : Enumerable.Range(1, FeatureCount).Select(i => "chroma" + i).ToList();
            }
        }

        /// <summary>
        /// Filterbank matrix of dimension [ChromaCount * (blockSize/2 + 1)].
        /// </summary>
        protected readonly float[][] _filterBank;

        /// <summary>
        /// Filterbank matrix of dimension [ChromaCount * (blockSize/2 + 1)].
        /// </summary>
        public float[][] FilterBank => _filterBank;

        /// <summary>
        /// FFT transformer.
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step.
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Chroma extractor options.
        /// </summary>
        protected readonly ChromaOptions _options;

        /// <summary>
        /// Construct extractor from configuration options.
        /// </summary>
        /// <param name="options">Extractor configuration options</param>
        public ChromaExtractor(ChromaOptions options) : base(options)
        {
            _options = options;
            _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

            FeatureCount = options.FeatureCount;

            _filterBank = FilterBanks.Chroma(_blockSize,
                                             SamplingRate,
                                             FeatureCount,
                                             options.Tuning,
                                             options.CenterOctave,
                                             options.OctaveWidth,
                                             options.Norm,
                                             options.BaseC);

            _fft = new RealFft(_blockSize);
            _spectrum = new float[_blockSize / 2 + 1];
        }

        /// <summary>
        /// Compute chroma feature vector in one frame.
        /// </summary>
        /// <param name="block">Block of data</param>
        /// <param name="features">Features (one chroma feature vector) computed in the block</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            _fft.PowerSpectrum(block, _spectrum, false);

            FilterBanks.Apply(_filterBank, _spectrum, features);
        }

        /// <summary>
        /// Does the extractor support parallelization. Returns true always.
        /// </summary>
        public override bool IsParallelizable() => true;

        /// <summary>
        /// Thread-safe copy of the extractor for parallel computations.
        /// </summary>
        public override FeatureExtractor ParallelCopy() => new ChromaExtractor(_options);
    }
}
