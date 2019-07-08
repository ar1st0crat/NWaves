using System;
using System.Diagnostics;

namespace NWaves.Utils
{
    [DebuggerStepThrough]
    public static class Guard
    {
        public static void AgainstNonPositive(double arg, string argName = "argument")
        {
            if (arg < 1e-10)
            {
                throw new ArgumentException($"{argName} must be positive!");
            }
        }

        public static void AgainstInequality(double arg1, double arg2, string arg1Name = "argument1", string arg2Name = "argument2")
        {
            if (Math.Abs(arg2 - arg1) > 1e-10)
            {
                throw new ArgumentException($"{arg1Name} must be equal to {arg2Name}!");
            }
        }

        public static void AgainstInvalidRange(double low, double high, string lowName = "low", string highName = "high")
        {
            if (high - low < 1e-10)
            {
                throw new ArgumentException($"{highName} must be greater than {lowName}!");
            }
        }

        public static void AgainstExceedance(double low, double high, string lowName = "low", string highName = "high")
        {
            if (low > high)
            {
                throw new ArgumentException($"{lowName} must not exceed {highName}!");
            }
        }

        public static void AgainstNotPowerOfTwo(int n, string argName = "Parameter")
        {
            var pow = (int)Math.Log(n, 2);

            if (n != 1 << pow)
            {
                throw new ArgumentException($"{argName} must be a power of 2!");
            }
        }

        public static void AgainstEvenNumber(int n, string argName = "Parameter")
        {
            if (n % 2 == 0)
            {
                throw new ArgumentException($"{argName} must be an odd number!");
            }
        }

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
