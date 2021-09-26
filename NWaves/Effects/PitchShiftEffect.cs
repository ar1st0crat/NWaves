using System;
using System.Linq;
using NWaves.Effects.Base;
using NWaves.Filters.Base;
using NWaves.Operations;
using NWaves.Operations.Tsm;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Effects
{
    /// <summary>
    /// Class representing offline Pitch Shift audio effect 
    /// based on one of the available TSM algorithms and linear interpolation. 
    /// <see cref="PitchShiftEffect"/> does not implement online processing (method <see cref="Process(float)"/>).
    /// </summary>
    public class PitchShiftEffect : AudioEffect
    {
        /// <summary>
        /// Gets or sets pitch shift ratio.
        /// </summary>
        public double Shift { get; set; }

        /// <summary>
        /// Gets or sets time-scale modification algorithm.
        /// </summary>
        public TsmAlgorithm Tsm { get; set; }

        /// <summary>
        /// Gets or sets window size (frame length).
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        /// Gets or sets hop length.
        /// </summary>
        public int HopSize { get; set; }

        /// <summary>
        /// Construct <see cref="PitchShiftEffect"/>.
        /// </summary>
        /// <param name="shift">Pitch shift ratio</param>
        /// <param name="windowSize">Window size (frame length)</param>
        /// <param name="hopSize">Hop length</param>
        /// <param name="tsm">Time-scale modification algorithm</param>
        public PitchShiftEffect(double shift,
                                int windowSize = 1024,
                                int hopSize = 128,
                                TsmAlgorithm tsm = TsmAlgorithm.PhaseVocoderPhaseLocking)
        {
            Shift = shift;
            WindowSize = windowSize;
            HopSize = hopSize;
            Tsm = tsm;
        }

        /// <summary>
        /// Process entire <paramref name="signal"/> offline and return new pitch-shifted signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            // 1) just stretch (TSM)

            var stretched = Operation.TimeStretch(signal, Shift, WindowSize, HopSize, algorithm: Tsm);

            // 2) and interpolate

            var x = Enumerable.Range(0, stretched.Length)                   // [0.0, 1.0, 2.0, 3.0, ...]
                              .Select(s => (float)s)
                              .ToArray();

            var xresampled = Enumerable.Range(0, signal.Length)
                                       .Select(s => (float)(Shift * s))    // [0.0, _shift, 2*_shift, ...]
                                       .ToArray();

            var resampled = new float[xresampled.Length];

            MathUtils.InterpolateLinear(x, stretched.Samples, xresampled, resampled);

            for (var i = 0; i < resampled.Length; i++)
            {
                resampled[i] = signal[i] * Dry + resampled[i] * Wet;
            }

            return new DiscreteSignal(signal.SamplingRate, resampled);
        }

        /// <summary>
        /// Process on sample. This method is not implemented in <see cref="PitchShiftEffect"/> class.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Reset effect.
        /// </summary>
        public override void Reset()
        {
        }
    }
}
