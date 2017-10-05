using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        public static DiscreteSignal Interpolate(DiscreteSignal signal, int factor)
        {
            return signal;
        }

        public static DiscreteSignal Decimate(DiscreteSignal signal, int factor)
        {
            return signal;
        }

        public static DiscreteSignal Resample(DiscreteSignal signal, int newSamplingRate)
        {
            return signal;
        }
    }
}
