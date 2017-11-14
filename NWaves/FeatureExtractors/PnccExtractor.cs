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
        public override IEnumerable<string> FeatureDescriptions =>
            Enumerable.Range(0, FeatureCount).Select(i => "pncc" + i);

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
        public double LambdaA { get; set; } = 0.999;
        public double LambdaB { get; set; } = 0.5;
        
        /// <summary>
        /// Forgetting factor in temporal masking formula
        /// </summary>
        public double LambdaT { get; set; } = 0.85;

        /// <summary>
        /// Forgetting factor in formula (15) in [Kim & Stern, 2016]
        /// </summary>
        public double LambdaMu { get; set; } = 0.999;

        /// <summary>
        /// Threshold for detecting excitation/non-excitation segments
        /// </summary>
        public double C { get; set; } = 2;

        /// <summary>
        /// Multiplier in formula (12) in [Kim & Stern, 2016]
        /// </summary>
        public double MuT { get; set; } = 0.2;

        /// <summary>
        /// Gammatone Filterbank matrix of dimension [filterCount * (fftSize/2 + 1)]
        /// </summary>
        public double[][] GammatoneFilterBank { get; }

        /// <summary>
        /// Ring buffer for efficient processing of consecutive spectra
        /// </summary>
        private SpectraRingBuffer _ringBuffer;

        /// <summary>
        /// Size of FFT (in samples)
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Size of overlap (in samples)
        /// </summary>
        private readonly int _hopSize;

        /// <summary>
        /// Type of the window function
        /// </summary>
        private readonly WindowTypes _window;

        /// <summary>
        /// Samples of the weighting window
        /// </summary>
        private readonly double[] _windowSamples;

        /// <summary>
        /// Pre-emphasis filter (if needed)
        /// </summary>
        private readonly PreEmphasisFilter _preemphasisFilter;

        /// <summary>
        /// Main constructor
        /// </summary>
        /// <param name="featureCount"></param>
        /// <param name="samplingRate"></param>
        /// <param name="filterbankSize"></param>
        /// <param name="lowFreq"></param>
        /// <param name="highFreq"></param>
        /// <param name="windowSize">Length of analysis window (in seconds)</param>
        /// <param name="overlapSize">Length of overlap (in seconds)</param>
        /// <param name="fftSize">Size of FFT (in samples)</param>
        /// <param name="preEmphasis"></param>
        /// <param name="window"></param>
        public PnccExtractor(int featureCount, int samplingRate,
                             int filterbankSize = 40, double lowFreq = 100, double highFreq = 6800,
                             double windowSize = 0.0256, double overlapSize = 0.010, int fftSize = 1024,
                             double preEmphasis = 0.0, WindowTypes window = WindowTypes.Hamming)
        {
            FeatureCount = featureCount;

            var windowLength = (int)(samplingRate * windowSize);
            _windowSamples = Window.OfType(window, windowLength);
            _window = window;

            _fftSize = fftSize >= windowLength ? fftSize : MathUtils.NextPowerOfTwo(windowLength);
            _hopSize = (int)(samplingRate * overlapSize);

            if (preEmphasis > 0.0)
            {
                _preemphasisFilter = new PreEmphasisFilter(preEmphasis);
            }

            GammatoneFilterBank = FilterBanks.Erb(filterbankSize, _fftSize, samplingRate, lowFreq, highFreq);
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
        /// <returns>List of pncc vectors</returns>
        public override IEnumerable<FeatureVector> ComputeFrom(DiscreteSignal signal)
        {
            var featureVectors = new List<FeatureVector>();
            
            var gammatoneSpectrum = new double[GammatoneFilterBank.Length];

            var spectrumQOut = new double[GammatoneFilterBank.Length];
            var filteredSpectrumQ = new double[GammatoneFilterBank.Length];
            var spectrumS = new double[GammatoneFilterBank.Length];
            var smoothedSpectrumS = new double[GammatoneFilterBank.Length];
            var avgSpectrumQ1 = new double[GammatoneFilterBank.Length];
            var avgSpectrumQ2 = new double[GammatoneFilterBank.Length];
            var smoothedSpectrum = new double[GammatoneFilterBank.Length];
            
            const double meanPower = 1e10;
            var mean = 4e07;
            
            var block = new double[_fftSize];
            var zeroblock = new double[_fftSize - _windowSamples.Length];

            _ringBuffer = new SpectraRingBuffer(2 * M + 1, GammatoneFilterBank.Length);

            // prepare everything for dct

            var dct = new Dct();
            dct.Init(GammatoneFilterBank.Length, FeatureCount);


            // 0) pre-emphasis (if needed)

            var filtered = (_preemphasisFilter != null) ? _preemphasisFilter.ApplyTo(signal) : signal;


            var i = 0;
            var timePos = 0;
            while (timePos + _windowSamples.Length < filtered.Samples.Length)
            {
                // prepare next block for processing

                FastCopy.ToExistingArray(filtered.Samples, block, _windowSamples.Length, timePos);
                FastCopy.ToExistingArray(zeroblock, block, zeroblock.Length, 0, _windowSamples.Length);
                

                // 1) apply window

                if (_window != WindowTypes.Rectangular)
                {
                    block.ApplyWindow(_windowSamples);
                }


                // 2) calculate power spectrum

                var spectrum = Transform.PowerSpectrum(block, _fftSize);


                // 3) apply gammatone filterbank

                ApplyFilterbank(spectrum, gammatoneSpectrum);



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
                        spectrumQOut[j] = spectrumQ[j] * 0.9;
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
                        filteredSpectrumQ[j] = Math.Max(spectrumQ[j] - spectrumQOut[j], 0.0);

                        if (i == 2 * M)
                        {
                            avgSpectrumQ1[j] = 0.9 * filteredSpectrumQ[j];
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
                        spectrumS[j] = filteredSpectrumQ[j]/Math.Max(spectrumQ[j], double.Epsilon);
                    }

                    for (var j = 0; j < smoothedSpectrumS.Length; j++)
                    {
                        smoothedSpectrumS[j] = 0.0;

                        var total = 0;
                        for (var k = Math.Max(j - N, 0);
                                 k < Math.Min(j + N + 1, GammatoneFilterBank.Length);
                                 k++, total++)
                        {
                            smoothedSpectrumS[j] += spectrumS[k];
                        }
                        smoothedSpectrumS[j] /= total;
                    }

                    var centralSpectrum = _ringBuffer.CentralSpectrum;

                    var sumPower = 0.0;
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


                    // 5) nonlinearity (power ^ 1/15)

                    for (var j = 0; j < smoothedSpectrum.Length; j++)
                    {
                        smoothedSpectrum[j] = Math.Pow(smoothedSpectrum[j], 1 / 15.0);
                    }

                    // 6) dct-II (normalized)

                    var pnccs = new double[FeatureCount];
                    dct.Dct2N(smoothedSpectrum, pnccs);
                    

                    // add pncc vector to output sequence

                    featureVectors.Add(new FeatureVector
                    {
                        Features = pnccs,
                        TimePosition = (double)timePos / signal.SamplingRate
                    });
                }

                i++;
                
                timePos += _hopSize;
            }

            return featureVectors;
        }

        /// <summary>
        /// Method applies gammatone filters to spectrum.
        /// </summary>
        /// <param name="spectrum">Original spectrum</param>
        /// <param name="gammatoneSpectrum">Output gammatone-spectral array</param>
        private void ApplyFilterbank(double[] spectrum, double[] gammatoneSpectrum)
        {
            for (var i = 0; i < GammatoneFilterBank.Length; i++)
            {
                gammatoneSpectrum[i] = 0.0;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    gammatoneSpectrum[i] += GammatoneFilterBank[i][j] * GammatoneFilterBank[i][j] * spectrum[j];
                }
            }
        }

        /// <summary>
        /// Helper Ring Buffer class for efficient processing of consecutive spectra
        /// </summary>
        class SpectraRingBuffer
        {
            private readonly double[][] _spectra;
            private int _count;
            private int _capacity;
            private int _current;

            public double[] CentralSpectrum;
            public double[] AverageSpectrum;

            public SpectraRingBuffer(int capacity, int spectrumSize)
            {
                _spectra = new double[capacity][];
                _capacity = capacity;
                _count = 0;
                _current = 0;
                AverageSpectrum = new double[spectrumSize];
            }

            public void Add(double[] spectrum)
            {
                if (_count < _capacity) _count++;

                _spectra[_current] = spectrum;

                for (var j = 0; j < spectrum.Length; j++)
                {
                    AverageSpectrum[j] = 0.0;
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
