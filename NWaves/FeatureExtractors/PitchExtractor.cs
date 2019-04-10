using System;
using System.Collections.Generic;
using NWaves.FeatureExtractors.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Pitch extractor calls autocorrelation method since it's best in terms of universality and quality.
    /// The feature vector contains 1 component : pitch.
    /// 
    /// If there's a need to create pitch extractor based on other time-domain method (YIN or ZcrSchmitt),
    /// then TimeDomainFeatureExtractor can be used.
    /// 
    /// If there's a need to create pitch extractor based on a certain spectral method (HSS or HPS),
    /// then SpectralDomainFeatureExtractor can be used.
    /// 
    /// Example:
    /// 
    /// var extractor = new TimeDomainFeaturesExtractor(sr, "en", 0.0256, 0.010);
    /// 
    /// extractor.AddFeature("yin", (s, start, end) => { return Pitch.FromYin(s, start, end); });
    /// 
    /// var pitches = extractor.ComputeFrom(signal);
    /// 
    /// </summary>
    public class PitchExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of features (currently 1 pitch value estimated by autocorrelation)
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
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

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
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="parameters"></param>
        public PitchExtractor(int samplingRate,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              float low = 80,
                              float high = 400,
                              double preEmphasis = 0,
                              WindowTypes window = WindowTypes.Rectangular)
            : base(samplingRate, frameDuration, hopDuration)
        {
            _low = low;
            _high = high;

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _preEmphasis = (float)preEmphasis;

            var fftSize = MathUtils.NextPowerOfTwo(2 * FrameSize - 1);
            _convolver = new Convolver(fftSize);

            _block = new float[FrameSize];
            _reversed = new float[FrameSize];
            _cc = new float[fftSize];

            FeatureDescriptions = new List<string>() { "pitch" };
        }

        /// <summary>
        /// Pitch tracking
        /// </summary>
        /// <param name="samples"></param>
        /// <returns></returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var samplingRate = SamplingRate;
            var frameSize = FrameSize;
            var hopSize = HopSize;

            var pitches = new List<FeatureVector>();

            var pitch1 = (int)(samplingRate / _high);    // 2,5 ms = 400Hz
            var pitch2 = (int)(samplingRate / _low);     // 12,5 ms = 80Hz

            var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;

            var lastSample = endSample - Math.Max(frameSize, hopSize);

            for (var i = startSample; i < lastSample; i += hopSize)
            {
                // prepare all blocks in memory for the current step:

                samples.FastCopyTo(_block, frameSize, i);
                
                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 1e-10)
                {
                    for (var k = 0; k < frameSize; k++)
                    {
                        var y = _block[k] - prevSample * _preEmphasis;
                        prevSample = _block[k];
                        _block[k] = y;
                    }
                    prevSample = samples[i + hopSize - 1];
                }

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    _block.ApplyWindow(_windowSamples);
                }

                _block.FastCopyTo(_reversed, frameSize);

                // 2) autocorrelation

                _convolver.CrossCorrelate(_block, _reversed, _cc);

                // 3) argmax of autocorrelation

                var start = pitch1 + FrameSize - 1;
                var end = Math.Min(start + pitch2, _cc.Length);

                var max = start < _cc.Length ? _cc[start] : 0;

                var peakIndex = start;
                for (var k = start; k < end; k++)
                {
                    if (_cc[k] > max)
                    {
                        max = _cc[k];
                        peakIndex = k - FrameSize + 1;
                    }
                }

                var f0 = max > 1.0f ? (float)samplingRate / peakIndex : 0;

                pitches.Add(new FeatureVector
                {
                    Features = new float[] { f0 },
                    TimePosition = (double)i / SamplingRate
                });
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
