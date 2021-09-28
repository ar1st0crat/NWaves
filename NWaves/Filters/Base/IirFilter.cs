using System;
using System.Collections.Generic;
using System.Linq;
using NWaves.Operations;
using NWaves.Signals;
using NWaves.Utils;

namespace NWaves.Filters.Base
{
    /// <summary>
    /// Represents Infinite Impulse Response (IIR) filter.
    /// </summary>
    public class IirFilter : LtiFilter
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly float[] _b;

        // Note.
        // This array is created from duplicated B coefficients:
        //      b                 _b
        // [1 2 3 4 5] -> [1 2 3 4 5 1 2 3 4 5]
        // 
        // Such memory layout leads to speed-up of online filtering.
        //

        /// <summary>
        /// Denominator part coefficients in filter's transfer function 
        /// (recursive part in difference equations).
        /// </summary>
        protected readonly float[] _a;

        /// <summary>
        /// Number of numerator coefficients.
        /// </summary>
        protected readonly int _numeratorSize;

        /// <summary>
        /// Number of denominator (feedback) coefficients.
        /// </summary>
        protected readonly int _denominatorSize;

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
            get => _tf ?? new TransferFunction(_b.Take(_numeratorSize).ToDoubles(), _a.ToDoubles());
            protected set => _tf = value;
        }
       
        /// <summary>
        /// Gets or sets default length of truncated impulse response.
        /// </summary>
        public int DefaultImpulseResponseLength { get; set; } = 512;

        /// <summary>
        /// Internal delay line (recursive part).
        /// </summary>
        protected float[] _delayLineA;

        /// <summary>
        /// Internal delay line (non-recursive part).
        /// </summary>
        protected float[] _delayLineB;

        /// <summary>
        /// Current offset in delay line (recursive part).
        /// </summary>
        protected int _delayLineOffsetA;

        /// <summary>
        /// Current offset in delay line (non-recursive part).
        /// </summary>
        protected int _delayLineOffsetB;

        /// <summary>
        /// Constructs <see cref="IirFilter"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/>.
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
        public IirFilter(IEnumerable<float> b, IEnumerable<float> a)
        {
            _numeratorSize = b.Count();
            _denominatorSize = a.Count();

            _b = new float[_numeratorSize * 2];

            for (var i = 0; i < _numeratorSize; i++)
            {
                _b[i] = _b[_numeratorSize + i] = b.ElementAt(i);
            }

            _a = a.ToArray();

            _delayLineB = new float[_numeratorSize];
            _delayLineA = new float[_denominatorSize];
            _delayLineOffsetB = _numeratorSize - 1;
            _delayLineOffsetA = _denominatorSize - 1;
        }

        /// <summary>
        /// <para>
        /// Constructs <see cref="IirFilter"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/> (double precision).
        /// </para>
        /// <para>
        /// NOTE. 
        /// It will simply cast values to floats. 
        /// If you need to preserve precision for filter design and analysis, use constructor <see cref="IirFilter(TransferFunction)"/>.
        /// </para>
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
        public IirFilter(IEnumerable<double> b, IEnumerable<double> a) : this(b.ToFloats(), a.ToFloats())
        {
        }

        /// <summary>
        /// Constructs <see cref="IirFilter"/> from transfer function <paramref name="tf"/>.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public IirFilter(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override DiscreteSignal ApplyTo(DiscreteSignal signal,
                                               FilteringMethod method = FilteringMethod.Auto)
        {
            switch (method)
            {
                case FilteringMethod.OverlapAdd:       // are you sure you wanna do this? It's IIR filter!
                case FilteringMethod.OverlapSave:
                {
                    var length = Math.Max(DefaultImpulseResponseLength, _denominatorSize + _numeratorSize);
                    var fftSize = MathUtils.NextPowerOfTwo(4 * length);
                    var ir = new DiscreteSignal(signal.SamplingRate, Tf.ImpulseResponse(length).ToFloats());
                    return Operation.BlockConvolve(signal, ir, fftSize, method);
                }
                case FilteringMethod.DifferenceEquation:
                {
                    return ApplyFilterDirectly(signal);
                }
                default:
                {
                    return this.FilterOnline(signal);
                }
            }
        }

        /// <summary>
        /// Processes one sample.
        /// </summary>
        /// <param name="sample">Input sample</param>
        public override float Process(float sample)
        {
            var output = 0f;

            _delayLineB[_delayLineOffsetB] = sample;

            for (int i = 0, j = _numeratorSize - _delayLineOffsetB; i < _numeratorSize; i++, j++)
            {
                output += _delayLineB[i] * _b[j];
            }

            var pos = 1;
            for (var p = _delayLineOffsetA + 1; p < _a.Length; p++)
            {
                output -= _a[pos++] * _delayLineA[p];
            }
            for (var p = 0; p < _delayLineOffsetA; p++)
            {
                output -= _a[pos++] * _delayLineA[p];
            }

            _delayLineA[_delayLineOffsetA] = output;

            if (--_delayLineOffsetB < 0)
            {
                _delayLineOffsetB = _numeratorSize - 1;
            }

            if (--_delayLineOffsetA < 0)
            {
                _delayLineOffsetA = _denominatorSize - 1;
            }

            return output;
        }

        /// <summary>
        /// The most straightforward implementation of the difference equation: 
        /// code the difference equation as it is.
        /// </summary>
        /// <param name="signal">Input signal</param>
        protected DiscreteSignal ApplyFilterDirectly(DiscreteSignal signal)
        {
            var input = signal.Samples;

            var output = new float[input.Length];

            for (var n = 0; n < input.Length; n++)
            {
                for (var k = 0; k < _numeratorSize; k++)
                {
                    if (n >= k) output[n] += _b[k] * input[n - k];
                }
                for (var m = 1; m < _denominatorSize; m++)
                {
                    if (n >= m) output[n] -= _a[m] * output[n - m];
                }
            }

            return new DiscreteSignal(signal.SamplingRate, output);
        }

        /// <summary>
        /// Changes filter coefficients online (numerator / non-recursive part).
        /// </summary>
        /// <param name="b">New coefficients</param>
        public void ChangeNumeratorCoeffs(float[] b)
        {
            if (b.Length == _numeratorSize)
            {
                for (var i = 0; i < _numeratorSize; i++)
                {
                    _b[i] = _b[_numeratorSize + i] = b[i];
                }
            }
        }

        /// <summary>
        /// Changes filter coefficients online (denominator / recursive part).
        /// </summary>
        /// <param name="a">New coefficients</param>
        public void ChangeDenominatorCoeffs(float[] a)
        {
            if (a.Length == _denominatorSize)
            {
                for (var i = 0; i < _denominatorSize; _a[i] = a[i], i++) { }
            }
        }

        /// <summary>
        /// Changes filter coefficients online (from transfer function <paramref name="tf"/>).
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public void Change(TransferFunction tf)
        {
            var b = tf.Numerator;

            if (b.Length == _numeratorSize)
            {
                for (var i = 0; i < _numeratorSize; i++)
                {
                    _b[i] = _b[_numeratorSize + i] = (float)b[i];
                }
            }

            var a = tf.Denominator;

            if (a.Length == _denominatorSize)
            {
                for (var i = 0; i < _a.Length; _a[i] = (float)a[i], i++) { }
            }
        }

        /// <summary>
        /// Resets filter.
        /// </summary>
        public override void Reset()
        {
            _delayLineOffsetB = _numeratorSize - 1;
            _delayLineOffsetA = _denominatorSize - 1;

            for (var i = 0; i < _delayLineB.Length; _delayLineB[i++] = 0) { }
            for (var i = 0; i < _delayLineA.Length; _delayLineA[i++] = 0) { }
        }

        /// <summary>
        /// Normalizes transfer function 
        /// (divides all filter coefficients by the first coefficient of TF denominator).
        /// </summary>
        public void Normalize()
        {
            var a0 = _a[0];

            if (Math.Abs(a0 - 1) < 1e-10f)
            {
                return;
            }

            if (Math.Abs(a0) < 1e-30f)
            {
                throw new ArgumentException("The coefficient a[0] can not be zero!");
            }

            for (var i = 0; i < _a.Length; _a[i++] /= a0) { }
            for (var i = 0; i < _b.Length; _b[i++] /= a0) { }

            _tf?.Normalize();
        }

        /// <summary>
        /// Creates <see cref="IirFilter"/> from sequential connection of IIR <paramref name="filter1"/> and any LTI <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">IIR filter</param>
        /// <param name="filter2">LTI filter</param>
        public static IirFilter operator *(IirFilter filter1, LtiFilter filter2)
        {
            var tf = filter1.Tf * filter2.Tf;

            return new IirFilter(tf.Numerator, tf.Denominator);
        }

        /// <summary>
        /// Creates <see cref="IirFilter"/> from parallel connection of IIR <paramref name="filter1"/> and any LTI <paramref name="filter2"/>.
        /// </summary>
        /// <param name="filter1">IIR filter</param>
        /// <param name="filter2">LTI filter</param>
        public static IirFilter operator +(IirFilter filter1, LtiFilter filter2)
        {
            var tf = filter1.Tf + filter2.Tf;

            return new IirFilter(tf.Numerator, tf.Denominator);
        }
    }
}
