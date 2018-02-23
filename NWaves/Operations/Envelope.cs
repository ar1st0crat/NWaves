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
        public static DiscreteSignal Envelope(DiscreteSignal signal, double lowpassCutoff = 0.05)
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
            var s = signal.Copy();
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] < 0)
                {
                    s[i] = -s[i];
                }
            }
            return s;
        }

        /// <summary>
        /// Half rectification
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <returns>Half rectified signal</returns>
        public static DiscreteSignal HalfRectify(DiscreteSignal signal)
        {
            var s = signal.Copy();
            for (var i = 0; i < s.Length; i++)
            {
                if (s[i] < 0)
                {
                    s[i] = 0;
                }
            }
            return s;
        }
    }
}
