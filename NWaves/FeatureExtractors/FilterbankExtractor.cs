using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
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
        protected readonly RealFft _fft;

        /// <summary>
        /// Non-linearity type (logE, log10, decibel, cubic root)
        /// </summary>
        protected readonly NonLinearityType _nonLinearityType;

        /// <summary>
        /// Spectrum calculation scheme (power/magnitude normalized/not normalized)
        /// </summary>
        protected readonly SpectrumType _spectrumType;

        /// <summary>
        /// Floor value for LOG calculations
        /// </summary>
        protected readonly float _logFloor;

        /// <summary>
        /// Delegate for calculating spectrum
        /// </summary>
        protected readonly Action<float[]> _getSpectrum;

        /// <summary>
        /// Delegate for post-processing spectrum
        /// </summary>
        protected readonly Action _postProcessSpectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a post-processed band spectrum at each step
        /// </summary>
        protected readonly float[] _bandSpectrum;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">Filterbank options</param>
        public FilterbankExtractor(FilterbankOptions options) : base(options)
        {
            FilterBank = options.FilterBank;
            FeatureCount = FilterBank.Length;

            _blockSize = 2 * (FilterBank[0].Length - 1);

            Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");

            _fft = new RealFft(_blockSize);

            // setup spectrum post-processing: =======================================================

            _logFloor = options.LogFloor;
            _nonLinearityType = options.NonLinearity;
            switch (_nonLinearityType)
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

            _spectrumType = options.SpectrumType;
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
            _bandSpectrum = new float[FilterBank.Length];
        }

        /// <summary>
        /// Compute sequence of filter bank channel outputs
        /// </summary>
        /// <param name="block"></param>
        /// <param name="features"></param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // 1) calculate magnitude/power spectrum (with/without normalization)

            _getSpectrum(block);        // _block -> _spectrum

            // 2) apply filterbank and take log10/ln/cubic_root of the result

            _postProcessSpectrum();     // _spectrum -> _bandSpectrum

            // fill output feature vector

            _bandSpectrum.FastCopyTo(features, FeatureCount);
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
            new FilterbankExtractor(
                new FilterbankOptions
                {
                    SamplingRate = SamplingRate,
                    FilterBank = FilterBank,
                    FrameDuration = FrameDuration,
                    HopDuration = HopDuration,
                    PreEmphasis = _preEmphasis,
                    NonLinearity = _nonLinearityType,
                    SpectrumType = _spectrumType,
                    Window = _window,
                    LogFloor = _logFloor
                });
    }
}
