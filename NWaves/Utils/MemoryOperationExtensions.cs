using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Utils
{
    public static class MemoryOperationExtensions
    {
        /// <summary>
        /// Convert array of doubles to array of floats
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static float[] ToFloats(this IEnumerable<double> values)
        {
            return values.Select(v => (float)v).ToArray();
        }

        /// <summary>
        /// Convert array of floats to array of doubles
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static double[] ToDoubles(this IEnumerable<float> values)
        {
            return values.Select(v => (double)v).ToArray();
        }

        #region single precision

        private const byte _32Bits = sizeof(float);

        /// <summary>
        /// Method simply copies source array to desination
        /// </summary>
        /// <param name="source">Source array</param>
        /// <returns>Source array copy</returns>
        public static float[] FastCopy(this float[] source)
        {
            var destination = new float[source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * _32Bits);
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
        public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
        }

        /// <summary>
        /// Method copies some fragment of the source array starting at specified offset
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="destinationOffset"></param>
        /// <returns>The copy of source array part</returns>
        public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new float[totalSize];
            Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
            return destination;
        }

        /// <summary>
        /// Method does fast in-memory merge of two arrays
        /// </summary>
        /// <param name="source1">The first array for merging</param>
        /// <param name="source2">The second array for merging</param>
        /// <returns>Merged array</returns>
        public static float[] MergeWithArray(this float[] source1, float[] source2)
        {
            var merged = new float[source1.Length + source2.Length];
            Buffer.BlockCopy(source1, 0, merged, 0, source1.Length * _32Bits);
            Buffer.BlockCopy(source2, 0, merged, source1.Length * _32Bits, source2.Length * _32Bits);
            return merged;
        }

        /// <summary>
        /// Method repeats given array N times
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="times">Number of times to repeat array</param>
        /// <returns>Array repeated N times</returns>
        public static float[] RepeatArray(this float[] source, int times)
        {
            var repeated = new float[source.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * _32Bits, source.Length * _32Bits);
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
        public static float[] PadZeros(this float[] source, int size = 0)
        {
            var zeroPadded = new float[size];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _32Bits);
            return zeroPadded;
        }

        #endregion

        #region double precision

        private const byte _64Bits = sizeof(double);
        
        /// <summary>
        /// Method simply copies source array to desination
        /// </summary>
        /// <param name="source">Source array</param>
        /// <returns>Source array copy</returns>
        public static double[] FastCopy(this double[] source)
        {
            var destination = new double[source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * _64Bits);
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
        public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
        }
        
        /// <summary>
        /// Method copies some fragment of the source array starting at specified offset
        /// </summary>
        /// <param name="source"></param>
        /// <param name="size"></param>
        /// <param name="sourceOffset"></param>
        /// <param name="destinationOffset"></param>
        /// <returns>The copy of source array part</returns>
        public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new double[totalSize];
            Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
            return destination;
        }
        
        /// <summary>
        /// Method does fast in-memory merge of two arrays
        /// </summary>
        /// <param name="source1">The first array for merging</param>
        /// <param name="source2">The second array for merging</param>
        /// <returns>Merged array</returns>
        public static double[] MergeWithArray(this double[] source1, double[] source2)
        {
            var merged = new double[source1.Length + source2.Length];
            Buffer.BlockCopy(source1, 0, merged, 0, source1.Length * _64Bits);
            Buffer.BlockCopy(source2, 0, merged, source1.Length * _64Bits, source2.Length * _64Bits);
            return merged;
        }
        
        /// <summary>
        /// Method repeats given array N times
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="times">Number of times to repeat array</param>
        /// <returns>Array repeated N times</returns>
        public static double[] RepeatArray(this double[] source, int times)
        {
            var repeated = new double[source.Length * times];

            var offset = 0;
            for (var i = 0; i < times; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * _64Bits, source.Length * _64Bits);
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
        public static double[] PadZeros(this double[] source, int size = 0)
        {
            var zeroPadded = new double[size];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _64Bits);
            return zeroPadded;
        }

        #endregion
    }
}