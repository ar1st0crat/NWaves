using System;
using System.Diagnostics;

namespace NWaves.Utils
{
    /// <summary>
    /// Static class containing the most widely used contracts / guard clauses
    /// </summary>
    [DebuggerStepThrough]
    public static class Guard
    {
        /// <summary>
        /// Guard against negative number or zero
        /// </summary>
        /// <param name="arg">Argument (number)</param>
        /// <param name="argName">Argument name</param>
        public static void AgainstNonPositive(double arg, string argName = "argument")
        {
            if (arg < 1e-30)
            {
                throw new ArgumentException($"{argName} must be positive!");
            }
        }

        /// <summary>
        /// Guard against inequality of two arguments
        /// </summary>
        /// <param name="arg1">The first argument</param>
        /// <param name="arg2">The second argument</param>
        /// <param name="arg1Name">Name of the first argument</param>
        /// <param name="arg2Name">Name of the second argument</param>
        public static void AgainstInequality(double arg1, double arg2, string arg1Name = "argument1", string arg2Name = "argument2")
        {
            if (Math.Abs(arg2 - arg1) > 1e-30)
            {
                throw new ArgumentException($"{arg1Name} must be equal to {arg2Name}!");
            }
        }

        /// <summary>
        /// Guard against the number being not in the given range
        /// </summary>
        /// <param name="value">Argument (number)</param>
        /// <param name="low">Lower boundary of the range</param>
        /// <param name="high">Upper boundary of the range</param>
        /// <param name="valueName">Argument name</param>
        public static void AgainstInvalidRange(double value, double low, double high, string valueName = "value")
        {
            if (value < low || value > high)
            {
                throw new ArgumentException($"{valueName} must be in range [{low}, {high}]!");
            }
        }

        /// <summary>
        /// Guard against the case when the first and the second arguments are not valid boundaries of a range
        /// </summary>
        /// <param name="low">The first argument</param>
        /// <param name="high">The second argument</param>
        /// <param name="lowName">Name of the first argument</param>
        /// <param name="highName">Name of the second argument</param>
        public static void AgainstInvalidRange(double low, double high, string lowName = "low", string highName = "high")
        {
            if (high - low < 1e-30)
            {
                throw new ArgumentException($"{highName} must be greater than {lowName}!");
            }
        }

        /// <summary>
        /// Guard against the first argument exceeding the second argument
        /// </summary>
        /// <param name="low">The first argument</param>
        /// <param name="high">The second argument</param>
        /// <param name="lowName">Name of the first argument</param>
        /// <param name="highName">Name of the second argument</param>
        public static void AgainstExceedance(double low, double high, string lowName = "low", string highName = "high")
        {
            if (low > high)
            {
                throw new ArgumentException($"{lowName} must not exceed {highName}!");
            }
        }

        /// <summary>
        /// Guard against integer number being not power of 2 (e.g. 8, 16, 128, etc.)
        /// </summary>
        /// <param name="n">Argument (number)</param>
        /// <param name="argName">Argument name</param>
        public static void AgainstNotPowerOfTwo(int n, string argName = "Parameter")
        {
            var pow = (int)Math.Log(n, 2);

            if (n != 1 << pow)
            {
                throw new ArgumentException($"{argName} must be a power of 2!");
            }
        }

        /// <summary>
        /// Guard against even integer number
        /// </summary>
        /// <param name="n">Argument (number)</param>
        /// <param name="argName">Argument name</param>
        public static void AgainstEvenNumber(int n, string argName = "Parameter")
        {
            if (n % 2 == 0)
            {
                throw new ArgumentException($"{argName} must be an odd number!");
            }
        }

        /// <summary>
        /// Guard against not ordered and not unique array
        /// </summary>
        /// <param name="values">Argument (array of values)</param>
        /// <param name="argName">Argument name</param>
        public static void AgainstNotOrdered(double[] values, string argName = "Values")
        {
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i] <= values[i - 1])
                {
                    throw new ArgumentException($"{argName} must be ordered!");
                }
            }
        }

        /// <summary>
        /// Guard against incorrect parameters for (equiripple) filter design
        /// </summary>
        /// <param name="freqs">Frequencies</param>
        /// <param name="desired">Desired magnitude response (gains)</param>
        /// <param name="weights">Weights</param>
        public static void AgainstIncorrectFilterParams(double[] freqs, double[] desired, double[] weights)
        {
            var n = freqs.Length;

            if (n < 4 || n % 2 != 0)
            {
                throw new ArgumentException("Frequency array must have even number of at least 4 values!");
            }

            if (freqs[0] != 0 || freqs[n - 1] != 0.5)
            {
                throw new ArgumentException("Frequency array must start with 0 and end with 0.5!");
            }

            Guard.AgainstInequality(desired.Length, n / 2, "Size of desired array", "half-size of freqs array");
            Guard.AgainstInequality(weights.Length, n / 2, "Size of weights array", "half-size of freqs array");
        }
    }
}
