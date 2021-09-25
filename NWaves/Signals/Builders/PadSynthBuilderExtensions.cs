using NWaves.Signals.Builders.Base;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class providing extension methods for <see cref="PadSynthBuilder"/>.
    /// </summary>
    public static class PadSynthBuilderExtensions
    {
        /// <summary>
        /// Set amplitudes of harmonics used in PadSynth algorithm.
        /// </summary>
        /// <param name="builder">Padsynth builder</param>
        /// <param name="amplitudes">Array of amplitudes</param>
        public static SignalBuilder SetAmplitudes(this SignalBuilder builder, float[] amplitudes)
        {
            PadSynthBuilder padSynth = builder as PadSynthBuilder;

            if (padSynth is null)
            {
                return builder;
            }

            padSynth.SetAmplitudeArray(amplitudes);
            
            return padSynth;
        }
    }
}
