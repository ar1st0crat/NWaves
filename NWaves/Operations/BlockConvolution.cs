using System;
using NWaves.Signals;

namespace NWaves.Operations
{
    public static partial class Operation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static DiscreteSignal OverlapAdd(DiscreteSignal signal, DiscreteSignal kernel)
        {
            if (signal.SamplingRate != kernel.SamplingRate)
            {
                throw new ArgumentException("Sampling rates should be the same!");
            }
            
            return signal.Copy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static DiscreteSignal OverlapSave(DiscreteSignal signal, DiscreteSignal kernel)
        {
            if (signal.SamplingRate != kernel.SamplingRate)
            {
                throw new ArgumentException("Sampling rates should be the same!");
            }

            return signal.Copy();
        }
    }
}
