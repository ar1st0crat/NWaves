using NWaves.Filters.Base;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// <see cref="ZiFilter64"/> is the special implementation of an LTI filter based on state vector (instead of delay lines). 
    /// <see cref="ZiFilter64"/> allows setting initial state (initial conditions for filter delays) and 
    /// provides additional method for zero-phase filtering <see cref="ZeroPhase(double[], int)"/>.
    /// </summary>
    public class ZiFilter64 : LtiFilter64
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly double[] _b;

        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations).
        /// </summary>
        protected readonly double[] _a;

        /// <summary>
        /// State vector.
        /// </summary>
        protected readonly double[] _zi;

        /// <summary>
        /// Gets state vector.
        /// </summary>
        public double[] Zi => _zi;

        /// <summary>
        /// Transfer function.
        /// </summary>
        protected TransferFunction _tf;

        /// <summary>
        /// Gets transfer function.
        /// </summary>
        public override TransferFunction Tf
        {
            // created lazily or set specifically if needed
            get => _tf ?? new TransferFunction(_b, _a);
            protected set => _tf = value;
        }

        /// <summary>
        /// Constructs <see cref="ZiFilter64"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/>.
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
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
        /// Constructs <see cref="ZiFilter64"/> from transfer function <paramref name="tf"/>.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public ZiFilter64(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Initializes filter with initial conditions <paramref name="zi"/>.
        /// </summary>
        /// <param name="zi">Vector of initial conditions</param>
        public virtual void Init(double[] zi)
        {
            Array.Copy(zi, 0, _zi, 0, Math.Min(zi.Length, _zi.Length));
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override double Process(double sample)
        {
            var output = _b[0] * sample + _zi[0];

            for (var j = 1; j < _zi.Length; j++)
            {
                _zi[j - 1] = _b[j] * sample - _a[j] * output + _zi[j];
            }

            return output;
        }

        /// <summary>
        /// Does zero-phase filtering (analog of filtfilt() in MATLAB/sciPy).
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="padLength">
        /// Number of elements by which to extend <paramref name="signal"/> at both ends before applying the filter. 
        /// The default value is 3 * (max{len(numerator), len(denominator)} - 1).
        /// </param>
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
        /// Changes filter coefficients online (numerator / non-recursive part).
        /// </summary>
        /// <param name="b">New coefficients</param>
        public void ChangeNumeratorCoeffs(double[] b)
        {
            if (b.Length == _b.Length)
            {
                for (var i = 0; i < _b.Length; _b[i] = b[i], i++) { }
            }
        }

        /// <summary>
        /// Changes filter coefficients online (denominator / recursive part).
        /// </summary>
        /// <param name="a">New coefficients</param>
        public void ChangeDenominatorCoeffs(double[] a)
        {
            if (a.Length == _a.Length)
            {
                for (var i = 0; i < _a.Length; _a[i] = a[i], i++) { }
            }
        }

        /// <summary>
        /// Changes filter coefficients online (from transfer function <paramref name="tf"/>).
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public void Change(TransferFunction tf)
        {
            var b = tf.Numerator;

            if (b.Length == _b.Length)
            {
                for (var i = 0; i < _b.Length; _b[i] = b[i], i++) { }
            }

            var a = tf.Denominator;

            if (a.Length == _a.Length)
            {
                for (var i = 0; i < _a.Length; _a[i] = a[i], i++) { }
            }
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            Array.Clear(_zi, 0, _zi.Length);
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);
    }
}
