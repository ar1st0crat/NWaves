using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Abstract class for representing any signal builder (generator)
    /// </summary>
    public abstract class SignalBuilder
    {
        /// <summary>
        /// Number of delay samples
        /// </summary>
        private int _delay;

        /// <summary>
        /// Number of times to repeat the signal
        /// </summary>
        private int _repeatTimes;

        /// <summary>
        /// List of signals to be superimposed with the resulting signal
        /// </summary>
        private readonly List<DiscreteSignal> _toSuperimpose = new List<DiscreteSignal>();
        
        /// <summary>
        /// Resulting signal
        /// </summary>
        protected DiscreteSignal Signal { get; set; }

        /// <summary>
        /// The length of the signal
        /// </summary>
        protected int Length { get; set; } = 1;

        /// <summary>
        /// Sampling rate of the signal
        /// </summary>
        protected int SamplingRate { get; set; } = 1;

        /// <summary>
        /// Dictionary of setters for each parameter
        /// </summary>
        protected Dictionary<string, Action<double>> ParameterSetters { get; set; }

        /// <summary>
        /// Brief descriptions of parameters (list of their names)
        /// </summary>
        /// <returns></returns>
        public virtual string[] GetParametersInfo()
        {
            return ParameterSetters.Keys.ToArray();
        }

        /// <summary>
        /// Method for setting parameter values
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
        /// Method for generating signal of particular shape 
        /// (must be implemented in subclasses).
        /// </summary>
        /// <returns>Generated signal</returns>
        protected abstract DiscreteSignal Generate();

        /// <summary>
        /// Final or intermediate build step
        /// </summary>
        /// <returns>The signal that is currently built</returns>
        public virtual DiscreteSignal Build()
        {
            var signal = Generate();

            // perhaps, superimpose
            signal = _toSuperimpose.Aggregate(signal, (current, s) => current.Superimpose(s));

            // perhaps, delay
            if (_delay != 0)
            {
                signal = signal.Delay(_delay);
            }

            // and perhaps, repeat
            if (_repeatTimes > 1)
            {
                signal = signal.Repeat(_repeatTimes);
            }

            return signal;
        }
        
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
        /// <param name="delay"></param>
        /// <returns></returns>
        public virtual SignalBuilder DelayedBy(int delay)
        {
            _delay += delay;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public virtual SignalBuilder SuperimposedWith(DiscreteSignal signal)
        {
            _toSuperimpose.Add(signal);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="times"></param>
        /// <returns></returns>
        public virtual SignalBuilder RepeatedTimes(int times)
        {
            _repeatTimes += times;
            return this;
        }
    }
}
