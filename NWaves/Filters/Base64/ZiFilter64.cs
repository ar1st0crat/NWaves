using NWaves.Filters.Base;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    public class ZiFilter64 : LtiFilter64
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations)
        /// </summary>
        protected readonly double[] _b;

        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations).
        /// </summary>
        protected readonly double[] _a;

        /// <summary>
        /// State vector
        /// </summary>
        protected readonly double[] _zi;
        public double[] Zi => _zi;

        /// <summary>
        /// Transfer function
        /// </summary>
        protected TransferFunction _tf;
        public override TransferFunction Tf
        {
            get => _tf ?? new TransferFunction(_b, _a);
            protected set => _tf = value;
        }

        /// <summary>
        /// Parameterized constructor (from arrays of 64-bit coefficients)
        /// </summary>
        /// <param name="b">TF numerator coefficients</param>
        /// <param name="a">TF denominator coefficients</param>
        public ZiFilter64(IEnumerable<double> b, IEnumerable<double> a)
        {
            _b = b.ToArray();
            _a = a.ToArray();

            var maxLength = _a.Length;

            if (_a.Length > _b.Length)
            {
                maxLength = _a.Length;
                _b = _b.PadZeros(maxLength);
            }
            else if (_a.Length < _b.Length)
            {
                maxLength = _b.Length;
                _a = _a.PadZeros(maxLength);
            }
            // don't check for equality

            _zi = new double[maxLength];
        }

        /// <summary>
        /// Parameterized constructor (from transfer function).
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public ZiFilter64(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Init filter
        /// </summary>
        /// <param name="zi"></param>
        public virtual void Init(double[] zi)
        {
            Array.Copy(zi, 0, _zi, 0, Math.Min(zi.Length, _zi.Length));
        }

        /// <summary>
        /// Apply filter to entire signal (offline)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        public override double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
        {
            return signal.Select(s => Process(s)).ToArray();
        }

        /// <summary>
        /// Online filtering with initial conditions
        /// </summary>
        /// <param name="input">Input sample</param>
        /// <returns>Output sample</returns>
        public override double Process(double input)
        {
            var output = _b[0] * input + _zi[0];

            for (var j = 1; j < _zi.Length; j++)
            {
                _zi[j - 1] = _b[j] * input - _a[j] * output + _zi[j];
            }

            return output;
        }

        /// <summary>
        /// Zero-phase filtering (analog of filtfilt() in MATLAB/sciPy)
        /// </summary>
        /// <param name="signal"></param>
        /// <param name="padLength"></param>
        /// <returns></returns>
        public double[] ZeroPhase(double[] signal, int padLength = 0)
        {
            if (padLength <= 0)
            {
                padLength = 3 * (Math.Max(_a.Length, _b.Length) - 1);
            }

            Guard.AgainstInvalidRange(padLength, signal.Length, "pad length", "Signal length");

            var output = new double[signal.Length];
            var edgeLeft = new double[padLength];
            var edgeRight = new double[padLength];


            // forward filtering: ============================================================

            var initialZi = Tf.Zi;
            var zi = initialZi.FastCopy();
            var baseSample = 2 * signal[0] - signal[padLength];

            for (int i = 0; i < zi.Length; zi[i++] *= baseSample) ;
            Init(zi);

            baseSample = signal[0];

            for (int k = 0, i = padLength; i > 0; k++, i--)
            {
                edgeLeft[k] = Process(2 * baseSample - signal[i]);
            }

            for (int i = 0; i < signal.Length; i++)
            {
                output[i] = Process(signal[i]);
            }

            baseSample = Enumerable.Last<double>(signal);

            for (int k = 0, i = signal.Length - 2; i > signal.Length - 2 - padLength; k++, i--)
            {
                edgeRight[k] = Process(2 * baseSample - signal[i]);
            }


            // backward filtering: ============================================================

            zi = initialZi;
            baseSample = edgeRight.Last();

            for (int i = 0; i < zi.Length; zi[i++] *= baseSample) ;
            Init(zi);

            for (int i = padLength - 1; i >= 0; i--)
            {
                Process(edgeRight[i]);
            }
            for (int i = output.Length - 1; i >= 0; i--)
            {
                output[i] = Process(output[i]);
            }
            for (int i = padLength - 1; i >= 0; i--)
            {
                Process(edgeLeft[i]);
            }

            return output;
        }

        /// <summary>
        /// Reset filter
        /// </summary>
        public override void Reset()
        {
            Array.Clear(_zi, 0, _zi.Length);
        }
    }
}
