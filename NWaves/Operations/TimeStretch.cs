using System;
using NWaves.Operations.Tsm;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// Time stretching
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="stretch"></param>
        /// <param name="fftSize"></param>
        /// <param name="hopSize"></param>
        /// <returns></returns>
        public static DiscreteSignal TimeStretch(DiscreteSignal signal, double stretch, int fftSize = 1024, int hopSize = -1)
        {
            if (Math.Abs(stretch - 1.0) < 1e-10)
            {
                return signal.Copy();
            }

            var hopAnalysis = hopSize > 0 ? hopSize : fftSize / 4;
            var hopSynthesis = (int)(hopAnalysis * stretch);

            var vocoder = new PhaseVocoder(hopAnalysis, hopSynthesis, fftSize);
            return vocoder.ApplyTo(signal);
        }
    }
}
