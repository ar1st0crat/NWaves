using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters.Fda;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.FeatureExtractors
{
    /// <summary>
    /// Power-Normalized Cepstral Coefficients extractor
    /// </summary>
    public class PnccExtractor : FeatureExtractor
    {
        /// <summary>
        /// Number of coefficients (including coeff #0)
        /// </summary>
        public override int FeatureCount { get; }

        /// <summary>
        /// Descriptions (simply "pncc0", "pncc1", "pncc2", etc.)
        /// </summary>
        public override List<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "pncc" + i).ToList();

        /// <summary>
        /// Window length for median-time power (2 * M + 1)
        /// </summary>
        public int M { get; set; } = 2;

        /// <summary>
        /// Window length for spectral smoothing (2 * N + 1)
        /// </summary>
        public int N { get; set; } = 4;

        /// <summary>
        /// Lambdas used in asymmetric noise suppression formula (4)
        /// </summary>
        public float LambdaA { get; set; } = 0.999f;
        public float LambdaB { get; set; } = 0.5f;
        
        /// <summary>
        /// Forgetting factor in temporal masking formula
        /// </summary>
        public float LambdaT { get; set; } = 0.85f;

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public float LambdaMu { get; set; } = 0.999f;

        /// <summary>
        /// Threshold for detecting excitation/non-excitation segments
        /// </summary>
        public float C { get; set; } = 2;

        /// <summary>
        /// Multiplier in formula (12) in [Kim & Stern, 2016]
        /// </summary>
        public float MuT { get; set; } = 0.2f;

        /// <summary>
        /// Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)].
        /// By default it's gammatone filterbank.
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
        private readonly Fft _fft;

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
        private float[] _block;

        /// <summary>
        /// Internal buffer for a signal spectrum at each step
        /// </summary>
        private float[] _spectrum;

        /// <summary>
        /// Internal buffers for gammatone spectrum and its derivatives
        /// </summary>
        private float[] _gammatoneSpectrum;
        private float[] _spectrumQOut;
        private float[] _filteredSpectrumQ;
        private float[] _spectrumS;
        private float[] _smoothedSpectrumS;
        private float[] _avgSpectrumQ1;
        private float[] _avgSpectrumQ2;
        private float[] _smoothedSpectrum;

        /// <summary>
        /// Internal buffer of zeros for quick memset
        /// </summary>
        private readonly float[] _zeroblock;

        /// <summary>
        /// Ring buffer for efficient processing of consecutive spectra
        /// </summary>
        private SpectraRingBuffer _ringBuffer;


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
        public PnccExtractor(int samplingRate,
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

            _fft = new Fft(fftSize);
            _dct = new Dct2(_filterbankSize, FeatureCount);

            _preEmphasis = (float)preEmphasis;

            _window = window;
            if (_window != WindowTypes.Rectangular)
            {
                _windowSamples = Window.OfType(_window, FrameSize);
            }

            _block = new float[_fftSize];
            _spectrum = new float[_fftSize / 2 + 1];
            _spectrumQOut = new float[_filterbankSize];
            _gammatoneSpectrum = new float[_filterbankSize];
            _filteredSpectrumQ = new float[_filterbankSize];
            _spectrumS = new float[_filterbankSize];
            _smoothedSpectrumS = new float[_filterbankSize];
            _avgSpectrumQ1 = new float[_filterbankSize];
            _avgSpectrumQ2 = new float[_filterbankSize];
            _smoothedSpectrum = new float[_filterbankSize];
            _zeroblock = new float[_fftSize];

            _ringBuffer = new SpectraRingBuffer(2 * M + 1, _filterbankSize);
        }

        /// <summary>
        /// PNCC algorithm according to [Kim & Stern, 2016]:
        ///     0) [Optional] pre-emphasis
        /// 
        /// Decompose signal into overlapping (hopSize) frames of length fftSize. In each frame do:
        /// 
        ///     1) Apply window (if rectangular window was specified then just do nothing)
        ///     2) Obtain power spectrum
        ///     3) Apply gammatone filters (squared)
        ///     4) Medium-time processing (asymmetric noise suppression, temporal masking, spectral smoothing)
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

            var hopSize = HopSize;
            var frameSize = FrameSize;

            const float meanPower = 1e10f;
            var mean = 4e07f;

            var d = _power != 0 ? 1.0 / _power : 0.0;

            var prevSample = startSample > 0 ? samples[startSample - 1] : 0.0f;
            
            var featureVectors = new List<FeatureVector>();

            var i = 0;
            var timePos = startSample;
            while (timePos + frameSize < endSample)
            {
                // prepare next block for processing

                _zeroblock.FastCopyTo(_block, _fftSize);
                samples.FastCopyTo(_block, frameSize, timePos);

                // 0) pre-emphasis (if needed)

                if (_preEmphasis > 0.0)
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

                FilterBanks.Apply(FilterBank, _spectrum, _gammatoneSpectrum);



                // =============================================================
                // 4) medium-time processing blocks:
                
                // 4.1) temporal integration (zero-phase moving average filter)

                _ringBuffer.Add(_gammatoneSpectrum);
                var spectrumQ = _ringBuffer.AverageSpectrum;

                // 4.2) asymmetric noise suppression

                if (i == 2 * M)
                {
                    for (var j = 0; j < _spectrumQOut.Length; j++)
                    {
                        _spectrumQOut[j] = spectrumQ[j] * 0.9f;
                    }
                }
                
                if (i >= 2 * M)
                {
                    for (var j = 0; j < _spectrumQOut.Length; j++)
                    {
                        if (spectrumQ[j] > _spectrumQOut[j])
                        {
                            _spectrumQOut[j] = LambdaA * _spectrumQOut[j] + (1 - LambdaA) * spectrumQ[j];
                        }
                        else
                        {
                            _spectrumQOut[j] = LambdaB * _spectrumQOut[j] + (1 - LambdaB) * spectrumQ[j];
                        }
                    }
                    
                    for (var j = 0; j < _filteredSpectrumQ.Length; j++)
                    {
                        _filteredSpectrumQ[j] = Math.Max(spectrumQ[j] - _spectrumQOut[j], 0.0f);

                        if (i == 2 * M)
                        {
                            _avgSpectrumQ1[j] = 0.9f * _filteredSpectrumQ[j];
                            _avgSpectrumQ2[j] = _filteredSpectrumQ[j];
                        }

                        if (_filteredSpectrumQ[j] > _avgSpectrumQ1[j])
                        {
                            _avgSpectrumQ1[j] = LambdaA * _avgSpectrumQ1[j] + (1 - LambdaA) * _filteredSpectrumQ[j];
                        }
                        else
                        {
                            _avgSpectrumQ1[j] = LambdaB * _avgSpectrumQ1[j] + (1 - LambdaB) * _filteredSpectrumQ[j];
                        }

                        // 4.3) temporal masking

                        var threshold = _filteredSpectrumQ[j];

                        _avgSpectrumQ2[j] *= LambdaT;
                        if (spectrumQ[j] < C * _spectrumQOut[j])
                        {
                            _filteredSpectrumQ[j] = _avgSpectrumQ1[j];
                        }
                        else
                        {
                            if (_filteredSpectrumQ[j] <= _avgSpectrumQ2[j])
                            {
                                _filteredSpectrumQ[j] = MuT * _avgSpectrumQ2[j];
                            }
                        }
                        _avgSpectrumQ2[j] = Math.Max(_avgSpectrumQ2[j], threshold);

                        _filteredSpectrumQ[j] = Math.Max(_filteredSpectrumQ[j], _avgSpectrumQ1[j]);
                    }


                    // 4.4) spectral smoothing 

                    for (var j = 0; j < _spectrumS.Length; j++)
                    {
                        _spectrumS[j] = _filteredSpectrumQ[j] / Math.Max(spectrumQ[j], float.Epsilon);
                    }

                    for (var j = 0; j < _smoothedSpectrumS.Length; j++)
                    {
                        _smoothedSpectrumS[j] = 0.0f;

                        var total = 0;
                        for (var k = Math.Max(j - N, 0);
                                 k < Math.Min(j + N + 1, _filterbankSize);
                                 k++, total++)
                        {
                            _smoothedSpectrumS[j] += _spectrumS[k];
                        }
                        _smoothedSpectrumS[j] /= total;
                    }

                    // 4.5) mean power normalization

                    var centralSpectrum = _ringBuffer.CentralSpectrum;

                    var sumPower = 0.0f;
                    for (var j = 0; j < _smoothedSpectrum.Length; j++)
                    {
                        _smoothedSpectrum[j] = _smoothedSpectrumS[j] * centralSpectrum[j];
                        sumPower += _smoothedSpectrum[j];
                    }

                    mean = LambdaMu * mean + (1 - LambdaMu) * sumPower;
                    
                    for (var j = 0; j < _smoothedSpectrum.Length; j++)
                    {
                        _smoothedSpectrum[j] *= meanPower / mean;
                    }
                    
                    // =============================================================


                    // 5) nonlinearity (power ^ d     or    Log10)

                    if (_power != 0)
                    {
                        for (var j = 0; j < _smoothedSpectrum.Length; j++)
                        {
                            _smoothedSpectrum[j] = (float) Math.Pow(_smoothedSpectrum[j], d);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < _smoothedSpectrum.Length; j++)
                        {
                            _smoothedSpectrum[j] = (float) Math.Log10(_smoothedSpectrum[j] + float.Epsilon);
                        }
                    }

                    // 6) dct-II (normalized)

                    var pnccs = new float[FeatureCount];
                    _dct.DirectN(_smoothedSpectrum, pnccs);
                    

                    // add pncc vector to output sequence

                    featureVectors.Add(new FeatureVector
                    {
                        Features = pnccs,
                        TimePosition = (double) timePos / SamplingRate
                    });
                }

                i++;
                
                timePos += hopSize;
            }

            return featureVectors;
        }


        /// <summary>
        /// Helper Ring Buffer class for efficient processing of consecutive spectra
        /// </summary>
        class SpectraRingBuffer
        {
            private readonly float[][] _spectra;
            private int _count;
            private int _capacity;
            private int _current;

            public float[] CentralSpectrum;
            public float[] AverageSpectrum;

            public SpectraRingBuffer(int capacity, int spectrumSize)
            {
                _spectra = new float[capacity][];
                _capacity = capacity;
                _count = 0;
                _current = 0;
                AverageSpectrum = new float[spectrumSize];
            }

            public void Add(float[] spectrum)
            {
                if (_count < _capacity) _count++;

                _spectra[_current] = spectrum;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    AverageSpectrum[j] = 0.0f;
                    for (var i = 0; i < _count; i++)
                    {
                        AverageSpectrum[j] += _spectra[i][j];
                    }
                    AverageSpectrum[j] /= _count;
                }

                CentralSpectrum = _spectra[(_current + _capacity / 2 + 1) % _capacity];

                _current = (_current + 1) % _capacity;
            }
        }
    }
}
