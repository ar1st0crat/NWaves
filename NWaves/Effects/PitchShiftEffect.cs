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
    public class PitchShiftEffect : AudioEffect
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
        /// Hop size
        /// </summary>
        private readonly int _hopSize;

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
        public PitchShiftEffect(double shift, int fftSize = 1024, int hopSize = 128, TsmAlgorithm tsm = TsmAlgorithm.PhaseVocoderPhaseLocking)
        {
            _shift = shift;
            _fftSize = fftSize;
            _hopSize = hopSize;
            _tsm = tsm;
        }

        /// <summary>
        /// Algorithm is essentially: 1) TSM; 2) linear interpolation
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        /// <returns>Pitch shifted signal</returns>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            // 1) just stretch

            var stretched = Operation.TimeStretch(signal, _shift, _fftSize, _hopSize, algorithm: _tsm);

            // 2) and interpolate

            var x = Enumerable.Range(0, stretched.Length)                   // [0.0, 1.0, 2.0, 3.0, ...]
                              .Select(s => (float)s)
                              .ToArray();

            var xresampled = Enumerable.Range(0, signal.Length)
                                       .Select(s => (float)(_shift * s))    // [0.0, _shift, 2*_shift, ...]
                                       .ToArray();

            var resampled = MathUtils.InterpolateLinear(x, stretched.Samples, xresampled);

            for (var i = 0; i < resampled.Length; i++)
            {
                resampled[i] = signal[i] * Dry + resampled[i] * Wet;
            }

            return new DiscreteSignal(signal.SamplingRate, resampled);
        }

        public override float Process(float sample)
        {
            throw new System.NotImplementedException();
        }

        public override void Reset()
        {
        }
    }
}
