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
        /// Constructor
        /// </summary>
        /// <param name="hopAnalysis"></param>
        /// <param name="hopSynthesis"></param>
        /// <param name="fftSize"></param>
        public PhaseVocoder(int hopAnalysis, int hopSynthesis, int fftSize = 0)
        {
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = hopSynthesis;
            _fftSize= (fftSize > 0) ? fftSize : 4 * Math.Max(hopAnalysis, hopSynthesis);
        }

        /// <summary>
        /// Phase Vocoder algorithm
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            var stretch = (float)_hopSynthesis / _hopAnalysis;

            var input = signal.Samples;
            var output = new float[(int)(input.Length * stretch) + _fftSize];

            var fft = new Fft(_fftSize);
            var hannWindow = Window.OfType(WindowTypes.Hann, _fftSize);

            var ratio = _fftSize / (2.0f * _hopAnalysis);
            var norm = 4.0f / (_fftSize * ratio);

            var omega = Enumerable.Range(0, _fftSize / 2 + 1)
                                  .Select(f => 2 * Math.PI * f / _fftSize)
                                  .ToArray();

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

                re.ApplyWindow(hannWindow);
                
                fft.Direct(re, im);

                for (var j = 0; j < _fftSize / 2 + 1; j++)
                {
                    var mag = Math.Sqrt(re[j] * re[j] + im[j] * im[j]);
                    var phase = Math.Atan2(im[j], re[j]);

                    var delta = phase - prevPhase[j];

                    var deltaUnwrapped = delta - _hopAnalysis * omega[j];
                    var deltaWrapped = MathUtils.Mod(deltaUnwrapped + Math.PI, 2 * Math.PI) - Math.PI;

                    var freq = omega[j] + deltaWrapped / _hopAnalysis;

                    phaseTotal[j] += _hopSynthesis * freq;
                    prevPhase[j] = phase;
                
                    re[j] = (float)(mag * Math.Cos(phaseTotal[j]));
                    im[j] = (float)(mag * Math.Sin(phaseTotal[j]));
                }

                for (var j = _fftSize / 2 + 1; j < _fftSize; j++)
                {
                    re[j] = im[j] = 0.0f;
                }

                fft.Inverse(re, im);

                for (var j = 0; j < re.Length; j++)
                {
                    output[posSynthesis + j] += re[j] * hannWindow[j] * norm;
                }

                posSynthesis += _hopSynthesis;
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset
        /// </summary>
        public void Reset()
        {
        }

        /*
        /// <summary>
        /// Phase Vocoder algorithm (slower, but more readable for tutorial)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
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
