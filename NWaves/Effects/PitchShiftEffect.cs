using System;
using NWaves.Filters;
using NWaves.Filters.Base;
using NWaves.Signals;

namespace NWaves.Effects
{
    /// <summary>
    /// Pitch Shift effect based on phase vocoder
    /// </summary>
    public class PitchShiftEffect : IFilter
    {
        /// <summary>
        /// Shift ratio
        /// </summary>
        private readonly double _shift;

        /// <summary>
        /// Size of FFT
        /// </summary>
        private readonly int _fftSize;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shift"></param>
        /// <param name="fftSize"></param>
        public PitchShiftEffect(double shift, int fftSize = 4096)
        {
            _shift = shift;
            _fftSize = fftSize;
        }

        /// <summary>
        /// Algorithm is based on Phase Vocoder
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="filteringOptions"></param>
        /// <returns></returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            if (Math.Abs(_shift - 1.0) < 1e-10)
            {
                return signal;
            }

            var hopAnalysis = _fftSize / 4;
            var hopSynthesis = (int)(hopAnalysis * _shift);

            var vocoder = new PhaseVocoder(hopAnalysis, hopSynthesis, _fftSize, _fftSize);
            signal = vocoder.ApplyTo(signal);
            
            // resample

            return signal;
        }
    }
}
