using System;
using System.Numerics;

namespace NWaves.Filters.Butterworth
{
    public static class PrototypeButterworth
    {
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
