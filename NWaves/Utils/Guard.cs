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

        public static void AgainstInvalidRange(double low, double high, string lowName = "low", string highName = "high")
        {
            if (high - low < 1e-10)
            {
                throw new ArgumentException($"{highName} must be greater than {lowName}!");
            }
        }
    }
}
