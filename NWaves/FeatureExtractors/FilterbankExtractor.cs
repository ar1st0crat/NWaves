using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// This extractor computes in each frame
    /// spectral energies in frequency bands defined by a given filterbank (channel outputs).
    /// 
    /// So it's like MFCC but without DCT-compressing of the filterbank-mapped spectrum.
    /// 
    /// </summary>
    public class FilterbankExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (number of filters in the filter bank)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "fb0", "fb1", "fb2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "fb" + i).ToList();

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (_blockSize/2 + 1)].
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Window samples (weights)
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Non-linearity type (logE, log10, decibel, cubic root)
        /// </summary>
        private readonly NonLinearityType _nonLinearityType;

        /// <summary>
        /// Spectrum calculation scheme (power/magnitude normalized/not normalized)
        /// </summary>
        private readonly SpectrumType _spectrumType;

        /// <summary>
        /// Floor value for LOG calculations
        /// </summary>
        private readonly float _logFloor;

        /// <summary>
        /// Delegate for calculating spectrum
        /// </summary>
        private readonly Action<float[]> _getSpectrum;

        /// <summary>
        /// Delegate for post-processing spectrum
        /// </summary>
        private readonly Action _postProcessSpectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a post-processed band spectrum at each step
        /// </summary>
        private readonly float[] _bandSpectrum;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="filterbank"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="nonLinearity"></param>
        /// <param name="spectrumType"></param>
        /// <param name="window"></param>
        /// <param name="logFloor"></param>
        public FilterbankExtractor(int samplingRate,
                                   int featureCount,
                                   float[][] filterbank,
                                   double frameDuration = 0.0256/*sec*/,
                                   double hopDuration = 0.010/*sec*/,
                                   double preEmphasis = 0,
                                   NonLinearityType nonLinearity = NonLinearityType.None,
                                   SpectrumType spectrumType = SpectrumType.Power,
                                   WindowTypes window = WindowTypes.Hamming,
                                   float logFloor = float.Epsilon)
            
            : base(samplingRate, frameDuration, hopDuration, preEmphasis)
        {
            FeatureCount = featureCount;

            FilterBank = filterbank;

            _blockSize = 2 * (filterbank[0].Length - 1);

            Guard.AgainstNotPowerOfTwo(_blockSize, "FFT size");
            Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");

            _fft = new RealFft(_blockSize);

            _window = window;
            _windowSamples = Window.OfType(_window, FrameSize);

            _preEmphasis = (float)preEmphasis;

            // setup spectrum post-processing: =======================================================

            _logFloor = logFloor;
            _nonLinearityType = nonLinearity;
            switch (nonLinearity)
            {
                case NonLinearityType.Log10:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog10(FilterBank, _spectrum, _bandSpectrum, _logFloor);
                    break;
                case NonLinearityType.LogE:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog(FilterBank, _spectrum, _bandSpectrum, _logFloor);
                    break;
                case NonLinearityType.ToDecibel:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndToDecibel(FilterBank, _spectrum, _bandSpectrum, _logFloor);
                    break;
                case NonLinearityType.CubicRoot:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndPow(FilterBank, _spectrum, _bandSpectrum, 0.33);
                    break;
                default:
                    _postProcessSpectrum = () => FilterBanks.Apply(FilterBank, _spectrum, _bandSpectrum);
                    break;
            }

            _spectrumType = spectrumType;
            switch (_spectrumType)
            {
                case SpectrumType.Magnitude:
                    _getSpectrum = block => _fft.MagnitudeSpectrum(block, _spectrum, false);
                    break;
                case SpectrumType.Power:
                    _getSpectrum = block => _fft.PowerSpectrum(block, _spectrum, false);
                    break;
                case SpectrumType.MagnitudeNormalized:
                    _getSpectrum = block => _fft.MagnitudeSpectrum(block, _spectrum, true);
                    break;
                case SpectrumType.PowerNormalized:
                    _getSpectrum = block => _fft.PowerSpectrum(block, _spectrum, true);
                    break;
            }

            // reserve memory for reusable blocks

            _spectrum = new float[_blockSize / 2 + 1];
            _bandSpectrum = new float[filterbank.Length];
        }

        /// <summary>
        /// Compute sequence of filter bank channel outputs
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <returns></returns>
        public override float[] ProcessFrame(float[] block)
        {
            // fill zeros to _blockSize if frameSize < fftSize

            for (var k = FrameSize; k < block.Length; block[k++] = 0) ;

            // 1) apply window

            block.ApplyWindow(_windowSamples);

            // 2) calculate magnitude/power spectrum (with/without normalization)

            _getSpectrum(block);        // _block -> _spectrum

            // 3) apply filterbank and take log10/ln/cubic_root of the result

            _postProcessSpectrum();     // _spectrum -> _bandSpectrum

            // add vector to output sequence

            return _bandSpectrum.FastCopy();
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
        public override FeatureExtractor ParallelCopy() =>
            new FilterbankExtractor( SamplingRate,
                                     FeatureCount,
                                     FilterBank,
                                     FrameDuration,
                                     HopDuration,
                                    _preEmphasis,
                                    _nonLinearityType,
                                    _spectrumType,
                                    _window,
                                    _logFloor);
    }
}
