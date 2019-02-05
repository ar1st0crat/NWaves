using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Signals.Builders
{
    /// <summary>
    /// Class for a simple generator of a straight line signal
    /// </summary>
    public class RampBuilder : SignalBuilder
    {
        /// <summary>
        /// Slope
        /// </summary>
        private double _slope;

        /// <summary>
        /// Intercept
        /// </summary>
        private double _intercept;

        /// <summary>
        /// Constructor
        /// </summary>
        public RampBuilder()
        {
            ParameterSetters = new Dictionary<string, Action<double>>
            {
                { "slope, k",     param => _slope = param },
                { "intercept, b", param => _intercept = param }
            };

            _slope = 0.0;
            _intercept = 0.0;
        }

        /// <summary>
        /// Method for generating one straight line according to simple formula:
        /// 
        ///     y[n] = slope * n + intercept
        /// 
        /// </summary>
        /// <returns></returns>
        public override float NextSample()
        {
            var sample = (float)(_slope * _n + _intercept);
            _n++;
            return sample;
        }

        public override void Reset()
        {
            _n = 0;
        }

        int _n;
    }
}
