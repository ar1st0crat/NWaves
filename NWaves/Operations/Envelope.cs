using System.Linq;
using NWaves.Filters.BiQuad;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Method for detecting the envelope of a signal
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="lowpassCutoff">LP filter cutoff frequency</param>
        /// <returns></returns>
        public static DiscreteSignal Envelope(DiscreteSignal signal, float lowpassCutoff = 0.05f)
        {
            var envelope = FullRectify(signal);

            var lowpassFilter = new LowPassFilter(lowpassCutoff);
            var smoothed = lowpassFilter.ApplyTo(envelope);

            return smoothed;
        }

        /// <summary>
        /// Full rectification
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Fully rectified signal</returns>
        public static DiscreteSignal FullRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate, 
                                      signal.Samples.Select(s => s < 0 ? -s : s));
        }

        /// <summary>
        /// Half rectification
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Half rectified signal</returns>
        public static DiscreteSignal HalfRectify(DiscreteSignal signal)
        {
            return new DiscreteSignal(signal.SamplingRate,
                                      signal.Samples.Select(s => s < 0 ? 0 : s));
        }
    }
}
