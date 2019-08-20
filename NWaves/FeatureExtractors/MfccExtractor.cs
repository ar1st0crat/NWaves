using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.FeatureExtractors.Base;
using NWaves.FeatureExtractors.Options;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Mel Frequency Cepstral Coefficients extractor.
    /// 
    /// Since so many variations of MFCC have been developed since 1980,
    /// this class is very general and allows customizing pretty everything:
    /// 
    ///  - filterbank (by default it's MFCC-FB24 HTK/Kaldi-style)
    ///  
    ///  - non-linearity type (logE, log10, decibel (librosa power_to_db analog), cubic root)
    ///  
    ///  - spectrum calculation type (power/magnitude normalized/not normalized)
    ///  
    ///  - DCT type (1,2,3,4 normalized or not): "1", "1N", "2", "2N", etc.
    ///  
    ///  - floor value for LOG-calculations (usually it's float.Epsilon; HTK default seems to be 1.0 and in librosa 1e-10 is used)
    /// 
    /// </summary>
    public class MfccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Descriptions (simply "mfcc0", "mfcc1", "mfcc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions
        {
            get
            {
                var names = Enumerable.Range(0, FeatureCount).Select(i => "mfcc" + i).ToList();
                if (_includeEnergy) names[0] = "log_En";
                return names;
            }
        }

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].
        /// By default it's mel filterbank.
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Lower frequency (Hz)
        /// </summary>
        protected readonly double _lowFreq;

        /// <summary>
        /// Upper frequency (Hz)
        /// </summary>
        protected readonly double _highFreq;

        /// <summary>
        /// Size of liftering window
        /// </summary>
        protected readonly int _lifterSize;

        /// <summary>
        /// Liftering window coefficients
        /// </summary>
        protected readonly float[] _lifterCoeffs;

        /// <summary>
        /// FFT transformer
        /// </summary>
        protected readonly RealFft _fft;

        /// <summary>
        /// DCT-II transformer
        /// </summary>
        protected readonly IDct _dct;

        /// <summary>
        /// DCT type ("1", "1N", "2", "2N", "3", "3N", "4", "4N")
        /// </summary>
        protected readonly string _dctType;

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
        /// Should the first MFCC coefficient be replaced with LOG(energy)
        /// </summary>
        protected readonly bool _includeEnergy;

        /// <summary>
        /// Floor value for LOG-energy calculation
        /// </summary>
        protected readonly float _logEnergyFloor;

        /// <summary>
        /// Delegate for calculating spectrum
        /// </summary>
        protected readonly Action<float[]> _getSpectrum;

        /// <summary>
        /// Delegate for post-processing spectrum
        /// </summary>
        protected readonly Action _postProcessSpectrum;

        /// <summary>
        /// Delegate for applying DCT
        /// </summary>
        protected readonly Action<float[]> _applyDct;
        
        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        protected readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for a post-processed mel-spectrum at each step
        /// </summary>
        protected readonly float[] _melSpectrum;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="options">MFCC options</param>
        public MfccExtractor(MfccOptions options) : base(options)
        {
            FeatureCount = options.FeatureCount;

            var filterbankSize = options.FilterBankSize;

            _lowFreq = options.LowFrequency;
            _highFreq = options.HighFrequency;

            if (options.FilterBank == null)
            {
                _blockSize = options.FftSize > FrameSize ? options.FftSize : MathUtils.NextPowerOfTwo(FrameSize);

                var melBands = FilterBanks.MelBands(filterbankSize, SamplingRate, _lowFreq, _highFreq);
                FilterBank = FilterBanks.Triangular(_blockSize, SamplingRate, melBands, mapper: Scale.HerzToMel);   // HTK/Kaldi-style
            }
            else
            {
                FilterBank = options.FilterBank;
                filterbankSize = FilterBank.Length;
                _blockSize = 2 * (FilterBank[0].Length - 1);

                Guard.AgainstExceedance(FrameSize, _blockSize, "frame size", "FFT size");
            }

            _fft = new RealFft(_blockSize);

            _lifterSize = options.LifterSize;
            _lifterCoeffs = _lifterSize > 0 ? Window.Liftering(FeatureCount, _lifterSize) : null;

            _includeEnergy = options.IncludeEnergy;
            _logEnergyFloor = options.LogEnergyFloor;

            // setup DCT: ============================================================================

            _dctType = options.DctType;
            switch (_dctType[0])
            {
                case '1': _dct = new Dct1(filterbankSize); break;
                case '3': _dct = new Dct3(filterbankSize); break;
                case '4': _dct = new Dct4(filterbankSize); break;
                default:  _dct = new Dct2(filterbankSize); break;
            }

            if (_dctType.EndsWith("N", StringComparison.OrdinalIgnoreCase))
            {
                _applyDct = mfccs => _dct.DirectNorm(_melSpectrum, mfccs);
            }
            else
            {
                _applyDct = mfccs => _dct.Direct(_melSpectrum, mfccs);
            }

            // setup spectrum post-processing: =======================================================

            _logFloor = options.LogFloor;
            _nonLinearityType = options.NonLinearity;
            switch (_nonLinearityType)
            {
                case NonLinearityType.Log10:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog10(FilterBank, _spectrum, _melSpectrum, _logFloor); break;
                case NonLinearityType.LogE:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndLog(FilterBank, _spectrum, _melSpectrum, _logFloor); break;
                case NonLinearityType.ToDecibel:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndToDecibel(FilterBank, _spectrum, _melSpectrum, _logFloor); break;
                case NonLinearityType.CubicRoot:
                    _postProcessSpectrum = () => FilterBanks.ApplyAndPow(FilterBank, _spectrum, _melSpectrum, 0.33); break;
                default:
                    _postProcessSpectrum = () => FilterBanks.Apply(FilterBank, _spectrum, _melSpectrum); break;
            }

            _spectrumType = options.SpectrumType;
            switch (_spectrumType)
            {
                case SpectrumType.Magnitude:
                    _getSpectrum = block => _fft.MagnitudeSpectrum(block, _spectrum, false); break;
                case SpectrumType.MagnitudeNormalized:
                    _getSpectrum = block => _fft.MagnitudeSpectrum(block, _spectrum, true); break;
                case SpectrumType.PowerNormalized:
                    _getSpectrum = block => _fft.PowerSpectrum(block, _spectrum, true); break;
                default:
                    _getSpectrum = block => _fft.PowerSpectrum(block, _spectrum, false); break;
            }

            // reserve memory for reusable blocks

            _spectrum = new float[_blockSize / 2 + 1];
            _melSpectrum = new float[filterbankSize];
        }


        /// <summary>
        /// Standard method for computing MFCC features.
        /// According to default configuration, in each frame do:
        /// 
        ///     0) Apply window (base extractor does it)
        ///     1) Obtain power spectrum X
        ///     2) Apply mel filters and log() the result: Y = Log(X * H)
        ///     3) Do dct: mfcc = Dct(Y)
        ///     4) [Optional] liftering of mfcc
        /// 
        /// </summary>
        /// <param name="block">Samples for analysis</param>
        /// <param name="features">MFCC vector</param>
        public override void ProcessFrame(float[] block, float[] features)
        {
            // 1) calculate magnitude/power spectrum (with/without normalization)

            _getSpectrum(block);        //  block -> _spectrum

            // 2) apply mel filterbank and take log10/ln/cubic_root of the result

            _postProcessSpectrum();     // _spectrum -> _melSpectrum

            // 3) dct

            _applyDct(features);        // _melSpectrum -> mfccs


            // 4) (optional) liftering

            if (_lifterCoeffs != null)
            {
                features.ApplyWindow(_lifterCoeffs);
            }

            // 5) (optional) replace first coeff with log(energy) 

            if (_includeEnergy)
            {
                features[0] = (float)Math.Log(Math.Max(block.Sum(x => x * x), _logEnergyFloor));
            }
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
            new MfccExtractor(
                new MfccOptions
                {
                    SamplingRate = SamplingRate,
                    FeatureCount = FeatureCount,
                    FrameDuration = FrameDuration,
                    HopDuration = HopDuration,
                    FilterBankSize = FilterBank.Length,
                    LowFrequency = _lowFreq,
                    HighFrequency = _highFreq,
                    FftSize = _blockSize,
                    FilterBank = FilterBank,
                    LifterSize = _lifterSize,
                    PreEmphasis = _preEmphasis,
                    DctType = _dctType,
                    NonLinearity = _nonLinearityType,
                    SpectrumType = _spectrumType,
                    Window = _window,
                    LogFloor = _logFloor,
                    IncludeEnergy = _includeEnergy,
                    LogEnergyFloor = _logEnergyFloor
                });
    }
}
