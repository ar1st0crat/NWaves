using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Utils;
using NWaves.Windows;

namespace NWaves.Operations.Tsm
{
    /// <summary>
    /// Conventional Phase Vocoder
    /// </summary>
    public class PhaseVocoder : IFilter
    {
        /// <summary>
        /// Hop size at analysis stage (STFT decomposition)
        /// </summary>
        private readonly int _hopAnalysis;

        /// <summary>
        /// Hop size at synthesis stage (STFT merging)
        /// </summary>
        private readonly int _hopSynthesis;

        /// <summary>
        /// Size of FFT for analysis and synthesis
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Should phase vocoder use phase locking algorithm [Puckette]
        /// </summary>
        private readonly bool _phaseLocking;

        /// <summary>
        /// Stretch ratio
        /// </summary>
        private readonly double _stretch;

        /// <summary>
        /// Internal FFT transformer
        /// </summary>
        private readonly Fft _fft;

        /// <summary>
        /// Window coefficients
        /// </summary>
        private readonly float[] _window;

        /// <summary>
        /// Window coefficients squared
        /// </summary>
        private readonly float[] _windowSquared;

        /// <summary>
        /// Linearly spaced frequencies
        /// </summary>
        private readonly double[] _omega;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="stretch"></param>
        /// <param name="hopAnalysis"></param>
        /// <param name="fftSize"></param>
        /// <param name="phaseLocking"></param>
        public PhaseVocoder(double stretch, int hopAnalysis, int fftSize = 0, bool phaseLocking = true)
        {
            _stretch = stretch;
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = (int)(hopAnalysis * stretch);
            _fftSize = (fftSize > 0) ? fftSize : 8 * Math.Max(_hopAnalysis, _hopSynthesis);
            _phaseLocking = phaseLocking;
            
            _fft = new Fft(_fftSize);
            _window = Window.OfType(WindowTypes.Hann, _fftSize);
            _windowSquared = _window.Select(w => w * w).ToArray();

            _omega = Enumerable.Range(0, _fftSize / 2 + 1)
                               .Select(f => 2 * Math.PI * f / _fftSize)
                               .ToArray();
        }

        /// <summary>
        /// Phase Vocoder algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            if (_phaseLocking)
            {
                return PhaseLocking(signal);
            }
            
            var input = signal.Samples;
            var output = new float[(int)(input.Length * _stretch) + _fftSize];

            var windowSum = new float[output.Length];

            var re = new float[_fftSize];
            var im = new float[_fftSize];
            var zeroblock = new float[_fftSize];

            var prevPhase = new double[_fftSize / 2 + 1];
            var phaseTotal = new double[_fftSize / 2 + 1];

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
            {
                input.FastCopyTo(re, _fftSize, posAnalysis);
                zeroblock.FastCopyTo(im, _fftSize);

                re.ApplyWindow(_window);
                
                _fft.Direct(re, im);

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var mag = Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    var phase = Math.Atan2(im[j], re[j]);

                    var delta = phase - prevPhase[j];

                    var deltaUnwrapped = delta - _hopAnalysis * _omega[j];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = _omega[j] + deltaWrapped / _hopAnalysis;

                    phaseTotal[j] += _hopSynthesis * freq;
                    prevPhase[j] = phase;

                    re[j] = (float)(mag * Math.Cos(phaseTotal[j]));
                    im[j] = (float)(mag * Math.Sin(phaseTotal[j]));
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    re[j] = im[j] = 0.0f;
                }

                _fft.Inverse(re, im);

                for (var j = 0; j < re.Length; j++)
                {
                    output[posSynthesis + j] += re[j] * _window[j];
                    windowSum[posSynthesis + j] += _windowSquared[j];
                }

                posSynthesis += _hopSynthesis;
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-2) continue;
                output[j] /= windowSum[j] * _fftSize/2;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Phase locking procedure
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        private DiscreteSignal PhaseLocking(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var output = new float[(int)(input.Length * _stretch) + _fftSize];

            var windowSum = new float[output.Length];
            
            var re = new float[_fftSize];
            var im = new float[_fftSize];
            var zeroblock = new float[_fftSize];

            var mag = new double[_fftSize / 2 + 1];
            var phase = new double[_fftSize / 2 + 1];

            var prevPhase = new double[_fftSize / 2 + 1];
            var phaseTotal = new double[_fftSize / 2 + 1];
            var delta = new double[_fftSize / 2 + 1];

            var peaks = new int[_fftSize / 4];
            var peakCount = 0;

            var posSynthesis = 0;
            for (var posAnalysis = 0; posAnalysis + _fftSize < input.Length; posAnalysis += _hopAnalysis)
            {
                input.FastCopyTo(re, _fftSize, posAnalysis);
                zeroblock.FastCopyTo(im, _fftSize);

                re.ApplyWindow(_window);

                _fft.Direct(re, im);

                for (var j = 0; j < mag.Length; j++)
                {
                    mag[j] = Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    phase[j] = Math.Atan2(im[j], re[j]);
                }

                // spectral peaks in magnitude spectrum
                
                peakCount = 0;
                
                for (var j = 2; j < mag.Length - 3; j++)
                {
                    if (mag[j] <= mag[j - 1] || mag[j] <= mag[j - 2] ||
                        mag[j] <= mag[j + 1] || mag[j] <= mag[j + 2])
                    {
                        continue;   // if not a peak
                    }

                    peaks[peakCount++] = j;
                }

                peaks[peakCount++] = mag.Length - 1;

                // assign phases at peaks to all neighboring frequency bins

                var leftPos = 0;

                for (var j = 0; j < peakCount - 1; j++)
                {
                    var peakPos = peaks[j];
                    var peakPhase = phase[peakPos];

                    delta[peakPos] = peakPhase - prevPhase[peakPos];

                    var deltaUnwrapped = delta[peakPos] - _hopAnalysis * _omega[peakPos];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = _omega[peakPos] + deltaWrapped / _hopAnalysis;

                    phaseTotal[peakPos] = phaseTotal[peakPos] + _hopSynthesis * freq;
                    
                    var rightPos = (peaks[j] + peaks[j + 1]) / 2;

                    for (var k = leftPos; k < rightPos; k++)
                    {
                        phaseTotal[k] = phaseTotal[peakPos] + phase[k] - phase[peakPos];

                        prevPhase[k] = phase[k];

                        re[k] = (float)(mag[k] * Math.Cos(phaseTotal[k]));
                        im[k] = (float)(mag[k] * Math.Sin(phaseTotal[k]));
                    }

                    leftPos = rightPos;
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    re[j] = im[j] = 0.0f;
                }

                _fft.Inverse(re, im);

                for (var j = 0; j < re.Length; j++)
                {
                    output[posSynthesis + j] += re[j] * _window[j];
                    windowSum[posSynthesis + j] += _windowSquared[j];
                }

                posSynthesis += _hopSynthesis;
            }

            for (var j = 0; j < output.Length; j++)
            {
                if (windowSum[j] < 1e-2) continue;
                output[j] /= windowSum[j] * (_fftSize/2 + 1);
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /*
        /// <summary>
        /// Phase Vocoder algorithm (slower, but more readable for tutorial)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringMethod method = FilteringMethod.Auto)
        {
            var stftAnalysis = new Stft(_fftSize, _hopAnalysis);
            var frames = stftAnalysis.Direct(signal);

            var omega = Enumerable.Range(0, _fftSize / 2 + 1)
                                  .Select(f => 2 * Math.PI * f / _fftSize)
                                  .ToArray();

            var prevPhase = new float[_fftSize / 2 + 1];
            var phaseTotal = new float[_fftSize / 2 + 1];

            for (var i = 0; i < frames.Count; i++)
            {
                var mag = frames[i].Magnitude;
                var phase = frames[i].Phase;

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var delta = phase[j] - prevPhase[j];
                    
                    var deltaUnwrapped = delta - _hopAnalysis * omega[j];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = omega[j] + deltaWrapped / _hopAnalysis;
                    
                    phaseTotal[j] += _hopSynthesis * freq;
                    prevPhase[j] = phase[j];
                }

                var re = new float[_fftSize];
                var im = new float[_fftSize];

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    re[j] = mag[j] * Math.Cos(phaseTotal[j]);
                    im[j] = mag[j] * Math.Sin(phaseTotal[j]);
                }

                frames[i] = new ComplexDiscreteSignal(1, re, im);
            }

            var stftSynthesis = new Stft(_fftSize, _hopSynthesis);
            return new DiscreteSignal(signal.SamplingRate, stftSynthesis.Inverse(frames));
        }
        */
    }
}
