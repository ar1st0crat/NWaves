using System;
using System.Numerics;

namespace NWaves.Filters.Butterworth
{
    /// <summary>
    /// Butterworth filter prototype.
    /// </summary>
    public static class PrototypeButterworth
    {
        /// <summary>
        /// Evaluates analog poles of Butterworth filter of given <paramref name="order"/>.
        /// </summary>
        /// <param name="order">Filter order</param>
        public static Complex[] Poles(int order)
        {
            var poles = new Complex[order];

            for (var k = 0; k < order; k++)
            {
                var theta = Math.PI * (2 * k + 1) / (2 * order);

                poles[k] = new Complex(-Math.Sin(theta), Math.Cos(theta));
            }

            return poles;
        }
    }
}
