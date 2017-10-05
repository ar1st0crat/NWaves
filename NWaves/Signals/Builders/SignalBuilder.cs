using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class SignalBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        protected DiscreteSignal Signal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int Length { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected int SamplingRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        protected Dictionary<string, Action<double>> ParameterSetters { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetParametersInfo()
        {
            return ParameterSetters.Keys.ToArray();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <returns></returns>
        public virtual SignalBuilder SetParameter(string parameterName, double parameterValue)
        {
            foreach (var parameterKey in ParameterSetters.Keys)
            {
                var keywords = parameterKey.Split(',').Select(s => s.Trim());

                if (keywords.Any(keyword => string.Compare(keyword, parameterName, StringComparison.OrdinalIgnoreCase) == 0))
                {
                    var setter = ParameterSetters[parameterKey];
                    setter.Invoke(parameterValue);
                    return this;
                }
            }

            return this;
        }

        /// <summary>
        /// Final or intermediate build step
        /// </summary>
        /// <returns>The signal that is currently built</returns>
        public abstract DiscreteSignal Build();


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sampleCount"></param>
        /// <returns></returns>
        public virtual SignalBuilder OfLength(int sampleCount)
        {
            Length = sampleCount;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="samplingRate"></param>
        /// <returns></returns>
        public virtual SignalBuilder SampledAt(int samplingRate)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual SignalBuilder FromSignal(DiscreteSignal signal)
        {
            Signal = signal.Copy();

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="delay"></param>
        /// <returns></returns>
        public virtual SignalBuilder DelayedBy(int delay)
        {
            Signal = Signal?.Delay(delay);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual SignalBuilder SuperimposedWith(DiscreteSignal signal)
        {
            Signal = Signal?.Superimpose(signal);

            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public virtual SignalBuilder RepeatedTimes(int times)
        {
            Signal = Signal?.Repeat(times);

            return this;
        }
    }
}
