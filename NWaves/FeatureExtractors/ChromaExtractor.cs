using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors
{
    public class ChromaExtractor : FeatureExtractor
    {
        /// <summary>
        /// Descriptions
        /// ("C", "C#", "D", "D#", etc. if chroma count == 12; "chroma1", "chroma2", etc. otherwise)
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
        /// Filterbank matrix of dimension [ChromaCount * (_blockSize/2 + 1)].
        /// </summary>
        protected readonly float[][] _filterBank;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Chroma extractor options
        /// </summary>
        protected readonly ChromaOptions _options;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="options"></param>
        public ChromaExtractor(ChromaOptions options) : base(options)
        {
            _options = options;
            _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

            _filterBank = FilterBanks.Chroma(_blockSize, SamplingRate, FeatureCount, options.Tuning, options.CenterOctave, options.OctaveWidth, options.Norm, options.BaseC);

            _fft = new RealFft(_blockSize);
            _spectrum = new float[_blockSize / 2 + 1];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <param name="features"></param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            _fft.PowerSpectrum(block, _spectrum, false);

            FilterBanks.Apply(_filterBank, _spectrum, features);
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
        public override FeatureExtractor ParallelCopy() => new ChromaExtractor(_options);
    }
}
