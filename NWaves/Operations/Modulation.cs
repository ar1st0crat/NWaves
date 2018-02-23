using System.Linq;
using NWaves.Signals;
using NWaves.Signals.Builders;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="frequency"></param>
        /// <param name="modulationIndex"></param>
        /// <returns></returns>
        public static DiscreteSignal ModulateAmplitude(DiscreteSignal signal, 
                                                       double frequency = 20/*Hz*/,
                                                       double modulationIndex = 0.5)
        {
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("amp", modulationIndex)
                                    .SetParameter("freq", frequency)
                                    .OfLength(signal.Length)
                                    .SampledAt(signal.SamplingRate)
                                    .Build();

            var output = signal.Samples.Zip(sinusoid, (s, m) => (1 + m) * s);

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="frequency"></param>
        /// <param name="modulationIndex"></param>
        /// <returns></returns>
        public static DiscreteSignal ModulateFrequency(DiscreteSignal signal,
                                                       double frequency = 20/*Hz*/,
                                                       double modulationIndex = 0.5)
        {
            // sin(2pi * (f + m * ym) * n)
            var sinusoid = new SinusoidBuilder()
                                    .SetParameter("amp", modulationIndex)
                                    .SetParameter("freq", frequency)
                                    .OfLength(signal.Length)
                                    .SampledAt(signal.SamplingRate)
                                    .Build();

            var output = signal.Samples.Zip(sinusoid, (s, m) => (1 + m) * s);

            return new DiscreteSignal(signal.SamplingRate, output);
        }
    }
}
