using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Signals;
using NWaves.Transforms;

namespace NWaves.Filters
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
        /// Size of FFT for analysis
        /// </summary>
        private readonly int _fftAnalysis;

        /// <summary>
        /// Size of FFT for synthesis
        /// </summary>
        private readonly int _fftSynthesis;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hopAnalysis"></param>
        /// <param name="hopSynthesis"></param>
        /// <param name="fftAnalysis"></param>
        /// <param name="fftSynthesis"></param>
        public PhaseVocoder(int hopAnalysis, int hopSynthesis, int fftAnalysis = 0, int fftSynthesis = 0)
        {
            _hopAnalysis = hopAnalysis;
            _hopSynthesis = hopSynthesis;
            _fftAnalysis = (fftAnalysis > 0) ? fftAnalysis : 4 * hopAnalysis;
            _fftSynthesis = (fftSynthesis > 0) ? fftSynthesis : 4 * hopSynthesis;
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
            var stftAnalysis = new Stft(_fftAnalysis, _hopAnalysis, fftSize: _fftAnalysis);
            var frames = stftAnalysis.Direct(signal);

            var omega = Enumerable.Range(0, _fftAnalysis)
                                  .Select(f => 2 * Math.PI * f / _fftAnalysis)
                                  .ToArray();

            var prevPhase = new double [_fftAnalysis];
            var phaseTotal = new double[_fftAnalysis];

            for (var i = 0; i < frames.Count; i++)
            {
                var mag = frames[i].Magnitude;
                var phase = frames[i].Phase;

                for (var j = 0; j < frames[i].Length; j++)
                {
                    var delta = phase[j] - prevPhase[j];
                    
                    var deltaUnwrapped = delta - _hopAnalysis * omega[j];
                    var deltaWrapped = (deltaUnwrapped + Math.PI) % (2 * Math.PI) - Math.PI;

                    var freq = omega[j] + deltaWrapped / _hopAnalysis;
                    phaseTotal[j] += freq * _hopSynthesis;
                    
                    prevPhase[j] = phase[j];
                }

                frames[i] = new ComplexDiscreteSignal(
                                    frames[i].SamplingRate,
                                    mag.Zip(phaseTotal, (m, p) => m * Math.Cos(p)),
                                    mag.Zip(phaseTotal, (m, p) => m * Math.Sin(p))
                                );
            }

            var stftSynthesis = new Stft(_fftSynthesis, _hopSynthesis, fftSize: _fftSynthesis);
            return new DiscreteSignal(signal.SamplingRate, stftSynthesis.Inverse(frames));
        }
    }
}
