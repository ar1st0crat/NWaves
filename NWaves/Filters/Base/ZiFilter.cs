using NWaves.Signals;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// <see cref="ZiFilter"/> is the special implementation of an LTI filter based on state vector (instead of delay lines). 
    /// <see cref="ZiFilter"/> allows setting initial state (initial conditions for filter delays) and 
    /// provides additional method for zero-phase filtering <see cref="ZeroPhase(DiscreteSignal, int)"/>.
    /// </summary>
    public class ZiFilter : LtiFilter
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly float[] _b;

        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations).
        /// </summary>
        protected readonly float[] _a;

        /// <summary>
        /// State vector.
        /// </summary>
        protected readonly float[] _zi;
        
        /// <summary>
        /// Gets state vector.
        /// </summary>
        public float[] Zi => _zi;

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
            get => _tf ?? new TransferFunction(_b.ToDoubles(), _a.ToDoubles());
            protected set => _tf = value;
        }

        /// <summary>
        /// Constructs <see cref="ZiFilter"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/>.
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
        public ZiFilter(IEnumerable<float> b, IEnumerable<float> a)
        {
            _b = b.ToArray();
            _a = a.ToArray();

            var maxLength = _a.Length;

            if (_a.Length > _b.Length)
            {
                maxLength = _a.Length;
                _b = _b.PadZeros(maxLength);
            }
            else if(_a.Length < _b.Length)
            {
                maxLength = _b.Length;
                _a = _a.PadZeros(maxLength);
            }
            // don't check for equality

            _zi = new float [maxLength];
        }

        /// <summary>
        /// <para>
        /// Constructs <see cref="ZiFilter"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/> (double precision).
        /// </para>
        /// <para>
        /// NOTE. 
        /// It will simply cast values to floats. 
        /// If you need to preserve precision for filter design and analysis, use constructor <see cref="ZiFilter(TransferFunction)"/>.
        /// </para>
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
        public ZiFilter(IEnumerable<double> b, IEnumerable<double> a) : this(b.ToFloats(), a.ToFloats())
        {
        }

        /// <summary>
        /// Constructs <see cref="ZiFilter"/> from transfer function <paramref name="tf"/>.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public ZiFilter(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Initializes filter with initial conditions <paramref name="zi"/>.
        /// </summary>
        /// <param name="zi">Vector of initial conditions</param>
        public virtual void Init(float[] zi)
        {
            Array.Copy(zi, 0, _zi, 0, Math.Min(zi.Length, _zi.Length));
        }

        /// <summary>
        /// Initializes filter with initial conditions <paramref name="zi"/>.
        /// </summary>
        /// <param name="zi">Vector of initial conditions</param>
        public virtual void Init(double[] zi)
        {
            Array.Copy(zi.ToFloats(), 0, _zi, 0, Math.Min(zi.Length, _zi.Length));
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
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
        public DiscreteSignal ZeroPhase(DiscreteSignal signal, int padLength = 0)
        {
            if (padLength <= 0)
            {
                padLength = 3 * (Math.Max(_a.Length, _b.Length) - 1);
            }

            Guard.AgainstInvalidRange(padLength, signal.Length, "pad length", "Signal length");

            var input = signal.Samples;
            var output = new float[signal.Length];
            var edgeLeft = new float[padLength];
            var edgeRight = new float[padLength];


            // forward filtering: ============================================================

            var initialZi = Tf.Zi;
            var zi = initialZi.FastCopy();
            var baseSample = 2 * input[0] - input[padLength];

            for (int i = 0; i < zi.Length; zi[i++] *= baseSample) ;
            Init(zi);

            baseSample = input[0];

            for (int k = 0, i = padLength; i > 0; k++, i--)
            {
                edgeLeft[k] = Process(2 * baseSample - input[i]);
            }

            for (int i = 0; i < input.Length; i++)
            {
                output[i] = Process(input[i]);
            }
            
            baseSample = input.Last();

            for (int k = 0, i = input.Length - 2; i > input.Length - 2 - padLength; k++, i--)
            {
                edgeRight[k] = Process(2 * baseSample - input[i]);
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

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Changes filter coefficients online (numerator / non-recursive part).
        /// </summary>
        /// <param name="b">New coefficients</param>
        public void ChangeNumeratorCoeffs(float[] b)
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
        public void ChangeDenominatorCoeffs(float[] a)
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
                for (var i = 0; i < _b.Length; _b[i] = (float)b[i], i++) { }
            }

            var a = tf.Denominator;

            if (a.Length == _a.Length)
            {
                for (var i = 0; i < _a.Length; _a[i] = (float)a[i], i++) { }
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
        /// Applies filter to entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal, FilteringMethod method = FilteringMethod.Auto) => this.FilterOnline(signal);


#if DEBUG
        /// <summary>
        /// Offline filtering with initial conditions (for tests)
        /// </summary>
        public DiscreteSignal FilterIc(DiscreteSignal signal)
        {
            var input = signal.Samples;
            var output = new float[signal.Length];

            for (var i = 0; i < output.Length; i++)
            {
                output[i] = _b[0] * input[i] + _zi[0];

                for (var j = 1; j < _zi.Length; j++)
                {
                    _zi[j - 1] = _b[j] * input[i] - _a[j] * output[i] + _zi[j];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }
#endif
    }
}
