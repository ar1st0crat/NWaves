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
    /// Simplified Power-Normalized Cepstral Coefficients extractor
    /// </summary>
    public class SpnccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (including coeff #0)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "spncc0", "spncc1", "spncc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "spncc" + i).ToList();

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public float LambdaMu { get; set; } = 0.999f;

        /// <summary>
        /// Filterbank (gammatone by default)
        /// </summary>
        public float[][] FilterBank { get; }

        /// <summary>
        /// Number of gammatone filters
        /// </summary>
        private readonly int _filterbankSize;

        /// <summary>
        /// Lower frequency
        /// </summary>
        private readonly double _lowFreq;

        /// <summary>
        /// Upper frequency
        /// </summary>
        private readonly double _highFreq;

        /// <summary>
        /// Nonlinearity coefficient (if 0 then Log10 is applied)
        /// </summary>
        private readonly int _power;

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
        private readonly Dct2 _dct;

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
        /// Internal buffer for a signal block at each step
        /// </summary>
        private readonly float[] _block;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private readonly float[] _spectrum;

        /// <summary>
        /// Internal buffer for gammatone spectrum
        /// </summary>
        private readonly float[] _filteredSpectrum;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <param name="featureCount"></param>
        /// <param name="frameDuration">Length of analysis window (in seconds)</param>
        /// <param name="hopDuration">Length of overlap (in seconds)</param>
        /// <param name="power"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="filterbank"></param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public SpnccExtractor(int samplingRate, 
                              int featureCount,
                              double frameDuration = 0.0256/*sec*/,
                              double hopDuration = 0.010/*sec*/,
                              int power = 15,
                              double lowFreq = 100,
                              double highFreq = 6800,
                              int filterbankSize = 40,
                              float[][] filterbank = null,
                              int fftSize = 0,
                              double preEmphasis = 0.0,
                              WindowTypes window = WindowTypes.Hamming)

            : base(samplingRate, frameDuration, hopDuration)
        {
            FeatureCount = featureCount;

            _power = power;

            if (filterbank == null)
            {
                _fftSize = fftSize > FrameSize ? fftSize : MathUtils.NextPowerOfTwo(FrameSize);
                _filterbankSize = filterbankSize;

                _lowFreq = lowFreq;
                _highFreq = highFreq;

                FilterBank = FilterBanks.Erb(_filterbankSize, _fftSize, samplingRate, _lowFreq, _highFreq);

                // use power spectrum:

                foreach (var filter in FilterBank)
                {
                    for (var j = 0; j < filter.Length; j++)
                    {
                        var ps = filter[j] * filter[j];
                        filter[j] = ps;
                    }
                }
            }
            else
            {
                FilterBank = filterbank;
                _filterbankSize = filterbank.Length;
                _fftSize = 2 * (filterbank[0].Length - 1);
            }

            _fft = new RealFft(_fftSize);
            _dct = new Dct2(_filterbankSize, FeatureCount);

            _preEmphasis = (float) preEmphasis;
            
            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _block = new float[_fftSize];
            _spectrum = new float[_fftSize / 2 + 1];
            _filteredSpectrum = new float[_filterbankSize];
        }

        /// <summary>
        /// S(implified)PNCC algorithm according to [Kim & Stern, 2016]:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum
        ///     3) Apply gammatone filters (squared)
        ///     4) Mean power normalization
        ///     5) Apply nonlinearity
        ///     6) Do dct-II (normalized)
        /// 
        /// </summary>
        /// <param name="samples">Samples for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of pncc vectors</returns>
        public override List<FeatureVector> ComputeFrom(float[] samples, int startSample, int endSample)
        {
            Guard.AgainstInvalidRange(startSample, endSample, "starting pos", "ending pos");

            var frameSize = FrameSize;
            var hopSize = HopSize;

            const float meanPower = 1e10f;
            var mean = 4e07f;

            var d = _power != 0 ? 1.0 / _power : 0.0;

            var featureVectors = new List<FeatureVector>();

            var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;

            var lastSample = endSample - Math.Max(frameSize, hopSize);

            for (var i = startSample; i < lastSample; i += hopSize)
            {
                // prepare next block for processing

                // copy 'frameSize' samples
                samples.FastCopyTo(_block, frameSize, i);
                // fill zeros to 'fftSize'
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

                if (_window != WindowTypes.Rectangular)
                {
                    _block.ApplyWindow(_windowSamples);
                }

                // 2) calculate power spectrum

                _fft.PowerSpectrum(_block, _spectrum);

                // 3) apply gammatone filterbank

                FilterBanks.Apply(FilterBank, _spectrum, _filteredSpectrum);

                // 4) mean power normalization:

                var sumPower = 0.0f;
                for (var j = 0; j < _filteredSpectrum.Length; j++)
                {
                    sumPower += _filteredSpectrum[j];
                }

                mean = LambdaMu * mean + (1 - LambdaMu) * sumPower;

                for (var j = 0; j < _filteredSpectrum.Length; j++)
                {
                    _filteredSpectrum[j] *= meanPower / mean;
                }

                // 5) nonlinearity (power ^ d     or     Log10)

                if (_power != 0)
                {
                    for (var j = 0; j < _filteredSpectrum.Length; j++)
                    {
                        _filteredSpectrum[j] = (float) Math.Pow(_filteredSpectrum[j], d);
                    }
                }
                else
                {
                    for (var j = 0; j < _filteredSpectrum.Length; j++)
                    {
                        _filteredSpectrum[j] = (float) Math.Log10(_filteredSpectrum[j] + float.Epsilon);
                    }
                }

                // 6) dct-II (normalized)

                var spnccs = new float[FeatureCount];
                _dct.DirectN(_filteredSpectrum, spnccs);

                // add pncc vector to output sequence

                featureVectors.Add(new FeatureVector
                {
                    Features = spnccs,
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
            new SpnccExtractor(SamplingRate,
                               FeatureCount, 
                               FrameDuration, 
                               HopDuration, 
                               _power, 
                               _lowFreq, 
                               _highFreq, 
                               _filterbankSize, 
                               FilterBank, 
                               _fftSize, 
                               _preEmphasis, 
                               _window);
    }
}
