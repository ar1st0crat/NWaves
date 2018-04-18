using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Pitch Shift effect based on classic phase vocoder
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
            // 1) just stretch
            var stretched = Operation.TimeStretch(signal, _shift, _fftSize);
            
            // 2) and interpolate
            var resampled = MathUtils.InterpolateLinear(
                                            Enumerable.Range(0, stretched.Length)   // [0.0, 1.0, 2.0, 3.0, ...]
                                                      .Select(s => (float)s)
                                                      .ToArray(),
                                            stretched.Samples,                      // stretched at 0.0, 1.0, 2.0, ...
                                            Enumerable.Range(0, signal.Length)      
                                                      .Select(s => (float)(_shift * s))
                                                      .ToArray());                  // [0.0, _shift, 2*_shift, ...]

            return new DiscreteSignal(signal.SamplingRate, resampled);
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
    }
}
