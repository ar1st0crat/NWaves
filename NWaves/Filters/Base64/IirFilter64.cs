using NWaves.Filters.Base;
using NWaves.Operations.Convolution;
using NWaves.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NWaves.Filters.Base64
{
    /// <summary>
    /// Represents Infinite Impulse Response (IIR) filter (double precision).
    /// </summary>
    public class IirFilter64 : LtiFilter64
    {
        /// <summary>
        /// Numerator part coefficients in filter's transfer function 
        /// (non-recursive part in difference equations).
        /// </summary>
        protected readonly double[] _b;

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
        protected readonly double[] _a;

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
            get => _tf ?? new TransferFunction(_b.Take(_numeratorSize).ToArray(), _a.ToArray());
            protected set => _tf = value;
        }

        /// <summary>
        /// Gets or sets default length of truncated impulse response.
        /// </summary>
        public int DefaultImpulseResponseLength { get; set; } = 512;

        /// <summary>
        /// Internal delay line (recursive part).
        /// </summary>
        protected double[] _delayLineA;

        /// <summary>
        /// Internal delay line (non-recursive part).
        /// </summary>
        protected double[] _delayLineB;

        /// <summary>
        /// Current offset in delay line (recursive part).
        /// </summary>
        protected int _delayLineOffsetA;

        /// <summary>
        /// Current offset in delay line (non-recursive part).
        /// </summary>
        protected int _delayLineOffsetB;

        /// <summary>
        /// Constructs <see cref="IirFilter64"/> from numerator <paramref name="b"/> and denominator <paramref name="a"/>.
        /// </summary>
        /// <param name="b">Numerator of transfer function</param>
        /// <param name="a">Denominator of transfer function</param>
        public IirFilter64(IEnumerable<double> b, IEnumerable<double> a)
        {
            _numeratorSize = b.Count();
            _denominatorSize = a.Count();

            _b = new double[_numeratorSize * 2];

            for (var i = 0; i < _numeratorSize; i++)
            {
                _b[i] = _b[_numeratorSize + i] = b.ElementAt(i);
            }

            _a = a.ToArray();

            _delayLineB = new double[_numeratorSize];
            _delayLineA = new double[_denominatorSize];
            _delayLineOffsetB = _numeratorSize - 1;
            _delayLineOffsetA = _denominatorSize - 1;
        }

        /// <summary>
        /// Constructs <see cref="IirFilter64"/> from transfer function <paramref name="tf"/>.
        /// </summary>
        /// <param name="tf">Transfer function</param>
        public IirFilter64(TransferFunction tf) : this(tf.Numerator, tf.Denominator)
        {
            Tf = tf;
        }

        /// <summary>
        /// Processes entire <paramref name="signal"/> and returns new filtered signal.
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="method">Filtering method</param>
        public override double[] ApplyTo(double[] signal, FilteringMethod method = FilteringMethod.Auto)
        {
            switch (method)
            {
                case FilteringMethod.OverlapAdd:       // are you sure you wanna do this? It's IIR filter!
                case FilteringMethod.OverlapSave:
                    {
                        var length = Math.Max(DefaultImpulseResponseLength, _denominatorSize + _numeratorSize);
                        var fftSize = MathUtils.NextPowerOfTwo(4 * length);
                        var ir = Tf.ImpulseResponse(length);
                        return new OlsBlockConvolver64(ir, fftSize).ApplyTo(signal);
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
        public override double Process(double sample)
        {
            var output = 0.0;

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
        /// Changes filter coefficients online (numerator / non-recursive part).
        /// </summary>
        /// <param name="b">New coefficients</param>
        public void ChangeNumeratorCoeffs(double[] b)
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
        public void ChangeDenominatorCoeffs(double[] a)
        {
            if (a.Length == _denominatorSize)
            {
                for (var i = 0; i < _denominatorSize; _a[i] = a[i], i++) { }
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

            if (Math.Abs(a0 - 1) < 1e-10)
            {
                return;
            }

            if (Math.Abs(a0) < 1e-30)
            {
                throw new ArgumentException("The coefficient a[0] can not be zero!");
            }

            for (var i = 0; i < _a.Length; _a[i++] /= a0) { }
            for (var i = 0; i < _b.Length; _b[i++] /= a0) { }

            _tf?.Normalize();
        }
    }
}
