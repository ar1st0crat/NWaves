using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders.Base
{
    /// <summary>
    /// Abstract class for all NWaves-style sample generators / signal builders.
    /// </summary>
    public abstract class SignalBuilder : ISampleGenerator, ISignalBuilder
    {
        /// <summary>
        /// Delay of the signal to build (used only in Build() method).
        /// </summary>
        private int _delay;

        /// <summary>
        /// Number of times to repeat the signal (used only in Build() method).
        /// </summary>
        private int _repeatTimes;

        /// <summary>
        /// List of signals to be superimposed with the signal to build (only in Build() method).
        /// </summary>
        private readonly List<DiscreteSignal> _toSuperimpose = new List<DiscreteSignal>();

        /// <summary>
        /// Resulting signal.
        /// </summary>
        protected DiscreteSignal Signal { get; set; }

        /// <summary>
        /// Dictionary of setters for each parameter.
        /// </summary>
        protected Dictionary<string, Action<double>> ParameterSetters { get; set; }

        /// <summary>
        /// Gets the sampling rate of the signal.
        /// </summary>
        public int SamplingRate { get; protected set; } = 1;

        /// <summary>
        /// Gets the length of the signal (number of samples).
        /// </summary>
        public int Length { get; protected set; }

        /// <summary>
        /// Gets the duration of the signal (in seconds).
        /// </summary>
        public double Duration { get; protected set; }

        /// <summary>
        /// Gets brief descriptions (or simply names) of parameters.
        /// </summary>
        public virtual string[] GetParametersInfo()
        {
            return ParameterSetters.Keys.ToArray();
        }

        /// <summary>
        /// Assigns value <paramref name="parameterValue"/> to parameter <paramref name="parameterName"/>.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="parameterValue">Parameter value</param>
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
        /// Generates new sample.
        /// </summary>
        public abstract float NextSample();

        /// <summary>
        /// Resets sample generator.
        /// </summary>
        public virtual void Reset()
        {
        }

        /// <summary>
        /// Generates signal by generating all its samples one-by-one.
        /// </summary>
        protected virtual DiscreteSignal Generate()
        {
            var signal = new DiscreteSignal(SamplingRate, Length);

            for (var i = 0; i < signal.Length; i++)
            {
                signal[i] = NextSample();
            }

            return signal;
        }

        /// <summary>
        /// Builds new entire signal.
        /// </summary>
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
        /// Sets the number of samples of the signal to build.
        /// </summary>
        /// <param name="sampleCount">Number of samples</param>
        public virtual SignalBuilder OfLength(int sampleCount)
        {
            Length = sampleCount;
            Duration = (double)sampleCount / SamplingRate;
            return this;
        }

        /// <summary>
        /// Sets the duration of the signal to build.
        /// </summary>
        /// <param name="seconds">Duration (in seconds)</param>
        public virtual SignalBuilder OfDuration(double seconds)
        {
            Duration = seconds;
            Length = (int)(seconds * SamplingRate);
            return this;
        }

        /// <summary>
        /// Sets the sampling rate of the signal to build.
        /// </summary>
        /// <param name="samplingRate">Sampling rate</param>
        public virtual SignalBuilder SampledAt(int samplingRate)
        {
            if (samplingRate <= 0)
            {
                throw new ArgumentException("Sampling rate must be positive!");
            }

            SamplingRate = samplingRate;

            if (Length <= 0)
            {
                OfDuration(Duration);
            }
            else
            {
                OfLength(Length);
            }

            return this;
        }

        /// <summary>
        /// Sets the delay of the signal to build.
        /// </summary>
        /// <param name="delay">Signal delay</param>
        public virtual SignalBuilder DelayedBy(int delay)
        {
            _delay += delay;
            return this;
        }

        /// <summary>
        /// Adds another one signal to superimpose with the signal to build.
        /// </summary>
        /// <param name="signal">Signal for superimposing</param>
        public virtual SignalBuilder SuperimposedWith(DiscreteSignal signal)
        {
            _toSuperimpose.Add(signal);
            return this;
        }

        /// <summary>
        /// Sets the number of times to repeat the signal to build.
        /// </summary>
        /// <param name="times">Number of times for repeating</param>
        public virtual SignalBuilder RepeatedTimes(int times)
        {
            _repeatTimes += times;
            return this;
        }
    }
}
