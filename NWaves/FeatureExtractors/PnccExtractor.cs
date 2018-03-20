using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Signals;
using NWaves.FeatureExtractors.Base;
using NWaves.Filters;
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
        public override string[] FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "pncc" + i).ToArray();

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
        /// Gammatone Filterbank matrix of dimension [filterbankSize * (fftSize/2 + 1)]
        /// </summary>
        private float[][] _gammatoneFilterBank;
        public float[][] FilterBank => _gammatoneFilterBank;

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
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Pre-emphasis coefficient
        /// </summary>
        private readonly double _preEmphasis;

        /// <summary>
        /// Ring buffer for efficient processing of consecutive spectra
        /// </summary>
        private SpectraRingBuffer _ringBuffer;


        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="power"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="frameSize">Length of analysis window (in seconds)</param>
        /// <param name="hopSize">Length of overlap (in seconds)</param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public PnccExtractor(int featureCount, int power = 15,
                             int filterbankSize = 40, double lowFreq = 100, double highFreq = 6800,
                             double frameSize = 0.0256/*sec*/, double hopSize = 0.010/*sec*/, int fftSize = 1024,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
            : base(frameSize, hopSize)
        {
            FeatureCount = featureCount;
            _power = power;

            _window = window;
            _fftSize = fftSize;

            _filterbankSize = filterbankSize;
            _lowFreq = lowFreq;
            _highFreq = highFreq;

            _preEmphasis = preEmphasis;
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
        /// <param name="signal">Signal for analysis</param>
        /// <param name="startSample">The number (position) of the first sample for processing</param>
        /// <param name="endSample">The number (position) of last sample for processing</param>
        /// <returns>List of pncc vectors</returns>
        public override List<FeatureVector> ComputeFrom(DiscreteSignal signal, int startSample, int endSample)
        {
            // ====================================== PREPARE =======================================

            var hopSize = (int)(signal.SamplingRate * HopSize);
            var frameSize = (int)(signal.SamplingRate * FrameSize);
            var windowSamples = Window.OfType(_window, frameSize);

            var fftSize = _fftSize >= frameSize ? _fftSize : MathUtils.NextPowerOfTwo(frameSize);

            _gammatoneFilterBank = FilterBanks.Erb(_filterbankSize, _fftSize, signal.SamplingRate, _lowFreq, _highFreq);

            // use power spectrum:

            foreach (var filter in _gammatoneFilterBank)
            {
                for (var j = 0; j < filter.Length; j++)
                {
                    var ps = filter[j] * filter[j];
                    filter[j] = ps;
                }
            }


            var fft = new Fft (fftSize);
            var dct = new Dct2(_filterbankSize, FeatureCount);
            

            var gammatoneSpectrum = new float[_filterbankSize];

            var spectrumQOut = new float[_filterbankSize];
            var filteredSpectrumQ = new float[_filterbankSize];
            var spectrumS = new float[_filterbankSize];
            var smoothedSpectrumS = new float[_filterbankSize];
            var avgSpectrumQ1 = new float[_filterbankSize];
            var avgSpectrumQ2 = new float[_filterbankSize];
            var smoothedSpectrum = new float[_filterbankSize];
            
            const float meanPower = 1e10f;
            var mean = 4e07f;

            var d = _power != 0 ? 1.0 / _power : 0.0;
            
            var block = new float[fftSize];           // buffer for currently processed signal block at each step
            var zeroblock = new float[fftSize];       // buffer of zeros for quick memset

            _ringBuffer = new SpectraRingBuffer(2 * M + 1, _filterbankSize);

            var spectrum = new float[fftSize / 2 + 1];


            // 0) pre-emphasis (if needed)

            if (_preEmphasis > 0.0)
            {
                var preemphasisFilter = new PreEmphasisFilter(_preEmphasis);
                signal = preemphasisFilter.ApplyTo(signal);
            }


            // ================================= MAIN PROCESSING ==================================

            var featureVectors = new List<FeatureVector>();

            var i = 0;
            var timePos = startSample;
            while (timePos + frameSize < endSample)
            {
                // prepare next block for processing

                zeroblock.FastCopyTo(block, zeroblock.Length);
                signal.Samples.FastCopyTo(block, frameSize, timePos);
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(windowSamples);
                }


                // 2) calculate power spectrum

                fft.PowerSpectrum(block, spectrum);


                // 3) apply gammatone filterbank

                FilterBanks.Apply(_gammatoneFilterBank, spectrum, gammatoneSpectrum);



                // =============================================================
                // 4) medium-time processing blocks:
                
                // 4.1) temporal integration (zero-phase moving average filter)

                _ringBuffer.Add(gammatoneSpectrum);
                var spectrumQ = _ringBuffer.AverageSpectrum;

                // 4.2) asymmetric noise suppression

                if (i == 2 * M)
                {
                    for (var j = 0; j < spectrumQOut.Length; j++)
                    {
                        spectrumQOut[j] = spectrumQ[j] * 0.9f;
                    }
                }
                
                if (i >= 2 * M)
                {
                    for (var j = 0; j < spectrumQOut.Length; j++)
                    {
                        if (spectrumQ[j] > spectrumQOut[j])
                        {
                            spectrumQOut[j] = LambdaA * spectrumQOut[j] + (1 - LambdaA) * spectrumQ[j];
                        }
                        else
                        {
                            spectrumQOut[j] = LambdaB * spectrumQOut[j] + (1 - LambdaB) * spectrumQ[j];
                        }
                    }
                    
                    for (var j = 0; j < filteredSpectrumQ.Length; j++)
                    {
                        filteredSpectrumQ[j] = Math.Max(spectrumQ[j] - spectrumQOut[j], 0.0f);

                        if (i == 2 * M)
                        {
                            avgSpectrumQ1[j] = 0.9f * filteredSpectrumQ[j];
                            avgSpectrumQ2[j] = filteredSpectrumQ[j];
                        }

                        if (filteredSpectrumQ[j] > avgSpectrumQ1[j])
                        {
                            avgSpectrumQ1[j] = LambdaA * avgSpectrumQ1[j] + (1 - LambdaA) * filteredSpectrumQ[j];
                        }
                        else
                        {
                            avgSpectrumQ1[j] = LambdaB * avgSpectrumQ1[j] + (1 - LambdaB) * filteredSpectrumQ[j];
                        }

                        // 4.3) temporal masking

                        var threshold = filteredSpectrumQ[j];

                        avgSpectrumQ2[j] *= LambdaT;
                        if (spectrumQ[j] < C * spectrumQOut[j])
                        {
                            filteredSpectrumQ[j] = avgSpectrumQ1[j];
                        }
                        else
                        {
                            if (filteredSpectrumQ[j] <= avgSpectrumQ2[j])
                            {
                                filteredSpectrumQ[j] = MuT * avgSpectrumQ2[j];
                            }
                        }
                        avgSpectrumQ2[j] = Math.Max(avgSpectrumQ2[j], threshold);

                        filteredSpectrumQ[j] = Math.Max(filteredSpectrumQ[j], avgSpectrumQ1[j]);
                    }


                    // 4.4) spectral smoothing 

                    for (var j = 0; j < spectrumS.Length; j++)
                    {
                        spectrumS[j] = filteredSpectrumQ[j] / Math.Max(spectrumQ[j], float.Epsilon);
                    }

                    for (var j = 0; j < smoothedSpectrumS.Length; j++)
                    {
                        smoothedSpectrumS[j] = 0.0f;

                        var total = 0;
                        for (var k = Math.Max(j - N, 0);
                                 k < Math.Min(j + N + 1, _filterbankSize);
                                 k++, total++)
                        {
                            smoothedSpectrumS[j] += spectrumS[k];
                        }
                        smoothedSpectrumS[j] /= total;
                    }

                    // 4.5) mean power normalization

                    var centralSpectrum = _ringBuffer.CentralSpectrum;

                    var sumPower = 0.0f;
                    for (var j = 0; j < smoothedSpectrum.Length; j++)
                    {
                        smoothedSpectrum[j] = smoothedSpectrumS[j] * centralSpectrum[j];
                        sumPower += smoothedSpectrum[j];
                    }

                    mean = LambdaMu * mean + (1 - LambdaMu) * sumPower;
                    
                    for (var j = 0; j < smoothedSpectrum.Length; j++)
                    {
                        smoothedSpectrum[j] *= meanPower / mean;
                    }
                    
                    // =============================================================


                    // 5) nonlinearity (power ^ d    or    Log10)

                    if (_power != 0)
                    {
                        for (var j = 0; j < smoothedSpectrum.Length; j++)
                        {
                            smoothedSpectrum[j] = (float)Math.Pow(smoothedSpectrum[j], d);
                        }
                    }
                    else
                    {
                        for (var j = 0; j < smoothedSpectrum.Length; j++)
                        {
                            smoothedSpectrum[j] = (float)Math.Log10(smoothedSpectrum[j] + float.Epsilon);
                        }
                    }

                    // 6) dct-II (normalized)

                    var pnccs = new float[FeatureCount];
                    dct.DirectN(smoothedSpectrum, pnccs);
                    

                    // add pncc vector to output sequence

                    featureVectors.Add(new FeatureVector
                    {
                        Features = pnccs,
                        TimePosition = (double)timePos / signal.SamplingRate
                    });
                }

                i++;
                
                timePos += hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Parallel computation of PNCC is not currently implemented
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public override List<FeatureVector>[] ParallelChunksComputeFrom(DiscreteSignal signal)
        {
            throw new NotImplementedException();
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
