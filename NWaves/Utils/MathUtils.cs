using System;

namespace NWaves.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public static class MathUtils
    {
        /// <summary>
        /// Method for computing next power of 2 (closest to the given number)
        /// </summary>
        /// <param name="n">Number</param>
        /// <returns>Next power of 2 closest to the number</returns>
        public static int NextPowerOfTwo(int n)
        {
            return (int)Math.Pow(2, Math.Ceiling(Math.Log(n, 2)));
        }
    }
}
