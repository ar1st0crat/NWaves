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

        public static void AgainstNotPowerOfTwo(int n, string argName = "Parameter")
        {
            var pow = (int)Math.Log(n, 2);

            if (n != 1 << pow)
            {
                throw new ArgumentException($"{argName} must be a power of 2!");
            }
        }
    }
}
