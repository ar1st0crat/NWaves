using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Helper class for copying float-valued arrays.
    /// The class wraps static methods of .NET Buffer class.
    /// </summary>
    static class FastCopy
    {
        private const byte Bytes = 4;

        /// <summary>
        /// Method simply copies source array to desination
        /// </summary>
        /// <param name="source">Source array</param>
        /// <returns>Source array copy</returns>
        public static float[] EntireArray(float[] source)
        {
            var destination = new float [source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * Bytes);
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
        public static float[] ArrayFragment(float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new float[totalSize];
            Buffer.BlockCopy(source, sourceOffset * Bytes, destination, destinationOffset * Bytes, size * Bytes);
            return destination;
        }

        /// <summary>
        /// Method copies an array (or its fragment) to existing array (or its part)
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="size"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="destinationOffset"></param>
        public static void ToExistingArray(float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            Buffer.BlockCopy(source, sourceOffset * Bytes, destination, destinationOffset * Bytes, size * Bytes);
        }

        /// <summary>
        /// Method does fast in-memory merge of two arrays
        /// </summary>
        /// <param name="source1">The first array for merging</param>
        /// <param name="source2">The second array for merging</param>
        /// <returns>Merged array</returns>
        public static float[] MergeArrays(float[] source1, float[] source2)
        {
            var merged = new float[source1.Length + source2.Length];
            Buffer.BlockCopy(source1, 0, merged, 0, source1.Length * Bytes);
            Buffer.BlockCopy(source2, 0, merged, source1.Length * Bytes, source2.Length * Bytes);
            return merged;
        }

        /// <summary>
        /// Method repeats given array N times
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="times">Number of times to repeat array</param>
        /// <returns>Array repeated N times</returns>
        public static float[] RepeatArray(float[] source, int times)
        {
            var repeated = new float[source.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * Bytes, source.Length * Bytes);
                offset += source.Length;
            }

            return repeated;
        }

        /// <summary>
        /// Method creates new zero-padded array from source array.
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="size">The size of a zero-padded array</param>
        /// <returns>Zero-padded array</returns>
        public static float[] PadZeros(float[] source, int size = 0)
        {
            var zeroPadded = new float[size];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * Bytes);
            return zeroPadded;
        }
    }
}
