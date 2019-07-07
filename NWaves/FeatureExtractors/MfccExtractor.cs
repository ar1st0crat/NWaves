using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Mel Frequency Cepstral Coefficients extractor
    /// </summary>
    public class MfccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (including coeff #0)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "mfcc0", "mfcc1", "mfcc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "mfcc" + i).ToList();

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].
        /// By default it's mel filterbank.
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Number of mel filters
        /// </summary>
        private readonly int _filterbankSize;

        /// <summary>
        /// Lower frequency (Hz)
        /// </summary>
        private readonly double _lowFreq;

        /// <summary>
        /// Upper frequency (Hz)
        /// </summary>
        private readonly double _highFreq;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// FFT transformer
        /// </summary>
        private readonly RealFft _fft;

        /// <summary>
        /// DCT-II transformer
        /// </summary>
        private readonly IDct _dct;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        private readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        private readonly float[] _lifterCoeffs;

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
        /// DCT type ("1", "1N", "2", "2N", "3", "3N", "4", "4N")
        /// </summary>
        private readonly string _dctType;

        /// <summary>
        /// 
        /// </summary>
        private readonly NonLinearityType _nonLinearityType;

        /// <summary>
        /// 
        /// </summary>
        private readonly SpectrumType _spectrumType;

        /// <summary>
        /// 
        /// </summary>
        private readonly bool _includeEnergy;

        /// <summary>
        /// 
        /// </summary>
        private readonly Action _getSpectrum;

        /// <summary>
        /// 
        /// </summary>
        private readonly Action _postProcessSpectrum;

        /// <summary>
        /// 
        /// </summary>
        private readonly Action<float[]> _applyDct;
        
        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a post-processed mel-spectrum at each step
        /// </summary>
        private readonly float[] _melSpectrum;

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
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="fftSize"></param>
        /// <param name="filterbank"></param>
        /// <param name="lifterSize"></param>
        /// <param name="preEmphasis"></param>
        /// <param name="includeEnergy"></param>
        /// <param name="dctType">"1", "1N", "2", "2N", "3", "3N"</param>
        /// <param name="postProcessType"></param>
        /// <param name="spectrumType"></param>
        /// <param name="window"></param>
        public MfccExtractor(int samplingRate,
                             int featureCount,
                             double frameDuration = 0.0256/*sec*/,
                             double hopDuration = 0.010/*sec*/,
                             int filterbankSize = 20,
                             double lowFreq = 0,
                             double highFreq = 0,
                             int fftSize = 0,
                             float[][] filterbank = null,
                             int lifterSize = 22,
                             double preEmphasis = 0.0,
                             bool includeEnergy = false,
                             string dctType = "2",
                             NonLinearityType postProcessType = NonLinearityType.Log10,
                             SpectrumType spectrumType = SpectrumType.PowerNormalized,
                             WindowTypes window = WindowTypes.Hamming)

            : base(samplingRate, frameDuration, hopDuration)
        {
            FeatureCount = featureCount;

            if (filterbank == null)
            {
                _fftSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);
                _filterbankSize = filterbankSize;

                _lowFreq = lowFreq;
                _highFreq = highFreq;

                FilterBank = FilterBanks.Triangular(_fftSize, SamplingRate,
                                    FilterBanks.MelBands(_filterbankSize, _fftSize, SamplingRate, _lowFreq, _highFreq));
            }
            else
            {
                FilterBank = filterbank;
                _filterbankSize = filterbank.Length;
                _fftSize = 2 * (filterbank[0].Length - 1);

                Guard.AgainstInvalidRange(FrameSize, _fftSize, "frame size", "FFT size");
            }

            _fft = new RealFft(_fftSize);
            
            _window = window;

            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _lifterSize = lifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _preEmphasis = (float)preEmphasis;
            _includeEnergy = includeEnergy;

            // setup DCT: ============================================================================

            _dctType = dctType;
            switch (dctType[0])
            {
                case '1':
                    _dct = new Dct1(_filterbankSize);
                    break;
                case '2':
                    _dct = new Dct2(_filterbankSize);
                    break;
                case '3':
                    _dct = new Dct3(_filterbankSize);
                    break;
                case '4':
                    _dct = new Dct4(_filterbankSize);
                    break;
                default:
                    throw new ArgumentException("Only DCT-1, 2, 3 and 4 are supported!");
            }

            if (dctType.Length > 1 && dctType[1] == 'N')
            {
                _applyDct = mfccs => _dct.DirectNorm(_melSpectrum, mfccs);
            }
            else
            {
                _applyDct = mfccs => _dct.Direct(_melSpectrum, mfccs);
            }

            // setup spectrum post-processing: =======================================================

            _nonLinearityType = postProcessType;
            switch (postProcessType)
            {
                case NonLinearityType.Log10:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog10(FilterBank, _spectrum, _melSpectrum);
                    break;
                case NonLinearityType.LogE:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog(FilterBank, _spectrum, _melSpectrum);
                    break;
                case NonLinearityType.ToDecibel:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndToDecibel(FilterBank, _spectrum, _melSpectrum);
                    break;
                case NonLinearityType.CubicRoot:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndCubicRoot(FilterBank, _spectrum, _melSpectrum);
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

            _spectrum = new float[_fftSize / 2 + 1];
            _melSpectrum = new float[_filterbankSize];
            _block = new float[_fftSize];
        }


        /// <summary>
        /// Standard method for computing mfcc features:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum X
        ///     3) Apply mel filters and log() the result: Y = Log10(X * H)
        ///     4) Do dct-II: mfcc = Dct(Y)
        ///     5) [Optional] liftering of mfcc
        /// 
        /// </summary>
        /// <param name="samples">Samples for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of mfcc vectors</returns>
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

                // 3) apply mel filterbank and take log10/ln/cubic_root of the result

                _postProcessSpectrum(); // _spectrum -> _melSpectrum

                // 4) dct

                var mfccs = new float[FeatureCount];

                _applyDct(mfccs);       // _melSpectrum -> mfccs


                // 5) (optional) liftering

                if (_lifterCoeffs != null)
                {
                    mfccs.ApplyWindow(_lifterCoeffs);
                }

                // 6) (optional) replace first coeff with log(energy) 

                if (_includeEnergy)
                {
                    var total = 0f;
                    for (int k = i; k < i + frameSize; total += samples[k] * samples[k], k++) ;
                    mfccs[0] = (float)(Math.Log(total));
                }

                // add mfcc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = mfccs,
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
            new MfccExtractor( SamplingRate, 
                               FeatureCount,
                               FrameDuration, 
                               HopDuration,
                              _filterbankSize, 
                              _lowFreq,
                              _highFreq,
                              _fftSize, 
                               FilterBank, 
                              _lifterSize, 
                              _preEmphasis,
                              _includeEnergy,
                              _dctType,
                              _nonLinearityType,
                              _spectrumType,
                              _window);
    }
}
