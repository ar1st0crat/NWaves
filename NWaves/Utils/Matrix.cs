using System;

namespace NWaves.Utils
{
    /// <summary>
    /// Represents 2D matrix.
    /// </summary>
    public class Matrix
    {
        private readonly double[][] _matrix;

        /// <summary>
        /// Gets or sets number of rows.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Gets or sets number of columns.
        /// </summary>
        public int Columns { get; set; }

        /// <summary>
        /// Constructs <see cref="Matrix"/> with given number of <paramref name="rows"/> and <paramref name="columns"/>.
        /// </summary>
        public Matrix(int rows, int columns = 0)
        {
            if (columns == 0) columns = rows;

            Guard.AgainstNonPositive(rows, "Number of rows");
            Guard.AgainstNonPositive(columns, "Number of columns");

            _matrix = new double[rows][];

            for (var i = 0; i < rows; i++)
            {
                _matrix[i] = new double[columns];
            }

            Rows = rows;
            Columns = columns;
        }

        /// <summary>
        /// Gets reference to underlying 2D array.
        /// </summary>
        public double[][] As2dArray() => _matrix;

        /// <summary>
        /// Gets transposed matrix.
        /// </summary>
        public Matrix T
        {
            get
            {
                var transposed = new Matrix(Columns, Rows);

                for (var i = 0; i < Columns; i++)
                {
                    for (var j = 0; j < Rows; j++)
                    {
                        transposed[i][j] = _matrix[j][i];
                    }
                }

                return transposed;
            }
        }

        /// <summary>
        /// Returns companion matrix.
        /// </summary>
        /// <param name="a">Input array</param>
        public static Matrix Companion(double[] a)
        {
            if (a.Length < 2)
            {
                throw new ArgumentException("The size of input array must be at least 2!");
            }

            if (Math.Abs(a[0]) < 1e-30)
            {
                throw new ArgumentException("The first coefficient must not be zero!");
            }

            var size = a.Length - 1;

            var companion = new Matrix(size);

            for (var i = 0; i < size; i++)
            {
                companion[0][i] = -a[i + 1] / a[0];
            }

            for (var i = 1; i < size; i++)
            {
                companion[i][i - 1] = 1;
            }

            return companion;
        }

        /// <summary>
        /// Returns identity matrix of given <paramref name="size"/>.
        /// </summary>
        public static Matrix Eye(int size)
        {
            var eye = new Matrix(size);

            for (var i = 0; i < size; i++)
            {
                eye[i][i] = 1;
            }

            return eye;
        }

        /// <summary>
        /// Returns sum of matrices <paramref name="m1"/> and <paramref name="m2"/>.
        /// </summary>
        public static Matrix operator +(Matrix m1, Matrix m2)
        {
            Guard.AgainstInequality(m1.Rows, m2.Rows, "Number of rows in first matrix", "number of rows in second matrix");
            Guard.AgainstInequality(m1.Columns, m2.Columns, "Number of columns in first matrix", "number of columns in second matrix");

            var result = new Matrix(m1.Rows, m1.Columns);

            for (var i = 0; i < m1.Rows; i++)
            {
                for (var j = 0; j < m1.Columns; j++)
                {
                    result[i][j] = m1[i][j] + m2[i][j];
                }
            }

            return result;
        }

        /// <summary>
        /// Subtracts matrix <paramref name="m2"/> from matrix <paramref name="m1"/>.
        /// </summary>
        public static Matrix operator -(Matrix m1, Matrix m2)
        {
            Guard.AgainstInequality(m1.Rows, m2.Rows, "Number of rows in first matrix", "number of rows in second matrix");
            Guard.AgainstInequality(m1.Columns, m2.Columns, "Number of columns in first matrix", "number of columns in second matrix");

            var result = new Matrix(m1.Rows, m1.Columns);

            for (var i = 0; i < m1.Rows; i++)
            {
                for (var j = 0; j < m1.Columns; j++)
                {
                    result[i][j] = m1[i][j] - m2[i][j];
                }
            }

            return result;
        }

        /// <summary>
        /// Gets row by its index.
        /// </summary>
        /// <param name="i">Row index</param>
        public double[] this[int i] => _matrix[i];
    }
}
