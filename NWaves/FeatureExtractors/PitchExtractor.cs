using System.Collections.Generic;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Features
{
    /// <summary>
    /// Pitch extractor can combine several algorithms for pitch estimation.
    /// By default YIN is used, hence the feature vector contains 1 component.
    /// </summary>
    public class PitchExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of pitch algorithms (1 by default, YIN, AutoCorrelation)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Names of pitch algorithms
        /// </summary>
        public override List<string> FeatureDescriptions { get; }

        /// <summary>
        /// Lower pitch frequency
        /// </summary>
        private readonly float _low;

        /// <summary>
        /// Upper pitch frequency
        /// </summary>
        private readonly float _high;

        /// <summary>
        /// Internal convolver
        /// </summary>
        private Convolver _convolver;

        /// <summary>
        /// Internal buffer for real parts of the currently processed block
        /// </summary>
        private float[] _block;

        /// <summary>
        /// Internal buffer for reversed real parts of the currently processed block
        /// </summary>
        private float[] _reversed;

        /// <summary>
        /// Internal buffer for cross-correlation signal
        /// </summary>
        private float[] _cc;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="parameters"></param>
        public PitchExtractor(int samplingRate,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              float low = 80,
                              float high = 400) 
            : base(samplingRate, frameDuration, hopDuration)
        {
            _low = low;
            _high = high;

            var fftSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(fftSize);

            _block = new float[FrameSize];    // buffer for the currently processed block
            _reversed = new float[FrameSize]; // buffer for the currently processed block
            _cc = new float[fftSize];         // buffer for cross-correlation signal

            FeatureDescriptions = new List<string>() { "autocorr" };
        }

        /// <summary>
        /// Pitch tracking
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            var samplingRate = signal.SamplingRate;
            var frameSize = FrameSize;

            var pitches = new List<FeatureVector>();

            var pitch1 = (int)(samplingRate / _high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(samplingRate / _low);     // 12,5 ms = 80Hz

            var i = startSample;
            while (i + frameSize < endSample)
            {
                signal.Samples.FastCopyTo(_block, frameSize, i);
                signal.Samples.FastCopyTo(_reversed, frameSize, i);

                _convolver.CrossCorrelate(_block, _reversed, _cc);

                var startPos = pitch1 + frameSize - 1;

                var max = _cc[startPos];
                var peakIndex = startPos;
                for (var k = startPos + 1; k <= pitch2 + startPos; k++)
                {
                    if (_cc[k] > max)
                    {
                        max = _cc[k];
                        peakIndex = k;
                    }
                }

                peakIndex -= (frameSize - 1);

                pitches.Add(new FeatureVector
                {
                    Features = new float[] { (float)samplingRate / peakIndex },
                    TimePosition = (double)i / SamplingRate
                });

                i += HopSize;
            }

            return pitches;
        }

        public override bool IsParallelizable() => true;

        /// <summary>
        /// Copy of current extractor that can work in parallel
        /// </summary>
        /// <returns></returns>
        public override FeatureExtractor ParallelCopy() => 
            new PitchExtractor(SamplingRate, FrameDuration, HopDuration, _low, _high);
    }
}
