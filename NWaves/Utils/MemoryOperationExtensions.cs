using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Utils
{
    /// <summary>
    /// Provides extension methods implementing fast operations with memory buffers.
    /// </summary>
    public static class MemoryOperationExtensions
    {
        /// <summary>
        /// Creates array of single-precision values from enumerable of double-precision values.
        /// </summary>
        public static float[] ToFloats(this IEnumerable<double> values)
        {
            return values.Select(v => (float)v).ToArray();
        }

        /// <summary>
        /// Creates array of double-precision values from enumerable of single-precision values.
        /// </summary>
        public static double[] ToDoubles(this IEnumerable<float> values)
        {
            return values.Select(v => (double)v).ToArray();
        }

        #region single precision

        private const byte _32Bits = sizeof(float);

        /// <summary>
        /// Creates fast copy of array.
        /// </summary>
        public static float[] FastCopy(this float[] source)
        {
            var destination = new float[source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * _32Bits);
            return destination;
        }

        /// <summary>
        /// Makes fast copy of array (or its part) to existing <paramref name="destination"/> array (or its part).
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="destination">Destination array</param>
        /// <param name="size">Number of elements to copy</param>
        /// <param name="sourceOffset">Offset in source array</param>
        /// <param name="destinationOffset">Offset in destination array</param>
        public static void FastCopyTo(this float[] source, float[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
        }

        /// <summary>
        /// Makes fast copy of array fragment starting at specified offset.
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="size">Number of elements to copy</param>
        /// <param name="sourceOffset">Offset in source array</param>
        /// <param name="destinationOffset">Offset in destination array</param>
        public static float[] FastCopyFragment(this float[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new float[totalSize];
            Buffer.BlockCopy(source, sourceOffset * _32Bits, destination, destinationOffset * _32Bits, size * _32Bits);
            return destination;
        }

        /// <summary>
        /// Performs fast merging of array with <paramref name="another"/> array.
        /// </summary>
        public static float[] MergeWithArray(this float[] source, float[] another)
        {
            var merged = new float[source.Length + another.Length];
            Buffer.BlockCopy(source, 0, merged, 0, source.Length * _32Bits);
            Buffer.BlockCopy(another, 0, merged, source.Length * _32Bits, another.Length * _32Bits);
            return merged;
        }

        /// <summary>
        /// Creates new array containing given array repeated <paramref name="n"/> times.
        /// </summary>
        public static float[] RepeatArray(this float[] source, int n)
        {
            var repeated = new float[source.Length * n];

            var offset = 0;
            for (var i = 0; i < n; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * _32Bits, source.Length * _32Bits);
                offset += source.Length;
            }

            return repeated;
        }

        /// <summary>
        /// Creates new zero-padded array of given <paramref name="size"/> from given array.
        /// </summary>
        public static float[] PadZeros(this float[] source, int size)
        {
            var zeroPadded = new float[size];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _32Bits);
            return zeroPadded;
        }

        #endregion

        #region double precision

        private const byte _64Bits = sizeof(double);

        /// <summary>
        /// Creates fast copy of array.
        /// </summary>
        public static double[] FastCopy(this double[] source)
        {
            var destination = new double[source.Length];
            Buffer.BlockCopy(source, 0, destination, 0, source.Length * _64Bits);
            return destination;
        }

        /// <summary>
        /// Makes fast copy of array (or its part) to existing <paramref name="destination"/> array (or its part).
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="destination">Destination array</param>
        /// <param name="size">Number of elements to copy</param>
        /// <param name="sourceOffset">Offset in source array</param>
        /// <param name="destinationOffset">Offset in destination array</param>
        public static void FastCopyTo(this double[] source, double[] destination, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
        }

        /// <summary>
        /// Makes fast copy of array fragment starting at specified offset.
        /// </summary>
        /// <param name="source">Source array</param>
        /// <param name="size">Number of elements to copy</param>
        /// <param name="sourceOffset">Offset in source array</param>
        /// <param name="destinationOffset">Offset in destination array</param>
        public static double[] FastCopyFragment(this double[] source, int size, int sourceOffset = 0, int destinationOffset = 0)
        {
            var totalSize = size + destinationOffset;
            var destination = new double[totalSize];
            Buffer.BlockCopy(source, sourceOffset * _64Bits, destination, destinationOffset * _64Bits, size * _64Bits);
            return destination;
        }

        /// <summary>
        /// Performs fast merging of array with <paramref name="another"/> array.
        /// </summary>
        public static double[] MergeWithArray(this double[] source, double[] another)
        {
            var merged = new double[source.Length + another.Length];
            Buffer.BlockCopy(source, 0, merged, 0, source.Length * _64Bits);
            Buffer.BlockCopy(another, 0, merged, source.Length * _64Bits, another.Length * _64Bits);
            return merged;
        }

        /// <summary>
        /// Creates new array containing given array repeated <paramref name="n"/> times.
        /// </summary>
        public static double[] RepeatArray(this double[] source, int n)
        {
            var repeated = new double[source.Length * n];

            var offset = 0;
            for (var i = 0; i < n; i++)
            {
                Buffer.BlockCopy(source, 0, repeated, offset * _64Bits, source.Length * _64Bits);
                offset += source.Length;
            }

            return repeated;
        }

        /// <summary>
        /// Creates new zero-padded array of given <paramref name="size"/> from given array.
        /// </summary>
        public static double[] PadZeros(this double[] source, int size)
        {
            var zeroPadded = new double[size];
            Buffer.BlockCopy(source, 0, zeroPadded, 0, source.Length * _64Bits);
            return zeroPadded;
        }

        #endregion
    }
}
