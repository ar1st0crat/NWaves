using System;
using System.Linq;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Pitch Shift effect based on one of the available TSM algorithms
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
        /// Algorithm of time-scale modification
        /// </summary>
        private readonly TsmAlgorithm _tsm;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="shift"></param>
        /// <param name="fftSize"></param>
        /// <param name="tsm"></param>
        public PitchShiftEffect(double shift, int fftSize = 4096, TsmAlgorithm tsm = TsmAlgorithm.Wsola)
        {
            _shift = shift;
            _fftSize = fftSize;
            _tsm = tsm;
        }

        /// <summary>
        /// Algorithm is based on Phase Vocoder
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="filteringOptions">Filtering options</param>
        /// <returns>Pitch shifted signal</returns>
        public DiscreteSignal ApplyTo(DiscreteSignal signal,
                                      FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            // 1) just stretch
            var stretched = Operation.TimeStretch(signal, _shift, _fftSize, algorithm: _tsm);
            
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
        /// Online filtering (frame-by-frame)
        /// </summary>
        /// <param name="input">Input frame</param>
        /// <param name="filteringOptions">Filtering frame</param>
        /// <returns>Processed frame</returns>
        public float[] Process(float[] input, FilteringOptions filteringOptions = FilteringOptions.Auto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public void Reset()
        {
        }
    }
}
