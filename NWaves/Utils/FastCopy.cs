using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Helper class for copying double-valued arrays.
    /// The class wraps static methods of .NET Buffer class.
    /// </summary>
    static class FastCopy
    {
        /// <summary>
        /// Method simply copies source array to desination
        /// </summary>
        /// <param name="source">Source array</param>
        /// <returns>Source array copy</returns>
        public static double[] EntireArray(double[] source)
        {
            var destination = new double [source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * 8);
            return destination;
        }

        /// <summary>
        /// Method copies some fragment of the source array starting at specified offset
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="destinationOffset"></param>
        /// <returns>The copy of source array part</returns>
        public static double[] ArrayFragment(double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new double[totalSize];
            Buffer.BlockCopy(source, sourceOffset * 8, destination, destinationOffset * 8, size * 8);
            return destination;
        }

        /// <summary>
        /// Method does fast in-memory merge of two arrays
        /// </summary>
        /// <param name="source1">The first array for merging</param>
        /// <param name="source2">The second array for merging</param>
        /// <returns>Merged array</returns>
        public static double[] MergeArrays(double[] source1, double[] source2)
        {
            var merged = new double[source1.Length + source2.Length];
            Buffer.BlockCopy(source1, 0, merged, 0, source1.Length * 8);
            Buffer.BlockCopy(source2, 0, merged, source1.Length * 8, source2.Length * 8);
            return merged;
        }

        /// <summary>
        /// Method repeats given array N times
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="times">Number of times to repeat array</param>
        /// <returns>Array repeated N times</returns>
        public static double[] RepeatArray(double[] source, int times)
        {
            var repeated = new double[source.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * 8, source.Length * 8);
                offset += source.Length;
            }

            return repeated;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="zeroCount"></param>
        /// <returns></returns>
        public static double[] PadZeros(double[] source, int zeroCount = 0)
        {
            if (zeroCount <= 0)
            {
                zeroCount = MathUtils.NextPowerOfTwo(source.Length);
            }

            var zeroPadded = new double[zeroCount];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * 8);
            return zeroPadded;
        }
    }
}
