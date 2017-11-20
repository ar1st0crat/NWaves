using NWaves.Filters.BiQuad;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Method for detecting the envelope of a signal
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="lowpassCutoff"></param>
        /// <returns></returns>
        public static DiscreteSignal Envelope(DiscreteSignal signal, double lowpassCutoff = 0.05)
        {
            var envelope = FullRectify(signal);

            var lowpassFilter = new LowPassFilter(lowpassCutoff);
            var smoothed = lowpassFilter.ApplyTo(envelope);

            return smoothed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static DiscreteSignal FullRectify(DiscreteSignal signal)
        {
            var s = signal.Copy();
            for (var i = 0; i < s.Length; i++)
            {
                if (s.Samples[i] < 0)
                {
                    s.Samples[i] = -s.Samples[i];
                }
            }
            return s;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static DiscreteSignal HalfRectify(DiscreteSignal signal)
        {
            var s = signal.Copy();
            for (var i = 0; i < s.Length; i++)
            {
                if (s.Samples[i] < 0)
                {
                    s.Samples[i] = 0;
                }
            }
            return s;
        }
    }
}
