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
    /// Extractor 
    /// </summary>
    public class FilterbankExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (number of frequency bands)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "fb0", "fb1", "fb2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "fb" + i).ToList();

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].
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
        /// Window samples
        /// </summary>
        private readonly float[] _windowSamples;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly float _preEmphasis;

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
        private readonly Action _getSpectrum;

        /// <summary>
        /// Delegate for post-processing spectrum
        /// </summary>
        private readonly Action _postProcessSpectrum;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a post-processed mel-spectrum at each step
        /// </summary>
        private readonly float[] _bandSpectrum;

        /// <summary>
        /// Internal buffer for a signal block at each step
        /// </summary>
        private readonly float[] _block;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameDuration"></param>
        /// <param name="hopDuration"></param>
        /// <param name="filterbank"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="nonLinearity"></param>
        /// <param name="spectrumType"></param>
        /// <param name="window"></param>
        /// <param name="logFloor"></param>
        public FilterbankExtractor(int samplingRate,
                                   int featureCount,
                                   double frameDuration = 0.0256/*sec*/,
                                   double hopDuration = 0.010/*sec*/,
                                   float[][] filterbank = null,
                                   double preEmphasis = 0,
                                   NonLinearityType nonLinearity = NonLinearityType.None,
                                   SpectrumType spectrumType = SpectrumType.Power,
                                   WindowTypes window = WindowTypes.Hamming,
                                   float logFloor = float.Epsilon) :
            base(samplingRate, frameDuration, hopDuration)
        {
            FeatureCount = featureCount;

            FilterBank = filterbank;

            var fftSize = 2 * (filterbank[0].Length - 1);

            Guard.AgainstNotPowerOfTwo(fftSize, "FFT size");
            Guard.AgainstExceedance(FrameSize, fftSize, "frame size", "FFT size");

            _fft = new RealFft(fftSize);

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
                    _postProcessSpectrum = () => { };
                    break;
            }

            _spectrumType = spectrumType;
            switch (_spectrumType)
            {
                case SpectrumType.Magnitude:
                    _getSpectrum = () => _fft.MagnitudeSpectrum(_block, _spectrum, false);
                    break;
                case SpectrumType.Power:
                    _getSpectrum = () => _fft.PowerSpectrum(_block, _spectrum, false);
                    break;
                case SpectrumType.MagnitudeNormalized:
                    _getSpectrum = () => _fft.MagnitudeSpectrum(_block, _spectrum, true);
                    break;
                case SpectrumType.PowerNormalized:
                    _getSpectrum = () => _fft.PowerSpectrum(_block, _spectrum, true);
                    break;
            }

            // reserve memory for reusable blocks

            _spectrum = new float[fftSize / 2 + 1];
            _bandSpectrum = new float[filterbank.Length];
            _block = new float[fftSize];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="startSample"></param>
        /// <param name="endSample"></param>
        /// <returns></returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var hopSize = HopSize;
            var frameSize = FrameSize;

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;

            var lastSample = endSample - Math.Max(frameSize, hopSize);

            for (var i = startSample; i < lastSample; i += hopSize)
            {
                // prepare next block for processing

                // copy frameSize samples
                samples.FastCopyTo(_block, frameSize, i);
                // fill zeros to fftSize if frameSize < fftSize
                for (var k = frameSize; k < _block.Length; _block[k++] = 0) ;


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

                _block.ApplyWindow(_windowSamples);

                // 2) calculate magnitude/power spectrum (with/without normalization)

                _getSpectrum();         // _block -> _spectrum

                // 3) apply filterbank and take log10/ln/cubic_root of the result

                _postProcessSpectrum(); // _spectrum -> _bandSpectrum

                // add mfcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = _bandSpectrum.FastCopy(),
                    TimePosition = (double)i / SamplingRate
                });
            }

            return featureVectors;
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
                                     FrameDuration,
                                     HopDuration,
                                     FilterBank,
                                    _preEmphasis,
                                    _nonLinearityType,
                                    _spectrumType,
                                    _window,
                                    _logFloor);
    }
}
